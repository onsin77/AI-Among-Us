#!/usr/bin/env python3
"""会议发言控制台(交互式):把发言/投票写进 agent 读取的 JSONL 文件,并实时打印模型输出。

用法:
    cd agent
    python tools/meeting_console.py                 # 默认 config.ini 的 meeting chat/out
    python tools/meeting_console.py --file data/meeting_chat.jsonl --out data/meeting_out.jsonl

输入格式(每行一条):
    Red: I saw Blue vent in Electrical      → 玩家发言
    /vote Blue                              → 要求 AI 投票(名字/skip)
    /trigger                                → 强制 AI 回应(不新增发言)
    /turn [next|me|skip]                    → 语音会议:通知换人发言了(默认 next)
    /vstatus                                → 语音会议状态(轮次/绑定/转写)
    /tail                                   → 打印最近的模型输出
    /exit                                   → 退出

说明:
- 只在游戏处于 MEETING 阶段时,agent 才会消费新行并调用 DeepSeek(默认每 6s 最多一次)。
- 文件是追加式 UTF-8 JSONL;agent 从启动时的文件末尾开始读,所以请在 agent 启动后写入。
- 模型输出同时写入 --out(JSONL),并会通过 chat/vote 指令发进游戏。
- /turn、/vstatus 走 HTTP 面板接口(http://127.0.0.1:8765/api/voice),需 agent 在运行。
"""

from __future__ import annotations

import argparse
import json
import sys
import time
from pathlib import Path

AGENT_DIR = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(AGENT_DIR))

from ai_agent.config import Config  # noqa: E402

WEB = f"http://127.0.0.1:{8765}"


def http_json(path: str, obj: dict | None = None):
    import urllib.request
    url = WEB + path
    try:
        if obj is None:
            with urllib.request.urlopen(url, timeout=4) as r:
                return json.load(r)
        req = urllib.request.Request(url, data=json.dumps(obj).encode(),
                                     headers={"Content-Type": "application/json"})
        with urllib.request.urlopen(req, timeout=6) as r:
            return json.load(r)
    except Exception as e:
        return {"error": str(e)}


def append(path: Path, obj: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a", encoding="utf-8") as fh:
        fh.write(json.dumps(obj, ensure_ascii=False) + "\n")


def read_new(path: Path, offset: int) -> tuple[list[dict], int]:
    if not path.exists():
        return [], offset
    size = path.stat().st_size
    if size <= offset:
        return [], size
    out = []
    with path.open("r", encoding="utf-8") as fh:
        fh.seek(offset)
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                out.append(json.loads(line))
            except json.JSONDecodeError:
                pass
        offset = fh.tell()
    return out, offset


def main() -> None:
    cfg = Config.load()
    ap = argparse.ArgumentParser(description="会议发言控制台")
    ap.add_argument("--file", default=str((AGENT_DIR / cfg.meeting_chat_file)))
    ap.add_argument("--out", default=str((AGENT_DIR / cfg.meeting_out_file)))
    args = ap.parse_args()
    chat_path = Path(args.file)
    out_path = Path(args.out)
    out_off = out_path.stat().st_size if out_path.exists() else 0
    print(f"输入文件: {chat_path}")
    print(f"输出文件: {out_path}")
    print("格式: '名字: 发言' | /vote 名字|skip | /trigger | /turn [next|me|skip] | /vstatus | /tail | /exit")
    while True:
        try:
            line = input("> ").strip()
        except (EOFError, KeyboardInterrupt):
            break
        if not line:
            continue
        if line in ("/exit", "/quit"):
            break
        if line == "/tail":
            msgs, out_off = read_new(out_path, out_off)
            for m in msgs:
                print(f"  AI({m.get('latency_ms')}ms) say={m.get('say')!r} vote={m.get('vote')!r}")
            if not msgs:
                print("  (暂无新输出)")
            continue
        if line.startswith("/vote"):
            target = line[5:].strip() or "skip"
            append(chat_path, {"type": "vote", "target": target})
            print(f"  -> 已写入投票请求: {target}")
        elif line == "/trigger":
            append(chat_path, {"type": "trigger"})
            print("  -> 已触发 AI 回应")
        elif ":" in line:
            speaker, text = line.split(":", 1)
            append(chat_path, {"speaker": speaker.strip(), "text": text.strip()})
            print(f"  -> 已写入发言 {speaker.strip()}")
        else:
            print("  无法识别。用 '名字: 发言' 或 /vote /trigger /tail /exit")
        # 立即尝试读一次输出(模型可能还没回,晚点用 /tail)
        time.sleep(0.3)
        msgs, out_off = read_new(out_path, out_off)
        for m in msgs:
            print(f"  AI({m.get('latency_ms')}ms) say={m.get('say')!r} vote={m.get('vote')!r}")


if __name__ == "__main__":
    main()
