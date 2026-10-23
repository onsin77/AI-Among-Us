"""语音会议主管线:VB-CABLE 采集 → 讯飞转写(音色分离) → 轮麦状态机 → DeepSeek 发言
→ XTTS 合成 → CABLE 回输(YY 自由发言模式收音)。

轮麦规则(用户组内约定):
- 会议开始后,L1 上报 speaking_order(报警人开始,按图标从左到右、从上到下,仅存活者)。
- 轮到某人发言:他开口(ASR 出现其音色的最终句)即视为发言中;静默超过 TURN_END_GAP
  视为本轮说完,轮到下一位;轮到 AI 时立刻生成并开口。
- 说话人绑定:新音色(rl)在"当前该发言的人还没绑定过"时出现 → 绑定给该人;
  已绑定音色在别人回合出现 = 插话(记录,不推进轮次);未知音色且当前该发言者已绑定 = 旁白。
- 全部发言结束 → 表决(最怀疑者);会议结束 → 会议目标思考(下一阶段 goals)→ 注入行为引擎;
  对局结束 → 自优化记忆复盘。

手动接口:POST /api/voice {"action":"next|me|skip_me|rebind|reset|set_order|vote_now|test_tts|…}。
"""

from __future__ import annotations

import asyncio
import json
import logging
import time
from collections import deque
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

from ..bridge import BridgeClient
from ..memory import GameMemory
from ..model import DeepSeekClient, ModelError, ModelResult
from ..world import WorldModel
from . import prompts
from .audioio import CableCapture, CablePlayer, list_devices
from .xfasr import AsrConfig, Segment, XfyunAsr
from .xtts import XttsClient, clean_for_tts

log = logging.getLogger("ai_agent.voice")

UTTER_GAP = 1.8        # 秒:最终句间隔超过此值 → 关闭一条发言(转写分组)
TURN_END_GAP = 3.5     # 秒:发言活动静默超过此值 → 认为本轮说完,轮到下一位
TURN_TIMEOUT = 30.0    # 秒:轮到某人后一直没开口 → 跳过他
VOTE_DELAY = 2.0       # 秒:全部发言结束后的表决等待
MY_TURN_GUARD = 60.0   # 秒:我方"生成+播放"总超时


@dataclass
class VoiceConfig:
    enabled: bool = True
    capture_device: str = "CABLE Output"
    play_device: str = "CABLE Input"
    # 节奏
    utter_gap: float = UTTER_GAP
    turn_end_gap: float = TURN_END_GAP
    turn_timeout: float = TURN_TIMEOUT
    # 讯飞
    asr_app_id: str = ""
    asr_api_key: str = ""
    asr_api_secret: str = ""
    asr_lang: str = "autodialect"
    # TTS
    tts_base_url: str = "http://127.0.0.1:17895"
    tts_voice: str = "xiaoyan"
    tts_speed: int = 55
    tts_volume: int = 60
    # 行为
    also_chat_in_game: bool = False   # 语音发言的同时是否也发游戏内文字聊天
    out_file: str = "data/voice_out.jsonl"
    vote_tier: str = "fast"           # 表决档位(deepseek-reasoner 太慢,默认 fast)


@dataclass
class _UtterBuf:
    rl: int = 0
    parts: list[str] = field(default_factory=list)
    last_at: float = 0.0

    def text(self) -> str:
        return "".join(self.parts).strip()


