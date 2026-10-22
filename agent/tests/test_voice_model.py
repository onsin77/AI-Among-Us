# -*- coding: utf-8 -*-
"""真模型冒烟:DeepSeek 对语音会议三类提示词(发言/表决/目标)的 JSON 契约与延迟。
运行:python tests/test_voice_model.py
"""
import asyncio
import json
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.config import Config                    # noqa: E402
from ai_agent.memory import GameMemory                # noqa: E402
from ai_agent.model import DeepSeekClient, ModelError  # noqa: E402
from ai_agent.voice.pipeline import VoiceMeetingPipeline  # noqa: E402
from ai_agent.voice import prompts                    # noqa: E402
from ai_agent.world import WorldModel                 # noqa: E402

PASS = FAIL = 0


def check(label, cond, detail=""):
    global PASS, FAIL
    if cond:
        PASS += 1
        print(f"[PASS] {label}")
    else:
        FAIL += 1
        print(f"[FAIL] {label} {detail}")


def build():
    cfg = Config.load()
    model = DeepSeekClient(cfg.deepseek_api_key, cfg.deepseek_base_url,
                           cfg.fast_model, cfg.deep_model)
    w = WorldModel()
    w.state = {
        "type": "state_full", "phase": "MEETING", "round": 2, "map": "Airship",
        "me": {"player_id": 5, "name": "唯心AI", "alive": True, "role": "Crewmate",
               "team": "Crew", "room": "MainHall",
               "tasks": {"total": 5, "done": 2, "list": [{"id": 1, "name": "FixWiring", "done": True}]}},
        "players": [
            {"id": 1, "name": "Red", "alive": True},
            {"id": 2, "name": "Blue", "alive": True},
            {"id": 3, "name": "Green", "alive": True},
            {"id": 5, "name": "唯心AI", "alive": True},
        ],
        "meeting": {"speaking_order": [2, 5, 1, 3]},
    }
    w.apply(dict(w.state))
    w.journey.clear()
    for r in ["MainHall", "Kitchen", "Electrical", "MainHall"]:
        w.journey.append((time.time(), r))
    p = VoiceMeetingPipeline(w, type("B", (), {"send": staticmethod(lambda obj: True)})(),
                             model, GameMemory(Path(__file__).parent / "data" / "memory_test"),
                             __import__("ai_agent.voice.pipeline", fromlist=["VoiceConfig"]).VoiceConfig())
    p.transcript.append({"speaker": "Blue", "text": "我在厨房发现了尸体,是 Green,我马上报了警。", "at": 0, "rl": 1})
    p.transcript.append({"speaker": "Blue", "text": "我这一局一直在厨房货舱这一片做任务。", "at": 1, "rl": 1})
    return p, model


async def main():
    p, model = build()
    if not model.available:
        print("[SKIP] 无 api key")
        return 0

    # 1) 报警人位次(第一个发言)
    msgs = p._build_speech_messages(0, 4)
    t0 = time.time()
    r = await model.chat(msgs, tier=prompts.tier_for_position(0, 4), json_mode=True,
                         temperature=0.7, max_tokens=prompts.max_tokens_for_position(0, 4))
    obj = r.json_obj or {}
    say = str(obj.get("say") or "")
    check("报警人发言:say 非空且短", 0 < len(say) <= 200, say)
    check("报警人发言:延迟<3s(极低思考档)", r.latency_ms < 3000, f"{r.latency_ms}ms")
    check("报警人发言:sus 是 0-100 映射", isinstance(obj.get("sus"), dict) and
          all(isinstance(v, (int, float)) for v in (obj.get("sus") or {}).values()), str(obj.get("sus")))
    print(f"  报警人 say = {say}")
    print(f"  报警人 sus = {obj.get('sus')}")

    # 2) 末位发言
    msgs = p._build_speech_messages(3, 4)
    r = await model.chat(msgs, tier="fast", json_mode=True, temperature=0.7, max_tokens=320)
    obj = r.json_obj or {}
    check("末位发言:say 非空", bool(str(obj.get("say") or "").strip()), str(obj.get("say")))
    print(f"  末位 say = {obj.get('say')}")

    # 3) 表决
    msgs = p._build_vote_messages()
    r = await model.chat(msgs, tier="fast", json_mode=True, temperature=0.4, max_tokens=260)
    obj = r.json_obj or {}
    vote = obj.get("vote")
    check("表决:vote ∈ {skip,玩家名}", vote in ("skip", None, "Red", "Blue", "Green"), str(vote))
    check("表决:reason 口语", isinstance(obj.get("reason"), str) and len(obj["reason"]) > 3)
    print(f"  vote={vote} reason={obj.get('reason')}")

    # 4) 会议目标
    msgs = p._build_goals_messages()
    r = await model.chat(msgs, tier="fast", json_mode=True, temperature=0.5, max_tokens=400)
    obj = r.json_obj or {}
    goals = obj.get("goals")
    check("目标:goals 列表", isinstance(goals, list) and 1 <= len(goals) <= 4, str(goals))
    if isinstance(goals, list) and goals:
        g = goals[0]
        check("目标:字段齐全", isinstance(g, dict) and g.get("action") and "weight" in g, str(g))
    print(f"  goals = {goals}")

    # 5) TTS 清理函数
    from ai_agent.voice.xtts import clean_for_tts
    check("TTS 清理", clean_for_tts("**我**在电力舱…修灯💡。") == "我在电力舱。修灯。")
    print(f"\nvoice model smoke: {PASS} pass, {FAIL} fail")
    return 1 if FAIL else 0


if __name__ == "__main__":
    try:
        code = asyncio.run(main())
    except ModelError as e:
        print(f"[ERROR] {e}")
        code = 1
    raise SystemExit(code)
