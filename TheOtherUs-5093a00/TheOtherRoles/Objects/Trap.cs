using TheOtherRoles.Attributes;

namespace TheOtherRoles.Objects;

public class Trap : NetworkCustomObject
{
    static Trap()
    {
        Register<Trap>((pos, payload) => new Trap(PlayerById(payload[0]), pos));
    }

    // 同一玩家可能同时踩中多个陷阱：用引用计数冻结，避免一个陷阱到期就把还在另一个陷阱里的玩家放开。
    private static readonly Dictionary<byte, int> activeFreezeCounts = new();
    private static readonly Dictionary<byte, bool> moveableBeforeTrapFreeze = new();

    public PlayerControl trapper;
    public List<PlayerControl> trapped = new();

    private readonly HashSet<byte> activeFrozenTargets = new();

    private static Sprite trapSprite = new ResourceSprite("Trapper_Trap_Ingame.png", 300);
    private Arrow arrow = new(Color.blue);
    private int neededCount = Trapper.trapCountToReveal;
    public bool revealed;
    public bool triggerable;
    private int usedCount;
    private string room;

    public GameObject ArrowObject => arrow?.arrow;

    public Trap(PlayerControl player, Vector3 pos)
    {
        GameObject.layer = 11;
        GameObject.name = "Trap " + Id;
        var position = new Vector3(pos.x, pos.y, pos.y + 0.001f);
        GameObject.transform.position = position;
        neededCount = Trapper.trapCountToReveal;

        Renderer.sprite = trapSprite;
        Renderer.color = Color.white * new Vector4(1, 1, 1, 0.5f);
        trapper = player;
        arrow.Update(position);
        arrow.arrow.SetActive(false);
        GameObject.SetActive(false);
        if (PlayerControl.LocalPlayer == player) GameObject.SetActive(true);

        var lastRoom = FastDestroyableSingleton<HudManager>.Instance?.roomTracker?.LastRoom?.RoomId ?? null;
        room = lastRoom != null ? FastDestroyableSingleton<TranslationController>.Instance.GetString((SystemTypes)lastRoom) : "室外";

        _ = new LateTask(() =>
        {
            if (GameObject == null || Renderer == null) return;
            triggerable = true;
            Renderer.color = Color.white;
        }, 5f);

        Message($"陷阱已放置: 房间={room} 位置={position} 所需触发={neededCount}", "Trapper");
    }

    public override void OnDestroy()
    {
        try
        {
            // 陷阱被清除时立即解冻，避免玩家卡在原地直到超时（甚至永久卡住）
            foreach (var playerId in activeFrozenTargets.ToArray())
                ReleaseTarget(playerId);
            trapped.Clear();
            arrow?.arrow?.Destroy();
        }
        catch { }
        base.OnDestroy();
    }

    public static void clearRevealedTraps()
    {
        var trapsToClear = AllObjects.OfType<Trap>().Where(x => x.revealed).ToArray();
        foreach (var t in trapsToClear)
        {
            t.Destroy();
        }
    }

    public static void ClearAllTraps(PlayerControl trapper, bool active)
    {
        var traps = AllObjects.OfType<Trap>().Where(x => x.trapper == trapper && (active || !x.revealed));
        foreach (var t in traps)
        {
            t?.Destroy();
        }
    }

