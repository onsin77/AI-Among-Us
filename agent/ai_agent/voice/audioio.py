"""VB-CABLE 音频采集/回输(sounddevice/PortAudio,线程回调 → asyncio 队列)。

链路约定(单根 VB-CABLE):
- YY 播放设备 = CABLE Input → 别人的语音进入虚拟线
- 本程序从 CABLE Output 录音(48k 立体声)→ 降混重采样 16k mono → 讯飞
- 本程序 TTS 播放到 CABLE Input → CABLE Output → YY 麦克风(YY 录音设备 = CABLE Output)
- 我方播放期间跳过 ASR 上行,避免把自己转写进去

设备选择:按名字匹配 capture/play 关键字,hostapi 优先 WASAPI → DirectSound → MME。
"""

from __future__ import annotations

import asyncio
import logging
import threading
import time
import wave
from io import BytesIO
from pathlib import Path

import numpy as np

log = logging.getLogger("ai_agent.audioio")

try:
    import sounddevice as sd
    HAS_SD = True
except Exception:  # 没装 sounddevice:语音功能整体禁用,但不影响 agent 其它部分
    sd = None
    HAS_SD = False

CAPTURE_SR = 48000
TARGET_SR = 16000
DECIM = CAPTURE_SR // TARGET_SR     # 3
HOSTAPI_PREF = ["WASAPI", "DirectSound", "MME", "WDM-KS"]


def list_devices() -> list[dict]:
    """枚举输入/输出设备(面板展示与诊断)。"""
    if not HAS_SD:
        return []
    out = []
    try:
        hostapis = sd.query_hostapis()
        for i, d in enumerate(sd.query_devices()):
            out.append({
                "id": i, "name": d["name"],
                "hostapi": hostapis[d["hostapi"]]["name"] if d["hostapi"] < len(hostapis) else "?",
                "in": d["max_input_channels"], "out": d["max_output_channels"],
                "default_sr": d["default_samplerate"],
            })
    except Exception as e:
        log.warning("list_devices failed: %s", e)
    return out


def find_device(name_part: str, want_input: bool) -> int | None:
    """按名字子串找设备;hostapi 按偏好排序,同 hostapi 取第一个匹配。"""
    if not HAS_SD:
        return None
    try:
        hostapis = sd.query_hostapis()
        pref = {name: idx for idx, name in enumerate(HOSTAPI_PREF)}
        candidates = []
        for i, d in enumerate(sd.query_devices()):
            if name_part.lower() not in d["name"].lower():
                continue
            if want_input and d["max_input_channels"] <= 0:
                continue
            if not want_input and d["max_output_channels"] <= 0:
                continue
            api = hostapis[d["hostapi"]]["name"]
            candidates.append((pref.get(api, 99), i))
        if not candidates:
            return None
        candidates.sort()
        return candidates[0][1]
    except Exception as e:
        log.warning("find_device(%s) failed: %s", name_part, e)
        return None


