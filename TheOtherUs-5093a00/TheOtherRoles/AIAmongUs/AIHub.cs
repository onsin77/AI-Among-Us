using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

namespace AIAmongUs;

public enum AIPhase { MENU, LOBBY, PLAYING, MEETING, ENDED }

/// <summary>
/// 桥接中枢(纯逻辑,无 MonoBehaviour):
/// 由 AIPatches 挂在 InnerNetClient.FixedUpdate 上的 postfix 驱动(主线程、全场景存活)。
/// 曾用 ClassInjector 注入 MonoBehaviour,实测在场景切换时被销毁(迭代1日志实锤),弃用。
/// 职责:phase 状态机、10Hz 采集(full/delta)、下行指令消费、游戏时钟与轮次。
/// </summary>
internal static class AIHub
{
    private const float CollectInterval = 0.1f;   // 10Hz
    private const float FullInterval = 1.0f;      // 全量快照自愈周期

    private static readonly StateCollector Collector = new();
    private static readonly Dictionary<string, string> PrevFields = new();

    private static AIPhase _phase = AIPhase.MENU;
    private static bool _forceFull = true;
    private static bool _clockRunning;
    private static float _gameStartClock;
    private static float _frozenGameTime;
    private static int _meetingsStarted;
    private static int _tick;

    private static float _collectAcc;
    private static float _fullAcc;
    private static float _bodiesAcc;
    private static float _heartbeatAcc;
    private static float _resultsTimer;
    private static readonly List<DeadBody> Bodies = new();

    private static int _badJsonLogged;
    private static bool _loggedFirstTick;

    public static AIPhase CurrentPhase => _phase;
    public static float GameTime => _clockRunning ? Time.realtimeSinceStartup - _gameStartClock : _frozenGameTime;
    public static int Round => _meetingsStarted + 1;

    // 供兄弟模块访问(避免各处重复判 phase)
    internal static bool IsPlayPhaseNow => _phase is AIPhase.PLAYING or AIPhase.MEETING;
    internal static float GameTimeNow => GameTime;
    internal static int RoundNow => Round;

    /// <summary>每帧由 FixedUpdate postfix 调用(主线程)。</summary>
    public static void Tick(float dt)
    {
        _heartbeatAcc += dt;
        if (_heartbeatAcc >= 5f)
        {
            _heartbeatAcc = 0f;
            AIBridgePlugin.Log?.LogInfo($"AIHub alive: phase={_phase} tick={_tick} clients={BridgeServer.ClientCount}");
        }

        _collectAcc += dt;
        _fullAcc += dt;
        _bodiesAcc += dt;

        if (_bodiesAcc >= 0.5f)
        {
            _bodiesAcc = 0f;
            RefreshBodies();
        }

        var guard = 0;
        while (_collectAcc >= CollectInterval && guard++ < 5)
        {
            _collectAcc -= CollectInterval;
            CollectTick();
        }
        if (guard >= 5) _collectAcc = 0f; // 卡帧后丢弃积压,避免雪崩

        // 每帧驱动:导航跟随(方向计算)与任务特权状态机
        Navigator.Tick(dt);
        TaskPrivilege.Tick(dt);

        // 房主自动收尾会议 Results:freeplay 假人不投票需要 ForceSkipAll 后 Proceed;
        // 本地/在线自托管房同理(房主负责点 Proceed,避免会议卡死)
        if (_phase == AIPhase.MEETING)
        {
            var mh = MeetingHud.Instance;
            var client = AmongUsClient.Instance;
            if (mh != null && client != null && client.AmHost)
            {
                try
                {
                    if (mh.CurrentState == MeetingHud.VoteStates.Results)
                    {
                        _resultsTimer += dt;
                        if (_resultsTimer >= 2.5f) { _resultsTimer = 0f; mh.HandleProceed(); }
                    }
                    else _resultsTimer = 0f;
                }
                catch { _resultsTimer = 0f; }
            }
            else _resultsTimer = 0f;
        }

        DrainInbound();
    }

    // ---------- phase ----------

    private static AIPhase ComputePhase()
    {
        var client = AmongUsClient.Instance;
        if (client == null) return AIPhase.MENU;
        switch (client.GameState)
        {
            case InnerNetClient.GameStates.NotJoined:
                return AIPhase.MENU;
            case InnerNetClient.GameStates.Joined:
                // FreePlay 全程停在 Joined(无 SendStartGame 流程):TutorialManager 存在 = 教学场景已加载
                if (client.NetworkMode == NetworkModes.FreePlay && TutorialManager.InstanceExists)
                {
                    if (MeetingHud.Instance != null || ExileController.Instance != null) return AIPhase.MEETING;
                    return AIPhase.PLAYING;
                }
                return AIPhase.LOBBY;
            case InnerNetClient.GameStates.Ended:
                return AIPhase.ENDED;
            case InnerNetClient.GameStates.Started:
                if (MeetingHud.Instance != null || ExileController.Instance != null) return AIPhase.MEETING;
                return AIPhase.PLAYING;
            default:
                return AIPhase.MENU;
        }
    }

