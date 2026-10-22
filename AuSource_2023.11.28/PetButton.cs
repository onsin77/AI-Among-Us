using UnityEngine;

public class PetButton : ActionButton
{
	public void SetTarget()
	{
		SetEnabled();
	}

	public override void DoClick()
	{
		if (((Behaviour)this).isActiveAndEnabled && canInteract && Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			PlayerControl.LocalPlayer.TryPet();
		}
	}
}
