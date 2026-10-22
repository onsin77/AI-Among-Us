"""XTTS 本地语音合成客户端(D:/ai/xtts,HTTP http://127.0.0.1:17895)。"""

from __future__ import annotations

import asyncio
import json
import logging
import re
import urllib.request
from dataclasses import dataclass

log = logging.getLogger("ai_agent.xtts")


@dataclass
class TtsConfig:
    base_url: str = "http://127.0.0.1:17895"
    voice: str = "xiaoyan"
    speed: int = 55
    volume: int = 60
    timeout: float = 20.0


_CLEAN_RE = re.compile(r"[*_~`#>|]+")
_EMOJI_RE = re.compile(
    "[\U0001f300-\U0001f9ff\U00002702-\U000027b0\U0000fe0f\U0001fa00-\U0001faff]+")


def clean_for_tts(text: str) -> str:
    """模型输出口语化清理:去 markdown/表情,省略号转停顿,限制长度。"""
    t = _CLEAN_RE.sub("", str(text or ""))
    t = _EMOJI_RE.sub("", t)
    t = t.replace("……", "。").replace("...", "。").replace("…", "。").replace("\n", "。")
    t = re.sub(r"[。,.、;；!！?？]{2,}", "。", t)
    t = re.sub(r"\s+", " ", t).strip(" ,，")
    return t[:400]


class XttsClient:
    def __init__(self, cfg: TtsConfig) -> None:
        self.cfg = cfg
        self.available = True

    async def synth(self, text: str, voice: str | None = None) -> bytes | None:
        """文本 → wav 字节(16k mono);失败返回 None(不抛)。"""
        t = clean_for_tts(text)
        if not t:
            return None
        body = json.dumps({
            "text": t,
            "voice": voice or self.cfg.voice,
            "speed": self.cfg.speed,
            "volume": self.cfg.volume,
        }).encode("utf-8")

        def _post() -> bytes:
            req = urllib.request.Request(self.cfg.base_url.rstrip("/") + "/tts",
                                         data=body, headers={"Content-Type": "application/json"})
            with urllib.request.urlopen(req, timeout=self.cfg.timeout) as resp:
                return resp.read()

        try:
            wav = await asyncio.to_thread(_post)
            self.available = True          # 成功后恢复,避免一次抖动永久标红
            return wav
        except Exception as e:
            self.available = False
            log.warning("tts failed: %s", e)
            return None

    async def ping(self) -> bool:
        def _get() -> bool:
            with urllib.request.urlopen(self.cfg.base_url.rstrip("/") + "/health", timeout=4) as resp:
                return resp.status == 200
        try:
            return await asyncio.to_thread(_get)
        except Exception:
            return False
