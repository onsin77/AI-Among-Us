using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using UnityEngine;

public class ImpostorRole : RoleBehaviour
{
	public override bool IsDead => false;

	public override void Deinitialize(PlayerControl targetPlayer)
	{
		PlayerTask playerTask = targetPlayer.myTasks.FirstOrDefault((PlayerTask t) => ((Object)t).name == "ImpostorRole");
		if (Object.op_Implicit((Object)(object)playerTask))
		{
			targetPlayer.myTasks.Remove(playerTask);
			Object.Destroy((Object)(object)((Component)playerTask).gameObject);
		}
	}

	public override void SpawnTaskHeader(PlayerControl playerControl)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)playerControl != (Object)(object)PlayerControl.LocalPlayer))
		{
			ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl);
			switch (GameOptionsManager.Instance.CurrentGameOptions.GameMode)
			{
			case GameModes.HideNSeek:
				orCreateTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RuleOneImpostor);
				break;
			case GameModes.Normal:
				orCreateTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorTask) + "\r\n" + Palette.ImpostorRed.ToTextColor() + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks) + "</color>";
				break;
			}
		}
	}

	public override bool CanUse(IUsable usable)
	{
		if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
		{
			return false;
		}
		Console console = usable as Console;
		if ((Object)(object)console != (Object)null)
		{
			return console.AllowImpostor;
		}
		return true;
	}

	public override bool DidWin(GameOverReason gameOverReason)
	{
		return GameManager.Instance.DidImpostorsWin(gameOverReason);
	}

	protected override bool IsValidTarget(GameData.PlayerInfo target)
	{
		if (!target.Role.CanBeKilled)
		{
			return false;
		}
		return base.IsValidTarget(target);
	}

	public override PlayerControl FindClosestTarget()
	{
		List<PlayerControl> playersInAbilityRangeSorted = GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList());
		if (playersInAbilityRangeSorted.Count <= 0)
		{
			return null;
		}
		return playersInAbilityRangeSorted[0];
	}

	public override void AppendTaskHint(StringBuilder taskStringBuilder)
	{
	}
}
