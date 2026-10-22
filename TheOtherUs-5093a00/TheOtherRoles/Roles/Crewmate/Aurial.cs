using TheOtherRoles.Attributes;
using TheOtherRoles.Patches;

namespace TheOtherRoles.Roles.Crewmate;

public static class Aurial
{
    public enum Faction
    {
        Crewmate,
        Neutral,
        Impostor
    }

    /// <summary>Imitator 借用灵光师能力时的会话快照。</summary>
    public sealed class SessionSnapshot
    {
        internal PlayerControl Holder;
        internal readonly Dictionary<byte, int> HitCounts = new();
        internal readonly HashSet<byte> RevealedPlayers = new();
    }

    private sealed class AnonymousVisualState
    {
        internal Vector3 LocalScale;
        internal float ColliderRadius;
        internal Vector2 ColliderOffset;
        internal bool HasCollider;
    }

    public static PlayerControl aurial;
    public static Color color = new Color32(179, 77, 153, byte.MaxValue);

    public static float cooldown = 20f;
    public static float radius = 2.5f;
    public static int hitsToReveal = 3;
    public static float vision = 0.35f;
    public static bool affectedByVortox;

    public static readonly Dictionary<byte, int> hitCounts = new();
    public static readonly HashSet<byte> revealedPlayers = new();
    private static readonly Dictionary<byte, AnonymousVisualState> anonymousPlayers = new();
    private static readonly HashSet<byte> anonymousApplied = new();
    private static readonly HashSet<byte> visibleTargetsBuffer = new(); // 每帧复用，避免重复分配
    private static int maintainTick;

    public static Sprite buttonSprite = new ResourceSprite("AurialRadiateButton.png");

    public static PlayerControl Player
    {
        get => aurial;
        set => aurial = value;
    }

    public static bool HasAnonymousWorldView
    {
        get
        {
            var local = PlayerControl.LocalPlayer;
            return InGame && !Helpers.InMeeting && ExileController.Instance == null
                && local != null && aurial != null && local.PlayerId == aurial.PlayerId && aurial.IsAlive();
        }
    }

    public static List<byte> GetRadiationTargets(byte casterId) => GetRadiationTargets(PlayerById(casterId));

