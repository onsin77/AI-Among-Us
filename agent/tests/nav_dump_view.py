#!/usr/bin/env python3
"""导航栅格诊断:向桥发 nav_dump,打印指定世界坐标窗口的 ASCII 位图。
用法:python nav_dump_view.py [x_min y_min x_max y_max](默认打印 GapRoom 一带)
字符:#=不可走 o=可达 .=可走但当前不可达
"""
import json
import socket
import sys

HOST, PORT = "127.0.0.1", 47600


def get_dump():
    s = socket.create_connection((HOST, PORT), timeout=5)
    s.sendall(b'{"type":"nav_dump","ref":1}\n')
    s.settimeout(0.6)
    buf = b""
    try:
        for _ in range(200):  # 最多收 200 块,防状态流灌死
            try:
                d = s.recv(65536)
            except socket.timeout:
                break
            if not d:
                break
            buf += d
            if b'"type":"nav_dump"' in buf:
                for line in buf.split(b"\n"):
                    if b'"type":"nav_dump"' in line:
                        try:
                            return json.loads(line)
                        except json.JSONDecodeError:
                            pass
    finally:
        s.close()
    for line in buf.split(b"\n"):
        if b'"type":"nav_dump"' in line:
            try:
                return json.loads(line)
            except json.JSONDecodeError:
                pass
    return None


def main():
    msg = get_dump()
    if not msg:
        print("no dump received")
        sys.exit(1)
    rows = msg["rows"]
    gw, gh = msg["gw"], msg["gh"]
    mn_x, mn_y = msg["min_x"], msg["min_y"]
    print(f"grid {gw}x{gh} min=({mn_x},{mn_y})")
    print("links:", json.dumps(msg.get("links")))

    if len(sys.argv) == 5:
        wx0, wy0, wx1, wy1 = map(float, sys.argv[1:5])
    else:
        wx0, wy0, wx1, wy1 = -1.0, 4.0, 13.0, 16.0

    def cx(x):
        return max(0, min(gw - 1, round((x - mn_x) / 0.5)))

    def cy(y):
        return max(0, min(gh - 1, round((y - mn_y) / 0.5)))

    x0, x1 = cx(wx0), cx(wx1)
    gy_top, gy_bot = cy(wy1), cy(wy0)  # 世界 y 大 = 行号大(上方)
    # 列标尺:每格=0.5 世界单位,偶数格标世界 x 整数位
    hdr1 = "     " + "".join(f"{int(mn_x + gx * 0.5):>1d}"[-1] if (mn_x + gx * 0.5) % 1 == 0 and int(mn_x + gx * 0.5) % 2 == 0 else " " for gx in range(x0, x1 + 1))
    hdr2 = "     " + "".join(":" if (mn_x + gx * 0.5) % 1 == 0 else " " for gx in range(x0, x1 + 1))
    print(hdr1)
    print(hdr2)
    for gy in range(gy_top, gy_bot - 1, -1):
        line = "".join(rows[gy][gx] for gx in range(x0, x1 + 1))
        print(f"{mn_y + gy * 0.5:5.1f} {line}")


if __name__ == "__main__":
    main()
