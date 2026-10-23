# AI-Among-Us

给 TheOtherUs mod(v1.2.9.8,游戏 Among Us 2023.11.28 Steam IL2CPP)接入 AI 玩家。
AI 能像真人一样跑图做任务、巡逻、击杀、报尸、开会发言(文字+真人语音)、投票,
还能自己开房、补 bot、连续打完一局又一局。

## 功能总览

| 里程碑 | 内容 |
|---|---|
| 迭代 1 | L1 游戏内采集(10Hz full/delta)+ TCP 桥 + L3 监控面板 |
| 迭代 2 | 控制指令全套 + 自动任务特权(站台 10s 拟真计时)+ 飞艇导航 |
| 迭代 3 | 规则行为引擎(crew/impostor/auto)+ 会议文件管线(DeepSeek)+ 可视化面板 |
| 迭代 4 | 语音会议(讯飞 RTASR-LLM 音色分离 + 轮麦状态机 + XTTS 回输)+ 会议决策 + 跨局记忆 |
| 迭代 4.1 | 整局自托管(开房/bot 补位/会议兜底/强制结算/连续对局) |
| 迭代 5 | 公平性守卫 + 全角色技能自动驾驶 + 分发整理(MVID 联机校验) |
| 2026-10-23 | 全库排查加固:安全/健壮性约 40 项修复(详见 `docs/迭代报告/全库排查_2026-10-23.md`) |

导航为实测定制形态(0.25m 栅格 + 胶囊边验证 + 门盘强制 + 锚点剪枝 + 梯子/飞艇移动平台特殊边),
七个"想当然"方案已证伪记录在案,详见 `docs/迭代报告/迭代2_控制指令与自动任务.md`。

## 三层架构

```
┌─ L1 游戏内插件 ────────────────────────────────┐
│ TheOtherUs-5093a00/TheOtherRoles/AIAmongUs/    │  BepInEx 6 IL2CPP,并入 TheOtherUs 程序集
│ 状态采集 10Hz / 导航寻路 / 指令执行 / 事件捕获  │
└───────────────┬────────────────────────────────┘
                │ TCP 127.0.0.1:47600(JSON 行协议,ack 带 ref)
┌───────────────┴────────────────────────────────┐
│ L3 AI 大脑  agent/                              │  Python 3.10+
│ 行为引擎 / 会议决策(DeepSeek) / 语音会议 / 托管 │  FastAPI 面板 http://127.0.0.1:8765
└─────────────────────────────────────────────────┘
```

- **L1**:Harmony patch 挂 `AmongUsClient.Update`(主循环)/`PlayerPhysics.FixedUpdate`(移动)/
  `ChatController.AddChat`(聊天);只读采集 + 指令执行,不改 mod 任何既有文件。
- **L2**:多客户端广播;可靠消息(事件/ack)必达,状态快照新覆盖旧;SendTimeout+静默看门狗。
- **L3**:行为引擎纯规则不接模型(坏人**永不报警**);会议发言/表决走 DeepSeek(fast/deep 双档);
  语音链 = VB-CABLE 采集 → 讯飞转写(rl 音色分离)→ 轮麦状态机 → XTTS 合成回输
  (AI 播报期间自动对 ASR 喂静音防自识别)。

## 目录结构

```
TheOtherUs-5093a00/TheOtherRoles/AIAmongUs/   # L1 插件源码(10 个 .cs)
  ├─ AIBridgePlugin/AIHub/AIPatches           #   入口/主循环/patch
  ├─ BridgeServer/MiniJson                    #   TCP 服务端/零依赖 JSON(深度受限)
  ├─ StateCollector/EventTap                  #   状态采集/事件捕获
  ├─ Navigator                                #   导航(栅格/A*/特殊边)
  └─ CommandExecutor/TaskPrivilege            #   指令执行/自动任务状态机
agent/
  ├─ ai_agent/                                # L3(bridge/world/behavior/meeting/session/...)
  │   └─ voice/                               #   语音会议(xfasr/xtts/audioio/pipeline)
  ├─ config.example.ini                       # 配置模板 → 复制为 config.ini 填凭据
  ├─ tests/                                   # 离线测试 + 需游戏的验收脚本
  ├─ tools/meeting_console.py                 # 会议发言控制台
  └─ start_ai.bat                             # 一键启动(monitor/crew/impostor/auto)
dist/TheOtherUs.dll                           # 分发交付物(所有玩家同一文件)
aubody/                                       # 本机游戏目录(不入库)
docs/                                         # 设计文档/接口说明/全部迭代报告
```

