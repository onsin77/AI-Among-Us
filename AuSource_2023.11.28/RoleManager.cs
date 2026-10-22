using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;

public class RoleManager : DestroyableSingleton<RoleManager>
{
	public class RoleAssignmentData
	{
		public RoleBehaviour Role;

		public int Count;

		public int Chance;

		public RoleAssignmentData(RoleBehaviour role, int count, int chance)
		{
			Role = role;
			Count = count;
			Chance = chance;
		}
	}

	private static readonly HashSet<RoleTypes> GhostRoles = new HashSet<RoleTypes>
	{
		RoleTypes.GuardianAngel,
		RoleTypes.CrewmateGhost,
		RoleTypes.ImpostorGhost
	};

	public RoleBehaviour[] AllRoles;

	public RoleEffectAnimation shapeshiftAnim;

	public RoleEffectAnimation protectAnim;

	public RoleEffectAnimation protectLoopAnim;

	public RoleBehaviour GetRole(RoleTypes roleType)
	{
		RoleBehaviour[] allRoles = AllRoles;
		foreach (RoleBehaviour roleBehaviour in allRoles)
		{
			if (roleBehaviour.Role == roleType)
			{
				return roleBehaviour;
			}
		}
		return null;
	}

	public void SetRole(PlayerControl targetPlayer, RoleTypes roleType)
	{
		if (!Object.op_Implicit((Object)(object)targetPlayer))
		{
			return;
		}
		GameData.PlayerInfo data = targetPlayer.Data;
		if (data == null)
		{
			Debug.LogError((object)("It shouldn't be possible, but " + ((Object)targetPlayer).name + " still doesn't have PlayerData during role selection."));
			return;
		}
		if (Object.op_Implicit((Object)(object)data.Role))
		{
			data.Role.Deinitialize(targetPlayer);
			Object.Destroy((Object)(object)((Component)data.Role).gameObject);
		}
		RoleBehaviour roleBehaviour = Object.Instantiate<RoleBehaviour>(AllRoles.First((RoleBehaviour r) => r.Role == roleType), ((Component)GameData.Instance).transform);
		roleBehaviour.Initialize(targetPlayer);
		targetPlayer.Data.Role = roleBehaviour;
		targetPlayer.Data.RoleType = roleType;
		if (roleType != RoleTypes.ImpostorGhost && roleType != RoleTypes.CrewmateGhost)
		{
			targetPlayer.Data.RoleWhenAlive = roleType;
		}
		roleBehaviour.AdjustTasks(targetPlayer);
		if (roleBehaviour.IsDead && !targetPlayer.Data.IsDead)
		{
			targetPlayer.Die(DeathReason.Kill, assignGhostRole: false);
		}
		else if (!roleBehaviour.IsDead && targetPlayer.Data.IsDead)
		{
			targetPlayer.Revive();
		}
	}

	public void SelectRoles()
	{
		List<ClientData> list = new List<ClientData>();
		AmongUsClient.Instance.GetAllClients(list);
		List<GameData.PlayerInfo> list2 = (from c in list
			where (Object)(object)c.Character != (Object)null
			where c.Character.Data != null
			where !c.Character.Data.Disconnected && !c.Character.Data.IsDead
			orderby c.Id
			select c.Character.Data).ToList();
		foreach (GameData.PlayerInfo allPlayer in GameData.Instance.AllPlayers)
		{
			if ((Object)(object)allPlayer.Object != (Object)null && allPlayer.Object.isDummy)
			{
				list2.Add(allPlayer);
			}
		}
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
		int numImpostors = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(list2.Count);
		DebugRoleAssignments(list2, ref numImpostors);
		GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Impostor, numImpostors, RoleTypes.Impostor);
		GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, RoleTypes.Crewmate);
	}

	public void AssignRoleOnDeath(PlayerControl player, bool specialRolesAllowed)
	{
		if (!((Object)(object)player == (Object)null) && player.Data.IsDead)
		{
			if (!player.Data.Role.IsImpostor & specialRolesAllowed)
			{
				TryAssignSpecialGhostRoles(player);
			}
			if (!IsGhostRole(player.Data.Role.Role))
			{
				player.RpcSetRole(player.Data.Role.DefaultGhostRole);
			}
		}
	}

	private void DebugRoleAssignments(List<GameData.PlayerInfo> players, ref int numImpostors)
	{
	}

	private static void TryAssignSpecialGhostRoles(PlayerControl player)
	{
		RoleTypes roleTypes = RoleTypes.GuardianAngel;
		int num = PlayerControl.AllPlayerControls.Count((PlayerControl pc) => pc.Data.IsDead && !pc.Data.Role.IsImpostor);
		IRoleOptionsCollection roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
		if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
		{
			player.RpcSetRole(roleTypes);
		}
		else if (num <= roleOptions.GetNumPerGame(roleTypes))
		{
			int chancePerGame = roleOptions.GetChancePerGame(roleTypes);
			if (HashRandom.Next(101) < chancePerGame)
			{
				player.RpcSetRole(roleTypes);
			}
		}
	}

	public static bool IsGhostRole(RoleTypes role)
	{
		return GhostRoles.Contains(role);
	}

	public static bool IsImpostorRole(RoleTypes roleType)
	{
		RoleBehaviour roleBehaviour = DestroyableSingleton<RoleManager>.Instance.AllRoles.First((RoleBehaviour r) => r.Role == roleType);
		if ((Object)(object)roleBehaviour != (Object)null)
		{
			return roleBehaviour.TeamType == RoleTeamTypes.Impostor;
		}
		return false;
	}
}
