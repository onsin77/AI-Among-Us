using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;

public class LogicRoleSelectionNormal : LogicRoleSelection
{
	public LogicRoleSelectionNormal(GameManager manager)
		: base(manager)
	{
	}

	public override void OnGameStart()
	{
	}

	public override void OnGameEnd()
	{
	}

	public override void FixedUpdate()
	{
	}

	public override void OnDestroy()
	{
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	public override void AssignRolesForTeam(List<GameData.PlayerInfo> players, IGameOptions opts, RoleTeamTypes team, int teamMax, RoleTypes? defaultRole)
	{
		int rolesAssigned = 0;
		IEnumerable<RoleBehaviour> source = DestroyableSingleton<RoleManager>.Instance.AllRoles.Where((RoleBehaviour role) => role.TeamType == team && !RoleManager.IsGhostRole(role.Role));
		List<RoleTypes> list = new List<RoleTypes>();
		IRoleOptionsCollection roleOptions = opts.RoleOptions;
		foreach (RoleManager.RoleAssignmentData item in from role in source
			where roleOptions.GetChancePerGame(role.Role) == 100
			select new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role), 100))
		{
			while (item.Count-- > 0)
			{
				list.Add(item.Role.Role);
			}
		}
		AssignRolesFromList(players, teamMax, list, ref rolesAssigned);
		List<RoleManager.RoleAssignmentData> list2 = (from role in source
			let chance = roleOptions.GetChancePerGame(role.Role)
			where chance > 0 && chance < 100
			select new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role), chance)).ToList();
		list.Clear();
		foreach (RoleManager.RoleAssignmentData item2 in list2)
		{
			for (int num = 0; num < item2.Count; num++)
			{
				if (HashRandom.Next(101) < item2.Chance)
				{
					list.Add(item2.Role.Role);
				}
			}
		}
		AssignRolesFromList(players, teamMax, list, ref rolesAssigned);
		if (defaultRole.HasValue)
		{
			while (list.Count < players.Count && list.Count + rolesAssigned < teamMax)
			{
				list.Add(defaultRole.Value);
			}
			AssignRolesFromList(players, teamMax, list, ref rolesAssigned);
		}
	}

	private void AssignRolesFromList(List<GameData.PlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
	{
		while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
		{
			int index = HashRandom.FastNext(roleList.Count);
			RoleTypes roleType = roleList[index];
			roleList.RemoveAt(index);
			int index2 = HashRandom.FastNext(players.Count);
			players[index2].Object.RpcSetRole(roleType);
			players.RemoveAt(index2);
			rolesAssigned++;
		}
	}

	public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
	{
		if (AmongUsClient.Instance.AmHost & assignGhostRole)
		{
			DestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, specialRolesAllowed: true);
		}
	}
}
