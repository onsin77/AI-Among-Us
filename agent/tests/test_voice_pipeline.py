# -*- coding: utf-8 -*-
"""语音会议管线离线单测:轮麦状态机/音色绑定/位次提示词/表决/怀疑度合并/手动控制。

不依赖音频设备、讯飞、DeepSeek:tts/player/model/bridge 全部打桩,直接喂转写 Segment。
运行:python tests/test_voice_pipeline.py
"""
import asyncio
import json
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.bridge import BridgeClient           # noqa: E402
from ai_agent.memory import GameMemory             # noqa: E402
from ai_agent.model import ModelResult             # noqa: E402
from ai_agent.voice.pipeline import VoiceConfig, VoiceMeetingPipeline  # noqa: E402
from ai_agent.voice.xfasr import Segment           # noqa: E402
from ai_agent.world import WorldModel              # noqa: E402
from ai_agent.voice import prompts                 # noqa: E402

PASS = FAIL = 0


def check(label, cond, detail=""):
    global PASS, FAIL
    if cond:
        PASS += 1
        print(f"[PASS] {label}")
    else:
        FAIL += 1
        print(f"[FAIL] {label} {detail}")


class StubModel:
    """按场景返回固定 JSON;记录每次请求的 messages。"""
    def __init__(self):
        self.calls = []
        self.next = {"say": "我在电力舱修灯。", "sus": {"Red": 80}, "vote": None}
        self.available = True

    async def chat(self, messages, tier="fast", json_mode=False, temperature=0.6, max_tokens=220):
        self.calls.append({"messages": messages, "tier": tier, "max_tokens": max_tokens})
        return ModelResult(text=json.dumps(self.next), json_obj=dict(self.next),
                           latency_ms=100, model="stub", usage=None)


class StubTts:
    def __init__(self):
        self.synthed = []
        self.cfg = type("C", (), {"voice": "xiaoyan", "base_url": "stub"})()

    async def synth(self, text, voice=None):
        self.synthed.append(text)
        return b"RIFFxxxxWAVEfmt fake"


class StubPlayer:
    def __init__(self):
        self.played = []
        self.speaking = False
        self.last_error = None

    def play_wav_bytes(self, wav):
        self.played.append(wav)
        return True

    def stop(self):
        self.speaking = False


class StubBridge:
    def __init__(self):
        self.sent = []

    async def send(self, obj):
        self.sent.append(obj)
        return True


def fake_world():
    w = WorldModel()
    w.state = {
        "type": "state_full", "phase": "PLAYING", "round": 2, "map": "Airship",
        "me": {"player_id": 5, "name": "唯心AI", "alive": True, "role": "Crewmate",
               "team": "Crew", "room": "MainHall",
               "tasks": {"total": 5, "done": 2, "list": [
                   {"id": 1, "name": "FixWiring", "done": True},
                   {"id": 2, "name": "SwipeCard", "done": True},
                   {"id": 3, "name": "EmptyGarbage", "done": False}]}},
        "players": [
            {"id": 1, "name": "Red", "alive": True},
            {"id": 2, "name": "Blue", "alive": True},
            {"id": 3, "name": "Green", "alive": False},
            {"id": 5, "name": "唯心AI", "alive": True},
        ],
        "meeting": None,
        "chat": [],
    }
    w.apply(dict(w.state))
    return w


def make_pipeline(world=None, **cfg_over):
    world = world or fake_world()
    cfg = VoiceConfig(utter_gap=0.15, turn_end_gap=0.5, turn_timeout=1.2, enabled=True)
    for k, v in cfg_over.items():
        setattr(cfg, k, v)
    mem = GameMemory(Path(__file__).parent / "data" / "memory_test")
    p = VoiceMeetingPipeline(world, StubBridge(), StubModel(), mem, cfg)
    p.tts = StubTts()
    p.player = StubPlayer()
    return p, world


async def seg(pipe, rl, text, final=True):
    await pipe._on_segment(Segment(seg_id=0, final=final, text=text, rl=rl))


