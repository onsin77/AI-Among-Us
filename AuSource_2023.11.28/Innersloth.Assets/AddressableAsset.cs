using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Innersloth.Assets;

public abstract class AddressableAsset : IDisposable, ISerializationCallbackReceiver
{
	public enum AssetLoadState
	{
		Null,
		Unloaded,
		Loading,
		Success,
		Error
	}

	protected static readonly Logger logger = new Logger("Assets", Logger.Level.Info, Logger.Category.Assets);

	void IDisposable.Dispose()
	{
		Destroy();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		OnSerialize();
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		OnDeserialize();
	}

	public bool IsLoading()
	{
		return GetState() == AssetLoadState.Loading;
	}

	public bool IsCompleted()
	{
		if (GetState() != AssetLoadState.Success)
		{
			return GetState() == AssetLoadState.Error;
		}
		return true;
	}

	public bool IsLoaded()
	{
		return GetState() == AssetLoadState.Success;
	}

	protected virtual void OnSerialize()
	{
	}

	protected virtual void OnDeserialize()
	{
	}

	public abstract AssetLoadState GetState();

	public abstract void LoadAsync(Action onSuccessCb = null, Action onErrorcb = null, Action onFinishedcb = null);

	public abstract IEnumerator CoLoadAsync(Action onFinished = null);

	public abstract void Unload();

	public abstract void Destroy();

	public abstract bool ReferenceEquals(AddressableAsset asset);

	public abstract bool ReferenceEquals(AssetReference asset);
}
[Serializable]
public class AddressableAsset<T> : AddressableAsset where T : Object
{
	[SerializeField]
	protected AssetReference assetRef;

	private CallbackResultGroup onLoadedCallbackGroup = new CallbackResultGroup();

	private AsyncOperationHandle handle;

	public CallbackResultGroup OnLoaded => onLoadedCallbackGroup;

	public AddressableAsset()
	{
		onLoadedCallbackGroup = new CallbackResultGroup();
	}

	public AddressableAsset(AssetReference assetRef)
	{
		this.assetRef = assetRef;
	}

	~AddressableAsset()
	{
		Destroy();
	}

	protected override void OnDeserialize()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (onLoadedCallbackGroup == null)
		{
			onLoadedCallbackGroup = new CallbackResultGroup();
		}
		handle = default;
	}

	public AddressableAsset<T> Clone()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		AddressableAsset<T> addressableAsset = new AddressableAsset<T>(assetRef);
		if (IsLoaded())
		{
			Addressables.ResourceManager.Acquire(handle);
			addressableAsset.handle = handle;
		}
		return addressableAsset;
	}

	public virtual T GetAsset()
	{
		if (!IsLoaded())
		{
			return default;
		}
		if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
		{
			object result = handle.Result;
			object obj = ((result is GameObject) ? result : null);
			if (obj == null)
			{
				return default;
			}
			return ((GameObject)obj).GetComponent<T>();
		}
		object result2 = handle.Result;
		return (T)((result2 is T) ? result2 : null);
	}

	public override IEnumerator CoLoadAsync(Action onFinished = null)
	{
		LoadAsync();
		yield return CoWaitUntilFinished();
		IsLoaded();
		onFinished?.Invoke();
	}

	public override void LoadAsync(Action onSuccessCb = null, Action onErrorcb = null, Action onFinishedcb = null)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (assetRef == null)
		{
			AddressableAsset.logger.Warning("[AddressableAsset<" + typeof(T).Name + ">] Tried to load a null asset reference!");
			return;
		}
		OnLoaded.AddOnSuccess(onSuccessCb);
		OnLoaded.AddOnError(onErrorcb);
		OnLoaded.AddOnFinished(onFinishedcb);
		if (IsLoading())
		{
			return;
		}
		if (IsCompleted())
		{
			AttachOnCompleteCallback();
			return;
		}
		if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
		{
			handle = AsyncOperationHandle<GameObject>.op_Implicit(Addressables.LoadAssetAsync<GameObject>((object)assetRef));
		}
		else
		{
			handle = AsyncOperationHandle<T>.op_Implicit(Addressables.LoadAssetAsync<T>((object)assetRef));
		}
		AttachOnCompleteCallback();
	}

	public IEnumerator CoWaitUntilFinished()
	{
		while (handle.IsValid() && (int)handle.Status == 0)
		{
			yield return null;
		}
	}

	public override void Unload()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (handle.IsValid())
		{
			Addressables.Release(handle);
			handle = default;
		}
	}

	public override void Destroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Unload();
		handle = default;
		OnLoaded.Destroy();
		assetRef = null;
		GC.SuppressFinalize(this);
	}

	public override bool ReferenceEquals(AddressableAsset asset)
	{
		if (asset == this)
		{
			return true;
		}
		if (asset is AddressableAsset<T> addressableAsset && addressableAsset.assetRef.RuntimeKey.Equals(assetRef.RuntimeKey))
		{
			return true;
		}
		return false;
	}

	public override bool ReferenceEquals(AssetReference asset)
	{
		if (asset == null)
		{
			return false;
		}
		if (assetRef == null)
		{
			return false;
		}
		return assetRef.RuntimeKey.Equals(asset.RuntimeKey);
	}

	public override AssetLoadState GetState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		if (!handle.IsValid())
		{
			return AssetLoadState.Unloaded;
		}
		if ((int)handle.Status == 0)
		{
			return AssetLoadState.Loading;
		}
		if ((int)handle.Status == 2)
		{
			return AssetLoadState.Error;
		}
		if ((int)handle.Status == 1)
		{
			return AssetLoadState.Success;
		}
		return AssetLoadState.Null;
	}

	private void AttachOnCompleteCallback()
	{
		handle.Completed -= HandleOnCompleted;
		handle.Completed += HandleOnCompleted;
	}

	private void HandleOnCompleted(AsyncOperationHandle obj)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (handle.IsValid())
		{
			handle.Completed -= HandleOnCompleted;
		}
		if ((int)obj.Status != 1)
		{
			AddressableAsset.logger.Error(string.Format("[{0}<{1}>] Could not load non-GameObject {2} {3}, invalid result. Exception: {4}", "AddressableAsset", typeof(T).Name, typeof(T), assetRef, handle.OperationException));
			OnLoaded.CallErrorAndClear();
			handle = default;
		}
		else
		{
			OnLoaded.CallSuccessAndClear();
		}
	}
}
