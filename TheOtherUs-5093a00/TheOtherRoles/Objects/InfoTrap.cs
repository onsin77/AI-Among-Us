namespace TheOtherRoles.Objects;

/// <summary>
/// 陷阱师的信息陷阱：玩家在范围内停留足够久即被记录一次（每轮每人一次）。
/// 使用主线的 NetworkCustomObject 房主权威系统生成与销毁，触发由独立 RPC 同步。
/// </summary>
public sealed class InfoTrap : NetworkCustomObject
{
    static InfoTrap()
    {
        Register<InfoTrap>((pos, payload) =>
        {
            var owner = PlayerById(payload[0]);
            if (owner == null) return null;
            TrapperPlus.NotifyTrapPlaced(owner.PlayerId);
            return new InfoTrap(owner, pos);
        });
    }

    private static Sprite trapSprite = new ResourceSprite("Trapper_Trap_Ingame.png", 300f);

    private readonly Dictionary<byte, float> dwellSeconds = new();
    private readonly HashSet<byte> completedTargets = new();
    private SpriteRenderer rangeRenderer;
    private static Sprite ringSprite;

    public PlayerControl Owner { get; }
    public byte OwnerId { get; }
    public float Radius { get; }

    public InfoTrap(PlayerControl owner, Vector3 position) : base()
    {
        Owner = owner;
        OwnerId = owner.PlayerId;

        var ship = ShipStatus.Instance;
        Radius = Math.Max(0.01f, TrapperPlus.trapSize * (ship != null ? ship.MaxLightRadius : 1f));

        GameObject.layer = 11;
        GameObject.name = $"InfoTrap {Id}";
        GameObject.transform.position = new Vector3(position.x, position.y, position.y + 0.001f);

        Renderer.sprite = trapSprite;
        Renderer.color = Color.white.SetAlpha(0.55f);
        ResizeMarker();
        rangeRenderer = CreateRangeRenderer();
        RefreshVisibility();
    }

    public static new InfoTrap Find(ushort networkId) => Find<InfoTrap>(networkId);

    public static void ClearAll(PlayerControl owner = null)
    {
        var traps = AllObjects.OfType<InfoTrap>()
            .Where(x => owner == null || x.OwnerId == owner.PlayerId)
            .ToArray();
        foreach (var trap in traps) trap?.Destroy();
    }

    /// <summary>按拥有者 id 清理（玩家对象可能已不存在，不能在此时退化为“清理全部”）。</summary>
    public static void ClearAllById(byte ownerId)
    {
        var traps = AllObjects.OfType<InfoTrap>().Where(x => x.OwnerId == ownerId).ToArray();
        foreach (var trap in traps) trap?.Destroy();
    }

    public void MarkTargetCompleted(byte targetId)
    {
        completedTargets.Add(targetId);
        dwellSeconds.Remove(targetId);
    }

    public void ResetForNextRound()
    {
        dwellSeconds.Clear();
        completedTargets.Clear();
    }

    public override void OnMeetingStart()
    {
        ResetForNextRound();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Update()
    {
        RefreshVisibility();

        // 只由房主进行停留判定，触发通过 RPC 广播
        if (!AmongUsClient.Instance.AmHost || !InGame || Helpers.InMeeting || ExileController.Instance != null
            || !TrapperPlus.IsActiveOwner(OwnerId))
        {
            dwellSeconds.Clear();
            return;
        }

        var playersInRange = new HashSet<byte>();
        var position = GameObject.transform.position;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Disconnected || player.Data.IsDead) continue;
            if (player.PlayerId == OwnerId || completedTargets.Contains(player.PlayerId)) continue;
            if (Vector2.Distance(position, player.GetTruePosition()) > Radius + 0.01f) continue;

            playersInRange.Add(player.PlayerId);
            dwellSeconds.TryGetValue(player.PlayerId, out var seconds);
            seconds += Time.deltaTime;
            dwellSeconds[player.PlayerId] = seconds;

            if (seconds < TrapperPlus.minDwellTime) continue;

            completedTargets.Add(player.PlayerId);
            dwellSeconds.Remove(player.PlayerId);
            TrapperPlus.ConfirmHostDwell(this, player);
        }

        foreach (var id in dwellSeconds.Keys.Where(id => !playersInRange.Contains(id)).ToArray())
            dwellSeconds.Remove(id);
    }

    private void RefreshVisibility()
    {
        if (GameObject == null)
        {
            IsActive = false;
            return;
        }

        var local = PlayerControl.LocalPlayer;
        var visible = local != null && (local.PlayerId == OwnerId || CanSeeGhostInfo);

        if (!GameObject.activeSelf) GameObject.SetActive(true);
        if (Renderer != null) Renderer.enabled = visible;
        if (rangeRenderer != null) rangeRenderer.enabled = visible;
        IsActive = visible;
    }

    private void ResizeMarker()
    {
        var sprite = Renderer.sprite;
        if (sprite == null) return;

        var size = sprite.bounds.size;
        var scaleX = size.x > 0f ? 0.35f / size.x : 1f;
        var scaleY = size.y > 0f ? 0.35f / size.y : scaleX;
        GameObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private SpriteRenderer CreateRangeRenderer()
    {
        var rangeObject = new GameObject("InfoTrap Range") { layer = 11 };
        rangeObject.transform.SetParent(GameObject.transform, false);

        // 父物体为贴合标记图被缩放过，这里反向补偿，使圆环世界直径恰好等于 2*Radius。
        var parentScale = GameObject.transform.localScale;
        var sx = parentScale.x > 0f ? parentScale.x : 1f;
        var sy = parentScale.y > 0f ? parentScale.y : 1f;
        rangeObject.transform.localScale = new Vector3(2f * Radius / sx, 2f * Radius / sy, 1f);
        rangeObject.transform.localPosition = new Vector3(0f, 0f, 0.01f);

        var renderer = rangeObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRingSprite();
        renderer.color = new Color(1f, 0.92f, 0.2f, 0.5f);
        renderer.sortingOrder = 100;
        return renderer;
    }

    /// <summary>
    /// 运行时生成黄色圆环贴图。原先用 LineRenderer + Shader.Find，IL2CPP 下着色器可能取不到，
    /// 圆环会完全不可见；SpriteRenderer 走精灵管线，稳定可见。
    /// </summary>
    private static Sprite GetRingSprite()
    {
        if (ringSprite != null) return ringSprite;

        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var center = (size - 1) * 0.5f;
        var outer = size * 0.5f - 1f;
        var inner = outer * 0.86f;
        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var inside = distance <= outer && distance >= inner;
                pixels[y * size + x] = new Color32(255, 235, 40, inside ? (byte)255 : (byte)0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return ringSprite;
    }
}
