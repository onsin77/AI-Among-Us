using UnityEngine;

public class ReportButton : ActionButton
{
	public void SetActive(bool isActive)
	{
		if ((Object)(object)GameManager.Instance == (Object)null)
		{
			SetDisabled();
			ToggleVisible(visible: false);
		}
		else if (GameManager.Instance.CanReportBodies())
		{
			if (isActive)
			{
				SetEnabled();
			}
			else
			{
				SetDisabled();
			}
		}
		else
		{
			ToggleVisible(visible: false);
		}
	}

	public override void DoClick()
	{
		if (((Behaviour)this).isActiveAndEnabled && GameManager.Instance.CanReportBodies())
		{
			PlayerControl.LocalPlayer.ReportClosest();
		}
	}
}
