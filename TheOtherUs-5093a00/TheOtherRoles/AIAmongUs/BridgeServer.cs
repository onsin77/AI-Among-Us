using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AIAmongUs;

/// <summary>
/// L2 通信桥服务端:TCP 127.0.0.1:Port,UTF-8 JSON 行协议。
/// 线程模型:
///   - accept 线程:接受连接(允许多客户端,全部广播);
///   - 每客户端 reader 线程:只读,行拆分后入 Inbound 队列;
///   - flusher 线程:只写,发送可靠队列(事件/ack/log)+ 易变槽(最新 state 快照,只保留最新);
///   - 主线程(Unity):在 AIHub.Update 里消费 Inbound 并产出上行消息。
/// 任何游戏对象访问都只发生在主线程。
/// </summary>
internal static class BridgeServer
{
    private static TcpListener _listener;
    private static readonly List<TcpClient> _clients = new();
    private static readonly object _clientLock = new();
    private static Thread _acceptThread;
    private static Thread _flushThread;
    private static volatile bool _running;

    private static readonly ConcurrentQueue<string> _reliable = new();
    private static volatile string _volatileState;
    private static long _volatileVersion;
    private static long _volatileSentVersion;

    internal static readonly ConcurrentQueue<string> Inbound = new();

    public static int Port { get; private set; }
    public static bool Running => _running;
    public static int ClientCount { get { lock (_clientLock) return _clients.Count; } }

