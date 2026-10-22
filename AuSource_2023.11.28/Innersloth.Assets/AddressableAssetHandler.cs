using UnityEngine;

namespace Innersloth.Assets;

public class AddressableAssetHandler : MonoBehaviour
{
	private AddressableAssetGroup assetGroup = new AddressableAssetGroup();

	public static (AddressableAssetHandler, AddressableAssetGroup) AddToGameObject(GameObject obj, AddressableAsset asset = null)
	{
		AddressableAssetHandler addressableAssetHandler = obj.AddComponent<AddressableAssetHandler>();
		addressableAssetHandler.assetGroup.Add(asset);
		return (addressableAssetHandler, addressableAssetHandler.assetGroup);
	}

	public static (AddressableAssetHandler, AddressableAssetGroup) GetOrCreate(GameObject obj)
	{
		AddressableAssetHandler addressableAssetHandler = obj.GetComponent<AddressableAssetHandler>();
		if ((Object)(object)addressableAssetHandler == (Object)null)
		{
			addressableAssetHandler = obj.AddComponent<AddressableAssetHandler>();
		}
		return (addressableAssetHandler, addressableAssetHandler.assetGroup);
	}

	private void OnDestroy()
	{
		assetGroup?.Destroy();
		assetGroup = null;
	}
}
