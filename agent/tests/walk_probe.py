#!/usr/bin/env python3
"""走测探针:开局后向指定坐标逐点 move_to,报告终态/位置/房间。
用法:python walk_probe.py [x y label] ...(默认一组争议点)
"""
import json
import socket
import sys
import time

HOST, PORT = "127.0.0.1", 47600


class P:
    def __init__(self):
        self.s = socket.create_connection((HOST, PORT), timeout=5)
        self.s.settimeout(0.4)
        self.buf = b""
        self.events = []

    def send(self, **cmd):
        self.s.sendall((json.dumps(cmd) + "\n").encode())

    def pump(self, sec):
        t0 = time.time()
        while time.time() - t0 < sec:
            try:
                d = self.s.recv(65536)
            except socket.timeout:
                continue
            if not d:
                raise ConnectionError("closed")
            self.buf += d
            while b"\n" in self.buf:
                line, self.buf = self.buf.split(b"\n", 1)
                try:
                    m = json.loads(line)
                except Exception:
                    continue
                if m.get("type") == "event":
                    self.events.append(m)
                    print("   [ev]", m.get("code"), m.get("note"))
                elif m.get("type") == "state_full":
                    self.state = m
                elif m.get("type") == "state_delta":
                    self.state.update(m)

    def me(self):
        self.pump(0.4)
        st = getattr(self, "state", None)
        if st:
            mm = st.get("me") or {}
            return mm.get("position"), mm.get("room")
        return None, None


def main():
    p = P()
    p.send(type="hello")
    p.pump(1)
    if "--skip-launch" not in sys.argv:
        p.send(type="debug_start", mode="freeplay", map=4, ref=1)
        t0 = time.time()
        started = False
        while time.time() - t0 < 75:
            p.pump(0.5)
            pos, room = p.me()
            if pos is not None:
                started = True
                print("PLAYING, me:", pos, room)
                break
        if not started:
            print("!! freeplay 未开局(Steam 在线?上次异常?)")
            raise SystemExit(1)
        time.sleep(2)
    args = sys.argv[1:]
    tests = []
    i = 0
    while i + 2 < len(args) + 1:
        if i + 3 <= len(args):
            tests.append((float(args[i]), float(args[i + 1]), args[i + 2] if i + 2 < len(args) else ""))
            i += 3
        else:
            break
    if not tests:
        tests = [(2.0, 11.0, "争议区西部"), (4.5, 12.0, "梯子正邻"), (6.0, 12.0, "坑洞中心假设")]
    for x, y, label in tests:
        p.events.clear()
        print(f"--- {label}: move_to ({x},{y})")
        p.send(type="move_to", pos=[x, y], tolerance=0.8, ref=50)
        t0 = time.time()
        while time.time() - t0 < 55:
            p.pump(0.5)
            if any(e.get("code") in ("MOVE_DONE", "MOVE_FAILED") for e in p.events):
                break
        pos, room = p.me()
        print(f"    终态: pos={pos} room={room}")
    p.s.close()


if __name__ == "__main__":
    main()