    public static void Start(int port)
    {
        if (_running) return;
        Port = port;
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();

        // 诊断:确认 OS 层 socket 真实存在(排除运行时静默失效)
        try
        {
            AIBridgePlugin.Log?.LogInfo($"listener endpoint: {_listener.LocalEndpoint}");
            using var probe = new TcpClient();
            probe.Connect(IPAddress.Loopback, port);
            AIBridgePlugin.Log?.LogInfo($"self-probe connect: {probe.Connected}");
        }
        catch (Exception e)
        {
            AIBridgePlugin.Log?.LogError($"listener diag failed: {e.Message}");
        }

        _running = true;
        _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "AIBridge-Accept" };
        _acceptThread.Start();
        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "AIBridge-Flush" };
        _flushThread.Start();
    }

    public static void Stop()
    {
        AIBridgePlugin.Log?.LogInfo($"BridgeServer.Stop called (running={_running})\n{System.Environment.StackTrace}");
        _running = false;
        try { _listener?.Stop(); } catch { }
        lock (_clientLock)
        {
            foreach (var c in _clients)
            {
                try { c.Close(); } catch { }
            }
            _clients.Clear();
        }
    }

    // ---- 主线程调用 ----

    /// <summary>可靠消息:事件/ack/log,必达(FIFO,不覆盖)。</summary>
    public static void EnqueueReliable(string json)
    {
        if (!_running) return;
        _reliable.Enqueue(json);
        // 防御:可靠队列积压过长(客户端死链但 TCP 未断)时丢弃最旧,避免内存膨胀
        while (_reliable.Count > 2048 && _reliable.TryDequeue(out _)) { }
    }

    /// <summary>易变消息:最新状态快照,新覆盖旧。</summary>
    public static void SetVolatileState(string json)
    {
        if (!_running) return;
        Interlocked.Increment(ref _volatileVersion);
        _volatileState = json;
    }

    // ---- 网络 ----

    private static void AcceptLoop()
    {
        while (_running)
        {
            TcpClient client = null;
            try
            {
                client = _listener.AcceptTcpClient();
                client.NoDelay = true;
                // 关键:同步写超时,半开/死连接最多阻塞 flusher 2 秒,绝不让单个坏客户端卡死发送管线
                client.SendTimeout = 2000;
            }
            catch (Exception)
            {
                if (!_running) return;
                Thread.Sleep(200); // listener 带病运行时不空转烧 CPU
                continue;
            }

            lock (_clientLock) _clients.Add(client);
            var reader = new Thread(() => ClientReadLoop(client)) { IsBackground = true, Name = "AIBridge-Read" };
            reader.Start();
            var modVer = typeof(TheOtherRoles.TheOtherRolesPlugin).Assembly.GetName().Version?.ToString() ?? "?";
            EnqueueReliable("{\"type\":\"hello\",\"proto\":1,\"plugin\":\"AIAmongUs 0.1.0\",\"mod\":\"TheOtherUs " + MiniJson.Escape(modVer) + "\"}");
            AIBridgePlugin.Log?.LogInfo($"bridge client connected ({ClientCount} total)");        }
    }

    private static void ClientReadLoop(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            var buf = new byte[4096];
            var lineBuf = new StringBuilder();
                while (_running)
                {
                    int n;
                    try { n = stream.Read(buf, 0, buf.Length); }
                    catch (Exception) { break; }
                    if (n <= 0) break;
                    lineBuf.Append(Encoding.UTF8.GetString(buf, 0, n));
                    // 行拆分(容忍 \r\n)
                    string pending = lineBuf.ToString();
                    int nl;
                    while ((nl = pending.IndexOf('\n')) >= 0)
                    {
                        var line = pending.Substring(0, nl).TrimEnd('\r').Trim();
                        if (line.Length > 0)
                        {
                            Inbound.Enqueue(line);
                            // 入站也限流:主线程每帧只消费 64 条,恶意/异常洪峰不允许无限积压
                            while (Inbound.Count > 4096 && Inbound.TryDequeue(out _)) { }
                        }
                        pending = pending.Substring(nl + 1);
                    }
                    lineBuf.Clear();
                    if (pending.Length > 0) lineBuf.Append(pending);
                    if (lineBuf.Length > 1 << 20) lineBuf.Clear(); // 单行超 1MB 视为恶意/异常,丢弃
                }
        }
        catch (Exception) { }
        finally
        {
            lock (_clientLock) _clients.Remove(client);
            try { client.Close(); } catch { }
            AIBridgePlugin.Log?.LogInfo($"bridge client disconnected ({ClientCount} left)");
        }
    }

    private static void FlushLoop()
    {
        while (_running)
        {
            try
            {
                Thread.Sleep(30);

                string state = null;
                long ver = Interlocked.Read(ref _volatileVersion);
                if (ver > Interlocked.Read(ref _volatileSentVersion))
                    state = _volatileState;

                List<TcpClient> dead = null;
                lock (_clientLock)
                {
                    if (_clients.Count == 0)
                    {
                        // 零客户端:可靠消息(事件/ack)必须留在队列里等重连——先出队再判空会把
                        // agent 断线重连窗口内的消息全部丢掉,打破"必达"契约;
                        // state 是易变的,标记已发无妨(采集 10Hz 持续刷新,新客户端马上能拿到)
                        if (state != null) Interlocked.Exchange(ref _volatileSentVersion, ver);
                        continue;
                    }
                    var batch = new List<string>();
                    while (_reliable.TryDequeue(out var r)) batch.Add(r);
                    if (batch.Count == 0 && state == null) continue;
                    foreach (var c in _clients)
                    {
                        try
                        {
                            var s = c.GetStream();
                            foreach (var m in batch) WriteLine(s, m);
                            if (state != null) WriteLine(s, state);
                        }
                        catch (Exception)
                        {
                            (dead ??= new List<TcpClient>()).Add(c);
                        }
                    }
                    if (dead != null)
                        foreach (var d in dead)
                        {
                            _clients.Remove(d);
                            try { d.Close(); } catch { } // 不 Close 的话该客户端的 reader 线程可能永远阻塞在 Read
                        }
                }
                if (state != null) Interlocked.Exchange(ref _volatileSentVersion, ver);
            }
            catch (Exception) { }
        }
    }

    private static void WriteLine(NetworkStream s, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        s.Write(bytes, 0, bytes.Length);
        s.Flush();
    }
}
