namespace TheOtherRoles.Objects;

public class KillTrap : NetworkCustomObject
{
    static KillTrap()
    {
        Register<KillTrap>((pos, payload) => new KillTrap(PlayerById(payload[0]), pos));
    }

    public static Sprite trapSprite = new ResourceSprite("Trap.png", 300);
    public static AudioRolloffMode rollOffMode = AudioRolloffMode.Linear;
    public AudioSource audioSource;
    public bool isDisabled;
    public bool isTriggered;
    public bool IsKilled;
    public PlayerControl trapper;
    public PlayerControl target;

    public static Sprite trapActiveSprite = new ResourceSprite("TrapActive.png", 300);
    public static AudioClip place => SoundEffectsManager.get("TrapperPlace");
    public static AudioClip activate => SoundEffectsManager.get("TrapperActivate");
    public static AudioClip disable => SoundEffectsManager.get("TrapperDisable");
    public static AudioClip countdown => SoundEffectsManager.get("TrapperCountdown");
    public static AudioClip kill => SoundEffectsManager.get("TrapperKill");

    public KillTrap(PlayerControl trapper, Vector3 pos)
    {
        var traps = AllObjects.OfType<KillTrap>().Where(x => x.trapper == trapper);
        // 最初の罠を消す
        if (traps.Count() >= EvilTrapper.numTrap)
        {
            var oldestTrap = traps.OrderBy(t => t.NetworkId).FirstOrDefault();
            oldestTrap?.Destroy();
        }

        // 罠を設置
        GameObject.name = "KillTrap " + Id;
        Renderer.sprite = trapSprite;
        Renderer.color = Color.white * new Vector4(1, 1, 1, 0.5f);
        Vector3 position = new(pos.x, pos.y, pos.z + 0.001f);
        GameObject.transform.position = position;
        GameObject.SetActive(true);
        isDisabled = false;
        IsActive = false;
        isTriggered = false;

        _ = new LateTask(() =>
        {
            IsActive = true;
            Renderer.color = Color.white;

        }, EvilTrapper.extensionTime);

        // 音を鳴らす
        audioSource = GameObject.gameObject.AddComponent<AudioSource>();
        audioSource.priority = 0;
        audioSource.spatialBlend = 1;
        audioSource.volume = 0.6f;
        audioSource.clip = place;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.maxDistance = EvilTrapper.placeSoundRange;
        audioSource.minDistance = 0.5f;
        audioSource.rolloffMode = rollOffMode;
        audioSource.PlayOneShot(place);
        this.trapper = trapper;
        Message($"邪恶陷阱已放置: 位置={position} 激活延迟={EvilTrapper.extensionTime}s", "EvilTrapper");
    }

    public override void OnDestroy()
    {
        try
        {
            if (audioSource != null) audioSource?.Stop();
        }
        catch { }
        base.OnDestroy();
    }

    public override void OnMeetingEnd(MeetingHud __instance)
    {
        this?.Destroy();
    }

    public static void ClearAllTraps(PlayerControl trapper, bool active)
    {
        var traps = AllObjects.OfType<KillTrap>().Where(x => x.trapper == trapper && (active || !x.isTriggered)).ToArray();
        foreach (var t in traps)
        {
            t?.Destroy();
        }
    }

