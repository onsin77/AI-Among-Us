using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Assets.InnerNet;
using InnerNet;
using Innersloth.IO;
using UnityEngine;

namespace AmongUs.Data.Legacy;

public static class LegacySaveManager
{
	private const int schemaVersion = 1;

	private static int lastSchemaVersion;

	private static bool loaded;

	private static bool loadedStats;

	private static bool loadedAnnounce;

	private static bool loadedQCFavorites;

	private static bool loadedStoreTabViewDates;

	private static bool loadedCachedPurchases;

	private static int accountLoginStatus = 0;

	private static string guardianEmail = "";

	private static bool deleteDeviceIDFailed = false;

	private static bool hasLoggedIn = false;

	private static bool isGuest = false;

	private static string epicAccountId = "";

	private static string lastPlayerName;

	private static uint colorConfig;

	private static string lastPet;

	private static string lastHat;

	private static string lastSkin;

	private static string lastVisor;

	private static string lastNamePlate;

	private static uint playerLevel = 0u;

	private static uint playerXp;

	private static uint playerXpRequiredForNextLevel;

	private static int privacyPolicyVersion = 0;

	private static Announcement lastAnnounce;

	private static bool showOnlineHelp = true;

	private static bool askRedeemDLC = true;

	private static bool warnedAboutGuestModeProgression = false;

	private static bool showMinPlayerWarning = true;

	private static string dobInfo = "";

	private static string birthDateSetDate = "";

	private static int birthDateDay = 1;

	private static int birthDateMonth = 1;

	private static int birthDateYear = 2021;

	private static string lastCosmicube;

	private static string storeBundlesViewDate = "";

	private static string storeHatsViewDate = "";

	private static string storeOutfitsViewDate = "";

	private static string storeVisorsViewDate = "";

	private static string storePetsViewDate = "";

	private static string storeNameplatesViewDate = "";

	private static string storeCosmicubeViewDate = "";

	private static byte showAdsScreen = 0;

	private static bool screenshake = true;

	private static bool streamerMode = false;

	private static bool colorblindMode = false;

	private static byte sfxVolume = byte.MaxValue;

	private static byte musicVolume = byte.MaxValue;

	private static bool vsync = false;

	private static uint lastLanguage = uint.MaxValue;

	private static ControlTypes touchConfig = ControlTypes.Keyboard;

	private static bool enableMouseMovement = false;

	private static float joyStickSize = 1f;

	private static bool crossplayAllPlatforms = true;

	private static bool enableFriendsListInvites = true;

	private static bool censorChat = true;

	private static int chatModeType = 0;

	private static string[] quickChatFavorites = new string[20];

	private static LegacySecureDataFile purchaseFile = new LegacySecureDataFile(Path.Combine(PlatformPaths.persistentDataPath, "secureNew"));

	private static HashSet<string> purchases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public static EOSManager.AccountLoginStatus AccountLoginStatus
	{
		get
		{
			LoadPlayerPrefs();
			return (EOSManager.AccountLoginStatus)accountLoginStatus;
		}
	}

	public static bool DeleteDeviceIDFailed
	{
		get
		{
			LoadPlayerPrefs();
			return deleteDeviceIDFailed;
		}
	}

	public static string GuardianEmail
	{
		get
		{
			LoadPlayerPrefs();
			return guardianEmail;
		}
	}

	public static string PlayerName
	{
		get
		{
			LoadPlayerPrefs();
			return lastPlayerName;
		}
	}

	public static byte BodyColor
	{
		get
		{
			LoadPlayerPrefs();
			return (byte)colorConfig;
		}
	}

	public static string LastPet
	{
		get
		{
			LoadPlayerPrefs();
			return lastPet;
		}
	}

	public static string LastHat
	{
		get
		{
			LoadPlayerPrefs();
			return lastHat;
		}
	}

	public static string LastSkin
	{
		get
		{
			LoadPlayerPrefs();
			return lastSkin;
		}
	}

	public static string LastVisor
	{
		get
		{
			LoadPlayerPrefs();
			return lastVisor;
		}
	}

	public static string LastNamePlate
	{
		get
		{
			LoadPlayerPrefs();
			return lastNamePlate;
		}
	}

	public static uint PlayerLevel
	{
		get
		{
			LoadPlayerPrefs();
			return playerLevel;
		}
	}

	public static uint PlayerXp
	{
		get
		{
			LoadPlayerPrefs();
			return playerXp;
		}
	}

	public static uint XpRequiredForNextLevel
	{
		get
		{
			LoadPlayerPrefs();
			return playerXpRequiredForNextLevel;
		}
	}

	public static int AcceptedPrivacyPolicy
	{
		get
		{
			LoadPlayerPrefs();
			return privacyPolicyVersion;
		}
	}

