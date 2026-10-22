using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu]
public class VisorData : CosmeticData, IAddressableAssetProvider<VisorViewData>
{
	[SerializeField]
	private bool behindHats;

	public const string EmptyId = "visor_EmptyVisor";

	public AssetReference ViewDataRef;

	public bool BehindHats => behindHats;

	public bool IsEmpty => ProductId == "visor_EmptyVisor";

	public override string GetItemCategory()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visor);
	}

	public AddressableAsset<VisorViewData> CreateAddressableAsset()
	{
		return new AddressableAsset<VisorViewData>(ViewDataRef);
	}

	public AssetReference GetAssetReference()
	{
		return ViewDataRef;
	}

	public override void PreviewOnPlayer(PoolablePlayer p, int colorId)
	{
		base.PreviewOnPlayer(p, colorId);
		p.SetVisor(this, colorId);
	}

	public override IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		AddressableAsset<VisorViewData> asset = CreateAddressableAsset();
		yield return asset.CoLoadAsync();
		Sprite arg = asset.GetAsset()?.IdleFrame;
		onLoaded(arg, asset);
	}
}
