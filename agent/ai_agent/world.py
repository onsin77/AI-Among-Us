"""世界模型:缓存最新状态快照(full/delta 合并)、连接元数据、日志与 ack。"""

from __future__ import annotations

import time
from collections import deque
from typing import Any


class WorldModel:
    def __init__(self) -> None:
        self.state: dict[str, Any] = {}
        self.state_updated_at: float = 0.0

        self.connected: bool = False
        self.last_hello: dict[str, Any] | None = None
        self.connected_at: float | None = None
        self.last_msg_at: float = 0.0
        self.last_pong_at: float = 0.0

        self._msg_times: deque[float] = deque(maxlen=600)
        self.msgs_total: int = 0

        self.logs: deque[dict[str, Any]] = deque(maxlen=200)
        self.acks: deque[dict[str, Any]] = deque(maxlen=200)
        self.events: deque[dict[str, Any]] = deque(maxlen=800)   # 事件流(按时间追加)
        self.event_seq: int = 0
        self.ack_seq: int = 0
        self.chat: deque[dict[str, Any]] = deque(maxlen=100)     # 聊天(来自 state.chat,去重追加)
        self._chat_seen: set[tuple] = set()

        # 导航缓存(L1 主动推送)
        self.nav_dump: dict[str, Any] | None = None
        self.nav_path: dict[str, Any] | None = None
        self.nav_stats: dict[str, Any] | None = None
        self.debug_players: dict[str, Any] | None = None

        # 我的行程(房间变化时间线,会议发言/复盘用;新对局自动清零)
        self.journey: deque[tuple[float, str]] = deque(maxlen=400)
        self._last_room: str | None = None
        self._journey_phase: str | None = None

        # 行为/会议模块状态(面板读取)
        self.behavior: dict[str, Any] = {"mode": "monitor", "role": None, "action": "idle"}
        self.meeting_ai: dict[str, Any] = {"enabled": False, "last": None}

    # ---------- 上行消息应用(全部在事件循环线程) ----------

    def apply(self, msg: dict[str, Any]) -> None:
        self.msgs_total += 1
        now = time.time()
        self._msg_times.append(now)
        self.last_msg_at = now

        mtype = msg.get("type")
        if mtype == "state_full":
            self.state = msg
            self.state_updated_at = now
        elif mtype == "state_delta":
            # 顶层浅合并(L1 的 delta 只做顶层字段裁剪)
            merged = dict(self.state)
            merged.update(msg)
            self.state = merged
            self.state_updated_at = now
        elif mtype == "hello":
            self.last_hello = msg
        elif mtype == "log":
            self.logs.appendleft(msg)
        elif mtype == "ack":
            self.ack_seq += 1
            msg["_seq"] = self.ack_seq
            self.acks.appendleft(msg)
        elif mtype == "pong":
            self.last_pong_at = now
        elif mtype == "event":
            self.event_seq += 1
            msg["_seq"] = self.event_seq
            self.events.append(msg)
        elif mtype == "nav_dump":
            self.nav_dump = msg
        elif mtype == "nav_path":
            self.nav_path = msg
        elif mtype == "nav_stats":
            self.nav_stats = msg
        elif mtype == "debug_players":
            self.debug_players = msg
        # 未知类型忽略(向前兼容)

        # state.chat 增量归档(聊天采集在 L1;去重后追加到 events 流之外)
        st = self.state
        if st.get("type") in ("state_full", "state_delta"):
            phase = st.get("phase")
            # 行程时间线:新对局(LOBBY/MENU→PLAYING)清零;PLAYING/MEETING 内记录房间变化
            if phase != self._journey_phase:
                if phase == "PLAYING" and self._journey_phase in ("LOBBY", "MENU", None):
                    self.journey.clear()
                    self._last_room = None
                self._journey_phase = phase
            me = st.get("me")
            room = (me or {}).get("room") if isinstance(me, dict) else None
            if room and phase in ("PLAYING", "MEETING") and room != self._last_room:
                self._last_room = str(room)
                self.journey.append((now, str(room)))
            if isinstance(st.get("chat"), list):
                for m in st["chat"]:
                    key = (m.get("name"), m.get("text"), m.get("t"))
                    if key in self._chat_seen:
                        continue
                    self._chat_seen.add(key)
                    self.chat.append(m)
                if len(self._chat_seen) > 500:
                    self._chat_seen.clear()
                    for m in self.chat:
                        self._chat_seen.add((m.get("name"), m.get("text"), m.get("t")))

    def on_connected(self) -> None:
        self.connected = True
        self.connected_at = time.time()

    def on_disconnected(self) -> None:
        self.connected = False
        self.connected_at = None

    # ---------- 面板查询 ----------

    def msgs_per_sec(self) -> float:
        now = time.time()
        while self._msg_times and now - self._msg_times[0] > 3.0:
            self._msg_times.popleft()
        if len(self._msg_times) < 2:
            return 0.0
        window = now - self._msg_times[0]
        return len(self._msg_times) / window if window > 0 else 0.0

    def status(self) -> dict[str, Any]:
        return {
            "connected": self.connected,
            "connected_at": self.connected_at,
            "last_msg_age": (time.time() - self.last_msg_at) if self.last_msg_at else None,
            "msgs_per_sec": round(self.msgs_per_sec(), 1),
            "msgs_total": self.msgs_total,
            "hello": self.last_hello,
        }
