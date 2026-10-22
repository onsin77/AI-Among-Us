using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class HatData : CosmeticData, IAddressableAssetProvider<HatViewData>
{
	public const string EmptyId = "hat_NoHat";

	public AssetReference ViewDataRef;

	public bool InFront;

	public bool NoBounce;

	public bool BlocksVisors;

	public string StoreName;

	public SkinData RelatedSkin;

	public bool IsEmpty => ProductId == "hat_NoHat";

	public AddressableAsset<HatViewData> CreateAddressableAsset()
	{
		return new AddressableAsset<HatViewData>(ViewDataRef);
	}

	public AssetReference GetAssetReference()
	{
		return ViewDataRef;
	}

	public override void PreviewOnPlayer(PoolablePlayer p, int colorId)
	{
		base.PreviewOnPlayer(p, colorId);
		p.SetHat(this, colorId);
	}

	public override string GetItemCategory()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerHat);
	}

	public override IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		AddressableAsset<HatViewData> asset = CreateAddressableAsset();
		yield return asset.CoLoadAsync();
		Sprite arg = asset.GetAsset()?.MainImage;
		onLoaded(arg, asset);
	}
}
