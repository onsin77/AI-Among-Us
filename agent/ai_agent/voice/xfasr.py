"""讯飞 RTASR-LLM 实时语音转写客户端(asyncio websockets,零第三方依赖除 websockets)。

文档:wss://office-api-ast-dx.iflyaisol.com/ast/communicate/v1
鉴权:参数按 key 升序 → urlencode 的 k=v&… → HmacSHA1(accessKeySecret) → Base64 → signature。
音频:pcm_s16le 16k 单声道,每 40ms 发 1280 字节 binary 帧;15s 无音频会被断开。
结束:文本帧 {"end":true,"sessionId":…}(sessionId 来自 started 响应)。
说话人分离:role_type=2 盲分;词级字段 rl:非零=切换到该说话人,0=延续上一句的说话人。

对上层暴露:
    session = XfyunAsr(cfg, on_segment=cb)
    await session.start()          # 建连,等 started
    await session.feed(pcm_bytes)  # 喂 16k mono pcm(内部按 1280B/40ms 节流发送)
    await session.close()          # 发 end 并收尾
    session.connected / session.last_error / session.reconnects
回调 on_segment(Segment):type=0 最终句 / 1 中间句;text 拼好的文本;rl 说话人(0=未知延续)。
"""

from __future__ import annotations

import asyncio
import base64
import hashlib
import hmac
import json
import logging
import time
import urllib.parse
import uuid
from dataclasses import dataclass, field
from typing import Any, Awaitable, Callable

log = logging.getLogger("ai_agent.xfasr")

WS_URL = "wss://office-api-ast-dx.iflyaisol.com/ast/communicate/v1"
FRAME_BYTES = 1280          # 40ms @16k s16le mono
FRAME_INTERVAL = 0.040
KEEPALIVE_SEC = 12.0        # 超 15s 无音频会被断,12s 补一帧静音


@dataclass
class AsrConfig:
    app_id: str = ""
    api_key: str = ""        # accessKeyId
    api_secret: str = ""     # accessKeySecret
    lang: str = "autodialect"
    role_type: int = 2       # 2=开启实时角色分离(盲分音色)
    pd: str = "game"         # 领域参数:游戏


@dataclass
class Segment:
    """一句转写结果(词已拼接)。"""
    seg_id: int
    final: bool
    text: str
    rl: int                  # 非零=说话人id(通常标在句首词);0=延续上一句说话人
    at: float = field(default_factory=time.time)


def build_signed_url(cfg: AsrConfig) -> str:
    # %z 生成真实本地时区偏移(+0800):写死字面量在非东八区机器上会因时间窗校验失败
    utc = time.strftime("%Y-%m-%dT%H:%M:%S%z", time.localtime())
    params = {
        "accessKeyId": cfg.api_key,
        "appId": cfg.app_id,
        "audio_encode": "pcm_s16le",
        "lang": cfg.lang,
        "pd": cfg.pd,
        "role_type": str(cfg.role_type),
        "samplerate": "16000",
        "utc": utc,
        "uuid": str(uuid.uuid4()),
    }
    base = "&".join(f"{urllib.parse.quote(k, safe='')}={urllib.parse.quote(v, safe='')}"
                    for k, v in sorted(params.items()))
    sig = base64.b64encode(hmac.new(cfg.api_secret.encode(), base.encode(), hashlib.sha1).digest()).decode()
    params["signature"] = sig
    return WS_URL + "?" + "&".join(f"{urllib.parse.quote(k, safe='')}={urllib.parse.quote(v, safe='')}"
                                   for k, v in params.items())


def parse_segment(msg: dict[str, Any]) -> Segment | None:
    """从响应 JSON 抽一句文本;无有效文本返回 None。"""
    data = msg.get("data") or {}
    st = ((data.get("cn") or {}).get("st") or {})
    rt_list = st.get("rt") or []
    if not rt_list:
        return None
    text_parts: list[str] = []
    rl = 0
    for rt in rt_list:
        for wseg in rt.get("ws") or []:
            for cw in wseg.get("cw") or []:
                text_parts.append(str(cw.get("w") or ""))
                r = cw.get("rl") or 0
                try:
                    r = int(r)
                except (TypeError, ValueError):
                    r = 0
                if r:
                    rl = r
    text = "".join(text_parts).strip()
    if not text:
        return None
    try:
        seg_id = int(data.get("seg_id", -1))
    except (TypeError, ValueError):
        seg_id = -1
    return Segment(seg_id=seg_id, final=(st.get("type") == "0"), text=text, rl=rl)


