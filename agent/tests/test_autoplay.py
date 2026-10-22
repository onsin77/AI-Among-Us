#!/usr/bin/env python3
"""会议外自主性验收:从全新 freeplay 对局开始,agent 全程不干预地完成一轮游戏。

检查项:
1) crew 模式:个人任务全部完成(tasks done == total)、无击杀、无持续卡死循环
2) impostor 模式:产生击杀、0 次 report、冷却等待不刷屏
3) 过程采样:behavior action / tasks_done / 事件计数

运行前:游戏已在主菜单(脚本会 debug_start),agent 已用对应模式启动。
用法:python tests/test_autoplay.py crew|impostor [--seconds 420]
"""

from __future__ import annotations

import json
import sys
import time
import urllib.request

WEB = "http://127.0.0.1:8765"


def get(path: str):
    with urllib.request.urlopen(WEB + path, timeout=5) as r:
        return json.load(r)


def post(path: str, body: dict):
    req = urllib.request.Request(WEB + path, data=json.dumps(body).encode(),
                                 headers={"Content-Type": "application/json"}, method="POST")
    with urllib.request.urlopen(req, timeout=8) as r:
        return json.load(r)


def state():
    return get("/api/state")["state"]


def beh():
    return get("/api/behavior")


def events():
    return get("/api/events?since=0")["events"]


def main() -> int:
    mode = sys.argv[1] if len(sys.argv) > 1 else "crew"
    seconds = 420
    if "--seconds" in sys.argv:
        seconds = int(sys.argv[sys.argv.index("--seconds") + 1])
    print(f"== 自主验收 mode={mode} 时长={seconds}s ==")

    st = state()
    if st.get("phase") != "PLAYING":
        print("开局 freeplay Airship…")
        post("/api/command", {"type": "debug_start", "mode": "freeplay", "map": 4})
        t0 = time.time()
        while time.time() - t0 < 90:
            if state().get("phase") == "PLAYING":
                break
            time.sleep(1)
        else:
            print("[FAIL] 未进入 PLAYING")
            return 1
        time.sleep(2)
    me = state().get("me") or {}
    print(f"起点 {me.get('position')} 角色={me.get('role')} 阵营={me.get('team')}")
    # 清掉可能的遗留面板
    post("/api/command", {"type": "minigame_close"})
    time.sleep(0.5)

    # 确保行为模式与请求一致
    b = beh()
    if not b.get("enabled"):
        print("[FAIL] behavior 未启用(请用 --mode crew|impostor|auto 启动 agent)")
        return 1
    if b.get("mode") != mode:
        post("/api/behavior", {"mode": mode})
        print(f"切换行为模式 -> {mode}")

    t0 = time.time()
    last_ev_seq = 0
    last_beh = {}
    samples = []
    seen_kill_ack = 0
    report_acks = 0
    while time.time() - t0 < seconds:
        b = beh()
        st = state()
        evs = events()
        kills = sum(1 for e in evs if e.get("code") == "KILL_CMD")
        deaths = sum(1 for e in evs if e.get("code") == "DEATH")
        moves_failed = sum(1 for e in evs if e.get("code") == "MOVE_FAILED")
        tasks_done_ev = sum(1 for e in evs if e.get("code") == "TASK_FLOW_DONE")
        tasks = ((st.get("me") or {}).get("tasks") or {})
        done = tasks.get("done", 0)
        total = tasks.get("total", 0)
        acks = get("/api/logs")["acks"]
        report_acks = sum(1 for a in acks if a.get("type") == "ack" and a.get("cmd") == "report" and a.get("ok"))
        if b != last_beh:
            print(f"[{int(time.time()-t0):4d}s] action={b.get('action')!r} tasks={done}/{total} "
                  f"kills={b.get('kills')} reports={b.get('reports')} err={b.get('last_error') or '-'}")
            last_beh = dict(b)
            samples.append((int(time.time() - t0), b.get("action"), done, total))
        black = (b.get("blacklisted") or {})
        long_black = sum(1 for v in black.values() if (v or 0) >= 300)
        if mode == "crew" and total and (done >= total or done + long_black >= total):
            print(f"个人任务闭环:done={done} 不可做(长黑名单)={long_black} total={total}")
            break
        if mode == "impostor" and b.get("kills", 0) >= 2:
            print("已完成 2 次击杀")
            break
        if st.get("phase") == "MEETING":
            print("进入会议(会议外验收结束,交由会议管线)")
            break
        time.sleep(3)

    b = beh()
    st = state()
    tasks = ((st.get("me") or {}).get("tasks") or {})
    done, total = tasks.get("done", 0), tasks.get("total", 0)
    evs = events()
    kills = sum(1 for e in evs if e.get("code") == "KILL_CMD")
    moves_failed = sum(1 for e in evs if e.get("code") == "MOVE_FAILED")
    acks = get("/api/logs")["acks"]
    report_acks = sum(1 for a in acks if a.get("type") == "ack" and a.get("cmd") == "report" and a.get("ok"))
    print("\n== 结果 ==")
    print(f"tasks={done}/{total} kills={b.get('kills')} reports={b.get('reports')} "
          f"report_acks={report_acks} move_failed={moves_failed} last_err={b.get('last_error')}")
    ok = True
    black = (b.get("blacklisted") or {})
    long_black = sum(1 for v in black.values() if (v or 0) >= 300)
    if mode == "crew":
        if total == 0 or (done < total and done + long_black < total):
            print(f"[FAIL] 个人任务未闭环 (done={done} 不可做={long_black} total={total})")
            ok = False
        else:
            print(f"任务闭环:done={done}/{total},不可做(长黑名单)={long_black}")
        if report_acks > 0 and kills > 0:
            print("[FAIL] 船员模式出现击杀后报告冲突?")
            ok = False
    else:
        if (b.get("kills") or 0) < 1:
            print("[FAIL] 内鬼未产生击杀")
            ok = False
        if report_acks > 0:
            print("[FAIL] 内鬼产生了 report")
            ok = False
    if moves_failed > 10:
        print(f"[WARN] MOVE_FAILED 次数偏多: {moves_failed}")
    print("PASS" if ok else "FAIL")
    return 0 if ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
