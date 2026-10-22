# -*- coding: utf-8 -*-
"""语音会议全链路 E2E(真音频,不需要游戏):
CABLE Input 播放两段不同音色(模拟 YY 里 Blue/Red 说话)→ CABLE Output 采集 → 讯飞转写(音色分离)
→ 轮麦状态机 → 我方发言(桩模型)→ XTTS 合成回输 CABLE → 表决(桩桥记录)。

前置:XTTS 服务在跑(127.0.0.1:17895);VB-CABLE 正常;config.ini 有讯飞凭据。
运行:python tests/test_voice_e2e.py
"""
import asyncio
import json
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.config import Config                    # noqa: E402
from ai_agent.memory import GameMemory                # noqa: E402
from ai_agent.model import ModelResult                # noqa: E402
from ai_agent.voice.pipeline import VoiceConfig, VoiceMeetingPipeline  # noqa: E402
from ai_agent.voice.xtts import XttsClient            # noqa: E402
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


class StubModel:
    def __init__(self):
        self.calls = []
        self.available = True

    async def chat(self, messages, tier="fast", json_mode=False, temperature=0.6, max_tokens=220):
        self.calls.append([m["content"] for m in messages])
        user = messages[1]["content"]
        if "表决" in user:
            obj = {"vote": "Red", "reason": "Red 的说法和 Blue 矛盾。", "sus": {"Red": 85}}
        elif "行动目标" in user:
            obj = {"goals": [{"action": "task", "weight": 80, "reason": "先做任务"},
                             {"action": "guard", "target": "Kitchen", "weight": 40,
                              "reason": "守住案发房间"}],
                   "summary": "Blue 与 Red 说法矛盾,先做任务并留意厨房"}
        else:
            obj = {"say": "我刚才一直在主舱做电线任务,做完往厨房方向走,没看到可疑的人。",
                   "sus": {"Red": 30}, "vote": None}
        return ModelResult(text=json.dumps(obj), json_obj=obj, latency_ms=120, model="stub")


class StubBridge:
    def __init__(self):
        self.sent = []

    async def send(self, obj):
        self.sent.append(obj)
        return True


async def synth(tts, text, voice):
    wav = await tts.synth(text, voice=voice)
    assert wav, f"合成失败:{text[:20]}"
    return wav


