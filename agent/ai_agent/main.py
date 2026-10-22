"""Agent 入口:python -m ai_agent.main [选项]

启动脚本开关(也可写进 config.ini 的 [behavior]/[meeting]/[startup] 段):
  --mode monitor|crew|impostor|auto   行为引擎模式(默认取 config.ini,monitor=纯监控)
      crew      好人:自动做任务 + 附近有尸体就报告
      impostor  坏人:巡逻 + 接近并击杀,**永不报警**(刀完人也不报)
      auto      按 L1 状态里的角色自动切换
  --meeting-file PATH   会议聊天输入 JSONL(默认 agent/data/meeting_chat.jsonl)
  --meeting-out PATH    会议 AI 输出复盘 JSONL(默认 agent/data/meeting_out.jsonl)
  --no-meeting          关闭会议管线
  --model fast|deep     会议推理模型档位(默认 fast=deepseek-chat)
  --autostart none|freeplay   启动后自动开一局(默认 none;freeplay 需 Steam 在线)
  --map N               autostart 的地图(默认 4=Airship)
  --selftest-model      只跑 3 个模型冒烟用例然后退出(不启动面板)
  --no-voice            关闭语音会议管线(讯飞转写+XTTS 回输)
  --config PATH         指定 config.ini
"""

from __future__ import annotations

import argparse
import asyncio
import json
import logging
import time
from pathlib import Path

import uvicorn

from .behavior import BehaviorConfig, BehaviorEngine
from .bridge import BridgeClient
from .config import Config
from .control import Controller
from .meeting import MeetingConfig, MeetingPipeline
from .memory import GameMemory
from .model import DeepSeekClient, ModelError
from .server import create_app
from .session import SessionConfig, SessionManager
from .voice.pipeline import VoiceConfig, VoiceMeetingPipeline
from .world import WorldModel

AGENT_DIR = Path(__file__).resolve().parent.parent


def _abs(p: str) -> Path:
    q = Path(p)
    return q if q.is_absolute() else (AGENT_DIR / q)


async def _selftest_model(client: DeepSeekClient) -> int:
    """模型冒烟:3 个会议场景,校验 JSON schema 与延迟。"""
    cases = [
        ("船员", "Crewmate(Crew)",
         "轮次:2。玩家名单:Red, Blue, Green。\n聊天记录:\nRed: I saw Blue vent in Electrical\nBlue: that's a lie, I was in MedBay",
         ["Blue", "Red", "Green"]),
        ("内鬼", "Impostor(Impostor)",
         "轮次:2。玩家名单:Red, Blue, Green。\n聊天记录:\nRed: where were you Blue?\nGreen: I was doing wires",
         ["Red", "Green"]),
        ("无人说话", "Crewmate(Crew)", "轮次:1。玩家名单:Red, Blue。\n聊天记录:\n(暂无发言)", ["Red", "Blue"]),
    ]
    ok = 0
    for label, role, user, roster in cases:
        sys_p = (f"你是 Among Us 的玩家「AI」。你的身份:{role}。\n"
                 "输出严格 JSON:{\"say\":\"...\",\"vote\":\"skip\"或某玩家名或null}")
        t0 = time.time()
        try:
            r = await client.chat([{"role": "system", "content": sys_p},
                                   {"role": "user", "content": user}],
                                  tier="fast", json_mode=True, max_tokens=160)
        except ModelError as e:
            print(f"[FAIL] {label}: {e}")
            continue
        obj = r.json_obj or {}
        valid = isinstance(obj.get("say"), str) and (
            obj.get("vote") is None or obj.get("vote") == "skip" or obj.get("vote") in roster)
        print(f"[{'PASS' if valid else 'FAIL'}] {label} {r.latency_ms}ms "
              f"say={obj.get('say')!r} vote={obj.get('vote')!r}")
        ok += 1 if valid else 0
    print(f"model selftest: {ok}/{len(cases)}")
    return 0 if ok == len(cases) else 1