    private static void ApplyPhase(AIPhase next)
    {
        if (next == _phase) return;
        var prev = _phase;
        _phase = next;
        _forceFull = true;

        switch (next)
        {
            case AIPhase.PLAYING when prev is AIPhase.LOBBY or AIPhase.MENU or AIPhase.ENDED:
                // 新对局开始:重置时钟与轮次
                _clockRunning = true;
                _gameStartClock = Time.realtimeSinceStartup;
                _frozenGameTime = 0f;
                _meetingsStarted = 0;
                PrevFields.Clear();
                BridgeServer.EnqueueReliable(LogMsg("info", "game start -> PLAYING"));
                break;
            case AIPhase.MEETING when prev == AIPhase.PLAYING:
                _meetingsStarted++;
                Navigator.Stop("meeting_start");
                BridgeServer.EnqueueReliable(LogMsg("info", $"meeting #{_meetingsStarted} start (round -> {Round})"));
                break;
            case AIPhase.MENU or AIPhase.LOBBY or AIPhase.ENDED when prev is AIPhase.PLAYING or AIPhase.MEETING:
                // 对局结束回大厅/菜单:冻结时钟
                _frozenGameTime = GameTime;
                _clockRunning = false;
                BridgeServer.EnqueueReliable(LogMsg("info", $"leave game ({prev} -> {next}), game_time={_frozenGameTime:F1}"));
                break;
        }
        BridgeServer.EnqueueReliable(LogMsg("info", $"phase {prev} -> {next}"));
    }

    // ---------- 采集 ----------

    private static void CollectTick()
    {
        ApplyPhase(ComputePhase());
        _tick++;
        if (!_loggedFirstTick)
        {
            _loggedFirstTick = true;
            AIBridgePlugin.Log?.LogInfo("AIHub first collect tick ok");
        }

        // 事件采集(10Hz diff)
        EventTap.Tick();
        // 导航图确保(进图后惰性生成一次)
        if (IsPlayPhaseNow) Navigator.EnsureGraph();

        var fields = Collector.Build(_phase, GameTime, Round, Bodies);
        var full = _forceFull || _fullAcc >= FullInterval;
        if (full) _fullAcc = 0f;

        var msg = Compose(fields, full);
        BridgeServer.SetVolatileState(msg);

        // 更新 delta 基线
        foreach (var kv in fields) PrevFields[kv.Key] = kv.Value;
    }

    private static string Compose(Dictionary<string, string> fields, bool full)
    {
        var sb = new System.Text.StringBuilder(1024);
        sb.Append("{\"type\":\"state_").Append(full ? "full" : "delta").Append('"')
          .Append(",\"tick\":").Append(_tick)
          .Append(",\"t\":").Append(MiniJson.F(GameTime, 1))
          .Append(",\"round\":").Append(Round)
          .Append(",\"phase\":\"").Append(_phase).Append('"');

        foreach (var kv in fields)
        {
            if (full || !PrevFields.TryGetValue(kv.Key, out var prev) || prev != kv.Value)
                sb.Append(",\"").Append(kv.Key).Append("\":").Append(kv.Value);
        }
        sb.Append('}');
        return sb.ToString();
    }

    private static void RefreshBodies()
    {
        Bodies.Clear();
        try
        {
            var found = UObject.FindObjectsOfType<DeadBody>();
            for (var i = 0; i < found.Length; i++)
                if (found[i] != null) Bodies.Add(found[i]);
        }
        catch { }
    }

    // ---------- 下行指令 ----------

    private static void DrainInbound()
    {
        var processed = 0;
        while (processed++ < 64 && BridgeServer.Inbound.TryDequeue(out var line))
            HandleLine(line);
    }

