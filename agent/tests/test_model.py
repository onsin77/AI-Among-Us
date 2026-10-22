#!/usr/bin/env python3
"""DeepSeek 真机冒烟:3 个会议场景,校验 JSON schema / 投票目标合法性 / 延迟。

运行:cd agent && python tests/test_model.py
(需要 agent/config.ini 里的 api_key 或环境变量 DEEPSEEK_API_KEY)
"""

from __future__ import annotations

import asyncio
import json
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.config import Config  # noqa: E402
from ai_agent.meeting import SYSTEM_PROMPT  # noqa: E402
from ai_agent.model import DeepSeekClient  # noqa: E402

CASES = [
    {
        "label": "船员被指认(应自辩/反推)",
        "role": "Crewmate(Crew)",
        "roster": ["Red", "Blue", "Green"],
        "chat": ["Red: I saw Blue vent in Electrical", "Blue: that's a lie, I was in MedBay"],
        "vote_required": False,
    },
    {
        "label": "内鬼被追问(应伪装/甩锅)",
        "role": "Impostor(Impostor)",
        "roster": ["Red", "Blue", "Green"],
        "chat": ["Red: where were you Blue?", "Green: I was doing wires in Electrical"],
        "vote_required": True,
    },
    {
        "label": "无人说话(应简短或沉默)",
        "role": "Crewmate(Crew)",
        "roster": ["Red", "Blue"],
        "chat": [],
        "vote_required": False,
    },
]


async def main() -> int:
    cfg = Config.load()
    client = DeepSeekClient(cfg.deepseek_api_key, cfg.deepseek_base_url, cfg.fast_model, cfg.deep_model)
    if not client.available:
        print("FAIL: api_key 未配置")
        return 1
    ok = 0
    for c in CASES:
        user = (f"轮次:2。玩家名单:{', '.join(c['roster'])}。\n聊天记录:\n"
                + ("\n".join(c["chat"]) if c["chat"] else "(暂无发言)"))
        if c["vote_required"]:
            user += "\n本轮必须给出投票建议。"
        msgs = [{"role": "system", "content": SYSTEM_PROMPT.format(name="AI", role=c["role"])},
                {"role": "user", "content": user}]
        t0 = time.time()
        try:
            r = await client.chat(msgs, tier="fast", json_mode=True, temperature=0.7, max_tokens=220)
        except Exception as e:
            print(f"[FAIL] {c['label']}: {e}")
            continue
        obj = r.json_obj or {}
        say = obj.get("say")
        vote = obj.get("vote")
        valid = isinstance(say, str) and (vote is None or vote == "skip" or vote in c["roster"])
        print(f"[{'PASS' if valid else 'FAIL'}] {c['label']} {r.latency_ms}ms "
              f"say={say!r} vote={vote!r} usage={r.usage.get('total_tokens') if r.usage else '?'}")
        if not valid:
            print("   raw:", r.text[:300])
        ok += 1 if valid else 0
    print(f"\nmodel test: {ok}/{len(CASES)}")
    return 0 if ok == len(CASES) else 1


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
