using System;
using AmongUs.Data.Legacy;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerData : AbstractUserSaveData
{
	private static readonly byte[] DEFAULT_ENCRYPTED = new byte[0];

	[JsonProperty("account")]
	private PlayerAccountData account = new PlayerAccountData();

	[JsonProperty("customization")]
	private PlayerCustomizationData customization = new PlayerCustomizationData();

	[JsonProperty("stats")]
	private PlayerStatsData stats = new PlayerStatsData();

	[JsonProperty("onboarding")]
	private PlayerOnboardingData onboarding = new PlayerOnboardingData();

	[JsonProperty("age")]
	private byte[] encryptedAge = DEFAULT_ENCRYPTED;

	private PlayerAgeData age = new PlayerAgeData();

	[JsonProperty("store")]
	private byte[] encryptedStore = DEFAULT_ENCRYPTED;

	private PlayerStoreData store = new PlayerStoreData();

	[JsonProperty("ads")]
	private byte[] encryptedAds = DEFAULT_ENCRYPTED;

	private PlayerAdsData ads = new PlayerAdsData();

	[JsonProperty("announcements")]
	private PlayerAnnouncementData announcements = new PlayerAnnouncementData();

	[JsonProperty("friends")]
	private PlayerFriendData friends = new PlayerFriendData();

	private PlayerPurchasesData purchases = new PlayerPurchasesData();

	protected override int CurrentVersion => 1;

	public PlayerAccountData Account => account;

	public PlayerCustomizationData Customization => customization;

	public PlayerStatsData Stats => stats;

	public PlayerOnboardingData Onboarding => onboarding;

	public PlayerAgeData Age => age;

	public PlayerStoreData Store => store;

	public PlayerAdsData Ads => ads;

	public PlayerPurchasesData Purchases => purchases;

	public PlayerAnnouncementData Announcements => announcements;

	public PlayerFriendData Friends => friends;

	protected override string FileName => "player.amogus";

	protected override AbstractSaveDataGroup[] Groups => new AbstractSaveDataGroup[9] { Account, Customization, Stats, Onboarding, Age, Store, Ads, Announcements, Friends };

	protected override void OnSaveStart()
	{
		base.OnSaveStart();
		if (!TrySerializeAndEncrypt(age, out encryptedAge))
		{
			DataManager.Logger.Error("[PlayerData::OnSaveStart()] Failed to serialize and encrypt PlayerAgeData.");
		}
		if (!TrySerializeAndEncrypt(store, out encryptedStore))
		{
			DataManager.Logger.Error("[PlayerData::OnSaveStart()] Failed to serialize and encrypt PlayerStoreData.");
		}
		if (!TrySerializeAndEncrypt(ads, out encryptedAds))
		{
			DataManager.Logger.Error("[PlayerData::OnSaveStart()] Failed to serialize and encrypt PlayerAdsData.");
		}
	}

	protected override void OnLoadComplete()
	{
		base.OnLoadComplete();
		purchases.UpdateLegacyPurchases();
		if (encryptedAge != null && encryptedAge.Length != 0)
		{
			TryDecryptAndDeserialize(ref age, encryptedAge);
		}
		if (encryptedStore != null && encryptedStore.Length != 0)
		{
			TryDecryptAndDeserialize(ref store, encryptedStore);
		}
		if (encryptedAds != null && encryptedAds.Length != 0)
		{
			TryDecryptAndDeserialize(ref ads, encryptedAds);
		}
	}

	protected override void HandleVersionMigration(int version)
	{
		base.HandleVersionMigration(version);
		if (version >= 1)
		{
			return;
		}
		if (LegacySaveManager.PlayerPrefsExists())
		{
			Account.LoginStatus = LegacySaveManager.AccountLoginStatus;
			Account.GuardianEmail = LegacySaveManager.GuardianEmail;
			Account.DeleteDeviceIDFailed = LegacySaveManager.DeleteDeviceIDFailed;
			Customization.Name = LegacySaveManager.PlayerName;
			Customization.Color = LegacySaveManager.BodyColor;
			Customization.Pet = LegacySaveManager.LastPet;
			Customization.Hat = LegacySaveManager.LastHat;
			Customization.Skin = LegacySaveManager.LastSkin;
			Customization.Visor = LegacySaveManager.LastVisor;
			Customization.NamePlate = LegacySaveManager.LastNamePlate;
			Stats.Level = LegacySaveManager.PlayerLevel;
			Stats.Xp = LegacySaveManager.PlayerXp;
			Stats.XpForNextLevel = LegacySaveManager.XpRequiredForNextLevel;
			Onboarding.LastAcceptedPrivacyPolicyVersion = LegacySaveManager.AcceptedPrivacyPolicy;
			Onboarding.ViewedOnlineHelpMessage = !LegacySaveManager.ShowOnlineHelp;
			Onboarding.ViewedMinPlayerWarning = !LegacySaveManager.ShowMinPlayerWarning;
			Onboarding.AlwaysShowMinPlayerWarning = LegacySaveManager.ShowMinPlayerWarning;
			if (DateTime.TryParse(LegacySaveManager.BirthDateSetDate, out var result))
			{
				Age.LatestSetDate = result;
			}
			int[] array = LegacySaveManager.LoadLocalDoB();
			if (array.Length == 3 && array[0] > 0 && array[1] > 0 && array[2] > 0)
			{
				Age.DateOfBirth = new DateTime(array[0], array[1], array[2]);
			}
			Store.ActiveCosmicube = LegacySaveManager.LastCosmicube;
			Store.SetLegacyPurchases();
			if (LegacySaveManager.StoreTabViewDatesFileExists())
			{
				foreach (StoreTab value in Enum.GetValues(typeof(StoreTab)))
				{
					if (LegacySaveManager.TryGetStoreTabViewDate(value, out var output))
					{
						Store.SetStoreTabViewDate(value, Constants.ParseDate(output));
					}
				}
			}
			Ads.AdsState = LegacySaveManager.ShowAdsScreen;
			Ads.HasPurchasedAdRemoval = LegacySaveManager.BoughtNoAds;
		}
		dataVersion = 1;
	}
}
