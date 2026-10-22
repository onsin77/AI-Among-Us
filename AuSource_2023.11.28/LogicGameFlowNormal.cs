using System.Linq;
using AmongUs.Data;
using Hazel;
using UnityEngine;

public class LogicGameFlowNormal : LogicGameFlow
{
	public LogicGameFlowNormal(GameManager manager)
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

	protected void EndGameForSabotage()
	{
		if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Manager.RpcEndGame(GameOverReason.ImpostorBySabotage, !DataManager.Player.Ads.HasPurchasedAdRemoval);
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverSabotage));
		}
	}

	public override bool IsGameOverDueToDeath()
	{
		var (num, num2, num3) = GetPlayerCounts();
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			return true;
		}
		if (num <= num2)
		{
			return true;
		}
		return false;
	}

	public override void CheckEndCriteria()
	{
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.LifeSupp, out var value))
		{
			LifeSuppSystemType lifeSuppSystemType = (LifeSuppSystemType)value;
			if (lifeSuppSystemType.Countdown < 0f)
			{
				EndGameForSabotage();
				lifeSuppSystemType.Countdown = 10000f;
			}
		}
		foreach (ISystemType value2 in ShipStatus.Instance.Systems.Values)
		{
			if (value2 is ICriticalSabotage { Countdown: <0f } criticalSabotage)
			{
				EndGameForSabotage();
				criticalSabotage.ClearSabotage();
			}
		}
		var (num, num2, num3) = GetPlayerCounts();
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				GameOverReason endReason = GameOverReason.HumansByVote;
				Manager.RpcEndGame(endReason, !DataManager.Player.Ads.HasPurchasedAdRemoval);
			}
			else if (!Object.op_Implicit((Object)(object)Minigame.Instance))
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorDead));
				Manager.ReviveEveryoneFreeplay();
			}
		}
		else if (num <= num2)
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				GameOverReason endReason2 = TempData.LastDeathReason switch
				{
					DeathReason.Kill => GameOverReason.ImpostorByKill, 
					DeathReason.Exile => GameOverReason.ImpostorByVote, 
					_ => GameOverReason.HumansDisconnect, 
				};
				Manager.RpcEndGame(endReason2, !DataManager.Player.Ads.HasPurchasedAdRemoval);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorKills));
				Manager.ReviveEveryoneFreeplay();
			}
		}
		else if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Manager.CheckEndGameViaTasks();
		}
		else if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
		{
			DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin));
			ShipStatus.Instance.Begin();
		}
	}
}
