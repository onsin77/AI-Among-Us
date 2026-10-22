using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Innersloth.Assets;

public static class AddressableAssetExtensions
{
	public static IEnumerator CoLoadAssetAsync(this MonoBehaviour behavior, AddressableAsset asset, Action onFinished)
	{
		yield return ((Component)behavior).gameObject.CoLoadAssetAsync(asset, onFinished);
	}

	public static IEnumerator CoLoadAssetAsync(this GameObject gameObject, AddressableAsset asset, Action onFinished)
	{
		AddressableAssetHandler.GetOrCreate(gameObject).Item2.Add(asset);
		yield return asset.CoLoadAsync(onFinished);
	}

	public static IEnumerator CoLoadAssetAsync<T>(this MonoBehaviour behavior, AssetReference reference, Action<T> onFinished) where T : Object
	{
		yield return ((Component)behavior).gameObject.CoLoadAssetAsync(reference, onFinished);
	}

	public static IEnumerator CoLoadAssetAsync<T>(this GameObject gameObject, AssetReference reference, Action<T> onFinished) where T : Object
	{
		AddressableAssetGroup item = AddressableAssetHandler.GetOrCreate(gameObject).Item2;
		AddressableAsset<T> asset = item.AddReference<T>(reference);
		yield return asset.CoLoadAsync(() =>
		{
			onFinished(asset.GetAsset());
		});
	}

	public static IEnumerator CoLoadAssetAsync<T>(this MonoBehaviour behavior, IAddressableAssetProvider<T> provider, Action<T> onFinished) where T : Object
	{
		yield return ((Component)behavior).gameObject.CoLoadAssetAsync(provider, onFinished);
	}

	public static IEnumerator CoLoadAssetAsync<T>(this GameObject gameObject, IAddressableAssetProvider<T> provider, Action<T> onFinished) where T : Object
	{
		AddressableAssetGroup item = AddressableAssetHandler.GetOrCreate(gameObject).Item2;
		AddressableAsset<T> asset = provider.CreateAddressableAsset();
		item.Add(asset);
		yield return asset.CoLoadAsync(() =>
		{
			onFinished(asset.GetAsset());
		});
	}

	public static void UnloadAddressableAsset(this MonoBehaviour behavior, AddressableAsset asset)
	{
		AddressableAssetHandler.GetOrCreate(((Component)behavior).gameObject).Item2.Remove(asset);
		asset?.Unload();
	}

	public static void UnloadAddressableAsset<T>(this MonoBehaviour behavior, IAddressableAssetProvider<T> provider) where T : Object
	{
		AddressableAssetGroup item = AddressableAssetHandler.GetOrCreate(((Component)behavior).gameObject).Item2;
		AddressableAsset<T> fromReference = item.GetFromReference<T>(provider.GetAssetReference());
		item.Remove(fromReference);
		fromReference?.Unload();
	}

	public static void LoadAssetAsync(this MonoBehaviour behavior, AddressableAsset asset, Action onSuccess, Action onError = null)
	{
		((Component)behavior).gameObject.LoadAssetAsync(asset, onSuccess, onError);
	}

	public static void LoadAssetAsync(this GameObject gameObject, AddressableAsset asset, Action onSuccess, Action onError = null)
	{
		AddressableAssetHandler.GetOrCreate(gameObject).Item2.Add(asset);
		asset.LoadAsync(onSuccess, onError);
	}

	public static void LoadAssetAsync<T>(this MonoBehaviour behavior, AssetReference reference, Action<T> onSuccess, Action onError = null) where T : Object
	{
		((Component)behavior).gameObject.LoadAssetAsync(reference, onSuccess, onError);
	}

	public static void LoadAssetAsync<T>(this GameObject gameObject, AssetReference reference, Action<T> onSuccess, Action onError = null) where T : Object
	{
		AddressableAssetGroup item = AddressableAssetHandler.GetOrCreate(gameObject).Item2;
		AddressableAsset<T> asset = item.AddReference<T>(reference);
		asset.LoadAsync(() =>
		{
			onSuccess(asset.GetAsset());
		}, onError);
	}

	public static void LoadAssetAsync<T>(this MonoBehaviour behavior, IAddressableAssetProvider<T> provider, Action<T> onSuccess, Action onError = null) where T : Object
	{
		((Component)behavior).gameObject.LoadAssetAsync(provider, onSuccess, onError);
	}

	public static void LoadAssetAsync<T>(this GameObject gameObject, IAddressableAssetProvider<T> provider, Action<T> onSuccess, Action onError = null) where T : Object
	{
		AddressableAssetGroup item = AddressableAssetHandler.GetOrCreate(gameObject).Item2;
		AddressableAsset<T> asset = provider.CreateAddressableAsset();
		item.Add(asset);
		asset.LoadAsync(() =>
		{
			onSuccess(asset.GetAsset());
		}, onError);
	}
}
