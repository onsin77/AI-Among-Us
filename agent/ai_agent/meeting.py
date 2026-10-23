"""会议内容管线(文件驱动 + DeepSeek)。

自由练习模式里假人不会聊天/投票,会议内容必须由外部喂入。接口是一个 JSONL 文件
(默认 `agent/data/meeting_chat.jsonl`,可用 --meeting-file 覆盖),每行一个 JSON:

    {"speaker":"Red","text":"I saw Blue vent in Electrical"}
    {"type":"chat","speaker":"Blue","text":"that's a lie"}
    {"type":"vote","target":"Blue"}      # 触发 AI 投票(可附在 chat 行后自动投)
    {"type":"trigger"}                   # 强制 AI 发言(不新增聊天)

AI 每次消费新行(按文件偏移增量读),在 MEETING 阶段调用 DeepSeek,输出严格 JSON:
    {"say": "...", "vote": "skip" | "<玩家名>" | null}
然后把 `chat` / `vote` 指令经 L1 通道发出,并把结果追加到输出文件(默认
`agent/data/meeting_out.jsonl`)供复盘;面板同时显示最后一条响应与延迟。
"""

from __future__ import annotations

import asyncio
import json
import logging
import re
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from .bridge import BridgeClient
from .model import DeepSeekClient, ModelError, ModelResult
from .world import WorldModel

log = logging.getLogger("ai_agent.meeting")

SYSTEM_PROMPT = """你是 Among Us 的玩家「{name}」。你的身份:{role}。
会议规则:所有活着的玩家可以发言、投票;船员要找出内鬼,内鬼要伪装并误导。
输出严格 JSON(不要 markdown):
{{"say": "你要在会议里说的话(0-2 句,中文或英文与聊天一致,可为空字符串)", "vote": "skip" 或 "某玩家名" 或 null}}
要求:
- 发言要像真人玩家:简短、口语、有依据(聊天/位置/任务),不要提到 AI、模型、系统。
- 如果你是内鬼:绝不承认,可撒谎/甩锅/建议跳过,别自相矛盾。
- 如果你是船员:根据信息推理,可疑就点名,没把握就 skip。
- vote 只能填上面玩家名单里的名字、或 "skip"、或 null(本轮不投)。"""


@dataclass
class MeetingConfig:
    chat_file: Path | None = None
    out_file: Path | None = None
    model_tier: str = "fast"
    min_interval: float = 6.0
    max_context: int = 14
    enabled: bool = True


