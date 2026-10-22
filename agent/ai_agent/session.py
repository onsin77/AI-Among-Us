"""整局托管(SessionManager):让 AI 自己开房、加 bot、开局、连续打完一局又一局。

支持三种托管形态:
- local   :本地服务器开房(127.0.0.1:22023,同机/局域网可直连),可加 DummyBehaviour bot,
            无真人也能整局跑通(推荐自动化验收)。
- online  :真在线开房(匹配服务器分配),好友用房间码/好友列表加入;人满(或手动)开局。
- freeplay:练习模式(假人),无胜负结算;用 max_meetings 截断后重开。

流程(阶段驱动,幂等,带冷却与重试):
    MENU  --host_game--> LOBBY --(加 bot / 设选项)--> start_game --> PLAYING
    PLAYING <-> MEETING ... --> ENDED --next_game--> LOBBY --start_game--> ...
每局结束按 max_games 决定是否继续;freeplay 按 max_meetings 结束当前练习局后重开。
"""

from __future__ import annotations

import asyncio
import logging
import time
from dataclasses import dataclass
from typing import Any

from .bridge import BridgeClient
from .world import WorldModel

log = logging.getLogger("ai_agent.session")


@dataclass
class SessionConfig:
    enabled: bool = False
    host_kind: str = "local"        # local | online | freeplay
    map_id: int = 4                 # 4=Airship
    bots: int = 0                   # 大厅里补的 bot 数(0=不补)
    min_players: int = 4            # 大厅人数达到即开局
    auto_start: bool = True
    auto_next: bool = True
    max_games: int = 0              # 0=无限
    max_meetings: int = 0           # freeplay/无结算时,会议数达到后换局(0=不限)
    num_impostors: int = 1
    discussion_time: int = 10
    voting_time: int = 25
    kill_cooldown: float = 8.0      # 自托管默认加速:内鬼 8s 一刀
    kill_distance: int = 2          # 0=短 1=中 2=长
    player_speed: float = 1.2
    start_delay: float = 2.5        # 进大厅后等多久再开局
    next_delay: float = 7.0         # 结算后等多久回大厅
    meeting_watchdog: float = 150.0
    host_timeout: float = 25.0
    bot_turn_timeout: float = 7.0   # bot 局语音轮麦等待(真人局用 config 默认)
    bot_turn_gap: float = 1.5
    bot_meetings: bool = True       # bot 局自动触发会议(假人不会报警/开会)
    bot_meeting_interval: float = 180.0
    takeover_mode: str = "auto"     # 开局后自动接管行为引擎


