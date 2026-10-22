#!/usr/bin/env python3
"""行为引擎单测(无需游戏/桥):
1) 坏人旁边有尸体也绝不 report(用户硬规则:刀完人不报警)
2) 坏人冷却就绪且有可见目标 → kill
3) 好人附近尸体 → report
4) 好人无尸体 → open_task
运行:cd agent && python tests/test_behavior.py
"""

from __future__ import annotations

import asyncio
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from ai_agent.behavior import BehaviorConfig, BehaviorEngine  # noqa: E402
from ai_agent.world import WorldModel  # noqa: E402


class FakeBridge:
    def __init__(self) -> None:
        self.sent: list[dict] = []

    async def send(self, obj: dict) -> bool:
        self.sent.append(obj)
        self.connected = True
        return True

    async def send_command(self, obj: dict) -> bool:
        self.sent.append(obj)
        return True


def make(mode: str) -> tuple[BehaviorEngine, FakeBridge, WorldModel]:
    world = WorldModel()
    bridge = FakeBridge()
    eng = BehaviorEngine(world, bridge, BehaviorConfig(mode=mode, patrol=False))  # type: ignore[arg-type]
    return eng, bridge, world


def base_state(role_team: str = "Crew", role: str = "Crewmate") -> dict:
    return {
        "phase": "PLAYING",
        "intro": False,
        "me": {"player_id": 0, "name": "AI", "alive": True, "team": role_team,
               "role": role, "kill_cooldown": 0.0, "tasks": {"list": []}},
        "players": [],
        "nearby_bodies": [],
    }


