using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using InnerNet;
using TheOtherRoles.Buttons;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles.Crewmate;

namespace AIAmongUs;

/// <summary>
/// 状态快照组装器。只做读取,不产生任何游戏副作用。
/// 输出 Dictionary&lt;key, jsonFragment&gt;,由 AIHub 拼装成 state_full / state_delta。
/// 每个分区独立 try/catch:单分区失败只丢该分区,不影响其余字段(采样必须稳)。
/// </summary>
internal sealed class StateCollector
{
    private static readonly string[] ColorNames =
    {
        "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple",
        "Brown", "Cyan", "Lime", "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
    };

    private static readonly string[] MapNames = { "Skeld", "Mira", "Polus", "Dleks", "Airship", "Fungle" };

    // CustomButton 实例 -> 稳定 key(HudManagerStartPatch 的字段名)。
    // 按钮随场景整批重建(CustomButton 构造 Add / 销毁 Remove),数量不变也要验证实例身份,
    // 否则同角色跨局会留下整批失效引用(实测 LOBBY→PLAYING 出现 unnamed_*)
    private static Dictionary<CustomButton, string> _buttonKeys = new();
    private static int _buttonKeyCount = -1;

    private static int _errCounter;

    public Dictionary<string, string> Build(AIPhase phase, float gameTime, int round, List<DeadBody> bodies)
    {
        var f = new Dictionary<string, string>();

        f["intro"] = MiniJson.B(IsIntroDisplayed());
        f["map"] = SafeMapName();
        // 诊断:网络层原始状态(phase 判定调试用)
        try
        {
            var c = AmongUsClient.Instance;
            f["net_debug"] = c == null
                ? "null"
                : $"{{\"game_state\":\"{c.GameState}\",\"network_mode\":\"{c.NetworkMode}\",\"am_host\":{MiniJson.B(c.AmHost)}}}";
        }
        catch { f["net_debug"] = "null"; }

        var me = PlayerControl.LocalPlayer;
        var inGame = phase is AIPhase.PLAYING or AIPhase.MEETING or AIPhase.ENDED;

        f["me"] = SafeMe(me, phase, inGame);
        f["players"] = SafePlayers(me, phase, inGame);
        f["nearby_bodies"] = SafeBodies(me, bodies, phase);
        f["sabotages"] = SafeSabotages(phase);
        f["sabotage_cooldown"] = SafeSabotageCooldown(phase);
        f["taskbar"] = SafeTaskbar(phase);
        f["meeting"] = SafeMeeting(phase);
        f["chat"] = EventTap.ChatJson();
        f["rooms"] = SafeRooms();
        f["lobby"] = SafeLobby();

        return f;
    }

    // ---------- lobby / 房间信息(自托管用) ----------

    private static string SafeLobby()
    {
        try
        {
            var c = AmongUsClient.Instance;
            if (c == null) return "null";
            var gd = GameData.Instance;
            var count = gd != null ? gd.PlayerCount : 0;
            var max = 10;
            try { max = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers; } catch { }
            string code = "";
            try { if (c.GameId != 0) code = GameCode.IntToGameName(c.GameId); } catch { }
            return "{\"game_id\":" + c.GameId
                 + ",\"code\":" + MiniJson.Str(code)
                 + ",\"players\":" + count
                 + ",\"max\":" + max
                 + ",\"am_host\":" + MiniJson.B(c.AmHost)
                 + ",\"network_mode\":" + MiniJson.Str(c.NetworkMode.ToString())
                 + ",\"state\":" + MiniJson.Str(c.GameState.ToString())
                 + "}";
        }
        catch { return "null"; }
    }

    // ---------- me ----------