	public static bool ShowOnlineHelp
	{
		get
		{
			LoadPlayerPrefs();
			return showOnlineHelp;
		}
	}

	public static bool ShowMinPlayerWarning
	{
		get
		{
			LoadPlayerPrefs();
			return showMinPlayerWarning;
		}
	}

	public static Announcement LastAnnouncement
	{
		get
		{
			LoadAnnouncement();
			return lastAnnounce;
		}
	}

	public static string LastCosmicube
	{
		get
		{
			LoadPlayerPrefs();
			return lastCosmicube;
		}
	}

	public static string BirthDateSetDate
	{
		get
		{
			LoadPlayerPrefs();
			return birthDateSetDate;
		}
	}

	public static bool BoughtNoAds => true;

	public static ShowAdsState ShowAdsScreen
	{
		get
		{
			LoadPlayerPrefs();
			return (ShowAdsState)showAdsScreen;
		}
	}

	public static bool Screenshake
	{
		get
		{
			LoadPlayerPrefs();
			return screenshake;
		}
	}

	public static bool StreamerMode
	{
		get
		{
			LoadPlayerPrefs();
			return streamerMode;
		}
	}

	public static bool ColorBlindMode
	{
		get
		{
			LoadPlayerPrefs();
			return colorblindMode;
		}
	}

	public static float MusicVolume
	{
		get
		{
			LoadPlayerPrefs();
			return (float)(int)musicVolume / 255f;
		}
	}

	public static float SfxVolume
	{
		get
		{
			LoadPlayerPrefs();
			return (float)(int)sfxVolume / 255f;
		}
	}

	public static bool VSync
	{
		get
		{
			LoadPlayerPrefs();
			return vsync;
		}
	}

	public static uint LastLanguage
	{
		get
		{
			LoadPlayerPrefs();
			if (lastLanguage > 16)
			{
				lastLanguage = TranslationController.SelectDefaultLanguage();
			}
			return lastLanguage;
		}
	}

	public static ControlTypes ControlMode
	{
		get
		{
			LoadPlayerPrefs();
			return touchConfig;
		}
	}

	public static bool EnableMouseMovement
	{
		get
		{
			LoadPlayerPrefs();
			return enableMouseMovement;
		}
	}

	public static float JoystickSize
	{
		get
		{
			LoadPlayerPrefs();
			return joyStickSize;
		}
	}

	public static bool CrossplayAllPlatforms
	{
		get
		{
			LoadPlayerPrefs();
			return true;
		}
	}

	public static bool EnableFriendsListInvites
	{
		get
		{
			LoadPlayerPrefs();
			return enableFriendsListInvites;
		}
	}

	public static bool CensorChat
	{
		get
		{
			LoadPlayerPrefs();
			return censorChat;
		}
	}

	public static QuickChatModes ChatModeType
	{
		get
		{
			LoadPlayerPrefs();
			if (chatModeType == 0)
			{
				chatModeType = 1;
			}
			return (QuickChatModes)chatModeType;
		}
	}

	public static string[] QuickChatFavorites
	{
		get
		{
			LoadQuickChatFavorites();
			return quickChatFavorites;
		}
	}

	public static bool PlayerPrefsExists()
	{
		return FileIO.Exists(Path.Combine(PlatformPaths.persistentDataPath, GetPrefsName()));
	}

