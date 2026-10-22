using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class SkinData : CosmeticData, IAddressableAssetProvider<SkinViewData>
{
	public const string EmptyId = "skin_None";

	public const string RHMId = "skin_rhm";

	public AssetReference ViewDataRef;

	public string StoreName;

	public bool IsEmpty => ProdId == "skin_None";

	public AddressableAsset<SkinViewData> CreateAddressableAsset()
	{
		return new AddressableAsset<SkinViewData>(ViewDataRef);
	}

	public AssetReference GetAssetReference()
	{
		return ViewDataRef;
	}

	public override IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		AddressableAsset<SkinViewData> asset = CreateAddressableAsset();
		yield return asset.CoLoadAsync();
		Sprite arg = asset.GetAsset()?.IdleFrame;
		onLoaded(arg, asset);
	}

	public override void PreviewOnPlayer(PoolablePlayer p, int colorId)
	{
		base.PreviewOnPlayer(p, colorId);
		p.SetSkin(this, colorId);
	}

	public override string GetItemCategory()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerSkin);
	}
}
