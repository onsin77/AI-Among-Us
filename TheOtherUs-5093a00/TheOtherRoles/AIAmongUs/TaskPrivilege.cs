using System;
using UnityEngine;

namespace AIAmongUs;

/// <summary>
/// 自动完成任务特权(说明书 3.3):
/// 校验 task ∈ me.tasks → 寻路至当前阶段 console → console.Use() 真实开面板
/// → 面板存活 10s 计时(计时器挂在 Minigame.Instance 生命周期上)
/// → NextStep() 一次只推一步 → 多阶段任务走向下一台重复 → 事件全程上报。
/// 禁止远程完成:必须人在任务点(usableDistance 内)+ 面板已打开才计时。
/// </summary>
internal static class TaskPrivilege
{
    private enum Stage { Idle, WaitPanel, Walk, Opening, Timing, StepDone, Aborted, Done }

    private static Stage _stage;
    private static uint _taskId;
    private static string _refJson;
    private static float _timer;
    private static int _stepsDone;
    private static int _lastMinigameFrameSeen;
    private static int _correctTries;
    private static int _reopens;     // 面板中途被关的重开次数(防"开了即关"死循环)
    private static bool _relaxedApproach;
    private static Minigame _opened;   // 仅用于关闭(存活判定仍用 Minigame.Instance)
    private static PlayerTask _task;
    private static string _abortReason;

    private const float HoldSeconds = 10f;

    public static bool Busy => _stage != Stage.Idle;

    public static string Start(uint taskId, string refJson)
    {
        if (_stage != Stage.Idle)
        {
            AIBridgePlugin.Log?.LogInfo($"open_task busy: stage={_stage} cur_task={_taskId} want={taskId}");
            return "busy";
        }
        var me = PlayerControl.LocalPlayer;
        if (me == null || me.Data == null) return "no_player";
        if (!AIAmongUs.AIHub.IsPlayPhaseNow) return "phase_guard";
        if (me.Data.IsDead) return "dead";
        if (Minigame.Instance != null) return "panel_open";

        // 边界1:只允许完成自己任务列表里的任务
        _task = null;
        var tasks = me.myTasks;
        for (var i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            if (t != null && t.Id == taskId) { _task = t; break; }
        }
        if (_task == null) return "task_not_mine";
        if (_task.IsComplete) return "task_done_already";
        // 边界1b:本地图无该任务台(如 Airship 无扫描仪的 SubmitScan)——立即拒绝,不发起走
        if (!CurrentConsolePos().HasValue) return "task_no_console";

        _taskId = taskId;
        _refJson = refJson;
        _stepsDone = 0;
        _correctTries = 0;
        _reopens = 0;
        _abortReason = null;
        EnterWalkStage();
        return null;
    }

    public static void Cancel(string reason)
    {
        if (_stage == Stage.Idle) return;
        _abortReason = reason;
        Finish(Stage.Aborted);
    }

    private static void EnterWalkStage()
    {
        CloseAnyMinigame();   // 防御:上一步面板未彻底销毁会触发 move_panel_open
        // Close() 是异步销毁(0.75s 动画),立即 MoveTo 会看到 panel_open;等销毁完成再走
        if (Minigame.Instance != null)
        {
            _stage = Stage.WaitPanel;
            _timer = 0f;
            return;
        }
        // 当前阶段的合法任务台:优先拿 console 对象(用其实际 UsableDistance 定接近距离)
        var c0 = NearestValidConsole();
        var pos = c0 != null ? (Vector2)((Component)c0).transform.position : CurrentConsolePos();
        if (!pos.HasValue)
        {
            _abortReason = "no_console_for_step";
            Finish(Stage.Aborted);
            return;
        }
        var me = PlayerControl.LocalPlayer;
        var useRange = c0 != null ? Mathf.Max(0.4f, c0.UsableDistance - 0.02f) : 0.75f;
        var approach = Navigator.ConsoleApproach(pos.Value, me.GetTruePosition(), useRange);
        var relaxed = false;
        if (!approach.HasValue)
        {
            // 兜底:台周 2.6u 内取最近可达格(贴墙台/桌心按钮等;我们直接实例化面板,不依赖 CanUse 距离)
            approach = Navigator.ConsoleApproach(pos.Value, me.GetTruePosition(), 2.6f);
            relaxed = approach.HasValue;
        }
        if (!approach.HasValue)
        {
            _abortReason = $"no_approach(console@{pos.Value.x:F1},{pos.Value.y:F1} range={useRange:F2})";
            Finish(Stage.Aborted);
            return;
        }
        _relaxedApproach = relaxed;
        var err = Navigator.MoveTo(approach.Value, $"task_{_taskId}_console", relaxed ? 0.5f : 0.25f);
        if (err != null)
        {
            _abortReason = $"move_{err}(console@{pos.Value.x:F1},{pos.Value.y:F1})";
            Finish(Stage.Aborted);
            return;
        }
        _stage = Stage.Walk;
        EventTap.Emit("TASK_START", null, null,
            $"task={_taskId}({_task.TaskType}) console@{pos.Value.x:F1},{pos.Value.y:F1} range={useRange:F2}");
    }

