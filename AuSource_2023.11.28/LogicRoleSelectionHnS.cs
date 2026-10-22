using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;

public class LogicRoleSelectionHnS : LogicRoleSelection
{
	private HideAndSeekManager hnsManager;

	public LogicRoleSelectionHnS(HideAndSeekManager manager)
		: base(manager)
	{
		hnsManager = manager;
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
		if (team == RoleTeamTypes.Crewmate)
		{
			players.ForEach((GameData.PlayerInfo player) =>
			{
				player.Object.RpcSetRole(RoleTypes.Engineer);
			});
			return;
		}
		bool flag = !AmongUsClient.Instance.IsGamePublic;
		if ((hnsManager.LogicOptionsHnS.HasImpostorPlayerID() && hnsManager.LogicOptionsHnS.ValidateImpostorPlayerID(players)) & flag)
		{
			GameData.PlayerInfo playerInfo = players.Find((GameData.PlayerInfo p) => p.PlayerId == hnsManager.LogicOptionsHnS.ImpostorPlayerID());
			playerInfo.Object.RpcSetRole(RoleTypes.Impostor);
			players.Remove(playerInfo);
		}
		else
		{
			PickSeekersRoundRobin(players, teamMax);
		}
	}

	private void PickSeekersRoundRobin(List<GameData.PlayerInfo> players, int teamMax)
	{
		for (int i = 0; i < teamMax; i++)
		{
			if (players.Count <= 0)
			{
				break;
			}
			PseudoRandomList<GameData.PlayerInfo> pseudoRandomList = new PseudoRandomList<GameData.PlayerInfo>(AmongUsClient.Instance.GameId);
			pseudoRandomList.AddRange(players);
			for (int j = 0; j < TempData.RoundsPlayedInSession; j++)
			{
				pseudoRandomList.PickRandom();
			}
			GameData.PlayerInfo playerInfo = pseudoRandomList.PickRandom();
			playerInfo.Object.RpcSetRole(RoleTypes.Impostor);
			players.Remove(playerInfo);
		}
	}

	public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
	{
		if (AmongUsClient.Instance.AmHost & assignGhostRole)
		{
			DestroyableSingleton<RoleManager>.Instance.AssignRoleOnDeath(player, specialRolesAllowed: false);
		}
	}
}
