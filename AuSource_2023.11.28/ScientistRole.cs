using TMPro;
using UnityEngine;

public class ScientistRole : RoleBehaviour
{
	public VitalsMinigame VitalsPrefab;

	private VitalsMinigame minigame;

	private float currentCharge;

	private float currentCooldown;

	public override bool IsDead => false;

	private bool IsCoolingDown => currentCooldown > 0f;

	private float RoleCooldownValue => GameManager.Instance.LogicOptions.GetScientistCooldown();

	private float RoleBatteryCharge => GameManager.Instance.LogicOptions.GetScientistBatteryCharge();

	public override void Initialize(PlayerControl player)
	{
		base.Initialize(player);
		Recharge();
	}

	public override void OnVotingComplete()
	{
		base.OnVotingComplete();
		if (Player.AmOwner && Player.AllTasksCompleted())
		{
			currentCooldown = 0f;
			Recharge();
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(currentCooldown, RoleCooldownValue);
		}
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)Player) || !Player.AmOwner || Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			return;
		}
		if ((Object)(object)minigame != (Object)null)
		{
			currentCharge -= Time.deltaTime;
			((TMP_Text)minigame.BatteryText).text = ((int)currentCharge).ToString();
			if (currentCharge <= 0f)
			{
				minigame.Close();
				if (Player.AllTasksCompleted())
				{
					currentCooldown = RoleCooldownValue;
				}
				else
				{
					RefreshAbilityButton();
				}
			}
		}
		else if (IsCoolingDown)
		{
			currentCooldown -= Time.deltaTime;
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(currentCooldown, RoleCooldownValue);
			if (currentCooldown <= 0f)
			{
				Recharge();
			}
		}
		else
		{
			RefreshAbilityButton();
		}
	}

	public bool Recharge()
	{
		float roleBatteryCharge = RoleBatteryCharge;
		bool result = currentCharge < roleBatteryCharge;
		currentCharge = roleBatteryCharge;
		RefreshAbilityButton();
		return result;
	}

	public override void UseAbility()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (currentCharge <= 0f || IsAffectedByComms || !PlayerControl.LocalPlayer.CanMove)
		{
			if ((Object)(object)minigame != (Object)null)
			{
				minigame.Close();
			}
			return;
		}
		Player.NetTransform.Halt();
		minigame = Object.Instantiate<VitalsMinigame>(VitalsPrefab);
		((Component)minigame).transform.SetParent(((Component)Camera.main).transform, false);
		((Component)minigame).transform.localPosition = new Vector3(0f, 0f, -50f);
		((Component)minigame.BatteryText).gameObject.SetActive(true);
		minigame.Begin(null);
	}

	public override bool CanUse(IUsable console)
	{
		return console is Console;
	}

	public override bool DidWin(GameOverReason gameOverReason)
	{
		return GameManager.Instance.DidHumansWin(gameOverReason);
	}

	private void RefreshAbilityButton()
	{
		if (currentCharge > 0f && !IsAffectedByComms)
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
		}
	}

	public override PlayerControl FindClosestTarget()
	{
		return null;
	}
}
