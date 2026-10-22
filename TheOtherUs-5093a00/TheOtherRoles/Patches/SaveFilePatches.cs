namespace TheOtherRoles.Patches;

[HarmonyPatch]
public static class SaveFilePatches
{
    // 存档隔离后缀：附带游戏本体版本号（如 _TOUE_20231128）。
    // 不同游戏版本的游戏选项数据（IGameOptions）序列化格式不同，共用同一个存档文件名
    // 会导致旧版游戏读到新版格式后解析失败，GameOptionsManager 初始化 NRE，游戏无法开始。
    private static string cachedSuffix;

    private static string Suffix
    {
        get
        {
            if (cachedSuffix != null) return cachedSuffix;
            try
            {
                var digits = new string(Application.version.Where(char.IsDigit).ToArray());
                cachedSuffix = "_TOUE_" + (digits.Length > 0 ? digits : "unknown");
            }
            catch
            {
                cachedSuffix = "_TOUE";
            }
            return cachedSuffix;
        }
    }

    [HarmonyPatch(typeof(AmongUs.Data.Settings.SettingsData), nameof(AmongUs.Data.Player.PlayerData.FileName), MethodType.Getter)]
    public class SettingsFilePatch
    {
        public static void Postfix(ref string __result)
        {
            __result += Suffix;
        }
    }

    [HarmonyPatch(typeof(AmongUs.Data.Player.PlayerData), nameof(AmongUs.Data.Player.PlayerData.FileName), MethodType.Getter)]
    public class SaveManagerPatch
    {
        public static void Postfix(ref string __result)
        {
            __result += Suffix;
        }
    }

    [HarmonyPatch(typeof(AmongUs.Data.Legacy.LegacySaveManager), nameof(AmongUs.Data.Legacy.LegacySaveManager.GetPrefsName))]
    public class LegacySaveManagerPatch
    {
        public static void Postfix(ref string __result)
        {
            __result += Suffix;
        }
    }
}
