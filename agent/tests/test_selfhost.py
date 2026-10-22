#!/usr/bin/env python3
"""整局自托管验收(需要游戏在 MENU、agent 已启动、XTTS 在跑)。

流程:本地开房 → 补 3 个 bot → 开局 → 强制 AI 内鬼(其余改船员)→ AI 杀人 →
bot 局自动紧急会议(语音转写/发言/投票)→ 分出胜负 → 自动下一局 → 第二局开局。

运行:python tests/test_selfhost.py [--bots 3] [--games 2] [--no-force-role]
"""
import argparse
import json
import sys
import time
import urllib.request

WEB = "http://127.0.0.1:8765"
BRIDGE = ("127.0.0.1", 47600)
PASS = FAIL = 0


def check(label, cond, detail=""):
    global PASS, FAIL
    if cond:
        PASS += 1
        print(f"[PASS] {label}")
    else:
        FAIL += 1
        print(f"[FAIL] {label} {detail}")


def http(path, obj=None, timeout=30):
    if obj is None:
        with urllib.request.urlopen(WEB + path, timeout=timeout) as r:
            return json.load(r)
    req = urllib.request.Request(WEB + path, data=json.dumps(obj).encode(),
                                 headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=timeout) as r:
        return json.load(r)


class Bridge:
    def __init__(self):
        import socket
        self.sock = socket.create_connection(BRIDGE, timeout=5)
        self.sock.settimeout(0.3)
        self.buf = b""
        self.ref = 0

    def send(self, **cmd):
        self.ref += 1
        cmd["ref"] = f"sh{self.ref}"
        self.sock.sendall((json.dumps(cmd) + "\n").encode())

    def pump(self, sec):
        out = []
        t0 = time.time()
        while time.time() - t0 < sec:
            try:
                data = self.sock.recv(65536)
            except Exception:
                continue
            if not data:
                break
            self.buf += data
            while b"\n" in self.buf:
                line, self.buf = self.buf.split(b"\n", 1)
                try:
                    out.append(json.loads(line))
                except Exception:
                    pass
        return out


def wait_for(fn, timeout, step=1.0):
    t0 = time.time()
    last = None
    while time.time() - t0 < timeout:
        last = fn()
        if last:
            return last
        time.sleep(step)
    return last


