using TheOtherRoles.Attributes;
using TheOtherRoles.Objects;

namespace TheOtherRoles.Roles.Crewmate;

public static class TrapperPlus
{
    private sealed class OwnerState
    {
        public int RemainingTraps;
        public readonly Dictionary<byte, RoleId> TriggeredPlayers = new();
        public TrapperPlusReport LastReport = EmptyReport();

        public OwnerState(int uses)
        {
            RemainingTraps = uses;
        }
    }

    public static PlayerControl Player;
    public static Color color = new Color32(167, 209, 179, byte.MaxValue);

    public static float cooldown = 10f;
    public static float minDwellTime = 1f;
    public static bool removeTrapsEachRound = true;
    public static int maxTraps = 5;
    public static float trapSize = 0.25f;
    public static int minPlayersToReport = 3;

    public static Sprite trapButtonSprite = new ResourceSprite("TrapperPlus_Place_Button.png");

    private static readonly Dictionary<byte, OwnerState> ownerStates = new();
    private static readonly Dictionary<byte, float> lastPlacementAt = new();

    public static PlayerControl trapper
    {
        get => Player;
        set => Player = value;
    }

    public static int charges
    {
        get => GetRemainingTraps(Player);
        set
        {
            if (Player != null) GetOrCreateState(Player.PlayerId).RemainingTraps = Math.Max(0, value);
        }
    }

    public static void clearAndReload()
    {
        InfoTrap.ClearAll();
        Player = null;
        ownerStates.Clear();
        lastPlacementAt.Clear();

        cooldown = CustomOptionHolder.trapperPlusCooldown.GetFloat();
        minDwellTime = CustomOptionHolder.trapperPlusMinDwellTime.GetFloat();
        removeTrapsEachRound = CustomOptionHolder.trapperPlusRemoveTrapsEachRound.GetBool();
        maxTraps = Math.Max(1, CustomOptionHolder.trapperPlusMaxTraps.GetInt());
        trapSize = Math.Max(0.01f, CustomOptionHolder.trapperPlusTrapSize.GetFloat());
        minPlayersToReport = Math.Max(1, CustomOptionHolder.trapperPlusMinPlayersToReport.GetInt());
    }

    public static void ClearAndReload()
    {
        clearAndReload();
    }

    public static bool CanPlaceTrap(PlayerControl owner = null)
    {
        owner ??= Player;
        return owner != null && owner.IsAlive() && GetRemainingTraps(owner) > 0;
    }

