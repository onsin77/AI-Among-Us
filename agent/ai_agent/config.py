"""配置加载:config.ini + 环境变量覆盖。"""

from __future__ import annotations

import configparser
import os
from dataclasses import dataclass, field
from pathlib import Path


@dataclass
class Config:
    bridge_host: str = "127.0.0.1"
    bridge_port: int = 47600
    web_host: str = "127.0.0.1"
    web_port: int = 8765
    deepseek_api_key: str = ""
    deepseek_base_url: str = "https://api.deepseek.com"
    fast_model: str = "deepseek-chat"
    deep_model: str = "deepseek-reasoner"
    # 行为引擎(迭代 3):monitor=只观察|crew=好人自动任务/报尸|impostor=坏人巡逻击杀|auto=按角色
    behavior_mode: str = "monitor"
    behavior_tick: float = 0.6
    behavior_auto_abilities: bool = True
    # 会议管线:从 JSONL 文件读聊天/投票触发,DeepSeek 产出 say/vote
    meeting_enabled: bool = True
    meeting_chat_file: str = "data/meeting_chat.jsonl"
    meeting_out_file: str = "data/meeting_out.jsonl"
    meeting_model_tier: str = "fast"
    meeting_min_interval: float = 6.0
    # 启动时自动开局:none|freeplay
    autostart: str = "none"
    autostart_map: int = 4
    # 语音会议(迭代 4):VB-CABLE 采集 → 讯飞转写 → 轮麦发言 → XTTS 回输
    voice_enabled: bool = True
    voice_capture_device: str = "CABLE Output"
    voice_play_device: str = "CABLE Input"
    voice_turn_end_gap: float = 3.5
    voice_turn_timeout: float = 30.0
    voice_also_chat: bool = False
    voice_out_file: str = "data/voice_out.jsonl"
    asr_app_id: str = ""
    asr_api_key: str = ""
    asr_api_secret: str = ""
    asr_lang: str = "autodialect"
    tts_base_url: str = "http://127.0.0.1:17895"
    tts_voice: str = "xiaoyan"
    tts_speed: int = 55
    tts_volume: int = 60
    memory_dir: str = "data/memory"
    memory_inject_last: int = 5
    # 整局托管(迭代 4.1):开房/加 bot/开局/连续下一局
    session_enabled: bool = False
    session_host_kind: str = "local"
    session_map: int = 4
    session_bots: int = 0
    session_min_players: int = 4
    session_auto_start: bool = True
    session_auto_next: bool = True
    session_max_games: int = 0
    session_max_meetings: int = 0
    session_bot_turn_timeout: float = 7.0
    config_path: Path = field(default_factory=lambda: Path("config.ini"))

    @classmethod
    def load(cls, path: str | Path | None = None) -> "Config":
        p = Path(path) if path else Path(__file__).resolve().parent.parent / "config.ini"
        cfg = cls(config_path=p)
        if not p.exists():
            return cfg  # 全默认值
        parser = configparser.ConfigParser()
        parser.read(p, encoding="utf-8")
        if parser.has_section("bridge"):
            cfg.bridge_host = parser.get("bridge", "host", fallback=cfg.bridge_host).strip()
            cfg.bridge_port = parser.getint("bridge", "port", fallback=cfg.bridge_port)
        if parser.has_section("web"):
            cfg.web_host = parser.get("web", "host", fallback=cfg.web_host).strip()
            cfg.web_port = parser.getint("web", "port", fallback=cfg.web_port)
        if parser.has_section("deepseek"):
            cfg.deepseek_api_key = parser.get("deepseek", "api_key", fallback="").strip()
            cfg.deepseek_base_url = parser.get("deepseek", "base_url", fallback=cfg.deepseek_base_url).strip()
            cfg.fast_model = parser.get("deepseek", "fast_model", fallback=cfg.fast_model).strip()
            cfg.deep_model = parser.get("deepseek", "deep_model", fallback=cfg.deep_model).strip()
        if parser.has_section("behavior"):
            cfg.behavior_mode = parser.get("behavior", "mode", fallback=cfg.behavior_mode).strip()
            cfg.behavior_tick = parser.getfloat("behavior", "tick", fallback=cfg.behavior_tick)
            cfg.behavior_auto_abilities = parser.getboolean("behavior", "auto_abilities",
                                                            fallback=cfg.behavior_auto_abilities)
        if parser.has_section("meeting"):
            cfg.meeting_enabled = parser.getboolean("meeting", "enabled", fallback=cfg.meeting_enabled)
            cfg.meeting_chat_file = parser.get("meeting", "chat_file", fallback=cfg.meeting_chat_file).strip()
            cfg.meeting_out_file = parser.get("meeting", "out_file", fallback=cfg.meeting_out_file).strip()
            cfg.meeting_model_tier = parser.get("meeting", "model_tier", fallback=cfg.meeting_model_tier).strip()
            cfg.meeting_min_interval = parser.getfloat("meeting", "min_interval", fallback=cfg.meeting_min_interval)
        if parser.has_section("startup"):
            cfg.autostart = parser.get("startup", "autostart", fallback=cfg.autostart).strip()
            cfg.autostart_map = parser.getint("startup", "map", fallback=cfg.autostart_map)
        if parser.has_section("voice"):
            cfg.voice_enabled = parser.getboolean("voice", "enabled", fallback=cfg.voice_enabled)
            cfg.voice_capture_device = parser.get("voice", "capture_device", fallback=cfg.voice_capture_device).strip()
            cfg.voice_play_device = parser.get("voice", "play_device", fallback=cfg.voice_play_device).strip()
            cfg.voice_turn_end_gap = parser.getfloat("voice", "turn_end_gap", fallback=cfg.voice_turn_end_gap)
            cfg.voice_turn_timeout = parser.getfloat("voice", "turn_timeout", fallback=cfg.voice_turn_timeout)
            cfg.voice_also_chat = parser.getboolean("voice", "also_chat_in_game", fallback=cfg.voice_also_chat)
            cfg.voice_out_file = parser.get("voice", "out_file", fallback=cfg.voice_out_file).strip()
        if parser.has_section("asr"):
            cfg.asr_app_id = parser.get("asr", "app_id", fallback="").strip()
            cfg.asr_api_key = parser.get("asr", "api_key", fallback="").strip()
            cfg.asr_api_secret = parser.get("asr", "api_secret", fallback="").strip()
            cfg.asr_lang = parser.get("asr", "lang", fallback=cfg.asr_lang).strip()
        if parser.has_section("tts"):
            cfg.tts_base_url = parser.get("tts", "base_url", fallback=cfg.tts_base_url).strip()
            cfg.tts_voice = parser.get("tts", "voice", fallback=cfg.tts_voice).strip()
            cfg.tts_speed = parser.getint("tts", "speed", fallback=cfg.tts_speed)
            cfg.tts_volume = parser.getint("tts", "volume", fallback=cfg.tts_volume)
        if parser.has_section("memory"):
            cfg.memory_dir = parser.get("memory", "dir", fallback=cfg.memory_dir).strip()
            cfg.memory_inject_last = parser.getint("memory", "inject_last", fallback=cfg.memory_inject_last)
        if parser.has_section("session"):
            cfg.session_enabled = parser.getboolean("session", "enabled", fallback=cfg.session_enabled)
            cfg.session_host_kind = parser.get("session", "host_kind", fallback=cfg.session_host_kind).strip()
            cfg.session_map = parser.getint("session", "map", fallback=cfg.session_map)
            cfg.session_bots = parser.getint("session", "bots", fallback=cfg.session_bots)
            cfg.session_min_players = parser.getint("session", "min_players", fallback=cfg.session_min_players)
            cfg.session_auto_start = parser.getboolean("session", "auto_start", fallback=cfg.session_auto_start)
            cfg.session_auto_next = parser.getboolean("session", "auto_next", fallback=cfg.session_auto_next)
            cfg.session_max_games = parser.getint("session", "max_games", fallback=cfg.session_max_games)
            cfg.session_max_meetings = parser.getint("session", "max_meetings", fallback=cfg.session_max_meetings)
            cfg.session_bot_turn_timeout = parser.getfloat("session", "bot_turn_timeout",
                                                           fallback=cfg.session_bot_turn_timeout)
        cfg.deepseek_api_key = os.environ.get("DEEPSEEK_API_KEY", cfg.deepseek_api_key).strip()
        cfg.asr_app_id = os.environ.get("XF_APP_ID", cfg.asr_app_id).strip()
        cfg.asr_api_key = os.environ.get("XF_API_KEY", cfg.asr_api_key).strip()
        cfg.asr_api_secret = os.environ.get("XF_API_SECRET", cfg.asr_api_secret).strip()
        return cfg
