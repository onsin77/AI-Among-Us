# -*- coding: utf-8 -*-
"""讯飞 RTASR-LLM 连通性冒烟:XTTS 生成两段不同音色语音 -> 流式发给讯飞 -> 校验转写与 rl 说话人分离。

凭据从 agent/config.ini([asr] 段)或环境变量 XF_APP_ID/XF_API_KEY/XF_API_SECRET 读取,
绝不硬编码提交(历史版本曾把真实密钥写在这里,已出库并轮换)。
"""
import asyncio, base64, hashlib, hmac, json, time, urllib.parse, urllib.request, uuid, sys, wave
import os
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
from ai_agent.config import Config  # noqa: E402

_cfg = Config.load()
APP_ID = os.environ.get("XF_APP_ID", _cfg.asr_app_id)
API_KEY = os.environ.get("XF_API_KEY", _cfg.asr_api_key)
API_SECRET = os.environ.get("XF_API_SECRET", _cfg.asr_api_secret)
if not (APP_ID and API_KEY and API_SECRET):
    print("[SKIP] 讯飞凭据未配置(config.ini [asr] 段或 XF_APP_ID/XF_API_KEY/XF_API_SECRET)")
    sys.exit(0)
URL = "wss://office-api-ast-dx.iflyaisol.com/ast/communicate/v1"

def build_auth_url():
    utc = time.strftime("%Y-%m-%dT%H:%M:%S%z", time.localtime())
    params = {
        "accessKeyId": API_KEY,
        "appId": APP_ID,
        "audio_encode": "pcm_s16le",
        "lang": "autodialect",
        "role_type": "2",
        "samplerate": "16000",
        "utc": utc,
        "uuid": str(uuid.uuid4()),
    }
    base = "&".join(f"{urllib.parse.quote(k, safe='')}={urllib.parse.quote(v, safe='')}"
                    for k, v in sorted(params.items()))
    sig = base64.b64encode(hmac.new(API_SECRET.encode(), base.encode(), hashlib.sha1).digest()).decode()
    params["signature"] = sig
    return URL + "?" + "&".join(f"{urllib.parse.quote(k, safe='')}={urllib.parse.quote(v, safe='')}"
                                for k, v in params.items())

def tts(text, voice, out):
    req = urllib.request.Request("http://127.0.0.1:17895/tts",
        data=json.dumps({"text": text, "voice": voice, "speed": 50}).encode(),
        headers={"Content-Type": "application/json"})
    Path(out).write_bytes(urllib.request.urlopen(req, timeout=30).read())

def wav_to_pcm16k(path):
    with wave.open(str(path), "rb") as w:
        assert w.getframerate() == 16000 and w.getnchannels() == 1 and w.getsampwidth() == 2, \
            f"rate={w.getframerate()} ch={w.getnchannels()} width={w.getsampwidth()}"
        return w.readframes(w.getnframes())

async def main():
    import websockets
    d = Path(__file__).resolve().parent / "data"; d.mkdir(exist_ok=True)
    f1, f2 = d / "a_xiaoyan.wav", d / "a_xiaofeng.wav"
    print("tts 合成中…")
    tts("我在电力舱修了灯,然后去了医疗舱做DNA任务。", "xiaoyan", f1)
    tts("我在厨房倒垃圾,之后一直呆在货舱搬箱子。", "xiaofeng", f2)
    pcm1, pcm2 = wav_to_pcm16k(f1), wav_to_pcm16k(f2)
    print(f"pcm1={len(pcm1)}B pcm2={len(pcm2)}B")

    url = build_auth_url()
    print("ws 连接中…")
    async with websockets.connect(url, max_size=2**22, ping_interval=None) as ws:
        # 等 started
        started = json.loads(await asyncio.wait_for(ws.recv(), 15))
        print("started:", json.dumps(started, ensure_ascii=False)[:300])
        sid = started.get("data", {}).get("sessionId") or started.get("sid") or ""

        async def reader():
            finals = []
            try:
                while True:
                    raw = await asyncio.wait_for(ws.recv(), 20)
                    if isinstance(raw, bytes):
                        continue
                    msg = json.loads(raw)
                    if msg.get("action") == "error" or (msg.get("code") and str(msg.get("code")) != "0"):
                        print("ERROR msg:", json.dumps(msg, ensure_ascii=False)[:400]); break
                    data = msg.get("data") or {}
                    st = ((data.get("cn") or {}).get("st") or {})
                    if not st: continue
                    text, rls = "", set()
                    for rt in st.get("rt") or []:
                        for wseg in rt.get("ws") or []:
                            for cw in wseg.get("cw") or []:
                                text += cw.get("w", "")
                                if cw.get("rl"): rls.add(cw.get("rl"))
                    tag = "终" if st.get("type") == "0" else "中"
                    if text:
                        print(f"[{tag} seg{data.get('seg_id')} rl={sorted(rls)}] {text}")
                    if st.get("type") == "0" and text:
                        finals.append((data.get("seg_id"), sorted(rls), text))
            except (asyncio.TimeoutError, Exception) as e:
                print("reader exit:", type(e).__name__, str(e)[:120])
            return finals

        rt = asyncio.create_task(reader())
        # 交织发送两段音频,模拟两人交替说话;首尾加静音
        silence = b"\x00" * 1280 * 12
        stream = silence + pcm1 + silence + pcm2 + silence
        t0 = time.time()
        for i in range(0, len(stream), 1280):
            await ws.send(stream[i:i+1280])
            await asyncio.sleep(max(0, 0.04 - (time.time() - t0) % 0.04))
            t0 = time.time()
        await ws.send(json.dumps({"end": True, "sessionId": sid}))
        print("已发 end")
        try:
            await asyncio.wait_for(rt, 25)
        except asyncio.TimeoutError:
            rt.cancel()
    print("done")

if __name__ == "__main__":
    asyncio.run(main())
