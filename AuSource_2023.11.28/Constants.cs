using System;
using System.Globalization;
using System.Threading;
using UnityEngine;

public static class Constants
{
	public static class BuildSymbols
	{
		public static readonly string Steam = "STEAMWORKS";

		public static readonly string DisableSteam = "DISABLESTEAMWORKS";

		public static readonly string EGS = "EPICGAMESSTORE";

		public static readonly string DisableEGS = "DISABLEEPIC";

		public static readonly string XboxOneEGS = "EOS_PLATFORM_XBOXONEGDK";

		public static readonly string SeriesXEGS = "EOS_PLATFORM_XSX";

		public static readonly string Itchio = "ITCHIO";

		public static readonly string Win10 = "MICROSOFT_GAME_CORE";

		public static readonly string Console = "CONSOLE";

		public static readonly string Controller = "CONTROLLER";

		public static readonly string ItchioAndroid = "ITCHIO_ANDROID";

		public static readonly string Staging = "STAGING";

		public static readonly string QABuild = "QA_BUILD";

		public static readonly string IsMinorDebug = "IS_MINOR_DEBUG";

		public static readonly string DebugTool = "DEBUG_TOOL";

		public static readonly string Debug = "DEBUG";

		public static readonly string ExternalPlaytest = "EXTERNAL_PLAYTEST";
	}

	public static class CDNPlatforms
	{
		public static readonly string Steam = "Steam";

		public static readonly string EGS = "EGS";

		public static readonly string Itch = "Itch";

		public static readonly string Win10 = "Win10";

		public static readonly string ItchAndroid = "ItchAndroid";

		public static readonly string GPAndroid = "GPAndroid";

		public static readonly string iOS = "iOS";

		public static readonly string PS4 = "PS4";

		public static readonly string PS5 = "PS5";

		public static readonly string Switch = "Switch";

		public static readonly string XB1 = "XB1";

		public static readonly string XSX = "XSX";
	}

	public const float ButtonLongClickTime = 0.5f;

	public const int AnimationFrameRate = 24;

	public const int MinNumPlayers = 4;

	public const int MaxNumPlayers = 15;

	public const string LocalNetAddress = "127.0.0.1";

	public const ushort GamePlayPort = 22023;

	public const ushort AnnouncementPort = 22024;

	public const string InfinitySymbol = "∞";

	public const string DummyNamePrefix = "Dummy";

	public static readonly int ShipOnlyMask = LayerMask.GetMask(new string[1] { "Ship" });

	public static readonly int ShipAndObjectsMask = LayerMask.GetMask(new string[2] { "Ship", "Objects" });

	public static readonly int ShipAndAllObjectsMask = LayerMask.GetMask(new string[3] { "Ship", "Objects", "ShortObjects" });

	public static readonly int NotShipMask = ~LayerMask.GetMask(new string[1] { "Ship" });

	public static readonly int Usables = ~LayerMask.GetMask(new string[2] { "Ship", "UI" });

	public static readonly int PlayersOnlyMask = LayerMask.GetMask(new string[2] { "Players", "Ghost" });

	public static readonly int LivingPlayersOnlyMask = LayerMask.GetMask(new string[1] { "Players" });

	public static readonly int ShadowMask = LayerMask.GetMask(new string[3] { "Shadow", "Objects", "IlluminatedBlocking" });

	public static readonly string[] MapNames = new string[6] { "The Skeld", "MIRA HQ", "Polus", "dlekS ehT", "Airship", "The Fungle" };

	public static readonly string Beans = "Beans";

	public static readonly string[] PlayStationControllers = new string[2] { "Sony DualShock 4", "Sony DualSense" };

	public static readonly string PrivacyPolicyURL = "https://www.innersloth.com/privacy-policy/";

	public static readonly string TermsOfUseURL = "https://www.innersloth.com/terms-of-use-for-the-games/";

	public static readonly string ParentPortalURL = "https://parents.innersloth.com/";

	public static readonly string UserIdTokenError = "{\"errors\":[{\"code\":500,\"title\":\"USERIDTOKEN does not exist\",\"keyword\":\"user_auth_error\",\"detail\":1337420,\"detail_text\":\"usertokenid does not exist\",\"error_text\":\"usertokenid does not exist\"}]}\n";

	public static string CCDBucketAccessToken = "f26f8ec35cd092a0c4c4e6bae53c33f9";

	public static CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("en-US");

	public static readonly int[] CompatVersions = new int[1] { GetBroadcastVersion() };