    private static Vector2? CurrentConsolePos()
    {
        try
        {
            var list = _task.FindValidConsolesPositions();
            if (list == null || list.Count == 0) return null;
            var me = PlayerControl.LocalPlayer;
            var myPos = me.GetTruePosition();
            var best = list[0]; var bestD = float.MaxValue;
            for (var i = 0; i < list.Count; i++)
            {
                var d = Vector2.Distance(list[i], myPos);
                if (d < bestD) { best = list[i]; bestD = d; }
            }
            return best;
        }
        catch { return null; }
    }

    private static Console NearestValidConsole()
    {
        var ship = ShipStatus.Instance;
        if (ship?.AllConsoles == null) return null;
        var me = PlayerControl.LocalPlayer;
        var myPos = me.GetTruePosition();
        Console best = null; var bestD = float.MaxValue;
        var all = ship.AllConsoles;
        for (var i = 0; i < all.Length; i++)
        {
            var c = all[i];
            if (c == null) continue;
            try { if (!_task.ValidConsole(c)) continue; } catch { continue; }
            var d = Vector2.Distance(((Component)c).transform.position, myPos);
            if (d < bestD) { best = c; bestD = d; }
        }
        return best;
    }

    public static void Tick(float dt)
    {
        switch (_stage)
        {
            case Stage.WaitPanel:
                _timer += dt;
                if (Minigame.Instance == null) { EnterWalkStage(); return; }
                if (_timer > 2.5f) { _abortReason = "panel_close_timeout"; Finish(Stage.Aborted); }
                return;

            case Stage.Walk:
                if (Navigator.Active) return; // 还在走
                if (Navigator.LastMoveResult == "MOVE_DONE")
                {
                    // 到达:验证距离并开面板
                    var c = NearestValidConsole();
                    if (c == null) { _abortReason = "console_lost"; Finish(Stage.Aborted); return; }
                    var cpos = (Vector2)((Component)c).transform.position;
                    var d = Vector2.Distance(cpos, PlayerControl.LocalPlayer.GetTruePosition());
                    var okDist2 = _relaxedApproach ? 2.7f : c.UsableDistance + 0.30f;
                    if (d > okDist2)
                    {
                        // 容差/量化偏差导致超距:补一次精确走位(最多 2 次),仍不行才放弃
                        if (_correctTries < 2)
                        {
                            _correctTries++;
                            var near = Navigator.ConsoleApproach(cpos, PlayerControl.LocalPlayer.GetTruePosition(), Mathf.Max(0.3f, c.UsableDistance - 0.02f));
                            if (near.HasValue && Navigator.MoveTo(near.Value, $"task_{_taskId}_fixpos", 0.12f) == null)
                            {
                                _stage = Stage.Walk;
                                return;
                            }
                        }
                        _abortReason = $"too_far:{d:F1}/{c.UsableDistance:F2}";
                        Finish(Stage.Aborted);
                        return;
                    }
                    // 不能用 c.Use():它按 FindTask 开"该台第一项未完成任务"的迷你游戏,
                    // 台上若有多项任务会开出别人的面板(实测 panel_did_not_open)。
                    // 直接复刻 Console.Use 的实例化流程,但绑定我们自己的任务。
                    try
                    {
                        var prefab = _task.GetMinigamePrefab();
                        if (prefab == null) { _abortReason = "no_minigame_prefab"; Finish(Stage.Aborted); return; }
                        var mg2 = UObject.Instantiate(prefab);
                        mg2.transform.SetParent(Camera.main.transform, false);
                        mg2.transform.localPosition = new Vector3(0f, 0f, -50f);
                        mg2.Console = c;
                        mg2.Begin(_task);
                        _opened = mg2;
                        AIBridgePlugin.Log?.LogInfo(
                            "task open: prefab=" + prefab.GetType().Name + " want=" + _taskId);
                    }
                    catch (Exception e) { _abortReason = $"use_fail:{e.Message}"; Finish(Stage.Aborted); return; }
                    _stage = Stage.Opening;
                    _timer = 0f;
                }
                else
                {
                    _abortReason = $"walk_{Navigator.LastMoveResult ?? "stopped"}";
                    Finish(Stage.Aborted);
                }
                return;

            case Stage.Opening:
                // 等待面板出现(部分 minigame 的 MyTask 为空,不做任务 ID 匹配)
                _timer += dt;
                var mgO = Minigame.Instance;
                if (mgO != null)
                {
                    _stage = Stage.Timing;
                    _timer = 0f;
                    EventTap.Emit("TASK_PANEL_OPEN", null, null, $"task={_taskId}");
                    return;
                }
                if (_timer > 3f)
                {
                    AIBridgePlugin.Log?.LogWarning(
                        $"panel_did_not_open: inst={(Minigame.Instance != null)} want={_taskId}");
                    var c2 = NearestValidConsole();
                    var dd = c2 != null
                        ? Vector2.Distance(((Component)c2).transform.position, PlayerControl.LocalPlayer.GetTruePosition())
                        : -1f;
                    _abortReason = $"panel_did_not_open(d={dd:F1})";
                    Finish(Stage.Aborted);
                }
                return;

            case Stage.Timing:
                // 边界2/3:面板被关(含别人视角无关的本地关闭)即取消计时
                if (Minigame.Instance == null)
                {
                    _abortReason = "panel_closed_early";
                    EventTap.Emit("TASK_ABORT", null, null, $"task={_taskId} reason=panel_closed");
                    // 面板中途被关:回走到任务台重开(外观与真人一致);但"开了即关"式的
                    // 异常 minigame 会 Walk→Opening→Timing→closed 无限循环,必须设上限
                    if (++_reopens > 3)
                    {
                        Finish(Stage.Aborted);
                        return;
                    }
                    EnterWalkStage();
                    return;
                }
                _timer += dt;
                _lastMinigameFrameSeen = Time.frameCount;
                if (_timer < HoldSeconds) return;

                // 到点:推进一步(NextStep 到最终步时内部自动 RpcCompleteTask 同步 host)
                try
                {
                    var npt = _task.TryCast<NormalPlayerTask>();
                    if (npt != null) npt.NextStep();
                    else _task.Complete();
                }
                catch (Exception e) { _abortReason = $"step_fail:{e.Message}"; Finish(Stage.Aborted); return; }

                _stepsDone++;
                _reopens = 0;   // 成功推进一步:重开计数重新起算(多阶段任务合法多次开面板)
                var done = false;
                try { done = _task.IsComplete; } catch { }
                EventTap.Emit("TASK_STEP", null, null, $"task={_taskId} step#{_stepsDone} done={done}");

                // 关面板,进入下一阶段或收尾
                CloseAnyMinigame();
                if (done) { Finish(Stage.Done); return; }
                _stage = Stage.StepDone;
                _timer = 0f;
                return;

            case Stage.StepDone:
                // 歇 1.2s 再走向下一阶段任务台(拟真节奏)
                _timer += dt;
                if (_timer >= 1.2f) EnterWalkStage();
                return;
        }
    }

