"""接管控制器:面板上的“开始/停止/关闭脚本”。

- start:把行为引擎切到指定模式(auto/crew/impostor),会议管线启用;可选自动开一局 freeplay。
  开启后 AI 接手当前/新对局(该做任务做任务、该刀人刀人、遇尸按规则处理)。
- stop:行为回 monitor(不再下指令)、会议暂停、并让游戏内角色停下(停导航+关面板)。
- shutdown:优雅退出整个 agent 进程(面板服务 + 桥连接一起关闭)。
"""

from __future__ import annotations

import asyncio
import logging
import time
from typing import Any

from .behavior import BehaviorEngine
from .bridge import BridgeClient
from .meeting import MeetingPipeline
from .world import WorldModel

log = logging.getLogger("ai_agent.control")


class Controller:
    def __init__(self, world: WorldModel, bridge: BridgeClient,
                 behavior: BehaviorEngine, meeting: MeetingPipeline) -> None:
        self.world = world
        self.bridge = bridge
        self.behavior = behavior
        self.meeting = meeting
        self.takeover = False
        self.mode = "monitor"
        self.started_at: float | None = None
        self.last_action: str | None = None
        self.server = None  # uvicorn.Server,shutdown 用

    def status(self) -> dict[str, Any]:
        return {
            "takeover": self.takeover,
            "mode": self.mode,
            "started_at": self.started_at,
            "uptime": round(time.time() - self.started_at, 0) if self.started_at else 0,
            "last_action": self.last_action,
            "phase": (self.world.state or {}).get("phase"),
            "behavior": self.behavior.status(),
            "meeting_enabled": self.meeting.cfg.enabled,
        }

    async def start(self, mode: str = "auto", autostart: bool = False, map_id: int = 4) -> dict[str, Any]:
        if mode not in ("auto", "crew", "impostor"):
            mode = "auto"
        if autostart and (self.world.state or {}).get("phase") != "PLAYING":
            ok = await self.bridge.send_command({"type": "debug_start", "mode": "freeplay", "map": map_id})
            self.last_action = f"debug_start freeplay map={map_id} queued={ok}"
            # 等开局放到后台:行为引擎本就只在 PLAYING 行动,阻塞 HTTP handler 60s 只会让面板假死
            self._autostart_watch = asyncio.get_running_loop().create_task(
                self._watch_playing())
        self.mode = mode
        self.behavior.cfg.mode = mode
        self.meeting.cfg.enabled = True
        self.takeover = True
        self.started_at = time.time()
        self.last_action = f"takeover start mode={mode} autostart={autostart}"
        self.world.behavior.update({"mode": mode, "running": True})
        self.world.meeting_ai["enabled"] = True
        log.info("takeover started: mode=%s autostart=%s", mode, autostart)
        return self.status()

    async def _watch_playing(self) -> None:
        for _ in range(60):
            if (self.world.state or {}).get("phase") == "PLAYING":
                self.last_action = "autostart: PLAYING"
                return
            await asyncio.sleep(1.0)
        self.last_action = "autostart: not in PLAYING after 60s (continue anyway)"

    async def stop(self) -> dict[str, Any]:
        self.behavior.cfg.mode = "monitor"
        self.meeting.cfg.enabled = False
        self.takeover = False
        self.mode = "monitor"
        self.started_at = None
        w = getattr(self, "_autostart_watch", None)
        if w is not None and not w.done():
            w.cancel()
        self._autostart_watch = None
        # 停住游戏里的角色:停导航 + 关面板(面板会让 TaskPrivilege 自行 abort)
        await self.bridge.send_command({"type": "move_stop"})
        await self.bridge.send_command({"type": "minigame_close"})
        self.world.behavior.update({"mode": "monitor", "action": "idle"})
        self.world.meeting_ai["enabled"] = False
        self.last_action = "takeover stop"
        log.info("takeover stopped")
        return self.status()

    async def shutdown(self) -> dict[str, Any]:
        await self.stop()
        self.last_action = "agent shutdown"
        log.info("agent shutdown requested")
        if self.server is not None:
            self.server.should_exit = True
        return self.status()
