using System.Text;
using UnityEngine;

public class CrewmateGhostRole : RoleBehaviour
{
	public Minigame HauntMenu;

	public override bool IsDead => true;

	public override bool IsAffectedByComms => false;

	public override bool CanUse(IUsable console)
	{
		return console is Console;
	}

	public override bool DidWin(GameOverReason gameOverReason)
	{
		return GameManager.Instance.DidHumansWin(gameOverReason);
	}

	public override void UseAbility()
	{
		if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			if (Minigame.Instance is HauntMenuMinigame)
			{
				Minigame.Instance.Close();
			}
		}
		else
		{
			Minigame minigame = Object.Instantiate<Minigame>(HauntMenu);
			((Component)minigame).transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance.AbilityButton).transform, false);
			((Component)minigame).transform.SetLocalZ(-5f);
			minigame.Begin(null);
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
		}
	}

	public override void SpawnTaskHeader(PlayerControl playerControl)
	{
		if (!((Object)(object)playerControl != (Object)(object)PlayerControl.LocalPlayer))
		{
			if (GameManager.Instance.LogicOptions.GetGhostsDoTasks())
			{
				PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl).Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks);
				return;
			}
			playerControl.ClearTasks();
			PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl).Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks);
		}
	}

	public override void AppendTaskHint(StringBuilder taskStringBuilder)
	{
	}
}
