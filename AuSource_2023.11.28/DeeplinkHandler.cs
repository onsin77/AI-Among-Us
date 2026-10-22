using System;
using System.Collections.Generic;
using System.Linq;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

public class DeeplinkHandler : MonoBehaviour
{
	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
	}

	private void Instance_LinkActivated(LinkActivation s)
	{
		Debug.Log((object)("Got init deeplink: " + s.Uri));
		if (!s.Uri.StartsWith("amongus://init/", StringComparison.OrdinalIgnoreCase) && !s.Uri.StartsWith("amongus://init?", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		Debug.Log((object)("Got init deeplink p2: " + string.Join("    ", s.QueryString.Select((KeyValuePair<string, string> k) => k.Key + "=" + k.Value))));
		if (s.QueryString.TryGetValue("serverport", out var value) && ushort.TryParse(value, out var result) && s.QueryString.TryGetValue("serverip", out var value2) && s.QueryString.TryGetValue("servername", out var value3) && s.QueryString.TryGetValue("usedtls", out var value4) && bool.TryParse(value4, out var result2))
		{
			value2 = Uri.UnescapeDataString(value2);
			if (value2.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				Debug.Log((object)"Got init deeplink :c come on, plz use https.");
			}
			else if (value2.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				ServerInfo serverInfo = new ServerInfo(((Object)this).name, value2, result, result2);
				StaticHttpRegionInfo newRegion = new StaticHttpRegionInfo(value3, StringNames.NoTranslation, value2, new ServerInfo[1] { serverInfo });
				DestroyableSingleton<ServerManager>.Instance.AddOrUpdateRegion(newRegion);
			}
			else
			{
				ServerInfo serverInfo2 = new ServerInfo(((Object)this).name, value2, result, result2);
				StaticRegionInfo newRegion2 = new StaticRegionInfo(value3, StringNames.NoTranslation, value2, new ServerInfo[1] { serverInfo2 });
				DestroyableSingleton<ServerManager>.Instance.AddOrUpdateRegion(newRegion2);
			}
		}
	}
}
