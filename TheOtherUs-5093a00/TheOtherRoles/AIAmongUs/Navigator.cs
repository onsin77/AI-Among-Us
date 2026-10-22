using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AIAmongUs;

/// <summary>
/// 导航:全船 0.5 格距栅格 + 8 邻域 A*(防穿角)+ 特殊边(梯子/飞艇移动平台)+ BFS 可达域选 goal。
/// 栅格标定(三道工序,全部自动适配任意地图):
///   1) PointBlocked = OverlapCircle(0.25) 命中实体(trigger/门/移动平台本体忽略);
///   2) 边验证:相邻可走格间 Linecast,封死格中心圆探不到的薄墙渗漏(飞艇坑洞南墙实测漏点);
///   3) 锚点剪枝:以任务台/管道/门/出生中心/全体玩家为锚做连通分量,无锚分量(坑洞内部/装饰密室)封死。
/// 特殊边:梯子两端/平台缺口两端互连;GapRoom 平台轨道中段封虚拟墙。
/// 跟随 = PlayerPhysics.FixedUpdate Postfix 覆写速度;卡住 0.6s×3 → 软避让卡死格重路由(上限 3 次)。
/// 乘坐:到登乘格 1.5u 内自动 RpcClimbLadder / RpcUsePlatform,轮询到位(乘坐期 moveable=false,
///      CanMove 检查天然不与游戏协程抢控制)。
/// </summary>
internal static class Navigator
{

    // ===== 栅格寻路(确定性:点是否可站立由实体判定,不依赖 LOS 射线) =====
    private const float Cell = 0.25f;         // 格距:0.25 才能可靠看见 ≥0.5 宽的门洞(0.5 格距会漏窄门,实测断图)
    private static bool[] _walk;              // 可走格
    private static byte[] _edgeMask;          // 每格 8 方向位:1=该方向邻格通道被实体阻断
    private static byte[] _wallDist;          // 每格到最近不可走格的格距(0=自身贴墙);路径代价用,让路径走走廊中间
    private static int _gw, _gh;              // 栅格尺寸
    private static Vector2 _gridMin;          // 栅格世界原点(左下)
    private static string _gridMap = "";
    private static bool _gridReady;
    private const int AStarNodeCap = 60000;   // A* 扩展节点上限(0.25 栅格防极端耗时)
    private static readonly List<Collider2D> _ignoreColliders = new();
    // 8 邻域方向表:0=E 1=NE 2=N 3=NW 4=W 5=SW 6=S 7=SE(对边 = (i+4)%8,斜向为奇数)
    private static readonly int[] DirX = { 1, 1, 0, -1, -1, -1, 0, 1 };
    private static readonly int[] DirY = { 0, 1, 1, 1, 0, -1, -1, -1 };

    /// <summary>特殊边:梯子(两端各一个 Ladder 组件)或飞艇移动平台(缺口两侧登乘点)。</summary>
    private sealed class Link
    {
        public int CellA = -1, CellB = -1;
        public Vector2 PosA, PosB;
        public Ladder LadderAtA, LadderAtB;    // 梯子:两端各自的 Ladder(用于就近端 RpcClimbLadder)
        public MovingPlatformBehaviour Platform;
        public bool IsPlatform;
        public bool IsDoor;            // 门专用边:两侧最近可走格直连(不走乘坐),解决门框把门洞判死
        public float Cost = 1f;
    }
    private static readonly List<Link> _links = new();

    /// <summary>一条路径中的乘坐点:BoardIdx=登乘格在 _path 中的下标,FarPos=着陆点世界坐标。</summary>
    private sealed class Crossing
    {
        public int BoardIdx;
        public Vector2 FarPos;
        public Link Lk;
        public float Timer;
        public int RpcCount;
        public Vector2 StartPos;
    }
    private static readonly List<Crossing> _pathCrossings = new(); // 当前路径的全部乘坐点
    private static Crossing _riding;                               // 正在乘坐的特殊边

    private static float _rerouteCount;
    private static readonly HashSet<int> _avoid = new();  // 本次移动内软避让的格(卡死/乘坐失败点)

    // 跟随状态
    private static bool _active;
    private static Vector2[] _path = Array.Empty<Vector2>();
    private static int _pathIdx;
    private static Vector2 _destPos;
    private static string _destLabel = "";
    private static float _arriveTol = 0.6f;
    private static float _stuckTimer;
    private static Vector2 _lastPos;
    private static float _lastDistDest;
    private static int _stuckCount;
    private static string _lastError;
    private static long _moveSeq; // 每次 move_to 递增,ack 与事件关联
    private static Vector2? _lastNudge;

    public static bool Active => _active;
    public static string Destination => _destLabel;
    /// <summary>上一次移动的终态:MOVE_DONE / MOVE_FAILED / MOVE_ABORT / cmd_stop / task_flow_end。</summary>
    public static string LastMoveResult { get; private set; }

    // ---------------- 图生成(栅格) ----------------

    private static float _lastBuildTime;
    private const float RebuildInterval = 25f;

    public static void EnsureGraph()
    {
        var ship = ShipStatus.Instance;
        if (ship == null || !AIHub.IsPlayPhaseNow) { _gridReady = false; return; }
        var mapName = StateCollector.SafeMapNameRaw();
        var mapChanged = _gridMap != mapName;
        var due = !_active && Time.realtimeSinceStartup - _lastBuildTime > RebuildInterval;
        if (_gridReady && !mapChanged && !due) return;
        BuildGrid(ship);
        _lastBuildTime = Time.realtimeSinceStartup;
        _gridMap = mapName;
        _gridReady = true;
    }

    private static void BuildGrid(ShipStatus ship)
    {
        _ignoreColliders.Clear();
        _links.Clear();

        // 门体 collider(含子物体:门框/感应件):可通行,不算阻挡
        try
        {
            var doors0 = ship.AllDoors;
            for (var i = 0; i < doors0.Length; i++)
            {
                var d = doors0[i];
                if (d == null) continue;
                var cols = ((Component)d).GetComponentsInChildren<Collider2D>(true);
                for (var j = 0; j < cols.Length; j++)
                    if (cols[j] != null) _ignoreColliders.Add(cols[j]);
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"door colliders: {e.Message}"); }

        // 任务台/交互台 collider:装饰物不是墙(否则台周 0.5u 内全判阻挡,人永远站不进 usableDistance)
        try
        {
            var consoles0 = ship.AllConsoles;
            for (var i = 0; i < consoles0.Length; i++)
            {
                var cc = consoles0[i];
                if (cc == null) continue;
                var cols = ((Component)cc).GetComponentsInChildren<Collider2D>(true);
                for (var j = 0; j < cols.Length; j++)
                    if (cols[j] != null) _ignoreColliders.Add(cols[j]);
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"console colliders: {e.Message}"); }

        // 移动平台本体 collider(含子物体):平台是乘具不是墙(当前位置挡住的格应可走)
        MovingPlatformBehaviour gapPlatform = null;
        try
        {
            var airship = ship.TryCast<AirshipStatus>();
            gapPlatform = airship != null ? airship.GapPlatform : null;
            if (gapPlatform != null)
            {
                var cols = ((Component)gapPlatform).GetComponentsInChildren<Collider2D>(true);
                for (var j = 0; j < cols.Length; j++)
                    if (cols[j] != null) _ignoreColliders.Add(cols[j]);
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"platform collider: {e.Message}"); }

        // 全船 bounds(房间 bounds 并包络)
        var rooms = ship.AllRooms;
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        for (var i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];
            if (r?.roomArea == null) continue;
            var b = r.roomArea.bounds;
            min = Vector2.Min(min, new Vector2(b.min.x, b.min.y));
            max = Vector2.Max(max, new Vector2(b.max.x, b.max.y));
        }
        _gridMin = min;
        _gw = Mathf.CeilToInt((max.x - min.x) / Cell) + 1;
        _gh = Mathf.CeilToInt((max.y - min.y) / Cell) + 1;
        _walk = new bool[_gw * _gh];

