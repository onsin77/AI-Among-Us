@echo off
rem ==========================================================================
rem  AIAmongUs L3 启动脚本(双击或命令行运行)
rem  开关说明:
rem    --mode monitor | crew | impostor | auto
rem        monitor  只监控(默认,不自动行动)
rem        crew     好人:自动跑图做任务;附近有尸体自动报告
rem        impostor 坏人:巡逻 + 接近并击杀;刀完人/任何情况都不报警
rem        auto     按 L1 状态里的角色自动切换
rem    --meeting-file PATH   会议聊天输入 JSONL(大家发言从这里读)
rem    --meeting-out PATH    会议 AI 输出复盘 JSONL
rem    --no-meeting          关闭会议 AI
rem    --model fast|deep     会议推理档位
rem    --autostart freeplay  启动后自动开一局(需 Steam 在线)
rem    --map 4               autostart 地图(4=Airship)
rem    --selftest-model      只测 DeepSeek 连通性/schema 后退出
rem  面板总控:http://127.0.0.1:8765 左列「脚本总控」可随时开始/停止接管、关闭脚本;
rem           会议发言:右侧「会议 AI」或 python tools/meeting_console.py
rem  示例:
rem    纯监控:            start_ai.bat
rem    好人自动任务:      start_ai.bat --mode crew --autostart freeplay
rem    坏人:              start_ai.bat --mode impostor --autostart freeplay
rem    自动 + 会议模拟:    start_ai.bat --mode auto --autostart freeplay
rem ==========================================================================
setlocal
cd /d "%~dp0"
python -m ai_agent.main %*
endlocal