	public const int Year = 2023;

	public const int Month = 11;

	public const int Day = 15;

	public const int Revision = 0;

	public const int VisualRevision = 0;

	public const int PrivacyPolicyVersion = 4;

	public const string AddressablesVersion = "2023_10_6_1026";

	public const string extraBuildVersionInfo = "(build num: 3708)";

	public const int pipelineBuildNumber = 3708;

	public const BuildEnvironment environment = BuildEnvironment.Production;

	public const byte EmptyPlayerID = byte.MaxValue;

	private static Thread _MainThread;

	public const string DefaultSpriteShader = "Sprites/Default";

	public const string PlayerColorShaderName = "PlayerShader";

	public const string PlayerColorShaderPath = "Unlit/PlayerShader";

	public const int LADDER_COOLDOWN = 3;

	public const int ZIPLINE_COOLDOWN = 3;

	public const int INITIAL_SABOTAGE_COOLDOWN = 10;

	public const int LOW_END_MEMORY_SIZE = 2500;

	private static int MODDER_VERSION = GetVersion(2222, 0, 0, 0);

	private const int MODDED_REVISION_MODIFIER_VALUE = 25;

	private static string cachedPlatformName = null;

	public static Thread MainThread => _MainThread;

	public static void SetMainThread()
	{
		_MainThread = Thread.CurrentThread;
	}

	internal static int GetBroadcastVersion()
	{
		return 50595550;
	}

	internal static bool IsVersionModded()
	{
		int broadcastVersion = GetBroadcastVersion();
		if (broadcastVersion == MODDER_VERSION)
		{
			return true;
		}
		return GetVersionComponents(broadcastVersion).Revision >= 25;
	}

	internal static int GetVersion(int year, int month, int day, int rev)
	{
		return year * 25000 + month * 1800 + day * 50 + rev;
	}

	internal static (int Year, int Month, int Day, int Revision) GetVersionComponents(int broadcastVersion)
	{
		int num = broadcastVersion / 25000;
		broadcastVersion -= num * 25000;
		int num2 = broadcastVersion / 1800;
		broadcastVersion -= num2 * 1800;
		int num3 = broadcastVersion / 50;
		broadcastVersion -= num3 * 50;
		int item = broadcastVersion;
		return (Year: num, Month: num2, Day: num3, Revision: item);
	}

	internal static byte[] GetBroadcastVersionBytes()
	{
		return BitConverter.GetBytes(GetBroadcastVersion());
	}

	public static bool ShouldPlaySfx()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
		{
			return DetectHeadset.Detect();
		}
		return true;
	}

	internal static bool ShouldFlipSkeld()
	{
		return false;
	}

	public static bool ShouldHorseAround()
	{
		try
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = new DateTime(utcNow.Year, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime dateTime2 = new DateTime(utcNow.Year, 4, 3, 6, 59, 0, 0, DateTimeKind.Utc);
			if (utcNow >= dateTime && utcNow <= dateTime2)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public static PlatformSpecificData GetPlatformData()
	{
		return new PlatformSpecificData
		{
			Platform = GetPlatformType(),
			PlatformName = GetPlatformName()
		};
	}

	public static void UpdateCachedPlatformName()
	{
		Debug.Log((object)"Updating cached platform name");
	}

	public static string GetPlatformName()
	{
		if (cachedPlatformName == null && Thread.CurrentThread == MainThread)
		{
			UpdateCachedPlatformName();
		}
		if (cachedPlatformName != null)
		{
			return cachedPlatformName;
		}
		return "TESTNAME";
	}

	public static string GetPurchasingPlatformType()
	{
		switch (GetPlatformType())
		{
		case Platforms.Android:
			return "android";
		case Platforms.Switch:
			return "switch";
		case Platforms.StandaloneSteamPC:
			return "steam";
		case Platforms.StandaloneEpicPC:
			return "egs";
		case Platforms.StandaloneWin10:
		case Platforms.Xbox:
			return "xbl";
		case Platforms.Playstation:
			return "psn";
		case Platforms.IPhone:
			return "ios";
		default:
			return "unsupported-" + GetPlatformType();
		}
	}

	public static Platforms GetPlatformType()
	{
		return Platforms.StandaloneSteamPC;
	}

	public static string GetCurrentPlatformName()
	{
		return "Steam";
	}

	public static void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public static DateTime ParseDate(string date)
	{
		return DateTime.Parse(date, CultureInfo);
	}
}