        var pr = PlayerRadius();
        _probeRadius = pr;
        var kept = 0;
        for (var gy = 0; gy < _gh; gy++)
        {
            for (var gx = 0; gx < _gw; gx++)
            {
                var p = GridToWorld(gx, gy);
                if (!PointBlocked(p, pr)) { _walk[gy * _gw + gx] = true; kept++; }
            }
        }
        AIBridgePlugin.Log?.LogInfo($"nav grid: player_radius={pr:F3}");

        // ---- 特殊端点登记(先封平台轨道,再强制端点格可走) ----
        var ladderLinks = 0;
        var platformLink = false;
        var ladderCells = new List<int>();
        try
        {
            var seen = new HashSet<byte>();
            var ladders = ship.Ladders;
            for (var i = 0; i < ladders.Length; i++)
            {
                var l = ladders[i];
                if (l == null || l.Destination == null) continue;
                if (seen.Contains(l.Id) || seen.Contains(l.Destination.Id)) continue;
                seen.Add(l.Id); seen.Add(l.Destination.Id);
                var pa = (Vector2)((Component)l).transform.position;
                var pb = (Vector2)((Component)l.Destination).transform.position;
                var lk = new Link { PosA = pa, PosB = pb, LadderAtA = l, LadderAtB = l.Destination, Cost = 1f };
                lk.CellA = WorldToCell(pa); if (lk.CellA >= 0) { _walk[lk.CellA] = true; ladderCells.Add(lk.CellA); }
                lk.CellB = WorldToCell(pb); if (lk.CellB >= 0) { _walk[lk.CellB] = true; ladderCells.Add(lk.CellB); }
                _links.Add(lk);
                ladderLinks++;
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"ladders: {e.Message}"); }

        try
        {
            if (gapPlatform != null)
            {
                var parent = ((Component)gapPlatform).transform.parent;
                var pa = (Vector2)parent.TransformPoint(gapPlatform.LeftPosition);
                var pb = (Vector2)parent.TransformPoint(gapPlatform.RightPosition);
                var blocked = BlockCorridor(pa, pb);
                var lk = new Link { PosA = pa, PosB = pb, Platform = gapPlatform, IsPlatform = true, Cost = 8f };
                lk.CellA = WorldToCell(pa); if (lk.CellA >= 0) _walk[lk.CellA] = true;
                lk.CellB = WorldToCell(pb); if (lk.CellB >= 0) _walk[lk.CellB] = true;
                _links.Add(lk);
                platformLink = true;
                AIBridgePlugin.Log?.LogInfo(
                    $"nav platform: ends {pa.x:F1},{pa.y:F1} <-> {pb.x:F1},{pb.y:F1} corridor_blocked={blocked}");
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"platform: {e.Message}"); }

        var playerBlocked = 0; // 玩家障碍改由转向层动态处理(假人会走,静态标格反而堵路)

        // ---- 工序1b:门洞强制(门位置=真实通道,门周 0.5u 圆盘内格子必可走;
        //      否则窄门洞格心距门框 < 玩家半径会被误封,实测把东西两半图断开) ----
        var doorsForced = 0;
        var doorCells = new HashSet<int>();
        try
        {
            var doors1 = ship.AllDoors;
            for (var i = 0; i < doors1.Length; i++)
            {
                var dr = doors1[i];
                if (dr == null) continue;
                var dp = (Vector2)((Component)dr).transform.position;
                var cgx = Mathf.RoundToInt((dp.x - _gridMin.x) / Cell);
                var cgy = Mathf.RoundToInt((dp.y - _gridMin.y) / Cell);
                var rad = Mathf.CeilToInt(0.5f / Cell);
                for (var dy = -rad; dy <= rad; dy++)
                {
                    for (var dx = -rad; dx <= rad; dx++)
                    {
                        if (dx * dx + dy * dy > rad * rad) continue;
                        var nx = cgx + dx; var ny = cgy + dy;
                        if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                        var c = ny * _gw + nx;
                        doorCells.Add(c);
                        if (!_walk[c]) { _walk[c] = true; doorsForced++; }
                    }
                }
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"door force: {e.Message}"); }

        // 形态学开运算已撤销:0.25 格距下它会吞掉真实 0.5u 走廊(实测 reach 29%)
        var morphCut = 0;

        // ---- 工序2:边验证(相邻可走格间胶囊扫掠,封实体阻断) ----
        _edgeMask = new byte[_walk.Length];
        var edgeBlocked = 0;
        for (var gy = 0; gy < _gh; gy++)
        {
            for (var gx = 0; gx < _gw; gx++)
            {
                var c = gy * _gw + gx;
                if (!_walk[c]) continue;
                for (var dir = 0; dir < 4; dir++) // E/NE/N/NW,另一半由对边镜像写入
                {
                    var nx = gx + DirX[dir]; var ny = gy + DirY[dir];
                    if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                    var nb = ny * _gw + nx;
                    if (!_walk[nb]) continue;
                    if (SegmentBlocked(GridToWorld(gx, gy), GridToWorld(nx, ny)))
                    {
                        _edgeMask[c] |= (byte)(1 << dir);
                        _edgeMask[nb] |= (byte)(1 << ((dir + 4) % 8));
                        edgeBlocked++;
                    }
                }
            }
        }

