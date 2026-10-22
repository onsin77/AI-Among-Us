using Beebyte.Obfuscator;

[Skip]
public interface IRegionInfo
{
	string Name { get; }

	string PingServer { get; }

	ServerInfo[] Servers { get; }

	StringNames TranslateName { get; }

	string TargetServer { get; }

	IRegionInfo Duplicate();

	bool Validate();
}
