#!/usr/bin/env python3
"""迭代 4 L1 验证:会议 speaking_order(发言顺序)字段。
freeplay 开局(飞艇)→ 设内鬼 → 杀最近假人 → report 进会议 → 校验:
1) speaking_order 非空且与 order 同集合(存活者);
2) reporter 在 speaking_order 首位;
3) speaking_order 与 order 的差异 = 报警人提前(布局顺序的旋转);
4) 死亡者不在 speaking_order。
用法:python tests/test_speaking_order.py(需游戏在主菜单,Steam 在线)
"""
import json
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from test_iter2 import Bridge, check, wait_phase  # noqa: E402


def main():
    br = Bridge()
    print("== 开局 freeplay Airship ==")
    br.cmd(type="debug_start", mode="freeplay", map=4, timeout=8)
    s = wait_phase(br, "PLAYING", 40)
    check("进入 PLAYING", s is not None, f"phase={s and s.get('phase')}")
    if s is None:
        return 1
    br.cmd(type="debug_set_role", role="Impostor", timeout=5)
    time.sleep(1.5)
    br.pump(1)

    # 击杀最近的存活假人(连发直至确认)
    victim = None
    for _ in range(30):
        br.cmd(type="debug_players", timeout=3)
        br.pump(0.6)
        dps = br.drain("debug_players")
        if dps:
            me_id = ((br.state() or {}).get("me") or {}).get("player_id")
            players = dps[-1].get("players") or []
            # freeplay 假人可能站界外 (-25,39.64),只选真实站位
            alive = [p for p in players if p.get("alive") and p.get("id") != me_id
                     and abs(p["pos"][0]) < 20]
            if alive:
                victim = alive[0]
                break
    if victim is None:
        check("找到可杀假人", False)
        return 1
    print(f"  目标假人 id={victim['id']} pos={victim['pos']}")
    br.cmd(type="move_to", pos=victim["pos"], tolerance=1.0, timeout=5)
    br.pump(4)
    # 等击杀冷却(开局 killTimer 可能高达 25s)
    for _ in range(50):
        s = br.state()
        cd = ((s or {}).get("me") or {}).get("kill_cooldown")
        if cd is None or cd <= 0.2:
            break
        br.pump(1.0)
    cd = ((br.state() or {}).get("me") or {}).get("kill_cooldown")
    print(f"  击杀冷却就绪 cd={cd}")
    killed = False
    last_err = None
    for _ in range(20):
        r = br.cmd(type="kill", target_id=victim["id"])
        if r and r.get("ok"):
            killed = True
            break
        last_err = r and r.get("err")
        br.pump(0.8)
    check("击杀假人", killed, str(last_err))
    if not killed:
        return 1

    r = br.cmd(type="report")
    check("report", r is not None and r.get("ok"), str(r))
    s = wait_phase(br, "MEETING", 25)
    check("进入 MEETING", s is not None)
    if s is None:
        return 1
    br.pump(2)
    mtg = (br.state() or {}).get("meeting") or {}
    order = mtg.get("order") or []
    spo = mtg.get("speaking_order") or []
    reporter = mtg.get("reporter")
    print(f"  order={order}")
    print(f"  speaking_order={spo}")
    print(f"  reporter={reporter}")
    check("speaking_order 非空", len(spo) > 0, str(spo))
    check("reporter 在首位", bool(spo) and spo[0] == reporter,
          f"spo[0]={spo[:1]} reporter={reporter}")
    alive_order = [p for p in order if p != victim["id"]]   # 我们杀的那个人是死的
    check("集合一致(存活者)", sorted(spo) == sorted(alive_order),
          f"spo={sorted(spo)} alive_order={sorted(alive_order)}")
    # 布局顺序 = 存活者按 id 升序,旋转到 reporter 首位
    if reporter in alive_order:
        rotated = [reporter] + [p for p in alive_order if p != reporter]
        check("布局旋转正确", spo == rotated, f"spo={spo} 期望={rotated}")
    # 死人不在
    check("死亡者不在 speaking_order", victim["id"] not in spo)

    # 收尾:快进散会
    br.cmd(type="debug_meeting_ff")
    br.pump(3)
    wait_phase(br, "PLAYING", 20)
    print("\nspeaking_order 验证完成")
    return 0


if __name__ == "__main__":
    fails = main()
    raise SystemExit(1 if fails else 0)