    /// <summary>关闭所有遗留迷你游戏(Instance 与自持引用都关;freeplay 下面板都是我们开的)。</summary>
    private static void CloseAnyMinigame()
    {
        try { if (Minigame.Instance != null) Minigame.Instance.Close(); } catch { }
        try { if (_opened != null) _opened.Close(); } catch { }
        _opened = null;
    }

    private static void Finish(Stage end)
    {
        if (end == Stage.Aborted)
        {
            CloseAnyMinigame();
        }
        var code = end == Stage.Done ? "TASK_FLOW_DONE" : "TASK_FLOW_ABORT";
        var note = end == Stage.Done
            ? $"task={_taskId} steps={_stepsDone}"
            : $"task={_taskId} reason={_abortReason}";
        var wasRef = _refJson;
        var ok = end == Stage.Done;
        _stage = Stage.Idle;
        _task = null;
        _refJson = null;
        Navigator.Stop("task_flow_end");
        EventTap.Emit(code, null, null, note);
        BridgeServer.EnqueueReliable(Ack(wasRef, "open_task", ok, ok ? null : _abortReason));
    }

    private static string Ack(string refJson, string cmd, bool ok, string err)
    {
        var sb = new System.Text.StringBuilder(96);
        sb.Append("{\"type\":\"ack\"");
        if (refJson != null) sb.Append(",\"ref\":").Append(refJson);
        sb.Append(",\"cmd\":").Append(MiniJson.Str(cmd));
        sb.Append(",\"ok\":").Append(MiniJson.B(ok));
        if (err != null) sb.Append(",\"err\":").Append(MiniJson.Str(err));
        sb.Append('}');
        return sb.ToString();
    }
}
