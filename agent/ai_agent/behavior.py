"""规则行为引擎(迭代 3):好人自动做任务/报尸;坏人巡逻/击杀,**永不报警**。

纯规则,不接模型;决策动作全部通过已验证的 L1 指令通道执行。
会议内容不在本模块:见 meeting.py(文件驱动 + DeepSeek)。

设计要点:
- 只在 PLAYING 行动;MEETING 期间完全让位(会议发言/投票由 MeetingPipeline 负责)。
- 坏人:击杀冷却就绪 → 找最近可见活人 → 靠近 → kill;**任何情况下不发 report**
  (用户明确要求:刀完人不应报警)。等冷却期间随机巡逻房间,避免呆站。
- 好人:附近有尸体 → 靠近并 report;否则按任务列表逐个 open_task;
  某任务返回 task_no_console / TASK_FLOW_ABORT(too_far 等) → 黑名单 10 分钟,继续下一个。
- 技能自动驾驶:白名单里的无目标增益/防御类职业技能 ready 即用(全角色适配);
  击杀类职业刀统一走 kill 通道(IsKillButton);开会/开面板类技能绝不自动用。
- 失败安全:所有指令异常仅记录日志,不让循环崩溃。
"""

from __future__ import annotations

import asyncio
import logging
import random
import time
from dataclasses import dataclass, field
from typing import Any

from .bridge import BridgeClient
from .world import WorldModel

log = logging.getLogger("ai_agent.behavior")

PATROL_ROOMS = [
    "MainHall", "CargoBay", "Engine", "MedBay", "Security", "Records",
    "Showers", "Kitchen", "Comms", "VaultRoom",
]

# 技能自动驾驶白名单(key=L1 abilities 里的 CustomButton 字段名)。
# 只放"无目标/最近目标、无副作用面板、不改变站位"的高置信技能;
# 会议召集(mayorMeetingButton)、开地图/摄像头面板(hacker*/CamButton)、
# 位移传送(usePortal/moveToPortal)与 UI 按钮(zoomOut/roleSummary)一律不自动用。
AUTO_ABILITIES = {
    # 好人:防御/增益/信息
    "engineerRepairButton",       # 工程师修当前破坏
    "medicShieldButton",          # 医生给最近玩家上盾
    "veteranAlertButton",         # 老兵开警戒
    "survivorVestButton",         # 幸存者穿背心
    "trackerTrackPlayerButton",   # 追踪最近玩家
    "trackerTrackCorpsesButton",  # 追踪尸体
    "bodyGuardGuardButton",       # 保镖守护
    "soulSightButton",            # 看魂
    "defuseButton",               # 拆弹
    "ghostEngineerButton",        # 幽灵工程师
    # 坏人侧:增益
    "lightsOutButton",            # 关灯
    "arsonistButton",             # 纵火狂浇油(需贴目标,CouldUse 自然限制)
    "werewolfRampageButton",      # 狼人狂暴
}
# 自动用技能的节流:同一 key 两次尝试至少间隔(秒),冷却中的 ready 翻转自然限频
ABILITY_RETRY_GAP = 8.0


@dataclass
class BehaviorConfig:
    mode: str = "monitor"          # monitor | crew | impostor | auto
    tick: float = 0.6              # 决策周期(秒)
    kill_range: float = 4.2        # 击杀接近距离上限(实际开火距离按 L1 kill_distance 收紧)
    report_range: float = 5.0      # 报尸触发距离
    task_blacklist_sec: float = 120.0
    patrol: bool = True
    auto_abilities: bool = True    # 技能自动驾驶(白名单见 AUTO_ABILITIES)


@dataclass
class _TaskState:
    blacklist: dict[int, float] = field(default_factory=dict)
    active_id: int | None = None
    started_at: float = 0.0


