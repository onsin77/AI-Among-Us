using System.Collections.Generic;
using AmongUs.GameOptions;

public abstract class LogicRoleSelection : GameLogicComponent
{
	public LogicRoleSelection(GameManager manager)
		: base(manager)
	{
	}

	public abstract void AssignRolesForTeam(List<GameData.PlayerInfo> players, IGameOptions opts, RoleTeamTypes team, int teamMax, RoleTypes? defaultRole);

	public abstract void OnPlayerDeath(PlayerControl player, bool assignGhostRole);
}