def test_impostor_never_reports():
    eng, br, world = make("impostor")
    st = base_state("Impostor", "Impostor")
    st["nearby_bodies"] = [{"victim": "Red", "distance": 1.5, "reported": False,
                            "position": {"x": 1.0, "y": 2.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert not any(c.get("type") == "report" for c in br.sent), f"impostor must never report: {br.sent}"
    print("[PASS] impostor never reports (body within 1.5u)")


def test_impostor_kills_in_range():
    eng, br, world = make("impostor")
    st = base_state("Impostor", "Impostor")
    st["players"] = [{"id": 3, "name": "Blue", "alive": True, "visible_to_me": True,
                      "distance": 3.0, "position": {"x": 5.0, "y": 5.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    kills = [c for c in br.sent if c.get("type") == "kill"]
    assert kills and kills[0]["target_id"] == 3, f"expected kill 3, got {br.sent}"
    assert not any(c.get("type") == "report" for c in br.sent)
    print("[PASS] impostor kills nearest visible target")


def test_impostor_approaches_when_far():
    eng, br, world = make("impostor")
    st = base_state("Impostor", "Impostor")
    st["players"] = [{"id": 3, "name": "Blue", "alive": True, "visible_to_me": True,
                      "distance": 4.6, "position": {"x": 9.0, "y": 1.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    moves = [c for c in br.sent if c.get("type") == "move_to"]
    assert moves and moves[0]["pos"] == [9.0, 1.0], f"expected approach move, got {br.sent}"
    assert not any(c.get("type") == "report" for c in br.sent)
    print("[PASS] impostor approaches far target without reporting")


def test_crew_reports_body():
    eng, br, world = make("crew")
    st = base_state()
    st["nearby_bodies"] = [{"victim": "Red", "distance": 2.0, "reported": False,
                            "position": {"x": 1.0, "y": 2.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert any(c.get("type") == "report" for c in br.sent), f"crew should report: {br.sent}"
    print("[PASS] crew reports nearby body")


def test_crew_runs_task():
    eng, br, world = make("crew")
    st = base_state()
    st["me"]["tasks"] = {"list": [
        {"id": 7, "name": "FixWiring", "done": False, "step": 0, "max_step": 3},
        {"id": 9, "name": "SortRecords", "done": True, "step": 1, "max_step": 1},
    ]}
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    tasks = [c for c in br.sent if c.get("type") == "open_task"]
    assert tasks and tasks[0]["task_id"] == 7, f"expected open_task 7, got {br.sent}"
    print("[PASS] crew opens first incomplete task")


def test_crew_blacklists_failed_task():
    eng, br, world = make("crew")
    st = base_state()
    st["me"]["tasks"] = {"list": [
        {"id": 7, "name": "EnterIdCode", "done": False, "step": 0, "max_step": 1},
        {"id": 8, "name": "SortRecords", "done": False, "step": 0, "max_step": 3},
    ]}
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert any(c.get("type") == "open_task" and c.get("task_id") == 7 for c in br.sent)
    # 模拟 L1 的 ack:task_no_console 失败
    ref = [c for c in br.sent if c.get("type") == "open_task"][0]["ref"]
    engine_pending = eng._pending.get(ref)
    assert engine_pending is not None
    world.acks.appendleft({"type": "ack", "ref": ref, "cmd": "open_task", "ok": False,
                           "err": "task_no_console", "_seq": 1})
    br.sent.clear()
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    tasks = [c for c in br.sent if c.get("type") == "open_task"]
    assert tasks and tasks[0]["task_id"] == 8, f"expected fallback to task 8, got {br.sent}"
    print("[PASS] crew blacklists unreachable task and moves on")


def test_meeting_idle():
    eng, br, world = make("impostor")
    st = base_state("Impostor", "Impostor")
    st["phase"] = "MEETING"
    st["players"] = [{"id": 3, "name": "Blue", "alive": True, "visible_to_me": True,
                      "distance": 1.0, "position": {"x": 1.0, "y": 1.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert not [c for c in br.sent if c.get("type") in ("kill", "move_to", "report")], br.sent
    print("[PASS] behavior idle during MEETING")


def test_auto_abilities():
    # 白名单技能 ready 即用;不在白名单的(如开面板类)绝不自动用
    eng, br, world = make("crew")
    st = base_state()
    st["me"]["abilities"] = [
        {"key": "medicShieldButton", "ready": True, "cooldown": 0.0, "uses": -1},
        {"key": "hackerButton", "ready": True, "cooldown": 0.0, "uses": -1},
    ]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    used = [c for c in br.sent if c.get("type") == "use_ability"]
    assert [c["key"] for c in used] == ["medicShieldButton"], \
        f"expected only whitelisted ability, got {used}"
    assert eng._abilities_used == 1
    # 节流:同 key 未到间隔不重发
    br.sent.clear()
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert not [c for c in br.sent if c.get("type") == "use_ability"], "ability must be throttled"
    # 关闭开关后不再自动用
    eng.cfg.auto_abilities = False
    eng._ability_tried_at.clear()
    br.sent.clear()
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert not [c for c in br.sent if c.get("type") == "use_ability"]
    print("[PASS] auto abilities whitelist + throttle + switch")


def test_kill_range_respects_setting():
    # L1 按房间 KillDistance 校验(2.5 档 + 余量):3.0 距离不该开火,而是贴近
    eng, br, world = make("impostor")
    st = base_state("Impostor", "Impostor")
    st["me"]["kill_distance"] = 2.5
    st["players"] = [{"id": 3, "name": "Blue", "alive": True, "visible_to_me": True,
                      "distance": 3.0, "position": {"x": 5.0, "y": 5.0}}]
    world.state = st
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert not [c for c in br.sent if c.get("type") == "kill"], \
        f"must not fire beyond kill_distance, got {br.sent}"
    assert any(c.get("type") == "move_to" for c in br.sent), "should approach instead"
    # 贴到 2.4 后允许开火
    br.sent.clear()
    st["players"][0]["distance"] = 2.4
    asyncio.run(eng._tick())  # type: ignore[attr-defined]
    assert any(c.get("type") == "kill" for c in br.sent), f"should kill in range, got {br.sent}"
    print("[PASS] kill range respects room KillDistance setting")


if __name__ == "__main__":
    test_impostor_never_reports()
    test_impostor_kills_in_range()
    test_impostor_approaches_when_far()
    test_crew_reports_body()
    test_crew_runs_task()
    test_crew_blacklists_failed_task()
    test_meeting_idle()
    test_auto_abilities()
    test_kill_range_respects_setting()
    print("\nall behavior tests passed")