class BehaviorEngine:
    def __init__(self, world: WorldModel, bridge: BridgeClient, cfg: BehaviorConfig) -> None:
        self.world = world
        self.bridge = bridge
        self.cfg = cfg
        self._stop = asyncio.Event()
        self._ev_seq = 0
        self._tasks = _TaskState()
        self._kills = 0
        self._reports = 0
        self._tasks_done = 0
        self._last_action = "idle"
        self._last_error: str | None = None
        self._last_action_at = 0.0
        self._next_patrol_at = 0.0
        self._pending_kill_target: int | None = None
        self._pending_report = False
        self._mode_effective: str | None = None
        self._last_panel_heal = 0.0
        self._meeting_seen = False
        self._ack_seq = 0
        self._pending: dict[str, dict] = {}
        self._ref_n = 0
        # 会议目标(会议结束后模型思考的下一阶段倾向;仅调整优先级,不改硬规则)
        self._meeting_goals: list[dict[str, Any]] = []
        # 技能自动驾驶:每 key 上次尝试时间(节流)与成功计数
        self._ability_tried_at: dict[str, float] = {}
        self._abilities_used = 0

    def set_meeting_goals(self, goals: list[Any]) -> None:
        """接收会议目标思考结果(action/target/weight),影响击杀目标偏好与巡逻房间。"""
        self._meeting_goals = [g for g in (goals or []) if isinstance(g, dict)][:4]

    def _goal_target(self, action: str) -> str | None:
        for g in self._meeting_goals:
            if str(g.get("action") or "") == action and g.get("target"):
                return str(g["target"])
        return None

    # ---------- 生命周期 ----------

    async def run(self) -> None:
        log.info("behavior engine started (mode=%s)", self.cfg.mode)
        self.world.behavior.update({"mode": self.cfg.mode, "running": True})
        while not self._stop.is_set():
            try:
                await self._tick()
            except Exception as e:  # 不让行为循环因单次异常退出
                self._last_error = f"{type(e).__name__}: {e}"
                log.exception("behavior tick failed")
            await asyncio.sleep(self.cfg.tick)

    async def stop(self) -> None:
        self._stop.set()
        self.world.behavior["running"] = False

    # ---------- 状态 ----------

    def status(self) -> dict[str, Any]:
        return {
            "mode": self.cfg.mode,
            "mode_effective": self._mode_effective,
            "role": self._role(),
            "action": self.world.behavior.get("action", "idle"),
            "last_action": self._last_action,
            "last_action_at": self._last_action_at,
            "last_error": self._last_error,
            "kills": self._kills,
            "reports": self._reports,
            "tasks_done": self._tasks_done,
            "abilities_used": self._abilities_used,
            "auto_abilities": self.cfg.auto_abilities,
            "blacklisted": {str(k): round(v - time.time(), 0) for k, v in self._tasks.blacklist.items()
                            if v > time.time()},
        }

    # ---------- 主循环 ----------

    async def _tick(self) -> None:
        st = self.world.state or {}
        phase = st.get("phase")
        me = st.get("me")

        if phase != "PLAYING" or not me or not me.get("alive"):
            self._set_action("idle")
            if phase == "MEETING":
                self._meeting_seen = True
            elif self._meeting_seen and phase == "PLAYING":
                self._meeting_seen = False
                self._pending_kill_target = None
                self._pending_report = False
                # 散会后重置任务计时:开会的时间不应计入 150s 任务看门狗(否则误拉黑)
                if self._tasks.active_id is not None:
                    self._tasks.started_at = time.time()
            return

        # 开场动画期间角色尚未确定(默认船员),绝不能行动:否则会误开任务面板
        # 并在动画结束后一直卡住(实测:内鬼再也无法移动/击杀)
        if st.get("intro") is not False:
            self._set_action("intro")
            return

        self._consume_events()

        if self.cfg.mode == "monitor":
            self._set_action("idle")
            return

        # 面板自愈:残留任务面板会让 move_to 一直 panel_open(上一版过场误开的遗留)
        if self._last_error and "panel_open" in str(self._last_error):
            if time.time() - self._last_panel_heal > 8.0:
                self._last_panel_heal = time.time()
                await self._send({"type": "minigame_close"})
                self._last_error = None

        role = self._role()
        if role is None:
            self._set_action("wait_role")
            return

        # 技能自动驾驶(好人/坏人通用:白名单里的增益技能 ready 即用)
        await self._ability_tick(me)

        if role == "impostor":
            await self._impostor_tick(st, me)
        else:
            await self._crew_tick(st, me)

    def _role(self) -> str | None:
        mode = self.cfg.mode
        if mode == "monitor":
            return None
        if mode == "crew":
            return "crew"
        if mode == "impostor":
            return "impostor"
        # auto:按状态判定
        me = (self.world.state or {}).get("me") or {}
        team = (me.get("team") or "").lower()
        if team.startswith("impostor") or me.get("role") in ("Impostor", "Shapeshifter"):
            return "impostor"
        if team:
            return "crew"
        return None

    # ---------- 技能自动驾驶 ----------

    async def _ability_tick(self, me: dict) -> None:
        """白名单技能 ready 即用:全职业适配的落地形式。

        L1 的 abilities 列出当前角色所有可用 CustomButton(感知层天然全职业);
        kill 类按钮(IsKillButton)走 kill 通道,不在这里;开会/开面板类不在白名单。
        """
        if not self.cfg.auto_abilities:
            return
        abilities = me.get("abilities") or []
        if not abilities:
            return
        now = time.time()
        for a in abilities:
            key = a.get("key")
            if key not in AUTO_ABILITIES or not a.get("ready"):
                continue
            if now - self._ability_tried_at.get(key, 0.0) < ABILITY_RETRY_GAP:
                continue
            self._ability_tried_at[key] = now
            ok = await self._send({"type": "use_ability", "key": key})
            if ok:
                self._abilities_used += 1
                self._last_action = f"use_ability {key}"
                self._last_action_at = now
                log.info("auto ability used: %s", key)
            # 失败(cooldown_or_not_usable 等)由节流兜底,不刷 last_error

    def _kill_range_now(self, me: dict) -> float:
        """实际开火距离:L1 按房间 KillDistance 设置校验(1/1.8/2.5+余量),贴身再杀。"""
        kd = me.get("kill_distance")
        if isinstance(kd, (int, float)) and kd > 0:
            return min(self.cfg.kill_range, float(kd) + 0.4)
        return self.cfg.kill_range

    # ---------- 坏人 ----------

    async def _impostor_tick(self, st: dict, me: dict) -> None:
        self._mode_effective = "impostor"
        # 硬规则:坏人永不 report(刀完人更不报)
        if self._pending_report:
            self._pending_report = False
            self._last_action = "impostor_skip_report"

        kill_range = self._kill_range_now(me)
        cd = me.get("kill_cooldown")
        if cd is not None and cd > 0.3:
            # 冷却期:附近有目标就贴着等待(下一步抬手就杀),没目标才巡逻
            near = self._nearest_visible_target(st, me, kill_range + 2.5)
            if near is not None:
                pos = near.get("position")
                d = near.get("distance")
                if pos and d is not None and d > kill_range:
                    await self._send({"type": "move_to", "pos": [pos["x"], pos["y"]], "tolerance": 1.0})
                self._set_action(f"kill_cd {cd:.1f}s stalk {near.get('name')}")
                return
            self._set_action(f"kill_cd {cd:.1f}s")
            await self._patrol_if_due()
            return

        target = self._nearest_visible_target(st, me, kill_range + 0.6)
        if target is None:
            self._set_action("hunt")
            await self._patrol_if_due()
            return
        dist = target.get("distance")
        if dist is not None and dist <= kill_range:
            ok = await self._send({"type": "kill", "target_id": target["id"]})
            if ok:
                self._last_action = f"kill {target.get('name')}({target.get('id')})"
                self._last_action_at = time.time()
                self._set_action("killed")
            else:
                self._last_error = "kill send failed"
            return

        pos = target.get("position")
        if pos:
            self._set_action(f"approach {target.get('name')}")
            await self._send({"type": "move_to", "pos": [pos["x"], pos["y"]], "tolerance": 1.0})

    def _nearest_visible_target(self, st: dict, me: dict, max_dist: float) -> dict | None:
        best = None
        best_d = 1e9
        my_id = me.get("player_id")
        kill_goal = self._goal_target("kill")   # 会议目标:优先击杀对象
        for p in st.get("players") or []:
            if not p.get("alive") or p.get("id") == my_id:
                continue
            if not p.get("visible_to_me") or p.get("distance") is None:
                continue
            if kill_goal and str(p.get("name")) == kill_goal:
                return p
            d = p["distance"]
            if d <= max_dist and d < best_d:
                best, best_d = p, d
        return best

    async def _patrol_if_due(self) -> None:
        if not self.cfg.patrol:
            return
        now = time.time()
        if now < self._next_patrol_at:
            return
        self._next_patrol_at = now + 8.0
        rooms = [r for r in ((self.world.state or {}).get("rooms") or []) if isinstance(r, str)]
        pool = rooms or PATROL_ROOMS
        # 只挑本图存在的房间,避免 not_found:room_on_map
        guard_room = self._goal_target("guard")
        room = guard_room if guard_room in pool else random.choice(pool)
        self._last_action = f"patrol {room}"
        await self._send({"type": "move_to", "room": room})

    # ---------- 好人 ----------

    async def _crew_tick(self, st: dict, me: dict) -> None:
        self._mode_effective = "crew"
        # 1) 报尸优先(可见尸体列表已由 L1 做距离/LOS 过滤)
        bodies = [b for b in (st.get("nearby_bodies") or []) if not b.get("reported")]
        if bodies:
            # 注意 distance==0.0 是合法值(贴脸),不能用 or 兜底(0 会被当 falsy 排到最后)
            b = min(bodies, key=lambda x: 99.0 if x.get("distance") is None else x["distance"])
            d = b.get("distance")
            if d is not None and d <= self.cfg.report_range + 0.2:
                ok = await self._send({"type": "report"})
                if ok:
                    self._reports += 1
                    self._last_action = f"report {b.get('victim')}"
                    self._last_action_at = time.time()
                    self._set_action("reported")
                    self._tasks.active_id = None
                    return
            pos = b.get("position")
            if pos and d is not None and d <= self.cfg.report_range:
                self._set_action(f"approach body {b.get('victim')}")
                self._last_action = f"move_to body {b.get('victim')}"
                self._last_action_at = time.time()
                await self._send({"type": "move_to", "pos": [pos["x"], pos["y"]], "tolerance": 1.2})
                return

        # 2) 任务流
        self._consume_events()
        now = time.time()
        # watchdog:任务开始 150s 无完成事件 → 拉黑换下一个(必须在 active 判定之前,否则永不触发)
        if self._tasks.active_id is not None and now - self._tasks.started_at > 150.0:
            tid = self._tasks.active_id
            self._tasks.blacklist[tid] = now + 60.0
            self._tasks.active_id = None
            self._last_error = f"task {tid} watchdog (no events 150s)"
            log.warning("task %s watchdog cleared", tid)
        if self._tasks.active_id is not None:
            self._set_action(f"task {self._tasks.active_id}")
            return

        tasks = ((me.get("tasks") or {}).get("list") or [])
        for t in tasks:
            if t.get("done"):
                continue
            tid = t.get("id")
            if tid in self._tasks.blacklist and now < self._tasks.blacklist[tid]:
                continue
            ok = await self._send({"type": "open_task", "task_id": tid})
            if ok:
                self._tasks.active_id = tid
                self._tasks.started_at = now
                self._last_action = f"open_task {tid}({t.get('name')})"
                self._last_action_at = now
                self._set_action(f"walk_task {t.get('name')}")
                return
        # 所有任务都完成/被拉黑:巡逻避免呆站(会议外自主性)
        self._set_action("no_task")
        await self._patrol_if_due()

    def _consume_events(self) -> None:
        for ev in list(self.world.events):
            seq = ev.get("_seq", 0)
            if seq <= self._ev_seq:
                continue
            self._ev_seq = seq
            code = ev.get("code")
            note = ev.get("note") or ""
            if code == "TASK_FLOW_DONE":
                self._tasks_done += 1
                self._tasks.active_id = None
            elif code == "TASK_FLOW_ABORT" and self._tasks.active_id is not None:
                tid = self._tasks.active_id
                # 无站位/无台:基本不可做,长时间拉黑;卡死/超距:短拉黑稍后重试
                dur = 900.0 if ("no_approach" in note or "task_no_console" in note) else self.cfg.task_blacklist_sec
                self._tasks.blacklist[tid] = time.time() + dur
                self._tasks.active_id = None
                self._last_error = f"task {tid} abort: {note}"
                log.info("task %s blacklisted: %s", tid, note)
            elif code == "KILL_CMD":
                self._kills += 1
                self._last_action = "kill confirmed"

        # ack 处理(只认引擎自己发出的 ref)。world.acks 是"最新在前"的 deque,
        # 必须按 seq 升序处理,否则较旧的未处理 ack 会被新 ack 的 seq 永久跳过(实测卡死)
        pending_acks = [a for a in list(self.world.acks) if a.get("_seq", 0) > self._ack_seq]
        pending_acks.sort(key=lambda a: a.get("_seq", 0))
        for a in pending_acks:
            self._ack_seq = max(self._ack_seq, a.get("_seq", 0))
            ref = str(a.get("ref"))
            pend = self._pending.pop(ref, None)
            if pend is None:
                continue
            if a.get("ok"):
                continue
            err = a.get("err") or "?"
            cmd = pend.get("type")
            if cmd == "open_task":
                tid = pend.get("task_id")
                if err == "busy":
                    # 上一轮任务还在跑:稍后重试,不拉黑
                    self._tasks.active_id = None
                elif tid is not None:
                    dur = 900.0 if "no_approach" in err else self.cfg.task_blacklist_sec
                    self._tasks.blacklist[tid] = time.time() + dur
                    self._tasks.active_id = None
                    self._last_error = f"task {tid}: {err}"
            elif cmd in ("kill", "report", "move_to"):
                self._last_error = f"{cmd}: {err}"

    # ---------- 工具 ----------

    def _set_action(self, action: str) -> None:
        self.world.behavior["action"] = action
        self.world.behavior["role"] = self._role()
        self.world.behavior["last"] = self._last_action

    async def _send(self, cmd: dict) -> bool:
        self._ref_n += 1
        ref = f"beh{self._ref_n}"
        payload = dict(cmd)
        payload["ref"] = ref
        ok = await self.bridge.send(payload)
        if ok:
            self._pending[ref] = payload
            self._pending = dict(list(self._pending.items())[-32:])
            self.world.acks.appendleft({"type": "cmd_sent", "cmd": cmd.get("type"), "ref": ref, "t": time.time()})
        else:
            self._last_error = f"bridge offline: {cmd.get('type')}"
        return ok
