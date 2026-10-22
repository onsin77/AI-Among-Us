using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReferenceDataManager : DestroyableSingleton<ReferenceDataManager>
{
	[SerializeField]
	private AssetReference referenceDataAsset;

	public Dictionary<StoreTab, string> StoreTabVersions;

	public ReferenceData Refdata { get; private set; }

	public IEnumerator Initialize()
	{
		AmongUsCDNCache.InitCache((IPlatformCDNCache)(object)new DefaultCDNCache());
		Addressables.WebRequestOverride = (UnityWebRequest request) =>
		{
			string s = ":" + Constants.CCDBucketAccessToken;
			request.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(s)));
			Debug.Log((object)("Sending addressables request to " + request.url));
		};
		AsyncOperationHandle<ReferenceData> refDataHandle = Addressables.LoadAssetAsync<ReferenceData>((object)referenceDataAsset);
		yield return refDataHandle;
		Refdata = refDataHandle.Result;
		SetStoreVersions();
	}

	private void SetStoreVersions()
	{
		StoreTabVersions = new Dictionary<StoreTab, string>
		{
			{
				StoreTab.Bundles,
				Refdata.StoreBundlesVersion
			},
			{
				StoreTab.Hats,
				Refdata.StoreHatsVersion
			},
			{
				StoreTab.Cubes,
				Refdata.StoreCubesVersion
			},
			{
				StoreTab.Skins,
				Refdata.StoreOutfitsVersion
			},
			{
				StoreTab.Pets,
				Refdata.StorePetsVersion
			},
			{
				StoreTab.Nameplates,
				Refdata.StoreNameplatesVersion
			},
			{
				StoreTab.Visors,
				Refdata.StoreVisorsVersion
			}
		};
	}

	public FeaturedItemGroup[] GetAllFeaturedItems()
	{
		return Refdata.FeaturedItemGroups.ToArray();
	}

	public void GetCurrentFeaturedItems(out List<FeaturedItemGroup> groups)
	{
		groups = new List<FeaturedItemGroup>();
		foreach (FeaturedItemGroup featuredItemGroup in Refdata.FeaturedItemGroups)
		{
			if (featuredItemGroup.LimitedTimeAvailable.IsAvailable())
			{
				groups.Add(featuredItemGroup);
			}
		}
	}
}
