#!/usr/bin/env python3
"""导航诊断:打印栅格窗口并叠加 A* 路径(nav_path 的 waypoints)。
用法:python nav_diag.py TO_X TO_Y [半径u] [--window CX CY R]
  TO_X TO_Y   规划目标(nav_path 的 to)
  半径u       显示窗口半径(默认 6)
  --window    改用中心点+半径显示(默认以玩家位置为中心)
字符:#=不可走 o=可达 .=可走但不可达 *=路径点 P=玩家 G=goal
"""
import json
import socket
import sys
import time

HOST, PORT = "127.0.0.1", 47600


class C:
    def __init__(self):
        self.s = socket.create_connection((HOST, PORT), timeout=5)
        self.s.settimeout(0.5)
        self.buf = b""
        self.state = {}

    def send(self, **cmd):
        self.s.sendall((json.dumps(cmd) + "\n").encode())

    def pump(self, sec, want=None):
        out = []
        t0 = time.time()
        while time.time() - t0 < sec:
            try:
                d = self.s.recv(65536)
            except socket.timeout:
                continue
            if not d:
                break
            self.buf += d
            while b"\n" in self.buf:
                line, self.buf = self.buf.split(b"\n", 1)
                try:
                    m = json.loads(line)
                except Exception:
                    continue
                t = m.get("type")
                if t == "state_full":
                    self.state = m
                elif t == "state_delta":
                    self.state.update(m)
                if want and t == want:
                    out.append(m)
        return out

    def get(self, want, send_cmd, timeout=8, **kw):
        self.send(ref=int(time.time() * 1000) % 100000, **send_cmd, **kw)
        t0 = time.time()
        while time.time() - t0 < timeout:
            got = self.pump(0.5, want)
            if got:
                return got[-1]
        return None


def main():
    argv = sys.argv[1:]
    if len(argv) < 2:
        print(__doc__)
        return
    tx, ty = float(argv[0]), float(argv[1])
    radius = float(argv[2]) if len(argv) > 2 else 6.0
    cx = cy = None
    if "--window" in argv:
        i = argv.index("--window")
        cx, cy = float(argv[i + 1]), float(argv[i + 2])

    c = C()
    c.send(type="hello")
    c.pump(1)
    me = (c.state.get("me") or {}).get("position") or {"x": 0, "y": 0}
    print(f"player: {me}  target: ({tx},{ty})")

    dump = c.get("nav_dump", {"type": "nav_dump"})
    path = c.get("nav_path", {"type": "nav_path", "to": [tx, ty]})
    if not dump:
        print("no dump")
        return
    rows, gw, gh = dump["rows"], dump["gw"], dump["gh"]
    mn_x, mn_y = dump["min_x"], dump["min_y"]
    STEP = 0.25

    wp = path.get("wp") or [] if path else []
    wps = [(wp[i], wp[i + 1]) for i in range(0, len(wp) - 1, 2)] if wp else []
    print(f"nav_path: ok={path.get('ok')} goal={path.get('goal_pos')} wp={len(wps)}")
    if path and path.get("crossings"):
        print("crossings:", path["crossings"])

    if cx is None:
        xs = [p[0] for p in wps] or [me["x"]]
        ys = [p[1] for p in wps] or [me["y"]]
        cx = (min(xs) + max(xs)) / 2
        cy = (min(ys) + max(ys)) / 2
        radius = max(radius, (max(xs) - min(xs)) / 2 + 1, (max(ys) - min(ys)) / 2 + 1)

    def cell(x, y):
        gx = round((x - mn_x) / STEP)
        gy = round((y - mn_y) / STEP)
        return gx, gy

    px, py = cell(me["x"], me["y"])
    gxs, gys = cell(cx, cy)
    rx = int(radius / STEP)
    x0, x1 = max(0, gxs - rx), min(gw - 1, gxs + rx)
    y0, y1 = max(0, gys - rx), min(gh - 1, gys + rx)

    marks = {}
    for (wx, wy) in wps:
        gx, gy = cell(wx, wy)
        marks[(gx, gy)] = "*"
    if path and path.get("goal_pos"):
        gx, gy = cell(path["goal_pos"][0], path["goal_pos"][1])
        marks[(gx, gy)] = "G"
    marks[(px, py)] = "P"
    # 目标点(即使不可达)也标个大写 T 的近似位置
    gx, gy = cell(tx, ty)
    if (gx, gy) not in marks:
        marks[(gx, gy)] = "T"

    print(f"window x={mn_x + x0 * STEP:.1f}..{mn_x + x1 * STEP:.1f} y={mn_y + y0 * STEP:.1f}..{mn_y + y1 * STEP:.1f}")
    # x 轴标尺:每 1u 标一次整数(两位数取末位)
    hdr = "      "
    for gx in range(x0, x1 + 1):
        wx = mn_x + gx * STEP
        if abs(wx - round(wx)) < 0.01:
            hdr += str(int(round(wx)) % 10)
        else:
            hdr += " "
    print(hdr)
    for gy in range(y1, y0 - 1, -1):
        line = []
        for gx in range(x0, x1 + 1):
            ch = rows[gy][gx]
            if (gx, gy) in marks:
                ch = marks[(gx, gy)]
            line.append(ch)
        print(f"{mn_y + gy * STEP:6.1f} " + "".join(line))


if __name__ == "__main__":
    main()
