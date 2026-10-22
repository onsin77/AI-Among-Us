using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;

namespace InnerNet;

public class GameFilterOptions
{
	public DateTime LastRefreshTime = DateTime.MinValue;

	public HashSet<string> FilterTags = new HashSet<string>();

	public void ResetToDefaults(DateTime lastRefreshTime)
	{
		LastRefreshTime = lastRefreshTime;
		FilterTags = new HashSet<string>(from StringNames n in Enum.GetValues(typeof(StringNames))
			where n.ToString().StartsWith("DefaultFilterTag_")
			select DestroyableSingleton<TranslationController>.Instance.GetString(n));
	}

	public void Serialize(MessageWriter writer)
	{
		writer.WritePacked(FilterTags.Count);
		foreach (string filterTag in FilterTags)
		{
			writer.Write(filterTag);
		}
	}

	public void Deserialize(MessageReader reader)
	{
		FilterTags.Clear();
		int num = reader.ReadPackedInt32();
		if (num < 16)
		{
			for (int i = 0; i < num; i++)
			{
				FilterTags.Add(reader.ReadString());
			}
		}
	}
}