    /// <summary>按钮点击入口：本地请求放置，房主负责验证与生成。</summary>
    public static void PlaceLocalInfoTrap()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local != Player || !local.CanMove || !InGame || Helpers.InMeeting ||
            ExileController.Instance != null || !CanPlaceTrap(local))
            return;

        var position = local.transform.position;
        if (AmongUsClient.Instance.AmHost)
        {
            ServerPlaceInfoTrap(local, position);
            return;
        }

        var writer = StartRPC(CustomRPC.PlaceInfoTrap, HostPlayer);
        writer.Write((byte)0);
        writer.Write(local.PlayerId);
        writer.Write(position);
        writer.EndRPC();
    }

    /// <summary>房主放置：完整验证后通过 NetworkCustomObject 生成并同步。</summary>
    public static void ServerPlaceInfoTrap(PlayerControl owner, Vector3 position)
    {
        if (!AmongUsClient.Instance.AmHost || owner == null || owner != Player) return;
        if (!owner.IsAlive() || !owner.CanMove || !InGame || Helpers.InMeeting || ExileController.Instance != null) return;
        if (!CanPlaceTrap(owner)) return;

        // 位置校验：防止客户端伪造远处落点（与低版本一致，容差 0.75）
        if (Vector2.Distance(owner.GetTruePosition(), position) > 0.75f)
        {
            Message($"信息陷阱放置被拒绝: 落点距离过远 ({Vector2.Distance(owner.GetTruePosition(), position):0.00})", "TrapperPlus");
            return;
        }

        var now = Time.realtimeSinceStartup;
        if (lastPlacementAt.TryGetValue(owner.PlayerId, out var last) && now - last < cooldown)
        {
            Message($"信息陷阱放置被拒绝: 冷却中 (剩余 {cooldown - (now - last):0.0}s)", "TrapperPlus");
            return;
        }
        lastPlacementAt[owner.PlayerId] = now;

        var trap = new InfoTrap(owner, position);
        NetworkCustomObject.NetworkSpawn(trap, [owner.PlayerId]);
        // 房主侧直接扣次数：NetworkSpawn 的本地 HandleSpawn 会因对象已存在而提前返回，
        // 负责扣次的工厂只会在其他客户端执行。
        NotifyTrapPlaced(owner.PlayerId);
        Message($"信息陷阱已放置: 位置={position} 半径={trap.Radius:0.00} 剩余={GetRemainingTraps(owner)}", "TrapperPlus");
    }

    /// <summary>所有客户端在陷阱生成时同步扣减剩余次数（房主经 NetworkSpawn，其余客户端经生成广播）。</summary>
    internal static void NotifyTrapPlaced(byte ownerId)
    {
        var state = GetOrCreateState(ownerId);
        state.RemainingTraps = Math.Max(0, state.RemainingTraps - 1);
    }

    /// <summary>房主确认停留：记录触发并广播给所有客户端。</summary>
    internal static void ConfirmHostDwell(InfoTrap trap, PlayerControl target)
    {
        if (trap == null || target == null || !AmongUsClient.Instance.AmHost) return;

        var roleSnapshot = GetPrimaryRole(target);
        var writer = StartRPC(CustomRPC.TriggerInfoTrap);
        writer.Write(trap.OwnerId);
        writer.Write((ushort)trap.NetworkId);
        writer.Write(target.PlayerId);
        writer.Write((byte)roleSnapshot);
        writer.EndRPC();
        RegisterTrigger(trap.OwnerId, trap.NetworkId, target.PlayerId, roleSnapshot);
        Message($"信息陷阱记录: 目标={target.Data?.PlayerName ?? target.PlayerId.ToString()} 职业={roleSnapshot} 本轮已记录={GetOrCreateState(trap.OwnerId).TriggeredPlayers.Count}", "TrapperPlus");
    }

    public static bool RegisterTrigger(byte ownerId, ushort trapId, byte targetId, RoleId roleSnapshot)
    {
        var trap = InfoTrap.Find(trapId);
        if (trap == null || trap.OwnerId != ownerId) return false;

        trap.MarkTargetCompleted(targetId);

        var state = GetOrCreateState(ownerId);
        if (state.TriggeredPlayers.ContainsKey(targetId)) return false;
        state.TriggeredPlayers.Add(targetId, roleSnapshot);
        return true;
    }

    public static RoleId GetPrimaryRole(PlayerControl target)
    {
        if (target == null) return RoleId.DefaultRole;

        var roleId = PlayerData.GetPlayerData(target)?.MainRole ?? RoleId.DefaultRole;
        if (roleId != RoleId.DefaultRole) return roleId;

        if (target.Data?.Role?.IsImpostor == true) return RoleId.Impostor;
        return RoleId.Crewmate;
    }

    /// <summary>会议开始：生成报告、按选项清理陷阱。</summary>
    [OnMeetingStart]
    public static void HandleMeetingStart()
    {
        var localId = PlayerControl.LocalPlayer?.PlayerId;
        var activeOwnerId = Player != null && Player.Data?.Disconnected == false ? Player.PlayerId : (byte?)null;
        if (Player != null) GetOrCreateState(Player.PlayerId);

        // 回收已离场房主遗留的状态与陷阱（按 id 清理，避免误删当前陷阱师的陷阱）
        foreach (var staleOwnerId in ownerStates.Keys.Where(id => id != activeOwnerId).ToArray())
        {
            InfoTrap.ClearAllById(staleOwnerId);
            ownerStates.Remove(staleOwnerId);
        }

        foreach (var (ownerId, state) in ownerStates.ToArray())
        {
            state.LastReport = BuildReport(state);
            var recorded = state.TriggeredPlayers.Count;
            state.TriggeredPlayers.Clear();

            if (localId == ownerId || CanSeeGhostInfo)
            {
                var owner = PlayerById(ownerId);
                var chat = FastDestroyableSingleton<HudManager>.Instance?.Chat;
                if (owner != null && chat != null)
                    chat.AddChat(owner, state.LastReport.ToLocalizedText(), true);
            }

            Message($"会议结算信息陷阱: 记录人数={recorded} 门槛={minPlayersToReport} 报告={state.LastReport.State}", "TrapperPlus");

            if (removeTrapsEachRound) state.RemainingTraps = maxTraps;
        }

        foreach (var trap in NetworkCustomObject.AllObjects.OfType<InfoTrap>().ToArray())
        {
            if (removeTrapsEachRound) trap?.Destroy();
            else trap?.ResetForNextRound();
        }
    }

    public static TrapperPlusReport GetLastReport(PlayerControl owner = null)
    {
        owner ??= Player;
        if (owner == null || !ownerStates.TryGetValue(owner.PlayerId, out var state)) return EmptyReport();
        return state.LastReport;
    }

    public static IReadOnlyDictionary<byte, RoleId> GetRoundEvidence(PlayerControl owner = null)
    {
        owner ??= Player;
        if (owner == null) return new Dictionary<byte, RoleId>();
        return new Dictionary<byte, RoleId>(GetOrCreateState(owner.PlayerId).TriggeredPlayers);
    }

    public static int GetRemainingTraps(PlayerControl owner = null)
    {
        owner ??= Player;
        return owner == null ? 0 : GetOrCreateState(owner.PlayerId).RemainingTraps;
    }

    public static void ResetOwnerState(PlayerControl owner, bool destroyOwnedTraps = true)
    {
        if (owner == null) return;
        if (destroyOwnedTraps) InfoTrap.ClearAll(owner);
        ownerStates[owner.PlayerId] = new OwnerState(maxTraps);
    }

    public static void RemoveOwnerState(PlayerControl owner, bool destroyOwnedTraps = true)
    {
        if (owner == null) return;
        if (destroyOwnedTraps) InfoTrap.ClearAll(owner);
        ownerStates.Remove(owner.PlayerId);
    }

    internal static bool IsActiveOwner(byte ownerId)
    {
        if (Player == null || Player.PlayerId != ownerId) return false;
        return Player.Data == null || !Player.Data.Disconnected;
    }

    [OnGameStart(50)]
    public static void ResetHostRateLimits()
    {
        lastPlacementAt.Clear();
    }

    public static void BeginActionRoundCooldowns()
    {
        lastPlacementAt.Clear();
    }

    public static void HandlePlacementRpc(PlayerControl sender, MessageReader reader)
    {
        var action = reader.ReadByte();
        var playerId = reader.ReadByte();
        switch (action)
        {
            case 0:
            {
                if (!AmongUsClient.Instance.AmHost || sender == null || sender.PlayerId != playerId) return;
                var position = reader.ReadVector3();
                var owner = PlayerById(playerId);
                if (owner == null) return;
                ServerPlaceInfoTrap(owner, position);
                break;
            }
        }
    }

    public static void HandleTriggerRpc(PlayerControl sender, MessageReader reader)
    {
        var ownerId = reader.ReadByte();
        var trapId = reader.ReadUInt16();
        var targetId = reader.ReadByte();
        var roleSnapshot = (RoleId)reader.ReadByte();
        if (sender != HostPlayer) return;
        RegisterTrigger(ownerId, trapId, targetId, roleSnapshot);
    }

    private static OwnerState GetOrCreateState(byte ownerId)
    {
        if (!ownerStates.TryGetValue(ownerId, out var state))
        {
            state = new OwnerState(maxTraps);
            ownerStates.Add(ownerId, state);
        }
        return state;
    }

    private static TrapperPlusReport BuildReport(OwnerState state)
    {
        if (state.TriggeredPlayers.Count < minPlayersToReport)
            return new TrapperPlusReport(TrapperPlusReport.ReportState.NotEnoughPlayers, []);

        var roles = state.TriggeredPlayers.Values.ToList();
        for (var i = roles.Count - 1; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (roles[i], roles[j]) = (roles[j], roles[i]);
        }
        return new TrapperPlusReport(TrapperPlusReport.ReportState.Ready, roles);
    }

    private static TrapperPlusReport EmptyReport()
        => new(TrapperPlusReport.ReportState.NoPlayers, []);
}