class CableCapture:
    """从虚拟线录音端采集,输出 16k mono s16le,每 40ms 一块(1280B)。"""

    def __init__(self, device_name: str = "CABLE Output") -> None:
        self.device_name = device_name
        self.device: int | None = None
        self._stream = None
        self._loop: asyncio.AbstractEventLoop | None = None
        self._queue: asyncio.Queue[bytes] = asyncio.Queue(maxsize=300)
        self.peak = 0                # 最近 ~1s 峰值(0-32767),面板音量条
        self._peaks: list[int] = []
        self.running = False
        self.last_error: str | None = None

    async def start(self) -> bool:
        if not HAS_SD:
            self.last_error = "sounddevice 未安装(pip install sounddevice)"
            return False
        self.device = find_device(self.device_name, want_input=True)
        if self.device is None:
            self.last_error = f"找不到录音设备:{self.device_name}"
            return False
        self._loop = asyncio.get_running_loop()
        loop_put = self._queue.put_nowait

        def callback(indata, frames, time_info, status):
            # indata: (frames, ch) int16 @48k → mono 平均 → 3:1 降采样
            try:
                arr = np.asarray(indata, dtype=np.int16)
                mono = arr.mean(axis=1) if arr.ndim == 2 else arr
                n = (len(mono) // DECIM) * DECIM
                if n > 0:
                    small = mono[:n].reshape(-1, DECIM).mean(axis=1).astype(np.int16)
                    self._peaks.append(int(np.abs(mono).max()))
                    if len(self._peaks) > 100:
                        del self._peaks[:50]
                    self.peak = max(self._peaks[-25:])
                    data = small.tobytes()
                    def _put():
                        try:
                            loop_put(data)
                        except asyncio.QueueFull:
                            try:
                                self._queue.get_nowait()
                                loop_put(data)
                            except asyncio.QueueEmpty:
                                pass
                    self._loop.call_soon_threadsafe(_put)
            except Exception as e:  # 回调里绝不让异常外泄(PortAudio 会崩)
                self.last_error = f"capture cb: {e}"

        last_err = None
        for ch in (2, 1):
            for blocksize in (1920, 0):    # 1920 帧 = 40ms@48k
                try:
                    self._stream = sd.InputStream(
                        device=self.device, channels=ch, samplerate=CAPTURE_SR,
                        dtype="int16", blocksize=blocksize, callback=callback)
                    self._stream.start()
                    self.running = True
                    log.info("capture started dev=%s(%s) ch=%d bs=%d", self.device, self.device_name, ch, blocksize)
                    return True
                except Exception as e:
                    last_err = e
        self.last_error = f"打开录音设备失败: {last_err}"
        return False

    def stop(self) -> None:
        self.running = False
        if self._stream is not None:
            try:
                self._stream.stop()
                self._stream.close()
            except Exception:
                pass
            self._stream = None

    def drain(self) -> None:
        """丢弃采集队列里的积压帧(会议切换时防上一场的尾巴喂进新 ASR 会话)。"""
        n = 0
        while True:
            try:
                self._queue.get_nowait()
                n += 1
            except asyncio.QueueEmpty:
                break
        if n:
            log.info("capture drained %d stale frames", n)

    async def read(self) -> bytes | None:
        """取一帧 16k pcm(约 40ms);无数据立即返回 None。"""
        try:
            return self._queue.get_nowait()
        except asyncio.QueueEmpty:
            return None


class CablePlayer:
    """把 16k mono wav/pcm 播放到虚拟线播放端(YY 从线另一头拾取)。"""

    def __init__(self, device_name: str = "CABLE Input") -> None:
        self.device_name = device_name
        self.device: int | None = None
        self.speaking = False
        self._done_event = threading.Event()
        self._stream = None
        self.last_error: str | None = None
        self._lock = threading.Lock()

    def _ensure_device(self) -> bool:
        if not HAS_SD:
            self.last_error = "sounddevice 未安装"
            return False
        if self.device is None:
            self.device = find_device(self.device_name, want_input=False)
            if self.device is None:
                self.last_error = f"找不到播放设备:{self.device_name}"
                return False
        return True

    def play_pcm(self, pcm: bytes, samplerate: int = 16000) -> bool:
        """同步播放(内部线程化);speaking 标志在播放期间为真。非阻塞返回。"""
        with self._lock:
            if self.speaking:
                self.last_error = "busy: 上一段还没播完"
                return False
            if not self._ensure_device():
                return False
            data = np.frombuffer(pcm, dtype=np.int16)
            if len(data) == 0:
                return False
            self._done_event = threading.Event()
            self.speaking = True
            threading.Thread(target=self._play_blocking, args=(data, samplerate), daemon=True).start()
            return True

    def play_wav_bytes(self, wav_bytes: bytes) -> bool:
        pcm, sr = wav_to_pcm(wav_bytes)
        if pcm is None:
            self.last_error = "wav 解析失败"
            return False
        return self.play_pcm(pcm, sr)

    def _play_blocking(self, data: np.ndarray, samplerate: int) -> None:
        try:
            event = self._done_event
            stream = sd.OutputStream(
                device=self.device, channels=1, samplerate=samplerate,
                dtype="int16",
                finished_callback=lambda: event.set())
            self._stream = stream
            with stream:
                stream.write(data)
            event.wait(timeout=1.0)
        except Exception as e:
            self.last_error = f"play: {e}"
            log.warning("playback failed: %s", e)
        finally:
            self._stream = None
            self.speaking = False

    def wait(self, timeout: float = 30.0) -> bool:
        return self._done_event.wait(timeout=timeout)

    def stop(self) -> None:
        s = self._stream
        if s is not None:
            try:
                s.abort()          # 立即掐断未播完的缓冲
            except Exception:
                pass
        # 先 abort 再降标志:顺序反了会出现"标志已降但缓冲还在响"的窗口,解禁后自识别
        self.speaking = False


def wav_to_pcm(wav_bytes: bytes) -> tuple[bytes | None, int]:
    """从 wav 字节抽 pcm 与采样率(仅支持 16bit;声道转 mono)。"""
    try:
        with wave.open(BytesIO(wav_bytes), "rb") as w:
            sr, ch, sw = w.getframerate(), w.getnchannels(), w.getsampwidth()
            if sw != 2:
                return None, 0
            raw = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)
            if ch > 1:
                raw = raw.reshape(-1, ch).mean(axis=1).astype(np.int16)
            return raw.tobytes(), sr
    except Exception:
        return None, 0
