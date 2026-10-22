using System.Collections.Generic;
using TheOtherRoles.Modules;

namespace AIAmongUs;

/// <summary>
/// 最小事件采集(无 Harmony patch,基于 10Hz 状态 diff):
/// 死亡/复活、会议开始/结束、进出管道、破坏激活/修复、任务阶段推进(被动观测)。
/// 自身行动事件(任务特权流程/移动)由各模块主动 Emit。
/// schema:{"type":"event","t","round","code","actor","target","loc","vis","note"}
/// </summary>
internal static class EventTap
{
    private sealed class PlayerWatch
    {
        public byte Id;
        public string Name;
        public bool Alive;
        public bool InVent;
    }

    private static readonly List<PlayerWatch> _players = new();
    private static bool _meetingActive;
    private static bool _inGame;
    private static readonly Dictionary<string, bool> _sabState = new();
    private static int[] _taskSteps = Array.Empty<int>();
    private static bool[] _taskDone = Array.Empty<bool>();

    // 聊天采集(ChatController.AddChat Postfix 写入;freeplay 假人不发言,联机时是真实来源)
    private sealed class ChatMsg { public string Name; public string Text; public float T; public bool Mine; }
    private static readonly List<ChatMsg> _chat = new();

    public static void OnChat(PlayerControl src, string text)
    {
        try
        {
            if (src?.Data == null || string.IsNullOrEmpty(text)) return;
            var name = src.Data.DefaultOutfit?.PlayerName ?? "?";
            var mine = src == PlayerControl.LocalPlayer;
            _chat.Add(new ChatMsg { Name = name, Text = text, T = AIAmongUs.AIHub.GameTimeNow, Mine = mine });
            while (_chat.Count > 50) _chat.RemoveAt(0);
            Emit("CHAT", name, null, "text=" + text);
        }
        catch { }
    }

    /// <summary>最近聊天 JSON 数组(供 state.chat;L3 会议推理读取)。</summary>
    public static string ChatJson()
    {
        var sb = new System.Text.StringBuilder(256);
        sb.Append('[');
        for (var i = 0; i < _chat.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var m = _chat[i];
            sb.Append("{\"name\":").Append(MiniJson.Str(m.Name))
              .Append(",\"text\":").Append(MiniJson.Str(m.Text))
              .Append(",\"t\":").Append(MiniJson.F(m.T, 1))
              .Append(",\"mine\":").Append(MiniJson.B(m.Mine)).Append('}');
        }
        sb.Append(']');
        return sb.ToString();
    }

    public static void ClearChat() { _chat.Clear(); }

    public static void Reset()
    {
        _players.Clear();
        _meetingActive = false;
        _inGame = false;
        _sabState.Clear();
        _taskSteps = Array.Empty<int>();
        _taskDone = Array.Empty<bool>();
        _chat.Clear();
    }

    public static void Tick()
    {
        var me = PlayerControl.LocalPlayer;
        var inGame = AIAmongUs.AIHub.IsPlayPhaseNow && me != null && me.Data != null;
        if (inGame && !_inGame)
        {
            Reset();
            _inGame = true;
            // 初始快照(不发事件)
            SnapshotPlayers();
        }
        else if (!inGame)
        {
            _inGame = false;
            return;
        }

        DiffPlayers();
        DiffMeeting();
        DiffSabotage();
        DiffTasks();
    }

    // ---------- 玩家死亡/复活/管道 ----------

