using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;

[CreateAssetMenu]
public class UnlockableCosmetic : ScriptableObject
{
	public enum UnlockableQuality
	{
		Nameplate = 1,
		Hat,
		Visor,
		Skin,
		Pet,
		Kill
	}

	public UnlockableQuality quality;

	public CosmeticData unlockableCosmetic;

	public int cost;

	public string GetProdId()
	{
		return unlockableCosmetic.ProdId;
	}

	public string GetBundleId()
	{
		return unlockableCosmetic.BundleId;
	}

	public IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		yield return unlockableCosmetic.CoLoadIcon(onLoaded);
	}
}