async def test_binding_and_turns():
    p, w = make_pipeline()
    bridge = p.bridge
    w.state["phase"] = "MEETING"
    w.state["meeting"] = {"speaking_order": [2, 5, 1], "can_vote": True, "reporter": 2}
    await p._setup_meeting()
    p._adopt_speaking_order()
    check("进会后发言顺序", p.speaking_order == [2, 5, 1] and p.turn_idx == 0)

    # Blue(id2) 作为报警人第一个说话:rl=1 → 绑定 Blue
    await seg(p, 1, "我在厨房发现的尸体。")
    await asyncio.sleep(0.05)
    p._close_stale_utter(force=True)
    check("rl1 绑定 Blue", p.rl2pid.get(1) == 2, str(p.rl2pid))
    check("Blue 发言入档", p.transcript and p.transcript[-1]["speaker"] == "Blue"
          and "尸体" in p.transcript[-1]["text"])
    check("轮次推进到 me", p.turn_idx == 1 and p._expected_pid() == 5)

    # 我的发言被触发(model 打桩),等待完成
    for _ in range(50):
        if p.my_state == "done":
            break
        await asyncio.sleep(0.1)
    check("我的发言完成", p.my_state == "done", p.my_state)
    check("我已合成语音", len(p.tts.synthed) == 1 and "电力舱" in p.tts.synthed[0])
    check("轮次推进到 Red", p._expected_pid() == 1)

    # Red(id1) 说话:rl=2 → 绑定
    await seg(p, 2, "我跟 Blue 一起在货舱。")
    p._close_stale_utter(force=True)
    check("rl2 绑定 Red", p.rl2pid.get(2) == 1, str(p.rl2pid))
    # 全部说完 → 自动表决
    for _ in range(60):
        if p.voted:
            break
        await asyncio.sleep(0.1)
    check("全部发言后表决", p.voted)
    votes = [s for s in bridge.sent if s.get("type") == "vote"]
    check("表决指令已发", len(votes) == 1, str(bridge.sent))

    # 怀疑度已合并(Red:80)
    check("怀疑度合并", p.suspicion.get("Red") == 80, str(p.suspicion))

    # 会议结束 → goals 思考被触发
    w.state["phase"] = "PLAYING"
    w.state["meeting"] = None
    await p._teardown_meeting(write_summary=True)
    for _ in range(30):
        if p.model.calls and any("行动目标" in c["messages"][1]["content"] for c in p.model.calls):
            break
        await asyncio.sleep(0.1)
    check("会议结束触发目标思考", any("行动目标" in c["messages"][1]["content"] for c in p.model.calls))


async def test_interjection_no_advance():
    p, w = make_pipeline()
    w.state["phase"] = "MEETING"
    w.state["meeting"] = {"speaking_order": [2, 5, 1], "can_vote": True, "reporter": 2}
    await p._setup_meeting()
    p._adopt_speaking_order()
    await seg(p, 1, "我是 Blue,报警的。")
    p._close_stale_utter(force=True)
    check("轮次到 me", p.turn_idx == 1)
    for _ in range(50):
        if p.my_state == "done":
            break
        await asyncio.sleep(0.1)
    check("我发言完轮到 Red", p._expected_pid() == 1)
    # 已绑定的 rl=1(Blue)在 Red 的回合插话:不推进
    before = p.turn_idx
    await seg(p, 1, "我补充一句,当时灯是灭的。")
    p._close_stale_utter(force=True)
    check("插话不推进轮次", p.turn_idx == before, f"{before}→{p.turn_idx}")
    check("插话记录在案", p.transcript[-1]["speaker"] == "Blue" and "灯是灭的" in p.transcript[-1]["text"])


