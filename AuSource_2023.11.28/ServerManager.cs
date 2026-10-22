using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Beebyte.Obfuscator;
using Hazel;
using Hazel.UPnP;
using Innersloth.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ServerManager : DestroyableSingleton<ServerManager>
{
	[Skip]
	[JsonObject]
	private struct JsonServerData
	{
		public int CurrentRegionIdx;

		public IRegionInfo[] Regions;

		internal void CleanAndMerge(IRegionInfo[] defaultRegions)
		{
			List<IRegionInfo> list = Regions.Where((IRegionInfo r) => r.Validate()).ToList();
			for (int num = defaultRegions.Length - 1; num >= 0; num--)
			{
				IRegionInfo region = defaultRegions[num];
				int num2 = list.FindIndex((IRegionInfo r) => r.Name.Equals(region.Name, StringComparison.OrdinalIgnoreCase));
				if (num2 == -1)
				{
					list.Insert(0, region);
				}
				else
				{
					list[num2] = region;
				}
			}
			Regions = list.ToArray();
		}
	}

	private struct PingWrapper(IRegionInfo region, Ping ping)
	{
		public IRegionInfo Region = region;

		public Ping Ping = ping;
	}

	public static readonly IRegionInfo[] DefaultRegions = new IRegionInfo[3]
	{
		new StaticHttpRegionInfo("North America", StringNames.ServerNA, "matchmaker.among.us", new ServerInfo[1]
		{
			new ServerInfo("Http-1", "https://matchmaker.among.us", 443, useDtls: false)
		}),
		new StaticHttpRegionInfo("Europe", StringNames.ServerEU, "matchmaker-eu.among.us", new ServerInfo[1]
		{
			new ServerInfo("Http-1", "https://matchmaker-eu.among.us", 443, useDtls: false)
		}),
		new StaticHttpRegionInfo("Asia", StringNames.ServerAS, "matchmaker-as.among.us", new ServerInfo[1]
		{
			new ServerInfo("Http-1", "https://matchmaker-as.among.us", 443, useDtls: false)
		})
	};

	private const bool useDtls = false;

	private const float PingTimeoutSeconds = 5f;

	private string serverInfoFileJson;

	private UpdateState state;

	public IRegionInfo CurrentRegion { get; private set; }

	public ServerInfo CurrentUdpServer { get; private set; }

	public bool IsHttp => CurrentRegion is StaticHttpRegionInfo;

	public string TargetServer => CurrentRegion.TargetServer;

	public IRegionInfo[] AvailableRegions { get; private set; } = DefaultRegions;

	private ServerInfo[] AvailableServers => CurrentRegion.Servers;

	public string UdpNetAddress => CurrentUdpServer.Ip;

	public ushort UdpNetPort => CurrentUdpServer.Port;

	public bool UdpUseDtls => CurrentUdpServer.UseDtls;

	internal void AddOrUpdateRegion(IRegionInfo newRegion)
	{
		if (DefaultRegions.All((IRegionInfo r) => !r.Name.Equals(newRegion.Name, StringComparison.OrdinalIgnoreCase)))
		{
			IEnumerable<IRegionInfo> source = AvailableRegions.Where((IRegionInfo r) => !r.Name.Equals(newRegion.Name, StringComparison.OrdinalIgnoreCase));
			if (source.Count() == AvailableRegions.Length)
			{
				Debug.Log((object)("Added new region: " + newRegion.Name));
				AvailableRegions = AvailableRegions.Append(newRegion).ToArray();
			}
			else
			{
				Debug.Log((object)("Updated existing region: " + newRegion.Name));
				AvailableRegions = source.Append(newRegion).ToArray();
			}
			CurrentRegion = newRegion;
			CurrentUdpServer = newRegion.Servers.Random();
			SaveServers();
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (!((Object)(object)DestroyableSingleton<ServerManager>.Instance != (Object)(object)this))
		{
			serverInfoFileJson = Path.Combine(PlatformPaths.persistentDataPath, "regionInfo.json");
			LoadServers();
			Task.Run((Action)HandleUpnp);
		}
	}

	private void HandleUpnp()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		try
		{
			UPnPHelper val = new UPnPHelper((ILogger)(object)NullLogger.Instance);
			try
			{
				try
				{
					val.DeleteForwardingRule(22024);
				}
				catch
				{
				}
				try
				{
					val.DeleteForwardingRule(22023);
				}
				catch
				{
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
	}

	[ContextMenu("Reselect Server")]
	internal void ReselectServer()
	{
		Debug.Log((object)"ServerManager::ReselectServer");
		CurrentRegion = CurrentRegion ?? DefaultRegions[0].Duplicate();
		if (AvailableServers.All((ServerInfo s) => s.Players == 0))
		{
			AvailableServers.Shuffle();
		}
		CurrentUdpServer = (from s in AvailableServers
			orderby s.ConnectionFailures, s.Players
			select s).First();
		Debug.Log((object)$"Selected server: {CurrentUdpServer}");
		state = UpdateState.Success;
	}

	public IEnumerator ReselectRegionFromDefaults()
	{
		Debug.Log((object)"ServerManager::ReselectRegionFromDefaults");
		AvailableRegions = DefaultRegions;
		List<Task<IPAddress[]>> dnsLookups = new List<Task<IPAddress[]>>();
		for (int i = 0; i < DefaultRegions.Length; i++)
		{
			IRegionInfo regionInfo = DefaultRegions[i];
			dnsLookups.Add(Dns.GetHostAddressesAsync(regionInfo.PingServer));
		}
		while (dnsLookups.Any((Task<IPAddress[]> task) => !task.IsCompleted))
		{
			yield return null;
		}
		List<PingWrapper> pings = new List<PingWrapper>();
		for (int num = 0; num < DefaultRegions.Length; num++)
		{
			IRegionInfo regionInfo2 = DefaultRegions[num];
			IPAddress[] result;
			try
			{
				result = dnsLookups[num].ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("DNS could not resolve " + regionInfo2.PingServer + " for reason: " + ex.Message));
				continue;
			}
			if (result == null || result.Length == 0)
			{
				Debug.LogError((object)("DNS - no IPs resolved for " + regionInfo2.PingServer));
			}
			else
			{
				pings.Add(new PingWrapper(regionInfo2, new Ping(result.Random().ToString())));
			}
		}
		float timeElapsedSeconds = 0f;
		while (pings.Count > 0 && timeElapsedSeconds < 5f && !pings.Any((PingWrapper p) => p.Ping.isDone && p.Ping.time >= 0))
		{
			yield return null;
			timeElapsedSeconds += Time.deltaTime;
		}
		IRegionInfo regionInfo3 = DefaultRegions.First();
		int num2 = int.MaxValue;
		foreach (PingWrapper item in pings)
		{
			if (item.Ping.isDone && item.Ping.time >= 0)
			{
				Debug.Log((object)("Ping time: " + item.Region.Name + " @ " + item.Ping.time));
				if (item.Ping.time < num2)
				{
					regionInfo3 = item.Region;
					num2 = item.Ping.time;
				}
			}
			item.Ping.DestroyPing();
		}
		CurrentRegion = regionInfo3.Duplicate();
		ReselectServer();
		SaveServers();
	}

	public IEnumerator WaitForServers()
	{
		while (state == UpdateState.Connecting)
		{
			yield return null;
		}
	}

	internal void SetRegion(IRegionInfo region)
	{
		CurrentRegion = region;
		ReselectServer();
		SaveServers();
	}

	public void SaveServers()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		try
		{
			FileIO.WriteAllText(contents: JsonConvert.SerializeObject((object)new JsonServerData
			{
				CurrentRegionIdx = AvailableRegions.IndexOf((IRegionInfo r) => r.Name == CurrentRegion.Name),
				Regions = AvailableRegions
			}, new JsonSerializerSettings
			{
				TypeNameHandling = (TypeNameHandling)4
			}), path: serverInfoFileJson);
		}
		catch
		{
		}
	}

	public void LoadServers()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		Debug.Log((object)"ServerManager::LoadServers");
		if (FileIO.Exists(serverInfoFileJson))
		{
			try
			{
				JsonServerData jsonServerData = JsonConvert.DeserializeObject<JsonServerData>(FileIO.ReadAllText(serverInfoFileJson), new JsonSerializerSettings
				{
					TypeNameHandling = (TypeNameHandling)4
				});
				jsonServerData.CleanAndMerge(DefaultRegions);
				AvailableRegions = jsonServerData.Regions;
				CurrentRegion = AvailableRegions[jsonServerData.CurrentRegionIdx.Wrap(AvailableRegions.Length)];
				CurrentUdpServer = CurrentRegion.Servers.Random();
				state = UpdateState.Success;
				SaveServers();
				return;
			}
			catch (Exception ex)
			{
				Debug.Log((object)$"Couldn't load regions: {ex}");
				Debug.LogException(ex, (Object)(object)this);
				((MonoBehaviour)this).StartCoroutine(ReselectRegionFromDefaults());
				return;
			}
		}
		((MonoBehaviour)this).StartCoroutine(ReselectRegionFromDefaults());
	}

	internal bool TrackServerFailure(string networkAddress)
	{
		ServerInfo srv = AvailableServers.FirstOrDefault((ServerInfo s) => s.Ip == networkAddress);
		if (srv != null)
		{
			srv.ConnectionFailures++;
			ServerInfo serverInfo = AvailableServers.OrderBy((ServerInfo s) => s.Players).FirstOrDefault((ServerInfo s) => s.ConnectionFailures < srv.ConnectionFailures);
			if (serverInfo != null)
			{
				CurrentUdpServer = serverInfo;
				AmongUsClient.Instance.SetEndpoint(serverInfo.Ip, serverInfo.Port, serverInfo.UseDtls);
				Debug.Log((object)("Attempting another server: " + serverInfo.Name));
				return true;
			}
		}
		return false;
	}
}
