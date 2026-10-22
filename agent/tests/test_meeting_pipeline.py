#!/usr/bin/env python3
"""会议管线单测(无需游戏/桥/真模型):文件喂入 → 推理 → chat/vote 指令。

运行:cd agent && python tests/test_meeting_pipeline.py
"""

from __future__ import annotations

import asyncio
import json
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.meeting import MeetingConfig, MeetingPipeline  # noqa: E402
from ai_agent.model import ModelResult  # noqa: E402
from ai_agent.world import WorldModel  # noqa: E402


class FakeBridge:
    def __init__(self) -> None:
        self.sent: list[dict] = []

    async def send(self, obj: dict) -> bool:
        self.sent.append(obj)
        return True


class StubModel:
    """返回固定 JSON,记录收到的 prompt。"""
    available = True

    def __init__(self, reply: dict) -> None:
        self.reply = reply
        self.calls: list[list[dict]] = []

    async def chat(self, messages, tier="fast", json_mode=False, temperature=0.6, max_tokens=220):
        self.calls.append(messages)
        return ModelResult(text=json.dumps(self.reply, ensure_ascii=False),
                           json_obj=self.reply, latency_ms=123, model="stub")


def meeting_state(can_vote: bool = True) -> dict:
    return {
        "phase": "MEETING",
        "round": 2,
        "me": {"player_id": 0, "name": "AI", "alive": True, "role": "Crewmate", "team": "Crew"},
        "players": [
            {"id": 1, "name": "Red", "alive": True},
            {"id": 2, "name": "Blue", "alive": True},
            {"id": 3, "name": "Green", "alive": False},
        ],
        "meeting": {"can_vote": can_vote, "did_vote": False, "votes": []},
        "chat": [],
    }


def setup(reply: dict, can_vote: bool = True, interval: float = 0.0):
    tmp = tempfile.mkdtemp(prefix="au_meet_")
    chat = Path(tmp) / "chat.jsonl"
    out = Path(tmp) / "out.jsonl"
    world = WorldModel()
    world.state = meeting_state(can_vote)
    bridge = FakeBridge()
    model = StubModel(reply)
    pipe = MeetingPipeline(world, bridge, model,  # type: ignore[arg-type]
                           MeetingConfig(chat_file=chat, out_file=out,
                                         min_interval=interval, enabled=True))
    return pipe, bridge, model, world, chat, out


def test_chat_and_vote():
    pipe, bridge, model, world, chat, out = setup({"say": "Blue 很可疑,投他", "vote": "Blue"})
    chat.write_text(json.dumps({"speaker": "Red", "text": "I saw Blue vent"}, ensure_ascii=False) + "\n",
                    encoding="utf-8")
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    types = [c.get("type") for c in bridge.sent]
    assert "chat" in types, f"expected chat: {bridge.sent}"
    assert "vote" in types, f"expected vote: {bridge.sent}"
    vote = [c for c in bridge.sent if c.get("type") == "vote"][0]
    assert vote.get("target_id") == 2, f"Blue should be id 2: {vote}"
    assert "I saw Blue vent" in model.calls[0][1]["content"]
    assert out.exists() and "Blue 很可疑" in out.read_text(encoding="utf-8")
    print("[PASS] file chat -> model -> chat + vote(Blue=2), output logged")


def test_vote_deferred_when_not_allowed():
    pipe, bridge, _, world, chat, _ = setup({"say": "", "vote": "skip"}, can_vote=False)
    chat.write_text(json.dumps({"speaker": "Red", "text": "who?"}, ensure_ascii=False) + "\n",
                    encoding="utf-8")
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    votes = [c for c in bridge.sent if c.get("type") == "vote"]
    assert not votes, f"vote must be deferred when can_vote=false: {bridge.sent}"
    assert pipe._pending_vote is not None, "vote should be pending"
    # 会议进入可投票后补投
    world.state["meeting"] = {"can_vote": True, "did_vote": False, "votes": []}
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    votes = [c for c in bridge.sent if c.get("type") == "vote"]
    assert votes and votes[0].get("skip") is True, f"expected deferred skip vote: {bridge.sent}"
    print("[PASS] vote deferred until can_vote, then sent")


def test_vote_forced_by_file():
    pipe, bridge, _, _, chat, _ = setup({"say": "ok", "vote": None})
    chat.write_text(json.dumps({"type": "vote", "target": "Red"}, ensure_ascii=False) + "\n",
                    encoding="utf-8")
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    votes = [c for c in bridge.sent if c.get("type") == "vote"]
    # 模型给 null,但外部显式要求投票 → 只能投 skip(模型未点名)
    assert votes, f"expected a vote command: {bridge.sent}"
    assert votes[0].get("skip") is True
    print("[PASS] explicit vote request with null model vote -> skip")


def test_invalid_name_falls_back_to_skip():
    pipe, bridge, _, _, chat, _ = setup({"say": "投 Nobody", "vote": "Nobody"})
    chat.write_text(json.dumps({"speaker": "Red", "text": "hi"}, ensure_ascii=False) + "\n",
                    encoding="utf-8")
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    votes = [c for c in bridge.sent if c.get("type") == "vote"]
    assert votes and votes[0].get("skip") is True, f"unknown name must skip: {bridge.sent}"
    print("[PASS] unknown vote name falls back to skip")


def test_noise_lines_ignored():
    pipe, bridge, model, _, chat, _ = setup({"say": "hello", "vote": None})
    chat.write_text("not json\n\n{broken\n" + json.dumps({"speaker": "Red", "text": "real"},
                                                        ensure_ascii=False) + "\n", encoding="utf-8")
    asyncio.run(pipe._poll())  # type: ignore[attr-defined]
    assert model.calls, "valid line should still trigger model"
    print("[PASS] malformed lines ignored, valid line processed")


if __name__ == "__main__":
    test_chat_and_vote()
    test_vote_deferred_when_not_allowed()
    test_vote_forced_by_file()
    test_invalid_name_falls_back_to_skip()
    test_noise_lines_ignored()
    print("\nall meeting pipeline tests passed")
