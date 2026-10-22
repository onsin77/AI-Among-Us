using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Innersloth.Assets;

public class AddressableAssetGroup : AddressableAsset
{
	public readonly CallbackResultGroup OnLoaded = new CallbackResultGroup();

	private List<AddressableAsset> Assets = new List<AddressableAsset>();

	public AddressableAssetGroup()
	{
	}

	public AddressableAssetGroup(IEnumerable<AddressableAsset> assetList)
	{
		Assets.AddRange(assetList);
	}

	~AddressableAssetGroup()
	{
		Destroy();
	}

	public void Add(AddressableAsset asset)
	{
		if (asset != null && !Assets.Contains(asset))
		{
			Assets.Add(asset);
		}
	}

	public void Add(IEnumerable<AddressableAsset> list)
	{
		Assets.AddRange(list);
	}

	public AddressableAsset<T> AddReference<T>(AssetReference assetRef) where T : Object
	{
		AddressableAsset<T> fromReference = GetFromReference<T>(assetRef);
		AddressableAsset<T> addressableAsset = ((fromReference != null) ? fromReference.Clone() : new AddressableAsset<T>(assetRef));
		Add(addressableAsset);
		return addressableAsset;
	}

	public AddressableAsset<T> GetFromReference<T>(AssetReference assetRef) where T : Object
	{
		foreach (AddressableAsset asset in Assets)
		{
			if (asset.ReferenceEquals(assetRef))
			{
				return asset as AddressableAsset<T>;
			}
		}
		return null;
	}

	public AddressableAsset<T> GetFromReferenceRecur<T>(AssetReference assetRef) where T : Object
	{
		foreach (AddressableAsset asset in Assets)
		{
			if (asset.ReferenceEquals(assetRef))
			{
				return asset as AddressableAsset<T>;
			}
			AddressableAsset<T> addressableAsset = (asset as AddressableAssetGroup)?.GetFromReferenceRecur<T>(assetRef);
			if (addressableAsset != null)
			{
				return addressableAsset;
			}
		}
		return null;
	}

	public bool Remove(AddressableAsset asset)
	{
		if (asset == null)
		{
			return false;
		}
		return Assets.Remove(asset);
	}

	public void Clear()
	{
		Assets.Clear();
	}

	public override AssetLoadState GetState()
	{
		if (AllAssetsLoaded())
		{
			return AssetLoadState.Success;
		}
		if (AnyAssetsLoading())
		{
			return AssetLoadState.Loading;
		}
		return AssetLoadState.Unloaded;
	}

	public override void LoadAsync(Action onSuccessCb = null, Action onErrorcb = null, Action onFinishedcb = null)
	{
		OnLoaded.AddOnSuccess(onSuccessCb);
		OnLoaded.AddOnError(onErrorcb);
		OnLoaded.AddOnFinished(onFinishedcb);
		foreach (AddressableAsset asset in Assets)
		{
			asset.LoadAsync(null, null, OnAssetLoadedInternal);
		}
	}

	private void OnAssetLoadedInternal()
	{
		if (!AnyAssetsLoading())
		{
			OnLoaded.CallSuccessAndClear();
		}
	}

	public override IEnumerator CoLoadAsync(Action onFinished = null)
	{
		LoadAsync();
		yield return CoWaitUntilFinished();
		onFinished?.Invoke();
	}

	public IEnumerator CoWaitUntilFinished()
	{
		while (AnyAssetsLoading())
		{
			yield return null;
		}
	}

	public bool AnyAssetsLoading()
	{
		return NumAssetsLoading() > 0;
	}

	public int NumAssetsLoading()
	{
		int num = 0;
		foreach (AddressableAsset asset in Assets)
		{
			if (asset.IsLoading())
			{
				num++;
			}
		}
		return num;
	}

	public bool AllAssetsLoaded()
	{
		foreach (AddressableAsset asset in Assets)
		{
			if (!asset.IsLoaded())
			{
				return false;
			}
		}
		return true;
	}

	public override void Unload()
	{
		foreach (AddressableAsset asset in Assets)
		{
			asset.Unload();
		}
	}

	public override void Destroy()
	{
		Unload();
		Clear();
		Assets = null;
		OnLoaded.Clear();
		GC.SuppressFinalize(this);
	}

	public override bool ReferenceEquals(AddressableAsset asset)
	{
		return false;
	}

	public override bool ReferenceEquals(AssetReference asset)
	{
		return false;
	}
}