async def test_turn_timeout_skips():
    p, w = make_pipeline(turn_timeout=0.4)
    w.state["phase"] = "MEETING"
    w.state["meeting"] = {"speaking_order": [2, 5, 1], "can_vote": True, "reporter": 2}
    await p._setup_meeting()
    p._adopt_speaking_order()
    # Blue 不说话:超时跳过 → 轮到我
    await asyncio.sleep(0.6)
    p._check_turn_timeout()
    check("超时跳过 Blue 轮到我", p._expected_pid() == 5 and 2 in p.spoken,
          f"idx={p.turn_idx} spoken={p.spoken}")
    for _ in range(50):
        if p.my_state == "done":
            break
        await asyncio.sleep(0.1)
    check("我发言后进入 Red", p._expected_pid() == 1)


async def test_position_prompts():
    p0, _ = make_pipeline()
    msgs = p0._build_speech_messages(0, 6)
    check("报警人位次=极低", "报警人" in msgs[1]["content"] and "立刻" in msgs[1]["content"])
    check("报警人 max_tokens 小", prompts.max_tokens_for_position(0, 6) == 130)
    msgs_late = p0._build_speech_messages(5, 6)
    check("末位=总结指认", "最怀疑" in msgs_late[1]["content"])
    check("系统提示含音色口语要求", "开麦" in msgs[0]["content"] and "赌怪" in msgs[0]["content"])
    # 内鬼提示
    w = fake_world()
    w.state["me"]["team"] = "Impostor"
    w.state["me"]["role"] = "Impostor"
    p1, _ = make_pipeline(world=w)
    check("内鬼角色提示", "内鬼" in p1._role_hint() and "编造" in p1._role_hint())


async def test_manual_control():
    p, w = make_pipeline()
    w.state["phase"] = "MEETING"
    w.state["meeting"] = {"speaking_order": [2, 5, 1], "can_vote": True, "reporter": 2}
    await p._setup_meeting()
    p._adopt_speaking_order()
    r = await p.control({"action": "next"})
    check("手动下一位", r["ok"] and p._expected_pid() == 5)
    r = await p.control({"action": "rebind", "rl": 3, "player": "Red"})
    check("手动改绑", r["ok"] and p.rl2pid.get(3) == 1)
    r = await p.control({"action": "reset"})
    check("重置绑定", r["ok"] and not p.rl2pid)
    r = await p.control({"action": "set_order", "players": ["Red", "唯心AI", "Blue"]})
    check("手动设顺序", r["ok"] and p.speaking_order == [1, 5, 2], str(p.speaking_order))
    r = await p.control({"action": "set_order", "players": ["Red", "不存在"]})
    check("未知玩家报错", not r["ok"])
    r = await p.control({"action": "disable"})
    check("手动停用", r["ok"] and not p.active)
    r = await p.control({"action": "enable"})
    check("手动启用", r["ok"] and p.active)


async def test_suspicion_clamp():
    p, _ = make_pipeline()
    p.suspicion = {"Red": 50}
    p._merge_suspicion({"Red": 500, "Ghost": 30})   # Ghost 不在名单 → 忽略;500 截断
    check("怀疑度截断+名单过滤", p.suspicion.get("Red") == 80 and "Ghost" not in p.suspicion,
          str(p.suspicion))


async def test_fallback_speech():
    p, w = make_pipeline()
    w.state["phase"] = "MEETING"
    w.state["meeting"] = {"speaking_order": [5, 1], "can_vote": True, "reporter": 5}
    p.model.next = {"say": "", "sus": {}, "vote": None}   # 模型给空 → 兜底发言
    await p._setup_meeting()
    p._adopt_speaking_order()
    for _ in range(50):
        if p.my_state == "done":
            break
        await asyncio.sleep(0.1)
    check("空输出兜底发言", len(p.tts.synthed) == 1 and p.tts.synthed[0], p.tts.synthed)


async def main():
    await test_binding_and_turns()
    await test_interjection_no_advance()
    await test_turn_timeout_skips()
    await test_position_prompts()
    await test_manual_control()
    await test_suspicion_clamp()
    await test_fallback_speech()
    print(f"\nvoice pipeline tests: {PASS} pass, {FAIL} fail")
    return 1 if FAIL else 0


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
