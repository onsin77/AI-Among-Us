using System;
using UnityEngine;
using UnityEngine.Networking;

public class DefaultCDNCache : IPlatformCDNCache
{
	public void InitCache(string cacheDir)
	{
	}

	public void SaveCatalog(string json, string localDataPath, string remoteHashValue)
	{
	}

	public void RemoveCatalog(string path)
	{
	}

	public UnityWebRequest BundleDownload(string url, CachedAssetBundle cachedAssetBundle)
	{
		return null;
	}

	public void SaveBundleCache(byte[] data)
	{
	}

	public void LoadBundleFromCache(Action<AssetBundle> callback, byte[] data = null)
	{
	}

	public bool CheckLocalCatalog(string path)
	{
		return false;
	}

	public string LoadLocalCatalog(string path)
	{
		return null;
	}
}
