using System;
using HarmonyLib;

namespace AIAmongUs;

/// <summary>
/// 主循环驱动:AmongUsClient.Update 的 postfix —— AmongUsClient 是游戏自带
/// DontDestroyOnLoad 常驻单例,Update 为引擎每帧直调(全场景、主线程、从启动到退出)。
/// 仅 Postfix、最低优先级、不改任何返回值,与 TheOtherUs 既有 patch 无冲突面。
/// (迭代1实测:ClassInjector 注入的 MonoBehaviour 会被场景切换销毁;
///   InnerNetClient.FixedUpdate(基类 private)patch 不触发 —— 均弃用。)
/// </summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
internal static class AIPatches
{
    private static bool _errored;

    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            AIHub.Tick(UnityEngine.Time.unscaledDeltaTime);
        }
        catch (Exception e)
        {
            if (!_errored)
            {
                _errored = true;
                AIBridgePlugin.Log?.LogError($"AIHub.Tick threw (suppressed further): {e}");
            }
        }
    }
}

/// <summary>
/// 导航速度覆写:原版 FixedUpdate 每物理帧用摇杆输入覆写 velocity(空输入即清零),
/// 故导航激活时在 Postfix 再覆写一次,保证物理帧间速度持续。
/// 与 mod 的体型 patch(同为 Postfix)无冲突;CanMove=false(面板/会议)时不覆写。
/// </summary>
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
internal static class AIMovementPatch
{
    private static bool _errored;

    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    private static void Postfix(PlayerPhysics __instance)
    {
        try
        {
            Navigator.PhysicsOverride(__instance);
        }
        catch (Exception e)
        {
            // 物理帧 ~60/s:持续故障时逐帧 LogError 会刷爆日志,只记第一次
            if (!_errored)
            {
                _errored = true;
                AIBridgePlugin.Log?.LogError($"movement override failed (suppressed further): {e.Message}");
            }
        }
    }
}

/// <summary>
/// 聊天采集:ChatController.AddChat 是本地收到/发出的聊天最终落点(RpcSendChat 本地也会调)。
/// 仅 Postfix 记录,不改 UI 行为。freeplay 假人不发言,联机时这里就是真实聊天来源。
/// </summary>
[HarmonyPatch(typeof(ChatController), "AddChat")]
internal static class AIChatPatch
{
    [HarmonyPostfix]
    private static void Postfix(PlayerControl sourcePlayer, string chatText)
    {
        try { EventTap.OnChat(sourcePlayer, chatText); }
        catch (Exception e) { AIBridgePlugin.Log?.LogWarning($"chat tap failed: {e.Message}"); }
    }
}
