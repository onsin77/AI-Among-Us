using UnityEngine;

public class AdminButton : ActionButton
{
	[SerializeField]
	private Animator MyAnimator;

	[SerializeField]
	private AudioClip RevealSFX;

	private bool useable;

	public override void DoClick()
	{
		if (useable)
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(GameManager.Instance.GetMapOptions());
		}
	}

	public void Refresh()
	{
		if ((Object)(object)GameManager.Instance == (Object)null || !(GameManager.Instance.LogicFlow is LogicGameFlowHnS logicGameFlowHnS))
		{
			ToggleVisible(visible: false);
			SetDisabled();
			return;
		}
		useable = logicGameFlowHnS.SeekerAdminMapEnabled(PlayerControl.LocalPlayer);
		if (!useable || (Object.op_Implicit((Object)(object)MapBehaviour.Instance) && MapBehaviour.Instance.IsOpen))
		{
			ToggleVisible(visible: false);
			SetDisabled();
		}
		else
		{
			ToggleVisible(visible: true);
			SetEnabled();
		}
	}

	public void AnimEvent_TurnOffAnimator()
	{
		((Behaviour)MyAnimator).enabled = false;
	}

	public void AnimEvent_PlayRevealSFX()
	{
		SoundManager.Instance.PlaySound(RevealSFX, loop: false);
	}
}