    public static List<byte> GetRadiationTargets(PlayerControl caster = null)
    {
        var result = new List<byte>();
        caster ??= aurial;
        if (caster == null || aurial == null || caster.PlayerId != aurial.PlayerId || caster.Data == null || !caster.IsAlive())
            return result;

        var origin = caster.GetTruePosition();
        var sqrRadius = Mathf.Max(0f, radius);
        sqrRadius *= sqrRadius;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.PlayerId == caster.PlayerId || !p.IsAlive()) continue;
            if ((p.GetTruePosition() - origin).sqrMagnitude <= sqrRadius) result.Add(p.PlayerId);
        }
        result.Sort();
        return result;
    }

    public static List<byte> ApplyRadiation(byte casterId, IEnumerable<byte> targetIds)
    {
        var newlyRevealed = new List<byte>();
        if (aurial == null || aurial.PlayerId != casterId || targetIds == null) return newlyRevealed;

        var seen = new HashSet<byte>();
        foreach (var targetId in targetIds)
        {
            if (targetId == casterId || !seen.Add(targetId) || PlayerById(targetId) == null) continue;

            hitCounts.TryGetValue(targetId, out var hits);
            var newCount = Math.Min(hitsToReveal, hits + 1);
            hitCounts[targetId] = newCount;
            if (newCount >= hitsToReveal && revealedPlayers.Add(targetId)) newlyRevealed.Add(targetId);
        }
        return newlyRevealed;
    }

    public static List<byte> ReceiveRadiation(byte casterId, IEnumerable<byte> targetIds) => ApplyRadiation(casterId, targetIds);

    public static int GetHitCount(byte playerId) => hitCounts.TryGetValue(playerId, out var count) ? count : 0;

    public static bool HasRevealed(byte playerId) => revealedPlayers.Contains(playerId);

    public static SessionSnapshot BeginImitation(PlayerControl imitator)
    {
        RestoreAnonymousWorldView();
        var snapshot = new SessionSnapshot { Holder = aurial };
        foreach (var pair in hitCounts) snapshot.HitCounts[pair.Key] = pair.Value;
        foreach (var id in revealedPlayers) snapshot.RevealedPlayers.Add(id);

        aurial = imitator;
        hitCounts.Clear();
        revealedPlayers.Clear();
        Message($"灵光师借用生效: aurial 现指向 {imitator?.Data?.PlayerName ?? imitator?.PlayerId.ToString() ?? "null"}", "Aurial");
        return snapshot;
    }

    public static void EndImitation(SessionSnapshot snapshot, bool restoreHolder = true)
    {
        RestoreAnonymousWorldView();
        aurial = restoreHolder ? snapshot?.Holder : null;
        hitCounts.Clear();
        revealedPlayers.Clear();
        if (snapshot == null || !restoreHolder)
        {
            Message($"灵光师借用归还: aurial → null", "Aurial");
            return;
        }

        foreach (var pair in snapshot.HitCounts) hitCounts[pair.Key] = pair.Value;
        foreach (var id in snapshot.RevealedPlayers) revealedPlayers.Add(id);
        Message($"灵光师借用归还: aurial → {aurial?.Data?.PlayerName ?? aurial?.PlayerId.ToString() ?? "null"}", "Aurial");
    }

    public static Faction GetCurrentFaction(PlayerControl target)
    {
        if (target == null) return Faction.Crewmate;

        if (target == SchrodingersCat.Player)
            return SchrodingersCat.State switch
            {
                SchrodingersCat.CatState.Crewmate => Faction.Crewmate,
                SchrodingersCat.CatState.Impostor => Faction.Impostor,
                _ => Faction.Neutral
            };

        var roleId = PlayerData.GetPlayerData(target)?.MainRole ?? RoleId.DefaultRole;
        if (roleId != RoleId.DefaultRole && RoleInfo.RoleInfoById.TryGetValue(roleId, out var info))
        {
            if (info.roleType == RoleType.Impostor) return Faction.Impostor;
            if (info.roleType == RoleType.Neutral) return Faction.Neutral;
            if (info.roleType == RoleType.Crewmate) return Faction.Crewmate;
        }

        var roleInfo = RoleInfo.getRoleInfoForPlayer(target, false, false)
            .FirstOrDefault(x => x.roleType is RoleType.Crewmate or RoleType.Impostor or RoleType.Neutral);
        if (roleInfo != null)
            return roleInfo.roleType switch
            {
                RoleType.Impostor => Faction.Impostor,
                RoleType.Neutral => Faction.Neutral,
                _ => Faction.Crewmate
            };

        if (target.IsNeutral()) return Faction.Neutral;
        if (target.Data?.Role?.IsImpostor == true) return Faction.Impostor;
        return Faction.Crewmate;
    }

    public static Color GetFactionColor(PlayerControl target)
    {
        var faction = GetCurrentFaction(target);
        if (affectedByVortox && Vortox.Reversal)
        {
            faction = faction switch
            {
                Faction.Crewmate => Faction.Impostor,
                Faction.Impostor => Faction.Crewmate,
                _ => faction
            };
        }
        return faction switch
        {
            Faction.Impostor => Palette.ImpostorRed,
            Faction.Neutral => Color.gray,
            _ => Color.green
        };
    }

    public static bool ShouldHideWorldIdentity(PlayerControl source, PlayerControl target)
    {
        // 零成本短路：本局没有灵光师（绝大多数局面）时直接返回。
        // 该函数会被 hidePlayerName 每帧对每个玩家调用多次，不能在这里做昂贵的可见性判定。
        if (aurial == null || target == null) return false;
        if (target == aurial) return false; // 灵光师看自己的名字不隐藏
        if (!HasAnonymousWorldView) return false;
        if (source == null || source.PlayerId != PlayerControl.LocalPlayer.PlayerId) return false;
        return true;
    }

    public static void RestoreAnonymousWorldViewIfInactive()
    {
        if (!HasAnonymousWorldView && anonymousPlayers.Count > 0) RestoreAnonymousWorldView();
    }

    public static void ApplyAnonymousWorldView()
    {
        if (!HasAnonymousWorldView) return;

        var local = PlayerControl.LocalPlayer;
        var visibleTargets = visibleTargetsBuffer;
        visibleTargets.Clear();
        // 低频兜底计数：帽子/面罩/皮肤只在被外部系统（伪装、蘑菇混战、morph）重置时才会偏离，
        // 无需每帧重设，每 30 帧兜底一次即可。
        var fullRefresh = false;
        if (++maintainTick >= 30)
        {
            maintainTick = 0;
            fullRefresh = true;
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.PlayerId == local.PlayerId || p.Data.Disconnected || !p.IsAlive()) continue;

            visibleTargets.Add(p.PlayerId);
            if (!anonymousPlayers.ContainsKey(p.PlayerId))
                anonymousPlayers[p.PlayerId] = CaptureVisualState(p);

            if (IsInvisible(p)) continue;

            try
            {
                var cosmetics = p.cosmetics;

                // Add 返回 true 表示该玩家本局尚未做过完整外观替换
                if (anonymousApplied.Add(p.PlayerId))
                {
                    // 进入匿名视图时执行一次完整外观替换（含皮肤与动画，开销较大）。
                    // 宠物用明确的 EmptyPet，与主线的匿名模式做法一致。
                    p.setLook("", 6, "", "", "", "pet_EmptyPet", false);
                    ApplyUniformPlayerSize(p);
                    ApplyAnonymousMaterial(cosmetics?.currentBodySprite?.BodySprite, p);
                    // 名字与色盲文本由主线 hidePlayerName 钩子（resetNameTagsAndColors）每帧处理，无需重复
                    continue;
                }

                // 每帧：读取托管字段做哨兵（几乎零开销，无 native 调用），仅在偏离时修复
                var needBodyRefresh = false;

                if (cosmetics != null && cosmetics.ColorId != 6)
                {
                    p.RawSetColor(6);
                    needBodyRefresh = true;
                }
                // 仅当确实带着可见宠物时才需要隐藏（PetData.IsEmpty 为游戏自带判定）
                if (cosmetics?.currentPet != null && cosmetics.currentPet.data?.IsEmpty == false)
                    p.RawSetPet("pet_EmptyPet", 6);
                if (Mathf.Abs(p.transform.localScale.x - 0.7f) > 0.001f) ApplyUniformPlayerSize(p);

                // 低频兜底：外部系统重置帽子/面罩/皮肤后，最迟 30 帧内恢复匿名
                if (fullRefresh)
                {
                    p.RawSetHat("", 6);
                    p.RawSetVisor("", 6);
                    p.RawSetSkin("", 6);
                    needBodyRefresh = true;
                }

                if (needBodyRefresh) ApplyAnonymousMaterial(cosmetics?.currentBodySprite?.BodySprite, p);
            }
            catch (Exception e)
            {
                Warn($"Aurial 匿名外观应用失败 (玩家 {p.PlayerId}): {e.Message}");
            }
        }

        // 只有在有玩家离场时才需要清理（可见目标与已接管玩家数量应该一致）
        if (anonymousPlayers.Count > visibleTargets.Count)
        {
            foreach (var id in anonymousPlayers.Keys.Where(id => !visibleTargets.Contains(id)).ToArray())
            {
                try
                {
                    RestoreAnonymousPlayer(PlayerById(id));
                }
                catch (Exception e)
                {
                    Warn($"Aurial 匿名外观恢复失败 (玩家 {id}): {e.Message}");
                }
                anonymousPlayers.Remove(id);
                anonymousApplied.Remove(id);
            }
        }
    }

    public static void ApplyAnonymousMaterial(SpriteRenderer renderer, PlayerControl target)
    {
        if (renderer == null || target == null) return;

        var revealed = revealedPlayers.Contains(target.PlayerId);
        var bodyColor = revealed ? GetFactionColor(target) : (Color)Palette.PlayerColors[6];
        var backColor = revealed
            ? new Color(bodyColor.r * 0.55f, bodyColor.g * 0.55f, bodyColor.b * 0.55f, bodyColor.a)
            : (Color)Palette.ShadowColors[6];
        var visorColor = revealed ? bodyColor : (Color)Palette.PlayerColors[6];

        renderer.material.SetColor("_BodyColor", bodyColor);
        renderer.material.SetColor("_BackColor", backColor);
        renderer.material.SetColor("_VisorColor", visorColor);
        renderer.material.SetFloat("_Outline", 0f);
        renderer.color = renderer.color.SetAlpha(Chameleon.visibility(target.PlayerId));
    }

    public static void RestoreAnonymousWorldView()
    {
        foreach (var id in anonymousPlayers.Keys.ToArray())
            RestoreAnonymousPlayer(PlayerById(id));
        anonymousPlayers.Clear();
        anonymousApplied.Clear();
    }

    private static AnonymousVisualState CaptureVisualState(PlayerControl target)
    {
        var state = new AnonymousVisualState { LocalScale = target.transform.localScale };
        var collider = target.Collider?.CastFast<CircleCollider2D>();
        if (collider != null)
        {
            state.HasCollider = true;
            state.ColliderRadius = collider.radius;
            state.ColliderOffset = collider.offset;
        }
        return state;
    }

    private static void ApplyUniformPlayerSize(PlayerControl target)
    {
        target.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        var collider = target.Collider?.CastFast<CircleCollider2D>();
        if (collider == null) return;
        collider.radius = 0.2233912f;
        collider.offset = 0.3636057f * Vector2.down;
    }

    private static void RestoreAnonymousPlayerSize(PlayerControl target)
    {
        if (target == null || !anonymousPlayers.TryGetValue(target.PlayerId, out var state)) return;
        if (Camouflager.camouflageTimer > 0f || isCamoComms || MushroomSabotageActive) return;

        target.transform.localScale = state.LocalScale;
        if (!state.HasCollider) return;
        var collider = target.Collider?.CastFast<CircleCollider2D>();
        if (collider == null) return;
        collider.radius = state.ColliderRadius;
        collider.offset = state.ColliderOffset;
    }

    private static bool IsInvisible(PlayerControl target)
    {
        if (target == Ninja.ninja && Ninja.isInvisable) return true;
        if (target == Phantom.Player && Phantom.isInvisable) return true;
        // isInvisable 前置短路，避免无豺狼时也每帧构造 LINQ 闭包
        if (Jackal.isInvisable && Jackal.jackal.Any(x => x == target)) return true;
        return false;
    }

    private static void RestoreAnonymousPlayer(PlayerControl target)
    {
        if (target == null || target.Data == null) return;

        if (IsInvisible(target))
        {
            RestoreAnonymousPlayerSize(target);
            return;
        }

        if (MushroomSabotageActive)
            target.setDefaultLook(false);
        else if (Camouflager.camouflageTimer > 0f || isCamoComms)
            target.setLook("", 6, "", "", "", "", false);
        else if (SurveillanceMinigamePatch.nightVisionIsActive)
            target.setLook("", 11, "", "", "", "", false);
        else if (target == Glitch.Player && Glitch.morphTimer > 0f && Glitch.morphTarget?.Data != null)
        {
            var outfit = Glitch.morphTarget.Data.DefaultOutfit;
            target.setLook(Glitch.morphTarget.Data.PlayerName, outfit.ColorId, outfit.HatId, outfit.VisorId, outfit.SkinId, outfit.PetId, false);
        }
        else
            target.setDefaultLook(false);

        RestoreAnonymousPlayerSize(target);
    }

    public static void ShowLocalPulse()
    {
        if (aurial == null || !aurial.AmOwner || DestroyableSingleton<HudManager>.Instance == null) return;

        var pulse = new GameObject("Aurial Radiation Pulse") { layer = 11 };
        pulse.transform.position = aurial.transform.position + new Vector3(0f, 0f, 0.05f);
        var renderer = pulse.AddComponent<SpriteRenderer>();
        renderer.sprite = buttonSprite;
        renderer.color = color.SetAlpha(0.55f);

        var extents = renderer.sprite != null ? renderer.sprite.bounds.extents.magnitude : 1f;
        var targetScale = Mathf.Max(radius / Mathf.Max(extents, 0.01f), 0.1f);

        DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.55f, new Action<float>(p =>
        {
            if (pulse == null) return;
            pulse.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, targetScale, p);
            renderer.color = color.SetAlpha(0.55f * (1f - p));
            if (p == 1f) UObject.Destroy(pulse);
        })));
    }

    public static void ShowLocalRadiationResult(IEnumerable<byte> targetIds, IEnumerable<byte> newlyRevealed)
    {
        if (aurial == null || !aurial.AmOwner) return;

        var revealed = new HashSet<byte>(newlyRevealed ?? []);
        foreach (var id in targetIds?.Distinct() ?? Enumerable.Empty<byte>())
        {
            if (PlayerById(id) == null) continue;

            if (revealed.Contains(id))
            {
                // 揭示瞬间立即刷新该玩家的阵营色材质（无需等待 30 帧兜底）
                try
                {
                    var revealedPlayer = PlayerById(id);
                    if (revealedPlayer != null)
                        ApplyAnonymousMaterial(revealedPlayer.cosmetics?.currentBodySprite?.BodySprite, revealedPlayer);
                }
                catch (Exception e)
                {
                    Warn($"Aurial 揭示材质刷新失败 (玩家 {id}): {e.Message}");
                }
            }

            var key = revealed.Contains(id) ? "Aurial.RadiationUnlocked" : "Aurial.RadiationProgress";
            ShowNotification(string.Format(GetString(key), GetHitCount(id), hitsToReveal));
        }
    }

    // ---- 辐射 RPC：本地请求 → 房主验证并广播打击列表 ----

    private static readonly Dictionary<byte, float> lastRadiationAt = new();
    private static float radiationAvailableAt;

    public static void RequestRadiation()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local != aurial || !local.IsAlive()) return;

        if (AmongUsClient.Instance.AmHost)
        {
            BroadcastRadiation(local.PlayerId);
            return;
        }

        var writer = StartRPC(CustomRPC.AurialRadiate, HostPlayer);
        writer.Write((byte)0);
        writer.Write(local.PlayerId);
        writer.EndRPC();
    }

    public static void HandleRadiationRpc(PlayerControl sender, MessageReader reader)
    {
        var action = reader.ReadByte();
        var casterId = reader.ReadByte();
        switch (action)
        {
            case 0:
                if (!AmongUsClient.Instance.AmHost || sender == null || sender.PlayerId != casterId) return;
                BroadcastRadiation(casterId);
                break;
            case 1:
            {
                if (sender != HostPlayer) return;
                var count = reader.ReadByte();
                var targets = new List<byte>(count);
                for (var i = 0; i < count; i++) targets.Add(reader.ReadByte());

                var newlyRevealed = ReceiveRadiation(casterId, targets);
                if (aurial != null && aurial.AmOwner)
                {
                    ShowLocalPulse();
                    ShowLocalRadiationResult(targets, newlyRevealed);
                }
                lastRadiationAt[casterId] = Time.realtimeSinceStartup;
                break;
            }
        }
    }

    private static void BroadcastRadiation(byte casterId)
    {
        var caster = PlayerById(casterId);
        if (!AmongUsClient.Instance.AmHost || caster == null || aurial != caster) return;
        if (!caster.IsAlive() || !caster.CanMove || !InGame || Helpers.InMeeting || ExileController.Instance != null) return;

        var now = Time.realtimeSinceStartup;
        if (now < radiationAvailableAt) return;
        if (lastRadiationAt.TryGetValue(casterId, out var last) && now - last < cooldown) return;
        lastRadiationAt[casterId] = now;

        var targets = GetRadiationTargets(casterId);
        var writer = StartRPC(CustomRPC.AurialRadiate);
        writer.Write((byte)1);
        writer.Write(casterId);
        writer.Write((byte)targets.Count);
        foreach (var id in targets) writer.Write(id);
        writer.EndRPC();

        var newlyRevealed = ReceiveRadiation(casterId, targets);
        if (aurial != null && aurial.AmOwner)
        {
            ShowLocalPulse();
            ShowLocalRadiationResult(targets, newlyRevealed);
        }
    }

    [OnGameStart(50)]
    public static void ResetHostRateLimits()
    {
        lastRadiationAt.Clear();
        radiationAvailableAt = Time.realtimeSinceStartup + cooldown;
    }

    public static void BeginActionRoundCooldowns()
    {
        if (!InGame) return;
        lastRadiationAt.Clear();
        radiationAvailableAt = Time.realtimeSinceStartup + cooldown;
    }

    public static void clearAndReload()
    {
        RestoreAnonymousWorldView();
        aurial = null;
        hitCounts.Clear();
        revealedPlayers.Clear();
        cooldown = CustomOptionHolder.aurialCooldown.GetFloat();
        radius = CustomOptionHolder.aurialRadius.GetFloat();
        hitsToReveal = Math.Max(1, CustomOptionHolder.aurialHitsToReveal.GetInt());
        vision = Mathf.Clamp(0.1f + CustomOptionHolder.aurialVision.GetSelection() * 0.05f, 0.1f, 1f);
        affectedByVortox = CustomOptionHolder.aurialAffectedByVortox.GetBool();
    }

    public static void ClearAndReload()
    {
        clearAndReload();
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static class AurialLightRadiusPatch
    {
        public static void Postfix(ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            if (player == null || aurial == null || !aurial.IsAlive()) return;
            if (player.PlayerId != aurial.PlayerId) return;
            if (Torch.torch.Any(x => x != null && x.PlayerId == player.PlayerId)) return;
            __result *= vision;
        }
    }
}
