"""FastAPI 面板服务:状态/事件/地图缓存/行为与会议控制/指令透传。"""

from __future__ import annotations

import time
from pathlib import Path

from fastapi import Body, FastAPI
from fastapi.responses import FileResponse, JSONResponse

from .behavior import BehaviorEngine
from .bridge import BridgeClient
from .control import Controller
from .meeting import MeetingPipeline
from .world import WorldModel

STATIC_DIR = Path(__file__).resolve().parent / "static"


def create_app(world: WorldModel, bridge: BridgeClient,
               behavior: BehaviorEngine | None = None,
               meeting: MeetingPipeline | None = None,
               controller: Controller | None = None,
               voice=None, session=None) -> FastAPI:
    app = FastAPI(title="AIAmongUs Agent", docs_url=None, redoc_url=None)

    @app.get("/")
    async def index() -> FileResponse:
        return FileResponse(STATIC_DIR / "index.html")

    @app.get("/api/status")
    async def status() -> dict:
        return world.status()

    @app.get("/api/state")
    async def state() -> dict:
        return {
            "meta": {
                "updated_at": world.state_updated_at,
                "age": (time.time() - world.state_updated_at) if world.state_updated_at else None,
            },
            "state": world.state,
        }

    @app.get("/api/logs")
    async def logs() -> dict:
        return {"logs": list(world.logs), "acks": list(world.acks)}

    @app.get("/api/events")
    async def events(since: int = 0, limit: int = 200) -> dict:
        evs = [e for e in world.events if e.get("_seq", 0) > since]
        return {"events": evs[-limit:], "seq": world.event_seq}

    @app.get("/api/chat")
    async def chat() -> dict:
        return {"chat": list(world.chat)}

    @app.get("/api/map")
    async def map_cache() -> dict:
        return {
            "nav_dump": world.nav_dump,
            "nav_path": world.nav_path,
            "nav_stats": world.nav_stats,
        }

    @app.get("/api/debug_players")
    async def debug_players() -> dict:
        return world.debug_players or {"players": []}

    @app.get("/api/control")
    async def control_status() -> dict:
        if controller is None:
            return {"takeover": False, "error": "controller 未初始化"}
        return controller.status()

    @app.post("/api/control")
    async def control_action(body: dict = Body(...)) -> JSONResponse:
        if controller is None:
            return JSONResponse({"ok": False, "error": "controller 未初始化"}, status_code=400)
        action = body.get("action")
        if action == "start":
            st = await controller.start(
                mode=str(body.get("mode") or "auto"),
                autostart=bool(body.get("autostart")),
                map_id=int(body.get("map") or 4),
            )
            return JSONResponse({"ok": True, "status": st})
        if action == "stop":
            st = await controller.stop()
            return JSONResponse({"ok": True, "status": st})
        if action == "shutdown":
            st = await controller.shutdown()
            return JSONResponse({"ok": True, "status": st})
        return JSONResponse({"ok": False, "error": "action 必须是 start|stop|shutdown"}, status_code=400)

    @app.get("/api/behavior")
    async def behavior_status() -> dict:
        if behavior is None:
            return {"enabled": False, "mode": "monitor"}
        return {"enabled": True, **behavior.status()}

    @app.post("/api/behavior")
    async def behavior_set(body: dict = Body(...)) -> JSONResponse:
        if behavior is None:
            return JSONResponse({"ok": False, "error": "behavior 未启用(以 --mode 启动)"}, status_code=400)
        mode = body.get("mode")
        if mode is not None:
            if mode not in ("monitor", "crew", "impostor", "auto"):
                return JSONResponse({"ok": False, "error": "mode 必须是 monitor|crew|impostor|auto"},
                                    status_code=400)
            behavior.cfg.mode = mode
            world.behavior.update({"mode": mode, "running": True})
        if "auto_abilities" in body:
            behavior.cfg.auto_abilities = bool(body.get("auto_abilities"))
        return {"ok": True, "mode": behavior.cfg.mode, "auto_abilities": behavior.cfg.auto_abilities}

    @app.get("/api/meeting")
    async def meeting_status() -> dict:
        if meeting is None:
            return {"enabled": False}
        return {"enabled": True, **meeting.status()}

    @app.post("/api/meeting/feed")
    async def meeting_feed(body: dict = Body(...)) -> JSONResponse:
        """面板/脚本喂一条会议内容:{"speaker":"Red","text":"..."} 或 {"type":"vote","target":"Blue"}。"""
        if meeting is None:
            return JSONResponse({"ok": False, "error": "meeting 未启用"}, status_code=400)
        meeting.feed_line(body)
        return JSONResponse({"ok": True})

    @app.post("/api/meeting/respond")
    async def meeting_respond() -> JSONResponse:
        """立即触发一次 AI 会议推理(不等文件新行)。"""
        if meeting is None:
            return JSONResponse({"ok": False, "error": "meeting 未启用"}, status_code=400)
        result = await meeting.respond_now()
        return JSONResponse({"ok": True, "result": result})

    @app.post("/api/command")
    async def command(body: dict = Body(...)) -> JSONResponse:
        if not isinstance(body.get("type"), str):
            return JSONResponse({"queued": False, "error": "missing 'type'"}, status_code=400)
        ok = await bridge.send_command(body)
        return JSONResponse({"queued": ok, "payload": body})

    # ---------- 语音会议(迭代 4) ----------

    @app.get("/api/voice")
    async def voice_status() -> dict:
        if voice is None:
            return {"enabled": False, "error": "voice 管线未初始化(--no-voice 或依赖缺失)"}
        return voice.status()

    @app.get("/api/voice/devices")
    async def voice_devices() -> dict:
        from .voice.audioio import list_devices
        return {"devices": list_devices()}

    @app.post("/api/voice")
    async def voice_control(body: dict = Body(...)) -> JSONResponse:
        if voice is None:
            return JSONResponse({"ok": False, "error": "voice 管线未初始化"}, status_code=400)
        result = await voice.control(body)
        return JSONResponse(result)

    # ---------- 整局托管(迭代 4.1) ----------

    @app.get("/api/session")
    async def session_status() -> dict:
        if session is None:
            return {"enabled": False, "error": "session 未初始化"}
        return session.status()

    @app.post("/api/session")
    async def session_control(body: dict = Body(...)) -> JSONResponse:
        if session is None:
            return JSONResponse({"ok": False, "error": "session 未初始化"}, status_code=400)
        result = await session.control(body)
        return JSONResponse(result)

    return app