	public static void LoadPlayerPrefs(bool overrideLoad = false)
	{
		if (!overrideLoad && loaded)
		{
			return;
		}
		touchConfig = ControlTypes.Keyboard;
		string path = Path.Combine(PlatformPaths.persistentDataPath, GetPrefsName());
		if (FileIO.Exists(path))
		{
			loaded = true;
			string[] array = FileIO.ReadAllText(path).Split(',');
			lastPlayerName = array[0];
			if (array.Length > 1 && int.TryParse(array[1], out var result))
			{
				touchConfig = (ControlTypes)result;
			}
			if (array.Length <= 2 || !uint.TryParse(array[2], out colorConfig))
			{
				colorConfig = (uint)Palette.PlayerColors.RandomIdx();
			}
			TryGetBool(array, 8, out showMinPlayerWarning);
			TryGetBool(array, 9, out showOnlineHelp);
			TryGetString(array, 10, out lastHat);
			TryGetByte(array, 11, out sfxVolume);
			TryGetByte(array, 12, out musicVolume);
			TryGetFloat(array, 13, out joyStickSize, 1f);
			TryGetString(array, 15, out lastSkin);
			TryGetString(array, 16, out lastPet);
			TryGetBool(array, 17, out censorChat, @default: true);
			TryGetUint(array, 18, out lastLanguage, uint.MaxValue);
			TryGetBool(array, 19, out vsync);
			TryGetByte(array, 20, out showAdsScreen);
			TryGetInt(array, 21, out privacyPolicyVersion);
			if (array.Length > 25)
			{
				TryGetInt(array, 23, out birthDateMonth);
				TryGetInt(array, 24, out birthDateDay);
				TryGetInt(array, 25, out birthDateYear);
			}
			if (array.Length > 26)
			{
				birthDateSetDate = array[26];
			}
			TryGetInt(array, 27, out chatModeType);
			TryGetBool(array, 28, out isGuest);
			if (array.Length > 29)
			{
				guardianEmail = array[29];
			}
			TryGetBool(array, 30, out hasLoggedIn);
			TryGetInt(array, 31, out accountLoginStatus);
			TryGetInt(array, 32, out lastSchemaVersion);
			TryGetString(array, 33, out lastVisor);
			TryGetString(array, 34, out lastNamePlate);
			TryGetUint(array, 35, out playerLevel);
			TryGetUint(array, 36, out playerXp);
			TryGetString(array, 37, out lastCosmicube);
			TryGetBool(array, 38, out crossplayAllPlatforms, @default: true);
			TryGetUint(array, 39, out playerXpRequiredForNextLevel);
			TryGetBool(array, 40, out enableMouseMovement);
			TryGetBool(array, 41, out askRedeemDLC, @default: true);
			TryGetBool(array, 42, out warnedAboutGuestModeProgression);
			TryGetBool(array, 43, out screenshake);
			TryGetBool(array, 44, out enableFriendsListInvites);
			TryGetBool(array, 45, out colorblindMode);
			TryGetBool(array, 46, out deleteDeviceIDFailed);
			TryGetBool(array, 47, out streamerMode);
		}
	}

	public static void DeleteAll()
	{
		string[] files = Directory.GetFiles(PlatformPaths.persistentDataPath);
		string[] array = files;
		foreach (string text in array)
		{
			File.Delete(text);
			Debug.Log((object)("File " + text + " deleted!"));
		}
		if (files.Length == 0)
		{
			Debug.LogWarning((object)("No files found at " + PlatformPaths.persistentDataPath + ". Deleted nothing."));
		}
	}

	private static string GetPrefsName()
	{
		if (!string.IsNullOrEmpty(epicAccountId))
		{
			return epicAccountId + "_playerPrefs";
		}
		return "playerPrefs";
	}

	private static void TryGetBool(string[] parts, int index, out bool value, bool @default = false)
	{
		value = @default;
		if (index < parts.Length && bool.TryParse(parts[index], out var result))
		{
			value = result;
		}
	}

	private static void TryGetByte(string[] parts, int index, out byte value)
	{
		value = 0;
		if (index < parts.Length)
		{
			byte.TryParse(parts[index], out value);
		}
	}

	private static void TryGetFloat(string[] parts, int index, out float value, float @default = 0f)
	{
		value = @default;
		if (index < parts.Length)
		{
			float.TryParse(parts[index], NumberStyles.Number, CultureInfo.InvariantCulture, out value);
		}
	}

	private static void TryGetDateTime(string[] parts, int index, out DateTime value)
	{
		value = default;
		if (index < parts.Length)
		{
			DateTime.TryParse(parts[index], DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out value);
		}
	}

	private static void TryGetInt(string[] parts, int index, out int value)
	{
		value = 0;
		if (index < parts.Length)
		{
			int.TryParse(parts[index], out value);
		}
	}

	private static void TryGetUint(string[] parts, int index, out uint value, uint @default = 0u)
	{
		value = @default;
		if (index < parts.Length)
		{
			uint.TryParse(parts[index], out value);
		}
	}

	private static void TryGetUlong(string[] parts, int index, out ulong value, ulong @default = 0uL)
	{
		value = @default;
		if (index < parts.Length)
		{
			ulong.TryParse(parts[index], out value);
		}
	}

	private static void TryGetString(string[] parts, int index, out string value, string @default = "")
	{
		value = @default;
		if (index < parts.Length)
		{
			value = parts[index];
		}
	}

	public static void LoadQuickChatFavorites()
	{
		if (loadedQCFavorites)
		{
			return;
		}
		loadedQCFavorites = true;
		string path = Path.Combine(PlatformPaths.persistentDataPath, "quickChatFavorites");
		if (FileIO.Exists(path))
		{
			quickChatFavorites = FileIO.ReadAllText(path).Split('\n');
			return;
		}
		if (quickChatFavorites == null)
		{
			quickChatFavorites = new string[20];
		}
		if (quickChatFavorites[0] == null)
		{
			for (int i = 0; i < 20; i++)
			{
				quickChatFavorites[i] = "";
			}
		}
	}

