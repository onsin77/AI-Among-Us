using Hazel;

namespace AmongUs.GameOptions;

public class GameOptionsMigratorV06toV07 : IGameOptionsMigrator
{
	private readonly ILogger logger;

	public GameOptionsMigratorV06toV07(ILogger logger)
	{
		this.logger = logger;
	}

	public bool TryMigrate(IGameOptions previousVersion, out IGameOptions updatedVersion)
	{
		updatedVersion = null;
		NormalGameOptionsV07 normalGameOptionsV = new NormalGameOptionsV07(logger);
		if (!(previousVersion is GameOptionsData gameOptionsData))
		{
			logger.WriteError($"GameOptionsMigratorV06toV07 expects previous version = 6, but got {previousVersion.Version}");
			return false;
		}
		normalGameOptionsV.MaxPlayers = gameOptionsData.MaxPlayers;
		normalGameOptionsV.Keywords = gameOptionsData.Keywords;
		normalGameOptionsV.MapId = gameOptionsData.MapId;
		normalGameOptionsV.PlayerSpeedMod = gameOptionsData.PlayerSpeedMod;
		normalGameOptionsV.CrewLightMod = gameOptionsData.CrewLightMod;
		normalGameOptionsV.ImpostorLightMod = gameOptionsData.ImpostorLightMod;
		normalGameOptionsV.KillCooldown = gameOptionsData.KillCooldown;
		normalGameOptionsV.NumCommonTasks = gameOptionsData.NumCommonTasks;
		normalGameOptionsV.NumLongTasks = gameOptionsData.NumLongTasks;
		normalGameOptionsV.NumShortTasks = gameOptionsData.NumShortTasks;
		normalGameOptionsV.NumEmergencyMeetings = gameOptionsData.NumEmergencyMeetings;
		normalGameOptionsV.EmergencyCooldown = gameOptionsData.EmergencyCooldown;
		normalGameOptionsV.NumImpostors = gameOptionsData.NumImpostors;
		normalGameOptionsV.GhostsDoTasks = gameOptionsData.GhostsDoTasks;
		normalGameOptionsV.KillDistance = gameOptionsData.KillDistance;
		normalGameOptionsV.DiscussionTime = gameOptionsData.DiscussionTime;
		normalGameOptionsV.VotingTime = gameOptionsData.VotingTime;
		normalGameOptionsV.ConfirmImpostor = gameOptionsData.ConfirmImpostor;
		normalGameOptionsV.VisualTasks = gameOptionsData.VisualTasks;
		normalGameOptionsV.AnonymousVotes = gameOptionsData.AnonymousVotes;
		normalGameOptionsV.TaskBarMode = gameOptionsData.TaskBarMode;
		normalGameOptionsV.IsDefaults = gameOptionsData.IsDefaults;
		normalGameOptionsV.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, gameOptionsData.RoleOptions.GetNumPerGame(RoleTypes.Shapeshifter), gameOptionsData.RoleOptions.GetChancePerGame(RoleTypes.Shapeshifter));
		normalGameOptionsV.RoleOptions.SetRoleRate(RoleTypes.Scientist, gameOptionsData.RoleOptions.GetNumPerGame(RoleTypes.Scientist), gameOptionsData.RoleOptions.GetChancePerGame(RoleTypes.Scientist));
		normalGameOptionsV.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, gameOptionsData.RoleOptions.GetNumPerGame(RoleTypes.GuardianAngel), gameOptionsData.RoleOptions.GetChancePerGame(RoleTypes.GuardianAngel));
		normalGameOptionsV.RoleOptions.SetRoleRate(RoleTypes.Engineer, gameOptionsData.RoleOptions.GetNumPerGame(RoleTypes.Engineer), gameOptionsData.RoleOptions.GetChancePerGame(RoleTypes.Engineer));
		normalGameOptionsV.SetFloat(FloatOptionNames.ShapeshifterCooldown, gameOptionsData.GetFloat(FloatOptionNames.ShapeshifterCooldown));
		normalGameOptionsV.SetFloat(FloatOptionNames.ShapeshifterDuration, gameOptionsData.GetFloat(FloatOptionNames.ShapeshifterDuration));
		normalGameOptionsV.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, gameOptionsData.GetBool(BoolOptionNames.ShapeshifterLeaveSkin));
		normalGameOptionsV.SetFloat(FloatOptionNames.ScientistBatteryCharge, gameOptionsData.GetFloat(FloatOptionNames.ScientistBatteryCharge));
		normalGameOptionsV.SetFloat(FloatOptionNames.ScientistCooldown, gameOptionsData.GetFloat(FloatOptionNames.ScientistCooldown));
		normalGameOptionsV.SetFloat(FloatOptionNames.GuardianAngelCooldown, gameOptionsData.GetFloat(FloatOptionNames.GuardianAngelCooldown));
		normalGameOptionsV.SetFloat(FloatOptionNames.ProtectionDurationSeconds, gameOptionsData.GetFloat(FloatOptionNames.ProtectionDurationSeconds));
		normalGameOptionsV.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, gameOptionsData.GetBool(BoolOptionNames.ImpostorsCanSeeProtect));
		normalGameOptionsV.SetFloat(FloatOptionNames.EngineerCooldown, gameOptionsData.GetFloat(FloatOptionNames.EngineerCooldown));
		normalGameOptionsV.SetFloat(FloatOptionNames.EngineerInVentMaxTime, gameOptionsData.GetFloat(FloatOptionNames.EngineerInVentMaxTime));
		updatedVersion = normalGameOptionsV;
		return true;
	}
}