class SessionManager:
    def __init__(self, world: WorldModel, bridge: BridgeClient, cfg: SessionConfig,
                 voice=None, controller=None) -> None:
        self.world = world
        self.bridge = bridge
        self.cfg = cfg
        self.voice = voice
        self.controller = controller
        self._stop = asyncio.Event()
        self._tick_n = 0
        self._phase: str | None = None
        self._phase_since = time.time()
        self._host_sent_at = 0.0
        self._host_tries = 0
        self._spawn_sent_at = 0.0
        self._spawn_tries = 0
        self._start_sent_at = 0.0
        self._start_tries = 0
        self._next_sent_at = 0.0
        self._next_sent = False
        self._meeting_started_at = 0.0
        self._meeting_ff_sent = False
        self._last_kill_seen = 0
        self._last_meeting_called_at = 0.0
        self._playing_since = time.time()
        self._options_sent = False
        self._last_gather_at = 0.0
        self._bots_spawned = False
        self._saw_bots = False
        self._end_sent = False
        self._leave_sent_at = 0.0
        self.games_started = 0
        self.games_finished = 0
        self.meetings_total = 0
        self.last_action = "idle"
        self.last_error: str | None = None
        self.started_at: float | None = None

    # ---------- 生命周期 ----------

    async def run(self) -> None:
        log.info("session manager started")
        while not self._stop.is_set():
            try:
                await self._tick()
            except Exception as e:
                self.last_error = f"{type(e).__name__}: {e}"
                log.exception("session tick failed")
            await asyncio.sleep(0.5)

    async def stop(self) -> None:
        self._stop.set()
        self._restore_voice_timing()

    # ---------- 对外控制 ----------

    async def start(self, body: dict[str, Any] | None = None) -> dict[str, Any]:
        body = body or {}
        self.cfg.enabled = True
        self.cfg.host_kind = str(body.get("host_kind") or self.cfg.host_kind)
        if self.cfg.host_kind not in ("local", "online", "freeplay"):
            self.cfg.host_kind = "local"
        self.cfg.bots = int(body.get("bots", self.cfg.bots) or 0)
        self.cfg.min_players = int(body.get("min_players", self.cfg.min_players) or 4)
        self.cfg.max_games = int(body.get("max_games", self.cfg.max_games) or 0)
        self.cfg.auto_next = bool(body.get("auto_next", self.cfg.auto_next))
        self.cfg.auto_start = bool(body.get("auto_start", self.cfg.auto_start))
        if "map" in body:
            self.cfg.map_id = int(body["map"])
        self.started_at = time.time()
        self._host_tries = 0
        self._spawn_tries = 0
        self._start_tries = 0
        self._next_sent = False
        self.last_action = f"session start ({self.cfg.host_kind}, bots={self.cfg.bots})"
        log.info("session start: kind=%s bots=%s min=%s max_games=%s",
                 self.cfg.host_kind, self.cfg.bots, self.cfg.min_players, self.cfg.max_games)
        # 无人操作游戏:顺手开启行为接管(若 controller 可用)
        if self.controller is not None and not getattr(self.controller, "takeover", False):
            try:
                await self.controller.start(mode=self.cfg.takeover_mode, autostart=False)
            except Exception as e:
                self.last_error = f"takeover: {e}"
        if self.cfg.bots > 0:
            self._apply_voice_bot_timing()
        return self.status()

    async def control(self, body: dict[str, Any]) -> dict[str, Any]:
        action = str(body.get("action") or "")
        if action == "start":
            return await self.start(body)
        if action == "stop":
            self.cfg.enabled = False
            self.last_action = "session stop"
            self._restore_voice_timing()
            return {"ok": True, "status": self.status()}
        if action == "start_now":
            await self._send({"type": "start_game", "min_players": 1})
            return {"ok": True}
        if action == "next_game":
            await self._send({"type": "next_game"})
            return {"ok": True}
        if action == "leave_game":
            await self._send({"type": "leave_game"})
            return {"ok": True}
        if action == "spawn_bots":
            n = int(body.get("count", self.cfg.bots or 3))
            await self._send({"type": "spawn_bots", "count": n})
            return {"ok": True}
        if action == "set_options":
            await self._send_options()
            return {"ok": True}
        return {"ok": False, "error": f"未知 action:{action}"}

    def status(self) -> dict[str, Any]:
        st = self.world.state or {}
        lobby = st.get("lobby") or {}
        return {
            "enabled": self.cfg.enabled,
            "host_kind": self.cfg.host_kind,
            "bots": self.cfg.bots,
            "min_players": self.cfg.min_players,
            "auto_start": self.cfg.auto_start,
            "auto_next": self.cfg.auto_next,
            "max_games": self.cfg.max_games,
            "phase": st.get("phase"),
            "phase_sec": round(time.time() - self._phase_since, 1),
            "round": st.get("round"),
            "lobby": lobby,
            "game_code": lobby.get("code") or "",
            "players": lobby.get("players") or 0,
            "am_host": lobby.get("am_host"),
            "games_started": self.games_started,
            "games_finished": self.games_finished,
            "meetings_total": self.meetings_total,
            "uptime": round(time.time() - self.started_at, 0) if self.started_at else 0,
            "last_action": self.last_action,
            "last_error": self.last_error,
        }

    # ---------- 阶段机 ----------

    async def _tick(self) -> None:
        self._tick_n += 1
        st = self.world.state or {}
        phase = st.get("phase")
        if phase != self._phase:
            self._on_phase(self._phase, phase)
            self._phase = phase
            self._phase_since = time.time()
        if not self.cfg.enabled:
            return
        if phase == "MENU":
            await self._menu_tick()
        elif phase == "LOBBY":
            await self._lobby_tick()
        elif phase == "PLAYING":
            await self._playing_tick()
        elif phase == "MEETING":
            await self._meeting_tick()
        elif phase == "ENDED":
            await self._ended_tick()

    def _on_phase(self, prev: str | None, cur: str | None) -> None:
        now = time.time()
        if cur == "LOBBY" and prev in ("MENU", "ENDED", None):
            self._options_sent = False
            self._bots_spawned = False
            self._end_sent = False
            self._spawn_tries = 0
            self._spawn_sent_at = 0.0
            self._start_tries = 0
            self._start_sent_at = 0.0
            self._next_sent = False
        if cur == "PLAYING" and prev == "MEETING" and self.cfg.bots > 0:
            asyncio.ensure_future(self._send({"type": "debug_gather_bots"}))
        if cur == "PLAYING" and prev != "PLAYING":
            self._saw_bots = False
        if cur == "PLAYING" and prev == "LOBBY":
            self.games_started += 1
            self._playing_since = now
            self._end_sent = False
            self._last_meeting_called_at = now          # 开局至少 12s 内不主动开会(避开开场动画)
            self._last_kill_seen = max([ev.get("_seq", 0) for ev in list(self.world.events)[-30:]] or [0])
            self.last_action = f"game #{self.games_started} started"
            log.info("session: game #%d started", self.games_started)
        if cur == "MEETING" and prev == "PLAYING":
            self.meetings_total += 1
            self._meeting_started_at = now
            self._meeting_ff_sent = False
            self._last_meeting_called_at = now      # 兜底计时从这次会议开始算,避免刚散会又开会
        if cur == "ENDED" and prev in ("MEETING", "PLAYING"):
            self.games_finished += 1
            self.last_action = f"game #{self.games_started} ended"
            log.info("session: game #%d ended (finished=%d)", self.games_started, self.games_finished)

    async def _menu_tick(self) -> None:
        if self.cfg.host_kind == "freeplay":
            if self.cfg.max_games and self.games_started >= self.cfg.max_games:
                if not self._next_sent:
                    self._next_sent = True
                    self.last_action = "max_games reached, stay in menu"
                    self.cfg.enabled = False
                    self._restore_voice_timing()
                return
            if time.time() - self._host_sent_at < self.cfg.host_timeout:
                return
            self._host_sent_at = time.time()
            await self._send({"type": "debug_start", "mode": "freeplay", "map": self.cfg.map_id})
            self.last_action = "debug_start freeplay"
            return
        if self.cfg.max_games and self.games_finished >= self.cfg.max_games:
            if not self._next_sent:
                self._next_sent = True
                self.last_action = f"max_games({self.cfg.max_games}) reached, stop hosting"
                self.cfg.enabled = False
                self._restore_voice_timing()
            return
        if time.time() - self._host_sent_at < self.cfg.host_timeout:
            return
        self._host_sent_at = time.time()
        self._host_tries += 1
        if self._host_tries > 8:
            self.last_error = "host retry limit"
            self.cfg.enabled = False
            return
        await self._send({"type": "host_game", "mode": self.cfg.host_kind, "map": self.cfg.map_id})
        self.last_action = f"host_game {self.cfg.host_kind}"
        log.info("session: host_game(%s) try %d", self.cfg.host_kind, self._host_tries)

    async def _lobby_tick(self) -> None:
        lobby = (self.world.state or {}).get("lobby") or {}
        players = int(lobby.get("players") or 0)
        elapsed = time.time() - self._phase_since

        # 0) 先落一次开局参数(内鬼数/讨论/投票/刀速)。开局前必须确保,否则可能 0 内鬼直接判负
        if not self._options_sent:
            await self._send_options()
            self._options_sent = True
            self.last_action = "set_game_options"
            return

        # 1) 自动开局
        #    注意:bot 不能在大厅预生成(开场动画会卡死),所以 bot 模式下大厅门槛按 1 人算,
        #    开局后再补 bot。
        effective_min = 1 if self.cfg.bots > 0 else self.cfg.min_players
        if not self.cfg.auto_start:
            return
        if players < effective_min:
            self.last_action = f"wait players {players}/{effective_min}"
            return
        if elapsed < self.cfg.start_delay:
            return
        if self._start_tries >= 6:
            return
        if time.time() - self._start_sent_at < 4.0:
            return
        self._start_sent_at = time.time()
        self._start_tries += 1
        await self._send({"type": "start_game", "min_players": effective_min})
        self.last_action = f"start_game (players={players})"
        log.info("session: start_game players=%d try=%d", players, self._start_tries)

    async def _playing_tick(self) -> None:
        await self._bot_spawn_tick()
        await self._bot_gather_tick()
        await self._bot_end_tick()
        await self._bot_meeting_tick()
        # 会议数截断(主要给 freeplay):到点退出当前局,重新开
        if self.cfg.max_meetings and self.meetings_total >= self.cfg.max_meetings:
            if time.time() - self._leave_sent_at > 6.0:
                self._leave_sent_at = time.time()
                asyncio.ensure_future(self._send({"type": "leave_game"}))
                self.last_action = "max_meetings reached, leave game"
                self.meetings_total = 0
                self._host_sent_at = 0.0
        self.last_action = self.last_action if self.last_action.startswith("game") else "playing"

    async def _bot_spawn_tick(self) -> None:
        """bot 局:开局(过场结束)后再补 bot,避免开场动画卡死。"""
        if self.cfg.bots <= 0:
            return
        st = self.world.state or {}
        now = time.time()
        # 进 PLAYING 后至少等 12s(过场动画期间 state 的 intro 可能还没翻转,直接补 bot 会卡死过场)
        if now - self._playing_since < 12.0:
            return
        if st.get("intro") is not False:
            return
        lobby = st.get("lobby") or {}
        players = int(lobby.get("players") or 0)
        target = 1 + self.cfg.bots
        if players >= target:
            self._saw_bots = True
            return
        if now - self._spawn_sent_at < 5.0 or self._spawn_tries >= 6:
            return
        self._spawn_sent_at = now
        self._spawn_tries += 1
        ok = await self._send({"type": "spawn_bots", "count": target - players})
        self._bots_spawned = self._bots_spawned or ok
        self.last_action = f"spawn_bots {target - players} (have {players})"

    async def _bot_gather_tick(self) -> None:
        """bot 局:定期把 dummy 聚到 AI 身边。假人不会移动/不会找人,否则 AI 会一直找不到目标。"""
        if not (self.cfg.bots > 0 and self._bots_spawned):
            return
        st = self.world.state or {}
        if st.get("intro") is not False:
            return
        now = time.time()
        if now - self._last_gather_at < 10.0:
            return
        alive_others = [p for p in (st.get("players") or []) if p.get("alive")]
        if not alive_others:
            return
        self._last_gather_at = now
        await self._send({"type": "debug_gather_bots"})

    async def _bot_end_tick(self) -> None:
        """bot 局:只剩 1 个对手(1v1 平局)时 mod 不会自然结算,由托管层强制结算。"""
        if not (self.cfg.bots > 0 and self._bots_spawned and self._saw_bots and not self._end_sent):
            return
        st = self.world.state or {}
        if st.get("intro") is not False:
            return
        if time.time() - self._phase_since < 5.0:
            return
        alive_others = [p for p in (st.get("players") or []) if p.get("alive")]
        if len(alive_others) > 1:
            return
        # 0 个对手(全灭)或 1v1 都算分出胜负;mod 不认 dummy,必须托管层结算
        self._end_sent = True
        await self._send({"type": "debug_end_game", "reason": "ImpostorByKill"})
        self.last_action = "bot mode: force end (1v1)"
        log.info("session: bot-mode force end game (1v1)")

    async def _bot_meeting_tick(self) -> None:
        """bot 局:假人不会报警,由托管层在杀人后/定时触发紧急会议,让整局循环跑起来。"""
        if not (self.cfg.bots > 0 and self.cfg.bot_meetings):
            return
        now = time.time()
        st = self.world.state or {}
        if st.get("intro") is not False:          # 开场动画未结束
            return
        if now - self._playing_since < 25.0:       # 给击杀冷却留出窗口(很多职业刀 25s)
            return
        st_meet = self.world.state or {}
        if len([p for p in (st_meet.get("players") or []) if p.get("alive")]) <= 1:
            return
        kill_seq = 0
        for ev in list(self.world.events)[-20:]:
            if ev.get("code") == "KILL_CMD":
                kill_seq = ev.get("_seq", 0)
        new_kill = kill_seq > self._last_kill_seen
        if new_kill:
            self._last_kill_seen = kill_seq
        fallback = now - self._last_meeting_called_at > self.cfg.bot_meeting_interval
        # 击杀后尽快开会(整局必须包含会议环节);兜底会议要等冷却窗口,避免刚开局就开会
        due = (new_kill and now - self._playing_since > 12.0) or               (fallback and now - self._playing_since > 25.0)
        if not due:
            return
        if now - self._last_meeting_called_at < 8.0:
            return
        self._last_meeting_called_at = now
        await self._send({"type": "debug_call_meeting"})
        self.last_action = "bot mode: call meeting"
        log.info("session: bot-mode call meeting (new_kill=%s)", new_kill)

    async def _meeting_tick(self) -> None:
        # 会议里也可能已经满足结束条件(最后一个 bot 被刀/被票),先尝试托管结算
        await self._bot_end_tick()
        if self._meeting_ff_sent:
            return
        if time.time() - self._meeting_started_at > self.cfg.meeting_watchdog:
            self._meeting_ff_sent = True
            await self._send({"type": "debug_meeting_ff"})
            self.last_action = "meeting watchdog: force skip all"
            log.warning("session: meeting watchdog fired")

    async def _ended_tick(self) -> None:
        if not self.cfg.auto_next or self._next_sent:
            return
        if self.cfg.max_games and self.games_finished >= self.cfg.max_games:
            self._next_sent = True
            self.last_action = f"max_games({self.cfg.max_games}) reached"
            self.cfg.enabled = False
            self._restore_voice_timing()
            return
        if time.time() - self._phase_since < self.cfg.next_delay:
            return
        self._next_sent = True
        await self._send({"type": "next_game"})
        self.last_action = "next_game"
        log.info("session: next_game")

    # ---------- voice 节奏 ----------

    def _apply_voice_bot_timing(self) -> None:
        if self.voice is None:
            return
        try:
            self.voice.apply_timing(self.cfg.bot_turn_gap, self.cfg.bot_turn_timeout)
        except Exception:
            pass

    def _restore_voice_timing(self) -> None:
        if self.voice is None:
            return
        try:
            self.voice.restore_timing()
        except Exception:
            pass

    # ---------- 工具 ----------

    async def _send_options(self) -> None:
        await self._send({
            "type": "set_game_options",
            "num_impostors": self.cfg.num_impostors,
            "discussion_time": self.cfg.discussion_time,
            "voting_time": self.cfg.voting_time,
            "kill_cooldown": self.cfg.kill_cooldown,
            "kill_distance": self.cfg.kill_distance,
            "player_speed": self.cfg.player_speed,
        })

    async def _send(self, cmd: dict) -> bool:
        if self.bridge is None:
            return False
        ok = await self.bridge.send_command(cmd)
        if not ok:
            self.last_error = f"bridge offline: {cmd.get('type')}"
        return ok
