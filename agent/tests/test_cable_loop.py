# -*- coding: utf-8 -*-
"""VB-CABLE 回路验证:XTTS wav 播放到 CABLE Input,同时从 CABLE Output 录音,检查能量与采样率。

前置:先跑 test_xfyun_auth.py 生成 data/a_xiaoyan.wav(依赖本机 XTTS 服务)。
"""
import queue, sys, time, wave
from pathlib import Path
import numpy as np
import sounddevice as sd

hostapis = sd.query_hostapis()
print("hostapis:", [(i, h["name"]) for i, h in enumerate(hostapis)])
cap = play = None
for i, d in enumerate(sd.query_devices()):
    if "CABLE Output" in d["name"] and d["max_input_channels"] > 0 and cap is None:
        cap = i
    if "CABLE Input" in d["name"] and "16ch" not in d["name"] and d["max_output_channels"] > 0 and play is None:
        play = i
print("capture dev:", cap, sd.query_devices(cap)["name"], "hostapi", sd.query_devices(cap)["hostapi"])
print("playback dev:", play, sd.query_devices(play)["name"], "hostapi", sd.query_devices(play)["hostapi"])

wav_path = Path(__file__).resolve().parent / "data" / "a_xiaoyan.wav"
if not wav_path.exists():
    print(f"[SKIP] 缺 {wav_path}(先跑 test_xfyun_auth.py 生成,需本机 XTTS 服务)")
    sys.exit(0)
with wave.open(str(wav_path), "rb") as w:
    data = np.frombuffer(w.readframes(w.getnframes()), dtype=np.int16)
sr = 16000

q = queue.Queue()
got = []

def cb(indata, frames, tt, status):
    got.append((int(np.abs(indata).max()), len(indata)))
    q.put(indata.copy())

# 尝试 48k 采集(WASAPI 混合格式),失败降级
import itertools
for ch, sr_in in [(2, 48000), (1, 48000), (16, 48000), (2, None)]:
    try:
        stream = sd.InputStream(device=cap, channels=ch, samplerate=sr_in, dtype="int16", callback=cb)
        stream.start()
        print(f"capture ok: ch={ch} sr={stream.samplerate}")
        break
    except Exception as e:
        print(f"capture fail ch={ch} sr={sr_in}: {str(e)[:100]}")
        stream = None
if stream is None:
    sys.exit(1)

time.sleep(0.5)
peak_before = max((g[0] for g in got), default=0)
print("播放前峰值:", peak_before)
sd.play(data, samplerate=sr, device=play)
t0 = time.time()
while time.time() - t0 < len(data) / sr + 1.0:
    time.sleep(0.2)
time.sleep(0.6)
stream.stop(); stream.close()
peaks = [g[0] for g in got]
print(f"总块数={len(peaks)} 采样率={stream.samplerate} 播放期间峰值={max(peaks)}")
ok = max(peaks) > 2000 and max(peaks) > peak_before * 3
print("CABLE 回路:", "PASS" if ok else "FAIL")
sys.exit(0 if ok else 1)