        // ---- 工序3:锚点连通分量剪枝(无锚分量=坑洞内部/装饰密室/家具陷阱,封死) ----
        var pruned = 0;
        try { pruned = PruneUnanchored(ship); }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"prune: {e.Message}"); }

        var roomPruned = 0; // 房间多边形剪枝已撤销:真实地板一半不在任何 roomArea 多边形内(实测 room_pruned=15985 误杀)

        // ---- 工序1d:门直连边(门框会让胶囊边测试封死真实门洞,实测整片房间变孤岛) ----
        // 做法:门周 1.4u 内收集可走格,取"分居门两侧 + 胶囊通畅 + 距离最近"的一对直连。
        // 不依赖门法线采样(门洞相对门中心常有偏移)。
        var doorLinks = 0;
        try
        {
            var doors2 = ship.AllDoors;
            var seenPairs = new HashSet<long>();
            var RAD = Mathf.CeilToInt(1.4f / Cell);
            for (var i = 0; i < doors2.Length; i++)
            {
                var dr = doors2[i];
                if (dr == null) continue;
                var dp = (Vector2)((Component)dr).transform.position;
                Collider2D dcol = null;
                try
                {
                    var pd = dr.TryCast<PlainDoor>();
                    dcol = pd != null ? (Collider2D)pd.myCollider : ((Component)dr).GetComponent<Collider2D>();
                }
                catch { }
                var normal = dcol != null ? (Vector2)dcol.transform.up : Vector2.up;
                if (normal.sqrMagnitude < 0.01f) normal = Vector2.up;
                normal.Normalize();

                var gx0 = Mathf.RoundToInt((dp.x - _gridMin.x) / Cell);
                var gy0 = Mathf.RoundToInt((dp.y - _gridMin.y) / Cell);
                var sides = new List<int>[2] { new(), new() };
                for (var dy = -RAD; dy <= RAD; dy++)
                {
                    for (var dx = -RAD; dx <= RAD; dx++)
                    {
                        var nx = gx0 + dx; var ny = gy0 + dy;
                        if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                        var c = ny * _gw + nx;
                        if (!_walk[c]) continue;
                        var wp = GridToWorld(nx, ny);
                        if (Vector2.Distance(wp, dp) > 1.45f) continue;
                        var side = Vector2.Dot(wp - dp, normal) >= 0f ? 0 : 1;
                        sides[side].Add(c);
                    }
                }
                int bestA = -1, bestB = -1; var bestD = float.MaxValue;
                for (var a = 0; a < sides[0].Count; a++)
                {
                    for (var b = 0; b < sides[1].Count; b++)
                    {
                        var ca = sides[0][a]; var cb = sides[1][b];
                        var pa = GridToWorld(ca % _gw, ca / _gw);
                        var pb = GridToWorld(cb % _gw, cb / _gw);
                        var d = Vector2.Distance(pa, pb);
                        if (d < 0.5f || d > 1.7f || d >= bestD) continue;
                        if (DoorLineBlocked(pa, pb)) continue;
                        bestA = ca; bestB = cb; bestD = d;
                    }
                }
                if (bestA < 0 || bestB < 0) continue;
                var lo = Mathf.Min(bestA, bestB); var hi = Mathf.Max(bestA, bestB);
                var key = (long)lo * 100000L + hi;
                if (!seenPairs.Add(key)) continue;
                _links.Add(new Link
                {
                    CellA = bestA, CellB = bestB,
                    PosA = GridToWorld(bestA % _gw, bestA / _gw),
                    PosB = GridToWorld(bestB % _gw, bestB / _gw),
                    IsDoor = true, Cost = Mathf.Max(0.3f, bestD)
                });
                doorLinks++;
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"door links: {e.Message}"); }

        // 剪枝后重新强制登乘格(可能被 hole 包围而误伤)
        foreach (var c in ladderCells) _walk[c] = true;
        foreach (var lk in _links)
        {
            if (lk.CellA >= 0) _walk[lk.CellA] = true;
            if (lk.CellB >= 0) _walk[lk.CellB] = true;
        }

        // ---- 工序4:离墙距离场(多源 BFS;A* 代价惩罚用,让路径避开贴墙根/家具边) ----
        BuildWallDist();

        // 连通性自检:从第一个可走格 BFS,统计可达比例(特殊边也算)
        var reachPct = -1;
        try
        {
            int anchor = -1, total = 0;
            for (var i = 0; i < _walk.Length; i++) { if (_walk[i]) { total++; if (anchor < 0) anchor = i; } }
            if (anchor >= 0)
            {
                var reach = FloodReach(anchor);
                var got = 0; for (var i = 0; i < reach.Length; i++) if (reach[i]) got++;
                reachPct = total > 0 ? got * 100 / total : 0;
            }
        }
        catch { }

        AIBridgePlugin.Log?.LogInfo(
            $"nav grid built: map={StateCollector.SafeMapNameRaw()} size={_gw}x{_gh} walkable={kept} " +
            $"edges_blocked={edgeBlocked} pruned={pruned} doors_forced={doorsForced} door_links={doorLinks} " +
            $"ladders={ladderLinks} platform={platformLink} reach={reachPct}%");
        BridgeServer.EnqueueReliable(
            "{\"type\":\"nav_stats\",\"grid\":true,\"cells\":" + (_gw * _gh) +
            ",\"walkable\":" + kept + ",\"edges_blocked\":" + edgeBlocked + ",\"pruned\":" + pruned +
            ",\"doors_forced\":" + doorsForced +
            ",\"ladders\":" + ladderLinks + ",\"platform\":" + MiniJson.B(platformLink) +
            ",\"reach_pct\":" + reachPct + "}");
    }

    /// <summary>锚点 = 真实可站立触点。返回封掉的格数。</summary>
    private static int PruneUnanchored(ShipStatus ship)
    {
        var anchors = new HashSet<int>();
        void AddAnchor(Vector2 p)
        {
            var c = NearestWalkable(p);
            if (c >= 0) anchors.Add(c);
        }
        try
        {
            var consoles = ship.AllConsoles;
            for (var i = 0; i < consoles.Length; i++)
                if (consoles[i] != null) AddAnchor(((Component)consoles[i]).transform.position);
        }
        catch { }
        try
        {
            var vents = ship.AllVents;
            for (var i = 0; i < vents.Length; i++)
                if (vents[i] != null) AddAnchor(((Component)vents[i]).transform.position);
        }
        catch { }
        try
        {
            var doors = ship.AllDoors;
            for (var i = 0; i < doors.Length; i++)
                if (doors[i] != null) AddAnchor(((Component)doors[i]).transform.position);
        }
        catch { }
        AddAnchor(ship.InitialSpawnCenter);
        try
        {
            var all = PlayerControl.AllPlayerControls;
            for (var i = 0; i < all.Count; i++)
                if (all[i] != null && all[i].Data != null) AddAnchor(all[i].GetTruePosition());
        }
        catch { }

        // 连通分量标记
        var comp = new int[_walk.Length];
        var nextId = 0;
        for (var start = 0; start < _walk.Length; start++)
        {
            if (!_walk[start] || comp[start] != 0) continue;
            nextId++;
            var q = new List<int> { start };
            comp[start] = nextId;
            while (q.Count > 0)
            {
                var cur = q[q.Count - 1]; q.RemoveAt(q.Count - 1);
                var cx = cur % _gw; var cy = cur / _gw;
                for (var dir = 0; dir < 8; dir++)
                {
                    var nx = cx + DirX[dir]; var ny = cy + DirY[dir];
                    if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                    var nb = ny * _gw + nx;
                    if (comp[nb] != 0 || !_walk[nb] || EdgeBlocked(cur, dir)) continue;
                    if ((dir & 1) == 1 && (!_walk[cy * _gw + nx] || !_walk[ny * _gw + cx])) continue;
                    comp[nb] = nextId;
                    q.Add(nb);
                }
            }
        }
        var keep = new HashSet<int>();
        foreach (var a in anchors) keep.Add(comp[a]);
        var pruned = 0;
        for (var i = 0; i < _walk.Length; i++)
        {
            if (!_walk[i]) continue;
            if (!keep.Contains(comp[i])) { _walk[i] = false; pruned++; }
        }
        return pruned;
    }

    /// <summary>多源 BFS 距离场:每格到最近不可走格的格距。贴墙(≤1)的格 A* 加重罚,
    /// 防路径贴家具边/墙根走——物理滑行在凹角会卡死(Engine/Armory/Records 实测)。</summary>
    private static void BuildWallDist()
    {
        _wallDist = new byte[_walk.Length];
        var q = new List<int>();
        for (var i = 0; i < _walk.Length; i++)
            if (!_walk[i]) q.Add(i);
        // 多源 BFS(经所有格,含不可走格自身作源)
        var head = 0;
        while (head < q.Count)
        {
            var cur = q[head++];
            var d = _wallDist[cur];
            if (d >= 255) continue;
            var cx = cur % _gw; var cy = cur / _gw;
            for (var dir = 0; dir < 8; dir++)
            {
                var nx = cx + DirX[dir]; var ny = cy + DirY[dir];
                if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                var nb = ny * _gw + nx;
                if (_wallDist[nb] != 0 || !_walk[nb]) continue;
                _wallDist[nb] = (byte)System.Math.Min(255, d + 1);
                q.Add(nb);
            }
        }
    }

    /// <summary>格的离墙惩罚系数(1=无惩罚;贴墙最重 3×)。</summary>
    private static float WallPenalty(int cell)
    {
        if (_wallDist == null) return 1f;
        var d = _wallDist[cell];
        if (d >= 4) return 1f;
        return 1f + (4 - d) * 0.5f;   // d=0(贴墙)→3.0,d=3→1.5
    }

    /// <summary>封住 a→b 轨道走廊的中间段(两端登乘区保留)。返回封掉的格数。</summary>
    private static int BlockCorridor(Vector2 a, Vector2 b)
    {
        var delta = b - a;
        var len = delta.magnitude;
        if (len < 1f) return 0;
        var dir = delta / len;
        var n = new Vector2(-dir.y, dir.x);
        const float halfW = 1.1f;                 // 平台宽约 2.2,半宽+冗余
        var steps = Mathf.CeilToInt(len / (Cell * 0.5f));
        var across = Mathf.CeilToInt(halfW / (Cell * 0.5f));
        var blocked = 0;
        for (var s = 1; s < steps; s++)
        {
            var t = (float)s / steps;
            if (t < 0.12f || t > 0.88f) continue; // 两端登乘格保留
            var c0 = a + dir * (len * t);
            for (var k = -across; k <= across; k++)
            {
                var ci = WorldToCell(c0 + n * (k * Cell * 0.5f));
                if (ci >= 0 && _walk[ci]) { _walk[ci] = false; blocked++; }
            }
        }
        return blocked;
    }

    private static Vector2 GridToWorld(int gx, int gy) => new(_gridMin.x + gx * Cell, _gridMin.y + gy * Cell);

    private static int WorldToCell(Vector2 p)
    {
        var gx = Mathf.RoundToInt((p.x - _gridMin.x) / Cell);
        var gy = Mathf.RoundToInt((p.y - _gridMin.y) / Cell);
        if (gx < 0 || gy < 0 || gx >= _gw || gy >= _gh) return -1;
        return gy * _gw + gx;
    }

    private static bool EdgeBlocked(int cell, int dir) => (_edgeMask[cell] & (1 << dir)) != 0;

    /// <summary>玩家真实站立半径(建图时读本地玩家 CircleCollider;探不到时退 0.2)。
    /// 曾用 0.25:比真实半径大,把能走人的窄门封死(Engine 东出口实测)。</summary>
    private static float PlayerRadius()
    {
        try
        {
            var col = PlayerControl.LocalPlayer?.Collider;
            var circle = col != null ? col.TryCast<CircleCollider2D>() : null;
            if (circle != null && circle.radius > 0.05f)
            {
                var s = Mathf.Abs(((Component)circle).transform.localScale.x);
                return Mathf.Clamp(circle.radius * s * 0.75f, 0.15f, 0.3f);
            }
        }
        catch { }
        return 0.15f;
    }

    /// <summary>collider 是否可忽略(trigger/门/移动平台本体)。</summary>
    private static bool Ignorable(Collider2D col)
    {
        if (col == null) return true;
        try { if (col.isTrigger) return true; } catch { }
        for (var j = 0; j < _ignoreColliders.Count; j++)
            if (_ignoreColliders[j] == col) return true;
        return false;
    }

    /// <summary>点是否落在实体碰撞体内(墙/家具;半径=玩家真实半径)。trigger 与门/平台忽略。</summary>
    private static bool PointBlocked(Vector2 p, float radius)
    {
        try
        {
            var cols = Physics2D.OverlapCircleAll(p, radius, Constants.ShipAndObjectsMask);
            for (var i = 0; i < cols.Length; i++)
                if (!Ignorable(cols[i])) return true;
            return false;
        }
        catch { return true; }
    }

    private static float _probeRadius = 0.15f; // 建图时=玩家真实半径×0.75

    /// <summary>门洞直线是否被实体阻断(Linecast;门/台/平台 collider 已忽略)。
    /// 门边专用:门洞中心线通畅即可通行,不受门框侧壁的胶囊误杀。</summary>
    private static bool DoorLineBlocked(Vector2 a, Vector2 b)
    {
        try
        {
            var hits = Physics2D.LinecastAll(a, b, Constants.ShipAndObjectsMask);
            for (var i = 0; i < hits.Length; i++)
                if (!Ignorable(hits[i].collider)) return true;
            return false;
        }
        catch { return true; }
    }

    /// <summary>两格中心间玩家胶囊扫掠是否被实体阻断(CircleCast,半径=玩家半径)。
    /// 线段 Linecast 测不出 1 格宽伪通道:格心各自离墙 ≥ 半径但 0.4 宽的胶囊过不去(实测两类卡死根因)。</summary>
    private static bool SegmentBlocked(Vector2 a, Vector2 b)
    {
        try
        {
            var delta = b - a;
            var dist = delta.magnitude;
            if (dist < 0.001f) return false;
            var hits = Physics2D.CircleCastAll(a, _probeRadius, delta / dist, dist, Constants.ShipAndObjectsMask);
            for (var i = 0; i < hits.Length; i++)
                if (!Ignorable(hits[i].collider)) return true;
            return false;
        }
        catch { return true; }
    }

    /// <summary>诊断:两点 LOS 探测(nav_probe 指令保留)。</summary>
    public static string Probe(Vector2 a, Vector2 b)
    {
        var sb = new StringBuilder(256);
        var delta = b - a;
        var dist = delta.magnitude;
        sb.Append("{\"dist\":").Append(MiniJson.F(dist, 2));
        try
        {
            var dir = delta / Mathf.Max(dist, 0.01f);
            var hits = Physics2D.RaycastAll(a, dir, Mathf.Max(dist - 0.05f, 0.01f), Constants.ShipAndObjectsMask);
            sb.Append(",\"hits\":[");
            var any = false;
            for (var i = 0; i < hits.Length; i++)
            {
                var col = hits[i].collider;
                if (col == null) continue;
                bool isTrigger = false;
                try { isTrigger = col.isTrigger; } catch { }
                var ignored = false;
                for (var j = 0; j < _ignoreColliders.Count; j++)
                    if (_ignoreColliders[j] == col) { ignored = true; break; }
                string nm = "?";
                try { nm = ((Component)col).gameObject.name; } catch { }
                sb.Append("{\"name\":").Append(MiniJson.Str(nm))
                  .Append(",\"trigger\":").Append(MiniJson.B(isTrigger))
                  .Append(",\"door\":").Append(MiniJson.B(ignored))
                  .Append(",\"at\":[")
                  .Append(MiniJson.F(hits[i].point.x, 1)).Append(',').Append(MiniJson.F(hits[i].point.y, 1))
                  .Append("]},");
                any = true;
            }
            if (any) sb.Length--;
            sb.Append("]}");
        }
        catch (Exception e)
        {
            sb.Append(",\"err\":").Append(MiniJson.Str(e.Message)).Append('}');
        }
        return sb.ToString();
    }

    // ---------------- 寻路(栅格 A*) ----------------

    /// <summary>找 p 附近最近的可走格(螺旋搜索,半径 16 格=4u)。</summary>
    private static int NearestWalkable(Vector2 p)
    {
        var gx0 = Mathf.RoundToInt((p.x - _gridMin.x) / Cell);
        var gy0 = Mathf.RoundToInt((p.y - _gridMin.y) / Cell);
        if (gx0 >= 0 && gy0 >= 0 && gx0 < _gw && gy0 < _gh && _walk[gy0 * _gw + gx0]) return gy0 * _gw + gx0;
        for (var r = 1; r <= 16; r++)
        {
            for (var dy = -r; dy <= r; dy++)
            {
                for (var dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                    var gx = gx0 + dx; var gy = gy0 + dy;
                    if (gx < 0 || gy < 0 || gx >= _gw || gy >= _gh) continue;
                    if (_walk[gy * _gw + gx]) return gy * _gw + gx;
                }
            }
        }
        return -1;
    }

    /// <summary>BFS 可达域(8 邻域防穿角+边验证+特殊边;不计软避让)。
    /// 软避让只作为 A* 附加代价,不参与可达性判定(否则避让块会封死窄出口,连坐 goal 选择)。</summary>
    private static bool[] FloodReach(int start)
    {
        var reach = new bool[_walk.Length];
        var q = new List<int> { start };
        reach[start] = true;
        while (q.Count > 0)
        {
            var cur = q[q.Count - 1];
            q.RemoveAt(q.Count - 1);
            ExpandBfs(cur, reach, q);
        }
        return reach;
    }

    private static void ExpandBfs(int cur, bool[] reach, List<int> q)
    {
        var cx = cur % _gw; var cy = cur / _gw;
        for (var dir = 0; dir < 8; dir++)
        {
            var nx = cx + DirX[dir]; var ny = cy + DirY[dir];
            if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
            var nb = ny * _gw + nx;
            if (reach[nb] || !_walk[nb] || EdgeBlocked(cur, dir)) continue;
            if ((dir & 1) == 1 && (!_walk[cy * _gw + nx] || !_walk[ny * _gw + cx])) continue;
            reach[nb] = true;
            q.Add(nb);
        }
        for (var li = 0; li < _links.Count; li++)
        {
            var lk = _links[li];
            int other;
            if (lk.CellA == cur) other = lk.CellB;
            else if (lk.CellB == cur) other = lk.CellA;
            else continue;
            if (other < 0 || reach[other] || !_walk[other]) continue;
            reach[other] = true;
            q.Add(other);
        }
    }

    /// <summary>在可达域内找 p 欧氏最近的格(半径 30 格内取距离最小者)。
    /// 注意不能用螺旋环扫描序的第一个——目标点常落在墙体里,
    /// 扫描序会把 goal 选中远处的格(实测把路径引去移动平台)。</summary>
    private static int NearestInReach(Vector2 p, bool[] reach)
    {
        var gx0 = Mathf.RoundToInt((p.x - _gridMin.x) / Cell);
        var gy0 = Mathf.RoundToInt((p.y - _gridMin.y) / Cell);
        var best = -1;
        float bestD = float.MaxValue;
        const int R = 60;   // 0.25 格距 → 15u 半径
        for (var dy = -R; dy <= R; dy++)
        {
            for (var dx = -R; dx <= R; dx++)
            {
                var gx = gx0 + dx; var gy = gy0 + dy;
                if (gx < 0 || gy < 0 || gx >= _gw || gy >= _gh) continue;
                var c = gy * _gw + gx;
                if (!reach[c]) continue;
                var d = dx * dx + dy * dy;
                if (d < bestD) { bestD = d; best = c; }
            }
        }
        return best;
    }

    /// <summary>roomArea.OverlapPoint 为真 ∧ 可走 ∧ 从 fromPos 可达 的格中,距房间 bounds.center 欧氏最近者。
    /// 修复:bounds.center 常落在墙体/房外(Kitchen/Brig 实测),导致 goal 选到缺口对面引发卡死。</summary>
    public static Vector2? RoomGoal(SystemTypes room, Vector2 fromPos)
    {
        EnsureGraph();
        if (!_gridReady) return null;
        var ship = ShipStatus.Instance;
        if (ship?.FastRooms == null || !ship.FastRooms.ContainsKey(room)) return null;
        var r = ship.FastRooms[room];
        if (r?.roomArea == null) return null;

        var start = NearestWalkable(fromPos);
        if (start < 0) return null;
        var reach = FloodReach(start);

        var b = r.roomArea.bounds;
        var center = b.center;
        int gx0 = Mathf.Max(0, Mathf.FloorToInt((b.min.x - _gridMin.x) / Cell) - 1);
        int gy0 = Mathf.Max(0, Mathf.FloorToInt((b.min.y - _gridMin.y) / Cell) - 1);
        int gx1 = Mathf.Min(_gw - 1, Mathf.CeilToInt((b.max.x - _gridMin.x) / Cell) + 1);
        int gy1 = Mathf.Min(_gh - 1, Mathf.CeilToInt((b.max.y - _gridMin.y) / Cell) + 1);
        var best = -1;
        float bestD = float.MaxValue;
        for (var gy = gy0; gy <= gy1; gy++)
        {
            for (var gx = gx0; gx <= gx1; gx++)
            {
                var c = gy * _gw + gx;
                if (!_walk[c] || !reach[c]) continue;
                var p = GridToWorld(gx, gy);
                if (!r.roomArea.OverlapPoint(p)) continue;
                var dx = p.x - center.x; var dy = p.y - center.y;
                var d = dx * dx + dy * dy;
                if (d < bestD) { bestD = d; best = c; }
            }
        }
        if (best < 0) return null;
        return GridToWorld(best % _gw, best / _gw);
    }

    /// <summary>console/交互点接近目标:可达 ∧ 距目标 ≤ useRange 的格中取距目标最近者(等距取离玩家近)。
    /// 比纯"最近可达格"强:任务台常嵌在大件家具里(MeetingRoom 会议桌实测),
    /// 最近可达格可能距 console 2.9u > Use 半径,走到了也开不了面板。无解则退化为最近可达格。</summary>
    public static Vector2? ConsoleApproach(Vector2 target, Vector2 fromPos, float useRange)
    {
        EnsureGraph();
        if (!_gridReady) return null;
        var start = NearestWalkable(fromPos);
        if (start < 0) return null;
        var reach = FloodReach(start);
        const int R = 48;   // 0.25 格距 → 12u 半径
        var gx0 = Mathf.RoundToInt((target.x - _gridMin.x) / Cell);
        var gy0 = Mathf.RoundToInt((target.y - _gridMin.y) / Cell);
        var best = -1;
        float bestDt = float.MaxValue, bestDp = float.MaxValue;
        for (var dy = -R; dy <= R; dy++)
        {
            for (var dx = -R; dx <= R; dx++)
            {
                var gx = gx0 + dx; var gy = gy0 + dy;
                if (gx < 0 || gy < 0 || gx >= _gw || gy >= _gh) continue;
                var c = gy * _gw + gx;
                if (!_walk[c] || !reach[c]) continue;
                var p = GridToWorld(gx, gy);
                var dt = Vector2.Distance(p, target);
                if (dt > useRange) continue;
                var dp = Vector2.Distance(p, fromPos);
                if (dt < bestDt - 0.05f || (dt < bestDt + 0.05f && dp < bestDp))
                {
                    bestDt = dt; bestDp = dp; best = c;
                }
            }
        }
        if (best < 0) return null;
        return GridToWorld(best % _gw, best / _gw);
    }

    /// <summary>raw 路径上相邻两格是否经特殊边连接(非 8 邻接即特殊)。</summary>
    private static Link LinkBetween(int u, int v)
    {
        for (var i = 0; i < _links.Count; i++)
        {
            var lk = _links[i];
            if ((lk.CellA == u && lk.CellB == v) || (lk.CellB == u && lk.CellA == v)) return lk;
        }
        return null;
    }

    /// <summary>诊断:规划 from→to 的路径但不执行(nav_path 指令)。</summary>
    public static string PlanJson(Vector2 from, Vector2 to)
    {
        EnsureGraph();
        var sb = new StringBuilder(2048);
        sb.Append("{\"type\":\"nav_path\",\"from\":[").Append(MiniJson.F(from.x, 1)).Append(',').Append(MiniJson.F(from.y, 1))
          .Append("],\"to\":[").Append(MiniJson.F(to.x, 1)).Append(',').Append(MiniJson.F(to.y, 1)).Append(']');
        if (!_gridReady) { sb.Append(",\"err\":\"no_graph\"}"); return sb.ToString(); }
        var start = NearestWalkable(from);
        sb.Append(",\"start_cell\":").Append(start);
        var reach = start >= 0 ? FloodReach(start) : null;
        var goal = reach != null ? NearestInReach(to, reach) : -1;
        sb.Append(",\"goal_cell\":").Append(goal);
        if (goal >= 0)
            sb.Append(",\"goal_pos\":[").Append(MiniJson.F(GridToWorld(goal % _gw, goal / _gw).x, 1))
              .Append(',').Append(MiniJson.F(GridToWorld(goal % _gw, goal / _gw).y, 1)).Append(']');
        var crossings = new List<Crossing>();
        var path = FindPath(from, to, crossings);
        sb.Append(",\"ok\":").Append(MiniJson.B(path != null));
        if (path == null) { sb.Append('}'); return sb.ToString(); }
        sb.Append(",\"wp\":[");
        for (var i = 0; i < path.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(MiniJson.F(path[i].x, 1)).Append(',').Append(MiniJson.F(path[i].y, 1));
        }
        sb.Append("],\"crossings\":[");
        for (var i = 0; i < crossings.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var c = crossings[i];
            sb.Append("{\"board_idx\":").Append(c.BoardIdx)
              .Append(",\"kind\":").Append(MiniJson.Str(c.Lk.IsPlatform ? "platform" : "ladder"))
              .Append(",\"far\":[").Append(MiniJson.F(c.FarPos.x, 1)).Append(',').Append(MiniJson.F(c.FarPos.y, 1)).Append("]}");
        }
        sb.Append("]}");
        return sb.ToString();
    }

    /// <summary>栅格 A*(8 邻域,斜向需两直邻可走防穿角;特殊边=梯子/平台)。返回世界坐标路径(抽稀后),
    /// crossingsOut 收录乘坐点(登乘格下标+着陆点),供跟路自动触发。</summary>
    private static List<Vector2> FindPath(Vector2 from, Vector2 to, List<Crossing> crossingsOut)
    {
        crossingsOut?.Clear();
        if (_walk == null || _walk.Length == 0) return null;
        var start = NearestWalkable(from);
        if (start < 0)
        {
            AIBridgePlugin.Log?.LogWarning($"no_path: start unreachable (grid {_gw}x{_gh})");
            return null;
        }
        // 先从 start 洗可达域(含特殊边,不计软避让——避让只影响 A* 代价,不封 goal,
        // 否则卡死在窄门口时避让块会把唯一出口封死,实测 no_route 连坐全图)
        var reach = FloodReach(start);
        var goal = NearestInReach(to, reach);
        if (goal < 0)
        {
            AIBridgePlugin.Log?.LogWarning($"no_path: goal not reachable from start (to={to})");
            return null;
        }

        var g = new float[_walk.Length];
        var came = new int[_walk.Length];
        var open = new List<int> { start };
        var inOpen = new bool[_walk.Length];
        var closed = new bool[_walk.Length];
        for (var i = 0; i < g.Length; i++) { g[i] = float.MaxValue; came[i] = -1; }
        g[start] = 0f;
        inOpen[start] = true;
        var goalX = goal % _gw; var goalY = goal / _gw;
        var expanded = 0;

        while (open.Count > 0)
        {
            var cur = open[0]; var curF = g[cur] + Heur(cur, goalX, goalY);
            for (var i = 1; i < open.Count; i++)
            {
                var f = g[open[i]] + Heur(open[i], goalX, goalY);
                if (f < curF) { cur = open[i]; curF = f; }
            }
            if (cur == goal) break;
            open.Remove(cur);
            inOpen[cur] = false;
            closed[cur] = true;
            if (++expanded > AStarNodeCap)
            {
                AIBridgePlugin.Log?.LogWarning($"no_path: A* node cap hit ({AStarNodeCap})");
                return null;
            }

            var cx = cur % _gw; var cy = cur / _gw;
            for (var dir = 0; dir < 8; dir++)
            {
                var nx = cx + DirX[dir]; var ny = cy + DirY[dir];
                if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                var nb = ny * _gw + nx;
                if (closed[nb] || !_walk[nb] || EdgeBlocked(cur, dir)) continue;
                if ((dir & 1) == 1 && (!_walk[cy * _gw + nx] || !_walk[ny * _gw + cx])) continue;
                var step = (dir & 1) == 1 ? Cell * 1.414f : Cell;
                // 软避让:卡死格加价 4×(不硬封,唯一通路仍可用);离墙近再加价
                var ng = g[cur] + step * WallPenalty(nb) * (_avoid.Contains(nb) ? 50f : 1f);
                if (ng >= g[nb]) continue;
                g[nb] = ng;
                came[nb] = cur;
                if (!inOpen[nb]) { open.Add(nb); inOpen[nb] = true; }
            }
            // 特殊边互连:cur 是梯子/平台端点 → 可跳对端(乘具不受软避让影响)
            for (var li = 0; li < _links.Count; li++)
            {
                var lk = _links[li];
                int other;
                if (lk.CellA == cur) other = lk.CellB;
                else if (lk.CellB == cur) other = lk.CellA;
                else continue;
                if (other < 0 || closed[other] || !_walk[other]) continue;
                var ngL = g[cur] + lk.Cost;
                if (ngL >= g[other]) continue;
                g[other] = ngL;
                came[other] = cur;
                if (!inOpen[other]) { open.Add(other); inOpen[other] = true; }
            }
        }

        if (goal != start && came[goal] < 0) return null;
        // 回溯 raw = start → goal
        var raw = new List<int>();
        var node = goal;
        while (node != -1) { raw.Add(node); node = came[node]; }
        raw.Reverse();

        // 抽稀:每 2 格取点(密跟路防贴角切进家具);特殊边两侧(登乘格/着陆格)强制保留
        var path = new List<Vector2>();
        for (var i = 0; i < raw.Count; i++)
        {
            var lkOut = i < raw.Count - 1 ? LinkBetween(raw[i], raw[i + 1]) : null;
            var lkIn = i > 0 ? LinkBetween(raw[i - 1], raw[i]) : null;
            var rideOut = lkOut != null && !lkOut.IsDoor;
            var rideIn = lkIn != null && !lkIn.IsDoor;
            var special = rideOut || rideIn;   // 门边只算连通,不触发乘坐
            if (rideOut)
            {
                crossingsOut?.Add(new Crossing
                {
                    BoardIdx = path.Count,
                    FarPos = GridToWorld(raw[i + 1] % _gw, raw[i + 1] / _gw),
                    Lk = lkOut
                });
            }
            if (special || i % 4 == 0 || i == raw.Count - 1)
                path.Add(GridToWorld(raw[i] % _gw, raw[i] / _gw));
        }
        // 终点即所求坐标时才追加(否则 goal 离 to 很远——目标不可达/console 嵌在家具里——
        // 直线冲 to 会穿墙,实测卡死;走到最近可达格即为终态)
        var goalWorld = GridToWorld(goal % _gw, goal / _gw);
        if (Vector2.Distance(goalWorld, to) <= 0.6f) path.Add(to);
        return path;
    }

    private static float Heur(int cell, int goalX, int goalY)
    {
        var dx = Mathf.Abs(cell % _gw - goalX);
        var dy = Mathf.Abs(cell / _gw - goalY);
        return (dx + dy) * Cell * 0.9f;
    }

    // ---------------- 诊断 dump ----------------

    /// <summary>栅格位图 + 从玩家位置 BFS 的可达域(nav_dump 指令)。</summary>
    public static string DumpGridJson()
    {
        var me = PlayerControl.LocalPlayer;
        var from = me != null ? me.GetTruePosition() : Vector2.zero;
        var start = NearestWalkable(from);
        var reach = start >= 0 ? FloodReach(start) : new bool[_walk.Length];
        var sb = new StringBuilder(_walk.Length * 2 + 512);
        sb.Append("{\"type\":\"nav_dump\",\"gw\":").Append(_gw).Append(",\"gh\":").Append(_gh)
          .Append(",\"min_x\":").Append(MiniJson.F(_gridMin.x, 1)).Append(",\"min_y\":").Append(MiniJson.F(_gridMin.y, 1))
          .Append(",\"start_cell\":").Append(start)
          .Append(",\"links\":[");
        for (var i = 0; i < _links.Count; i++)
        {
            var lk = _links[i];
            if (i > 0) sb.Append(',');
            sb.Append("{\"kind\":").Append(MiniJson.Str(lk.IsPlatform ? "platform" : "ladder"))
              .Append(",\"a\":[").Append(MiniJson.F(lk.PosA.x, 1)).Append(',').Append(MiniJson.F(lk.PosA.y, 1)).Append(']')
              .Append(",\"b\":[").Append(MiniJson.F(lk.PosB.x, 1)).Append(',').Append(MiniJson.F(lk.PosB.y, 1)).Append("]}");
        }
        sb.Append("],\"ladders_raw\":[");
        try
        {
            var ladders = ShipStatus.Instance.Ladders;
            var wrote = false;
            for (var i = 0; i < ladders.Length; i++)
            {
                var l = ladders[i];
                if (l == null) continue;
                if (wrote) sb.Append(',');
                var lp = (Vector2)((Component)l).transform.position;
                sb.Append("{\"id\":").Append(l.Id)
                  .Append(",\"pos\":[").Append(MiniJson.F(lp.x, 1)).Append(',').Append(MiniJson.F(lp.y, 1)).Append(']')
                  .Append(",\"dest_id\":").Append(l.Destination != null ? l.Destination.Id : -1).Append('}');
                wrote = true;
            }
        }
        catch (Exception e) { sb.Append("{\"err\":").Append(MiniJson.Str(e.Message)).Append('}'); }
        sb.Append("],\"rows\":[");
        for (var gy = 0; gy < _gh; gy++)
        {
            if (gy > 0) sb.Append(',');
            sb.Append(MiniJson.Str(RowString(gy, reach)));
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static string RowString(int gy, bool[] reach)
    {
        var chars = new char[_gw];
        for (var gx = 0; gx < _gw; gx++)
        {
            var c = gy * _gw + gx;
            if (!_walk[c]) chars[gx] = '#';
            else if (reach[c]) chars[gx] = 'o';
            else chars[gx] = '.';
        }
        return new string(chars);
    }

    // ---------------- 指令接口 ----------------

    /// <summary>move_to 入口。label 用于回执/事件描述。返回 null=成功排队,否则错误码。</summary>
    public static string MoveTo(Vector2 dest, string label, float arriveTol)
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || me.Data == null) return "no_player";
        if (me.Data.IsDead) return "dead";
        if (Minigame.Instance != null) return "panel_open";
        EnsureGraph();
        if (!_gridReady) return "no_graph";

        var from = me.GetTruePosition();
        var path = FindPath(from, dest, _pathCrossings);
        if (path == null || path.Count == 0) return "no_path";

        _path = path.ToArray();
        _pathIdx = 0;
        _destPos = dest;
        _destLabel = label ?? $"{dest.x:F1},{dest.y:F1}";
        _arriveTol = arriveTol > 0.01f ? arriveTol : 0.6f;
        _active = true;
        _stuckTimer = 0f; _stuckCount = 0; _rerouteCount = 0; _lastPos = from; _lastDistDest = Vector2.Distance(from, dest);
        _riding = null;
        _avoid.Clear();
        _moveSeq++;
        return null;
    }

    public static void Stop(string reason = null)
    {
        if (!_active) return;
        _active = false;
        _riding = null;
        _avoid.Clear();
        LastMoveResult = reason ?? "stopped";
        try
        {
            var me = PlayerControl.LocalPlayer;
            if (me != null && me.MyPhysics != null) me.MyPhysics.SetNormalizedVelocity(Vector2.zero);
        }
        catch { }
        if (reason != null) _lastError = reason;
    }

    // ---------------- 每帧驱动 ----------------

    public static void Tick(float dt)
    {
        if (!_active) return;
        // 特殊边乘坐期:梯子/平台协程控制位置(moveable=false),轮询到位
        if (_riding != null)
        {
            TickRide(dt);
            return;
        }
        var me = PlayerControl.LocalPlayer;
        if (me == null || me.Data == null || me.Data.IsDead) { Stop("dead"); EmitMoveEvent("MOVE_ABORT", "dead"); return; }
        if (Minigame.Instance != null) { Stop("panel_open"); EmitMoveEvent("MOVE_ABORT", "panel_open"); return; }
        if (!me.CanMove) return; // 会议/动画中暂缓,不中止

        var pos = me.GetTruePosition();
        var wpIdx = Mathf.Min(_pathIdx, _path.Length - 1);
        var wp = _pathIdx < _path.Length ? _path[_pathIdx] : _destPos;

        // 乘坐触发:拉直目标即登乘格且已贴近(先于到达判定,1.5 > 常规容差)
        if (wpIdx < _path.Length)
        {
            for (var ci = 0; ci < _pathCrossings.Count; ci++)
            {
                var c = _pathCrossings[ci];
                if (c.BoardIdx != wpIdx) continue;
                if (Vector2.Distance(pos, wp) > 1.5f) continue;
                StartRide(c, pos, me);
                return;
            }
        }

        var distWp = Vector2.Distance(pos, wp);
        if (distWp <= _arriveTol)
        {
            _pathIdx++;
            _stuckTimer = 0f; _stuckCount = 0;
            if (_pathIdx >= _path.Length)
            {
                AIBridgePlugin.Log?.LogInfo(
                    $"nav done at {pos.x:F2},{pos.y:F2} dest=({_destPos.x:F1},{_destPos.y:F1}) ddest={Vector2.Distance(pos, _destPos):F2}");
                Stop(null);
                LastMoveResult = "MOVE_DONE";
                EmitMoveEvent("MOVE_DONE", $"dest={_destLabel}");
                return;
            }
            wp = _path[_pathIdx];
        }

        // 卡住检测:0.6s 内既无位移、也没向目标净推进(贴墙滑行时位移小但仍在推进,不能误判)
        _stuckTimer += dt;
        if (_stuckTimer >= 0.6f)
        {
            _stuckTimer = 0f;
            var dNow = Vector2.Distance(pos, _destPos);
            var progressed = _lastDistDest - dNow;
            if (progressed > 0.25f)
            {
                _lastDistDest = dNow;
                _stuckCount = 0;
            }
            else if (Vector2.Distance(pos, _lastPos) < 0.18f)
            {
                _stuckCount++;
                if (_stuckCount >= 3)
                {
                    var dwp = _pathIdx < _path.Length ? Vector2.Distance(pos, _path[_pathIdx]) : -1f;
                    AIBridgePlugin.Log?.LogInfo(
                        $"nav stuck at {pos.x:F2},{pos.y:F2} wp[{_pathIdx}/{_path.Length}] " +
                        $"wpdist={dwp:F2} dest=({_destPos.x:F1},{_destPos.y:F1}) ddest={Vector2.Distance(pos, _destPos):F2} avoid={_avoid.Count}");
                    if (!RerouteFrom(pos, $"stuck near {pos.x:F1},{pos.y:F1}"))
                        return;
                }
                else
                {
                    // 侧向扰动脱困
                    var dir = (wp - pos).normalized;
                    var side = new Vector2(-dir.y, dir.x) * ((_stuckCount & 1) == 0 ? 1 : -1);
                    _lastNudge = (dir + side * 0.8f).normalized;
                }
            }
            else { _stuckCount = Math.Max(0, _stuckCount - 1); _lastDistDest = dNow; }
            _lastPos = pos;
        }

        // 方向写入(_lastNudge 单帧生效;直行受阻时转向绕行,防顶墙角卡死)
        var moveDir = _lastNudge.HasValue ? _lastNudge.Value : Steer(pos, (wp - pos).normalized);
        _lastNudge = null;
        _steerDir = moveDir;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            try { me.MyPhysics.SetNormalizedVelocity(moveDir); } catch { }
        }
    }

    /// <summary>软避让卡死格后从 pos 重路由。返回 false=已判失败(MOVE_FAILED 已发)。</summary>
    private static bool RerouteFrom(Vector2 pos, string why)
    {
        // 重路由上限 3 次;超过判定失败
        if (++_rerouteCount > 3)
        {
            Stop("stuck");
            EmitMoveEvent("MOVE_FAILED", $"{why} x{_rerouteCount} -> {_destLabel}");
            return false;
        }
        var cell = NearestWalkable(pos);
        if (cell >= 0)
        {
            // 软避让:卡死格周围 ±3 格块加价(A* 代价 4×,不硬封——硬封会把窄出口堵死连坐 no_route)
            var cx0 = cell % _gw; var cy0 = cell / _gw;
            for (var dy = -4; dy <= 4; dy++)
            {
                for (var dx = -4; dx <= 4; dx++)
                {
                    var nx = cx0 + dx; var ny = cy0 + dy;
                    if (nx < 0 || ny < 0 || nx >= _gw || ny >= _gh) continue;
                    _avoid.Add(ny * _gw + nx);
                }
            }
        }
        var path = FindPath(pos, _destPos, _pathCrossings);
        if (path == null || path.Count == 0)
        {
            Stop("stuck");
            EmitMoveEvent("MOVE_FAILED", $"{why} no_route -> {_destLabel}");
            return false;
        }
        _path = path.ToArray(); _pathIdx = 0; _riding = null;
        _stuckCount = 0;
        AIBridgePlugin.Log?.LogInfo($"nav reroute #{_rerouteCount} ({why}) -> {_destLabel}");
        return true;
    }

    private static void StartRide(Crossing c, Vector2 pos, PlayerControl me)
    {
        c.Timer = 0f; c.RpcCount = 1; c.StartPos = pos;
        try
        {
            if (c.Lk.IsPlatform)
            {
                me.RpcUsePlatform(); // host/freeplay 直接触发 GapPlatform.Use(3u 内生效)
            }
            else
            {
                var nearA = Vector2.Distance(pos, c.Lk.PosA) <= Vector2.Distance(pos, c.Lk.PosB);
                me.MyPhysics.RpcClimbLadder(nearA ? c.Lk.LadderAtA : c.Lk.LadderAtB);
            }
        }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"ride start: {e.Message}"); }
        _riding = c;
        try { me.MyPhysics.SetNormalizedVelocity(Vector2.zero); } catch { }
        AIBridgePlugin.Log?.LogInfo(
            $"nav ride {(c.Lk.IsPlatform ? "platform" : "ladder")} board {pos.x:F1},{pos.y:F1} -> {c.FarPos.x:F1},{c.FarPos.y:F1}");
    }

    private static void TickRide(float dt)
    {
        var me = PlayerControl.LocalPlayer;
        var pos = me != null && me.Data != null ? me.GetTruePosition() : _riding.StartPos;
        _riding.Timer += dt;
        var tol = _riding.Lk.IsPlatform ? 2.2f : 1.5f;
        if (Vector2.Distance(pos, _riding.FarPos) <= tol)
        {
            var c = _riding;
            _riding = null;
            _pathIdx = c.BoardIdx + 1; // 下一点在着陆侧(修:不推进会走回登乘侧)
            _stuckTimer = 0f; _stuckCount = 0; _lastPos = pos;
            AIBridgePlugin.Log?.LogInfo($"nav ride done at {pos.x:F1},{pos.y:F1}");
            return;
        }
        // 平台 RPC 3s 未生效则重试(可能首发时平台在对侧):最多 3 次
        if (_riding.Lk.IsPlatform && _riding.RpcCount < 3 && _riding.Timer > 3f * _riding.RpcCount)
        {
            _riding.RpcCount++;
            try { me.RpcUsePlatform(); } catch { }
        }
        var timeout = _riding.Lk.IsPlatform ? 18f : 10f;
        if (_riding.Timer > timeout || me == null || me.Data == null || me.Data.IsDead)
        {
            var why = _riding.Lk.IsPlatform ? "platform_fail" : "ladder_fail";
            _riding = null;
            AIBridgePlugin.Log?.LogWarning($"nav ride timeout ({why}) at {pos.x:F1},{pos.y:F1}");
            RerouteFrom(pos, why);
        }
    }

    private static Vector2 _steerDir;

    /// <summary>前方一小段是否被实体挡住(玩家半径圆扫)。门/台/平台已忽略。</summary>
    private static bool DirBlocked(Vector2 p, Vector2 dir, float reach)
    {
        try
        {
            var hits = Physics2D.CircleCastAll(p, Mathf.Max(0.16f, _probeRadius * 0.9f), dir, reach, Constants.ShipAndObjectsMask);
            for (var i = 0; i < hits.Length; i++)
                if (!Ignorable(hits[i].collider)) return true;
            // 行人动态避让(玩家有物理碰撞;不放进栅格,因为它会走动)
            // 注意:CircleCast 从自身位置发出必然命中自己的 collider,必须先排除本地玩家
            var meCol = PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Collider : null;
            var ph = Physics2D.CircleCastAll(p, Mathf.Max(0.3f, _probeRadius + 0.2f), dir, reach, Constants.LivingPlayersOnlyMask);
            for (var i = 0; i < ph.Length; i++)
            {
                var col = ph[i].collider;
                if (col == null || col == meCol) continue;
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    /// <summary>路径拉直:在 [idx, 下一乘坐点] 范围内找离玩家最远的"直线可达"路点。
    /// 视线用与建图同一胶囊判定,天然贴墙但不会顶墙;比逐点跟随更顺。</summary>
    private static int BestVisibleWaypoint(Vector2 pos, int idx)
    {
        if (_path.Length == 0) return 0;
        var limit = _path.Length - 1;
        for (var ci = 0; ci < _pathCrossings.Count; ci++)
        {
            var b = _pathCrossings[ci].BoardIdx;
            if (b >= idx) { limit = Mathf.Min(limit, b); break; }   // 不越过乘坐点
        }
        var far = Mathf.Min(limit, idx + 8);
        for (var k = far; k > idx; k--)
        {
            if (!SegmentBlocked(pos, _path[k])) return k;
        }
        return Mathf.Min(idx, _path.Length - 1);
    }

    /// <summary>简单避障转向:直行受阻时尝试 ±25°/±50°/±75°/±100°,取第一个通畅方向。</summary>
    private static Vector2 Steer(Vector2 pos, Vector2 desired)
    {
        if (desired.sqrMagnitude < 0.001f) return desired;
        if (!DirBlocked(pos, desired, 0.45f)) return desired;
        for (var a = 25; a <= 100; a += 25)
        {
            var rad = a * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad); var sin = Mathf.Sin(rad);
            for (var s = 0; s < 2; s++)
            {
                var k = s == 0 ? sin : -sin;
                var cand = new Vector2(desired.x * cos - desired.y * k, desired.x * k + desired.y * cos).normalized;
                if (!DirBlocked(pos, cand, 0.4f)) return cand;
            }
        }
        return desired;
    }

    /// <summary>由 PlayerPhysics.FixedUpdate Postfix 调用:物理帧间持续保持速度。
    /// 乘坐期(moveable=false → CanMove=false)与会议/面板期天然不覆写。</summary>
    public static void PhysicsOverride(PlayerPhysics phys)
    {
        if (!_active || _riding != null) return;
        try
        {
            var me = phys.myPlayer;
            if (me == null || me != PlayerControl.LocalPlayer || !me.CanMove) return;
            if (_pathIdx >= _path.Length) return;
            var dir = _steerDir;
            if (dir.sqrMagnitude < 0.001f)
            {
                var wp = _path[_pathIdx];
                dir = (wp - me.GetTruePosition()).normalized;
            }
            if (dir.sqrMagnitude > 0.001f) phys.SetNormalizedVelocity(dir);
        }
        catch { }
    }

    public static string StatusJson()
    {
        var sb = new StringBuilder(128);
        sb.Append("{\"active\":").Append(MiniJson.B(_active))
          .Append(",\"dest\":").Append(MiniJson.Str(_destLabel))
          .Append(",\"cells\":").Append(_walk == null ? 0 : _walk.Length)
          .Append(",\"path_idx\":").Append(_pathIdx)
          .Append(",\"path_len\":").Append(_path.Length)
          .Append(",\"links\":").Append(_links.Count)
          .Append(",\"riding\":").Append(MiniJson.B(_riding != null))
          .Append(",\"reroute\":").Append(_rerouteCount)
          .Append(",\"last_err\":").Append(MiniJson.Str(_lastError ?? ""))
          .Append(",\"map\":").Append(MiniJson.Str(_gridMap)).Append('}');
        return sb.ToString();
    }

    private static void EmitMoveEvent(string code, string note)
    {
        EventTap.Emit(code, null, null, note);
    }
}
