using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GuardianAngelRole : CrewmateGhostRole
{
	private PlayerControl currentTarget;

	private float cooldownSecondsRemaining;

	private bool IsCoolingDown => cooldownSecondsRemaining > 0f;

	public override bool IsAffectedByComms => CommsSabotaged;

	public override void UseAbility()
	{
		if (((Behaviour)this).isActiveAndEnabled && Object.op_Implicit((Object)(object)currentTarget) && !IsCoolingDown && PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove && !IsAffectedByComms)
		{
			PlayerControl.LocalPlayer.CmdCheckProtect(currentTarget);
			SoundManager.Instance.PlaySound(UseSound, loop: false);
			SetPlayerTarget(null);
		}
	}

	public override void SetPlayerTarget(PlayerControl target)
	{
		RoleTeamTypes teamType = PlayerControl.LocalPlayer.Data.Role.TeamType;
		if ((Object)(object)target != (Object)(object)currentTarget && (Object)(object)currentTarget != (Object)null)
		{
			currentTarget.ToggleHighlight(active: false, teamType);
		}
		currentTarget = target;
		if (Object.op_Implicit((Object)(object)currentTarget) && !target.Data.IsDead && !IsAffectedByComms)
		{
			currentTarget.ToggleHighlight(active: true, teamType);
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
		}
	}

	public override void OnVotingComplete()
	{
		base.OnVotingComplete();
		if (Player.AmOwner)
		{
			cooldownSecondsRemaining = 0f;
			UpdateCooldown();
		}
	}

	public override void SetCooldown()
	{
		cooldownSecondsRemaining = GameManager.Instance.LogicOptions.GetGuardianAngelCooldown();
		UpdateCooldown();
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)Player) && Player.AmOwner && !Object.op_Implicit((Object)(object)MeetingHud.Instance) && cooldownSecondsRemaining > 0f)
		{
			cooldownSecondsRemaining -= Time.deltaTime;
			UpdateCooldown();
		}
	}

	private void UpdateCooldown()
	{
		DestroyableSingleton<HudManager>.Instance.AbilityButton.SetCoolDown(cooldownSecondsRemaining, GameManager.Instance.LogicOptions.GetGuardianAngelCooldown());
	}

	public override PlayerControl FindClosestTarget()
	{
		List<PlayerControl> playersInAbilityRangeSorted = GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList(), ignoreColliders: true);
		if (playersInAbilityRangeSorted.Count <= 0)
		{
			return null;
		}
		playersInAbilityRangeSorted.Sort((PlayerControl a, PlayerControl b) =>
		{
			if (a.Data == null || b.Data == null || (Object)(object)a.Data.Role == (Object)null || (Object)(object)b.Data.Role == (Object)null)
			{
				return 0;
			}
			if (a.Data.Role.IsImpostor && !b.Data.Role.IsImpostor)
			{
				return 1;
			}
			return (!a.Data.Role.IsImpostor && b.Data.Role.IsImpostor) ? (-1) : 0;
		});
		return playersInAbilityRangeSorted[0];
	}

	public override void AppendTaskHint(StringBuilder taskStringBuilder)
	{
		RoleBehaviour.AppendTaskHint(this, taskStringBuilder);
	}
}