	public static bool GetPurchase(string itemKey, string bundleKey)
	{
		LoadSecureData();
		bool flag = false;
		if (!string.IsNullOrEmpty(bundleKey))
		{
			flag = purchases.Contains(bundleKey);
		}
		if (!flag && !string.IsNullOrEmpty(itemKey))
		{
			flag = purchases.Contains(itemKey);
		}
		return flag;
	}

	public static int[] LoadLocalDoB()
	{
		LoadSecureData();
		if (string.IsNullOrEmpty(dobInfo))
		{
			return new int[3];
		}
		string[] array = dobInfo.Split('-');
		if (array.Length != 3)
		{
			return new int[3];
		}
		if (!int.TryParse(array[0], out var result))
		{
			return new int[3];
		}
		if (!int.TryParse(array[1], out var result2))
		{
			return new int[3];
		}
		if (!int.TryParse(array[2], out var result3))
		{
			return new int[3];
		}
		if (result != 0 || result2 != 0 || result3 != 0)
		{
			return new int[3] { result, result2, result3 };
		}
		return new int[3];
	}

	private static void LoadSecureData()
	{
		if (purchaseFile.Loaded)
		{
			return;
		}
		try
		{
			purchaseFile.LoadData((BinaryReader reader) =>
			{
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					string text = reader.ReadString();
					if (text.Split('-').Length == 3)
					{
						dobInfo = text;
					}
					else
					{
						purchases.Add(text);
					}
				}
			});
		}
		catch (NullReferenceException)
		{
		}
		catch (Exception ex2)
		{
			Debug.Log((object)("Deleted corrupt secure file outer: " + ex2));
			purchaseFile.Delete();
		}
	}

	private static void LoadAnnouncement()
	{
		if (loadedAnnounce)
		{
			return;
		}
		loadedAnnounce = true;
		string path = Path.Combine(PlatformPaths.persistentDataPath, "announcement");
		if (FileIO.Exists(path))
		{
			string[] array = FileIO.ReadAllText(path).Split(default(char));
			if (array.Length >= 2)
			{
				Announcement announcement = default;
				TryGetInt(array, 0, out announcement.Number);
				announcement.Text = array[1];
				lastAnnounce = announcement;
			}
			else
			{
				lastAnnounce = default;
			}
		}
	}

	public static bool StoreTabViewDatesFileExists()
	{
		return FileIO.Exists(Path.Combine(PlatformPaths.persistentDataPath, "storeTabViewDates"));
	}

	public static void LoadStoreTabViewDates()
	{
		if (!loadedStoreTabViewDates)
		{
			loadedStoreTabViewDates = true;
			string path = Path.Combine(PlatformPaths.persistentDataPath, "storeTabViewDates");
			if (FileIO.Exists(path))
			{
				string[] array = FileIO.ReadAllText(path).Split('\n');
				storeBundlesViewDate = array[0];
				storeHatsViewDate = array[1];
				storeOutfitsViewDate = array[2];
				storeVisorsViewDate = array[3];
				storePetsViewDate = array[4];
				storeNameplatesViewDate = array[5];
				storeCosmicubeViewDate = array[6];
			}
			else
			{
				storeBundlesViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Bundles];
				storeHatsViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Hats];
				storeOutfitsViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Skins];
				storeVisorsViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Visors];
				storePetsViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Pets];
				storeNameplatesViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Nameplates];
				storeCosmicubeViewDate = DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[StoreTab.Cubes];
			}
		}
	}

	public static bool TryGetStoreTabViewDate(StoreTab tab, out string output)
	{
		LoadStoreTabViewDates();
		switch (tab)
		{
		default:
			Debug.LogWarning((object)string.Format("[{0}] {1}() failed! {2} '{3}' has not been configured.", "LegacySaveManager", "TryGetStoreTabViewDate", "StoreTab", tab));
			output = "";
			return false;
		case StoreTab.Bundles:
			output = storeBundlesViewDate;
			return true;
		case StoreTab.Hats:
			output = storeHatsViewDate;
			return true;
		case StoreTab.Skins:
			output = storeOutfitsViewDate;
			return true;
		case StoreTab.Visors:
			output = storeVisorsViewDate;
			return true;
		case StoreTab.Pets:
			output = storePetsViewDate;
			return true;
		case StoreTab.Nameplates:
			output = storeNameplatesViewDate;
			return true;
		case StoreTab.Cubes:
			output = storeCosmicubeViewDate;
			return true;
		}
	}

	private static bool RunMigrations()
	{
		bool result = false;
		if (lastSchemaVersion < 1)
		{
			if (touchConfig <= ControlTypes.ScreenJoystick)
			{
				touchConfig++;
			}
			result = true;
		}
		return result;
	}
}
