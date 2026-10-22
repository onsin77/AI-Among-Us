using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AIAmongUs;

/// <summary>
/// AI 桥插件入口:与主 mod 同程序集的第二个 BepInEx 插件。
/// 不修改 TheOtherUs 任何既有文件;迭代 1 为只读采集 + TCP 通道,
/// 主循环通过 InnerNetClient.FixedUpdate postfix 驱动(注入式 MonoBehaviour 已实测被场景切换销毁,弃用)。
/// </summary>
[BepInPlugin(Guid, "AI Among Us Bridge", "0.1.0")]
[BepInDependency("TheOtherUs.Options.v3")] // 主 mod 先加载(职业/选项数据就绪)
[BepInProcess("Among Us.exe")]
public class AIBridgePlugin : BasePlugin
{
    public const string Guid = "ai.amongus.bridge";
    internal static AIBridgePlugin Instance;
    internal static new ManualLogSource Log;

    private Harmony _harmony;

    public override void Load()
    {
        Instance = this;
        Log = base.Log;

        var enabled = Config.Bind("Bridge", "Enabled", true, "启用 AI 桥(状态采集 + TCP)");
        var port = Config.Bind("Bridge", "Port", 47600, "TCP 监听端口(仅 127.0.0.1)");

        if (!enabled.Value)
        {
            Log.LogWarning("AIAmongUs bridge disabled by config");
            return;
        }

        try
        {
            _harmony = new Harmony("ai.amongus.bridge");
            // 只 patch 我们自己的类型(PatchAll(Type)),绝不触碰程序集内 mod 既有 patch
            _harmony.PatchAll(typeof(AIPatches));

            BridgeServer.Start(port.Value);
            Log.LogInfo($"AIAmongUs bridge v0.1.0 loaded: TCP 127.0.0.1:{port.Value}, tick 10Hz (AmongUsClient.Update postfix)");
        }
        catch (System.Exception e)
        {
            Log.LogError($"AIAmongUs bridge init failed: {e}");
        }
    }
}
