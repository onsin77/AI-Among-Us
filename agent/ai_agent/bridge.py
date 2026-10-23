"""L2 桥客户端:asyncio TCP,JSON 行协议,断线 1s 重连,5s 心跳。"""

from __future__ import annotations

import asyncio
import itertools
import json
import logging
import time

from .world import WorldModel

log = logging.getLogger("ai_agent.bridge")


class BridgeClient:
    def __init__(self, host: str, port: int, world: WorldModel) -> None:
        self.host = host
        self.port = port
        self.world = world
        self._reader: asyncio.StreamReader | None = None
        self._writer: asyncio.StreamWriter | None = None
        self._ref_counter = itertools.count(1)
        self._stopping = asyncio.Event()

    @property
    def connected(self) -> bool:
        return self._writer is not None and not self._writer.is_closing()

    # ---------- 生命周期 ----------

    async def run(self) -> None:
        """连接主循环,直到 stop() 被调用。

        任何异常(含 world.apply 的数据错误)都不允许杀死本任务:
        桥一死面板就永久 disconnected,只能重启进程。
        """
        while not self._stopping.is_set():
            try:
                await self._session()
            except (OSError, asyncio.IncompleteReadError, ConnectionError) as e:
                log.debug("bridge session ended: %s", e)
            except asyncio.CancelledError:
                raise
            except Exception:
                log.exception("bridge session crashed (will reconnect in 1s)")
            self.world.on_disconnected()
            self._writer = None
            self._reader = None
            if self._stopping.is_set():
                return
            await asyncio.sleep(1.0)

    async def stop(self) -> None:
        self._stopping.set()
        w = self._writer
        if w is not None:
            try:
                w.close()
                await w.wait_closed()
            except Exception:
                pass

    async def _session(self) -> None:
        log.info("connecting to L1 bridge at %s:%s ...", self.host, self.port)
        self._reader, self._writer = await asyncio.open_connection(self.host, self.port)
        self.world.on_connected()
        log.info("bridge connected")
        await self.send({"type": "hello", "proto": 1, "agent": "AIAmongUs 0.1.0"})

        ping_task = asyncio.create_task(self._ping_loop())
        try:
            while True:
                # 12s 无任何下行数据视为死链(服务端 5s 心跳 + 10Hz 状态,正常远低于该阈值)
                line = await asyncio.wait_for(self._reader.readline(), timeout=12.0)
                if not line:
                    raise ConnectionError("EOF from L1")
                text = line.decode("utf-8", errors="replace").strip()
                if not text:
                    continue
                try:
                    msg = json.loads(text)
                except json.JSONDecodeError:
                    log.warning("bad json from L1: %.120s", text)
                    continue
                if isinstance(msg, dict):
                    self.world.apply(msg)
        except asyncio.TimeoutError:
            raise ConnectionError("L1 silent for 12s (watchdog)")
        finally:
            ping_task.cancel()
            # 断连状态统一由 run() 收口;这里不重复调用(run() 一定会执行到)

    async def _ping_loop(self) -> None:
        while True:
            await asyncio.sleep(5.0)
            await self.send({"type": "ping", "ref": f"ping{next(self._ref_counter)}"})

    # ---------- 发送 ----------

    async def send(self, obj: dict) -> bool:
        w = self._writer
        if w is None or w.is_closing():
            return False
        try:
            w.write((json.dumps(obj, ensure_ascii=False) + "\n").encode("utf-8"))
            await w.drain()
            return True
        except Exception as e:
            log.debug("send failed: %s", e)
            return False

    async def send_command(self, obj: dict) -> bool:
        if "ref" not in obj:
            obj = dict(obj)
            # L1 的 ack 广播给所有客户端;用带前缀的 ref 避免与其它客户端(测试脚本)的整数 ref 撞号
            obj["ref"] = f"a{next(self._ref_counter)}"
        ok = await self.send(obj)
        if ok:
            self.world.acks.appendleft({"type": "cmd_sent", "cmd": obj.get("type"), "ref": obj.get("ref"),
                                        "t": time.time()})
        return ok
