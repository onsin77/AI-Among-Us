using UnityEngine;

public class SabotageButton : ActionButton
{
	public override void DoClick()
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && !PlayerControl.LocalPlayer.inVent && GameManager.Instance.SabotagesEnabled())
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Sabotage
			});
		}
	}

	public void Refresh()
	{
		if ((Object)(object)GameManager.Instance == (Object)null)
		{
			ToggleVisible(visible: false);
			SetDisabled();
		}
		else if (PlayerControl.LocalPlayer.inVent || !GameManager.Instance.SabotagesEnabled() || PlayerControl.LocalPlayer.petting)
		{
			ToggleVisible(PlayerControl.LocalPlayer.Data.Role.IsImpostor && GameManager.Instance.SabotagesEnabled());
			SetDisabled();
		}
		else
		{
			SetEnabled();
		}
	}
}
