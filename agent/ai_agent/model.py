"""DeepSeek 模型客户端:异步封装(urllib + to_thread,零新增依赖),严格 JSON 输出。

用法:
    client = DeepSeekClient(api_key, base_url, fast_model, deep_model)
    r = await client.chat([{"role":"user","content":"..."}], tier="fast", json_mode=True)
    r["text"], r["json"], r["latency_ms"], r["usage"]
"""

from __future__ import annotations

import asyncio
import json
import logging
import re
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from typing import Any

log = logging.getLogger("ai_agent.model")


class ModelError(RuntimeError):
    pass


@dataclass
class ModelResult:
    text: str
    json_obj: dict[str, Any] | None
    latency_ms: int
    model: str
    usage: dict[str, Any] | None = None


def extract_json(text: str) -> dict[str, Any] | None:
    """从模型输出提取 JSON 对象:容忍 ```json 围栏、前后缀说明文字。"""
    if not text:
        return None
    s = text.strip()
    if s.startswith("```"):
        s = re.sub(r"^```[a-zA-Z]*\s*", "", s)
        s = re.sub(r"\s*```$", "", s).strip()
    try:
        obj = json.loads(s)
        return obj if isinstance(obj, dict) else None
    except json.JSONDecodeError:
        pass
    # 退化:抓第一个平衡的 {...}
    start = s.find("{")
    if start < 0:
        return None
    depth = 0
    in_str = False
    esc = False
    for i in range(start, len(s)):
        ch = s[i]
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
                esc = True
            elif ch == '"':
                in_str = False
            continue
        if ch == '"':
            in_str = True
        elif ch == "{":
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                try:
                    obj = json.loads(s[start:i + 1])
                    return obj if isinstance(obj, dict) else None
                except json.JSONDecodeError:
                    return None
    return None


class DeepSeekClient:
    def __init__(self, api_key: str, base_url: str = "https://api.deepseek.com",
                 fast_model: str = "deepseek-chat", deep_model: str = "deepseek-reasoner",
                 timeout: float = 25.0) -> None:
        self.api_key = api_key
        self.base_url = base_url.rstrip("/")
        self.fast_model = fast_model
        self.deep_model = deep_model
        self.timeout = timeout

    @property
    def available(self) -> bool:
        return bool(self.api_key)

    async def chat(self, messages: list[dict[str, str]], tier: str = "fast",
                   json_mode: bool = False, temperature: float = 0.6,
                   max_tokens: int = 220) -> ModelResult:
        if not self.api_key:
            raise ModelError("deepseek api_key 未配置(agent/config.ini 或 DEEPSEEK_API_KEY)")
        model = self.deep_model if tier == "deep" else self.fast_model
        payload: dict[str, Any] = {
            "model": model,
            "messages": messages,
            "max_tokens": max_tokens,
            "temperature": temperature,
            "stream": False,
        }
        if json_mode:
            payload["response_format"] = {"type": "json_object"}

        def _post() -> tuple[dict[str, Any], int]:
            req = urllib.request.Request(
                self.base_url + "/chat/completions",
                data=json.dumps(payload).encode("utf-8"),
                headers={
                    "Authorization": f"Bearer {self.api_key}",
                    "Content-Type": "application/json",
                },
                method="POST",
            )
            t0 = time.time()
            try:
                with urllib.request.urlopen(req, timeout=self.timeout) as resp:
                    body = json.load(resp)
            except urllib.error.HTTPError as e:
                detail = e.read().decode("utf-8", errors="replace")[:300]
                raise ModelError(f"HTTP {e.code}: {detail}") from e
            except (urllib.error.URLError, TimeoutError) as e:
                raise ModelError(f"network: {e}") from e
            return body, int((time.time() - t0) * 1000)

        body, latency = await asyncio.to_thread(_post)
        try:
            text = body["choices"][0]["message"]["content"] or ""
        except (KeyError, IndexError) as e:
            raise ModelError(f"bad response: {str(body)[:200]}") from e
        return ModelResult(
            text=text,
            json_obj=extract_json(text),
            latency_ms=latency,
            model=model,
            usage=body.get("usage"),
        )

    def chat_sync(self, messages: list[dict[str, str]], **kw: Any) -> ModelResult:
        """同步版本(测试/脚本用)。"""
        return asyncio.run(self.chat(messages, **kw))
