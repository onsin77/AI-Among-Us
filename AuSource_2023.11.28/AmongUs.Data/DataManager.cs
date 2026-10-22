using AmongUs.Data.Player;
using AmongUs.Data.Settings;

namespace AmongUs.Data;

public static class DataManager
{
	private static PlayerData player;

	private static SettingsData settings;

	private static readonly Logger logger = new Logger("", Logger.Level.Info, Logger.Category.DataManager);

	public static PlayerData Player
	{
		get
		{
			if (player == null)
			{
				player = new PlayerData();
				player.ForceLoad();
			}
			return player;
		}
	}

	public static SettingsData Settings
	{
		get
		{
			if (settings == null)
			{
				settings = new SettingsData();
				settings.ForceLoad();
			}
			return settings;
		}
	}

	public static Logger Logger => logger;

	public static void DeleteAll()
	{
		Settings.Delete();
		Player.Delete();
	}
}