    private static string SafeMe(PlayerControl me, AIPhase phase, bool inGame)
    {
        try
        {
            if (me == null || me.Data == null) return "null";
            var sb = new StringBuilder(256);
            var outfit = me.Data.DefaultOutfit;
            sb.Append('{');
            sb.Append("\"player_id\":").Append(me.PlayerId).Append(',');
            sb.Append("\"color\":").Append(MiniJson.Str(ColorOf(me.Data.DefaultOutfit.ColorId))).Append(',');
            sb.Append("\"name\":").Append(MiniJson.Str(outfit.PlayerName ?? "")).Append(',');
            sb.Append("\"alive\":").Append(MiniJson.B(!me.Data.IsDead)).Append(',');

            // 角色信息:intro 前角色未分配,读不到就给 null
            string role = null, team = null;
            try
            {
                var pd = PlayerData.GetPlayerData(me);
                if (inGame && pd != null && pd.MainRole != RoleId.DefaultRole)
                {
                    role = pd.MainRole.ToString();
                    team = me.IsImpostor() ? "Impostor" : me.IsNeutral() ? "Neutral" : "Crew";
                }
                else if (me.Data.Role != null)
                {
                    role = me.Data.RoleType.ToString();
                    team = me.Data.Role.IsImpostor ? "Impostor" : "Crew";
                }
            }
            catch { }
            sb.Append("\"role\":").Append(role == null ? "null" : MiniJson.Str(role)).Append(',');
            sb.Append("\"team\":").Append(team == null ? "null" : MiniJson.Str(team)).Append(',');

            var pos = me.GetTruePosition();
            sb.Append("\"position\":{\"x\":").Append(MiniJson.F(pos.x)).Append(",\"y\":").Append(MiniJson.F(pos.y)).Append("},");
            sb.Append("\"room\":").Append(inGame ? MiniJson.Str(RoomOf(pos) ?? "") : "null").Append(',');

            // 任务
            sb.Append("\"tasks\":").Append(inGame ? TasksJson(me) : "null").Append(',');

            // 击杀冷却:优先职业刀的 CustomButton 冷却(mod 角色走按钮,原版 killTimer 常是 0/无效)
            float killCd = float.MaxValue;
            var modKillCd = -1f;
            try
            {
                var btns = CustomButton.Buttons;
                for (var bi = 0; bi < btns.Count; bi++)
                {
                    var b = btns[bi];
                    if (b == null || !b.IsKillButton) continue;
                    try { if (b.HasButton != null && !b.HasButton()) continue; } catch { continue; }
                    modKillCd = b.Timer;
                    break;
                }
            }
            catch { }
            if (modKillCd >= 0f) killCd = modKillCd;
            else { try { killCd = me.killTimer; } catch { } }
            sb.Append("\"kill_cooldown\":").Append(killCd == float.MaxValue ? "null" : MiniJson.F(Math.Max(0f, killCd), 1)).Append(',');

            // 本局允许的击杀距离(设置档位 1/1.8/2.5;L3 据此贴身击杀,不超距)
            float killDist = 2.5f;
            try
            {
                var opts = GameOptionsManager.Instance.CurrentGameOptions;
                var arr = opts.GetFloatArray(AmongUs.GameOptions.FloatArrayOptionNames.KillDistances);
                var idx = Mathf.Clamp(opts.GetInt(AmongUs.GameOptions.Int32OptionNames.KillDistance), 0, arr.Length - 1);
                killDist = arr[idx];
            }
            catch { }
            sb.Append("\"kill_distance\":").Append(MiniJson.F(killDist, 1)).Append(',');

            var canVent = false;
            try { canVent = me.RoleCanUseVents(); } catch { }
            sb.Append("\"can_vent\":").Append(MiniJson.B(canVent)).Append(',');

            var inVent = false;
            try { inVent = me.inVent || Vent.currentVent != null; } catch { }
            sb.Append("\"in_vent\":").Append(MiniJson.B(inVent)).Append(',');

            var canMove = false;
            try { canMove = me.CanMove; } catch { }
            sb.Append("\"can_move\":").Append(MiniJson.B(canMove)).Append(',');

            sb.Append("\"abilities\":").Append(AbilitiesJson()).Append(',');

            // 修饰
            var mods = "[]";
            try
            {
                var pd = PlayerData.GetPlayerData(me);
                if (pd?.Modifiers != null && pd.Modifiers.Count > 0)
                {
                    var ms = new StringBuilder("[");
                    foreach (var m in pd.Modifiers) ms.Append(MiniJson.Str(m.ToString())).Append(',');
                    if (pd.Modifiers.Count > 0) ms.Length--;
                    mods = ms.Append(']').ToString();
                }
            }
            catch { }
            sb.Append("\"modifiers\":").Append(mods).Append(',');

            var cuffed = false;
            try { cuffed = Sheriff.handcuffedPlayers != null && Sheriff.handcuffedPlayers.Contains(me.PlayerId); } catch { }
            sb.Append("\"handcuffed\":").Append(MiniJson.B(cuffed));

            sb.Append('}');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "me");
        }
    }

    private static string TasksJson(PlayerControl me)
    {
        try
        {
            var sb = new StringBuilder(128);
            var total = 0; var done = 0;
            var list = new StringBuilder("[");
            var tasks = me.myTasks;
            var hasList = false;
            for (var i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                if (t == null) continue;
                hasList = true;
                total++;
                var complete = false;
                try { complete = t.IsComplete; } catch { }
                if (complete) done++;
                var npt = t.TryCast<NormalPlayerTask>();
                var step = -1; var max = -1;
                if (npt != null) { step = npt.taskStep; max = npt.MaxStep; }
                list.Append("{\"id\":").Append(t.Id)
                    .Append(",\"name\":").Append(MiniJson.Str(t.TaskType.ToString()))
                    .Append(",\"done\":").Append(MiniJson.B(complete))
                    .Append(",\"room\":").Append(MiniJson.Str(t.StartAt.ToString()))
                    .Append(",\"step\":").Append(step)
                    .Append(",\"max_step\":").Append(max).Append("},");
            }
            if (hasList) list.Length--;
            list.Append(']');
            sb.Append("{\"total\":").Append(total).Append(",\"done\":").Append(done).Append(",\"list\":").Append(list).Append('}');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "tasks");
        }
    }

    private static string AbilitiesJson()
    {
        try
        {
            var buttons = CustomButton.Buttons;
            if (buttons == null || buttons.Count == 0) return "[]";
            RebuildButtonKeysIfNeeded(buttons.Count);

            var sb = new StringBuilder(128);
            var any = false;
            sb.Append('[');
            for (var i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                if (b == null) continue;
                try
                {
                    if (b.HasButton == null || !b.HasButton()) continue;
                }
                catch { continue; }

                bool ready = false;
                try { ready = !b.IsCoolingDown && (b.CouldUse == null || b.CouldUse()); }
                catch { }

                if (!_buttonKeys.TryGetValue(b, out var key)) key = "unnamed_" + i;
                sb.Append("{\"key\":").Append(MiniJson.Str(key))
                  .Append(",\"ready\":").Append(MiniJson.B(ready))
                  .Append(",\"cooldown\":").Append(MiniJson.F(Math.Max(0f, b.Timer), 1))
                  .Append(",\"max\":").Append(MiniJson.F(b._MaxTimer, 1))
                  .Append(",\"uses\":").Append(b.UsesCount)
                  .Append(",\"effect_active\":").Append(MiniJson.B(b.IsEffectActive))
                  .Append("},");
                any = true;
            }
            if (any) sb.Length--;
            sb.Append(']');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "abilities");
        }
    }

    private static void RebuildButtonKeysIfNeeded(int currentCount)
    {
        if (_buttonKeyCount == currentCount && !ButtonMapStale()) return;
        _buttonKeys = new Dictionary<CustomButton, string>();
        var fields = typeof(HudManagerStartPatch).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        foreach (var fi in fields)
        {
            if (fi.FieldType != typeof(CustomButton)) continue;
            try
            {
                if (fi.GetValue(null) is CustomButton b && b != null)
                    _buttonKeys[b] = fi.Name;
            }
            catch { }
        }
        _buttonKeyCount = currentCount;
    }

    /// <summary>映射是否过期:字典里任一按钮已不在当前 Buttons 列表(场景切换后整批换实例)。</summary>
    private static bool ButtonMapStale()
    {
        try
        {
            var buttons = TheOtherRoles.Buttons.CustomButton.Buttons;
            if (buttons == null || buttons.Count == 0) return false;
            foreach (var kv in _buttonKeys)
            {
                var found = false;
                for (var i = 0; i < buttons.Count; i++)
                {
                    if (ReferenceEquals(buttons[i], kv.Key)) { found = true; break; }
                }
                if (!found) return true;
            }
            return false;
        }
        catch { return true; }
    }

    // ---------- players ----------

    private static string SafePlayers(PlayerControl me, AIPhase phase, bool inGame)
    {
        try
        {
            var sb = new StringBuilder(512);
            sb.Append('[');
            var any = false;

            var iAmDead = false;
            var radius = 5f;
            var ship = ShipStatus.Instance;
            if (me != null && me.Data != null)
            {
                iAmDead = me.Data.IsDead;
                if (ship != null && inGame) radius = ship.CalculateLightRadius(me.Data);
            }
            var myPos = me != null ? me.GetTruePosition() : Vector2.zero;

            var all = PlayerControl.AllPlayerControls;
            for (var i = 0; i < all.Count; i++)
            {
                var p = all[i];
                if (p == null || p.Data == null || p == me) continue;

                var alive = !p.Data.IsDead && !p.Data.Disconnected;
                bool visible;
                if (!inGame && phase != AIPhase.LOBBY) continue;
                if (phase == AIPhase.LOBBY) visible = true;
                else if (iAmDead) visible = true;
                else if (!alive) visible = false;
                else if (p.inVent) visible = false;
                else
                {
                    var tp = p.GetTruePosition();
                    var d = Vector2.Distance(tp, myPos);
                    visible = d <= radius && !PhysicsHelpers.AnythingBetween(myPos, tp, Constants.ShipAndObjectsMask, false);
                }

                sb.Append("{\"id\":").Append(p.PlayerId)
                  .Append(",\"color\":").Append(MiniJson.Str(ColorOf(p.Data.DefaultOutfit.ColorId)))
                  .Append(",\"name\":").Append(MiniJson.Str(p.Data.DefaultOutfit.PlayerName ?? ""))
                  .Append(",\"alive\":").Append(MiniJson.B(alive))
                  .Append(",\"in_vent\":").Append(MiniJson.B(p.inVent))
                  .Append(",\"visible_to_me\":").Append(MiniJson.B(visible));

                if (visible)
                {
                    var tp = p.GetTruePosition();
                    var d = Vector2.Distance(tp, myPos);
                    sb.Append(",\"distance\":").Append(MiniJson.F(d))
                      .Append(",\"position\":{\"x\":").Append(MiniJson.F(tp.x)).Append(",\"y\":").Append(MiniJson.F(tp.y)).Append("}")
                      .Append(",\"room\":").Append(inGame ? MiniJson.Str(RoomOf(tp) ?? "") : "null");
                }
                else
                {
                    sb.Append(",\"distance\":null,\"position\":null,\"room\":null");
                }
                sb.Append("},");
                any = true;
            }
            if (any) sb.Length--;
            sb.Append(']');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "players");
        }
    }

    // ---------- bodies ----------

    private static string SafeBodies(PlayerControl me, List<DeadBody> bodies, AIPhase phase)
    {
        try
        {
            if (!IsPlayPhase(phase) || bodies == null || bodies.Count == 0) return "[]";
            var sb = new StringBuilder(128);
            var any = false;
            sb.Append('[');
            var myPos = me != null ? me.GetTruePosition() : Vector2.zero;
            for (var i = 0; i < bodies.Count; i++)
            {
                var b = bodies[i];
                if (b == null) continue;
                var bp = b.TruePosition;
                var d = Vector2.Distance(bp, myPos);
                if (d > 7f) continue;
                string victim = null;
                try
                {
                    var info = GameData.Instance?.GetPlayerById(b.ParentId);
                    victim = info?.DefaultOutfit?.PlayerName;
                }
                catch { }
                sb.Append("{\"victim\":").Append(victim == null ? "null" : MiniJson.Str(victim))
                  .Append(",\"player_id\":").Append(b.ParentId)
                  .Append(",\"room\":").Append(MiniJson.Str(RoomOf(bp) ?? ""))
                  .Append(",\"distance\":").Append(MiniJson.F(d))
                  .Append(",\"reported\":").Append(MiniJson.B(b.Reported))
                  .Append(",\"position\":{\"x\":").Append(MiniJson.F(bp.x)).Append(",\"y\":").Append(MiniJson.F(bp.y)).Append('}')
                  .Append("},");
                any = true;
            }
            if (any) sb.Length--;
            sb.Append(']');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "bodies");
        }
    }

    // ---------- sabotage ----------

    private static string SafeSabotages(AIPhase phase)
    {
        try
        {
            if (!IsPlayPhase(phase)) return "[]";
            var ship = ShipStatus.Instance;
            if (ship?.Systems == null) return "[]";
            var sys = ship.Systems;
            var sb = new StringBuilder(96);
            var any = false;
            sb.Append('[');

            void Add(string type, bool active, double? timer = null)
            {
                sb.Append("{\"type\":").Append(MiniJson.Str(type))
                  .Append(",\"active\":").Append(MiniJson.B(active));
                if (timer.HasValue) sb.Append(",\"timer\":").Append(MiniJson.F(timer.Value, 1));
                sb.Append("},");
                any = true;
            }

            var sw = GetSys(sys, SystemTypes.Electrical)?.TryCast<SwitchSystem>();
            if (sw != null) Add("Lights", sw.IsActive);

            var reactor = GetSys(sys, SystemTypes.Reactor)?.TryCast<ReactorSystemType>();
            reactor ??= GetSys(sys, SystemTypes.Laboratory)?.TryCast<ReactorSystemType>();
            if (reactor != null) Add("Reactor", reactor.IsActive, reactor.IsActive ? reactor.Countdown : null);

            var oxy = GetSys(sys, SystemTypes.LifeSupp)?.TryCast<LifeSuppSystemType>();
            if (oxy != null) Add("Oxygen", oxy.IsActive, oxy.IsActive ? oxy.Countdown : null);

            var commsSys = GetSys(sys, SystemTypes.Comms);
            if (commsSys != null)
            {
                var hudOv = commsSys.TryCast<HudOverrideSystemType>();
                var hqHud = hudOv == null ? commsSys.TryCast<HqHudSystemType>() : null;
                var commsActive = (hudOv != null && hudOv.IsActive) || (hqHud != null && hqHud.IsActive);
                Add("Comms", commsActive);
            }

            var heli = GetSys(sys, SystemTypes.HeliSabotage)?.TryCast<HeliSabotageSystem>();
            if (heli != null) Add("Heli", heli.IsActive);

            if (any) sb.Length--;
            sb.Append(']');
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "sabotages");
        }
    }

    // ---------- 系统访问 helper(供 EventTap / CommandExecutor 复用) ----------

    /// <summary>Systems 在 interop 下是 Il2CppSystem 字典,统一用别名类型收参。</summary>
    internal static ISystemType GetSys(ISystem.Dictionary<SystemTypes, ISystemType> systems, SystemTypes key)
    {
        if (systems == null || !systems.ContainsKey(key)) return null;
        try { return systems[key]; } catch { return null; }
    }

    internal static ISystemType GetSystem(ISystem.Dictionary<SystemTypes, ISystemType> systems, SystemTypes key)
        => GetSys(systems, key);

    /// <summary>按逻辑名判定系统是否处于破坏激活态。</summary>
    internal static bool SystemActive(ISystemType s, string key)
    {
        if (s == null) return false;
        switch (key)
        {
            case "Lights":
                var sw = s.TryCast<SwitchSystem>();
                return sw != null && sw.IsActive;
            case "Reactor":
                var r = s.TryCast<ReactorSystemType>();
                return r != null && r.IsActive;
            case "Oxygen":
                var o = s.TryCast<LifeSuppSystemType>();
                return o != null && o.IsActive;
            case "Comms":
                var h = s.TryCast<HudOverrideSystemType>();
                if (h != null) return h.IsActive;
                var hq = s.TryCast<HqHudSystemType>();
                return hq != null && hq.IsActive;
            case "Heli":
                var he = s.TryCast<HeliSabotageSystem>();
                return he != null && he.IsActive;
            default:
                return false;
        }
    }

    internal static string SafeMapNameRaw()
    {
        try
        {
            var client = AmongUsClient.Instance;
            if (client != null && client.NetworkMode == NetworkModes.FreePlay)
                return "freeplay_map" + client.TutorialMapId;
            if (GameOptionsManager.Instance == null) return "?";
            return "map" + GameOptionsManager.Instance.CurrentGameOptions.MapId;
        }
        catch { return "?"; }
    }

    /// <summary>按 abilities 里的 key 反查当前有效的 CustomButton(use_ability 指令用)。</summary>
    internal static CustomButton FindButtonByKey(string key)
    {
        var buttons = TheOtherRoles.Buttons.CustomButton.Buttons;
        if (buttons == null || buttons.Count == 0) return null;
        RebuildButtonKeysIfNeeded(buttons.Count);
        // 只返回当前列表里的实例:映射表重建后旧对象已失效
        for (var i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];
            if (b == null) continue;
            if (_buttonKeys.TryGetValue(b, out var k) && k == key) return b;
        }
        return null;
    }

    private static string SafeSabotageCooldown(AIPhase phase)
    {
        try
        {
            if (!IsPlayPhase(phase)) return "null";
            var systems = ShipStatus.Instance?.Systems;
            var sab = GetSys(systems, SystemTypes.Sabotage)?.TryCast<SabotageSystemType>();
            return sab == null ? "null" : MiniJson.F(Math.Max(0f, sab.Timer), 1);
        }
        catch (Exception e)
        {
            return ErrFragment(e, "sab_cd");
        }
    }

    // ---------- meeting ----------

    private static string SafeMeeting(AIPhase phase)
    {
        try
        {
            var mh = MeetingHud.Instance;
            if (phase != AIPhase.MEETING || mh == null) return "null";

            byte? reporter = null;
            var order = new List<byte>();
            try
            {
                var states = mh.playerStates;
                for (var i = 0; i < states.Length; i++)
                {
                    var s = states[i];
                    if (s == null) continue;
                    order.Add(s.TargetPlayerId);
                    if (s.DidReport && reporter == null) reporter = s.TargetPlayerId;
                }
            }
            catch { }
            if (reporter == null) { try { reporter = mh.reporterId; } catch { } }
            order.Sort();

            var me = PlayerControl.LocalPlayer;
            var myId = me != null ? me.PlayerId : (byte)255;
            var didVote = false; var myVote = -1;
            var votes = new StringBuilder(64);
            votes.Append('[');
            var voteCount = 0;
            try
            {
                var states = mh.playerStates;
                for (var i = 0; i < states.Length; i++)
                {
                    var s = states[i];
                    if (s == null) continue;
                    if (voteCount > 0) votes.Append(',');
                    votes.Append("{\"id\":").Append(s.TargetPlayerId)
                         .Append(",\"voted\":").Append(MiniJson.B(s.DidVote))
                         .Append(",\"for\":").Append((int)s.VotedFor).Append('}');
                    voteCount++;
                    if (s.TargetPlayerId == myId)
                    {
                        didVote = s.DidVote;
                        myVote = (int)s.VotedFor;
                    }
                }
            }
            catch { }
            votes.Append(']');
            var canVote = false;
            try { canVote = mh.CurrentState == MeetingHud.VoteStates.NotVoted && me != null && !me.Data.IsDead && !didVote; } catch { }

            // 发言顺序(轮麦制约定):报警人开始,之后按会议图标布局从左到右、从上到下。
            // 布局顺序用真实 transform 坐标计算(每行 y 相同、行间 y 递减),兼容 mod 翻页/改排版;
            // 翻页(CrowdedPlayer>12人)时被隐藏的图标不在本页,不参与顺序。
            var speaking = new List<byte>();
            try
            {
                var cells = new List<(byte id, float x, float y)>();
                var states2 = mh.playerStates;
                for (var i = 0; i < states2.Length; i++)
                {
                    var s = states2[i];
                    if (s == null || s.AmDead) continue;
                    try { if (!s.gameObject.activeSelf) continue; } catch { }
                    var lp = s.transform.localPosition;
                    cells.Add((s.TargetPlayerId, lp.x, lp.y));
                }
                cells.Sort((a, b) =>
                {
                    var rowA = Mathf.Round(a.y * 10f); var rowB = Mathf.Round(b.y * 10f);
                    if (rowA != rowB) return rowB.CompareTo(rowA); // 上排在前
                    return a.x.CompareTo(b.x);                      // 同排左在前
                });
                foreach (var c in cells) speaking.Add(c.id);
                if (reporter.HasValue)
                {
                    speaking.Remove(reporter.Value);
                    speaking.Insert(0, reporter.Value);
                }
            }
            catch { }

            var sb = new StringBuilder(256);
            sb.Append("{\"reporter\":").Append(reporter.HasValue ? reporter.Value.ToString() : "null")
              .Append(",\"state\":").Append(MiniJson.Str(mh.CurrentState.ToString()))
              .Append(",\"discussion_timer\":").Append(MiniJson.F(mh.discussionTimer, 1))
              .Append(",\"can_vote\":").Append(MiniJson.B(canVote))
              .Append(",\"did_vote\":").Append(MiniJson.B(didVote))
              .Append(",\"my_vote\":").Append(myVote)
              .Append(",\"votes\":").Append(votes.ToString())
              .Append(",\"order\":[");
            for (var i = 0; i < order.Count; i++)
            {
                sb.Append(order[i]);
                if (i < order.Count - 1) sb.Append(',');
            }
            sb.Append("],\"speaking_order\":[");
            for (var i = 0; i < speaking.Count; i++)
            {
                sb.Append(speaking[i]);
                if (i < speaking.Count - 1) sb.Append(',');
            }
            sb.Append("]}");
            return sb.ToString();
        }
        catch (Exception e)
        {
            return ErrFragment(e, "meeting");
        }
    }

    /// <summary>当前地图存在的房间名(FastRooms 键;L3 巡逻/站位用,避免跑到本图没有的房间)。</summary>
    private static string SafeRooms()
    {
        try
        {
            var ship = ShipStatus.Instance;
            if (ship?.FastRooms == null) return "[]";
            var sb = new StringBuilder(160);
            sb.Append('[');
            var any = false;
            foreach (var kv in ship.FastRooms)
            {
                if (any) sb.Append(',');
                sb.Append(MiniJson.Str(kv.Key.ToString()));
                any = true;
            }
            sb.Append(']');
            return sb.ToString();
        }
        catch { return "[]"; }
    }

    // ---------- helpers ----------

    private static string SafeMapName()
    {
        try
        {
            var client = AmongUsClient.Instance;
            // FreePlay 的地图由 TutorialMapId 决定(选项里的 MapId 是上次房间的残留)
            if (client != null && client.NetworkMode == NetworkModes.FreePlay)
            {
                var tid = client.TutorialMapId;
                return tid >= 0 && tid < MapNames.Length ? MiniJson.Str(MapNames[tid]) : MiniJson.Str("Map" + tid);
            }
            if (GameOptionsManager.Instance == null) return "null";
            var mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            return mapId >= 0 && mapId < MapNames.Length ? MiniJson.Str(MapNames[mapId]) : MiniJson.Str("Map" + mapId);
        }
        catch (Exception e)
        {
            return ErrFragment(e, "map");
        }
    }

    private static bool IsIntroDisplayed()
    {
        try
        {
            return DestroyableSingleton<HudManager>.InstanceExists
                   && DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed;
        }
        catch { return false; }
    }

    private static string SafeTaskbar(AIPhase phase)
    {
        try
        {
            if (!IsPlayPhase(phase)) return "null";
            var gd = GameData.Instance;
            if (gd == null || gd.TotalTasks <= 0) return "null";
            return MiniJson.F((float)gd.CompletedTasks / gd.TotalTasks, 3);
        }
        catch (Exception e)
        {
            return ErrFragment(e, "taskbar");
        }
    }

    internal static string RoomOf(Vector2 pos)
    {
        var ship = ShipStatus.Instance;
        if (ship?.AllRooms == null) return null;
        var rooms = ship.AllRooms;
        for (var i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];
            if (r?.roomArea == null) continue;
            if (r.roomArea.OverlapPoint(pos)) return r.RoomId.ToString();
        }
        return null;
    }

    internal static string ColorOf(int colorId)
    {
        return colorId >= 0 && colorId < ColorNames.Length ? ColorNames[colorId] : "Color" + colorId;
    }

    private static bool IsPlayPhase(AIPhase phase) => phase is AIPhase.PLAYING or AIPhase.MEETING;

    private static string ErrFragment(Exception e, string section)
    {
        if (_errCounter < 40)
        {
            _errCounter++;
            AIBridgePlugin.Log?.LogError($"state section '{section}' failed: {e.Message}");
        }
        return "null";
    }
}
