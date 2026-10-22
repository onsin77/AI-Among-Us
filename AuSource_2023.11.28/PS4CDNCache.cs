using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class PS4CDNCache : IPlatformCDNCache
{
	private string cacheDirectory;

	private Hash128 latestHashUsed;

	private string latestNameUsed;

	private Dictionary<string, byte[]> savedBundlesBytes;

	private static readonly Logger logger = new Logger("CDNCache", Logger.Level.Debug, Logger.Category.Addressables);

	public void InitCache(string cacheDir)
	{
		cacheDirectory = Path.Combine(Application.persistentDataPath, cacheDir);
		if (!Directory.Exists(cacheDirectory))
		{
			Directory.CreateDirectory(cacheDirectory);
		}
		savedBundlesBytes = new Dictionary<string, byte[]>();
	}

	public void SaveCatalog(string json, string localDataPath, string remoteHashValue)
	{
		string directoryName = Path.GetDirectoryName(localDataPath);
		logger.Debug("Saving cached content catalog to " + localDataPath);
		if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		logger.Debug("Writing cache");
		File.WriteAllText(localDataPath, json);
		File.WriteAllText(localDataPath.Replace(".json", ".hash"), remoteHashValue);
		logger.Debug("Finish Writing cache");
	}

	public void RemoveCatalog(string path)
	{
		File.Delete(path);
	}

	private void DeleteDirectory(string path)
	{
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}

	public UnityWebRequest BundleDownload(string url, CachedAssetBundle cachedAssetBundle)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		string text = Path.Combine(cacheDirectory, cachedAssetBundle.name, ((object)cachedAssetBundle.hash/*cast due to constrained. prefix*/).ToString());
		logger.Debug("File to load is: " + text);
		UnityWebRequest result;
		if (File.Exists(text))
		{
			logger.Debug("We have cache");
			result = null;
		}
		else
		{
			logger.Debug("We dont have cache");
			DeleteDirectory(Path.Combine(cacheDirectory, cachedAssetBundle.name));
			result = UnityWebRequest.Get(url);
		}
		latestHashUsed = cachedAssetBundle.hash;
		latestNameUsed = cachedAssetBundle.name;
		return result;
	}

	public void SaveBundleCache(byte[] data)
	{
		string text = Path.Combine(cacheDirectory, latestNameUsed, ((object)latestHashUsed/*cast due to constrained. prefix*/).ToString());
		logger.Debug("File to save is: " + text);
		string text2 = Path.Combine(cacheDirectory, latestNameUsed);
		logger.Debug("Dir to create is: " + text2);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
			logger.Debug("Directory Created");
		}
		logger.Debug("Final path to save is: " + text);
		File.WriteAllBytes(text, data);
	}

	public void LoadBundleFromCache(Action<AssetBundle> callback, byte[] data = null)
	{
		if (data != null)
		{
			logger.Debug("Loading passed bundle data ");
			AssetBundle obj = AssetBundle.LoadFromMemory(data);
			callback?.Invoke(obj);
			return;
		}
		if (savedBundlesBytes.ContainsKey(latestNameUsed))
		{
			logger.Debug("We already loaded this bundle from disk before");
			AssetBundle obj2 = AssetBundle.LoadFromMemory(savedBundlesBytes[latestNameUsed]);
			callback?.Invoke(obj2);
			return;
		}
		logger.Debug("No bundle data found, we need to load it from disk");
		string text = Path.Combine(cacheDirectory, latestNameUsed);
		if (Directory.Exists(text))
		{
			byte[] array = File.ReadAllBytes(Path.Combine(text, ((object)latestHashUsed/*cast due to constrained. prefix*/).ToString()));
			savedBundlesBytes[latestNameUsed] = array;
			AssetBundle obj3 = AssetBundle.LoadFromMemory(array);
			callback?.Invoke(obj3);
		}
		else
		{
			callback?.Invoke(null);
		}
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
