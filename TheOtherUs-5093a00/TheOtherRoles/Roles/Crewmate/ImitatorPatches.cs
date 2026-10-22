namespace TheOtherRoles.Roles.Crewmate;

[HarmonyPatch]
public static class ImitatorPatches
{
    private static readonly Dictionary<byte, GameObject> selectionButtons = new();
    private static readonly Dictionary<byte, uint> buttonAnimationTokens = new();
    private static byte pendingTargetId = byte.MaxValue;
    private static RoleId pendingRole = RoleId.DefaultRole;
    private static bool selectionSentThisMeeting;

    public static void NotifyTargetInvalidated(byte targetId)
    {
        if (pendingTargetId != targetId) return;
        pendingTargetId = byte.MaxValue;
        pendingRole = RoleId.DefaultRole;
        if (selectionButtons.TryGetValue(targetId, out var button) && button != null)
        {
            SetButtonColor(button, Color.white);
            button.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(RoleHelpers), nameof(RoleHelpers.ResetRoleSelection))]
    private static class GlobalRoleResetPatch
    {
        public static void Prefix()
        {
            if (Imitator.Player != null && Imitator.IsActive) Imitator.Abort(Imitator.Player.PlayerId);
        }
    }

    [HarmonyPatch(typeof(RPCProcedure), nameof(RPCProcedure.erasePlayerRoles))]
    private static class EraseRolePatch
    {
        public static void Prefix(byte playerId)
        {
            Imitator.NotifyPlayerStateChanged(playerId);
            if (Imitator.Player?.PlayerId == playerId && Imitator.IsActive) Imitator.Abort(playerId);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    private static class MeetingStartPatch
    {
        public static void Prefix()
        {
            Imitator.BeginMeeting();
            pendingTargetId = byte.MaxValue;
            pendingRole = RoleId.DefaultRole;
            selectionSentThisMeeting = false;
            // 注意：这里绝不能销毁 selectionButtons 里的按钮。会议开始前的入口（ServerStart/Deserialize）
            // 创建的按钮是唯一可靠的可见按钮——会议开始时投票行尚未就位，重建会失败。
            // 重复创建由 AddSelectionButtons 自身的去重与迁移逻辑处理。
            buttonAnimationTokens.Clear();

            if (!Imitator.HasAnyActiveSession) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || !Imitator.IsActiveFor(player.PlayerId)) continue;

                if (player.AmOwner)
                {
                    Imitator.TryGetSessionGeneration(player, Imitator.GetEffectiveRole(player), out var generation);
                    var writer = StartRPC(CustomRPC.EndImitation);
                    writer.Write(player.PlayerId);
                    writer.Write((int)generation);
                    writer.EndRPC();
                }
                Imitator.End(player.PlayerId);
            }
        }