class MeetingPipeline:
    def __init__(self, world: WorldModel, bridge: BridgeClient,
                 model: DeepSeekClient | None, cfg: MeetingConfig) -> None:
        self.world = world
        self.bridge = bridge
        self.model = model
        self.cfg = cfg
        self._stop = asyncio.Event()
        self._offset = 0
        self._statements: list[dict[str, Any]] = []
        self._last_response_at = 0.0
        self._last_result: dict[str, Any] | None = None
        self._last_error: str | None = None
        self._vote_requested: str | None = None
        # 已回应过的发言总数(单调递增;不能用列表下标——statements 截断后下标会回退,
        # 永远追不上 responded_upto,会议 AI 会"失聪")
        self._total_statements = 0
        self._responded_upto = 0
        self._trigger = False
        self._ref_n = 0
        self._pending_vote: tuple[str | None, float] | None = None

    # ---------- 生命周期 ----------

    async def run(self) -> None:
        # 启动时跳过历史行:只处理启动之后新写入的会议内容(避免重放旧局发言)
        if self.cfg.chat_file is not None:
            try:
                if self.cfg.chat_file.exists():
                    self._offset = self.cfg.chat_file.stat().st_size
            except OSError:
                self._offset = 0
        self.world.meeting_ai.update({"enabled": True, "chat_file": str(self.cfg.chat_file or ""),
                                     "model": self.cfg.model_tier})
        log.info("meeting pipeline started (file=%s)", self.cfg.chat_file)
        while not self._stop.is_set():
            try:
                await self._poll()
            except Exception as e:
                self._last_error = f"{type(e).__name__}: {e}"
                log.exception("meeting tick failed")
            await asyncio.sleep(0.6)

    async def stop(self) -> None:
        self._stop.set()
        self.world.meeting_ai["enabled"] = False

    def status(self) -> dict[str, Any]:
        return {
            "enabled": self.cfg.enabled,
            "chat_file": str(self.cfg.chat_file or ""),
            "out_file": str(self.cfg.out_file or ""),
            "statements": len(self._statements),
            "last": self._last_result,
            "last_error": self._last_error,
            "model_tier": self.cfg.model_tier,
        }

    # ---------- 主循环 ----------

    async def _poll(self) -> None:
        if not self.cfg.enabled:
            return
        phase = (self.world.state or {}).get("phase")
        new_lines = self._read_new_lines()
        if new_lines:
            for ln in new_lines:
                t = ln.get("type", "chat")
                if t == "chat":
                    self._statements.append({
                        "speaker": str(ln.get("speaker") or "?"),
                        "text": str(ln.get("text") or ""),
                        "t": time.time(),
                    })
                    self._total_statements += 1
                    if len(self._statements) > self.cfg.max_context:
                        self._statements = self._statements[-self.cfg.max_context:]
                elif t == "vote":
                    self._vote_requested = ln.get("target")
                    self._trigger = True
                elif t == "trigger":
                    self._trigger = True

        if phase != "MEETING":
            self._pending_vote = None
            return

        # 到点补投(之前 can_vote 不满足时挂起)
        if self._pending_vote is not None:
            target, at = self._pending_vote
            if time.time() - at < 10.0:
                await self._try_vote(target)
            else:
                self._pending_vote = None

        has_new = self._total_statements > self._responded_upto
        if not (has_new or self._trigger):
            return
        if time.time() - self._last_response_at < self.cfg.min_interval:
            return

        self._trigger = False
        # 只有推理真正落地才推进水位:失败/无模型时保留批次,等下一轮重试
        if await self._respond():
            self._responded_upto = self._total_statements

    def _read_new_lines(self) -> list[dict[str, Any]]:
        f = self.cfg.chat_file
        if f is None:
            return []
        try:
            if not f.exists():
                return []
            size = f.stat().st_size
            if size < self._offset:  # 文件被截断/重建
                self._offset = 0
            if size == self._offset:
                return []
            # 二进制读:偏移按字节算,不受多字节字符影响;只消费到最后一个完整行,
            # 写了一半的行留在文件里等下次(否则该行会永久丢失)
            with f.open("rb") as fh:
                fh.seek(self._offset)
                raw = fh.read()
            last_nl = raw.rfind(b"\n")
            if last_nl < 0:
                return []
            chunk = raw[:last_nl + 1].decode("utf-8", errors="replace")
            self._offset += last_nl + 1
            out: list[dict[str, Any]] = []
            for line in chunk.splitlines():
                line = line.strip()
                if not line:
                    continue
                try:
                    obj = json.loads(line)
                except json.JSONDecodeError:
                    continue
                if isinstance(obj, dict):
                    out.append(obj)
            return out
        except OSError as e:
            self._last_error = f"read chat file: {e}"
            return []

    # ---------- 推理与执行 ----------

    def _my_name(self) -> str:
        return ((self.world.state or {}).get("me") or {}).get("name") or "AI"

    def _my_role_text(self) -> str:
        me = (self.world.state or {}).get("me") or {}
        role = me.get("role") or "未分配"
        team = me.get("team") or "?"
        return f"{role}({team})"

    def _roster(self) -> list[str]:
        names = []
        me_id = ((self.world.state or {}).get("me") or {}).get("player_id")
        for p in (self.world.state or {}).get("players") or []:
            if p.get("id") == me_id:
                continue
            if p.get("alive"):
                names.append(str(p.get("name")))
        return names

    async def _respond(self) -> bool:
        """推理一次并执行发言/投票。返回 False 表示本轮未落地(批次保留待重试)。"""
        if self.model is None or not self.model.available:
            self._last_error = "model unavailable (no api key)"
            self._last_response_at = time.time()  # 也按 min_interval 节流重试
            return False
        me = self._my_name()
        roster = self._roster()
        chat_lines = []
        for s in self._statements[-self.cfg.max_context:]:
            if s["speaker"] == me:
                continue
            chat_lines.append(f"{s['speaker']}: {s['text']}")
        round_no = (self.world.state or {}).get("round")
        user = (
            f"轮次:{round_no}。玩家名单:{', '.join(roster) if roster else '(无)'}。\n"
            f"聊天记录(按时间):\n" + ("\n".join(chat_lines) if chat_lines else "(暂无发言)")
        )
        if self._vote_requested is not None:
            user += f"\n本轮必须给出投票建议(当前表决请求:{self._vote_requested})。"
        messages = [
            {"role": "system", "content": SYSTEM_PROMPT.format(name=me, role=self._my_role_text())},
            {"role": "user", "content": user},
        ]
        t0 = time.time()
        try:
            res: ModelResult = await self.model.chat(messages, tier=self.cfg.model_tier,
                                                    json_mode=True, temperature=0.7, max_tokens=220)
        except ModelError as e:
            self._last_error = str(e)
            self.world.meeting_ai.update({"last_error": self._last_error})
            log.warning("model error: %s", e)
            self._last_response_at = time.time()  # 节流:失败也隔 min_interval 再试
            return False
        except Exception as e:  # 非预期异常同样不丢批次
            self._last_error = f"{type(e).__name__}: {e}"
            self.world.meeting_ai.update({"last_error": self._last_error})
            log.exception("model call crashed")
            self._last_response_at = time.time()
            return False
        self._last_response_at = time.time()
        obj = res.json_obj
        if not isinstance(obj, dict):
            # 模型没给合法 JSON:按失败处理,批次保留(避免整轮发言静默作废)
            self._last_error = f"model non-JSON: {res.text[:120]!r}"
            self.world.meeting_ai.update({"last_error": self._last_error})
            log.warning("%s", self._last_error)
            return False
        say = str(obj.get("say") or "").strip()
        vote = obj.get("vote")
        self._last_result = {
            "at": time.time(),
            "input_lines": len(chat_lines),
            "say": say,
            "vote": vote,
            "latency_ms": res.latency_ms,
            "model": res.model,
            "raw": res.text[:400],
        }
        self.world.meeting_ai.update({"last": self._last_result, "last_error": None})
        self._append_out({"type": "ai", **self._last_result, "me": me})

        if say:
            await self._send_chat(say)
        if vote is not None or self._vote_requested is not None:
            await self._try_vote(vote if isinstance(vote, str) else None)
        # 表决请求已消费:不复位会跨会议残留,让 AI 之后每轮都被迫投票
        self._vote_requested = None
        return True

    async def _send_chat(self, text: str) -> None:
        text = re.sub(r"<.*?>", "", text).replace("\n", " ").strip()
        if not text:
            return
        text = text[:120]
        await self._send({"type": "chat", "text": text})

    async def _try_vote(self, target: str | None) -> None:
        st = (self.world.state or {}).get("meeting") or {}
        if not st.get("can_vote"):
            # 挂起重试:保留最初时间戳,10s 后过期放弃(否则每次重试都刷新,永不超时)
            prev_at = self._pending_vote[1] if self._pending_vote else time.time()
            self._pending_vote = (target, prev_at)
            if self._last_result is not None:
                self._last_result["vote_deferred"] = True
            return
        names = {str((p.get("name") or "")).lower(): p.get("id")
                 for p in (self.world.state or {}).get("players") or [] if p.get("alive")}
        my_id = ((self.world.state or {}).get("me") or {}).get("player_id")
        names.pop(str(((self.world.state or {}).get("me") or {}).get("name") or "").lower(), None)
        if target is None or str(target).lower() == "skip":
            await self._send({"type": "vote", "skip": True})
            self._pending_vote = None
            return
        tid = names.get(str(target).lower())
        if tid is None:
            try:
                tid = int(target)
            except (TypeError, ValueError):
                tid = None
        if tid is None or tid == my_id:
            await self._send({"type": "vote", "skip": True})
        else:
            await self._send({"type": "vote", "target_id": tid})
        self._pending_vote = None

    def _append_out(self, obj: dict[str, Any]) -> None:
        f = self.cfg.out_file
        if f is None:
            return
        try:
            f.parent.mkdir(parents=True, exist_ok=True)
            with f.open("a", encoding="utf-8") as fh:
                fh.write(json.dumps(obj, ensure_ascii=False) + "\n")
            # 注意:这里绝不改 self._offset(那是输入 chat_file 的读取偏移;
            # 真人对局中写入输出期间其他人还在往 chat_file 写新行,误改会丢聊天)
        except OSError as e:
            self._last_error = f"write out file: {e}"

    async def _send(self, cmd: dict) -> bool:
        self._ref_n += 1
        payload = dict(cmd)
        payload["ref"] = f"mtg{self._ref_n}"
        ok = await self.bridge.send(payload)
        if ok:
            self.world.acks.appendleft({"type": "cmd_sent", "cmd": cmd.get("type"),
                                        "ref": payload["ref"], "t": time.time()})
        else:
            self._last_error = "bridge offline"
        return ok

    # ---------- 面板/测试直用 ----------

    def feed_line(self, obj: dict[str, Any]) -> None:
        """面板或脚本直接把一行会议内容写入内存(同时落盘,保证重放一致)。"""
        t = obj.get("type", "chat")
        if t == "chat":
            self._statements.append({"speaker": str(obj.get("speaker") or "?"),
                                     "text": str(obj.get("text") or ""), "t": time.time()})
            self._total_statements += 1
        elif t == "vote":
            self._vote_requested = obj.get("target")
        if self.cfg.chat_file is not None:
            try:
                self.cfg.chat_file.parent.mkdir(parents=True, exist_ok=True)
                with self.cfg.chat_file.open("a", encoding="utf-8") as fh:
                    fh.write(json.dumps(obj, ensure_ascii=False) + "\n")
                # 自己写的行不重复消费(内存里已经加过)
                self._offset = self.cfg.chat_file.stat().st_size
            except OSError:
                pass

    async def respond_now(self) -> dict[str, Any]:
        """测试/面板触发一次推理,返回最后结果。"""
        await self._respond()
        self._responded_upto = self._total_statements
        self._trigger = False
        return self._last_result or {}