    private static void SnapshotPlayers()
    {
        _players.Clear();
        var all = PlayerControl.AllPlayerControls;
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p?.Data == null) continue;
            _players.Add(new PlayerWatch
            {
                Id = p.PlayerId,
                Name = p.Data.DefaultOutfit?.PlayerName ?? "?",
                Alive = !p.Data.IsDead && !p.Data.Disconnected,
                InVent = p.inVent
            });
        }
    }

    private static void DiffPlayers()
    {
        var all = PlayerControl.AllPlayerControls;
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p?.Data == null) continue;
            var w = FindWatch(p.PlayerId);
            var alive = !p.Data.IsDead && !p.Data.Disconnected;
            var name = p.Data.DefaultOutfit?.PlayerName ?? "?";
            if (w == null)
            {
                _players.Add(new PlayerWatch { Id = p.PlayerId, Name = name, Alive = alive, InVent = p.inVent });
                continue;
            }
            if (w.Alive && !alive)
            {
                w.Alive = false;
                string reason = null;
                try
                {
                    var pd = PlayerData.GetPlayerData(p);
                    if (pd != null && pd.DeathReason != CustomDeathReason.Null) reason = pd.DeathReason.ToString();
                }
                catch { }
                var killer = "";
                try
                {
                    var pd = PlayerData.GetPlayerData(p);
                    if (pd?.KilledBy != null) killer = pd.KilledBy.Data?.DefaultOutfit?.PlayerName ?? "";
                }
                catch { }
                Emit("DEATH", null, name, $"reason={reason ?? "unknown"} killer={killer}");
            }
            else if (!w.Alive && alive)
            {
                w.Alive = true;
                Emit("REVIVE", null, name, null);
            }
            if (w.InVent != p.inVent)
            {
                w.InVent = p.inVent;
                Emit(p.inVent ? "VENT_ENTER" : "VENT_EXIT", name, null,
                    p == PlayerControl.LocalPlayer ? "self" : null);
            }
            w.Name = name;
        }
    }

    private static PlayerWatch FindWatch(byte id)
    {
        for (var i = 0; i < _players.Count; i++)
            if (_players[i].Id == id) return _players[i];
        return null;
    }

    // ---------- 会议 ----------

    private static void DiffMeeting()
    {
        var mh = MeetingHud.Instance;
        if (mh != null && !_meetingActive)
        {
            _meetingActive = true;
            byte? reporter = null;
            try
            {
                var states = mh.playerStates;
                for (var i = 0; i < states.Length; i++)
                {
                    var s = states[i];
                    if (s != null && s.DidReport) { reporter = s.TargetPlayerId; break; }
                }
            }
            catch { }
            Emit("MEETING_START", reporter.HasValue ? IdName(reporter.Value) : null, null,
                $"state={mh.CurrentState}");
        }
        else if (mh == null && _meetingActive)
        {
            _meetingActive = false;
            Emit("MEETING_END", null, null, null);
        }
    }

    private static string IdName(byte id)
    {
        try
        {
            var info = GameData.Instance?.GetPlayerById(id);
            return info?.DefaultOutfit?.PlayerName ?? id.ToString();
        }
        catch { return id.ToString(); }
    }

    // ---------- 破坏 ----------

    private static void DiffSabotage()
    {
        var ship = ShipStatus.Instance;
        var sys = ship?.Systems;
        if (sys == null) return;

        void Check(string key, SystemTypes type)
        {
            try
            {
                var s = StateCollector.GetSystem(sys, type);
                var active = s != null && StateCollector.SystemActive(s, key);
                var prev = _sabState.TryGetValue(key, out var v) && v;
                if (active != prev)
                {
                    _sabState[key] = active;
                    Emit(active ? "SABOTAGE_START" : "SABOTAGE_FIXED", null, null, $"type={key}");
                }
            }
            catch { }
        }

        Check("Lights", SystemTypes.Electrical);
        Check("Reactor", SystemTypes.Reactor);
        Check("Reactor", SystemTypes.Laboratory);
        Check("Oxygen", SystemTypes.LifeSupp);
        Check("Comms", SystemTypes.Comms);
        Check("Heli", SystemTypes.HeliSabotage);
    }

    // ---------- 任务(被动观测:步骤/完成变化) ----------

    private static void DiffTasks()
    {
        var me = PlayerControl.LocalPlayer;
        var tasks = me?.myTasks;
        if (tasks == null) return;
        if (_taskSteps.Length != tasks.Count)
        {
            _taskSteps = new int[tasks.Count];
            _taskDone = new bool[tasks.Count];
            for (var i = 0; i < tasks.Count; i++)
            {
                var npt = tasks[i]?.TryCast<NormalPlayerTask>();
                _taskSteps[i] = npt?.taskStep ?? -1;
                _taskDone[i] = tasks[i] != null && tasks[i].IsComplete;
            }
            return;
        }
        for (var i = 0; i < tasks.Count; i++)
        {
            var t = tasks[i];
            if (t == null) continue;
            var npt = t.TryCast<NormalPlayerTask>();
            var step = npt?.taskStep ?? -1;
            var done = t.IsComplete;
            if (step != _taskSteps[i] && !done)
            {
                _taskSteps[i] = step;
                Emit("TASK_STEP", null, null, $"task={t.Id}({t.TaskType}) step={step}");
            }
            if (done && !_taskDone[i])
            {
                _taskDone[i] = true;
                Emit("TASK_DONE", null, null, $"task={t.Id}({t.TaskType})");
            }
        }
    }

    // ---------- 发射 ----------

    public static void Emit(string code, string actor, string target, string note)
    {
        var me = PlayerControl.LocalPlayer;
        var room = "";
        try { if (me != null) room = StateCollector.RoomOf(me.GetTruePosition()) ?? ""; } catch { }
        if (actor == null && me?.Data != null)
            actor = me.Data.DefaultOutfit?.PlayerName ?? "self";

        var sb = new System.Text.StringBuilder(160);
        sb.Append("{\"type\":\"event\",\"t\":").Append(MiniJson.F(AIAmongUs.AIHub.GameTimeNow, 1))
          .Append(",\"round\":").Append(AIAmongUs.AIHub.RoundNow)
          .Append(",\"code\":").Append(MiniJson.Str(code))
          .Append(",\"actor\":").Append(actor == null ? "null" : MiniJson.Str(actor))
          .Append(",\"target\":").Append(target == null ? "null" : MiniJson.Str(target))
          .Append(",\"loc\":").Append(MiniJson.Str(room))
          .Append(",\"note\":").Append(note == null ? "null" : MiniJson.Str(note))
          .Append('}');
        BridgeServer.EnqueueReliable(sb.ToString());
    }
}
