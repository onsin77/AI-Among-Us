using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class NamePlateData : CosmeticData, IAddressableAssetProvider<NamePlateViewData>
{
	public const string EmptyId = "nameplate_NoPlate";

	public AssetReference ViewDataRef;

	public bool IsEmpty => ProductId == "nameplate_NoPlate";

	public override string GetItemCategory()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NamePlate);
	}

	public AddressableAsset<NamePlateViewData> CreateAddressableAsset()
	{
		return new AddressableAsset<NamePlateViewData>(ViewDataRef);
	}

	public AssetReference GetAssetReference()
	{
		return ViewDataRef;
	}

	public override IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		AddressableAsset<NamePlateViewData> asset = CreateAddressableAsset();
		yield return asset.CoLoadAsync();
		Sprite arg = asset.GetAsset()?.Image;
		onLoaded(arg, asset);
	}
}
