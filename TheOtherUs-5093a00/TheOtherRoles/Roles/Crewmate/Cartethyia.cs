using TheOtherRoles.Patches;

namespace TheOtherRoles.Roles.Crewmate;

public static class Cartethyia
{
    public enum SwordState : byte
    {
        None,
        Shadow,
        Sword
    }

    public static PlayerControl cartethyia;
    public static Color color = new Color32(232, 202, 120, byte.MaxValue);

    public static float markCooldown = 10f;
    public static int swordsRequired = 2;
    public static bool abilityTriggersSword = true;

    public static readonly Dictionary<byte, SwordState> swords = new();
    public static PlayerControl currentTarget;
    public static bool usedFinalJudgmentThisMeeting;

    public static Sprite buttonSprite = new ResourceSprite("CartethyiaMarkButton.png", 515f);
    public static Sprite meetingSwordSprite = new ResourceSprite("CartethyiaMeetingSword.png", 1254f);
    public static Sprite transitionSprite = new ResourceSprite("CartethyiaJudgmentTransition.png", 100f);

    public static PlayerControl Player
    {
        get => cartethyia;
        set => cartethyia = value;
    }

    public static int SwordCount => swords.Values.Count(x => x == SwordState.Sword);

    public static void clearAndReload()
    {
        RestoreMeetingIcons();
        cartethyia = null;
        currentTarget = null;
        swords.Clear();
        usedFinalJudgmentThisMeeting = false;
        markCooldown = CustomOptionHolder.cartethyiaMarkCooldown.GetFloat();
        swordsRequired = Math.Max(1, CustomOptionHolder.cartethyiaSwordsRequired.GetInt());
        abilityTriggersSword = CustomOptionHolder.cartethyiaAbilityTriggersSword.GetBool();
    }

    public static void ClearAndReload()
    {
        clearAndReload();
    }

    public static bool HasSword(byte playerId)
        => swords.TryGetValue(playerId, out var state) && state == SwordState.Sword;

    public static void ApplyShadow(byte targetId)
    {
        if (!InGame || cartethyia == null) return;
        var target = PlayerById(targetId);
        if (target == null) return;
        if (swords.TryGetValue(targetId, out var state) && state != SwordState.None) return;

        swords[targetId] = SwordState.Shadow;
        if (PlayerControl.LocalPlayer == cartethyia)
            ShowNotification(string.Format(GetString("Cartethyia.ShadowApplied"), target.Data?.PlayerName ?? "?"));
    }

    public static void PromoteSword(byte playerId)
    {
        if (!InGame || cartethyia == null) return;
        if (swords.TryGetValue(playerId, out var state) && state == SwordState.Shadow)
            swords[playerId] = SwordState.Sword;
    }

    /// <summary>被剑影标记者发动击杀时，将其剑影升华为审判之剑。</summary>
    public static void OnKillInitiated(PlayerControl killer)
    {
        if (killer == null) return;
        PromoteSword(killer.PlayerId);
        RefreshMeetingIcons();
    }

