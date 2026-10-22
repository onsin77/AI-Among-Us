using System.IO;
using Steamworks;
using UnityEngine;

namespace Innersloth.IO;

public class DefaultFileIO : IPlatformFileIO
{
	private string userName;

	private string platformName;

	public virtual string GetUserName()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(userName))
		{
			userName = SteamUser.GetSteamID().m_SteamID.ToString();
		}
		return userName;
	}

	public virtual string GetPlatformName()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(platformName))
		{
			platformName = ((object)Application.platform/*cast due to constrained. prefix*/).ToString().Replace("Player", "").Replace("Editor", "");
			platformName += " (Steam)";
		}
		return platformName;
	}

	public string GetRootDataPath()
	{
		return PlatformPaths.persistentDataPath;
	}

	public virtual bool Exists(string path)
	{
		return File.Exists(path);
	}

	public virtual string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public virtual void WriteAllText(string path, string contents)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllText(path, contents);
	}

	public virtual byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public virtual void WriteAllBytes(string path, byte[] bytes)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		File.WriteAllBytes(path, bytes);
	}

	public virtual void Delete(string path)
	{
		File.Delete(path);
	}
}
