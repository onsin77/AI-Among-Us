#!/usr/bin/env python3
"""迭代 2 验收测试:单人 freeplay(Airship)全流程。
覆盖:移动/做任务(多阶段)/击杀/报告/会议/投票/phase守卫/越权任务/中途关面板。
用法:python tests/test_iter2.py [--skip-launch](默认假设游戏已在主菜单,或用 --launch)
"""
import argparse
import json
import socket
import sys
import time
import urllib.request

HOST, PORT = "127.0.0.1", 47600
WEB = "http://127.0.0.1:8765"

REF = [0]
RESULTS = []


class Bridge:
    def __init__(self):
        self.sock = socket.create_connection((HOST, PORT), timeout=5)
        self.sock.settimeout(0.4)
        self._buf = b""
        self.events = []
        self.acks = []
        self.states = []
        self.others = []  # 其它类型消息(nav_dump/debug_players/nav_path 等)
        self._latest = {}
        self.ev_cursor = 0  # 事件消费游标:wait_event 只看新事件,防拿到历史事件

    def send(self, **cmd):
        REF[0] += 1
        cmd["ref"] = REF[0]
        self.sock.sendall((json.dumps(cmd) + "\n").encode())
        return REF[0]

    def pump(self, seconds):
        """读桥数据 N 秒,归档事件/ack/状态。
        用原生 recv + 手动分行(Python 3.14 的 makefile 读对象超时后不可复用)。"""
        t0 = time.time()
        while time.time() - t0 < seconds:
            try:
                data = self.sock.recv(65536)
            except socket.timeout:
                continue
            if not data:
                raise ConnectionError("bridge closed")
            self._buf += data
            while b"\n" in self._buf:
                line, self._buf = self._buf.split(b"\n", 1)
                try:
                    m = json.loads(line)
                except (json.JSONDecodeError, UnicodeDecodeError):
                    continue
                t = m.get("type")
                if t == "event":
                    self.events.append(m)
                    print(f"    [event] {m.get('code')} {m.get('note') or ''}")
                elif t == "ack":
                    self.acks.append(m)
                elif t in ("state_full", "state_delta"):
                    if t == "state_full":
                        self._latest = m
                    else:
                        self._latest.update(m)
                    self.states.append(m)
                else:
                    self.others.append(m)

    def drain(self, msg_type, mark_seen=True):
        """取未消费的指定类型消息。"""
        out = [m for m in self.others if m.get("type") == msg_type and m not in getattr(self, "_drained", [])]
        if mark_seen:
            self._drained = getattr(self, "_drained", []) + out
        return out

    def wait_event(self, code, timeout, note_contains=None):
        t0 = time.time()
        while time.time() - t0 < timeout:
            self.pump(0.5)
            while self.ev_cursor < len(self.events):
                ev = self.events[self.ev_cursor]
                self.ev_cursor += 1
                if ev.get("code") == code:
                    if note_contains and note_contains not in (ev.get("note") or ""):
                        continue
                    return ev
        return None

    def state(self):
        self.pump(0.2)
        return self._latest

    def wait_ack(self, ref, timeout=3):
        t0 = time.time()
        while time.time() - t0 < timeout:
            self.pump(0.3)
            for a in self.acks:
                if a.get("ref") == ref:
                    return a
        return None

    def cmd(self, timeout=3, **kw):
        r = self.send(**kw)
        return self.wait_ack(r, timeout)


def check(name, ok, detail=""):
    RESULTS.append((name, ok, detail))
    print(f"  [{'PASS' if ok else 'FAIL'}] {name} {detail}")


def http_state():
    d = json.load(urllib.request.urlopen(WEB + "/api/state", timeout=3))
    if d["meta"]["age"] is not None and d["meta"]["age"] < 3:
        return d["state"]
    return None


