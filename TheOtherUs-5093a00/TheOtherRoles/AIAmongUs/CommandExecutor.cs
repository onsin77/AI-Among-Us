using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TheOtherRoles.Buttons;
using TheOtherRoles.Modules;
using TheOtherRoles.Roles;
using UnityEngine;

namespace AIAmongUs;

/// <summary>
/// 下行控制指令统一执行器(迭代 2 全套)。
/// 每条指令:phase 白名单 → 存活/状态守卫 → 执行 → ack。
/// err 码:phase_guard / not_found / too_far / cooldown / not_allowed / dead / busy / internal。
/// </summary>
internal static class CommandExecutor
{
    public static string Handle(string type, Dictionary<string, object> d, string refJson)
    {
        try
        {
            return type switch
            {
                "move_to" => CmdMoveTo(d, refJson),
                "move_stop" => CmdMoveStop(refJson),
                "kill" => CmdKill(d, refJson),
                "report" => CmdReport(d, refJson),
                "open_task" => CmdOpenTask(d, refJson),
                "interact" => CmdInteract(d, refJson),
                "use_ability" => CmdUseAbility(d, refJson),
                "use_sabotage" => CmdUseSabotage(d, refJson),
                "repair" => CmdRepair(d, refJson),
                "vote" => CmdVote(d, refJson),
                "chat" => CmdChat(d, refJson),
                "vent_enter" => CmdVent(d, refJson, true),
                "vent_exit" => CmdVent(d, refJson, false),
                "minigame_close" => CmdMinigameClose(refJson),
                "host_game" => CmdHostGame(d, refJson),
                "start_game" => CmdStartGame(d, refJson),
                "next_game" => CmdNextGame(refJson),
                "leave_game" => CmdLeaveGame(refJson),
                "spawn_bots" => CmdSpawnBots(d, refJson),
                "debug_gather_bots" => CmdGatherBots(refJson),
                "set_game_options" => CmdSetGameOptions(d, refJson),
                "debug_set_role" => CmdDebugSetRole(d, refJson),
                "nav_info" => CmdNavInfo(refJson),
                "nav_probe" => CmdNavProbe(d, refJson),
                "nav_dump" => CmdNavDump(refJson),
                "nav_path" => CmdNavPath(d, refJson),
                "debug_players" => CmdDebugPlayers(refJson),
                "debug_outfits" => CmdDebugOutfits(refJson),
                "debug_meeting_ff" => CmdDebugMeetingFF(refJson),
                "debug_call_meeting" => CmdDebugCallMeeting(refJson),
                "debug_end_game" => CmdDebugEndGame(d, refJson),
                _ => "unknown_command"
            };
        }
        catch (Exception e)
        {
            AIBridgePlugin.Log?.LogError($"cmd {type} internal error: {e}");
            return "internal:" + e.Message;
        }
    }

    // ---------- helpers ----------

    /// <summary>当前房间设置允许的击杀距离(KillDistance 档位常量 1/1.8/2.5 + 胶囊余量)。</summary>
    private static float AllowedKillDistance()
    {
        try
        {
            var opts = GameOptionsManager.Instance.CurrentGameOptions;
            var arr = opts.GetFloatArray(FloatArrayOptionNames.KillDistances);
            var idx = Mathf.Clamp(opts.GetInt(Int32OptionNames.KillDistance), 0, arr.Length - 1);
            return arr[idx] + 0.6f;
        }
        catch { return 3.1f; }
    }

    private static PlayerControl Me()
    {
        var me = PlayerControl.LocalPlayer;
        return me != null && me.Data != null ? me : null;
    }

    private static string GuardPlay(PlayerControl me, bool allowMeeting = false)
    {
        var phase = AIAmongUs.AIHub.CurrentPhase;
        var ok = phase == AIPhase.PLAYING || (allowMeeting && phase == AIPhase.MEETING);
        if (!ok) return $"phase_guard:{phase}";
        if (me == null) return "no_player";
        if (me.Data.IsDead) return "dead";
        return null;
    }

    private static string GetStr(Dictionary<string, object> d, string key)
    {
        return d.TryGetValue(key, out var v) && v is string s ? s : null;
    }

