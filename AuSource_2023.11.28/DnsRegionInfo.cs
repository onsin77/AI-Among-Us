using System.Net;
using Beebyte.Obfuscator;
using Newtonsoft.Json;

[Skip]
public class DnsRegionInfo : IRegionInfo
{
	public readonly string Fqdn;

	public readonly string DefaultIp;

	public readonly ushort Port = 22023;

	public readonly bool UseDtls;

	private ServerInfo[] cachedServers;

	public string Name { get; }

	[JsonIgnore]
	public string PingServer => Servers.Random().Ip;

	[JsonIgnore]
	public ServerInfo[] Servers
	{
		get
		{
			if (cachedServers == null)
			{
				PopulateServers();
			}
			return cachedServers;
		}
	}

	public StringNames TranslateName { get; }

	public string TargetServer { get; }

	public DnsRegionInfo(string fqdn, string name, StringNames translateName, string defaultIp, ushort port, bool useDtls = true)
	{
		if (port == 0)
		{
			port = 22023;
		}
		Fqdn = fqdn;
		Name = name;
		TranslateName = translateName;
		DefaultIp = defaultIp;
		Port = port;
		UseDtls = useDtls;
	}

	private void PopulateServers()
	{
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(Fqdn);
			ServerInfo[] array = new ServerInfo[hostAddresses.Length];
			for (int i = 0; i < hostAddresses.Length; i++)
			{
				array[i] = new ServerInfo($"{Name}-{i}", hostAddresses[i].ToString(), Port, UseDtls);
			}
			cachedServers = array;
		}
		catch
		{
			cachedServers = new ServerInfo[1]
			{
				new ServerInfo(Name ?? "", DefaultIp, 22023)
			};
		}
	}

	private DnsRegionInfo(string fqdn, string name, StringNames translateName, ServerInfo[] servers)
	{
		Fqdn = fqdn;
		Name = name;
		TranslateName = translateName;
		cachedServers = servers;
	}

	public bool Validate()
	{
		return !string.IsNullOrWhiteSpace(Fqdn);
	}

	public IRegionInfo Duplicate()
	{
		return new DnsRegionInfo(Fqdn, Name, TranslateName, Servers);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is IRegionInfo regionInfo)
		{
			return regionInfo.Name.Equals(Name);
		}
		return false;
	}
}
