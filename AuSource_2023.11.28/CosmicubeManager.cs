using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CosmicubeManager : DestroyableSingleton<CosmicubeManager>
{
	public static Dictionary<UnlockableCosmetic.UnlockableQuality, Color> qualityColorMap = new Dictionary<UnlockableCosmetic.UnlockableQuality, Color>
	{
		{
			UnlockableCosmetic.UnlockableQuality.Nameplate,
			Palette.CosmicubeQuality_NamePlate
		},
		{
			UnlockableCosmetic.UnlockableQuality.Hat,
			Palette.CosmicubeQuality_Hat
		},
		{
			UnlockableCosmetic.UnlockableQuality.Visor,
			Palette.CosmicubeQuality_Visor
		},
		{
			UnlockableCosmetic.UnlockableQuality.Skin,
			Palette.CosmicubeQuality_Skin
		},
		{
			UnlockableCosmetic.UnlockableQuality.Pet,
			Palette.CosmicubeQuality_Pet
		},
		{
			UnlockableCosmetic.UnlockableQuality.Kill,
			Palette.CosmicubeQuality_Kill
		}
	};

	private List<CosmicubeData> allCubes;

	public CosmicubeData GetCubeDataByID(string id)
	{
		return allCubes.FirstOrDefault((CosmicubeData h) => h.ProdId == id) ?? allCubes.Find((CosmicubeData c) => c.IsEmpty);
	}

	public void Initialize()
	{
		allCubes = DestroyableSingleton<ReferenceDataManager>.Instance.Refdata.cosmicubes;
		IEnumerable<CosmicubeData> enumerable = allCubes.Where((CosmicubeData x) => x.IsAlwaysAvailable || x.LimitedTimeAvailable.IsAvailable());
		List<AssetReference> list = new List<AssetReference>();
		foreach (CosmicubeData item in enumerable)
		{
			list.Add(item.StoreImageRef);
		}
		if (list.Count > 0)
		{
			((MonoBehaviour)this).StartCoroutine(StartDownload(list));
		}
	}

	private IEnumerator StartDownload(List<AssetReference> toDownload)
	{
		List<AssetReference> reallyDownload = new List<AssetReference>();
		foreach (AssetReference aref in toDownload)
		{
			if (aref.RuntimeKeyIsValid())
			{
				AsyncOperationHandle<long> dlSizeHandle = Addressables.GetDownloadSizeAsync(aref.RuntimeKey);
				yield return dlSizeHandle;
				if (dlSizeHandle.Result > 0)
				{
					Debug.Log((object)$"Adding {aref.RuntimeKey} to download list, its download size is {dlSizeHandle.Result}");
					reallyDownload.Add(aref);
				}
				Addressables.Release<long>(dlSizeHandle);
			}
		}
		yield return null;
		if (reallyDownload.Count > 0)
		{
			Debug.Log((object)$"Beginning addressables download of {reallyDownload.Count} potential cosmicubes");
			float startTime = Time.time;
			yield return Addressables.DownloadDependenciesAsync((IEnumerable)reallyDownload.Select((AssetReference x) => x.RuntimeKey), (MergeMode)1, true);
			Debug.Log((object)$"Finished addressables download, took {Time.time - startTime} seconds");
		}
		else
		{
			Debug.Log((object)"There were no cosmicubes to download ahead of time");
		}
	}

	public IEnumerable<CosmicubeData> GetAllCubeData()
	{
		return allCubes;
	}

	public CosmicubeData[] GetAvailableCubeData()
	{
		return (from h in GetAllCubeData()
			where h.LimitedTimeAvailable.IsAvailable()
			where !h.Free && !h.NotInStore
			select h).ToArray();
	}

	public CosmicubeData[] GetUnlockedCubes()
	{
		return (from o in allCubes.Where((CosmicubeData s, int index) => (s.Free || DataManager.Player.Purchases.GetPurchase(s.ProdId, "")) && !s.IsEmpty)
			orderby o.Order descending, o.ProdId
			select o).ToArray();
	}

	public CosmicubeData GetDataForPodId(string podId)
	{
		return allCubes.FirstOrDefault((CosmicubeData c) => c.podId == podId);
	}

	public float GetCompletionProgress(CosmicubeData cube)
	{
		int num = 0;
		foreach (UnlockableCosmetic allUnlockable in cube.allUnlockables)
		{
			if ((Object)(object)allUnlockable == (Object)null || DataManager.Player.Purchases.GetPurchase(allUnlockable.GetProdId(), allUnlockable.GetBundleId()))
			{
				num++;
			}
		}
		return (float)num / (float)cube.allUnlockables.Count;
	}

	public bool IsCompleted(CosmicubeData cube)
	{
		int num = 0;
		foreach (UnlockableCosmetic allUnlockable in cube.allUnlockables)
		{
			if ((Object)(object)allUnlockable == (Object)null || DataManager.Player.Purchases.GetPurchase(allUnlockable.GetProdId(), allUnlockable.GetBundleId()))
			{
				num++;
			}
		}
		return num == cube.allUnlockables.Count;
	}

	static CosmicubeManager()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
	}
}