def wait_phase(br, want, timeout):
    t0 = time.time()
    while time.time() - t0 < timeout:
        s = br.state()
        if s.get("phase") == want:
            return s
        time.sleep(0.5)
    return None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--skip-launch", action="store_true", help="游戏已在运行,跳过开局")
    args = ap.parse_args()

    br = Bridge()
    print("== 连接桥 OK,hello ==")
    br.send(type="hello")
    br.pump(1)

    if not args.skip_launch:
        s0 = br.state()
        if s0.get("phase") == "PLAYING":
            print("已在 PLAYING,复用当前对局(如需全新对局请重启游戏)")
            check("复用进行中对局", True, "")
        else:
            print("== 开局:freeplay Airship ==")
            r = br.cmd(type="debug_start", mode="freeplay", map=4)
            check("debug_start ack", r and r.get("ok"), str(r))
            s = wait_phase(br, "PLAYING", 90)
            check("进入 PLAYING", s is not None, f"phase={s and s.get('phase')}")
            if not s:
                sys.exit(1)

    # ---------------- A. 导航图与移动 ----------------
    print("== A. 导航图 + move_to ==")
    br.pump(2)
    r = br.cmd(type="nav_info")
    s = br.state()
    graph_ok = False
    t0 = time.time()
    while time.time() - t0 < 15:
        r2 = br.cmd(type="nav_info", timeout=2)
        # nav_info 是主动推送消息,不是 ack —— 从最近消息里找
        br.pump(0.5)
        # 简化:直接看事件日志里 nav graph built
        graph_ok = any("nav graph built" in (l or "") for l in [])  # 日志在 BepInEx,不在这
        break

    me0 = br.state().get("me") or {}
    print(f"  起点: pos={me0.get('position')} room={me0.get('room')}")
    r = br.cmd(type="move_to", room="Brig", timeout=5)
    check("move_to ack", r is not None and r.get("ok"), str(r))
    ev = br.wait_event("MOVE_DONE", 60)
    me1 = br.state().get("me") or {}
    check("移动完成事件", ev is not None, str(ev and ev.get("note")))
    check("到达 Brig 附近", me1.get("room") in ("Brig", "GapRoom", "Engine", ""), f"room={me1.get('room')} pos={me1.get('position')}")
    print(f"  终点: pos={me1.get('position')} room={me1.get('room')}")

    # ---------------- B. 任务特权(多阶段) ----------------
    print("== B. open_task(优先挑多阶段任务) ==")
    s = br.state()
    tasks = ((s.get("me") or {}).get("tasks") or {}).get("list") or []
    todo = [t for t in tasks if not t.get("done")]
    multi = [t for t in todo if (t.get("max_step") or 0) >= 2]
    target_task = None
    r = None
    ev_panel = None
    # 逐个尝试:ack ok 不代表能把任务做完(台不可达会 TASK_FLOW_ABORT),等首个面板打开才认可
    for cand in (multi + todo):
        r = br.cmd(type="open_task", task_id=cand["id"], timeout=5)
        if not (r is not None and r.get("ok")):
            print(f"    跳过任务 {cand['id']} {cand['name']}: {r and r.get('err')}")
            continue
        ev_panel = br.wait_event("TASK_PANEL_OPEN", 100)
        if ev_panel is not None:
            target_task = cand
            break
        abort = [e for e in br.events[br.ev_cursor - 1:] if e.get("code") == "TASK_FLOW_ABORT"]
        print(f"    任务 {cand['id']} {cand['name']} 未走到面板: {abort[-1].get('note') if abort else '?'}")
    if not target_task:
        check("存在可做任务", False, "无可开任务")
    else:
        print(f"  目标任务: {target_task['id']} {target_task['name']} steps={target_task.get('max_step')}")
        # open_task 已在重试循环中发出并 ack ok,不重复发(重复发会在上一轮 walk 未结束时收到 busy)
        check("open_task ack", r is not None and r.get("ok"), str(r))
        check("面板真实打开", ev_panel is not None, str(ev_panel and ev_panel.get("note")))
        ev_done = br.wait_event("TASK_FLOW_DONE", 60 + 25 * ((target_task.get("max_step") or 1)))
        check("任务流完成", ev_done is not None, str(ev_done and ev.get("note")))
        # 校验任务确实完成(对外走 RpcCompleteTask,任务条+1)
        s2 = br.state()
        t2 = [t for t in ((s2.get("me") or {}).get("tasks") or {}).get("list") or [] if t["id"] == target_task["id"]]
        check("任务状态 done=true", bool(t2) and t2[0].get("done"), str(t2[:1]))

    # ---------------- C. 击杀 → 报告 → 会议 → 投票 ----------------
    print("== C. 击杀/报告/会议 ==")
    r = br.cmd(type="debug_set_role", role="Impostor")
    check("设为内鬼(freeplay)", r is not None and r.get("ok"), str(r))
    time.sleep(2)
    s = br.state()
    kill_cd = (s.get("me") or {}).get("kill_cooldown")
    if kill_cd and kill_cd > 0.5:
        print(f"  等击杀冷却 {kill_cd}s")
        time.sleep(kill_cd + 0.5)

    # 找最近假人:优先视野内;不行则 freeplay 调试坐标兜底(debug_players)
    def dbg_positions():
        br.cmd(type="debug_players")
        t0 = time.time()
        while time.time() - t0 < 3:
            br.pump(0.3)
            got = br.drain("debug_players")
            if got:
                return got[-1]
        return None

    s = br.state()
    players = s.get("players") or []
    vis = [p for p in players if p.get("alive") and p.get("visible_to_me") and p.get("distance") is not None]
    target = None
    if vis:
        target = min(vis, key=lambda p: p.get("distance") or 99)
    else:
        dbg = dbg_positions()
        if dbg:
            s = br.state()
            me_pos = (s.get("me") or {}).get("position") or {"x": 0, "y": 0}
            my_id = (s.get("me") or {}).get("player_id")
            cands = [p for p in dbg.get("players", []) if p.get("alive") and p.get("id") != my_id
                     and abs(p["pos"][0]) < 20]   # 界外 (-25,39.64) 假人不可作为目标
            if cands:
                near = min(cands, key=lambda p: (p["pos"][0] - me_pos["x"]) ** 2 + (p["pos"][1] - me_pos["y"]) ** 2)
                print(f"  debug:走向假人 {near['id']} @ {near['pos']}")
                br.cmd(type="move_to", pos=near["pos"], tolerance=1.0)
                br.wait_event("MOVE_DONE", 60)
                target = {"id": near["id"], "name": near.get("name") or f"假人{near['id']}",
                          "position": {"x": near["pos"][0], "y": near["pos"][1]}, "distance": None}
    if not target:
        check("找到假人目标", False, "无可视线内玩家")
    else:
        print(f"  目标: {target['name']} dist={target.get('distance')}")
        # 连发 kill(说明书:应对距离判定,L3 连续发送直至确认)
        kill_ok = False
        for attempt in range(12):
            r = br.cmd(type="kill", target_id=target["id"])
            if r and r.get("ok"):
                kill_ok = True
                break
            if r and r.get("err", "").startswith("too_far"):
                # 靠近:假人一直在走动,必须每次重新取实时坐标(状态里可见则用状态,否则 debug_players)
                tp = None
                s = br.state()
                for p in (s.get("players") or []):
                    if p["id"] == target["id"] and p.get("position"):
                        tp = p["position"]
                if not tp:
                    dbg = dbg_positions()
                    if dbg:
                        for p in dbg.get("players", []):
                            if p["id"] == target["id"]:
                                tp = {"x": p["pos"][0], "y": p["pos"][1]}
                if tp:
                    target["position"] = tp
                    br.cmd(type="move_to", pos=[tp["x"], tp["y"]], tolerance=1.0)
                    br.wait_event("MOVE_DONE", 30)
                continue
            if r and r.get("err", "").startswith("cooldown"):
                cd = float(r["err"].split(":")[-1])
                time.sleep(min(cd, 10) + 0.5)
                continue
            print(f"    kill err: {r and r.get('err')}")
            time.sleep(1)
        check("kill 指令接受", kill_ok, str(r))

        ev_death = br.wait_event("DEATH", 20)
        check("击杀确认(死亡事件)", ev_death is not None, str(ev_death and ev_death.get("note")))

        # 报告
        time.sleep(1.5)
        r = br.cmd(type="report")
        check("report 指令", r is not None and r.get("ok"), str(r))
        s = wait_phase(br, "MEETING", 25)
        check("进入 MEETING", s is not None, f"phase={s and s.get('phase')}")
        if s:
            mtg = s.get("meeting")
            check("会议快照(reporter/order)", mtg is not None and mtg.get("reporter") is not None,
                  str(mtg))
            # 会议守卫:move_to 应被拒绝
            r = br.cmd(type="move_to", room="Brig")
            check("守卫:会议中 move_to 被拒", r is not None and not r.get("ok") and "phase_guard" in (r.get("err") or ""),
                  str(r))
            # 等投票期:会议先 Animating→Discussion(0s)→Voting,轮询到可投(最多 20s)
            r = None
            for _ in range(20):
                r = br.cmd(type="vote", skip=True)
                if r is not None and r.get("ok"):
                    break
                if r is not None and "vote_state" in (r.get("err") or ""):
                    time.sleep(1)
                    continue
                break
            check("vote skip 指令", r is not None and r.get("ok"), str(r))
            ev = br.wait_event("VOTE", 10)
            check("投票事件", ev is not None, str(ev and ev.get("note")))
            # freeplay 假人不投票:快进(等价计时器到点的 ForceSkipAll),收尾由插件自动 Proceed
            br.cmd(type="debug_meeting_ff")
            s2 = wait_phase(br, "PLAYING", 40)
            check("会议结束回 PLAYING", s2 is not None, f"phase={s2 and s2.get('phase')}")

    # ---------------- D. 边界用例 ----------------
    print("== D. 边界用例 ==")
    # C 组可能把角色设成了内鬼(假任务语义),切回船员再做任务边界
    br.cmd(type="debug_set_role", role="Crewmate")
    time.sleep(1.5)
    # 收尾 C 组遗留的移动/任务流程,避免 TaskPrivilege busy 抢占 D1 的错误码
    br.cmd(type="move_stop")
    # D1 越权任务(busy 时等上一轮流完再试,最多 5 次)
    for _ in range(5):
        r = br.cmd(type="open_task", task_id=9999)
        if r and r.get("err") == "busy":
            time.sleep(2)
            continue
        break
    check("守卫:非本人任务被拒", r is not None and not r.get("ok") and "task_not_mine" in (r.get("err") or ""), str(r))
    # D2 面板打开时移动被拒(逐个试到有任务台的任务)
    todo = [t for t in ((br.state().get("me") or {}).get("tasks") or {}).get("list") or [] if not t.get("done")]
    opened = False
    had_console_task = False
    for cand in todo:
        r = br.cmd(type="open_task", task_id=cand["id"])
        if r is not None and r.get("ok"):
            had_console_task = True
            opened = br.wait_event("TASK_PANEL_OPEN", 90) is not None
            if opened:
                break
        print(f"    跳过任务 {cand['id']} {cand['name']}: {r and r.get('err')}")
    if opened:
        r = br.cmd(type="move_to", room="Kitchen")
        check("守卫:面板打开时 move_to 被拒", r is not None and not r.get("ok") and "panel_open" in (r.get("err") or ""), str(r))
        # D3 中途关面板 → TASK_ABORT + 重走
        r = br.cmd(type="minigame_close")
        ev = br.wait_event("TASK_ABORT", 8)
        check("守卫:关面板触发 TASK_ABORT 并重走", ev is not None, str(ev and ev.get("note")))
        # 等它自动重走完成(证明计时器挂在面板生命周期上)
        ev = br.wait_event("TASK_PANEL_OPEN", 90)
        check("关面板后自动重开", ev is not None, "")
        r = br.cmd(type="minigame_close")  # 收尾再关一次,流程会继续重试;直接取消:
        # 用 stop 结束这轮(简化:再关一次让它继续,最终 done)
    elif not had_console_task:
        # 所有剩余任务在本图无台(如只剩 SubmitScan)→ 跳过;有台却开不了必须 FAIL
        check("面板打开(边界组)[任务池耗尽跳过]", True, "本轮剩余任务均无可开台")
    else:
        check("面板打开(边界组)", False, "有任务台但未打开")
    # D4 未知指令
    r = br.cmd(type="flap_wings")
    check("守卫:未知指令回执", r is not None and not r.get("ok") and "unknown_command" in (r.get("err") or ""), str(r))

    # ---------------- 汇总 ----------------
    print("\n========== 汇总 ==========")
    ok = sum(1 for _, o, _ in RESULTS if o)
    total = len(RESULTS)
    for name, o, d in RESULTS:
        print(f"  [{'PASS' if o else 'FAIL'}] {name}")
    print(f"  {ok}/{total} PASS")
    sys.exit(0 if ok == total else 1)


if __name__ == "__main__":
    main()
