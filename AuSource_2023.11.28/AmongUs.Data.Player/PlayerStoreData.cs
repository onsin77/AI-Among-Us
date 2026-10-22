using System;
using System.Collections.Generic;
using AmongUs.Data.Legacy;
using Newtonsoft.Json;
using UnityEngine;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerStoreData : AbstractSaveDataGroup
{
	public const string DEFAULT_ACTIVE_COSMICUBE = "";

	public static readonly DateTime DEFAULT_LAST_VIEW_DATE = DateTime.MinValue;

	[JsonProperty("activeCosmicube")]
	private string activeCosmicube = "";

	[JsonProperty("bundlesViewDate")]
	private DateTime bundlesViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("hatsViewDate")]
	private DateTime hatsViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("outfitsViewDate")]
	private DateTime outfitsViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("visorsViewDate")]
	private DateTime visorsViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("petsViewDate")]
	private DateTime petsViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("nameplatesViewDate")]
	private DateTime nameplatesViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("cosmicubeViewDate")]
	private DateTime cosmicubeViewDate = DEFAULT_LAST_VIEW_DATE;

	[JsonProperty("legacyPurchases")]
	private List<string> legacyPurchases = new List<string>();

	public const string LEGACY_PURCHASE_GEOFF_HAT = "hat_geoff";

	public const string LEGACY_PURCHASE_TEST_PET = "pet_test";

	public const string LEGACY_PURCHASE_PURPLE_TRAFFIC_HAT = "hat_traffic_purple";

	public const string LEGACY_PURCHASE_NEW_YEARS_2018_HAT = "hats_newyears2018";

	public const string LEGACY_PURCHASE_HOLIDAY_2018_HAT = "hat_holiday2018";

	public const string LEGACY_PURCHASE_HALLOWEEN_HAT_2019 = "hats_halloween2019";

	public static readonly string[] LEGACY_PURCHASE_KEYS = new string[6] { "hat_geoff", "pet_test", "hat_traffic_purple", "hats_newyears2018", "hat_holiday2018", "hats_halloween2019" };

	public string ActiveCosmicube
	{
		get
		{
			return activeCosmicube;
		}
		set
		{
			SetValue(ref activeCosmicube, value, OnActiveCosmicubeChanged);
		}
	}

	public DateTime LastBundlesViewDate
	{
		get
		{
			return bundlesViewDate;
		}
		set
		{
			SetValue(ref bundlesViewDate, value, OnBundlesViewDateChanged);
		}
	}

	public DateTime LastHatsViewDate
	{
		get
		{
			return hatsViewDate;
		}
		set
		{
			SetValue(ref hatsViewDate, value, OnHatsViewDateChanged);
		}
	}

	public DateTime LastOutfitsViewDate
	{
		get
		{
			return outfitsViewDate;
		}
		set
		{
			SetValue(ref outfitsViewDate, value, OnOutfitsViewDateChanged);
		}
	}

	public DateTime LastVisorsViewDate
	{
		get
		{
			return visorsViewDate;
		}
		set
		{
			SetValue(ref visorsViewDate, value, OnVisorsViewDateChanged);
		}
	}

	public DateTime LastPetsViewDate
	{
		get
		{
			return petsViewDate;
		}
		set
		{
			SetValue(ref petsViewDate, value, OnPetsViewDateChanged);
		}
	}

	public DateTime LastNameplatesViewDate
	{
		get
		{
			return nameplatesViewDate;
		}
		set
		{
			SetValue(ref nameplatesViewDate, value, OnNameplatesViewDateChanged);
		}
	}

	public DateTime LastCosmicubeViewDate
	{
		get
		{
			return cosmicubeViewDate;
		}
		set
		{
			SetValue(ref cosmicubeViewDate, value, OnCosmicubeViewDateChanged);
		}
	}

	public event Action OnActiveCosmicubeChanged;

	public event Action OnBundlesViewDateChanged;

	public event Action OnHatsViewDateChanged;

	public event Action OnOutfitsViewDateChanged;

	public event Action OnVisorsViewDateChanged;

	public event Action OnPetsViewDateChanged;

	public event Action OnNameplatesViewDateChanged;

	public event Action OnCosmicubeViewDateChanged;

	public event Action OnHasGeoffHatDropChanged;

	public event Action OnHasTestPetDropChanged;

	public event Action OnHasTrafficPurpleHatDropChanged;

	public event Action OnCachedPurchasedBundlesChanged;

	public DateTime GetStoreTabViewDate(StoreTab tab)
	{
		switch (tab)
		{
		case StoreTab.Bundles:
			return LastBundlesViewDate;
		case StoreTab.Hats:
			return LastHatsViewDate;
		case StoreTab.Skins:
			return LastOutfitsViewDate;
		case StoreTab.Visors:
			return LastVisorsViewDate;
		case StoreTab.Pets:
			return LastPetsViewDate;
		case StoreTab.Nameplates:
			return LastNameplatesViewDate;
		case StoreTab.Cubes:
			return LastCosmicubeViewDate;
		default:
			Debug.LogError((object)string.Format("[{0}] {1}.{2} has not been configured. Returning DateTime.Now", "PlayerStoreData", "StoreTab", tab));
			return DateTime.Now;
		}
	}

	public void SetStoreTabViewDate(StoreTab tab, DateTime val)
	{
		switch (tab)
		{
		case StoreTab.Bundles:
			LastBundlesViewDate = val;
			break;
		case StoreTab.Hats:
			LastHatsViewDate = val;
			break;
		case StoreTab.Skins:
			LastOutfitsViewDate = val;
			break;
		case StoreTab.Visors:
			LastVisorsViewDate = val;
			break;
		case StoreTab.Pets:
			LastPetsViewDate = val;
			break;
		case StoreTab.Nameplates:
			LastNameplatesViewDate = val;
			break;
		case StoreTab.Cubes:
			LastCosmicubeViewDate = val;
			break;
		default:
			Debug.LogError((object)string.Format("[{0}] {1}.{2} has not been configured.", "PlayerStoreData", "StoreTab", tab));
			break;
		}
	}

	public void SetLegacyPurchases()
	{
		string[] lEGACY_PURCHASE_KEYS = LEGACY_PURCHASE_KEYS;
		foreach (string key in lEGACY_PURCHASE_KEYS)
		{
			TryAddLegacyUnlock(key);
		}
	}

	private void TryAddLegacyUnlock(string key)
	{
		if (LegacySaveManager.GetPurchase(key, null) && !string.IsNullOrEmpty(key) && !legacyPurchases.Contains(key))
		{
			legacyPurchases.Add(key);
		}
	}

	public bool HasLegacyPurchase(string key)
	{
		return legacyPurchases.Contains(key);
	}

	public string[] GetLegacyPurchases()
	{
		return legacyPurchases.ToArray();
	}
}