class VoiceMeetingPipeline:
    def __init__(self, world: WorldModel, bridge: BridgeClient, model: DeepSeekClient,
                 memory: GameMemory, cfg: VoiceConfig) -> None:
        self.world = world
        self.bridge = bridge
        self.model = model
        self.memory = memory
        self.cfg = cfg
        self.tts = XttsClient(cfg=self._tts_cfg())
        self.capture = CableCapture(cfg.capture_device)
        self.player = CablePlayer(cfg.play_device)

        self._stop = asyncio.Event()
        self._prev_phase: str | None = None
        self.active = False                 # 会议进行中(采集+ASR 运行)
        self.manual_on = cfg.enabled        # 总开关(config/面板)

        # 会议态(每场会议重置)
        self.speaking_order: list[int] = []  # player_id 顺序
        self.turn_idx = 0
        self.spoken: set[int] = set()
        self.transcript: deque[dict[str, Any]] = deque(maxlen=120)   # {speaker,text,at}
        self.rl2pid: dict[int, int] = {}
        self.live_text = ""
        self.last_activity_at = 0.0
        self.turn_started_at = 0.0
        self.meeting_started_at = 0.0
        self._utter = _UtterBuf()
        self._last_rl = 0
        self._my_task: asyncio.Task | None = None
        self._vote_task: asyncio.Task | None = None
        self._goals_task: asyncio.Task | None = None
        self._summary_task: asyncio.Task | None = None
        self._pending_vote: tuple[str | None, float] | None = None
        self.voted = False
        self.my_state = "idle"              # idle|waiting|thinking|speaking|done
        self.my_last: dict[str, Any] | None = None
        self._ref_n = 0
        self._asr_reconnect_at = 0.0
        self._asr_reconnects = 0

        # 对局态(每局重置)
        self.suspicion: dict[str, int] = {}
        self.goals: list[dict[str, Any]] = []
        self._meeting_count_game = 0
        self._game_started_at = 0.0

        self.asr: XfyunAsr | None = None
        self.last_error: str | None = None
        self.out_file: Path | None = Path(cfg.out_file)
        # setup/teardown 互斥:相位抖动(MEETING↔PLAYING 抖动)时两者都是 ensure_future,
        # 不加锁会互相踩掉对方的 asr/采集(asr 被置 None 后 _check_asr_reconnect 直接躺平)
        self._mt_lock = asyncio.Lock()
        # TTS 静音的收尾余量:播放标志降下后残留缓冲还能响 ~百毫秒,立刻解禁会自识别
        self._muted_until = 0.0

    def _tts_cfg(self):
        from .xtts import TtsConfig
        return TtsConfig(base_url=self.cfg.tts_base_url, voice=self.cfg.tts_voice,
                         speed=self.cfg.tts_speed, volume=self.cfg.tts_volume)

    # ================= 生命周期 =================

    async def run(self) -> None:
        log.info("voice pipeline started (enabled=%s)", self.manual_on)
        while not self._stop.is_set():
            try:
                await self._tick()
            except Exception as e:
                self.last_error = f"{type(e).__name__}: {e}"
                log.exception("voice tick failed")
            await asyncio.sleep(0.1)

    async def stop(self) -> None:
        self._stop.set()
        for t in (self._my_task, self._vote_task, self._goals_task, self._summary_task):
            if t:
                t.cancel()
        await self._teardown_meeting()

    # ================= 主循环 =================

    async def _tick(self) -> None:
        st = self.world.state or {}
        phase = st.get("phase")
        if phase != self._prev_phase:
            self._on_phase_change(self._prev_phase, phase)
            self._prev_phase = phase

        if self.active:
            await self._drain_audio()
            self._adopt_speaking_order()
            self._close_stale_utter()
            self._check_turn_timeout()
            self._retry_pending_vote()
            self._check_asr_reconnect()

    def _on_phase_change(self, prev: str | None, cur: str | None) -> None:
        if cur == "MEETING" and prev != "MEETING":
            if self.manual_on:
                asyncio.ensure_future(self._setup_meeting())
        elif cur != "MEETING" and prev == "MEETING":
            asyncio.ensure_future(self._teardown_meeting(write_summary=True))
        # 对局边界(记忆/怀疑度归零)
        if cur == "PLAYING" and prev in ("LOBBY", "MENU", None):
            self.suspicion = {}
            self.goals = []
            self._meeting_count_game = 0
            self._game_started_at = time.time()
        if cur in ("LOBBY", "MENU", "ENDED") and prev in ("PLAYING", "MEETING"):
            self._fire_game_summary()

    async def _setup_meeting(self) -> None:
        async with self._mt_lock:
            if self.active:
                return
            self.active = True
            # 语音会议期间接管会议大脑:暂停文件会议管线,防止双重应答(散会恢复)
            mp = getattr(self, "_meeting", None)
            if mp is not None:
                self._meeting_prev_enabled = bool(mp.cfg.enabled)
                mp.cfg.enabled = False
            self._meeting_count_game += 1
            now = time.time()
            self.meeting_started_at = now
            self.turn_idx = 0
            self.spoken = set()
            self.transcript.clear()
            self.rl2pid = {}
            self._utter = _UtterBuf()
            self._last_rl = 0
            self.live_text = ""
            self.voted = False
            self.my_state = "waiting"
            self.my_last = None
            self.last_activity_at = now
            self.turn_started_at = now
            self.speaking_order = []          # 等 L1 上报
            log.info("voice meeting #%d setup", self._meeting_count_game)

            # 上一场散会尾巴还积在采集队列里:先丢弃,防止当成这场的新发言(幽灵发言)
            self.capture.drain()
            if not self.capture.running:
                ok = await self.capture.start()
                if not ok:
                    log.warning("capture 启动失败:%s(继续用超时轮次推进)", self.capture.last_error)
            self._asr_reconnects = 0
            await self._start_asr()
            self._maybe_trigger_my_turn(force_reporter_check=True)

    async def _start_asr(self) -> None:
        was_session = self.asr is not None
        if self.asr is not None:
            try:
                await self.asr.close()
            except Exception:
                pass
            self.asr = None
        asr = XfyunAsr(AsrConfig(
            app_id=self.cfg.asr_app_id, api_key=self.cfg.asr_api_key,
            api_secret=self.cfg.asr_api_secret, lang=self.cfg.asr_lang), on_segment=self._on_segment)
        ok = await asr.start()
        if not ok:
            self.last_error = asr.last_error
            log.warning("asr start failed: %s", asr.last_error)
            self._schedule_asr_reconnect()
        else:
            # 重连成功:rl 音色编号是新会话重新分配的,旧绑定不可信,清掉按轮次重绑
            if was_session and self.rl2pid:
                log.info("asr 重连,清空 %d 条音色绑定待重绑", len(self.rl2pid))
                self.rl2pid = {}
                self._last_rl = 0
        self.asr = asr

    def _schedule_asr_reconnect(self) -> None:
        self._asr_reconnect_at = time.time() + 3.0

    def _check_asr_reconnect(self) -> None:
        if not self.active or self.asr is None:
            return
        if self.asr.connected:
            return
        if time.time() < self._asr_reconnect_at:
            return
        self._asr_reconnects += 1
        if self._asr_reconnects > 60:
            self.last_error = "asr 重连次数耗尽(本场会议放弃 ASR,轮麦仅靠超时推进)"
            return
        self._schedule_asr_reconnect()
        asyncio.ensure_future(self._start_asr())

    async def _teardown_meeting(self, write_summary: bool = False) -> None:
        async with self._mt_lock:
            was_active = self.active
            self.active = False
            mp = getattr(self, "_meeting", None)
            if mp is not None and getattr(self, "_meeting_prev_enabled", None) is not None:
                mp.cfg.enabled = self._meeting_prev_enabled
                self._meeting_prev_enabled = None
            self._close_stale_utter(force=True)
            if self.asr is not None:
                try:
                    await self.asr.close()
                except Exception:
                    pass
                self.asr = None
            if self._my_task and not self._my_task.done():
                self._my_task.cancel()
                self._my_task = None
            if self._vote_task and not self._vote_task.done():
                self._vote_task.cancel()
                self._vote_task = None
            self.capture.stop()
            self.player.stop()
            self.my_state = "idle"
            if was_active and write_summary:
                self._append_out({"type": "voice_meeting_end", "at": time.time(),
                                  "utterances": len(self.transcript),
                                  "turns_done": len(self.spoken), "voted": self.voted,
                                  "suspicion": self.suspicion})
                self._fire_goals()

    # ---------- 音频面 ----------

    async def _drain_audio(self) -> None:
        if not self.capture.running:
            return
        # 我正在播 TTS:喂静音,避免把自己转写进去;结束后再保持 0.3s 静音,
        # 盖住播放设备残留缓冲的尾音与整批搬运的采样空窗
        if self.player.speaking:
            self._muted_until = time.time() + 0.3
        muted = time.time() < self._muted_until
        n = 0
        while n < 60:                       # 每轮最多搬 60 帧(2.4s),防卡 tick
            frame = await self.capture.read()
            if frame is None:
                break
            n += 1
            if self.asr is not None and self.asr.connected:
                await self.asr.feed(b"\x00" * len(frame) if muted else frame)

    # ---------- 转写回调(ASR rx 任务内) ----------

    async def _on_segment(self, seg: Segment) -> None:
        if not self.active:
            return
        now = time.time()
        if seg.rl:
            self._last_rl = seg.rl
        rl = seg.rl or self._last_rl
        if not seg.final:
            self.live_text = seg.text
            return
        self.live_text = ""
        # 分组:同音色且间隔小 → 同一条发言
        if self._utter.parts and (self._utter.rl != rl or now - self._utter.last_at > self.cfg.utter_gap):
            self._close_utter()
        if not self._utter.parts:
            self._utter.rl = rl
        self._utter.parts.append(seg.text)
        self._utter.last_at = now
        self.last_activity_at = now

    def _close_stale_utter(self, force: bool = False) -> None:
        if not self._utter.parts:
            return
        gap = time.time() - self._utter.last_at
        if force or gap > self.cfg.utter_gap:
            self._close_utter()

    def _close_utter(self) -> None:
        text = self._utter.text()
        rl = self._utter.rl
        self._utter = _UtterBuf()
        if not text:
            return
        now = time.time()
        expected = self._expected_pid()
        owner_pid: int | None = None
        if rl and rl in self.rl2pid:
            owner_pid = self.rl2pid[rl]
        elif rl and expected is not None and expected not in self.spoken:
            # 新音色出现在"该他发言"的窗口 → 绑定
            self.rl2pid[rl] = expected
            owner_pid = expected
            log.info("voice bind rl=%d -> %s", rl, self._pid_name(expected))
        name = self._pid_name(owner_pid) if owner_pid is not None else f"音色{rl or '?'}"
        self.transcript.append({"speaker": name, "text": text, "at": now, "rl": rl})
        self.last_activity_at = now
        self._append_out({"type": "voice_utter", "at": now, "speaker": name, "rl": rl, "text": text})
        if owner_pid is not None and owner_pid == expected:
            self.spoken.add(owner_pid)
            self._advance_turn()

    # ---------- 轮麦推进 ----------

    def _expected_pid(self) -> int | None:
        if 0 <= self.turn_idx < len(self.speaking_order):
            return self.speaking_order[self.turn_idx]
        return None

    def _me_id(self) -> int | None:
        return ((self.world.state or {}).get("me") or {}).get("player_id")

    def _my_pos(self) -> int:
        me = self._me_id()
        try:
            return self.speaking_order.index(me)
        except (ValueError, TypeError):
            return -1

    def _adopt_speaking_order(self) -> None:
        if self.speaking_order:
            return
        mtg = (self.world.state or {}).get("meeting") or {}
        order = mtg.get("speaking_order")
        if isinstance(order, list) and order:
            self.speaking_order = [int(x) for x in order]
            self.turn_idx = 0
            self.turn_started_at = time.time()
            log.info("speaking_order: %s", [self._pid_name(p) for p in self.speaking_order])
            self._maybe_trigger_my_turn(force_reporter_check=True)

    def _maybe_trigger_my_turn(self, force_reporter_check: bool = False) -> None:
        if not self.active or not self.speaking_order:
            return
        if self.my_state in ("thinking", "speaking", "done"):
            return
        if self._my_task is not None and not self._my_task.done():
            return   # 防竞态:advance 与 tick 超时检查可能接连触发,只允许一个发言任务
        me = self._me_id()
        if me is None:
            return
        if not force_reporter_check and self._expected_pid() != me:
            return
        # 轮到我(或我是报警人且队首是我)
        if self._expected_pid() != me:
            return
        if me in self.spoken:
            return
        self._my_task = asyncio.ensure_future(self._my_speak())

    def _check_turn_timeout(self) -> None:
        if not self.active or not self.speaking_order:
            return
        me = self._me_id()
        expected = self._expected_pid()
        if expected == me:
            busy = (self.my_state in ("thinking", "speaking")
                    or (self._my_task is not None and not self._my_task.done()))
            if busy:
                self.turn_started_at = time.time()   # 我方发言由 _my_speak 流程收尾,不超时
                self._maybe_trigger_my_turn()
                return
            if me in self.spoken:
                if time.time() - self.last_activity_at > self.cfg.turn_end_gap:
                    self._advance_turn()
                return
            # 发言任务不在跑也没说完(异常残留):落到底部超时兜底,我方轮次不许永久卡死
        now = time.time()
        if expected is None:
            return
        if expected in self.spoken:
            if now - self.last_activity_at > self.cfg.turn_end_gap:
                self._advance_turn()
            return
        # 该他发言但还没开口
        if now - max(self.turn_started_at, self.last_activity_at) > self.cfg.turn_timeout:
            self._append_out({"type": "voice_turn_skip", "at": now,
                              "speaker": self._pid_name(expected), "reason": "timeout"})
            self.spoken.add(expected)
            self._advance_turn()

    def _advance_turn(self) -> None:
        self.turn_idx += 1
        n = len(self.speaking_order)
        # 跳过已说过/已跳过(在 spoken 集里)的人
        while self.turn_idx < n and self.speaking_order[self.turn_idx] in self.spoken:
            self.turn_idx += 1
        self.turn_started_at = time.time()
        if self.turn_idx >= n:
            log.info("所有发言轮次结束,%.1fs 后表决", VOTE_DELAY)
            if self._vote_task is None or self._vote_task.done():
                self._vote_task = asyncio.ensure_future(self._vote_after_delay())
            return
        self._maybe_trigger_my_turn()

    async def _vote_after_delay(self) -> None:
        await asyncio.sleep(VOTE_DELAY)
        if not self.active or self.voted:
            return
        await self._do_vote()

    # ---------- 我方发言 ----------

    async def _my_speak(self) -> None:
        self.my_state = "thinking"
        me = self._me_id()
        idx = self._my_pos()
        total = len(self.speaking_order)
        t0 = time.time()
        say, sus, err = "", {}, None
        try:
            messages = self._build_speech_messages(idx, total)
            res: ModelResult = await asyncio.wait_for(
                self.model.chat(messages, tier=prompts.tier_for_position(idx, total),
                                json_mode=True, temperature=0.7,
                                max_tokens=prompts.max_tokens_for_position(idx, total)),
                timeout=12.0)
            obj = res.json_obj or {}
            say = str(obj.get("say") or "").strip()
            sus = obj.get("sus") if isinstance(obj.get("sus"), dict) else {}
            self.my_last = {"at": t0, "latency_ms": res.latency_ms, "model": res.model,
                            "pos": idx, "say": say}
        except (ModelError, asyncio.TimeoutError, Exception) as e:
            err = f"{type(e).__name__}: {e}"
            log.warning("my speech model failed: %s", err)
            self.my_last = {"at": t0, "latency_ms": int((time.time() - t0) * 1000), "pos": idx,
                            "say": "", "error": err}
        self._merge_suspicion(sus)
        try:
            clean = clean_for_tts(say)
            if not clean:
                # 兜底:极简发言(不能冷场)
                clean = self._fallback_line(idx)
                if self.my_last is not None:
                    self.my_last["fallback"] = True

            self.my_state = "speaking"
            self._append_out({"type": "voice_turn", "at": time.time(), "speaker": self._pid_name(me),
                              "pos": idx, "text": clean, "sus": self.suspicion,
                              "latency_ms": (self.my_last or {}).get("latency_ms")})
            wav = await self.tts.synth(clean)
            ok = False
            if wav:
                ok = self.player.play_wav_bytes(wav)
            if not ok:
                log.warning("tts 播放失败,仅文字:%s", self.player.last_error or self.tts.cfg.base_url)
            if self.cfg.also_chat_in_game:
                await self._send({"type": "chat", "text": clean[:120]})
            # 等播放完(≤ 上限)
            deadline = time.time() + MY_TURN_GUARD
            while self.player.speaking and time.time() < deadline:
                await asyncio.sleep(0.15)
        except asyncio.CancelledError:
            raise
        except Exception as e:
            # TTS/播放链路意外异常:必须收敛轮次,否则 my_state 卡在 speaking、整场轮麦死锁
            self.last_error = f"my speak: {type(e).__name__}: {e}"
            log.exception("my speak failed after model")
        finally:
            self.live_text = ""
            self._finish_my_turn(me)

    def _finish_my_turn(self, me: int | None) -> None:
        self.my_state = "done"
        if me is not None:
            self.spoken.add(me)
        self._advance_turn()

    def _fallback_line(self, idx: int) -> str:
        if idx <= 0:
            return "我这边刚发现的情况都说了,先这样,大家怎么看?"
        role = self._my_role_text()
        if "Impostor" in role or "Neutral" in role:
            return "我先过,我没什么特别的信息,听大家的。"
        return "我刚在做任务,没看到可疑的情况,先听大家说。"

    # ---------- 表决 ----------

    async def _do_vote(self) -> None:
        if self.voted or not self.active:
            return
        me_alive = ((self.world.state or {}).get("me") or {}).get("alive")
        if not me_alive:
            self.voted = True   # 死人不投
            return
        t0 = time.time()
        vote, reason = "skip", ""
        try:
            messages = self._build_vote_messages()
            res = await asyncio.wait_for(
                self.model.chat(messages, tier=self.cfg.vote_tier, json_mode=True,
                                temperature=0.4, max_tokens=260), timeout=15.0)
            obj = res.json_obj or {}
            vote = obj.get("vote") if isinstance(obj.get("vote"), str) else "skip"
            reason = str(obj.get("reason") or "")
            self._merge_suspicion(obj.get("sus") if isinstance(obj.get("sus"), dict) else {})
        except (ModelError, asyncio.TimeoutError, Exception) as e:
            log.warning("vote model failed: %s", e)
            vote = "skip"
        self.voted = True
        self._append_out({"type": "voice_vote", "at": time.time(), "vote": vote,
                          "reason": reason, "suspicion": self.suspicion,
                          "latency_ms": int((time.time() - t0) * 1000)})
        await self._try_vote(vote)

    async def _try_vote(self, target: str | None) -> None:
        st = (self.world.state or {}).get("meeting") or {}
        if not st.get("can_vote"):
            self._pending_vote = (target, time.time())
            return
        await self._send_vote_cmd(target)

    async def _send_vote_cmd(self, target: str | None) -> None:
        self._pending_vote = None
        players = ((self.world.state or {}).get("players") or [])
        names = {str(p.get("name") or "").lower(): p.get("id") for p in players if p.get("alive")}
        my_name = str(((self.world.state or {}).get("me") or {}).get("name") or "").lower()
        names.pop(my_name, None)
        if target is None or str(target).lower() == "skip":
            await self._send({"type": "vote", "skip": True})
            return
        tid = names.get(str(target).lower())
        if tid is None:
            await self._send({"type": "vote", "skip": True})
        else:
            await self._send({"type": "vote", "target_id": tid})

    def _retry_pending_vote(self) -> None:
        if self._pending_vote is None or self.voted:
            return
        target, at = self._pending_vote
        if time.time() - at > 20.0:
            self._pending_vote = None
            return
        st = (self.world.state or {}).get("meeting") or {}
        if st.get("can_vote"):
            asyncio.ensure_future(self._send_vote_cmd(target))

    # ---------- 会议目标 / 复盘 ----------

    def _fire_goals(self) -> None:
        if not self.transcript and not self.suspicion:
            return   # 整场会议毫无信息(无转写也无怀疑度)才跳过
        self._goals_task = asyncio.ensure_future(self._do_goals())

    async def _do_goals(self) -> None:
        if self.model is None or not self.model.available:
            return
        try:
            res = await asyncio.wait_for(self.model.chat(
                self._build_goals_messages(), tier="fast", json_mode=True,
                temperature=0.5, max_tokens=400), timeout=20.0)
            obj = res.json_obj or {}
            goals = [g for g in (obj.get("goals") or []) if isinstance(g, dict)][:4]
            self.goals = goals
            self.world.meeting_ai["goals"] = goals
            self._append_out({"type": "goals", "at": time.time(), "goals": goals,
                              "summary": obj.get("summary")})
            log.info("会议目标:%s", obj.get("summary"))
            # 轻量注入行为引擎(仅倾向,不改变硬规则)
            beh = getattr(self, "_behavior", None)
            if beh is not None:
                try:
                    beh.set_meeting_goals(goals)
                except Exception:
                    pass
        except (ModelError, asyncio.TimeoutError, Exception) as e:
            log.warning("goals model failed: %s", e)

    def _fire_game_summary(self) -> None:
        if self._meeting_count_game == 0 and not self.suspicion:
            return
        self._summary_task = asyncio.ensure_future(self._do_game_summary())

    async def _do_game_summary(self) -> None:
        if self.model is None or not self.model.available:
            return
        try:
            res = await asyncio.wait_for(self.model.chat(
                self._build_summary_messages(), tier="fast", json_mode=True,
                temperature=0.6, max_tokens=420), timeout=25.0)
            obj = res.json_obj or {}
            self.memory.record_game({
                "at": time.strftime("%Y-%m-%d %H:%M"),
                "role": self._my_role_text(),
                "map": (self.world.state or {}).get("map"),
                "meetings": self._meeting_count_game,
                "summary": obj.get("summary"),
                "result": obj.get("result"),
                "lessons": obj.get("lessons") or [],
                "player_notes": obj.get("player_notes") or {},
                "final_suspicion": self.suspicion,
            })
            log.info("对局复盘已写入记忆")
        except (ModelError, asyncio.TimeoutError, Exception) as e:
            log.warning("game summary failed: %s", e)

    # ---------- 提示词组装 ----------

    def _my_role_text(self) -> str:
        me = (self.world.state or {}).get("me") or {}
        return f"{me.get('role') or '未知'}({me.get('team') or '?'})"

    def _pid_name(self, pid: int | None) -> str:
        if pid is None:
            return "?"
        st = self.world.state or {}
        me = st.get("me") or {}
        if me.get("player_id") == pid:
            return str(me.get("name") or "我")
        for p in st.get("players") or []:
            if p.get("id") == pid:
                return str(p.get("name") or f"#{pid}")
        return f"#{pid}"

    def _roster(self) -> list[str]:
        st = self.world.state or {}
        me_id = (st.get("me") or {}).get("player_id")
        return [str(p.get("name")) for p in (st.get("players") or [])
                if p.get("alive") and p.get("id") != me_id]

    def _transcript_text(self) -> str:
        my_name = self._pid_name(self._me_id())
        lines = []
        for u in self.transcript:
            lines.append(f"{u['speaker']}: {u['text']}")
        return "\n".join(lines) if lines else "(还没有人发言)"

    def _my_journey_text(self) -> str:
        """给模型的我方行程/事件摘要(基于世界模型,不编造)。"""
        st = self.world.state or {}
        me = st.get("me") or {}
        parts: list[str] = []
        journey = list(getattr(self.world, "journey", []) or [])
        if journey:
            rooms = []
            for _, r in journey[-60:]:
                if not rooms or rooms[-1] != r:
                    rooms.append(r)
            parts.append("本轮去过的区域:" + " → ".join(rooms[-12:]))
        tasks = (me.get("tasks") or {})
        done = [t.get("name") for t in (tasks.get("list") or []) if t.get("done")]
        if done:
            parts.append(f"已完成任务:{len(done)} 个(" + ",".join(str(x) for x in done[:6]) + ")")
        elif tasks.get("total"):
            parts.append("任务进度 0(注意:如果你是坏人,你的'任务'是假装做)")
        kills = 0
        for ev in self.world.events:
            if ev.get("code") == "KILL_CMD":
                kills += 1
        if kills:
            parts.append(f"本轮你实际击杀次数:{kills}(仅你自己知道)")
        deaths = [ev for ev in self.world.events if ev.get("code") == "DEATH"]
        if deaths:
            parts.append("本轮已知的死亡事件:" +
                         ";".join(f"{ev.get('target') or ev.get('actor')}@{ev.get('loc') or '?'}"
                                  for ev in deaths[-5:]))
        sabs = [s.get("type") for s in (st.get("sabotages") or []) if s.get("active")]
        if sabs:
            parts.append("当前破坏:" + ",".join(str(x) for x in sabs))
        return "\n".join(parts) if parts else "(没有可用的行程信息)"

    def _suspicion_text(self) -> str:
        if not self.suspicion:
            return "(暂无)"
        return ", ".join(f"{k}={v}" for k, v in sorted(self.suspicion.items(),
                                                       key=lambda kv: -kv[1]))

    def _role_hint(self) -> str:
        team = str((((self.world.state or {}).get("me") or {}).get("team")) or "").lower()
        if team.startswith("impostor"):
            return prompts.ROLE_HINTS["impostor"]
        if team.startswith("neutral"):
            return prompts.ROLE_HINTS["neutral"]
        return prompts.ROLE_HINTS["crew"]

    def _base_context(self) -> str:
        st = self.world.state or {}
        roster = self._roster()
        mem = self.memory.prompt_snippet(roster)
        return (
            f"轮次:{st.get('round')}。存活玩家(不含你):{', '.join(roster) or '(无)'}。\n"
            f"你到目前的行程/事件:\n{self._my_journey_text()}\n\n"
            f"你当前对各玩家的怀疑度:{self._suspicion_text()}\n"
            + (f"\n{mem}" if mem else "")
        )

    def _build_speech_messages(self, idx: int, total: int) -> list[dict[str, str]]:
        pos_text, level = prompts.position_prompt(idx, total)
        user = (
            f"{self._base_context()}\n"
            f"发言顺序共 {total} 人,你是第 {idx + 1} 个发言。\n"
            f"到目前为止的会议发言记录:\n{self._transcript_text()}\n\n"
            f"【当前位次要求(思考强度:{level})】{pos_text}\n"
            "现在轮到你了,立刻给出要说的话。"
        )
        me = self._pid_name(self._me_id())
        return [{"role": "system", "content": prompts.SYSTEM_VOICE.format(
            name=me, role=self._my_role_text().split("(")[0],
            team=((self.world.state or {}).get("me") or {}).get("team") or "?",
            role_hint=self._role_hint())},
            {"role": "user", "content": user}]

    def _build_vote_messages(self) -> list[dict[str, str]]:
        user = (
            f"{self._base_context()}\n"
            f"全部发言记录:\n{self._transcript_text()}\n\n"
            "发言阶段已结束,现在表决:给出 vote(玩家名或 skip)与一句话理由。"
        )
        me = self._pid_name(self._me_id())
        return [{"role": "system", "content": prompts.VOTE_SYSTEM.format(
            name=me, role=self._my_role_text())},
            {"role": "user", "content": user}]

    def _build_goals_messages(self) -> list[dict[str, str]]:
        user = (
            f"{self._base_context()}\n"
            f"上一场会议发言:\n{self._transcript_text()}\n\n"
            f"会议后怀疑度:{self._suspicion_text()}\n"
            "请给出会议结束后的行动目标(goals)。"
        )
        me = self._pid_name(self._me_id())
        return [{"role": "system", "content": prompts.GOALS_SYSTEM.format(
            name=me, role=self._my_role_text())},
            {"role": "user", "content": user}]

    def _build_summary_messages(self) -> list[dict[str, str]]:
        events = [f"[{ev.get('round')}] {ev.get('code')} {ev.get('actor') or ''} {ev.get('note') or ''}"
                  for ev in list(self.world.events)[-40:]]
        user = (
            f"你的身份:{self._my_role_text()}。会议次数:{self._meeting_count_game}。\n"
            f"最终怀疑度:{self._suspicion_text()}\n"
            f"对局事件摘录:\n" + ("\n".join(events) if events else "(无)") + "\n"
            f"各会议发言摘录(最后一场):\n{self._transcript_text()}\n"
            "请复盘本局并输出经验教训。"
        )
        me = self._pid_name(self._me_id())
        return [{"role": "system", "content": prompts.GAME_SUMMARY_SYSTEM.format(name=me)},
            {"role": "user", "content": user}]

    def _merge_suspicion(self, sus: dict | None) -> None:
        if not isinstance(sus, dict):
            return
        roster = set(self._roster())
        for k, v in sus.items():
            name = str(k)
            if roster and name not in roster:
                continue
            try:
                val = max(0, min(100, int(round(float(v)))))
            except (TypeError, ValueError):
                continue
            old = self.suspicion.get(name)
            self.suspicion[name] = val if old is None else int(round(old * 0.4 + val * 0.6))

    # ---------- 工具 ----------

    def _append_out(self, obj: dict[str, Any]) -> None:
        f = self.out_file
        if f is None:
            return
        try:
            f.parent.mkdir(parents=True, exist_ok=True)
            with f.open("a", encoding="utf-8") as fh:
                fh.write(json.dumps(obj, ensure_ascii=False) + "\n")
        except OSError as e:
            self.last_error = f"write out: {e}"

    async def _send(self, cmd: dict) -> bool:
        self._ref_n += 1
        payload = dict(cmd)
        payload["ref"] = f"voc{self._ref_n}"
        ok = await self.bridge.send(payload)
        if ok:
            self.world.acks.appendleft({"type": "cmd_sent", "cmd": cmd.get("type"),
                                        "ref": payload["ref"], "t": time.time()})
        else:
            self.last_error = "bridge offline"
        return ok

    # ---------- 状态/面板 ----------

    def apply_timing(self, turn_end_gap: float, turn_timeout: float) -> None:
        """临时改轮麦节奏(bot 局:无人说话,缩短等待;停用后 restore_timing 还原)。"""
        if not hasattr(self, "_timing_default"):
            self._timing_default = (self.cfg.turn_end_gap, self.cfg.turn_timeout)
        self.cfg.turn_end_gap = float(turn_end_gap)
        self.cfg.turn_timeout = float(turn_timeout)

    def restore_timing(self) -> None:
        if hasattr(self, "_timing_default"):
            self.cfg.turn_end_gap, self.cfg.turn_timeout = self._timing_default

    def status(self) -> dict[str, Any]:
        st = self.world.state or {}
        expected = self._expected_pid()
        return {
            "enabled": self.manual_on,
            "active": self.active,
            "phase": st.get("phase"),
            "capture": {"running": self.capture.running, "peak": self.capture.peak,
                        "device": self.capture.device, "error": self.capture.last_error},
            "asr": {"connected": bool(self.asr and self.asr.connected),
                    "segments": self.asr.segments if self.asr else 0,
                    "reconnects": self._asr_reconnects,
                    "error": self.asr.last_error if self.asr else None},
            "tts": {"speaking": self.player.speaking, "voice": self.tts.cfg.voice,
                    "ok": self.tts.available},
            "speaking_order": [self._pid_name(p) for p in self.speaking_order],
            "turn_idx": self.turn_idx,
            "current_speaker": self._pid_name(expected) if expected is not None else None,
            "is_my_turn": expected is not None and expected == self._me_id(),
            "my_state": self.my_state,
            "spoken": [self._pid_name(p) for p in sorted(self.spoken)],
            "bindings": {str(rl): self._pid_name(pid) for rl, pid in self.rl2pid.items()},
            "live_text": self.live_text,
            "transcript": list(self.transcript)[-40:],
            "suspicion": dict(sorted(self.suspicion.items(), key=lambda kv: -kv[1])),
            "my_last": self.my_last,
            "voted": self.voted,
            "goals": self.goals,
            "memory": self.memory.stats(),
            "last_error": self.last_error,
        }

    # ---------- 手动控制 ----------

    async def control(self, body: dict[str, Any]) -> dict[str, Any]:
        action = str(body.get("action") or "")
        if action in ("next", "me", "skip_me", "vote_now") and not self.active:
            return {"ok": False, "error": "当前不在语音会议中"}
        if action == "enable":
            self.manual_on = True
            if (self.world.state or {}).get("phase") == "MEETING":
                await self._setup_meeting()
        elif action == "disable":
            self.manual_on = False
            await self._teardown_meeting()
        elif action == "next":
            self._append_out({"type": "voice_turn_skip", "at": time.time(),
                              "speaker": self._pid_name(self._expected_pid()),
                              "reason": "manual"})
            expected = self._expected_pid()
            if expected is not None:
                self.spoken.add(expected)
            self._advance_turn()
        elif action == "me":
            if self.my_state in ("thinking", "speaking"):
                return {"ok": True, "note": "已在发言中"}
            if self._my_task is not None and not self._my_task.done():
                return {"ok": True, "note": "发言任务进行中"}
            self.my_state = "waiting"
            self._my_task = asyncio.ensure_future(self._my_speak())
        elif action == "skip_me":
            me = self._me_id()
            if me is not None:
                self.spoken.add(me)
            self.my_state = "done"
            self._advance_turn()
        elif action == "rebind":
            try:
                rl = int(body.get("rl"))
                player = str(body.get("player") or "")
            except (TypeError, ValueError):
                return {"ok": False, "error": "rl/player 参数错误"}
            pid = self._name_pid(player)
            if pid is None:
                return {"ok": False, "error": f"找不到玩家 {player}"}
            old = self.rl2pid.get(rl)
            self.rl2pid = {k: v for k, v in self.rl2pid.items() if v != pid}
            self.rl2pid[rl] = pid
            self._append_out({"type": "voice_rebind", "at": time.time(), "rl": rl,
                              "player": player, "replaced": self._pid_name(old)})
        elif action == "reset":
            self.rl2pid = {}
            self._last_rl = 0
        elif action == "set_order":
            players = body.get("players")
            if isinstance(players, list) and players:
                order = [self._name_pid(str(p)) for p in players]
                if None in order:
                    return {"ok": False, "error": "名单里有未知玩家"}
                self.speaking_order = [p for p in order if p is not None]
                self.turn_idx = min(self.turn_idx, max(0, len(self.speaking_order) - 1))
        elif action == "vote_now":
            if self._vote_task is None or self._vote_task.done():
                self._vote_task = asyncio.ensure_future(self._do_vote())
        elif action == "test_tts":
            text = str(body.get("text") or "大家好,这是语音测试。")
            wav = await self.tts.synth(text)
            ok = bool(wav) and self.player.play_wav_bytes(wav or b"")
            return {"ok": ok, "speaking": self.player.speaking}
        elif action == "set_voice":
            v = str(body.get("voice") or "")
            if v in ("xiaoyan", "xiaofeng"):
                self.tts.cfg.voice = v
            else:
                return {"ok": False, "error": "voice 必须是 xiaoyan|xiaofeng"}
        else:
            return {"ok": False, "error": f"未知 action:{action}"}
        return {"ok": True}

    def _name_pid(self, name: str) -> int | None:
        st = self.world.state or {}
        me = st.get("me") or {}
        if str(me.get("name") or "") == name:
            return me.get("player_id")
        for p in st.get("players") or []:
            if str(p.get("name") or "") == name:
                return p.get("id")
        return None
