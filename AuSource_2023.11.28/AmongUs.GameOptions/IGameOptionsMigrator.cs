namespace AmongUs.GameOptions;

public interface IGameOptionsMigrator
{
	bool TryMigrate(IGameOptions previousVerison, out IGameOptions updatedVersion);
}