    private static double? GetNum(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var v) && v is double n) return n;
        return null;
    }

    private static PlayerControl FindPlayer(Dictionary<string, object> d, string idKey)
    {
        var id = GetNum(d, idKey);
        if (id == null) return null;
        // byte 目标必须显式验范围:256 会被 (byte) 截断回绕成 0,命中错误玩家
        if (id.Value < 0 || id.Value > 255) return null;
        var all = PlayerControl.AllPlayerControls;
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p != null && p.Data != null && p.PlayerId == (byte)id.Value) return p;
        }
        return null;
    }

    // ---------- 移动 ----------

    private static string CmdMoveTo(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;

        Vector2 dest;
        var label = "";
        var tol = 0.6f;

        var room = GetStr(d, "room");
        if (room != null)
        {
            // 房间名 → 房间内真实可达格(bounds.center 常落墙外,不可用作目标点)
            if (!System.Enum.TryParse<SystemTypes>(room, true, out var st)) return "not_found:room";
            var ship = ShipStatus.Instance;
            if (ship?.FastRooms == null || !ship.FastRooms.ContainsKey(st)) return "not_found:room_on_map";
            var r = ship.FastRooms[st];
            if (r?.roomArea == null) return "not_found:room_area";
            var goal = Navigator.RoomGoal(st, me.GetTruePosition());
            dest = goal ?? r.roomArea.bounds.center; // 无可达内点时退回中心,由 FindPath 就近取格
            label = room;
        }
        else
        {
            var pos = d.TryGetValue("pos", out var pv) && pv is List<object> pl && pl.Count == 2
                && pl[0] is double px && pl[1] is double py
                ? new Vector2((float)px, (float)py)
                : (Vector2?)null;
            if (pos == null) return "not_found:pos_or_room";
            dest = pos.Value;
            label = $"({dest.x:F1},{dest.y:F1})";
        }
        var t = GetNum(d, "tolerance");
        if (t != null) tol = (float)t.Value;

        var err = Navigator.MoveTo(dest, label, tol);
        return err; // null = 已排队
    }

    private static string CmdMoveStop(string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        Navigator.Stop("cmd_stop");
        return null;
    }

    // ---------- 击杀 ----------

    private static string CmdKill(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;

        var target = FindPlayer(d, "target_id");
        if (target == null || target.Data == null) return "not_found:target";
        if (target.Data.IsDead) return "dead:target";
        if (target == me) return "not_allowed:self";

        var dist = Vector2.Distance(me.GetTruePosition(), target.GetTruePosition());
        // 距离上限尊重游戏设置(KillDistance 档位 1/1.8/2.5)+ 目标胶囊余量,不做超距击杀
        var maxKill = AllowedKillDistance();
        if (dist > maxKill) return $"too_far:{dist:F1}/{maxKill:F1}";

        // 优先走 mod 职业刀(CustomButton),否则原版 CmdCheckMurder
        CustomButton killBtn = null;
        try
        {
            var buttons = CustomButton.Buttons;
            for (var i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                if (b == null || !b.IsKillButton) continue;
                try { if (b.HasButton != null && b.HasButton()) { killBtn = b; break; } } catch { }
            }
        }
        catch { }

        if (killBtn != null)
        {
            var ready = false;
            try { ready = !killBtn.IsCoolingDown; } catch { }
            if (!ready)
            {
                float tm = 0f;
                try { tm = killBtn.Timer; } catch { }
                // 本地自托管房(测试 bot 局):部分职业刀是特殊按钮(诅咒/换弹等),
                // 按钮不可用时回退原版击杀;仅限 LocalGame 房主,线上真人局不改行为
                var selfHost = AmongUsClient.Instance.AmHost
                               && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
                float vanillaCd = float.MaxValue;
                try { vanillaCd = me.killTimer; } catch { }
                if (selfHost && vanillaCd <= 0.05f)
                {
                    me.CmdCheckMurder(target);
                    EventTap.Emit("KILL_CMD", null, target.Data.DefaultOutfit?.PlayerName,
                        $"vanilla_fallback dist={dist:F1}");
                    return null;
                }
                return $"cooldown:{tm:F1}";
            }
            // mod 按钮的目标由游戏每帧刷新(currentTarget 语义),点击与按热键完全等价
            killBtn.onClickEvent();
        }
        else
        {
            // 原版:必须冷却完毕(killTimer<=0)
            try { if (me.killTimer > 0f) return $"cooldown:{me.killTimer:F1}"; } catch { }
            me.CmdCheckMurder(target);
        }
        EventTap.Emit("KILL_CMD", null, target.Data.DefaultOutfit?.PlayerName, $"dist={dist:F1}");
        return null;
    }

    // ---------- 报告 ----------

    private static string CmdReport(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;

        DeadBody body = null;
        var victimId = GetNum(d, "body_player_id");
        if (victimId != null && (victimId.Value < 0 || victimId.Value > 255)) return "bad_param:body_player_id";
        var bodies = UObject.FindObjectsOfType<DeadBody>();
        for (var i = 0; i < bodies.Length; i++)
        {
            var b = bodies[i];
            if (b == null || b.Reported) continue;
            if (victimId != null && b.ParentId != (byte)victimId.Value) continue;
            var dist = Vector2.Distance(b.TruePosition, me.GetTruePosition());
            if (dist > 5.5f) continue;
            if (PhysicsHelpers.AnythingBetween(me.GetTruePosition(), b.TruePosition, Constants.ShipAndObjectsMask, false)) continue;
            body = b;
            break;
        }
        if (body == null) return "not_found:body_in_range";

        var victim = GameData.Instance?.GetPlayerById(body.ParentId);
        if (victim == null) return "not_found:victim_info";
        me.CmdReportDeadBody(victim);
        return null;
    }

    // ---------- 任务 ----------

    private static string CmdOpenTask(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var id = GetNum(d, "task_id");
        if (id == null) return "not_found:task_id";
        return TaskPrivilege.Start((uint)id.Value, refJson);
    }

    private static string CmdInteract(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var cid = GetNum(d, "console_id");
        Console target = null;
        var ship = ShipStatus.Instance;
        if (ship?.AllConsoles == null) return "no_map";
        var all = ship.AllConsoles;
        if (cid != null)
        {
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].ConsoleId == (int)cid.Value) { target = all[i]; break; }
        }
        if (target == null) return "not_found:console";

        var dist = Vector2.Distance(((Component)target).transform.position, me.GetTruePosition());
        if (dist > 2.2f) return $"too_far:{dist:F1}";
        // 若该台挂着自己未完成的任务:不能 target.Use()——共用台会按 FindTask 开出
        // "该台第一项未完成任务"的面板(实测开错),复刻 TaskPrivilege 的实例化流程绑定自己的任务;
        // 非自己任务的台(地图/门开关等)仍走 Use()。
        try
        {
            var tasks = me.myTasks;
            for (var i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                if (t == null || t.IsComplete) continue;
                bool valid;
                try { valid = t.ValidConsole(target); } catch { continue; }
                if (!valid) continue;
                var prefab = t.GetMinigamePrefab();
                if (prefab == null) return "no_minigame_prefab";
                var mg = UObject.Instantiate(prefab);
                mg.transform.SetParent(Camera.main.transform, false);
                mg.transform.localPosition = new Vector3(0f, 0f, -50f);
                mg.Console = target;
                mg.Begin(t);
                return null;
            }
        }
        catch (Exception e) { return "internal:" + e.Message; }
        target.Use();
        return null;
    }

    // ---------- 技能(全职业统一入口) ----------

    private static string CmdUseAbility(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var key = GetStr(d, "key");
        if (key == null) return "not_found:key";
        // 会议技能(末日预言/神谕等)在 MEETING 也可用;行动类按钮在会议中
        // HasButton()==false 会被下面自然拒绝,因此这里放行会议阶段
        var g = GuardPlay(me, allowMeeting: true);
        if (g != null) return g;

        var btn = Navigator2FindButton(key);
        if (btn == null) return "not_found:button";
        bool ready = false;
        try { ready = !btn.IsCoolingDown && (btn.CouldUse == null || btn.CouldUse()); }
        catch (Exception e) { return "internal:coulduse " + e.Message; }
        if (!ready) return "cooldown_or_not_usable";
        btn.onClickEvent();
        EventTap.Emit("TOR_USE_ABILITY", null, null, $"key={key}");
        return null;
    }

    private static CustomButton Navigator2FindButton(string key)
    {
        return StateCollector.FindButtonByKey(key);
    }

    // ---------- 破坏 / 修复 ----------

    private static string CmdUseSabotage(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        // 子类型字段用 kind:顶层的 "type" 已被指令路由占用,同名会读到指令名本身
        var typeName = GetStr(d, "kind") ?? GetStr(d, "sab_type");
        if (typeName == null || !System.Enum.TryParse<SystemTypes>(typeName, true, out var sys))
            return "not_found:kind";
        // 守卫:破坏仅限可破坏阵营(内鬼/可破坏职业)
        bool can = false;
        try { can = me.Data.Role != null && me.Data.Role.IsImpostor || me.CanUseSabotage(); } catch { }
        if (!can) return "not_allowed:no_sabotage_role";

        var ship = ShipStatus.Instance;
        if (ship == null) return "no_map";
        var sab = StateCollector.GetSystem(ship.Systems, SystemTypes.Sabotage)?.TryCast<SabotageSystemType>();
        if (sab != null && sab.Timer > 0f) return $"cooldown:{sab.Timer:F1}";
        ship.RpcUpdateSystem(SystemTypes.Sabotage, (byte)sys);
        EventTap.Emit("SABOTAGE_CMD", null, null, $"type={sys}");
        return null;
    }

    private static string CmdRepair(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        // 子类型字段用 kind(顶层 type 是指令路由名,同名冲突)
        var typeName = GetStr(d, "kind") ?? GetStr(d, "sab_type");
        if (typeName == null) return "not_found:kind";
        var ship = ShipStatus.Instance;
        if (ship == null) return "no_map";

        switch (typeName.ToLowerInvariant())
        {
            case "lights":
            case "electrical":
                // 就地修灯:把 ActualSwitches 逐位异或到与 Expected 一致。
                // 真人必须站在电气房开关台前,这里同样校验位置(不允许远程修灯)
                var sw = StateCollector.GetSystem(ship.Systems, SystemTypes.Electrical)?.TryCast<SwitchSystem>();
                if (sw == null) return "not_found:switch_system";
                if (!sw.IsActive) return "not_active";
                var switchConsole = FindSystemConsole(SystemTypes.Electrical);
                var errDist = RequireNearConsole(switchConsole);
                if (errDist != null) return errDist;
                var diff = sw.ActualSwitches ^ sw.ExpectedSwitches;
                if (diff == 0) return "not_active";
                for (var bit = 0; bit < 5; bit++)
                    if ((diff & (1 << bit)) != 0)
                        ship.RpcUpdateSystem(SystemTypes.Electrical, (byte)bit);
                return null;
            case "comms":
            {
                var commsConsole = FindSystemConsole(SystemTypes.Comms);
                var errDist2 = RequireNearConsole(commsConsole);
                if (errDist2 != null) return errDist2;
                ship.RpcUpdateSystem(SystemTypes.Comms, 0);
                return null;
            }
            case "reactor":
            case "oxygen":
            case "oxy":
            case "life_supp":
                var sys = typeName.ToLowerInvariant().StartsWith("reactor")
                    ? SystemTypes.Reactor : SystemTypes.LifeSupp;
                // 需要人在 console 旁:找对应系统 console,发 0x40|consoleId
                var c = FindSystemConsole(sys);
                if (c == null) return "not_found:console";
                var dist = Vector2.Distance(((Component)c).transform.position, me.GetTruePosition());
                if (dist > 2.5f) return $"too_far:{dist:F1}";
                ship.RpcUpdateSystem(sys, (byte)(0x40 | (c.ConsoleId & 3)));
                return null;
            default:
                return "not_found:type";
        }
    }

    /// <summary>修复类操作的位置校验:人必须站在对应系统台旁(否则=远程修复)。</summary>
    private static string RequireNearConsole(Console c)
    {
        if (c == null) return "not_found:console";
        var me = PlayerControl.LocalPlayer;
        var dist = Vector2.Distance(((Component)c).transform.position, me.GetTruePosition());
        return dist > 3.0f ? $"too_far:{dist:F1}" : null;
    }

    private static Console FindSystemConsole(SystemTypes sys)
    {
        var ship = ShipStatus.Instance;
        var all = ship?.AllConsoles;
        if (all == null) return null;
        var me = PlayerControl.LocalPlayer;
        if (me == null) return null;
        var myPos = me.GetTruePosition();
        Console best = null; var bestD = float.MaxValue;
        for (var i = 0; i < all.Length; i++)
        {
            var c = all[i];
            if (c == null || c.Room != sys) continue;
            var d = Vector2.Distance(((Component)c).transform.position, myPos);
            if (d < bestD) { best = c; bestD = d; }
        }
        return best;
    }

    // ---------- 会议 ----------

    private static string CmdVote(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        if (AIAmongUs.AIHub.CurrentPhase != AIPhase.MEETING) return "phase_guard:not_meeting";
        var mh = MeetingHud.Instance;
        if (mh == null) return "not_found:meeting";
        try { if (mh.CurrentState != MeetingHud.VoteStates.NotVoted) return "not_allowed:vote_state"; } catch { }
        try { if (!me.CanUseMeetingAbility()) return "not_allowed:meeting_ability"; } catch { }

        byte target;
        if (d.TryGetValue("skip", out var sv) && sv is bool b && b) target = 253;
        else
        {
            var id = GetNum(d, "target_id");
            if (id == null) return "not_found:target_id";
            if (id.Value < 0 || id.Value > 255) return "bad_param:target_id";
            target = (byte)id.Value;
        }
        mh.Confirm(target);
        EventTap.Emit("VOTE", null, null, $"target={target}");
        return null;
    }

    private static string CmdChat(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        var text = GetStr(d, "text");
        if (text == null) return "not_found:text";
        var phase = AIAmongUs.AIHub.CurrentPhase;
        if (phase != AIPhase.MEETING && phase != AIPhase.LOBBY) return $"phase_guard:{phase}";
        me.RpcSendChat(text);
        return null;
    }

    // ---------- 管道 ----------

    private static string CmdVent(Dictionary<string, object> d, string refJson, bool enter)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var ship = ShipStatus.Instance;
        if (ship?.AllVents == null) return "no_map";
        var all = ship.AllVents;

        if (enter)
        {
            if (me.inVent) return "not_allowed:already_in_vent";
            // 职业守卫:与 Vent.CanUse(mod patch)同判据,无管道权限的职业拒绝钻管
            bool canVent = false;
            try { canVent = me.RoleCanUseVents(); } catch { }
            if (!canVent) return "not_allowed:no_vent_role";
            var id = GetNum(d, "vent_id");
            Vent v = null; var bestD = float.MaxValue;
            for (var i = 0; i < all.Length; i++)
            {
                var vv = all[i];
                if (vv == null) continue;
                if (id != null && vv.Id != (int)id.Value) continue;
                var dd = Vector2.Distance(((Component)vv).transform.position, me.GetTruePosition());
                if (dd < bestD) { v = vv; bestD = dd; }
            }
            if (v == null) return "not_found:vent";
            if (bestD > 2.5f) return $"too_far:{bestD:F1}";
            me.MyPhysics.RpcEnterVent(v.Id);
            return null;
        }
        else
        {
            if (!me.inVent) return "not_allowed:not_in_vent";
            var cur = Vent.currentVent;
            me.MyPhysics.RpcExitVent(cur != null ? cur.Id : 0);
            return null;
        }
    }

    // ---------- 调试 ----------

    private static string CmdMinigameClose(string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var mg = Minigame.Instance;
        if (mg == null) return "not_found:minigame";
        try { mg.Close(); } catch (Exception e) { return "internal:" + e.Message; }
        return null;
    }

    private static string CmdDebugSetRole(Dictionary<string, object> d, string refJson)
    {
        var me = Me();
        if (me == null) return "no_player";
        var client = AmongUsClient.Instance;
        // freeplay 天然可用;本地/在线自托管房主也可(自治测试与开局纠错用)
        if (client == null) return "no_client";
        var selfHost = client.AmHost && client.NetworkMode == NetworkModes.LocalGame;
        if (client.NetworkMode != NetworkModes.FreePlay && !selfHost)
            return "not_allowed:freeplay_or_local_host";
        var roleStr = GetStr(d, "role") ?? "Impostor";
        if (!System.Enum.TryParse<RoleTypes>(roleStr, true, out var role)) return "not_found:role";
        // 可选 target_id:房主可改任意玩家(自托管 bot 局确定性分配角色用)
        var tid = GetNum(d, "target_id");
        if (tid != null)
        {
            var target = FindPlayer(d, "target_id");
            if (target == null || target.Data == null) return "not_found:target";
            target.RpcSetRole(role);
            EventTap.Emit("SET_ROLE", null, target.Data.DefaultOutfit?.PlayerName, role.ToString());
            return null;
        }
        me.RpcSetRole(role);
        return null;
    }

    private static string CmdNavInfo(string refJson)
    {
        BridgeServer.EnqueueReliable("{\"type\":\"nav_info\"," + Navigator.StatusJson().Substring(1));
        return null;
    }

    private static string CmdNavDump(string refJson)
    {
        var g = GuardPlay(Me());
        if (g != null) return g;
        BridgeServer.EnqueueReliable(Navigator.DumpGridJson());
        return null;
    }

    private static string CmdNavProbe(Dictionary<string, object> d, string refJson)
    {
        var g = GuardPlay(Me());
        if (g != null) return g;
        Vector2? P(string k)
        {
            if (d.TryGetValue(k, out var v) && v is List<object> pl && pl.Count == 2
                && pl[0] is double px && pl[1] is double py)
                return new Vector2((float)px, (float)py);
            return null;
        }
        var a = P("a");
        var b = P("b");
        if (a == null || b == null) return "not_found:a/b";
        BridgeServer.EnqueueReliable("{\"type\":\"nav_probe\",\"a\":[" + MiniJson.F(a.Value.x, 1) + "," + MiniJson.F(a.Value.y, 1) +
            "],\"b\":[" + MiniJson.F(b.Value.x, 1) + "," + MiniJson.F(b.Value.y, 1) + "]," + Navigator.Probe(a.Value, b.Value).Substring(1));
        return null;
    }

    private static string CmdNavPath(Dictionary<string, object> d, string refJson)
    {
        var g = GuardPlay(Me());
        if (g != null) return g;
        Vector2? P(string k)
        {
            if (d.TryGetValue(k, out var v) && v is List<object> pl && pl.Count == 2
                && pl[0] is double px && pl[1] is double py)
                return new Vector2((float)px, (float)py);
            return null;
        }
        var to = P("to");
        if (to == null)
        {
            var room = GetStr(d, "room");
            if (room != null && System.Enum.TryParse<SystemTypes>(room, true, out var st))
            {
                var ship = ShipStatus.Instance;
                if (ship?.FastRooms != null && ship.FastRooms.ContainsKey(st) && ship.FastRooms[st]?.roomArea != null)
                    to = ship.FastRooms[st].roomArea.bounds.center;
            }
        }
        if (to == null) return "not_found:to";
        var me = Me();
        var from = P("from") ?? (me != null ? me.GetTruePosition() : Vector2.zero);
        BridgeServer.EnqueueReliable(Navigator.PlanJson(from, to.Value));
        return null;
    }

    private static string CmdDebugOutfits(string refJson)
    {
        var gd = GameData.Instance;
        if (gd == null) return "not_found:gamedata";
        var sb = new System.Text.StringBuilder(512);
        sb.Append("{\"type\":\"debug_outfits\",\"players\":[");
        for (var i = 0; i < gd.AllPlayers.Count; i++)
        {
            var p = gd.AllPlayers[i];
            if (p == null) continue;
            var o = p.DefaultOutfit;
            if (i > 0) sb.Append(',');
            sb.Append("{\"id\":").Append(p.PlayerId)
              .Append(",\"name\":").Append(MiniJson.Str(p.DefaultOutfit?.PlayerName ?? ""))
              .Append(",\"hat\":").Append(MiniJson.Str(o?.HatId ?? "null"))
              .Append(",\"skin\":").Append(MiniJson.Str(o?.SkinId ?? "null"))
              .Append(",\"visor\":").Append(MiniJson.Str(o?.VisorId ?? "null"))
              .Append(",\"pet\":").Append(MiniJson.Str(o?.PetId ?? "null"))
              .Append(",\"plate\":").Append(MiniJson.Str(o?.NamePlateId ?? "null"))
              .Append(",\"color\":").Append(o?.ColorId ?? -1)
              .Append('}');
        }
        sb.Append("]}");
        BridgeServer.EnqueueReliable(sb.ToString());
        return null;
    }

    private static string CmdDebugPlayers(string refJson)
    {
        var g = GuardPlay(Me());
        if (g != null) return g;
        var sb = new System.Text.StringBuilder(512);
        sb.Append("{\"type\":\"debug_players\",\"players\":[");
        var all = PlayerControl.AllPlayerControls;
        var any = false;
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p == null || p.Data == null) continue;
            var pos = p.GetTruePosition();
            if (any) sb.Append(',');
            sb.Append("{\"id\":").Append(p.PlayerId)
              .Append(",\"name\":").Append(MiniJson.Str(p.Data.DefaultOutfit?.PlayerName ?? ""))
              .Append(",\"alive\":").Append(MiniJson.B(!p.Data.IsDead))
              .Append(",\"pos\":[").Append(MiniJson.F(pos.x)).Append(',').Append(MiniJson.F(pos.y)).Append("]}");
            any = true;
        }
        sb.Append("]}");
        BridgeServer.EnqueueReliable(sb.ToString());
        return null;
    }

    // ---------- 自托管:开房 / 开局 / 加 bot / 下一局 ----------

    /// <summary>主菜单开房:local=本地服务器(朋友可用你的IP直连),online=在线大厅(好友列表/房间码加入)。</summary>
    private static string CmdHostGame(Dictionary<string, object> d, string refJson)
    {
        var phase = AIHub.CurrentPhase;
        if (phase != AIPhase.MENU)
            return phase == AIPhase.LOBBY || phase == AIPhase.PLAYING ? null : $"phase_guard:{phase}";
        var mode = (GetStr(d, "mode") ?? "local").ToLowerInvariant();
        var map = (int)(GetNum(d, "map") ?? 4);
        EventTap.Emit("HOST_GAME", null, null, $"mode={mode} map={map}");
        if (mode == "online")
        {
            // 真在线开房:匹配服务器分配主机,好友可从好友列表/房间码加入(Steam 必须在线)
            AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoCreateOnlineGame());
            return null;
        }
        return AIHub.StartLocalGame(mode == "freeplay" ? "freeplay" : "local", map);
    }

    /// <summary>房主在大厅开局(等价点击开始按钮;人数不足时仍会发 StartGame,由游戏自判)。</summary>
    private static string CmdStartGame(Dictionary<string, object> d, string refJson)
    {
        var c = AmongUsClient.Instance;
        if (c == null) return "no_client";
        if (!c.AmHost) return "not_allowed:not_host";
        if (c.GameState != InnerNetClient.GameStates.Joined) return $"phase_guard:{c.GameState}";
        if (AIHub.CurrentPhase != AIPhase.LOBBY) return $"phase_guard:{AIHub.CurrentPhase}";
        var gd = GameData.Instance;
        var min = (int)(GetNum(d, "min_players") ?? 4);
        if (gd != null && gd.PlayerCount < min) return $"not_enough_players:{gd.PlayerCount}/{min}";
        c.StartGame();
        EventTap.Emit("START_GAME", null, null, $"players={gd?.PlayerCount ?? 0}");
        return null;
    }

    /// <summary>结算后回大厅(下一局),由 EndGameNavigation.NextGame 或等价流程完成。</summary>
    private static string CmdNextGame(string refJson)
    {
        var c = AmongUsClient.Instance;
        if (c == null) return "no_client";
        try
        {
            var nav = UObject.FindObjectOfType<EndGameNavigation>();
            if (nav != null)
            {
                nav.NextGame();
                EventTap.Emit("NEXT_GAME", null, null, "EndGameNavigation");
                return null;
            }
            // 没有结算导航(广告页等):直接回大厅
            GameManager.DestroyInstance();
            c.JoinGame();
            EventTap.Emit("NEXT_GAME", null, null, "JoinGame fallback");
            return null;
        }
        catch (Exception e) { return "internal:" + e.Message; }
    }

    private static string CmdLeaveGame(string refJson)
    {
        var c = AmongUsClient.Instance;
        if (c == null) return "no_client";
        try
        {
            c.ExitGame(DisconnectReasons.ExitGame);
            return null;
        }
        catch (Exception e) { return "internal:" + e.Message; }
    }

    /// <summary>本地局添加本地 bot(DummyBehaviour):它们会走动、会议投票,让房间可开局/可整局自托管。
    /// 仅限 LocalGame:dummy 不会网络同步,online 房里只有自己看得见。</summary>
    private static string CmdSpawnBots(Dictionary<string, object> d, string refJson)
    {
        var client = AmongUsClient.Instance;
        if (client == null) return "no_client";
        if (!client.AmHost) return "not_allowed:not_host";
        if (client.NetworkMode != NetworkModes.LocalGame) return "not_allowed:local_bots_only";
        // 大厅预生成 bot 会让开场动画的 CosmeticsCache.PopulateFromPlayers 卡死(实测),
        // 因此 bot 只允许在 PLAYING 里补;大厅阶段留给真人加入。
        if (AIHub.CurrentPhase != AIPhase.PLAYING) return $"phase_guard:{AIHub.CurrentPhase}";
        // 开场动画期间生成会让 bot 卡在界外/抢不到出生流程(实测会毁掉整局)
        if (HudManager.InstanceExists && HudManager.Instance.IsIntroDisplayed) return "intro_guard";
        var gd = GameData.Instance;
        if (gd == null) return "not_found:gamedata";
        var ship = ShipStatus.Instance;
        if (ship == null) return "not_found:ship";
        var prefab = client.PlayerPrefab;
        if (prefab == null) return "not_found:player_prefab";

        var want = (int)(GetNum(d, "count") ?? 3);
        var cap = 10;
        try { cap = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers; } catch { }
        var spawned = 0;
        for (var i = 0; i < want; i++)
        {
            if (gd.PlayerCount >= cap) break;
            var id = gd.GetAvailableId();
            if (id < 0) break;
            try
            {
                var pc = UObject.Instantiate(prefab);
                pc.PlayerId = (byte)id;
                var info = gd.AddPlayer(pc);
                client.Spawn(pc);
                pc.isDummy = true;
                var dummy = pc.GetComponent<DummyBehaviour>();
                if (dummy != null) dummy.enabled = true;
                // NetTransform 必须启用:会议结束 SpawnPlayer 依赖它重新站位(禁用会让 bot 卡在原地)
                pc.SetName("Bot " + (i + 1), true);
                var color = (byte)((i + 1) % Palette.PlayerColors.Length);
                try
                {
                    // 必须给合法的"空外观"ID:AddPlayer 默认是 "missing",
                    // 开场动画加载外观时会 InvalidKeyException 把 intro 卡死(实测)
                    pc.SetColor(color);
                    pc.SetHat("hat_NoHat", color);
                    pc.SetSkin("skin_None", color);
                    pc.SetVisor("visor_EmptyVisor", color);
                    pc.SetPet("pet_EmptyPet");
                    pc.SetNamePlate("nameplate_NoPlate");
                }
                catch { }
                info.DefaultOutfit.dontCensorName = true;
                try
                {
                    // 必须给 bot 真实任务:TotalTasks=0 时任何一次报尸/紧急会议都会
                    // 触发 CheckTaskCompletion -> 立刻判"船员任务胜利"(实测)
                    gd.RpcSetTasks(pc.PlayerId, PickTaskIds());
                }
                catch { }
                // 中途生成:先按会议点出生,再挪到本地玩家身旁(便于自托管局立刻有目标)
                try
                {
                    pc.moveable = true;
                    ship.SpawnPlayer(pc, gd.PlayerCount, initialSpawn: false);
                    var me = PlayerControl.LocalPlayer;
                    if (me != null)
                    {
                        var pos = SafeAround(me.GetTruePosition(), i);
                        // bot 是无主对象(OwnerId=-2),RpcSnapTo 不会生效,必须直接改 transform
                        pc.transform.position = new Vector3(pos.x, pos.y, pc.transform.position.z);
                        try { pc.NetTransform.SnapTo(pos); } catch { }
                        try { pc.MyPhysics.ResetMoveState(); } catch { }
                    }
                }
                catch { }
                spawned++;
            }
            catch (Exception e)
            {
                AIAmongUs.AIBridgePlugin.Log?.LogWarning($"spawn bot {i} failed: {e.Message}");
            }
        }
        EventTap.Emit("SPAWN_BOTS", null, null, $"spawned={spawned} total={gd.PlayerCount}");
        if (spawned == 0) return "spawn_failed";
        return null;
    }

    /// <summary>在 basePos 周围找一个不在墙里的落点(找不到就原地)。</summary>
    private static Vector2 SafeAround(Vector2 basePos, int idx)
    {
        var cand = new[]
        {
            new Vector2(0.7f, 0f), new Vector2(-0.7f, 0f), new Vector2(0f, 0.7f), new Vector2(0f, -0.7f),
            new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f),
        };
        for (var k = 0; k < cand.Length; k++)
        {
            var p = basePos + cand[(idx + k) % cand.Length];
            try
            {
                if (Physics2D.OverlapCircle(p, 0.12f, Constants.ShipAndObjectsMask) == null) return p;
            }
            catch { return p; }
        }
        return basePos;
    }

    /// <summary>把 bot 挪到本地玩家身旁(bot 局专用:会议/过场后假人可能落到图外)。</summary>
    private static string CmdGatherBots(string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var ship = ShipStatus.Instance;
        if (ship == null) return "not_found:ship";
        var basePos = me.GetTruePosition();
        var n = 0;
        var all = PlayerControl.AllPlayerControls;
        for (var i = 0; i < all.Count; i++)
        {
            var p = all[i];
            if (p == null || p == me || p.Data == null || !p.isDummy || p.Data.IsDead) continue;
            var pos = SafeAround(basePos, n);
            p.transform.position = new Vector3(pos.x, pos.y, p.transform.position.z);
            try { p.NetTransform.SnapTo(pos); } catch { }
            n++;
        }
        return n > 0 ? null : "not_found:dummy";
    }

    /// <summary>房主强制结算(bot 局用:mod 胜负判定不认 dummy,1v1 平局不会自然结束)。</summary>
    private static string CmdDebugEndGame(Dictionary<string, object> d, string refJson)
    {
        var client = AmongUsClient.Instance;
        if (client == null) return "no_client";
        if (!client.AmHost) return "not_allowed:not_host";
        if (client.IsGameOver) return "not_allowed:already_over";
        var gm = GameManager.Instance;
        if (gm == null) return "not_found:gamemanager";
        var reason = GameOverReason.ImpostorByKill;
        var rs = GetStr(d, "reason");
        if (rs != null && System.Enum.TryParse<GameOverReason>(rs, true, out var parsed)) reason = parsed;
        try { gm.RpcEndGame(reason, false); }
        catch (Exception e) { return "internal:" + e.Message; }
        EventTap.Emit("END_GAME", null, null, reason.ToString());
        return null;
    }

    /// <summary>本地 bot 自托管局触发会议(假人不会报警)。仅限 LocalGame:freeplay 走杀+报,
    /// 在线真人局必须由真人合法触发(紧急按钮有位置/冷却/次数限制,不能绕过)。</summary>
    private static string CmdDebugCallMeeting(string refJson)
    {
        var me = Me();
        var g = GuardPlay(me);
        if (g != null) return g;
        var client = AmongUsClient.Instance;
        if (client == null) return "no_client";
        if (!client.AmHost) return "not_allowed:not_host";
        if (client.NetworkMode != NetworkModes.LocalGame) return "not_allowed:local_bots_only";
        try
        {
            // 开场动画/已在进行会议时不能开会(实测会让本局瞬间判负并断线)
            if (HudManager.InstanceExists && HudManager.Instance.IsIntroDisplayed) return "intro_guard";
            if (MeetingHud.Instance != null || ExileController.Instance != null) return "already_meeting";
            // 破坏激活时游戏本身不允许开会;这里直接复用游戏的 ReportDeadBody(null)
            me.CmdReportDeadBody(null);
        }
        catch (Exception e) { return "internal:" + e.Message; }
        EventTap.Emit("CALL_MEETING", null, null, "debug");
        return null;
    }

    /// <summary>会议快进:全员未投者跳过(等价投票计时器到点)。任何模式的房主可用;
    /// Results 收尾由 AIHub 自动 Proceed。</summary>
    private static string CmdDebugMeetingFF(string refJson)
    {
        var client = AmongUsClient.Instance;
        if (client == null) return "no_client";
        if (!client.AmHost) return "not_allowed:not_host";
        var mh = MeetingHud.Instance;
        if (mh == null) return "not_found:meeting";
        try { mh.ForceSkipAll(); }
        catch (Exception e) { return "internal:" + e.Message; }
        EventTap.Emit("MEETING_FF", null, null, "force skip all");
        return null;
    }

    /// <summary>从本图任务池取 4 个合法任务索引(Common+Short+Long)。</summary>
    private static byte[] PickTaskIds()
    {
        var ship = ShipStatus.Instance;
        if (ship == null) return new byte[0];
        var pool = new List<byte>();
        void Add(Il2CppReferenceArray<NormalPlayerTask> list)
        {
            if (list == null) return;
            for (var i = 0; i < list.Length; i++)
            {
                var t = list[i];
                if (t != null) pool.Add((byte)t.Index);
            }
        }
        Add(ship.CommonTasks);
        Add(ship.ShortTasks);
        Add(ship.LongTasks);
        if (pool.Count == 0) return new byte[0];
        var n = System.Math.Min(4, pool.Count);
        var ids = new byte[n];
        for (var i = 0; i < n; i++) ids[i] = pool[i];
        return ids;
    }

    /// <summary>设置开局参数(内鬼数/讨论/投票时间等),仅大厅房主有效。</summary>
    private static string CmdSetGameOptions(Dictionary<string, object> d, string refJson)
    {
        var client = AmongUsClient.Instance;
        if (client == null) return "no_client";
        if (!client.AmHost) return "not_allowed:not_host";
        try
        {
            var opts = GameOptionsManager.Instance.CurrentGameOptions;
            var imp = GetNum(d, "num_impostors");
            if (imp != null) opts.SetInt(Int32OptionNames.NumImpostors, (int)imp.Value);
            var disc = GetNum(d, "discussion_time");
            if (disc != null) opts.SetInt(Int32OptionNames.DiscussionTime, (int)disc.Value);
            var vote = GetNum(d, "voting_time");
            if (vote != null) opts.SetInt(Int32OptionNames.VotingTime, (int)vote.Value);
            var speed = GetNum(d, "player_speed");
            if (speed != null) opts.SetFloat(FloatOptionNames.PlayerSpeedMod, (float)speed.Value);
            var emerg = GetNum(d, "emergency_cooldown");
            if (emerg != null) opts.SetInt(Int32OptionNames.EmergencyCooldown, (int)emerg.Value);
            var killcd = GetNum(d, "kill_cooldown");
            if (killcd != null) opts.SetFloat(FloatOptionNames.KillCooldown, (float)killcd.Value);
            var killdist = GetNum(d, "kill_distance");
            if (killdist != null) opts.SetInt(Int32OptionNames.KillDistance, (int)killdist.Value);
            GameOptionsManager.Instance.GameHostOptions = opts;   // 落盘,重开也生效
            return null;
        }
        catch (Exception e) { return "internal:" + e.Message; }
    }
}
