using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Innersloth.Assets;

public interface IAddressableAssetProvider<T> where T : Object
{
	AddressableAsset<T> CreateAddressableAsset();

	AssetReference GetAssetReference();
}
