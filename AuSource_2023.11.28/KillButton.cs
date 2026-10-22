using System.Collections.Generic;
using UnityEngine;

public class KillButton : ActionButton
{
	private PlayerControl currentTarget;

	public override void DoClick()
	{
		if (((Behaviour)this).isActiveAndEnabled && Object.op_Implicit((Object)(object)currentTarget) && !isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove)
		{
			PlayerControl.LocalPlayer.CmdCheckMurder(currentTarget);
			SetTarget(null);
		}
	}

	public void CheckClick(PlayerControl target)
	{
		if (Object.op_Implicit((Object)(object)currentTarget) && (Object)(object)currentTarget == (Object)(object)target)
		{
			DoClick();
		}
		else if (Object.op_Implicit((Object)(object)currentTarget) && (Object)(object)currentTarget != (Object)(object)target && PlayerControl.LocalPlayer.Data.Role.GetPlayersInAbilityRangeSorted(new List<PlayerControl>()).Contains(target))
		{
			SetTarget(target);
			DoClick();
		}
	}

	public void SetTarget(PlayerControl target)
	{
		if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.Data != null && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer.Data.Role))
		{
			RoleTeamTypes teamType = PlayerControl.LocalPlayer.Data.Role.TeamType;
			if (Object.op_Implicit((Object)(object)currentTarget) && (Object)(object)currentTarget != (Object)(object)target)
			{
				currentTarget.ToggleHighlight(active: false, teamType);
			}
			currentTarget = target;
			if (Object.op_Implicit((Object)(object)currentTarget))
			{
				currentTarget.ToggleHighlight(active: true, teamType);
				SetEnabled();
			}
			else
			{
				SetDisabled();
			}
		}
	}

	private void Start()
	{
		isCoolingDown = true;
		graphic.SetCooldownNormalizedUvs();
		SetTarget(null);
	}
}