        public static void Postfix(MeetingHud __instance)
        {
            Imitator.ShowMeetingCarry(__instance);
            AddSelectionButtons(__instance);
            // 兜底：若所有创建入口触发时投票行都还没就位（全停在屏幕外），上面的调用会一无所获。
            // 延迟 1 秒重试一次——AddSelectionButtons 自带去重与迁移逻辑，重复调用无副作用。
            _ = new LateTask(() => AddSelectionButtons(MeetingHud.Instance), 1f, "ImitatorButtonRetry");
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
    private static class MeetingServerStartPatch
    {
        public static void Postfix(MeetingHud __instance) => AddSelectionButtons(__instance);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
    private static class MeetingDeserializePatch
    {
        public static void Postfix(MeetingHud __instance, [HarmonyArgument(1)] bool initialState)
        {
            if (initialState) AddSelectionButtons(__instance);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    private static class VotingCompletePatch
    {
        public static void Postfix()
        {
            if (selectionSentThisMeeting) return;

            var local = PlayerControl.LocalPlayer;
            if (local == null || Imitator.Player != local || local.IsDead()) return;
            selectionSentThisMeeting = true;

            var targetId = pendingTargetId;
            var role = pendingRole;
            var target = PlayerById(targetId);
            if (targetId == byte.MaxValue || target?.Data == null || !target.Data.IsDead
                || target.Data.Disconnected || Imitator.GetEligibleRole(target) != role)
            {
                if (pendingTargetId != byte.MaxValue)
                    Message($"效颦者选择在提交时失效，已按“未选择”提交 (目标={target?.Data?.PlayerName ?? pendingTargetId.ToString()}, 记录职业={role}, 当前可借用={Imitator.GetEligibleRole(target)})", "Imitator");
                targetId = byte.MaxValue;
                role = RoleId.DefaultRole;
            }
            else
            {
                Message($"效颦者提交选择: 目标={target.Data.PlayerName} 职业={role}", "Imitator");
            }

            var writer = StartRPC(CustomRPC.SetImitatorTarget, HostPlayer);
            writer.Write((byte)0);
            writer.Write(local.PlayerId);
            writer.Write(targetId);
            writer.Write((byte)role);
            writer.Write((int)Imitator.CurrentMeetingGeneration);
            writer.EndRPC();

            if (AmongUsClient.Instance.AmHost
                && Imitator.TryHostAuthorizeSelection(local, local.PlayerId, targetId, role, Imitator.CurrentMeetingGeneration,
                    out var canonicalTargetId, out var canonicalRole))
                BroadcastCanonicalSelection(local.PlayerId, canonicalTargetId, canonicalRole, Imitator.CurrentMeetingGeneration);

            foreach (var button in selectionButtons.Values)
                if (button != null) button.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), "OnDestroy")]
    private static class MeetingDestroyPatch
    {
        public static void Postfix()
        {
            selectionButtons.Clear();
            buttonAnimationTokens.Clear();
            Imitator.ClearMeetingCarries();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    private static class MeetingDeathRefreshPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // 会议中有人死亡（如赌怪猜测击杀）后，其职业可能成为新的可借用目标。
            // 选择按钮只在会议开始时创建一次，这里增量补建（已存在的会被跳过）。
            if (!Helpers.InMeeting || MeetingHud.Instance == null) return;
            AddSelectionButtons(MeetingHud.Instance);
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    private static class ExileWrapUpPatch
    {
        public static void Postfix() => TryHostStartImitations();
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    private static class AirshipWrapUpPatch
    {
        public static void Postfix() => TryHostStartImitations();
    }

    public static void HandleSelectionRpc(PlayerControl sender, MessageReader reader)
    {
        var action = reader.ReadByte();
        var imitatorId = reader.ReadByte();
        var targetId = reader.ReadByte();
        var roleSnapshot = (RoleId)reader.ReadByte();
        var generation = (uint)reader.ReadInt32();
        switch (action)
        {
            case 0:
                if (AmongUsClient.Instance.AmHost
                    && Imitator.TryHostAuthorizeSelection(sender, imitatorId, targetId, roleSnapshot, generation,
                        out var canonicalTargetId, out var canonicalRole))
                    BroadcastCanonicalSelection(imitatorId, canonicalTargetId, canonicalRole, generation);
                break;
            case 1:
                if (sender == HostPlayer) Imitator.ApplyHostSelection(imitatorId, targetId, roleSnapshot, generation);
                break;
        }
    }

    private static void BroadcastCanonicalSelection(byte imitatorId, byte targetId, RoleId role, uint generation)
    {
        var writer = StartRPC(CustomRPC.SetImitatorTarget, HostPlayer);
        writer.Write((byte)1);
        writer.Write(imitatorId);
        writer.Write(targetId);
        writer.Write((byte)role);
        writer.Write((int)generation);
        writer.EndRPC();
        Imitator.ApplyHostSelection(imitatorId, targetId, role, generation);
    }

    // 放逐结束后由 ExileControllerPatch.WrapUpPostfix 调用（潜艇图走 ExileCutscene 销毁路径，
    // 不触发基类 WrapUp，所以不能只挂在 ExileController.WrapUp 上）。
    internal static void TryHostStartImitations()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var starts = Imitator.GetAuthorizedStartsForHost();
        if (starts.Count > 0) Message($"开始处理 {starts.Count} 个效颦者借用授权", "Imitator");

        foreach (var start in starts)
        {
            Message($"启动借用: 效颦者={PlayerById(start.ImitatorId)?.Data?.PlayerName ?? start.ImitatorId.ToString()} 职业={start.Role} 代际={start.MeetingGeneration}", "Imitator");
            var writer = StartRPC(CustomRPC.StartImitation, HostPlayer);
            writer.Write(start.ImitatorId);
            writer.Write((byte)start.Role);
            writer.Write((int)start.MeetingGeneration);
            writer.EndRPC();
            Imitator.StartAuthorized(start.ImitatorId, start.Role, start.MeetingGeneration);
        }
    }

    private static void AddSelectionButtons(MeetingHud meeting)
    {
        var local = PlayerControl.LocalPlayer;
        if (meeting == null || local == null || Imitator.Player != local || local.IsDead()) return;

        var playerData = PlayerData.GetPlayerData(local);
        if (playerData == null || playerData.MainRole != RoleId.Imitator) return;

        var createdCount = 0;
        // 按钮是投票行的子物体，随上一场会议一起销毁，先清理失效记录
        foreach (var staleId in selectionButtons.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToArray())
            selectionButtons.Remove(staleId);

        foreach (var pva in meeting.playerStates ?? Enumerable.Empty<PlayerVoteArea>())
        {
            if (pva == null) continue;
            // CrowdedPlayer 分页（15人支持）会把未使用/非当前页的投票行停泊在屏幕外（y≈-28~-38），
            // 这些行可能依然激活且带着有效目标 id；在停泊行上建按钮等于建隐形按钮，必须跳过。
            if (pva.transform.position.y < -10f) continue;

            var target = PlayerById(pva.TargetPlayerId);
            var role = Imitator.GetEligibleRole(target);
            if (!Imitator.IsCompatible(role)) continue;

            // 该目标已有按钮：挂在可见行上则跳过（防止同位置叠加两个按钮导致一次点击触发两次切换）；
            // 挂在停泊行上（早期入口创建时行尚未就位）则销毁并在当前可见行重建。
            if (selectionButtons.TryGetValue(pva.TargetPlayerId, out var existing) && existing != null)
            {
                if (existing.transform.position.y >= -10f) continue;
                UObject.Destroy(existing);
                selectionButtons.Remove(pva.TargetPlayerId);
            }

            var cancelButton = pva.Buttons?.transform.Find("CancelButton")?.gameObject;
            if (cancelButton == null && pva.Buttons != null && pva.Buttons.transform.childCount > 0)
                cancelButton = pva.Buttons.transform.GetChild(0).gameObject;
            if (cancelButton == null) continue;

            var targetId = target.PlayerId;
            // 挂到投票行根节点：不能放进取消按钮的 Buttons 容器——原版会议里该容器在点开投票卡前是隐藏的，
            // 放进去按钮会跟着隐身。位置直接用取消按钮的世界坐标加偏移，不做任何坐标帧换算，
            // 缩放由克隆继承（世界缩放与模板一致），翻页时按钮跟随投票行移动。
            var button = UObject.Instantiate(cancelButton);
            button.name = "ImitatorSelectButton";
            // 克隆会复制模板的激活状态：未点开的卡片上取消按钮本体是隐藏的（activeSelf=false），
            // 不强制激活的话按钮生来就是关闭状态——不渲染、也无法点击。
            button.SetActive(true);
            button.transform.SetParent(pva.transform);
            button.transform.position = cancelButton.transform.position + new Vector3(-0.75f, 0f, 0.1f);
            var buttonRenderer = button.GetComponent<SpriteRenderer>();
            buttonRenderer.sprite = Imitator.selectSprite;
            SetButtonColor(button, Color.white);
            Message($"效颦者按钮定位: 目标={targetId} 世界坐标={button.transform.position} 取消钮={cancelButton.transform.position} 贴图={(buttonRenderer.sprite != null ? "OK" : "缺失")}", "Imitator");

            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((Action)(() => ToggleSelection(meeting, targetId, role)));

            selectionButtons[targetId] = button;
            createdCount++;
        }

        if (createdCount > 0) Message($"效颦者选择按钮已创建 x{createdCount}", "Imitator");
    }

    private static void ToggleSelection(MeetingHud meeting, byte targetId, RoleId role)
    {
        if (meeting == null)
        {
            Message("效颦者点击未生效: meeting 引用为空", "Imitator");
            return;
        }
        if ((int)meeting.state == 4) return;

        if (pendingTargetId == targetId)
        {
            pendingTargetId = byte.MaxValue;
            pendingRole = RoleId.DefaultRole;
            Message($"效颦者取消选择: 目标={targetId}", "Imitator");
        }
        else
        {
            var currentRole = Imitator.GetEligibleRole(PlayerById(targetId));
            if (currentRole != role)
            {
                Message($"效颦者点击未生效: 可借用职业已变化 (点击时={currentRole}, 按钮创建时={role})", "Imitator");
                return;
            }
            pendingTargetId = targetId;
            pendingRole = role;
            Message($"效颦者已选择: 目标={targetId} 职业={role}", "Imitator");
        }

        foreach (var (id, button) in selectionButtons)
        {
            if (button == null) continue;
            var renderer = button.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;

            var selected = id == pendingTargetId;
            renderer.sprite = selected ? Imitator.deselectSprite : Imitator.selectSprite;
            AnimateButtonColor(id, button, selected ? Palette.AcceptedGreen : Color.white);
        }
    }

    // 点击后的变色动画：先闪亮，再在 0.3 秒内过渡到状态色（选中绿 / 未选中白）。
    // 连续点击时通过 token 只保留最新一次动画，避免多个协程互相覆盖颜色。
    // 动画是纯表现层：整体 try/catch，任何失败都不影响选择状态。
    // 用托管迭代器而不是 Effects.Lerp（后者返回 il2Cpp 枚举器，Coroutines.Start 不接受）。
    private static void AnimateButtonColor(byte targetId, GameObject button, Color targetColor)
    {
        try
        {
            var renderer = button != null ? button.GetComponent<SpriteRenderer>() : null;
            if (renderer == null) return;

            buttonAnimationTokens.TryGetValue(targetId, out var token);
            buttonAnimationTokens[targetId] = ++token;

            var flashColor = Color.Lerp(targetColor, Color.white, 0.65f);
            renderer.color = flashColor;
            Coroutines.Start(ColorLerpRoutine(targetId, renderer, flashColor, targetColor, token));
        }
        catch (Exception e)
        {
            Warn($"效颦者按钮动画失败: {e.Message}");
        }
    }

    private static IEnumerator ColorLerpRoutine(byte targetId, SpriteRenderer renderer, Color from, Color to, uint token)
    {
        const float duration = 0.3f;
        for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            // 按钮被销毁或有更新的动画接管时立即停止
            if (renderer == null || !buttonAnimationTokens.TryGetValue(targetId, out var current) || current != token) yield break;
            renderer.color = Color.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        if (renderer != null && buttonAnimationTokens.TryGetValue(targetId, out var last) && last == token)
            renderer.color = to;
    }

    private static void SetButtonColor(GameObject button, Color color)
    {
        var renderer = button != null ? button.GetComponent<SpriteRenderer>() : null;
        if (renderer != null) renderer.color = color;
    }
}
