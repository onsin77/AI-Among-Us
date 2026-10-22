using System;
using System.Collections.Generic;
using UnityEngine;

public class Logger
{
	public enum Category
	{
		None,
		Network,
		Server,
		UI,
		Gameplay,
		Assets,
		DataManager,
		Addressables
	}

	public enum Level
	{
		None,
		Error,
		Warning,
		Info,
		Debug
	}

	public static class Tags
	{
		public const string EOSManager = "EOSManager";

		public const string CosmicubeMenu = "CosmicubeMenu";

		public const string InventoryManager = "InventoryManager";

		public const string CreateGameOptions = "CreateGameOptions";

		public const string Announcement = "Announcement";

		public const string ActionButton = "ActionButton";

		public const string ChatController = "ChatController";

		public const string QuickChat = "QuickChat";

		public const string FriendsListManager = "FriendsListManager";

		public const string MeetingHud = "MeetingHud";

		public const string PlayerCustomizationMenu = "PlayerCustomizationMenu";

		public const string AmongUsClient = "AmongUsClient";

		public const string ControllerManager = "ControllerManager";

		public const string PlayerControl = "PlayerControl";

		public const string PlayerPhysics = "PlayerPhysics";

		public const string Minigame = "Minigame";

		public const string ShipStatus = "ShipStatus";

		public const string StoreMenu = "StoreMenu";

		public const string StatsManager = "StatsManager";

		public const string StoreManager = "StoreManager";

		public const string DataManager = "DataManager";

		public const string GameManager = "GameManager";

		public const string Assets = "Assets";

		public const string FilterTagManager = "FilterTagManager";

		public const string HttpMatchmakerManager = "HttpMatchmakerManager";

		public const string AchievementManager = "AchievementManager";

		public const string CDNCache = "CDNCache";

		private static Dictionary<string, Level> CustomLevelOverrides;

		public static Level GetTagLevelOverride(string tag, Level currentLevel)
		{
			return Level.None;
		}

		public static void ReloadOverrides()
		{
		}
	}

	private static readonly Dictionary<Category, string> ColorDict = new Dictionary<Category, string>
	{
		{
			Category.Gameplay,
			"lime"
		},
		{
			Category.Network,
			"magenta"
		},
		{
			Category.UI,
			"yellow"
		},
		{
			Category.DataManager,
			"#FC8C03"
		}
	};

	private static Logger globalInstance;

	private readonly Category category;

	private readonly string tag;

	private Level level;

	public static Logger GlobalInstance
	{
		get
		{
			if (globalInstance == null)
			{
				globalInstance = new Logger(string.Empty, Level.Debug, Category.None);
			}
			return globalInstance;
		}
	}

	public Logger(string tag, Level level, Category category)
	{
		this.tag = tag;
		this.category = category;
		OverrideLevel(tag, ref level);
		SetLogLevel(level);
	}

	private static void OverrideLevel(string tag, ref Level level)
	{
		level = Tags.GetTagLevelOverride(tag, level);
	}

	public void SetLogLevel(Level level)
	{
		this.level = level;
	}

	public void Debug(object message, Object context = null)
	{
		OverrideLevel(tag, ref level);
		if (level >= Level.Debug)
		{
			Debug.Log((object)FormatMessageForConsole(message), context);
		}
		else
		{
			BugReportService.Instance.Debug(FormatMessageForBugReport(message), tag);
		}
	}

	public void Info(object message, Object context = null)
	{
		OverrideLevel(tag, ref level);
		if (level >= Level.Info)
		{
			Debug.Log((object)FormatMessageForConsole(message), context);
		}
		else
		{
			BugReportService.Instance.Info(FormatMessageForBugReport(message), tag);
		}
	}

	public void Warning(object message, Object context = null)
	{
		OverrideLevel(tag, ref level);
		if (level >= Level.Warning)
		{
			Debug.LogWarning((object)FormatMessageForConsole(message), context);
		}
		else
		{
			BugReportService.Instance.Warning(FormatMessageForBugReport(message), tag);
		}
	}

	public void Error(object message, Object context = null)
	{
		OverrideLevel(tag, ref level);
		if (level >= Level.Error)
		{
			Debug.LogError((object)FormatMessageForConsole(message), context);
		}
		else
		{
			BugReportService.Instance.Error(FormatMessageForBugReport(message), tag);
		}
	}

	public void Exception(Exception e)
	{
		BugReportService.Instance.Exception(e);
	}

	private string FormatMessageForConsole(object message)
	{
		string text = DateTime.UtcNow.ToString();
		string text2 = (string.IsNullOrEmpty(tag) ? string.Empty : (tag + " :: "));
		string text3 = ((category == Category.None) ? string.Empty : $"{category} :: ");
		string arg = "[" + text + "]: " + text3 + text2;
		return $"{arg}{message}";
	}

	private string FormatMessageForBugReport(object message)
	{
		string text = DateTime.UtcNow.ToString();
		return "[" + text + "]: " + message.ToString();
	}
}