    public static void triggerTrap(byte targetId, ushort trapId)
    {
        var t = Find<Trap>(trapId);
        var target = PlayerById(targetId);
        if (Trapper.trapper == null || t == null || target == null || t.trapped.Contains(target)) return;

        t.usedCount++;
        t.triggerable = false;
        if (targetId == PlayerControl.LocalPlayer.PlayerId || targetId == Trapper.trapper?.PlayerId)
        {
            SoundEffectsManager.play("trapperTrap");
        }

        t.FreezeTarget(target);
        target.NetTransform.Halt();
        if (PlayerControl.LocalPlayer.PlayerId == t.trapper?.PlayerId) t.arrow.arrow.SetActive(true);

        t.trapped.Add(target);
        if (t.usedCount == t.neededCount) t.revealed = true;

        Message($"陷阱被踩中: 目标={target.Data?.PlayerName ?? targetId.ToString()} 触发数={t.usedCount}/{t.neededCount}", "Trapper");

        // 1.2.9.6 原版行为：达标的那次踩中立即把日志发给设陷师，不等待夹住时间结束
        if (t.revealed && (PlayerControl.LocalPlayer == t.trapper || CanSeeGhostInfo))
        {
            var message = $"陷阱 {t.room} 日志: \n";
            t.trapped = t.trapped.OrderBy(_ => rnd.Next()).ToList();
            message = t.trapped.Aggregate(message, (current, p) => current + Trapper.infoType switch
            {
                0 => RoleInfo.GetRolesString(p, false, false, false) + "\n",
                1 when (p.IsEvilNeutral() || p.IsKillerNeutral() || p.IsImpostor()) ^ Vortox.Reversal => "邪恶职业 \n",
                1 => "善良职业 \n",
                _ => p.Data.PlayerName + "\n"
            });

            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(Trapper.trapper, $"{message}", true);
            Message($"陷阱日志已发送 (信息类型={Trapper.infoType})", "Trapper");
        }

        _ = new LateTask(() =>
        {
            if (t.GameObject == null) return;
            if (!t.activeFrozenTargets.Contains(targetId)) return;

            t.ReleaseTarget(targetId);
            if (t.arrow?.arrow != null)
            {
                t.arrow.arrow.SetActive(false);
                t.triggerable = true;
            }
        }, Trapper.trapDuration);
    }

    public override void Update()
    {
        // 效颦者借用期间，原始设陷师的旧陷阱不应继续生效
        if (Imitator.IsImitating(Trapper.trapper, RoleId.Trapper) && trapper != Trapper.trapper)
        {
            GameObject.SetActive(false);
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (arrow.arrow.active) arrow.Update();

        var canSee = CanSeeGhostInfo || player == trapper || trapped.Any(x => x.PlayerId == player.PlayerId);
        GameObject.SetActive(canSee);

        if (revealed || !triggerable || trapped.Any(x => x.PlayerId == player.PlayerId)) return;

        if (player.inVent || !player.CanMove) return;
        var distance = Vector2.Distance(GameObject.transform.position, player.GetTruePosition());
        if (distance < 0.6f && player.PlayerId != trapper?.PlayerId && player.IsAlive())
        {
            var writer = StartRPC(CustomRPC.TriggerTrap);
            writer.Write(player.PlayerId);
            writer.Write(NetworkId);
            writer.EndRPC();
            triggerTrap(player.PlayerId, NetworkId);
        }
    }

    private void FreezeTarget(PlayerControl target)
    {
        if (target == null || !activeFrozenTargets.Add(target.PlayerId)) return;

        if (activeFreezeCounts.TryGetValue(target.PlayerId, out var count))
        {
            activeFreezeCounts[target.PlayerId] = count + 1;
        }
        else
        {
            activeFreezeCounts[target.PlayerId] = 1;
            moveableBeforeTrapFreeze[target.PlayerId] = target.moveable;
        }
        target.moveable = false;
    }

    private void ReleaseTarget(byte playerId)
    {
        if (!activeFrozenTargets.Remove(playerId) || !activeFreezeCounts.TryGetValue(playerId, out var count)) return;

        if (count > 1)
        {
            activeFreezeCounts[playerId] = count - 1;
            return;
        }

        activeFreezeCounts.Remove(playerId);
        var moveable = moveableBeforeTrapFreeze.TryGetValue(playerId, out var before) && before;
        moveableBeforeTrapFreeze.Remove(playerId);

        var target = PlayerById(playerId);
        if (target != null) target.moveable = moveable;
    }

    [OnGameStart, OnGameEnd]
    public static void ResetFreezeState()
    {
        activeFreezeCounts.Clear();
        moveableBeforeTrapFreeze.Clear();
    }
}
