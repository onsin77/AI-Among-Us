using System.Collections.Generic;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerFriendData : AbstractSaveDataGroup
{
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public class CachedName
	{
		[JsonProperty("id")]
		private string puid;

		[JsonProperty("name")]
		private string name;

		public string PUID => puid;

		public string Name => name;

		public CachedName(string puid, string name)
		{
			this.puid = puid;
			this.name = name;
		}

		public void ChangeName(string newName)
		{
			name = newName;
		}
	}

	[JsonProperty("names")]
	private List<CachedName> cachedNames = new List<CachedName>();

	public string GetCachedName(string puid)
	{
		foreach (CachedName cachedName in cachedNames)
		{
			if (cachedName.PUID == puid)
			{
				return cachedName.Name;
			}
		}
		return null;
	}

	public void UpdateCachedName(string puid, string name)
	{
		foreach (CachedName cachedName in cachedNames)
		{
			if (cachedName.PUID == puid)
			{
				DataManager.Logger.Info("[PlayerFriendData] Updating existing cached name for friend\r\n[puid: " + puid + "]\r\n[name: " + name + "]\r\n");
				cachedName.ChangeName(name);
				HandleChange();
				return;
			}
		}
		DataManager.Logger.Info("[PlayerFriendData] Creating NEW cached name for friend\r\n[puid: " + puid + "]\r\n[name: " + name + "]\r\n");
		cachedNames.Add(new CachedName(puid, name));
		HandleChange();
	}
}
