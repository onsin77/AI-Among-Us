using System.Linq;
using Beebyte.Obfuscator;

[Skip]
public class StaticRegionInfo : IRegionInfo
{
	public string Name { get; }

	public string PingServer { get; }

	public ServerInfo[] Servers { get; }

	public string TargetServer { get; }

	public StringNames TranslateName { get; }

	public StaticRegionInfo(string name, StringNames translateName, string pingServer, ServerInfo[] servers)
	{
		Name = name;
		PingServer = pingServer;
		Servers = servers;
		TranslateName = translateName;
	}

	public IRegionInfo Duplicate()
	{
		ServerInfo[] array = new ServerInfo[Servers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			ServerInfo serverInfo = Servers[i];
			array[i] = new ServerInfo(serverInfo.Name, serverInfo.Ip, serverInfo.Port)
			{
				ConnectionFailures = serverInfo.ConnectionFailures,
				Players = serverInfo.Players
			};
		}
		return new StaticRegionInfo(Name, TranslateName, PingServer, array);
	}

	public bool Validate()
	{
		if (!string.IsNullOrWhiteSpace(PingServer))
		{
			return Servers.All((ServerInfo s) => !string.IsNullOrWhiteSpace(s.Ip));
		}
		return false;
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
