using System;
using System.Collections;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PetData : CosmeticData, IAddressableAssetProvider<PetBehaviour>
{
	public const string EmptyId = "pet_EmptyPet";

	public StringNames StoreName;

	public AssetReference PetPrefabRef;

	public bool IsEmpty => ProdId == "pet_EmptyPet";

	public AddressableAsset<PetBehaviour> CreateAddressableAsset()
	{
		return new AddressableAsset<PetBehaviour>(PetPrefabRef);
	}

	public AssetReference GetAssetReference()
	{
		return PetPrefabRef;
	}

	public override IEnumerator CoLoadIcon(Action<Sprite, AddressableAsset> onLoaded)
	{
		AddressableAsset<PetBehaviour> asset = CreateAddressableAsset();
		yield return asset.CoLoadAsync();
		onLoaded(SpritePreview, asset);
	}

	public override string GetItemCategory()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerPet);
	}

	public override void PreviewOnPlayer(PoolablePlayer p, int colorId)
	{
		base.PreviewOnPlayer(p, colorId);
		p.SetPetIdle(this, colorId);
	}
}
