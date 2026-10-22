using UnityEngine;

public abstract class LogicGameFlow : GameLogicComponent
{
	public LogicGameFlow(GameManager manager)
		: base(manager)
	{
	}

	public abstract void CheckEndCriteria();

	public abstract bool IsGameOverDueToDeath();

	protected (int aliveHumans, int aliveImpostor, int impostorCount) GetPlayerCounts()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (playerInfo == null || playerInfo.Disconnected || (Object)(object)playerInfo.Role == (Object)null)
			{
				continue;
			}
			if (playerInfo.Role.IsImpostor)
			{
				num3++;
			}
			if (!playerInfo.IsDead)
			{
				if (playerInfo.Role.IsImpostor)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
			else if (playerInfo.Role is ImpostorGhostRole { WasManuallyPicked: not false })
			{
				num2++;
			}
		}
		return (aliveHumans: num, aliveImpostor: num2, impostorCount: num3);
	}
}