    public static void activateTrap(PlayerControl trapper, PlayerControl target, ushort trapId)
    {
        var trap = Find<KillTrap>(trapId);
        if (trap == null || trap.isDisabled) return;
        // 有効にする

        trap.isTriggered = true;
        trap.target = target;
        var spriteRenderer = trap.GameObject.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = trapActiveSprite;

        ClearAllTraps(trapper, false);

        if (PlayerControl.LocalPlayer == trapper)
        {
            TMP_Text text;
            RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
            GameObject gameObject = UObject.Instantiate(roomTracker.gameObject);
            UObject.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
            gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
            gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
            gameObject.transform.localScale = Vector3.one * 1.5f;
            text = gameObject.GetComponent<TMP_Text>();
            text.text = string.Format(GetString("trapperGotTrapText"), target?.Data?.PlayerName ?? "NULL");
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            {
                if (p == 1f && text != null && text.gameObject != null)
                {
                    UObject.Destroy(text.gameObject);
                }
            })));
        }

        // 音を鳴らす
        trap.audioSource.Stop();
        trap.audioSource.loop = true;
        trap.audioSource.priority = 0;
        trap.audioSource.spatialBlend = 1;
        trap.audioSource.maxDistance = EvilTrapper.killSoundRange;
        trap.audioSource.minDistance = 0.5f;
        trap.audioSource.clip = countdown;
        trap.audioSource.Play();

        // ターゲットを動けなくする
        target.NetTransform.Halt();
        Message($"邪恶陷阱触发: 目标={target.Data?.PlayerName ?? target.PlayerId.ToString()} 击杀倒计时={EvilTrapper.killTimer}s", "EvilTrapper");

        var moveableFlag = false;
        HudManager.Instance.StartCoroutine(Effects.Lerp(EvilTrapper.killTimer, new Action<float>((p) =>
        {
            try
            {
                if (InMeeting) return;
                if (trap == null || trap.GameObject == null || !trap.isTriggered) //　解除された場合の処理
                {
                    if (!moveableFlag)
                    {
                        target.moveable = true;
                        moveableFlag = true;
                    }
                    return;
                }
                else if ((p == 1f) && !target.Data.IsDead)
                {
                    // 正常にキルが発生する場合の処理
                    target.moveable = true;
                    // 击杀由房主执行：设陷师本人可能是机器人/已掉线，缺少本地客户端时
                    // 旧逻辑（只有设陷师客户端才能触发）会导致受害者永远不死。
                    if (AmongUsClient.Instance.AmHost && trap.trapper != null)
                    {
                        var writer = StartRPC(CustomRPC.TrapperKill);
                        writer.Write(trap.trapper.PlayerId);
                        writer.Write(target.PlayerId);
                        writer.Write(trapId);
                        writer.EndRPC();
                        trapKill(trap.trapper, target, trapId);
                    }
                }
                else
                {
                    // カウントダウン中の処理
                    target.moveable = false;
                    target.NetTransform.Halt();
                    target.transform.position = trap.GameObject.transform.position + new Vector3(0, 0.3f, 0);
                }
            }
            catch (Exception e)
            {
                Error("An error occured during the countdown");
                Error(e.Message);
            }
        })));
    }

    public static void disableTrap(ushort trapId)
    {
        var trap = Find<KillTrap>(trapId);
        if (trap == null) return;
        trap.isTriggered = false;
        trap.isDisabled = true;
        trap.audioSource.Stop();
        trap.audioSource.maxDistance = EvilTrapper.killSoundRange;
        trap.audioSource.PlayOneShot(disable);
        _ = new LateTask(trap.Destroy, disable.length + 1f, "Destroy KillTrap");
    }

    public override void Update()
    {
        // 陷阱只对特定阵营可见：伪装者、已触发（受害者/赶来的救援者）与幽灵。
        // 用 Renderer.enabled 而不是材质透明度，避免材质/着色器差异导致“隐身”失效。
        var canSee = isTriggered || PlayerControl.LocalPlayer.IsImpostor() || CanSeeGhostInfo;
        if (Renderer != null && Renderer.enabled != canSee)
            Renderer.enabled = canSee;

        if (GameObject != null && isTriggered && !InMeeting)
        {
            var distance = Vector2.Distance(GameObject.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
            if (PlayerControl.LocalPlayer != target && PlayerControl.LocalPlayer.IsAlive() && distance < 0.75f)
            {
                var writer = StartRPC(CustomRPC.DisableTrap);
                writer.Write(NetworkId);
                writer.EndRPC();
                disableTrap(NetworkId);
            }
        }

        if (!hasTrappedPlayer() && !InMeeting)
        {
            if (!IsActive || isTriggered || PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.inVent || isDisabled || InMeeting) return;
            if (PlayerControl.LocalPlayer.IsImpostor() && !EvilTrapper.friendlyFire) return;

            var distance = Vector2.Distance(GameObject.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
            if (distance < EvilTrapper.trapRange)
            {
                target = PlayerControl.LocalPlayer;
                if (PlayerControl.LocalPlayer.AmOwner)
                {
                    var writer = StartRPC(CustomRPC.ActivateTrap);
                    writer.Write(trapper.PlayerId);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(NetworkId);
                    writer.EndRPC();
                    activateTrap(trapper, PlayerControl.LocalPlayer, NetworkId);
                }
            }
        }
    }

    public override void OnMeetingStart()
    {
        try
        {
            foreach (var trap in AllObjects.OfType<KillTrap>().ToArray())
            {
                if (trap.target.IsAlive() && PlayerControl.LocalPlayer == trap.target)
                {
                    trap.audioSource.Stop();
                    RpcCustomMurderPlayer(trap.trapper, trap.target, false);
                }
                else
                {
                    trap.Destroy();
                }
            }
        }
        catch (Exception e)
        {
            Error(e);
        }
    }

    public static bool hasTrappedPlayer()
    {
        foreach (var trap in AllObjects.OfType<KillTrap>().ToArray())
        {
            if (trap.target != null) return true;
        }
        return false;
    }

    public static bool isTrapped(PlayerControl p)
    {
        foreach (var trap in AllObjects.OfType<KillTrap>())
        {
            if (trap.target == p) return true;
        }
        return false;
    }

    public static void trapKill(PlayerControl trapper, PlayerControl target, ushort trapId)
    {
        var trap = Find<KillTrap>(trapId);
        if (trap == null) return;
        trap.IsKilled = true;
        Message($"邪恶陷阱击杀: 目标={target?.Data?.PlayerName ?? "null"} 设陷师={trapper?.Data?.PlayerName ?? "null"}", "EvilTrapper");

        var audioSource = trap.audioSource;
        audioSource.Stop();

        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = EvilTrapper.killSoundRange;
        audioSource.PlayOneShot(kill);
        CustomMurderPlayer(trapper, target, false);
        EvilTrapper.isTrapKill = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(kill.length + 0.1f, new Action<float>((p) =>
        {
            if (p == 1f)
            {
                Message("Destroy On trapKill");
                trap?.Destroy();
            }
        })));
    }
}