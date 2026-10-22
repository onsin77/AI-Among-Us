#!/usr/bin/env python3
"""B 组任务流快速验证:开局→挑任务→open_task→打印 TASK 事件与面板状态。
用法:python task_probe.py [task_name](默认挑第一个可开任务)
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
        self.events, self.acks = [], []
        self.state = {}
        self.ev_cursor = 0

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
                t = m.get("type")
                if t == "event":
                    self.events.append(m)
                    print("   [ev]", m.get("code"), m.get("note"))
                elif t == "ack":
                    self.acks.append(m)
                elif t == "state_full":
                    self.state = m
                elif t == "state_delta":
                    self.state.update(m)

    def cmd(self, **kw):
        ref = int(time.time() * 1000) % 1_000_000
        kw["ref"] = ref
        self.send(**kw)
        t0 = time.time()
        while time.time() - t0 < 4:
            self.pump(0.3)
            for a in self.acks:
                if a.get("ref") == ref:
                    return a
        return None

    def wait_ev(self, code, timeout):
        t0 = time.time()
        while time.time() - t0 < timeout:
            self.pump(0.4)
            while self.ev_cursor < len(self.events):
                e = self.events[self.ev_cursor]
                self.ev_cursor += 1
                if e.get("code") == code:
                    return e
        return None


def main():
    p = P()
    p.send(type="hello")
    p.pump(1)
    if "--skip-launch" not in sys.argv:
        p.cmd(type="debug_start", mode="freeplay", map=4)
        t0 = time.time()
        while time.time() - t0 < 75:
            p.pump(0.5)
            if (p.state.get("me") or {}).get("position"):
                print("PLAYING")
                break
        time.sleep(2)
    want = sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith("-") else None
    tasks = ((p.state.get("me") or {}).get("tasks") or {}).get("list") or []
    todo = [t for t in tasks if not t.get("done")]
    print("tasks:", [(t["id"], t["name"], t.get("max_step")) for t in todo])
    if want:
        todo = [t for t in todo if t["name"] == want] + [t for t in todo if t["name"] != want]
    for cand in todo:
        print(f"== 尝试 {cand['id']} {cand['name']}")
        r = p.cmd(type="open_task", task_id=cand["id"])
        print("  ack:", r)
        if not (r and r.get("ok")):
            continue
        ev = p.wait_ev("TASK_PANEL_OPEN", 100)
        print("  panel:", ev)
        if ev:
            ev2 = p.wait_ev("TASK_FLOW_DONE", 60 + 20 * (cand.get("max_step") or 1))
            print("  flow:", ev2)
            s = ((p.state.get("me") or {}).get("tasks") or {}).get("list") or []
            t2 = [t for t in s if t["id"] == cand["id"]]
            print("  done?", t2[:1])
            break
    p.s.close()


if __name__ == "__main__":
    main()