class XfyunAsr:
    """单会议一条会话;断线由上层(voice pipeline)重建。"""

    def __init__(self, cfg: AsrConfig,
                 on_segment: Callable[[Segment], Awaitable[None] | None]) -> None:
        self.cfg = cfg
        self._on_segment = on_segment
        self._ws = None
        self._session_id = ""
        self._rx_task: asyncio.Task | None = None
        self._send_task: asyncio.Task | None = None
        self._queue: asyncio.Queue[bytes | None] = asyncio.Queue(maxsize=400)
        self._last_sent_at = 0.0
        self._closing = False
        self.connected = False
        self.last_error: str | None = None
        self.started_at: float | None = None
        self.segments = 0
        self.bytes_in = 0

    # ---------- 生命周期 ----------

    async def start(self) -> bool:
        try:
            import websockets
        except ImportError:
            self.last_error = "websockets 未安装(pip install websockets)"
            return False
        if not (self.cfg.app_id and self.cfg.api_key and self.cfg.api_secret):
            self.last_error = "asr 凭据未配置([asr] app_id/api_key/api_secret)"
            return False
        url = build_signed_url(self.cfg)
        try:
            # ping 保活:静默断链(NAT 超时等)必须能被检测出来,否则 connected 恒真、
            # 上层永不重连,整场会议"装聋"。ping_interval=None 会关掉这层检测。
            self._ws = await asyncio.wait_for(
                websockets.connect(url, max_size=2 ** 22,
                                   ping_interval=15, ping_timeout=10, close_timeout=5), 12)
        except Exception as e:
            self.last_error = f"ws connect: {e}"
            log.warning("asr connect failed: %s", e)
            return False
        try:
            raw = await asyncio.wait_for(self._ws.recv(), 12)
            started = json.loads(raw)
            self._session_id = ((started.get("data") or {}).get("sessionId")) or started.get("sid") or ""
        except Exception as e:
            self.last_error = f"started: {e}"
            await self._safe_close()
            return False
        self.connected = True
        self.started_at = time.time()
        self._last_sent_at = time.time()
        self._rx_task = asyncio.create_task(self._rx_loop())
        self._send_task = asyncio.create_task(self._tx_loop())
        log.info("asr session started sid=%s", self._session_id[:8])
        return True

    async def close(self) -> None:
        self._closing = True
        # 发送 end(发后不得再发音频)
        ws = self._ws
        if ws is not None and self.connected:
            try:
                await ws.send(json.dumps({"end": True, "sessionId": self._session_id}))
            except Exception:
                pass
        if self._send_task:
            self._send_task.cancel()
        if self._rx_task:
            try:
                await asyncio.wait_for(asyncio.shield(self._rx_task), 5)
            except (asyncio.TimeoutError, asyncio.CancelledError):
                self._rx_task.cancel()
        await self._safe_close()

    async def _safe_close(self) -> None:
        ws = self._ws
        self._ws = None
        self.connected = False
        if ws is not None:
            try:
                await ws.close()
            except Exception:
                pass

    # ---------- 数据面 ----------

    async def feed(self, pcm: bytes) -> None:
        """喂 16k mono s16le PCM;非阻塞(队列满则丢弃最旧,音频宁丢勿积压)。"""
        if not self.connected:
            return
        while True:
            try:
                self._queue.put_nowait(pcm)
                self.bytes_in += len(pcm)
                return
            except asyncio.QueueFull:
                try:
                    self._queue.get_nowait()
                except asyncio.QueueEmpty:
                    pass

    async def _tx_loop(self) -> None:
        try:
            while not self._closing:
                try:
                    item = await asyncio.wait_for(self._queue.get(), timeout=1.0)
                except asyncio.TimeoutError:
                    item = None
                now = time.time()
                gap = now - self._last_sent_at
                if item is None:
                    if gap >= KEEPALIVE_SEC:
                        await self._send_frame(b"\x00" * FRAME_BYTES)   # 静音保活
                    continue
                # 按真实节奏发送(约 40ms/帧),过快可能引擎出错
                if gap < FRAME_INTERVAL:
                    await asyncio.sleep(FRAME_INTERVAL - gap)
                await self._send_frame(item)
        except asyncio.CancelledError:
            raise
        except Exception as e:
            self.last_error = f"tx: {e}"
            self.connected = False

    async def _send_frame(self, frame: bytes) -> None:
        ws = self._ws
        if ws is None:
            return
        # 只整帧发(音频源回调本就按 40ms 切好)
        for i in range(0, len(frame), FRAME_BYTES):
            await ws.send(frame[i:i + FRAME_BYTES])
            self._last_sent_at = time.time()

    async def _rx_loop(self) -> None:
        try:
            while not self._closing:
                raw = await self._ws.recv()
                if isinstance(raw, bytes):
                    continue
                msg = json.loads(raw)
                action = msg.get("action") or msg.get("msg_type")
                if action == "error":
                    self.last_error = json.dumps(msg, ensure_ascii=False)[:200]
                    log.warning("asr error: %s", self.last_error)
                    break
                code = msg.get("code")
                if code is not None and str(code) not in ("0", ""):
                    self.last_error = f"code={code} {msg.get('desc', '')}"
                    log.warning("asr bad code: %s", self.last_error)
                    break
                seg = parse_segment(msg)
                if seg is None:
                    continue
                self.segments += 1
                cb = self._on_segment(seg)
                if asyncio.iscoroutine(cb):
                    await cb
        except asyncio.CancelledError:
            raise
        except Exception as e:
            if not self._closing:
                self.last_error = f"rx: {e}"
                log.info("asr rx closed: %s", e)
        finally:
            self.connected = False
