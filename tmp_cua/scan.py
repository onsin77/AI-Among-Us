import json, subprocess, time, urllib.request

def phase():
    try:
        d = json.load(urllib.request.urlopen("http://127.0.0.1:8765/api/state", timeout=3))
        s = d["state"]
        return s.get("phase"), s.get("map")
    except Exception as e:
        return f"ERR:{e}", None

def click(x, y):
    subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File",
                    "D:/games/au/AI among us/tmp_cua/click.ps1", "-x", str(x), "-y", str(y)],
                   capture_output=True, timeout=15)

# 网格扫描地图卡片区域(左侧列 + 中部列)
points = []
for y in range(200, 900, 110):
    points.append((115, y))
for y in range(200, 900, 180):
    points.append((300, y))

for x, y in points:
    p, m = phase()
    if p != "MENU":
        print(f"PHASE CHANGED to {p} (map={m}) before click {x},{y}")
        break
    click(x, y)
    time.sleep(1.6)
    p, m = phase()
    print(f"clicked ({x},{y}) -> phase={p} map={m}")
    if p != "MENU":
        print("*** GAME STARTED ***")
        break
