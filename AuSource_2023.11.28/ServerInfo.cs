using Beebyte.Obfuscator;
using Newtonsoft.Json;

[Skip]
public class ServerInfo
{
	public readonly string Name = "Custom";

	public readonly string Ip;

	public readonly ushort Port;

	public readonly bool UseDtls;

	public int Players;

	public int ConnectionFailures;

	[JsonIgnore]
	public string HttpUrl
	{
		get
		{
			if (Ip.IndexOf("://") == -1)
			{
				return $"https://{Ip}:{Port}/";
			}
			return $"{Ip}:{Port}/";
		}
	}

	public ServerInfo(string name, string ip, ushort port, bool useDtls = true)
	{
		Name = name;
		Ip = ip;
		Port = port;
		UseDtls = useDtls;
	}

	public override string ToString()
	{
		return $"{Name}: {Ip}:{Port} {UseDtls}";
	}

	public override int GetHashCode()
	{
		return Ip.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is ServerInfo serverInfo && serverInfo.Ip == Ip)
		{
			return serverInfo.Port == Port;
		}
		return false;
	}
}
