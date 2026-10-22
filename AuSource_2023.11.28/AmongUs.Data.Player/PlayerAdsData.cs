using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerAdsData : AbstractSaveDataGroup
{
	public const ShowAdsState DEFAULT_ADS_STATE = ShowAdsState.NotAccepted;

	public const bool DEFAULT_HAS_PURCHASED_AD_REMOVAL = false;

	[JsonProperty("adsState")]
	private ShowAdsState adsState;

	[JsonProperty("hasPurchasedAdRemoval")]
	private bool hasPurchasedAdRemoval;

	public ShowAdsState AdsState
	{
		get
		{
			return adsState;
		}
		set
		{
			SetValue(ref adsState, value, OnShowAdsStateChanged);
		}
	}

	public bool HasPurchasedAdRemoval
	{
		get
		{
			return true;
		}
		set
		{
			SetValue(ref hasPurchasedAdRemoval, value, OnHasPurchasedAdRemovalChanged);
		}
	}

	public event Action OnShowAdsStateChanged;

	public event Action OnHasPurchasedAdRemovalChanged;
}