    private static void HandleLine(string line)
    {
        object parsed;
        try { parsed = MiniJson.Parse(line); }
        catch (System.Exception e)
        {
            if (_badJsonLogged++ < 10)
                AIBridgePlugin.Log?.LogWarning($"bad json from agent: {e.Message} | {Truncate(line)}");
            BridgeServer.EnqueueReliable(Ack(null, null, false, "bad_json"));
            return;
        }

        if (parsed is not Dictionary<string, object> d) return;
        var type = d.TryGetValue("type", out var t) ? t as string : null;
        if (string.IsNullOrEmpty(type)) return;
        // ref 允许数字或字符串(agent/面板用带前缀字符串避免跨客户端撞号)
        string refJson = null;
        if (d.TryGetValue("ref", out var r))
        {
            if (r is double rd) refJson = ((long)rd).ToString();
            else if (r is string rs) refJson = MiniJson.Str(rs);
        }
        var hasRef = refJson != null;

        switch (type)
        {
            case "ping":
                BridgeServer.EnqueueReliable("{\"type\":\"pong\",\"tick\":" + _tick + (hasRef ? ",\"ref\":" + refJson : "") + "}");
                break;
            case "hello":
                _forceFull = true;
                BridgeServer.EnqueueReliable(Ack(refJson, "hello", true, null));
                break;
            case "get_state":
                _forceFull = true;
                BridgeServer.EnqueueReliable(Ack(refJson, "get_state", true, null));
                break;
            case "debug_start":
                // 调试指令:直接驱动本地开局(联调用,等价点击菜单按钮)
                var mode = d.TryGetValue("mode", out var m) && m is string ms ? ms : "freeplay";
                var mapId = d.TryGetValue("map", out var mv) && mv is double md ? (int)md : 0;
                var startErr = StartLocalGame(mode, mapId);
                BridgeServer.EnqueueReliable(startErr == null
                    ? Ack(refJson, "debug_start", true, $"{mode} map={mapId}")
                    : Ack(refJson, "debug_start", false, startErr));
                break;
            case "debug_force_start":
                // 调试指令:freeplay 卡在 CosmeticsCache 时,手动补跑 RunTutorial 尾部流程
                ForceFreeplayBegin(refJson);
                break;
            default:
            {
                // 迭代 2:控制指令全套,交由执行器(phase 守卫在执行器内)
                var err = CommandExecutor.Handle(type, d, refJson);
                if (err == null)
                {
                    BridgeServer.EnqueueReliable(Ack(refJson, type, true, null));
                }
                else
                {
                    // 联调期:成功与失败都回执;busy 等软错误也走 ok=false
                    BridgeServer.EnqueueReliable(Ack(refJson, type, false, err));
                }
                break;
            }
        }
    }

    /// <summary>
    /// 调试:程序化触发本地开局(FreePlay / LocalGame)。
    /// 直接复刻 HostLocalGameButton.CoStartGame 的核心调用(原按钮挂在 inactive 的
    /// popover 容器里,对其调 OnClick 的 StartCoroutine 会静默失败,故绕开按钮)。
    /// 仅主菜单场景有效。返回 null=已发起,null 以外=错误码。
    /// </summary>
    internal static string StartLocalGame(string mode, int mapId)
    {
        try
        {
            var client = AmongUsClient.Instance;
            if (client == null) return "no_client";
            if (client.GameState != InnerNetClient.GameStates.NotJoined)
                return $"already_in_game({client.GameState})";

            client.TutorialMapId = mapId;
            // LocalGame 的地图取自 GameOptions,必须同步设置(MapId 是 byte 选项)
            try { GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, (byte)mapId); } catch { }
            var freeplay = mode != "local";
            client.NetworkMode = freeplay ? NetworkModes.FreePlay : NetworkModes.LocalGame;
            if (freeplay)
            {
                DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
                client.MainMenuScene = "MainMenu";
                client.OnlineScene = "Tutorial";
            }
            else
            {
                DestroyableSingleton<InnerNetServer>.Instance.StartAsServer();
                client.MainMenuScene = "MatchMaking";
                client.OnlineScene = "OnlineGame";
            }
            client.SetEndpoint("127.0.0.1", 22023, false);
            client.Connect(MatchMakerModes.HostAndClient, null);
            return null;
        }
        catch (System.Exception e)
        {
            return e.Message;
        }
    }

    /// <summary>
    /// 调试:RunTutorial 若卡在 CosmeticsCache.PopulateFromPlayers()(mod 自定义外观
    /// 依赖网络时可能长时间不返回),手动补跑其尾部:Begin → StartGame → StartSFX。
    /// StartGame 会把 GameState 翻到 Started,phase 机随即进入 PLAYING。
    /// </summary>
    private static void ForceFreeplayBegin(string refJson)
    {
        try
        {
            var ship = ShipStatus.Instance;
            var gm = GameManager.Instance;
            if (ship == null || gm == null)
            {
                BridgeServer.EnqueueReliable(Ack(refJson, "debug_force_start", false, $"ship={ship != null} gm={gm != null}"));
                return;
            }
            ship.Begin();
            gm.StartGame();
            ship.StartSFX();
            BridgeServer.EnqueueReliable(Ack(refJson, "debug_force_start", true, null));
        }
        catch (System.Exception e)
        {
            BridgeServer.EnqueueReliable(Ack(refJson, "debug_force_start", false, e.Message));
        }
    }

    private static string Ack(string refJson, string cmd, bool ok, string err)
    {
        var sb = new System.Text.StringBuilder(96);
        sb.Append("{\"type\":\"ack\"");
        if (refJson != null) sb.Append(",\"ref\":").Append(refJson);
        if (cmd != null) sb.Append(",\"cmd\":").Append(MiniJson.Str(cmd));
        sb.Append(",\"ok\":").Append(MiniJson.B(ok));
        if (err != null) sb.Append(",\"err\":").Append(MiniJson.Str(err));
        sb.Append('}');
        return sb.ToString();
    }

    private static string LogMsg(string level, string msg)
    {
        return "{\"type\":\"log\",\"level\":" + MiniJson.Str(level) + ",\"msg\":" + MiniJson.Str(msg) +
               ",\"t\":" + MiniJson.F(GameTime, 1) + "}";
    }

    private static string Truncate(string s) => s.Length <= 120 ? s : s.Substring(0, 120);
}