    /// <summary>被剑影标记者使用技能时触发的升华（本地玩家调用，通过 RPC 同步）。</summary>
    public static void OnLocalAbilityUsed()
    {
        try
        {
            if (!InGame || Helpers.InMeeting || !abilityTriggersSword || cartethyia == null ||
                AmongUsClient.Instance == null) return;

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.IsDead) return;
            if (!swords.TryGetValue(local.PlayerId, out var state) || state != SwordState.Shadow) return;

            var writer = StartRPC(CustomRPC.CartethyiaPromoteSword);
            writer.Write(local.PlayerId);
            writer.EndRPC();
            PromoteSword(local.PlayerId);

            if (local == cartethyia) RefreshMeetingIcons();
        }
        catch (Exception e)
        {
            Error($"Cartethyia.OnLocalAbilityUsed: {e}");
        }
    }

    public static void OnPlayerDeath(PlayerControl player)
    {
        if (player == null) return;
        swords.Remove(player.PlayerId);
    }

    public static bool CanUseFinalJudgment()
        => cartethyia != null && cartethyia.IsAlive() && !usedFinalJudgmentThisMeeting && Helpers.InMeeting && SwordCount >= swordsRequired;

    private static RoleType GetTargetRoleType(PlayerControl target)
    {
        if (target == null) return RoleType.Crewmate;

        var roleId = PlayerData.GetPlayerData(target)?.MainRole ?? RoleId.DefaultRole;
        if (roleId != RoleId.DefaultRole && RoleInfo.RoleInfoById.TryGetValue(roleId, out var info))
            return info.roleType;

        var roleInfo = RoleInfo.getRoleInfoForPlayer(target, false, false).FirstOrDefault();
        if (roleInfo != null) return roleInfo.roleType;

        if (target.Data?.Role?.IsImpostor == true) return RoleType.Impostor;
        return RoleType.Crewmate;
    }

    public static void ExecuteFinalJudgment(byte targetId)
    {
        if (!InGame || cartethyia == null || MeetingHud.Instance == null) return;

        var target = PlayerById(targetId);
        if (target == null || usedFinalJudgmentThisMeeting || cartethyia.Data.IsDead || !HasSword(targetId)) return;

        usedFinalJudgmentThisMeeting = true;

        // 消耗全部审判之剑
        foreach (var id in swords.Where(x => x.Value == SwordState.Sword).Select(x => x.Key).ToList())
            swords.Remove(id);

        RestoreMeetingIcons();

        if (GetTargetRoleType(target) == RoleType.Crewmate)
        {
            // 目标是好人：审判反噬，卡提希娅自杀
            if (cartethyia.IsAlive())
            {
                PlayerData.SetDeathReason(cartethyia, CustomDeathReason.Judgment, cartethyia);
                cartethyia.CustomExiled(cartethyia, true);
            }
            if (PlayerControl.LocalPlayer == cartethyia || CanSeeGhostInfo)
                ShowNotification(GetString("Cartethyia.JudgmentBacklash"));
            if (Balancer.currentTarget == cartethyia) Balancer.currentTarget = null;
            CleanupMeetingVotes(cartethyia.PlayerId);
            ShowJudgmentTransition();
            return;
        }

        // 目标是中立或内鬼：处决
        if (target.IsAlive())
        {
            PlayerData.SetDeathReason(target, CustomDeathReason.Judgment, cartethyia);
            target.CustomExiled(cartethyia, true);
        }
        if (PlayerControl.LocalPlayer == cartethyia || CanSeeGhostInfo)
            ShowNotification(string.Format(GetString("Cartethyia.JudgmentExecuted"), target.Data?.PlayerName ?? "?"));
        if (Balancer.currentTarget == target) Balancer.currentTarget = null;
        CleanupMeetingVotes(targetId);
        ShowJudgmentTransition();
    }

    private static void RestoreMeetingIcons()
    {
        if (MeetingHud.Instance == null) return;
        foreach (var pva in MeetingHud.Instance.playerStates)
        {
            var icon = pva.transform.FindChild("CartethyiaSwordIcon");
            if (icon != null) UObject.Destroy(icon.gameObject);
        }
    }

    private static void RefreshMeetingIcons()
    {
        if (MeetingHud.Instance == null || cartethyia == null) return;
        BuildSwordIcons(MeetingHud.Instance);
    }

    internal static void BuildSwordIcons(MeetingHud meeting)
    {
        if (meeting == null || cartethyia == null) return;
        if (PlayerControl.LocalPlayer != cartethyia) return;

        foreach (var pva in meeting.playerStates)
        {
            var target = PlayerById(pva.TargetPlayerId);
            var hasSword = target != null && !target.Data.IsDead && HasSword(target.PlayerId);
            var existing = pva.transform.FindChild("CartethyiaSwordIcon");

            if (!hasSword)
            {
                if (existing != null) UObject.Destroy(existing.gameObject);
                continue;
            }
            if (existing != null) continue;

            var cancelButton = pva.Buttons?.transform.Find("CancelButton")?.gameObject;
            if (cancelButton == null) continue;
            var icon = UObject.Instantiate(cancelButton, pva.transform);
            icon.name = "CartethyiaSwordIcon";
            icon.transform.localPosition = new Vector3(1f, 0.03f, -1f);
            var renderer = icon.GetComponent<SpriteRenderer>();
            renderer.sprite = meetingSwordSprite;
            renderer.color = color;

            if (cartethyia.IsAlive() && CanUseFinalJudgment())
            {
                var button = icon.GetComponent<PassiveButton>();
                button.OnClick.RemoveAllListeners();
                var id = target.PlayerId;
                button.OnClick.AddListener((Action)(() => OnSwordClick(meeting, id)));
            }
        }
    }

    private static void OnSwordClick(MeetingHud meeting, byte targetId)
    {
        var target = PlayerById(targetId);
        if (target == null || target.Data.IsDead) return;
        if (!CanUseFinalJudgment() || !HasSword(targetId)) return;
        // Voted(1) 之后与 Animating(4) 状态不再接受点击（与 VoteStates 的语义保持一致）
        if ((int)meeting.state == 1 || (int)meeting.state == 4) return;

        var writer = StartRPC(CustomRPC.CartethyiaFinalJudgment);
        writer.Write(targetId);
        writer.EndRPC();
        ExecuteFinalJudgment(targetId);
    }

    private static void CleanupMeetingVotes(byte deadId)
    {
        if (MeetingHud.Instance == null) return;

        Helpers.ExtendMeetingTime(CustomOptionHolder.guessExtendmeetingTime.GetFloat());
        MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, deadId);

        foreach (var pva in MeetingHud.Instance.playerStates)
        {
            if (CustomOptionHolder.guessReVote.GetBool() || pva.VotedFor == deadId)
            {
                pva.UnsetVote();
                var pc = PlayerById(pva.TargetPlayerId);
                if (pc == null || pc.AmOwner) MeetingHud.Instance.ClearVote();
            }
        }
        if (AmongUsClient.Instance.AmHost) MeetingHud.Instance.CheckForEndVoting();
    }

    public static void ShowJudgmentTransition()
    {
        try
        {
            if (DestroyableSingleton<HudManager>.Instance == null || PlayerControl.LocalPlayer == null) return;

            var sprite = transitionSprite;
            if (sprite == null) return;

            var fullScreen = DestroyableSingleton<HudManager>.Instance.FullScreen;
            var overlay = new GameObject("CartethyiaJudgmentTransition");
            var renderer = overlay.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 1f, 1f, 0f);

            if (fullScreen != null)
            {
                overlay.layer = fullScreen.gameObject.layer;
                overlay.transform.SetParent(fullScreen.transform.parent);
                overlay.transform.localPosition = fullScreen.transform.localPosition;
                overlay.transform.localScale = fullScreen.transform.localScale;
                renderer.sortingLayerName = fullScreen.sortingLayerName;
                renderer.sortingOrder = fullScreen.sortingOrder + 1;
            }
            else
            {
                var cam = Camera.main;
                if (cam == null) return;
                overlay.layer = 5;
                overlay.transform.SetParent(cam.transform, false);
                overlay.transform.localPosition = new Vector3(0f, 0f, 10f);
                renderer.sortingOrder = 1000;
                var sizeY = cam.orthographicSize * 2f;
                var sizeX = sizeY * cam.aspect;
                var spriteSize = sprite.bounds.size;
                overlay.transform.localScale = Vector3.one * Mathf.Max(sizeX / spriteSize.x, sizeY / spriteSize.y);
            }

            DestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(2.2f, new Action<float>(p =>
            {
                if (overlay == null) return;
                var alpha = p < 0.15f ? p / 0.15f : (p < 0.8f ? 1f : Mathf.Clamp01((1f - p) / 0.2f));
                renderer.color = new Color(1f, 1f, 1f, alpha);
                if (p == 1f) UObject.Destroy(overlay);
            })));
        }
        catch (Exception e)
        {
            Error($"Cartethyia.ShowJudgmentTransition: {e}");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class Cartethyia_Patch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!InGame || cartethyia == null || __instance == null) return;

            usedFinalJudgmentThisMeeting = false;
            if (PlayerControl.LocalPlayer != cartethyia) return;

            BuildSwordIcons(__instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), "OnDestroy")]
    public static class CartethyiaMeetingClose_Patch
    {
        public static void Postfix()
        {
            RestoreMeetingIcons();
        }
    }
}