## 快速开始(AI 玩家本人)

> 前置:**Steam 必须在线**(实测 EOS 认证失败会让本地 UDP 握手静默卡死)。

1. 用 `aubody/Among Us.exe` 启动游戏(已部署好 BepInEx + 本项目 dll)。
2. 配置:`cp agent/config.example.ini agent/config.ini`,填入 DeepSeek key
   (语音另需讯飞 RTASR 凭据)。config.ini 已被 .gitignore 排除,也可用环境变量
   `DEEPSEEK_API_KEY` / `XF_APP_ID` / `XF_API_KEY` / `XF_API_SECRET` 覆盖。
3. 启动大脑:`cd agent && python -m ai_agent.main`(或双击 `start_ai.bat`);
   依赖:`pip install -r requirements.txt`(语音另需 `sounddevice numpy websockets`)。
4. 打开面板 http://127.0.0.1:8765 → 左列「脚本总控」→ start(可选 autostart 自动开一局
   freeplay)。模式:`monitor` 只看 / `crew` 做任务+报尸 / `impostor` 巡逻击杀(永不报警) /
   `auto` 按角色切换。stop 会让角色原地停下;会议发言可用右侧「会议 AI」或
   `python tools/meeting_console.py`。

依赖本机服务的可选功能:XTTS 语音服务(默认 `http://127.0.0.1:17895`)+ VB-CABLE 虚拟声卡 +
YY(语音会议),见 `docs/语音会议使用说明.md`。

## 构建与测试(L1 mod)

```bash
# 杀游戏(dll 被占用) → 构建 → 部署(重试) → 启动游戏 → 验收
powershell -Command "Get-Process 'Among Us' -ErrorAction SilentlyContinue | Stop-Process -Force"; sleep 8
cd TheOtherUs-5093a00/TheOtherRoles && dotnet build -c Release TheOtherRoles.csproj
cp bin/Release/net6.0/TheOtherUs.dll ../../aubody/BepInEx/plugins/   # 失败就等几秒重试
cd ../../aubody && ./"Among Us.exe"                                   # 需 Steam 在线
cd ../agent && python tests/test_iter2.py                             # 游戏内验收
```

**离线测试**(无需游戏,改完即跑):

```bash
cd agent
python tests/test_behavior.py           # 行为引擎规则(坏人永不报警等)
python tests/test_meeting_pipeline.py   # 会议文件管线
python tests/test_voice_pipeline.py     # 语音轮麦/表决/绑定(32 项)
```

需游戏/凭据的验收脚本(autoplay/selfhost/meeting_ingame/voice_e2e 等)同目录,按文件头说明使用。

## 分发与联机(重要)

TheOtherUs 联机时做 **version + MVID 双重校验**(MVID 每次编译随机变化):
任何一名玩家与房主的 dll 不一致,房主的开始按钮就会被禁用。
所以**所有玩家必须使用 `dist/TheOtherUs.dll` 这同一个文件**——包括已装官方 TheOtherUs 的人。
换 dll 后如何分发、哪些文件要拷,见 `dist/分发说明.md`。

## 公平性边界(AI 不开挂)

除"自动做任务"(人在台前+面板开 10 秒计时)外,一切动作走与真人按键等价的通道并带守卫:
击杀按房间 KillDistance 档位限距;报尸需 5.5u 内且视线无遮挡;修灯/通讯/反应堆必须站在对应台旁;
钻管道要有管道权限;职业技能冷却/可用性全按 mod 按钮判定;紧急会议只能由真人合法触发
(`debug_call_meeting`/`spawn_bots` 仅限本地自托管局)。行为引擎硬规则:**坏人任何情况下不报警**。

## 文档索引

- `docs/01_总体设计文档.md` — 三层架构/协议 schema/迭代规划
- `docs/00_源码导读笔记.md` — 游戏源码导读 + AI 接入点速查
- `docs/启动与会议接口.md` — 面板/指令/会议输入输出速查(`docs/会议接口说明.md` 为早期归档)
- `docs/语音会议使用说明.md` — VB-CABLE/YY/XTTS 链路配置
- `docs/迭代报告/` — 迭代 1-5 与全库排查的完整改动清单、自查报告、实测数据

## 已知限制

- 会议内需要选人弹窗的多步技能尚未自动化;语音会议的真人局节奏(YY 实战)待校准。
- 飞艇 GapRoom 移动平台路线偶发卡死(同路线重试常自愈),专项待做。
- 自由练习模式假人不会聊天/投票,会议内容需文件喂入或走真人语音(见语音会议文档)。
