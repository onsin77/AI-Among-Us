using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SwitchCDNCache : IPlatformCDNCache
{
	private const string CACHE_NAME = "AmongUsCache";

	private const string CACHE_NAME_DIR = "AmongUsCache:/";

	private const string CATALOG_DIR = "Catalog";

	private Hash128 latestHashUsed;

	private string latestNameUsed;

	private Dictionary<string, byte[]> savedBundlesBytes;

	private static readonly Logger logger = new Logger("CDNCache", Logger.Level.Debug, Logger.Category.Addressables);

	public void InitCache(string cacheDir)
	{
	}

	public void SaveCatalog(string json, string localDataPath, string remoteHashValue)
	{
	}

	public void RemoveCatalog(string path)
	{
		DeleteDirectory(Path.Combine("AmongUsCache:/", "Catalog"));
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
		path = Path.Combine("AmongUsCache:/", "Catalog", path.TrimStart('/'));
		logger.Debug("Checking if catalog exists: " + path);
		return CheckFileExists(path);
	}

	public string LoadLocalCatalog(string path)
	{
		byte[] outputData = null;
		path = Path.Combine("AmongUsCache:/", "Catalog", path.TrimStart('/'));
		Load(ref outputData, path);
		if (outputData != null)
		{
			return Encoding.UTF8.GetString(outputData);
		}
		return string.Empty;
	}

	private void DeleteDirectory(string path)
	{
	}

	private void Load(ref byte[] outputData, string filename)
	{
	}

	private void Save(byte[] dataByteArray, string fileDir)
	{
	}

	private bool CheckFileExists(string path)
	{
		return false;
	}
}