def main() -> None:
    parser = argparse.ArgumentParser(description="AIAmongUs L3 Agent(含行为引擎与会议管线)",
                                     formatter_class=argparse.RawDescriptionHelpFormatter,
                                     epilog=__doc__)
    parser.add_argument("--config", default=None, help="config.ini 路径(默认 agent/config.ini)")
    parser.add_argument("--mode", choices=["monitor", "crew", "impostor", "auto"], default=None,
                        help="行为模式(覆盖 config.ini)")
    parser.add_argument("--meeting-file", default=None, help="会议聊天输入 JSONL 路径")
    parser.add_argument("--meeting-out", default=None, help="会议 AI 输出 JSONL 路径")
    parser.add_argument("--no-meeting", action="store_true", help="关闭会议管线")
    parser.add_argument("--model", choices=["fast", "deep"], default=None, help="会议推理模型档位")
    parser.add_argument("--autostart", choices=["none", "freeplay"], default=None,
                        help="启动后自动开局")
    parser.add_argument("--map", type=int, default=None, help="autostart 地图(4=Airship)")
    parser.add_argument("--selftest-model", action="store_true", help="只跑模型冒烟用例")
    parser.add_argument("--no-voice", action="store_true", help="关闭语音会议管线")
    args = parser.parse_args()

    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")
    cfg = Config.load(args.config)

    if args.mode:
        cfg.behavior_mode = args.mode
    if args.meeting_file:
        cfg.meeting_chat_file = args.meeting_file
    if args.meeting_out:
        cfg.meeting_out_file = args.meeting_out
    if args.no_meeting:
        cfg.meeting_enabled = False
    if args.model:
        cfg.meeting_model_tier = args.model
    if args.autostart:
        cfg.autostart = args.autostart
    if args.map is not None:
        cfg.autostart_map = args.map

    model = DeepSeekClient(cfg.deepseek_api_key, cfg.deepseek_base_url,
                           cfg.fast_model, cfg.deep_model)

    if args.selftest_model:
        raise SystemExit(asyncio.run(_selftest_model(model)))

    world = WorldModel()
    bridge = BridgeClient(cfg.bridge_host, cfg.bridge_port, world)
    # 行为引擎/会议管线始终创建(面板“开始/停止”可运行时切换),默认按 config.ini
    behavior = BehaviorEngine(world, bridge, BehaviorConfig(
        mode=cfg.behavior_mode, tick=cfg.behavior_tick,
        auto_abilities=cfg.behavior_auto_abilities))
    meeting = MeetingPipeline(world, bridge, model, MeetingConfig(
        chat_file=_abs(cfg.meeting_chat_file),
        out_file=_abs(cfg.meeting_out_file),
        model_tier=cfg.meeting_model_tier,
        min_interval=cfg.meeting_min_interval,
        enabled=cfg.meeting_enabled,
    ))
    if not model.available:
        logging.warning("meeting 管线已启用但 DeepSeek api_key 为空:会议将不产生发言")

    # 语音会议(迭代 4):讯飞转写 + 轮麦 + XTTS 回输
    voice = None
    if not args.no_voice:
        try:
            voice = VoiceMeetingPipeline(world, bridge, model, GameMemory(
                _abs(cfg.memory_dir), cfg.memory_inject_last), VoiceConfig(
                enabled=cfg.voice_enabled,
                capture_device=cfg.voice_capture_device,
                play_device=cfg.voice_play_device,
                turn_end_gap=cfg.voice_turn_end_gap,
                turn_timeout=cfg.voice_turn_timeout,
                also_chat_in_game=cfg.voice_also_chat,
                out_file=str(_abs(cfg.voice_out_file)),
                asr_app_id=cfg.asr_app_id, asr_api_key=cfg.asr_api_key,
                asr_api_secret=cfg.asr_api_secret, asr_lang=cfg.asr_lang,
                tts_base_url=cfg.tts_base_url, tts_voice=cfg.tts_voice,
                tts_speed=cfg.tts_speed, tts_volume=cfg.tts_volume,
            ))
            voice._behavior = behavior          # 会议目标轻量注入
            voice._meeting = meeting            # 语音会议期间暂停文件会议管线(防双重应答)
        except Exception as e:
            logging.warning("voice 管线初始化失败(忽略):%s", e)
            voice = None

    controller = Controller(world, bridge, behavior, meeting)
    session = SessionManager(world, bridge, SessionConfig(
        enabled=cfg.session_enabled,
        host_kind=cfg.session_host_kind,
        map_id=cfg.session_map,
        bots=cfg.session_bots,
        min_players=cfg.session_min_players,
        auto_start=cfg.session_auto_start,
        auto_next=cfg.session_auto_next,
        max_games=cfg.session_max_games,
        max_meetings=cfg.session_max_meetings,
        bot_turn_timeout=cfg.session_bot_turn_timeout,
    ), voice=voice, controller=controller)

    app = create_app(world, bridge, behavior=behavior, meeting=meeting, controller=controller,
                     voice=voice, session=session)

    @app.on_event("startup")
    async def _startup() -> None:
        loop = asyncio.get_running_loop()
        app.state.loop = loop
        app.state.bridge_task = loop.create_task(bridge.run())
        app.state.behavior_task = loop.create_task(behavior.run())
        app.state.meeting_task = loop.create_task(meeting.run())
        app.state.session_task = loop.create_task(session.run())
        if voice is not None:
            app.state.voice_task = loop.create_task(voice.run())
        if cfg.autostart == "freeplay":
            app.state.autostart_task = loop.create_task(_autostart(bridge, cfg.autostart_map))

    @app.on_event("shutdown")
    async def _shutdown() -> None:
        await session.stop()
        if voice is not None:
            await voice.stop()
        await behavior.stop()
        await meeting.stop()
        await bridge.stop()

    server = uvicorn.Server(uvicorn.Config(app, host=cfg.web_host, port=cfg.web_port,
                                           log_level="warning", access_log=False))
    controller.server = server
    server.run()


async def _autostart(bridge: BridgeClient, map_id: int) -> None:
    """等桥连上后发 debug_start(freeplay)。"""
    for _ in range(60):
        if bridge.connected:
            await bridge.send_command({"type": "debug_start", "mode": "freeplay", "map": map_id})
            logging.info("autostart: debug_start freeplay map=%s", map_id)
            return
        await asyncio.sleep(1.0)
    logging.warning("autostart: bridge not connected, skipped")


if __name__ == "__main__":
    main()