def fmt_mmss(t):
    return f"{int(t)//60}:{int(t)%60:02d}"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--bots", type=int, default=3)
    ap.add_argument("--games", type=int, default=2)
    ap.add_argument("--no-force-role", action="store_true")
    args = ap.parse_args()

    # 0) 环境
    st = http("/api/status")
    check("agent 已连桥", st.get("connected"), str(st))
    sess0 = http("/api/session")
    if sess0.get("phase") != "MENU":
        print(f"当前阶段 {sess0.get('phase')},先回菜单…")
        br = Bridge()
        br.send(type="leave_game")
        br.pump(3)
        time.sleep(4)

    # 1) 启动整局托管
    r = http("/api/session", {"action": "start", "host_kind": "local", "bots": args.bots,
                              "min_players": 1 + args.bots, "auto_start": True,
                              "auto_next": True, "max_games": args.games, "map": 4})
    check("整局托管启动", r.get("enabled"), json.dumps(r, ensure_ascii=False)[:200])

    br = Bridge()
    events = []
    seen = set()

    def scan(sec):
        nonlocal events
        msgs = br.pump(sec)
        for m in msgs:
            if m.get("type") == "event":
                key = (m.get("code"), m.get("_seq", id(m)))
                if key not in seen:
                    seen.add(key)
                    events.append(m)
                    print(f"  [event] {m.get('code')} {m.get('note') or ''}")

    # 2) 大厅(bot 在开局后补,大厅只等房主)
    lobby = wait_for(lambda: (lambda s: s if s.get("phase") == "LOBBY" and s.get("players") >= 1 else None)(http("/api/session")), 90, 1.0)
    check("进入大厅", bool(lobby), str(lobby and {"phase": lobby.get("phase"), "players": lobby.get("players")}))
    if not lobby:
        return 1
    print(f"  大厅人数={lobby.get('players')} host={lobby.get('am_host')} mode={lobby.get('lobby',{}).get('network_mode')}")

    # 3) 开局
    def playing():
        s = http("/api/session")
        return s if s.get("phase") == "PLAYING" else None
    s = wait_for(playing, 90, 1.0)
    check("自动开局进入 PLAYING", bool(s), str(s and s.get("phase")))
    if not s:
        return 1
    scan(1)
    print(f"  本局开始: games_started={s.get('games_started')} round={s.get('round')}")

    # 4) 确定性角色:AI 内鬼,其余船员(等开场动画结束,否则会被 mod 角色分配覆盖)
    def intro_done():
        x = http("/api/state")["state"]
        return x if x.get("phase") == "PLAYING" and x.get("intro") is False else None
    st_intro = wait_for(intro_done, 60, 1.0)
    check("开场动画结束", bool(st_intro), str(st_intro and st_intro.get("intro")))
    if not st_intro:
        return 1
    # 开局后补 bot(等过场结束)
    bots_ready = wait_for(lambda: (lambda x: x if (x.get("lobby") or {}).get("players", 0) >= 1 + args.bots else None)(
        http("/api/state")["state"]), 45, 1.0)
    check(f"开局后补齐 {args.bots} 个 bot", bool(bots_ready),
          str(bots_ready and (bots_ready.get("lobby") or {}).get("players")))
    time.sleep(0.5)
    stt = http("/api/state")["state"]
    me = stt.get("me") or {}
    if not me:
        check("本局结束过快(me=null)", False, f"phase={stt.get('phase')} t={stt.get('t')}")
        return 1
    print(f"  AI 身份: {me.get('role')}/{me.get('team')}  位置={me.get('position')}")
    if not args.no_force_role and not str(me.get("team", "")).startswith("Impostor"):
        others = [p for p in (stt.get("players") or []) if p.get("alive")]
        for p in others:
            br.send(type="debug_set_role", role="Crewmate", target_id=p["id"])
            br.pump(0.25)
        br.send(type="debug_set_role", role="Impostor")
        br.pump(0.8)
        stt = http("/api/state")["state"]
        me = stt.get("me") or {}
        check("强制 AI=内鬼/其余=船员", str(me.get("team", "")).startswith("Impostor"),
              f"{me.get('role')}/{me.get('team')}")

    # 5) 等击杀(行为引擎自动巡猎;刀 CD 已在开局参数里设为 8s)
    def wait_event(codes, timeout):
        t0 = time.time()
        while time.time() - t0 < timeout:
            scan(0.7)
            for e in events:
                if e.get("code") in codes:
                    return e
        return None
    def killed():
        scan(0.7)
        x = http("/api/state")["state"]
        dead = [p for p in (x.get("players") or []) if not p.get("alive")]
        return x if dead else None
    killed_ok = wait_for(killed, 120, 1.0)
    dead_names = [p.get("name") for p in (killed_ok or {}).get("players") or [] if not p.get("alive")]
    check("AI 完成击杀", bool(killed_ok), str(dead_names))
    if not killed_ok:
        return 1

    # 6) 会议(bot 局自动触发)
    def wait_phase(want, timeout):
        t0 = time.time()
        last = None
        while time.time() - t0 < timeout:
            scan(0.7)
            last = http("/api/session")
            if last.get("phase") == want:
                return last
        return None
    s = wait_phase("MEETING", 45)
    check("进入 MEETING", bool(s), str(s and s.get("phase")))
    if not s:
        return 1
    time.sleep(3)
    v = http("/api/voice")
    print(f"  voice: active={v.get('active')} asr={v.get('asr',{}).get('connected')} order={v.get('speaking_order')}")
    check("语音管线接管会议", v.get("active"), str(v.get("last_error")))
    check("发言顺序已上报", len(v.get("speaking_order") or []) >= 2, str(v.get("speaking_order")))

    # 7) AI 发言(TTS 真播)+ 表决
    def spoke():
        vv = http("/api/voice")
        return vv if vv.get("my_state") == "done" and (vv.get("my_last") or {}).get("say") else None
    v2 = wait_for(spoke, 40, 1.0)
    check("AI 会议发言(模型+语音)", bool(v2), str(v2 and v2.get("my_state")))
    if v2:
        print(f"  发言: {str((v2.get('my_last') or {}).get('say'))[:90]}")
    v3 = wait_for(lambda: (lambda x: x if x.get("voted") else None)(http("/api/voice")), 45, 1.0)
    check("AI 完成表决", bool(v3), str(v3 and v3.get("voted")))

    # 8) 本局结束
    s = wait_phase("ENDED", 200)
    check("本局分出胜负(ENDED)", bool(s), str(s and s.get("phase")))
    scan(1)
    if s:
        print(f"  games_finished={s.get('games_finished')} 事件={[e.get('code') for e in events][-8:]}")

    # 9) 自动下一局
    if args.games > 1:
        def second():
            scan(0.7)
            x = http("/api/session")
            return x if x.get("games_started", 0) >= 2 and x.get("phase") in ("PLAYING", "LOBBY", "MEETING") else None
        s2 = wait_for(second, 120, 1.0)
        check("自动进入第二局", bool(s2), str(s2 and {"phase": s2.get("phase"), "started": s2.get("games_started")}))
        if s2:
            print(f"  第二局: phase={s2.get('phase')} started={s2.get('games_started')} players={s2.get('players')}")
            # 第二局也完整打完:补 bot → 击杀 → 会议 → ENDED,且 max_games 到时托管自动停
            bots2 = wait_for(lambda: (lambda x: x if (x.get("lobby") or {}).get("players", 0) >= 1 + args.bots else None)(
                http("/api/state")["state"]), 60, 1.0)
            check("第二局补齐 bot", bool(bots2), str(bots2 and (bots2.get("lobby") or {}).get("players")))
            def fin2():
                scan(0.7)
                x = http("/api/session")
                return x if x.get("games_finished", 0) >= 2 else None
            s3 = wait_for(fin2, 420, 2.0)
            check("第二局也打完(max_games=2)", bool(s3), str(s3 and {"fin": s3.get("games_finished"), "phase": s3.get("phase")}))

    http("/api/session", {"action": "stop"})
    print(f"\nselfhost: {PASS} pass, {FAIL} fail")
    return 0 if FAIL == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
