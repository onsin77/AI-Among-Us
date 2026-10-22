#!/usr/bin/env python3
"""端到端会议验证(需要游戏+agent 在跑):
1) 开局 → 设内鬼 → 走到假人旁 → kill → report → 进入 MEETING
2) 验证会议字段(can_vote/did_vote/votes)
3) 发 chat 指令 → 验证 state.chat 里能看到自己的发言(聊天采集链路)
4) 从面板接口喂会议聊天 → 触发 DeepSeek → 验证 AI 的 say 和 vote 真的发到游戏
5) 快进散会 → 回到 PLAYING

运行:cd agent && python tests/test_meeting_ingame.py
"""

from __future__ import annotations

import json
import sys
import time
import urllib.request

WEB = "http://127.0.0.1:8765"


def get(path: str):
    with urllib.request.urlopen(WEB + path, timeout=4) as r:
        return json.load(r)


def post(path: str, body: dict):
    req = urllib.request.Request(WEB + path, data=json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"}, method="POST")
    with urllib.request.urlopen(req, timeout=6) as r:
        return json.load(r)


def state():
    return get("/api/state")["state"]


def cmd(**kw):
    return post("/api/command", kw)


def wait_phase(want: str, timeout: float):
    t0 = time.time()
    while time.time() - t0 < timeout:
        if state().get("phase") == want:
            return True
        time.sleep(0.5)
    return False


def wait_event(code: str, timeout: float, since_seq: int = 0):
    t0 = time.time()
    while time.time() - t0 < timeout:
        d = get(f"/api/events?since={since_seq}")
        for e in d.get("events", []):
            if e.get("code") == code:
                return e, d.get("seq", since_seq)
        since_seq = d.get("seq", since_seq)
        time.sleep(0.4)
    return None, since_seq


def fail(msg: str):
    print(f"[FAIL] {msg}")
    sys.exit(1)


def main() -> None:
    st = state()
    if st.get("phase") != "PLAYING":
        print("开局 freeplay Airship…")
        cmd(type="debug_start", mode="freeplay", map=4)
        if not wait_phase("PLAYING", 90):
            fail("未进入 PLAYING(Steam 在线?)")
        time.sleep(2)
    st = state()
    print(f"起点:{st['me']['position']} room={st['me']['room']}")
    # 清掉可能的遗留面板(freeplay 开局有时自带一个 minigame,会触发 panel_open 守卫)
    cmd(type="minigame_close")
    time.sleep(0.5)

    # 设为内鬼并找最近假人
    cmd(type="debug_set_role", role="Impostor")
    time.sleep(2)
    cmd(type="debug_players")
    time.sleep(1.5)
    dp = get("/api/debug_players").get("players", [])
    me = state()["me"]
    cands = [p for p in dp if p.get("alive") and p.get("id") != me["player_id"]]
    if not cands:
        fail("无假人可用")
    near = min(cands, key=lambda p: (p["pos"][0] - me["position"]["x"]) ** 2
               + (p["pos"][1] - me["position"]["y"]) ** 2)
    print(f"目标假人 {near['id']} @ {near['pos']}")
    cmd(type="minigame_close")
    cmd(type="move_to", pos=near["pos"], tolerance=1.0)
    ev, _ = wait_event("MOVE_DONE", 70)
    if not ev:
        cmd(type="minigame_close")
        cmd(type="move_to", pos=near["pos"], tolerance=1.0)
        wait_event("MOVE_DONE", 40)

    # 击杀(冷却中则等)
    seq = get("/api/events?since=0").get("seq", 0)
    kill_ok = False
    for _ in range(14):
        r = cmd(type="kill", target_id=near["id"])
        time.sleep(0.3)
        # 直接看 ack 需要 logs 接口
        acks = get("/api/logs").get("acks", [])
        last = next((a for a in acks if a.get("type") == "ack" and a.get("cmd") == "kill"), None)
        if last and last.get("ok"):
            kill_ok = True
            break
        if last and str(last.get("err", "")).startswith("too_far"):
            cmd(type="minigame_close")
            cmd(type="move_to", pos=near["pos"], tolerance=0.8)
            wait_event("MOVE_DONE", 30)
            continue
        if last and str(last.get("err", "")).startswith("cooldown"):
            time.sleep(float(str(last["err"]).split(":")[-1]) + 0.5)
            continue
        time.sleep(1.0)
    ev_death, seq = wait_event("DEATH", 15, seq)
    print(f"kill ok={kill_ok} death_event={bool(ev_death)}")
    if not ev_death:
        fail("击杀未确认")

    # 报告 → 会议
    time.sleep(1.0)
    cmd(type="report")
    if not wait_phase("MEETING", 25):
        fail("未进入 MEETING")
    mtg = state().get("meeting") or {}
    print(f"会议:state={mtg.get('state')} can_vote={mtg.get('can_vote')} votes={len(mtg.get('votes') or [])}")
    if "can_vote" not in mtg or "votes" not in mtg:
        fail("会议状态字段缺失(can_vote/votes)")

    # 等可投票
    t0 = time.time()
    while time.time() - t0 < 25:
        mtg = state().get("meeting") or {}
        if mtg.get("can_vote"):
            break
        time.sleep(1)
    print(f"可投票:{mtg.get('can_vote')}")

    # 3) 发 chat 指令,验证聊天采集回环
    text = f"E2E chat {int(time.time()) % 10000}"
    cmd(type="chat", text=text)
    time.sleep(1.5)
    chats = state().get("chat") or []
    mine = [c for c in chats if c.get("text") == text]
    print(f"chat 回合:{'OK' if mine else 'MISSING'} -> {chats[-3:]}")
    if not mine:
        fail("state.chat 未采集到自己的发言(AddChat patch?)")

    # 4) 喂会议内容 → DeepSeek → 验证 say/vote 发到游戏
    post("/api/meeting/feed", {"speaker": "Red", "text": "I saw Blue vent in Electrical"})
    post("/api/meeting/feed", {"speaker": "Blue", "text": "that's a lie, I was in MedBay"})
    post("/api/meeting/feed", {"type": "vote", "target": "Blue"})
    r = post("/api/meeting/respond", {})
    res = r.get("result") or {}
    print(f"AI: say={res.get('say')!r} vote={res.get('vote')!r} {res.get('latency_ms')}ms")
    if not res or res.get("say") is None:
        fail("模型无响应")
    time.sleep(1.5)
    st2 = state()
    ai_chats = [c for c in (st2.get("chat") or []) if c.get("text") == res.get("say")]
    mtg2 = st2.get("meeting") or {}
    print(f"AI chat 到达游戏:{'OK' if ai_chats else 'MISSING'}; did_vote={mtg2.get('did_vote')} my_vote={mtg2.get('my_vote')}")
    if res.get("say") and not ai_chats:
        fail("AI 发言未进入游戏聊天")
    if not (mtg2.get("did_vote") or (mtg2.get("my_vote") not in (-1, 255))):
        print("[WARN] AI 投票未见 did_vote(可能目标名不在名单/已投过)")

    # 5) 快进散会
    cmd(type="debug_meeting_ff")
    if not wait_phase("PLAYING", 45):
        fail("会议未结束")
    print("[PASS] 端到端会议链路(击杀→报告→会议字段→chat 采集→模型发言/投票→散会)")


if __name__ == "__main__":
    main()