async def main():
    cfg_ini = Config.load(Path(__file__).resolve().parent.parent / "config.ini")
    check("讯飞凭据已配置", bool(cfg_ini.asr_app_id and cfg_ini.asr_api_secret))

    world = WorldModel()
    world.state = {
        "type": "state_full", "phase": "MEETING", "round": 1, "map": "Airship",
        "me": {"player_id": 5, "name": "唯心AI", "alive": True, "role": "Crewmate",
               "team": "Crew", "room": "MainHall", "tasks": {"total": 4, "done": 1, "list": []}},
        "players": [
            {"id": 1, "name": "Red", "alive": True},
            {"id": 2, "name": "Blue", "alive": True},
            {"id": 5, "name": "唯心AI", "alive": True},
        ],
        "meeting": {"reporter": 2, "state": "NotVoted", "can_vote": True,
                    "speaking_order": [2, 5, 1]},
        "chat": [],
    }
    cfg = VoiceConfig(
        enabled=True, turn_end_gap=2.5, turn_timeout=25,
        asr_app_id=cfg_ini.asr_app_id, asr_api_key=cfg_ini.asr_api_key,
        asr_api_secret=cfg_ini.asr_api_secret,
        tts_base_url=cfg_ini.tts_base_url, tts_voice="xiaoyan",
    )
    p = VoiceMeetingPipeline(world, StubBridge(), StubModel(), GameMemory(
        Path(__file__).parent / "data" / "memory_test"), cfg)
    run_task = asyncio.create_task(p.run())       # 真实主循环(采集→ASR→状态机)

    print("== 建会(采集+讯飞)==")
    await p._setup_meeting()
    await asyncio.sleep(1.0)
    check("采集运行", p.capture.running, str(p.capture.last_error))
    check("讯飞已连", p.asr is not None and p.asr.connected, str(p.asr and p.asr.last_error))
    p._adopt_speaking_order()
    check("发言顺序", p.speaking_order == [2, 5, 1], str(p.speaking_order))

    # ---- Blue(xiaofeng 音色)作为报警人先说 ----
    # 注意:用独立播放器模拟"YY 播放别人的声音进 CABLE Input"。
    # 不能用 p.player:那是我方 TTS 专用通道,播放期间管线会静音 ASR(自保护)。
    from ai_agent.voice.audioio import CablePlayer
    yy = CablePlayer(cfg.play_device)
    tts = XttsClient(None) if False else p.tts
    print("== Blue 发言(xiaofeng 播进 CABLE Input)==")
    wav_blue = await synth(tts, "我在厨房发现了尸体,死的是 Green,我马上就报了警。", "xiaofeng")
    yy.play_wav_bytes(wav_blue)
    t0 = time.time()
    while time.time() - t0 < 25:
        if p.transcript and p.transcript[-1]["speaker"] == "Blue":
            break
        await asyncio.sleep(0.3)
    check("Blue 转写并绑定", bool(p.transcript) and p.transcript[-1]["speaker"] == "Blue",
          json.dumps(list(p.transcript), ensure_ascii=False))
    check("Blue 原文含关键词", any("尸体" in u["text"] or "厨房" in u["text"] for u in p.transcript))
    check("轮次推进到我", p._expected_pid() == 5, f"idx={p.turn_idx}")

    # ---- 我的发言(桩模型→真 XTTS→CABLE 回输)----
    print("== 我方发言(真 XTTS)==")
    t0 = time.time()
    while time.time() - t0 < 30 and p.my_state != "done":
        await asyncio.sleep(0.3)
    check("我的发言完成", p.my_state == "done", p.my_state)
    check("我方发言有内容", bool(p.my_last and p.my_last.get("say")), str(p.my_last))
    check("轮次推进到 Red", p._expected_pid() == 1, f"idx={p.turn_idx}")

    # ---- Red(xiaoyan 音色)发言 → 全部结束 → 自动表决 ----
    print("== Red 发言(xiaoyan 播进 CABLE Input)==")
    wav_red = await synth(tts, "我不认同,我整局都在货舱搬箱子,Blue 说的灯灭了我不清楚。", "xiaoyan")
    yy.play_wav_bytes(wav_red)
    t0 = time.time()
    while time.time() - t0 < 25:
        if any(u["speaker"] == "Red" for u in p.transcript):
            break
        await asyncio.sleep(0.3)
    check("Red 转写并绑定", any(u["speaker"] == "Red" for u in p.transcript),
          json.dumps(list(p.transcript), ensure_ascii=False))

    print("== 等待自动表决 ==")
    t0 = time.time()
    while time.time() - t0 < 20:
        if any(s.get("type") == "vote" for s in p.bridge.sent):
            break
        await asyncio.sleep(0.3)
    votes = [s for s in p.bridge.sent if s.get("type") == "vote"]
    check("表决指令已发(投 Red)", len(votes) == 1 and votes[0].get("target_id") == 1,
          str(p.bridge.sent))
    check("怀疑度已更新", p.suspicion.get("Red", 0) >= 60, str(p.suspicion))

    # ---- 散会 → 目标思考 ----
    print("== 散会 ==")
    world.state["phase"] = "PLAYING"
    world.state["meeting"] = None
    await asyncio.sleep(1.5)
    check("采集已停", not p.capture.running)
    check("讯飞已断开收尾", p.asr is None)
    t0 = time.time()
    while time.time() - t0 < 10 and not p.goals:
        await asyncio.sleep(0.3)
    check("会议目标已生成", bool(p.goals), str(p.goals))

    await p.stop()
    run_task.cancel()
    print(f"\nvoice E2E: {PASS} pass, {FAIL} fail")
    print("转写记录:")
    for u in p.transcript:
        print(f"  [{u['speaker']}] {u['text']}")
    return 1 if FAIL else 0


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
