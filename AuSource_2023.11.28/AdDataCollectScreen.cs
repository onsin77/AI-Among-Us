using System.Collections;
using AmongUs.Data;
using UnityEngine;

public class AdDataCollectScreen : MonoBehaviour
{
	public void ForceShow()
	{
		DataManager.Player.Ads.AdsState = ShowAdsState.NotAccepted;
		DataManager.Player.Save();
		((Component)this).gameObject.SetActive(true);
	}

	public IEnumerator Show()
	{
		if (DataManager.Player.Ads.AdsState == ShowAdsState.NotAccepted && !DataManager.Player.Ads.HasPurchasedAdRemoval)
		{
			((Component)this).gameObject.SetActive(true);
			while (((Component)this).gameObject.activeSelf)
			{
				yield return null;
			}
		}
	}

	public void Update()
	{
		if (DataManager.Player.Ads.HasPurchasedAdRemoval)
		{
			DataManager.Player.Ads.AdsState = ShowAdsState.Purchased;
			DataManager.Player.Save();
			Close();
		}
	}

	public void Close()
	{
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	public void SetPersonalized()
	{
		if (DataManager.Player.Ads.AdsState != ShowAdsState.Purchased)
		{
			DataManager.Player.Ads.AdsState = ShowAdsState.Personalized;
			DataManager.Player.Save();
		}
		Close();
	}

	public void SetNonPersonalized()
	{
		if (DataManager.Player.Ads.AdsState != ShowAdsState.Purchased)
		{
			DataManager.Player.Ads.AdsState = ShowAdsState.NonPersonalized;
			DataManager.Player.Save();
		}
		Close();
	}
}
