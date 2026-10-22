using TheOtherRoles.Objects;

namespace TheOtherRoles.Roles.Crewmate;

public static class Imitator
{
    public sealed class Selection
    {
        public byte TargetId { get; internal set; } = byte.MaxValue;
        public RoleId Role { get; internal set; }
    }

    public readonly record struct AuthorizedStart(byte ImitatorId, RoleId Role, uint MeetingGeneration);

    private sealed class MeetingAuthorization
    {
        public byte TargetId;
        public RoleId Role;
        public uint MeetingGeneration;
    }

    private sealed class PendingRedemption
    {
        public byte TargetId;
        public uint SessionGeneration;
        public bool SelfSacrificeConfirmed;
        public float ExpiresAt;
    }

    private sealed class MeetingCarry
    {
        public byte ImitatorId;
        public RoleId CopiedRole;
        public string OracleReport;
        public byte JailedId = byte.MaxValue;
        public int JailUses;
        public TrapperPlusReport TrapperReport;
    }

    /// <summary>对目标职业类型的所有静态字段做快照，借用结束时恢复。</summary>
    private sealed class StaticSnapshot
    {
        private readonly List<(FieldInfo Field, object Value)> values = new();

        public StaticSnapshot(params Type[] types)
        {
            foreach (var type in types.Where(t => t != null).Distinct())
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.IsLiteral || field.IsInitOnly) continue;
                    try
                    {
                        values.Add((field, field.GetValue(null)));
                    }
                    catch (Exception e)
                    {
                        Warn($"Could not snapshot {type.Name}.{field.Name}: {e.Message}");
                    }
                }
            }
        }

        public void Restore()
        {
            foreach (var (field, value) in values)
            {
                try
                {
                    field.SetValue(null, value);
                }
                catch (Exception e)
                {
                    Warn($"Could not restore {field.DeclaringType?.Name}.{field.Name}: {e.Message}");
                }
            }
        }
    }

    /// <summary>通风格管外观快照（借用安保借用时恢复）。</summary>
    private sealed class VentSnapshot
    {
        private readonly Vent vent;
        private readonly string name;
        private readonly Sprite sprite;
        private readonly Color color;
        private readonly Sprite fungleSprite;
        private readonly Color fungleColor;
        private readonly Dictionary<FieldInfo, object> animationFields = new();

        public VentSnapshot(Vent vent)
        {
            this.vent = vent;
            if (vent == null) return;

            name = vent.name;
            sprite = vent.myRend?.sprite;
            color = vent.myRend != null ? vent.myRend.color : Color.white;

            if (vent.transform.childCount > 3)
            {
                var child = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                if (child != null)
                {
                    fungleSprite = child.sprite;
                    fungleColor = child.color;
                }
            }

            foreach (var fieldName in new[] { "EnterVentAnim", "ExitVentAnim" })
            {
                var field = typeof(Vent).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) animationFields[field] = field.GetValue(vent);
            }
        }

        public void Restore()
        {
            if (vent == null) return;

            vent.name = name;
            if (vent.myRend != null)
            {
                vent.myRend.sprite = sprite;
                vent.myRend.color = color;
            }

            if (vent.transform.childCount > 3)
            {
                var child = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                if (child != null)
                {
                    child.sprite = fungleSprite;
                    child.color = fungleColor;
                }
            }

            foreach (var (field, value) in animationFields) field.SetValue(vent, value);
        }
    }

    private sealed class ImitationSession
    {
        private readonly PlayerControl imitator;
        private readonly byte sourceTargetId;
        private readonly PlayerControl originalHolder;
        private readonly StaticSnapshot roleSnapshot;
        private readonly StaticSnapshot portalSnapshot;
        private readonly Aurial.SessionSnapshot aurialSnapshot;
        private readonly List<SurvCamera> originalPendingCameras;
        private readonly List<Vent> originalPendingVents;
        private readonly List<VentSnapshot> ventSnapshots;
        private readonly bool originalFirstPortalActive;
        private readonly bool originalSecondPortalActive;
        private readonly List<(Trap Trap, bool GameObjectWasActive, bool ArrowWasActive)> suspendedLegacyTraps;
        private readonly bool localPlayerWasHandcuffed;
        private readonly List<GameObject> localSeerSouls = new();
        private readonly List<SpriteRenderer> localAlchemystSouls = new();
        private readonly List<(GameObject Object, bool WasActive)> hiddenSourceObjects = new();

        internal bool dreamKillConsumed;

        private bool originalHolderInvalidated;

        public RoleId Role { get; }
        public uint Generation { get; }
        public bool LocalPlayerWasHandcuffed => localPlayerWasHandcuffed;

        public ImitationSession(PlayerControl imitator, byte sourceTargetId, RoleId role)
        {
            this.imitator = imitator;
            this.sourceTargetId = sourceTargetId;
            Role = role;
            Generation = NextSessionGeneration();

            originalHolder = role == RoleId.Aurial ? Aurial.aurial : GetOriginalHolder(role, sourceTargetId);

            if (role is RoleId.Sheriff or RoleId.Deputy && PlayerControl.LocalPlayer != null)
            {
                var localId = PlayerControl.LocalPlayer.PlayerId;
                localPlayerWasHandcuffed = Sheriff.handcuffedPlayers?.Contains(localId) == true
                    || Sheriff.handcuffedKnows?.ContainsKey(localId) == true;
            }

            var stateType = GetStateType(role);
            if (role != RoleId.Aurial) roleSnapshot = new StaticSnapshot(stateType);

            if (role == RoleId.Portalmaker)
            {
                portalSnapshot = new StaticSnapshot(typeof(Portal));
                originalFirstPortalActive = Portal.firstPortal?.portalGameObject?.activeSelf == true;
                originalSecondPortalActive = Portal.secondPortal?.portalGameObject?.activeSelf == true;
                if (Portal.firstPortal?.portalGameObject != null) Portal.firstPortal.portalGameObject.SetActive(false);
                if (Portal.secondPortal?.portalGameObject != null) Portal.secondPortal.portalGameObject.SetActive(false);
            }

            if (role == RoleId.SecurityGuard)
            {
                originalPendingCameras = ModOption.camerasToAdd;
                originalPendingVents = ModOption.ventsToSeal;
                var ship = MapUtilities.CachedShipStatus;
                ventSnapshots = ship?.AllVents?.Where(v => v != null).Select(v => new VentSnapshot(v)).ToList() ?? new List<VentSnapshot>();
                ModOption.camerasToAdd = new List<SurvCamera>();
                ModOption.ventsToSeal = new List<Vent>();
            }

            if (role == RoleId.Trapper)
            {
                suspendedLegacyTraps = NetworkCustomObject.AllObjects.OfType<Trap>()
                    .Where(trap => trap != null && trap.trapper != imitator)
                    .Select(trap => (Trap: trap,
                        GameObjectWasActive: trap.GameObject != null && trap.GameObject.activeSelf,
                        ArrowWasActive: trap.ArrowObject != null && trap.ArrowObject.activeSelf))
                    .ToList();
                foreach (var (trap, _, _) in suspendedLegacyTraps)
                {
                    trap.GameObject?.SetActive(false);
                    trap.ArrowObject?.SetActive(false);
                }
            }

            CaptureAndHideSourceObjects(role);

            try
            {
                if (role == RoleId.Aurial) aurialSnapshot = Aurial.BeginImitation(imitator);
                else InitializeFreshRole(imitator, role);
            }
            catch
            {
                RestoreOriginalState();
                throw;
            }
        }

        public MeetingCarry Finish(bool preserveMeetingCarry)
        {
            MeetingCarry result = null;
            try
            {
                try
                {
                    if (preserveMeetingCarry) result = CaptureMeetingCarry();
                }
                catch (Exception e)
                {
                    Warn($"Could not capture borrowed {Role} meeting data: {e}");
                }

                try
                {
                    CleanupBorrowedState();
                }
                catch (Exception e)
                {
                    Warn($"Borrowed {Role} cleanup failed: {e}");
                }
            }
            finally
            {
                RestoreOriginalState();
            }
            return result;
        }

        public void InvalidateOriginalHolder(byte playerId)
        {
            if (originalHolder?.PlayerId == playerId) originalHolderInvalidated = true;
        }

        public bool SourceIs(byte playerId) => sourceTargetId == playerId;

        public bool OriginalHolderIs(byte playerId) => originalHolder?.PlayerId == playerId;

        /// <summary>本地玩家的击杀可视化（Seer 灵魂、Alchemyst 灵魂）。</summary>
        public void RecordLocalDeath(PlayerControl deadPlayer, PlayerControl killer, CustomDeathReason deathReason)
        {
            if (deadPlayer == null || deadPlayer == imitator) return;
            if (imitator == null || !imitator.AmOwner || !imitator.IsAlive()) return;

            var position = deadPlayer.transform.position;
            switch (Role)
            {
                case RoleId.Seer:
                {
                    if (Seer.mode is not (0 or 2)) break;
                    var renderer = CreateLocalSoul("Imitator Seer Soul", position, Seer.soulSprite);
                    if (renderer == null) break;

                    localSeerSouls.Add(renderer.gameObject);

                    if (Seer.limitSoulDuration)
                    {
                        var soul = renderer.gameObject;
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration, new Action<float>(p =>
                        {
                            if (renderer != null)
                            {
                                var color = renderer.color;
                                color.a = Mathf.Clamp01(1f - p);
                                renderer.color = color;
                            }
                            if (p == 1f)
                            {
                                if (soul != null) UObject.Destroy(soul);
                                localSeerSouls.Remove(soul);
                            }
                        })));
                    }
                    break;
                }
                case RoleId.Alchemyst:
                {
                    var entry = new Alchemyst.DeadPlayer(deadPlayer, DateTime.UtcNow, deathReason, killer ?? deadPlayer, position);
                    Alchemyst.deadBodies ??= new List<Tuple<Alchemyst.DeadPlayer, Vector3>>();
                    Alchemyst.deadBodies.Add(Tuple.Create(entry, position));

                    var soulRenderer = CreateLocalSoul("Imitator Alchemyst Soul", position, Alchemyst.soulSprite);
                    if (soulRenderer != null)
                    {
                        Alchemyst.souls ??= new List<SpriteRenderer>();
                        Alchemyst.souls.Add(soulRenderer);
                        localAlchemystSouls.Add(soulRenderer);
                    }
                    break;
                }
            }
        }

        private void RestoreOriginalState()
        {
            List<PlayerControl> sheriffList = null;
            List<byte> handcuffedPlayers = null;
            Dictionary<byte, float> handcuffedKnows = null;
            PlayerControl deputy = null;
            PlayerControl formerDeputy = null;
            var remainingHandcuffs = 0f;

            if (Role == RoleId.Sheriff)
            {
                sheriffList = Sheriff.Player?.Where(x => x != null && x.PlayerId != imitator.PlayerId).ToList() ?? new List<PlayerControl>();
                handcuffedPlayers = new List<byte>(Sheriff.handcuffedPlayers ?? new List<byte>());
                handcuffedKnows = new Dictionary<byte, float>(Sheriff.handcuffedKnows ?? new Dictionary<byte, float>());
                deputy = Sheriff.Deputy;
                formerDeputy = Sheriff.formerDeputy;
                remainingHandcuffs = Sheriff.remainingHandcuffs;
            }

            if (Role == RoleId.Aurial)
            {
                Aurial.EndImitation(aurialSnapshot, !originalHolderInvalidated && IsStillMainRole(aurialSnapshot?.Holder, Role));
            }
            else
            {
                roleSnapshot?.Restore();
                if (originalHolderInvalidated)
                {
                    ClearRestoredHolder(Role, originalHolder?.PlayerId ?? byte.MaxValue);
                    if (Role == RoleId.TrapperPlus) TrapperPlus.RemoveOwnerState(originalHolder);
                }
            }

            if (Role == RoleId.Sheriff)
            {
                if (originalHolderInvalidated && originalHolder != null)
                    sheriffList.RemoveAll(x => x?.PlayerId == originalHolder.PlayerId);

                Sheriff.Player = sheriffList;
                Sheriff.handcuffedPlayers = handcuffedPlayers;
                Sheriff.handcuffedKnows = handcuffedKnows;
                Sheriff.Deputy = deputy;
                Sheriff.formerDeputy = formerDeputy;
                Sheriff.remainingHandcuffs = remainingHandcuffs;
            }

            if (Role == RoleId.Portalmaker)
            {
                portalSnapshot?.Restore();
                if (Portal.firstPortal?.portalGameObject != null)
                    Portal.firstPortal.portalGameObject.SetActive(originalFirstPortalActive);
                if (Portal.secondPortal?.portalGameObject != null)
                    Portal.secondPortal.portalGameObject.SetActive(originalSecondPortalActive);
            }

            if (Role == RoleId.SecurityGuard)
            {
                ModOption.camerasToAdd = originalPendingCameras ?? new List<SurvCamera>();
                ModOption.ventsToSeal = originalPendingVents ?? new List<Vent>();
            }

            if (Role == RoleId.Trapper)
            {
                foreach (var (trap, gameObjectWasActive, arrowWasActive) in suspendedLegacyTraps ?? new List<(Trap, bool, bool)>())
                {
                    if (trap == null) continue;
                    if (originalHolderInvalidated && trap.trapper == originalHolder)
                    {
                        trap.Destroy();
                        continue;
                    }
                    trap.GameObject?.SetActive(gameObjectWasActive);
                    trap.ArrowObject?.SetActive(arrowWasActive);
                }
            }

            foreach (var (obj, wasActive) in hiddenSourceObjects)
                if (obj != null) obj.SetActive(wasActive);
        }

        private void CaptureAndHideSourceObjects(RoleId role)
        {
            switch (role)
            {
                case RoleId.Tracker:
                    HideArrows(Tracker.localArrows);
                    Hide(Tracker.arrow?.arrow);
                    Hide(Tracker.DangerMeterParent);
                    break;
                case RoleId.Snitch:
                    HideArrows(Snitch.localArrows);
                    Hide(Snitch.text?.gameObject);
                    break;
                case RoleId.Prophet:
                    HideArrows(Prophet.arrows);
                    break;
            }

            void Hide(GameObject obj)
            {
                if (obj == null || hiddenSourceObjects.Any(x => x.Object == obj)) return;
                hiddenSourceObjects.Add((obj, obj.activeSelf));
                obj.SetActive(false);
            }

            void HideArrows(IEnumerable<Arrow> arrows)
            {
                foreach (var arrow in arrows?.ToArray() ?? Array.Empty<Arrow>()) Hide(arrow?.arrow);
            }
        }

        private MeetingCarry CaptureMeetingCarry()
        {
            var carry = new MeetingCarry { ImitatorId = imitator.PlayerId, CopiedRole = Role };

            if (Role == RoleId.TrapperPlus)
            {
                var roles = TrapperPlus.GetRoundEvidence(imitator).Values.ToList();
                var state = roles.Count < TrapperPlus.minPlayersToReport
                    ? TrapperPlusReport.ReportState.NotEnoughPlayers
                    : TrapperPlusReport.ReportState.Ready;
                if (state == TrapperPlusReport.ReportState.Ready)
                {
                    for (var i = roles.Count - 1; i > 0; i--)
                    {
                        var j = rnd.Next(i + 1);
                        (roles[i], roles[j]) = (roles[j], roles[i]);
                    }
                }
                carry.TrapperReport = new TrapperPlusReport(state, roles);
            }
            else if (Role == RoleId.Oracle && imitator.AmOwner && imitator.IsAlive() && Oracle.Confesser != null)
            {
                carry.OracleReport = Oracle.BuildReport();
            }
            else if (Role == RoleId.Jailor && Jailor.Jailed != null)
            {
                carry.JailedId = Jailor.Jailed.PlayerId;
                carry.JailUses = Jailor.usesCount;
            }
            return carry;
        }

        private void CleanupBorrowedState()
        {
            switch (Role)
            {
                case RoleId.Portalmaker:
                    if (Portal.firstPortal?.portalGameObject != null) UObject.Destroy(Portal.firstPortal.portalGameObject);
                    if (Portal.secondPortal != Portal.firstPortal && Portal.secondPortal?.portalGameObject != null)
                        UObject.Destroy(Portal.secondPortal.portalGameObject);
                    Portal.firstPortal = null;
                    Portal.secondPortal = null;
                    Portal.bothPlacedAndEnabled = false;
                    Portal.isTeleporting = false;
                    Portal.teleportedPlayers?.Clear();
                    break;
                case RoleId.Tracker:
                    DestroyArrows(Tracker.localArrows);
                    if (Tracker.arrow?.arrow != null) UObject.Destroy(Tracker.arrow.arrow);
                    if (Tracker.DangerMeterParent != null) UObject.Destroy(Tracker.DangerMeterParent);
                    break;
                case RoleId.Detective:
                    FootprintHolder.Instance?.ClearFootprints(imitator);
                    break;
                case RoleId.Seer:
                    foreach (var soul in localSeerSouls.ToArray())
                        if (soul != null) UObject.Destroy(soul);
                    localSeerSouls.Clear();
                    break;
                case RoleId.Snitch:
                    DestroyArrows(Snitch.localArrows);
                    if (Snitch.text != null) UObject.Destroy(Snitch.text);
                    break;
                case RoleId.Prophet:
                    DestroyArrows(Prophet.arrows);
                    break;
                case RoleId.SecurityGuard:
                    if (imitator?.AmOwner == true) CloseBorrowedDevices(Role);
                    foreach (var camera in ModOption.camerasToAdd?.ToArray() ?? Array.Empty<SurvCamera>())
                        if (camera != null) UObject.Destroy(camera.gameObject);
                    foreach (var ventSnapshot in ventSnapshots ?? new List<VentSnapshot>()) ventSnapshot.Restore();
                    ModOption.camerasToAdd = new List<SurvCamera>();
                    ModOption.ventsToSeal = new List<Vent>();
                    break;
                case RoleId.Alchemyst:
                    foreach (var soul in localAlchemystSouls.ToArray())
                        if (soul != null) UObject.Destroy(soul.gameObject);
                    localAlchemystSouls.Clear();
                    break;
                case RoleId.Hacker:
                    if (imitator?.AmOwner == true) CloseBorrowedDevices(Role);
                    break;
                case RoleId.Veteran:
                    Veteran.alertActive = false;
                    break;
                case RoleId.Trapper:
                    Trap.ClearAllTraps(imitator, true);
                    break;
                case RoleId.Redemptor:
                    if (Redemptor.arrow?.arrow != null) UObject.Destroy(Redemptor.arrow.arrow);
                    if (Redemptor.text != null) UObject.Destroy(Redemptor.text);
                    Redemptor.Prayering = false;
                    Redemptor.Revelating = false;
                    break;
                case RoleId.TrapperPlus:
                    TrapperPlus.RemoveOwnerState(imitator);
                    break;
            }

            if (Role is RoleId.Engineer or RoleId.Spy) ExitBorrowedVent();

            if (imitator?.AmOwner == true) ResetBorrowedButtons(Role);
        }

        private void ExitBorrowedVent()
        {
            if (imitator?.AmOwner != true || !imitator.inVent || imitator.MyPhysics == null) return;

            if (Vent.currentVent != null) imitator.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            imitator.MyPhysics.ExitAllVents();
        }
    }

    public static PlayerControl Player;
    public static readonly Color color = new Color32(179, 217, 77, byte.MaxValue);

    public static Sprite selectSprite = new ResourceSprite("ImitateSelect.png");
    public static Sprite deselectSprite = new ResourceSprite("ImitateDeselect.png");

    private static readonly Dictionary<byte, Selection> selections = new();
    private static readonly Dictionary<byte, ImitationSession> sessions = new();
    private static readonly Dictionary<byte, MeetingCarry> meetingCarries = new();
    private static readonly HashSet<byte> consumedSourceIds = new();
    private static readonly Dictionary<byte, MeetingAuthorization> meetingAuthorizations = new();
    private static readonly HashSet<byte> meetingSelectionRequests = new();
    private static readonly Dictionary<byte, PendingRedemption> pendingRedemptions = new();

    private static uint nextSessionGeneration;
    private static uint meetingGeneration;
    private static uint hostStartedMeetingGeneration;
    private static bool meetingSelectionOpen;

    public static readonly HashSet<RoleId> CompatibleRoles =
    [
        RoleId.Portalmaker,
        RoleId.Engineer,
        RoleId.Sheriff,
        RoleId.Deputy,
        RoleId.BodyGuard,
        RoleId.Jumper,
        RoleId.Detective,
        RoleId.Veteran,
        RoleId.Medic,
        RoleId.Seer,
        RoleId.Hacker,
        RoleId.Tracker,
        RoleId.Snitch,
        RoleId.Prophet,
        RoleId.Spy,
        RoleId.SecurityGuard,
        RoleId.Alchemyst,
        RoleId.Trapper,
        RoleId.Redemptor,
        RoleId.Jailor,
        RoleId.Dreamcatcher,
        RoleId.Oracle,
        RoleId.TrapperPlus,
        RoleId.Aurial
    ];

    public static byte SelectedTargetId
        => Player != null && selections.TryGetValue(Player.PlayerId, out var selection) ? selection.TargetId : byte.MaxValue;

    public static RoleId SelectedRole
        => Player != null && selections.TryGetValue(Player.PlayerId, out var selection) ? selection.Role : RoleId.DefaultRole;

    public static RoleId ActiveCopiedRole
        => Player != null && sessions.TryGetValue(Player.PlayerId, out var session) ? session.Role : RoleId.DefaultRole;

    public static bool IsActive => Player != null && sessions.ContainsKey(Player.PlayerId);

    public static bool HasAnyActiveSession => sessions.Count > 0;

    public static uint CurrentMeetingGeneration => meetingGeneration;

    private static uint NextSessionGeneration()
    {
        nextSessionGeneration++;
        if (nextSessionGeneration == 0) nextSessionGeneration++;
        return nextSessionGeneration;
    }

    public static bool IsActiveFor(byte playerId) => sessions.ContainsKey(playerId);

    public static void BeginMeeting()
    {
        pendingRedemptions.Clear();
        meetingGeneration++;
        if (meetingGeneration == 0) meetingGeneration++;
        meetingSelectionOpen = true;
        meetingAuthorizations.Clear();
        meetingSelectionRequests.Clear();

        foreach (var imitatorId in selections.Keys.ToArray()) ClearSelection(imitatorId, false);
    }

    public static Selection GetSelection(byte imitatorId)
    {
        if (!selections.TryGetValue(imitatorId, out var selection))
        {
            selection = new Selection();
            selections[imitatorId] = selection;
        }
        return selection;
    }

    public static void SetSelection(byte imitatorId, byte targetId, RoleId roleSnapshot)
    {
        var selection = GetSelection(imitatorId);
        if (targetId == byte.MaxValue || !IsCompatible(roleSnapshot))
        {
            selection.TargetId = byte.MaxValue;
            selection.Role = RoleId.DefaultRole;
            return;
        }

        var target = PlayerById(targetId);
        if (target?.Data == null || !target.Data.IsDead || target.Data.Disconnected || GetEligibleRole(target) != roleSnapshot)
        {
            selection.TargetId = byte.MaxValue;
            selection.Role = RoleId.DefaultRole;
        }
        else
        {
            selection.TargetId = targetId;
            selection.Role = roleSnapshot;
        }
    }

    public static bool TryHostAuthorizeSelection(PlayerControl sender, byte imitatorId, byte targetId, RoleId roleSnapshot,
        uint generation, out byte canonicalTargetId, out RoleId canonicalRole)
    {
        canonicalTargetId = byte.MaxValue;
        canonicalRole = RoleId.DefaultRole;

        if (!AmongUsClient.Instance.AmHost || !meetingSelectionOpen || MeetingHud.Instance == null) return false;
        if (generation == 0 || generation != meetingGeneration)
        {
            Message($"效颦者授权失败: 会话代际不符 ({generation} vs {meetingGeneration})", "Imitator");
            return false;
        }
        if (sender == null || sender.PlayerId != imitatorId || meetingSelectionRequests.Contains(imitatorId))
        {
            Message($"效颦者授权失败: 请求者校验未过 (sender={sender?.PlayerId}, imitator={imitatorId}, 本轮已请求={meetingSelectionRequests.Contains(imitatorId)})", "Imitator");
            return false;
        }

        var imitator = PlayerById(imitatorId);
        if (imitator == null || Player != imitator || !imitator.IsAlive())
        {
            Message($"效颦者授权失败: 效颦者状态异常 (存在={imitator != null}, 是当前效颦者={imitator == Player}, 存活={imitator?.IsAlive()})", "Imitator");
            return false;
        }

        var playerData = PlayerData.GetPlayerData(imitator);
        if (playerData == null || playerData.MainRole != RoleId.Imitator)
        {
            Message($"效颦者授权失败: 主职业不是效颦者 ({playerData?.MainRole})", "Imitator");
            return false;
        }
        if ((int)MeetingHud.Instance.state != 4)
        {
            Message($"效颦者授权失败: 会议状态为 {(int)MeetingHud.Instance.state}（需要 4=Results）", "Imitator");
            return false;
        }

        meetingSelectionRequests.Add(imitatorId);

        var target = PlayerById(targetId);
        if (targetId != byte.MaxValue && target != null && GetEligibleRole(target) == roleSnapshot)
        {
            canonicalTargetId = targetId;
            canonicalRole = roleSnapshot;
            SetSelection(imitatorId, targetId, roleSnapshot);
            meetingAuthorizations[imitatorId] = new MeetingAuthorization
            {
                TargetId = targetId,
                Role = roleSnapshot,
                MeetingGeneration = generation
            };
            Message($"效颦者授权通过: 目标={target.Data?.PlayerName ?? targetId.ToString()} 职业={roleSnapshot}", "Imitator");
        }
        else
        {
            Message($"效颦者授权: 目标无效，已清除选择 (targetId={targetId}, 记录职业={roleSnapshot}, 当前可借用职业={GetEligibleRole(target)})", "Imitator");
            ClearSelection(imitatorId, false);
        }
        return true;
    }

    public static bool ApplyHostSelection(byte imitatorId, byte targetId, RoleId roleSnapshot, uint generation)
    {
        if (generation == 0 || generation != meetingGeneration) return false;

        if (targetId == byte.MaxValue || roleSnapshot == RoleId.DefaultRole)
        {
            ClearSelection(imitatorId, false);
            meetingAuthorizations.Remove(imitatorId);
            return true;
        }

        SetSelection(imitatorId, targetId, roleSnapshot);
        var selection = GetSelection(imitatorId);
        if (selection.TargetId != targetId || selection.Role != roleSnapshot) return false;

        meetingAuthorizations[imitatorId] = new MeetingAuthorization
        {
            TargetId = targetId,
            Role = roleSnapshot,
            MeetingGeneration = generation
        };
        return true;
    }

    public static IReadOnlyList<AuthorizedStart> GetAuthorizedStartsForHost()
    {
        if (!AmongUsClient.Instance.AmHost || meetingGeneration == 0 || hostStartedMeetingGeneration == meetingGeneration)
            return Array.Empty<AuthorizedStart>();

        hostStartedMeetingGeneration = meetingGeneration;
        meetingSelectionOpen = false;

        return meetingAuthorizations
            .Where(entry => entry.Value.MeetingGeneration == meetingGeneration)
            .Select(entry => new AuthorizedStart(entry.Key, entry.Value.Role, entry.Value.MeetingGeneration))
            .ToArray();
    }

    public static bool StartAuthorized(byte imitatorId, RoleId copiedRole, uint generation)
    {
        if (generation == 0 || generation != meetingGeneration) return false;
        if (!meetingAuthorizations.TryGetValue(imitatorId, out var authorization) || authorization.MeetingGeneration != generation) return false;
        if (authorization.Role != copiedRole) return false;

        meetingAuthorizations.Remove(imitatorId);
        meetingSelectionOpen = false;
        return Start(imitatorId, copiedRole);
    }

    public static bool Start(byte imitatorId, RoleId copiedRole)
    {
        var imitator = PlayerById(imitatorId);
        if (imitator == null || Player == null || Player.PlayerId != imitatorId)
        {
            Message($"效颦者启动失败: 效颦者未就绪 (存在={imitator != null}, 字段持有者={Player?.PlayerId})", "Imitator");
            return false;
        }
        if (imitator.Data == null || imitator.Data.IsDead || imitator.Data.Disconnected)
        {
            Message($"效颦者启动失败: 效颦者状态异常 (死亡={imitator.Data?.IsDead}, 断线={imitator.Data?.Disconnected})", "Imitator");
            return false;
        }

        var playerData = PlayerData.GetPlayerData(imitator);
        if (playerData == null || playerData.MainRole != RoleId.Imitator || !IsCompatible(copiedRole))
        {
            Message($"效颦者启动失败: 身份校验未过 (主职业={playerData?.MainRole}, 目标职业={copiedRole}, 兼容={IsCompatible(copiedRole)})", "Imitator");
            ClearSelection(imitatorId);
            return false;
        }

        if (sessions.TryGetValue(imitatorId, out var existing))
        {
            if (existing.Role == copiedRole) return true;
            EndInternal(imitatorId, false);
        }

        try
        {
            var selection = GetSelection(imitatorId);
            var source = PlayerById(selection.TargetId);
            if (source?.Data == null || !source.Data.IsDead || source.Data.Disconnected
                || selection.Role != copiedRole || GetEligibleRole(source) != copiedRole)
            {
                Message($"效颦者启动失败: 借用来源失效 (来源={source?.Data?.PlayerName ?? selection.TargetId.ToString()}, 死亡={source?.Data?.IsDead}, 选择职业={selection.Role}, 请求职业={copiedRole}, 可借用={GetEligibleRole(source)})", "Imitator");
                ClearSelection(imitatorId);
                return false;
            }

            sessions[imitatorId] = new ImitationSession(imitator, source.PlayerId, copiedRole);
            if (imitator.AmOwner) ResetBorrowedButtons(copiedRole);
            ClearSelection(imitatorId);
            Message($"效颦者借用启动成功: {copiedRole} (来源 {source.Data?.PlayerName ?? source.PlayerId.ToString()})", "Imitator");
            return true;
        }
        catch (Exception e)
        {
            Error($"Failed to start {copiedRole} imitation: {e}");
            EndInternal(imitatorId, false);
            ClearSelection(imitatorId);
            return false;
        }
    }

    public static void End(byte imitatorId) => EndInternal(imitatorId, true);

    public static void EndAuthorized(byte imitatorId, uint generation)
    {
        if (generation != 0 && sessions.TryGetValue(imitatorId, out var session) && session.Generation == generation)
            EndInternal(imitatorId, true);
    }

    public static void EndAll(bool preserveMeetingCarry)
    {
        foreach (var imitatorId in sessions.Keys.ToArray()) EndInternal(imitatorId, preserveMeetingCarry);
    }

    public static void Abort(byte imitatorId) => EndInternal(imitatorId, false);

    public static void NotifyPlayerRoleInvalidated(byte playerId)
    {
        foreach (var session in sessions.Values) session.InvalidateOriginalHolder(playerId);
    }

    public static void NotifyPlayerStateChanged(byte playerId)
    {
        pendingRedemptions.Remove(playerId);
        foreach (var (imitatorId, session) in sessions.ToArray())
        {
            if (!session.OriginalHolderIs(playerId)) continue;
            session.InvalidateOriginalHolder(playerId);
            EndInternal(imitatorId, false);
        }
        ClearSelectionsForTarget(playerId);
        CancelForPlayer(playerId);
    }

    public static void NotifyPlayerRevived(byte playerId)
    {
        pendingRedemptions.Remove(playerId);
        consumedSourceIds.Remove(playerId);
        ClearSelectionsForTarget(playerId);
        CancelForPlayer(playerId);
    }

    public static void NotifyRoleConsumed(RoleId role, byte sourcePlayerId)
    {
        consumedSourceIds.Add(sourcePlayerId);
        ClearSelectionsForTarget(sourcePlayerId);
        foreach (var (imitatorId, session) in sessions.ToArray())
            if (session.Role == role || session.SourceIs(sourcePlayerId))
                EndInternal(imitatorId, false);
    }

    public static void NotifyRoleAssigned(RoleId role, byte newHolderId)
    {
        if (!IsCompatible(role)) return;
        foreach (var (imitatorId, session) in sessions.ToArray())
            if (session.Role == role && imitatorId != newHolderId)
                EndInternal(imitatorId, false);
    }

    public static bool TryGetBorrowedSourceRole(PlayerControl player, out RoleId role)
    {
        role = RoleId.DefaultRole;
        if (player == null) return false;

        foreach (var session in sessions.Values)
        {
            if (!session.SourceIs(player.PlayerId)) continue;
            role = session.Role;
            return true;
        }
        return false;
    }

    private static void CancelForPlayer(byte playerId)
    {
        foreach (var (imitatorId, session) in sessions.ToArray())
            if (imitatorId == playerId || session.SourceIs(playerId))
                EndInternal(imitatorId, false);
    }

    private static void ClearSelectionsForTarget(byte targetId)
    {
        foreach (var (imitatorId, selection) in selections.ToArray())
        {
            if (selection.TargetId != targetId) continue;
            ClearSelection(imitatorId, false);
            meetingAuthorizations.Remove(imitatorId);
        }
        ImitatorPatches.NotifyTargetInvalidated(targetId);
    }

    public static bool IsImitating(PlayerControl player, RoleId role = RoleId.DefaultRole)
    {
        if (player == null || !sessions.TryGetValue(player.PlayerId, out var session)) return false;
        return role == RoleId.DefaultRole || session.Role == role;
    }

    public static bool HasActiveRole(RoleId role) => sessions.Values.Any(session => session.Role == role);

    public static bool TryConsumeDreamKill(PlayerControl owner)
    {
        if (owner == null || !sessions.TryGetValue(owner.PlayerId, out var session) || session.Role != RoleId.Dreamcatcher)
            return false;
        if (session.dreamKillConsumed) return false;
        session.dreamKillConsumed = true;
        return true;
    }

    public static bool IsDreamKillAvailable(PlayerControl owner)
        => owner != null && sessions.TryGetValue(owner.PlayerId, out var session)
            && session.Role == RoleId.Dreamcatcher && !session.dreamKillConsumed;

    public static bool IsSessionCurrent(PlayerControl owner, RoleId role)
        => IsImitating(owner, role);

    public static bool TryGetSessionGeneration(PlayerControl owner, RoleId role, out uint generation)
    {
        generation = 0u;
        if (owner == null || !sessions.TryGetValue(owner.PlayerId, out var session) || session.Role != role) return false;
        generation = session.Generation;
        return true;
    }

    public static bool IsSessionCurrent(PlayerControl owner, RoleId role, uint generation)
        => generation != 0 && owner != null && sessions.TryGetValue(owner.PlayerId, out var session)
            && session.Role == role && session.Generation == generation;

    public static void EnableBorrowedPortals(PlayerControl owner)
    {
        if (!IsImitating(owner, RoleId.Portalmaker)) return;
        if (Portal.firstPortal == null || Portal.secondPortal == null) return;
        Portal.bothPlacedAndEnabled = true;
        Portal.firstPortal.portalGameObject?.SetActive(true);
        Portal.secondPortal.portalGameObject?.SetActive(true);
    }

    public static bool PrepareBorrowedRedemption(PlayerControl caster, byte targetId, uint generation)
    {
        var target = PlayerById(targetId);
        if (!IsSessionCurrent(caster, RoleId.Redemptor, generation)) return false;
        if (target == null || target == caster || target.Data == null || !target.Data.IsDead || target.Data.Disconnected) return false;

        pendingRedemptions[caster.PlayerId] = new PendingRedemption
        {
            TargetId = targetId,
            SessionGeneration = generation,
            SelfSacrificeConfirmed = false,
            ExpiresAt = Time.time + Mathf.Max(10f, Redemptor.reviveDuration + 10f)
        };
        return true;
    }

    public static void AuthorizeBorrowedRedemptionDeath(PlayerControl caster)
    {
        if (caster == null || !pendingRedemptions.TryGetValue(caster.PlayerId, out var pending)) return;
        if (!IsSessionCurrent(caster, RoleId.Redemptor, pending.SessionGeneration)) return;

        var target = PlayerById(pending.TargetId);
        if (target?.Data == null || !target.Data.IsDead || target.Data.Disconnected)
        {
            pendingRedemptions.Remove(caster.PlayerId);
            return;
        }
        pending.SelfSacrificeConfirmed = true;
        pending.ExpiresAt = Time.time + Mathf.Max(5f, Redemptor.reviveDuration + 5f);
    }

    public static bool TryCompleteBorrowedRedemption(PlayerControl caster, byte targetId, uint generation)
    {
        if (caster == null || generation == 0) return false;
        if (!pendingRedemptions.TryGetValue(caster.PlayerId, out var pending)) return false;
        if (pending.TargetId != targetId || pending.SessionGeneration != generation) return false;
        if (!pending.SelfSacrificeConfirmed || Time.time > pending.ExpiresAt) return false;
        if (caster.Data == null || !caster.Data.IsDead) return false;

        var playerData = PlayerData.GetPlayerData(caster);
        if (playerData == null || playerData.MainRole != RoleId.Imitator) return false;

        pendingRedemptions.Remove(caster.PlayerId);
        var target = PlayerById(targetId);
        if (target == null || target == caster || target.Data == null || !target.Data.IsDead || target.Data.Disconnected)
            return false;

        Redemptor.RevivePlayer(targetId);
        return true;
    }

    public static bool TryCompleteBorrowedPrayer(PlayerControl caster, byte targetId, uint generation)
    {
        var target = PlayerById(targetId);
        if (!Redemptor.prayer || !IsSessionCurrent(caster, RoleId.Redemptor, generation)) return false;
        if (target == null || target == caster || target.Data == null || !target.Data.IsDead || target.Data.Disconnected) return false;

        Redemptor.RevivePlayer(targetId);
        Redemptor.RevivedPlayer = target;
        Redemptor.target = null;
        return true;
    }

    public static void RecordLocalDeath(PlayerControl deadPlayer, PlayerControl killer, CustomDeathReason deathReason)
    {
        if (deadPlayer != null) consumedSourceIds.Remove(deadPlayer.PlayerId);
        foreach (var session in sessions.Values.ToArray()) session.RecordLocalDeath(deadPlayer, killer, deathReason);
    }

    public static RoleId GetEffectiveRole(PlayerControl player)
    {
        if (player == null || !sessions.TryGetValue(player.PlayerId, out var session))
            return PlayerData.GetPlayerData(player)?.MainRole ?? RoleId.DefaultRole;
        return session.Role;
    }

    public static RoleId GetEligibleRole(PlayerControl target)
    {
        if (target?.Data == null || !target.Data.IsDead || target.Data.Disconnected) return RoleId.DefaultRole;
        if (consumedSourceIds.Contains(target.PlayerId)) return RoleId.DefaultRole;

        var roleId = PlayerData.GetPlayerData(target)?.MainRole ?? RoleId.DefaultRole;
        return IsCompatible(roleId) ? roleId : RoleId.DefaultRole;
    }

    public static bool IsCompatible(RoleId role) => CompatibleRoles.Contains(role);

    public static void ShowMeetingCarry(MeetingHud meeting)
    {
        if (meeting == null) return;

        foreach (var carry in meetingCarries.Values.ToArray())
        {
            var imitator = PlayerById(carry.ImitatorId);
            if (imitator == null) continue;

            if (carry.TrapperReport != null && (imitator.AmOwner || CanSeeGhostInfo))
                FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(imitator, carry.TrapperReport.ToLocalizedText(), true);

            if (!string.IsNullOrWhiteSpace(carry.OracleReport) && imitator.AmOwner)
                FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(imitator, carry.OracleReport, true);

            if (carry.JailedId != byte.MaxValue)
                ShowCarriedJail(meeting, imitator, PlayerById(carry.JailedId), carry.JailUses);
        }
    }

    public static bool TryGetCarriedJail(PlayerControl participant, out PlayerControl owner, out PlayerControl jailed)
    {
        owner = null;
        jailed = null;
        if (participant == null || MeetingHud.Instance == null) return false;

        foreach (var carry in meetingCarries.Values)
        {
            if (carry.CopiedRole != RoleId.Jailor || carry.JailedId == byte.MaxValue) continue;

            var carryOwner = PlayerById(carry.ImitatorId);
            var carryJailed = PlayerById(carry.JailedId);
            if (participant.PlayerId != carry.ImitatorId && participant.PlayerId != carry.JailedId) continue;
            if (!CanUseCarriedJail(carryOwner, carryJailed)) continue;

            owner = carryOwner;
            jailed = carryJailed;
            return true;
        }
        return false;
    }

    public static bool CanExecuteCarriedJail(PlayerControl owner, PlayerControl jailed)
    {
        if (!CanUseCarriedJail(owner, jailed)) return false;
        return meetingCarries.TryGetValue(owner.PlayerId, out var carry)
            && carry.CopiedRole == RoleId.Jailor && carry.JailedId == jailed.PlayerId
            && carry.JailUses > 0;
    }

    public static void ClearCarriedJail(byte ownerId, byte jailedId)
    {
        if (!meetingCarries.TryGetValue(ownerId, out var carry) || carry.CopiedRole != RoleId.Jailor || carry.JailedId != jailedId)
            return;

        meetingCarries.Remove(ownerId);
        var meeting = MeetingHud.Instance;
        foreach (var pva in meeting?.playerStates ?? Enumerable.Empty<PlayerVoteArea>())
        {
            var cell = pva.transform.FindChild("ImitatorJailCell");
            if (cell != null) UObject.Destroy(cell.gameObject);
            var icon = pva.transform.FindChild("ImitatorJailTargetIcon");
            if (icon != null) UObject.Destroy(icon.gameObject);
        }
    }

    public static void ClearMeetingCarries() => meetingCarries.Clear();

    public static void clearAndReload()
    {
        foreach (var imitatorId in sessions.Keys.ToArray()) EndInternal(imitatorId, false);

        selections.Clear();
        sessions.Clear();
        meetingCarries.Clear();
        consumedSourceIds.Clear();
        meetingAuthorizations.Clear();
        meetingSelectionRequests.Clear();
        pendingRedemptions.Clear();
        meetingSelectionOpen = false;
        meetingGeneration = 0u;
        hostStartedMeetingGeneration = 0u;
        nextSessionGeneration = 0u;
        Player = null;
    }

    public static void ClearAndReload()
    {
        clearAndReload();
    }

    private static void EndInternal(byte imitatorId, bool preserveMeetingCarry)
    {
        if (!sessions.Remove(imitatorId, out var session))
        {
            ClearSelection(imitatorId);
            return;
        }

        try
        {
            var carry = session.Finish(preserveMeetingCarry);
            if (carry != null) meetingCarries[imitatorId] = carry;
        }
        catch (Exception e)
        {
            Error($"Failed to finish {session.Role} imitation: {e}");
        }
        finally
        {
            if (session.Role == RoleId.Deputy && !session.LocalPlayerWasHandcuffed)
                CustomButton.setAllButtonsHandcuffedStatus(false);
            ClearSelection(imitatorId);
        }
    }

    private static void ClearSelection(byte imitatorId, bool clearAuthorization = true)
    {
        var selection = GetSelection(imitatorId);
        selection.TargetId = byte.MaxValue;
        selection.Role = RoleId.DefaultRole;
        if (clearAuthorization) meetingAuthorizations.Remove(imitatorId);
    }

    private static void CloseBorrowedDevices(RoleId role)
    {
        try
        {
            if (MapBehaviour.Instance != null && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
            if (Minigame.Instance != null) Minigame.Instance.ForceClose();

            switch (role)
            {
                case RoleId.Hacker:
                {
                    var vitals = Hacker.vitals;
                    var doorLog = Hacker.doorLog;
                    if (vitals != null) vitals.ForceClose();
                    if (doorLog != null && doorLog != vitals) doorLog.ForceClose();
                    Hacker.vitals = null;
                    Hacker.doorLog = null;
                    break;
                }
                case RoleId.SecurityGuard:
                    if (SecurityGuard.minigame != null) SecurityGuard.minigame.ForceClose();
                    SecurityGuard.minigame = null;
                    break;
            }
        }
        catch (Exception e)
        {
            Warn("Could not close borrowed device UI: " + e.Message);
        }

        if (PlayerControl.LocalPlayer != null) PlayerControl.LocalPlayer.moveable = true;

        var buttons = role != RoleId.Hacker
            ? new[] { HudManagerStartPatch.securityGuardButton, HudManagerStartPatch.securityGuardCamButton }
            : new[] { HudManagerStartPatch.hackerButton, HudManagerStartPatch.hackerVitalsButton, HudManagerStartPatch.hackerAdminTableButton };

        foreach (var button in buttons.Where(button => button != null))
        {
            button.IsEffectActive = false;
            button.EffectTimer = 0f;
            button.Timer = button.MaxTimer;
            if (button.actionButton?.cooldownTimerText != null)
                button.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }
    }

    private static void ResetBorrowedButtons(RoleId role)
    {
        var buttons = role switch
        {
            RoleId.Portalmaker => new[] { HudManagerStartPatch.portalmakerPlacePortalButton, HudManagerStartPatch.usePortalButton, HudManagerStartPatch.portalmakerMoveToPortalButton },
            RoleId.Engineer => new[] { HudManagerStartPatch.engineerRepairButton },
            RoleId.Sheriff => new[] { HudManagerStartPatch.sheriffKillButton },
            RoleId.Deputy => new[] { HudManagerStartPatch.deputyHandcuffButton },
            RoleId.BodyGuard => new[] { HudManagerStartPatch.bodyGuardGuardButton },
            RoleId.Jumper => new[] { HudManagerStartPatch.jumperMarkButton, HudManagerStartPatch.jumperJumpButton },
            RoleId.Veteran => new[] { HudManagerStartPatch.veteranAlertButton },
            RoleId.Medic => new[] { HudManagerStartPatch.medicShieldButton },
            RoleId.Hacker => new[] { HudManagerStartPatch.hackerButton, HudManagerStartPatch.hackerVitalsButton, HudManagerStartPatch.hackerAdminTableButton },
            RoleId.Tracker => new[] { HudManagerStartPatch.trackerTrackPlayerButton, HudManagerStartPatch.trackerTrackCorpsesButton },
            RoleId.Prophet => new[] { HudManagerStartPatch.prophetButton },
            RoleId.SecurityGuard => new[] { HudManagerStartPatch.securityGuardButton, HudManagerStartPatch.securityGuardCamButton },
            RoleId.Alchemyst => new[] { HudManagerStartPatch.alchemystButton, HudManagerStartPatch.alchemystKillButton },
            RoleId.Trapper => new[] { HudManagerStartPatch.trapperButton },
            RoleId.Redemptor => new[] { HudManagerStartPatch.redemptorRevelationButton, HudManagerStartPatch.redemptorPrayerButton, HudManagerStartPatch.redemptorReviveButton },
            RoleId.Jailor => new[] { HudManagerStartPatch.jailorButton },
            RoleId.Dreamcatcher => new[] { HudManagerStartPatch.dreamcatcherButton },
            RoleId.Oracle => new[] { HudManagerStartPatch.oracleButton },
            RoleId.TrapperPlus => new[] { HudManagerStartPatch.trapperPlusButton },
            RoleId.Aurial => new[] { HudManagerStartPatch.aurialRadiateButton },
            _ => Array.Empty<CustomButton>()
        };

        foreach (var button in buttons.Where(button => button != null))
        {
            button.Timer = button.MaxTimer < 1f ? 0f : button.MaxTimer;
            button.EffectTimer = 0f;
            button.IsEffectActive = false;
            button.Multiplier = 1f;
            if (button.actionButton?.cooldownTimerText != null)
                button.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }
    }

    private static bool IsStillMainRole(PlayerControl player, RoleId role)
    {
        if (player == null) return false;
        if (player.Data != null && player.Data.Disconnected) return false;
        var playerData = PlayerData.GetPlayerData(player);
        return playerData != null && playerData.MainRole == role;
    }

    private static PlayerControl GetOriginalHolder(RoleId role, byte preferredPlayerId = byte.MaxValue)
        => role switch
        {
            RoleId.Portalmaker => Portalmaker.portalmaker,
            RoleId.Engineer => Engineer.engineer,
            RoleId.Sheriff => Sheriff.Player?.FirstOrDefault(x => x?.PlayerId == preferredPlayerId) ?? Sheriff.Player?.FirstOrDefault(),
            RoleId.Deputy => Sheriff.Deputy,
            RoleId.BodyGuard => BodyGuard.bodyguard,
            RoleId.Jumper => Jumper.jumper,
            RoleId.Detective => Detective.detective,
            RoleId.Veteran => Veteran.veteran,
            RoleId.Medic => Medic.medic,
            RoleId.Seer => Seer.seer,
            RoleId.Hacker => Hacker.hacker,
            RoleId.Tracker => Tracker.tracker,
            RoleId.Snitch => Snitch.snitch,
            RoleId.Prophet => Prophet.prophet,
            RoleId.Spy => Spy.spy,
            RoleId.SecurityGuard => SecurityGuard.securityGuard,
            RoleId.Alchemyst => Alchemyst.Player,
            RoleId.Trapper => Trapper.trapper,
            RoleId.Redemptor => Redemptor.Player,
            RoleId.Jailor => Jailor.Player,
            RoleId.Dreamcatcher => Dreamcatcher.Player,
            RoleId.Oracle => Oracle.Player,
            RoleId.TrapperPlus => TrapperPlus.Player,
            _ => null
        };

    private static void ClearRestoredHolder(RoleId role, byte invalidPlayerId)
    {
        switch (role)
        {
            case RoleId.Portalmaker:
                Portalmaker.portalmaker = null;
                break;
            case RoleId.Engineer:
                Engineer.engineer = null;
                break;
            case RoleId.Sheriff:
                Sheriff.Player?.RemoveAll(x => x?.PlayerId == invalidPlayerId);
                break;
            case RoleId.Deputy:
                Sheriff.Deputy = null;
                break;
            case RoleId.BodyGuard:
                BodyGuard.bodyguard = null;
                break;
            case RoleId.Jumper:
                Jumper.jumper = null;
                break;
            case RoleId.Detective:
                Detective.detective = null;
                break;
            case RoleId.Veteran:
                Veteran.veteran = null;
                break;
            case RoleId.Medic:
                Medic.medic = null;
                break;
            case RoleId.Seer:
                Seer.seer = null;
                break;
            case RoleId.Hacker:
                Hacker.hacker = null;
                break;
            case RoleId.Tracker:
                Tracker.tracker = null;
                break;
            case RoleId.Snitch:
                Snitch.snitch = null;
                break;
            case RoleId.Prophet:
                Prophet.prophet = null;
                break;
            case RoleId.Spy:
                Spy.spy = null;
                break;
            case RoleId.SecurityGuard:
                SecurityGuard.securityGuard = null;
                break;
            case RoleId.Alchemyst:
                Alchemyst.Player = null;
                break;
            case RoleId.Trapper:
                Trapper.trapper = null;
                break;
            case RoleId.Redemptor:
                Redemptor.Player = null;
                break;
            case RoleId.Jailor:
                Jailor.Player = null;
                break;
            case RoleId.Dreamcatcher:
                Dreamcatcher.Player = null;
                break;
            case RoleId.Oracle:
                Oracle.Player = null;
                break;
            case RoleId.TrapperPlus:
                TrapperPlus.Player = null;
                break;
        }
    }

    private static Type GetStateType(RoleId role)
        => role switch
        {
            RoleId.Portalmaker => typeof(Portalmaker),
            RoleId.Engineer => typeof(Engineer),
            RoleId.Sheriff or RoleId.Deputy => typeof(Sheriff),
            RoleId.BodyGuard => typeof(BodyGuard),
            RoleId.Jumper => typeof(Jumper),
            RoleId.Detective => typeof(Detective),
            RoleId.Veteran => typeof(Veteran),
            RoleId.Medic => typeof(Medic),
            RoleId.Seer => typeof(Seer),
            RoleId.Hacker => typeof(Hacker),
            RoleId.Tracker => typeof(Tracker),
            RoleId.Snitch => typeof(Snitch),
            RoleId.Prophet => typeof(Prophet),
            RoleId.Spy => typeof(Spy),
            RoleId.SecurityGuard => typeof(SecurityGuard),
            RoleId.Alchemyst => typeof(Alchemyst),
            RoleId.Trapper => typeof(Trapper),
            RoleId.Redemptor => typeof(Redemptor),
            RoleId.Jailor => typeof(Jailor),
            RoleId.Dreamcatcher => typeof(Dreamcatcher),
            RoleId.Oracle => typeof(Oracle),
            RoleId.TrapperPlus => typeof(TrapperPlus),
            _ => null
        };

    private static void InitializeFreshRole(PlayerControl player, RoleId role)
    {
        switch (role)
        {
            case RoleId.Portalmaker:
                Portalmaker.portalmaker = player;
                Portal.firstPortal = null;
                Portal.secondPortal = null;
                Portal.bothPlacedAndEnabled = false;
                Portal.isTeleporting = false;
                Portal.teleportedPlayers = new List<Portal.tpLogEntry>();
                break;
            case RoleId.Engineer:
                Engineer.engineer = player;
                Engineer.UsedFix = false;
                Engineer.remainingFixes = CustomOptionHolder.engineerNumberOfFixes.GetInt();
                break;
            case RoleId.Sheriff:
                InitializeSheriff(player, false);
                break;
            case RoleId.Deputy:
                InitializeSheriff(player, true);
                break;
            case RoleId.BodyGuard:
                BodyGuard.bodyguard = player;
                BodyGuard.guarded = null;
                BodyGuard.currentTarget = null;
                BodyGuard.usedGuard = false;
                break;
            case RoleId.Jumper:
                Jumper.jumper = player;
                Jumper.jumpLocation = Vector3.zero;
                Jumper.usedPlace = false;
                Jumper.Charges = Math.Max(0, Mathf.FloorToInt(Jumper.MaxCharges));
                break;
            case RoleId.Detective:
                Detective.detective = player;
                Detective.timer = 0f;
                break;
            case RoleId.Veteran:
                Veteran.veteran = player;
                Veteran.alertActive = false;
                break;
            case RoleId.Medic:
                Medic.medic = player;
                Medic.shielded = null;
                Medic.futureShielded = null;
                Medic.currentTarget = null;
                Medic.usedShield = false;
                Medic.meetingAfterShielding = false;
                break;
            case RoleId.Seer:
                Seer.seer = player;
                Seer.deadBodyPositions = new List<Vector3>();
                break;
            case RoleId.Hacker:
                Hacker.hacker = player;
                Hacker.vitals = null;
                Hacker.doorLog = null;
                Hacker.hackerTimer = 0f;
                Hacker.chargesVitals = Math.Max(0, Mathf.FloorToInt(Hacker.toolsNumber));
                Hacker.chargesAdminTable = Math.Max(0, Mathf.FloorToInt(Hacker.toolsNumber));
                Hacker.rechargedTasks = Hacker.rechargeTasksNumber;
                break;
            case RoleId.Tracker:
                Tracker.tracker = player;
                Tracker.localArrows = new List<Arrow>();
                Tracker.deadBodyPositions = new List<Vector3>();
                Tracker.currentTarget = null;
                Tracker.tracked = null;
                Tracker.usedTracker = false;
                Tracker.timeUntilUpdate = 0f;
                Tracker.corpsesTrackingTimer = 0f;
                Tracker.arrow = new Arrow(Color.blue);
                Tracker.arrow.arrow?.SetActive(false);
                Tracker.DangerMeterParent = null;
                Tracker.Meter = null;
                break;
            case RoleId.Snitch:
                Snitch.snitch = player;
                Snitch.localArrows = new List<Arrow>();
                Snitch.needsUpdate = true;
                Snitch.text = null;
                break;
            case RoleId.Prophet:
                Prophet.prophet = player;
                Prophet.currentTarget = null;
                Prophet.examined = new Dictionary<PlayerControl, bool>();
                Prophet.examinesLeft = Prophet.examineNum;
                Prophet.isRevealed = false;
                Prophet.arrows = new List<Arrow>();
                break;
            case RoleId.Spy:
                Spy.spy = player;
                break;
            case RoleId.SecurityGuard:
                SecurityGuard.securityGuard = player;
                SecurityGuard.remainingScrews = SecurityGuard.totalScrews;
                SecurityGuard.placedCameras = 0;
                SecurityGuard.charges = SecurityGuard.maxCharges;
                SecurityGuard.rechargedTasks = SecurityGuard.rechargeTasksNumber;
                SecurityGuard.ventTarget = null;
                SecurityGuard.minigame = null;
                break;
            case RoleId.Alchemyst:
                Alchemyst.Player = player;
                Alchemyst.target = null;
                Alchemyst.soulTarget = null;
                Alchemyst.deadBodies = new List<Tuple<Alchemyst.DeadPlayer, Vector3>>();
                Alchemyst.futureDeadBodies = new List<Tuple<Alchemyst.DeadPlayer, Vector3>>();
                Alchemyst.souls = new List<SpriteRenderer>();
                Alchemyst.meetingStartTime = DateTime.UtcNow;
                Alchemyst.skillUseCount = 0;
                Alchemyst.canUseKill = false;
                Alchemyst.CurrentTarget = null;
                break;
            case RoleId.Trapper:
                Trapper.trapper = player;
                Trapper.charges = Trapper.maxCharges;
                Trapper.rechargedTasks = Trapper.rechargeTasksNumber;
                break;
            case RoleId.Redemptor:
                Redemptor.Player = player;
                Redemptor.target = null;
                Redemptor.arrow = null;
                Redemptor.Revelating = false;
                Redemptor.Prayering = false;
                Redemptor.RevivedPlayer = null;
                Redemptor.text = null;
                break;
            case RoleId.Jailor:
                Jailor.Player = player;
                Jailor.currentTarget = null;
                Jailor.Jailed = null;
                Jailor.usesCount = CustomOptionHolder.jailorUseCount.GetInt();
                break;
            case RoleId.Dreamcatcher:
                Dreamcatcher.Player = player;
                Dreamcatcher.CurrentTarget = null;
                Dreamcatcher.Dreamed = null;
                Dreamcatcher.LastDreamed = null;
                Dreamcatcher.ShieldUsed = false;
                break;
            case RoleId.Oracle:
                Oracle.Player = player;
                Oracle.CurrentTarget = null;
                Oracle.Confesser = null;
                break;
            case RoleId.TrapperPlus:
                TrapperPlus.Player = player;
                TrapperPlus.ResetOwnerState(player);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported Imitator role");
        }
    }

    private static void InitializeSheriff(PlayerControl player, bool deputy)
    {
        Sheriff.Player = new List<PlayerControl>(Sheriff.Player ?? new List<PlayerControl>());
        Sheriff.handcuffedPlayers = deputy ? new List<byte>() : new List<byte>(Sheriff.handcuffedPlayers ?? new List<byte>());
        Sheriff.handcuffedKnows = deputy ? new Dictionary<byte, float>() : new Dictionary<byte, float>(Sheriff.handcuffedKnows ?? new Dictionary<byte, float>());
        Sheriff.Player.RemoveAll(existing => existing?.PlayerId == player.PlayerId);
        Sheriff.currentTarget = null;
        if (deputy) Sheriff.remainingHandcuffs = CustomOptionHolder.deputyNumberOfHandcuffs.GetFloat();
    }

    private static SpriteRenderer CreateLocalSoul(string name, Vector3 position, Sprite sprite)
    {
        if (sprite == null) return null;

        var obj = new GameObject(name) { layer = 5 };
        obj.transform.position = new Vector3(position.x, position.y, position.y / 1000f - 1f);
        obj.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        return renderer;
    }

    private static void DestroyArrows(IEnumerable<Arrow> arrows)
    {
        foreach (var arrow in arrows?.ToArray() ?? Array.Empty<Arrow>())
            if (arrow?.arrow != null) UObject.Destroy(arrow.arrow);
    }

    private static void ShowCarriedJail(MeetingHud meeting, PlayerControl owner, PlayerControl jailed, int uses)
    {
        if (!CanUseCarriedJail(owner, jailed)) return;

        if (PlayerControl.LocalPlayer == jailed)
        {
            FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(jailed, GetString("Jailor.JailedChat"), true);
            Coroutines.Start(Jailor.ShowJailed());
        }
        else if (PlayerControl.LocalPlayer == owner)
        {
            FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(owner, GetString("Jailor.JailorChat"), true);
        }

        var voteArea = meeting.playerStates?.FirstOrDefault(area => area.TargetPlayerId == jailed.PlayerId);
        var cancelButton = voteArea?.Buttons?.transform?.Find("CancelButton")?.gameObject;
        if (voteArea == null || cancelButton == null) return;

        var cell = UObject.Instantiate(cancelButton, voteArea.transform);
        cell.name = "ImitatorJailCell";
        cell.transform.localPosition = new Vector3(-0.95f, 0.01f, -2f);
        cell.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        cell.transform.parent = cancelButton.transform.parent.parent;
        cell.GetComponent<SpriteRenderer>().sprite = Jailor.jailedSprite;
        cell.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();

        if (!owner.AmOwner || uses <= 0 || !CanExecuteCarriedJail(owner, jailed)) return;

        var targetButton = UObject.Instantiate(cancelButton, voteArea.transform);
        targetButton.name = "ImitatorJailTargetIcon";
        targetButton.transform.localPosition = new Vector3(1f, 0.03f, -1f);
        targetButton.transform.parent = cancelButton.transform.parent.parent;
        var targetRenderer = targetButton.GetComponent<SpriteRenderer>();
        targetRenderer.sprite = Jailor.TargetSprite;
        targetRenderer.color = Color.red;

        var passiveButton = targetButton.GetComponent<PassiveButton>();
        passiveButton.OnClick.RemoveAllListeners();
        passiveButton.OnClick.AddListener((Action)(() =>
        {
            if ((int)meeting.state is 1 or 4) return;
            if (!CanExecuteCarriedJail(owner, jailed)) return;

            var writer = StartRPC(CustomRPC.ExiledJailed);
            writer.Write(owner.PlayerId);
            writer.Write(jailed.PlayerId);
            writer.EndRPC();
            Jailor.ExiledJailed(owner, jailed);

            targetButton?.Destroy();
            cell?.Destroy();
        }));
    }

    private static bool CanUseCarriedJail(PlayerControl owner, PlayerControl jailed)
    {
        if (owner == null || jailed == null) return false;
        if (!owner.IsAlive() || !jailed.IsAlive() || !owner.CanUseMeetingAbility()) return false;
        if (Blackmailer.blackmailed == owner) return false;
        return !Gaoler.IsPrisoner(owner);
    }
}
