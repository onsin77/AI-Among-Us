"""自优化记忆:跨局经验(games.jsonl)+ 玩家印象(players.json),注入会议/复盘提示词。

文件在 agent/data/memory/ 下,纯追加/合并,损坏行跳过,不影响主流程。
"""

from __future__ import annotations

import json
import logging
import time
from pathlib import Path
from typing import Any

log = logging.getLogger("ai_agent.memory")


class GameMemory:
    def __init__(self, dir_: Path, inject_last: int = 5, max_players: int = 40) -> None:
        self.dir = dir_
        self.inject_last = inject_last
        self.max_players = max_players
        self.games_file = dir_ / "games.jsonl"
        self.players_file = dir_ / "players.json"
        self._players: dict[str, str] = {}
        try:
            self.dir.mkdir(parents=True, exist_ok=True)
        except OSError as e:
            log.warning("memory dir create failed: %s", e)
        self._load()

    # ---------- 载入 ----------

    def _load(self) -> None:
        try:
            if self.players_file.exists():
                obj = json.loads(self.players_file.read_text(encoding="utf-8"))
                if isinstance(obj, dict):
                    self._players = {str(k): str(v)[:200] for k, v in obj.items()}
        except Exception as e:
            log.warning("players.json load failed: %s", e)

    def recent_games(self, n: int | None = None) -> list[dict[str, Any]]:
        out: list[dict[str, Any]] = []
        try:
            if not self.games_file.exists():
                return out
            lines = self.games_file.read_text(encoding="utf-8").splitlines()
            for line in lines[-(n or self.inject_last) * 2:]:   # 多读一倍,过滤坏行
                line = line.strip()
                if not line:
                    continue
                try:
                    obj = json.loads(line)
                except json.JSONDecodeError:
                    continue
                if isinstance(obj, dict):
                    out.append(obj)
        except OSError as e:
            log.warning("games.jsonl read failed: %s", e)
        return out[-(n or self.inject_last):]

    # ---------- 写入 ----------

    def record_game(self, summary: dict[str, Any]) -> None:
        summary = dict(summary)
        summary.setdefault("at", time.strftime("%Y-%m-%d %H:%M"))
        try:
            with self.games_file.open("a", encoding="utf-8") as fh:
                fh.write(json.dumps(summary, ensure_ascii=False) + "\n")
        except OSError as e:
            log.warning("games.jsonl append failed: %s", e)
        notes = summary.get("player_notes")
        if isinstance(notes, dict) and notes:
            self.update_players(notes)

    def update_players(self, notes: dict[str, str]) -> None:
        for name, note in notes.items():
            name = str(name).strip()
            note = str(note or "").strip()
            if not name or not note:
                continue
            old = self._players.get(name, "")
            merged = note if note in old else (f"{old} | {note}" if old else note)
            self._players[name] = merged[:300]
        # 防膨胀:太多玩家时丢最旧的
        if len(self._players) > self.max_players:
            keep = dict(list(self._players.items())[-self.max_players:])
            self._players = keep
        try:
            self.players_file.write_text(
                json.dumps(self._players, ensure_ascii=False, indent=1), encoding="utf-8")
        except OSError as e:
            log.warning("players.json write failed: %s", e)

    # ---------- 提示词注入 ----------

    def prompt_snippet(self, roster: list[str] | None = None) -> str:
        """会议提示词的"历史经验"段;无内容返回空串。roster 限定只注入在场玩家印象。"""
        parts: list[str] = []
        games = self.recent_games()
        lessons: list[str] = []
        for g in games[-3:]:
            for s in g.get("lessons") or []:
                if s:
                    lessons.append(str(s))
        if lessons:
            parts.append("【历史经验(自优化记忆)】\n" + "\n".join(f"- {s}" for s in lessons[-6:]))
        notes = []
        for name in (roster or list(self._players.keys())):
            n = self._players.get(name)
            if n:
                notes.append(f"- {name}:{n}")
        if notes:
            parts.append("【玩家印象】\n" + "\n".join(notes))
        return ("\n\n".join(parts) + "\n\n") if parts else ""

    def stats(self) -> dict[str, Any]:
        return {"games": len(self.recent_games(50)), "players": len(self._players),
                "dir": str(self.dir)}
