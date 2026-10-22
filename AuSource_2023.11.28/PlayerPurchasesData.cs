using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.Data.Player;
using UnityEngine;

public class PlayerPurchasesData
{
	private HashSet<string> purchases;

	public PlayerPurchasesData()
	{
		purchases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public void UpdateLegacyPurchases()
	{
		string[] lEGACY_PURCHASE_KEYS = PlayerStoreData.LEGACY_PURCHASE_KEYS;
		foreach (string key in lEGACY_PURCHASE_KEYS)
		{
			ClearPurchase(key);
		}
		lEGACY_PURCHASE_KEYS = DataManager.Player.Store.GetLegacyPurchases();
		foreach (string purchased in lEGACY_PURCHASE_KEYS)
		{
			SetPurchased(purchased);
		}
	}

	public bool GetPurchase(string itemKey, string bundleKey)
	{
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

	public void SetPurchased(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			Debug.LogWarning((object)"[PlayerPurchasesData] Trying to SetPurchased for EMPTY key. Aborting.");
			return;
		}
		if (key == "bought_ads")
		{
			DataManager.Player.Ads.AdsState = ShowAdsState.Purchased;
			DataManager.Player.Save();
		}
		if (!purchases.Contains(key))
		{
			purchases.Add(key);
		}
	}

	public void ClearPurchase(string key)
	{
		purchases.Remove(key);
	}

	public void ClearAllPurchases()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string purchase in purchases)
		{
			if (PlayerStoreData.LEGACY_PURCHASE_KEYS.Contains(purchase))
			{
				hashSet.Add(purchase);
			}
		}
		purchases = hashSet;
	}
}
