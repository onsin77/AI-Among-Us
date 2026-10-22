using UnityEngine;

public class TabGroup : MonoBehaviour
{
	public SpriteRenderer Button;

	public ButtonRolloverHandler Rollover;

	public GameObject Content;

	internal void Close()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Button.color = Palette.DisabledGrey;
		Rollover.OutColor = Palette.DisabledGrey;
		Content.SetActive(false);
	}

	internal void Open()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick && (Object)(object)ControllerManager.Instance.CurrentUiState.CurrentSelection != (Object)null && (Object)(object)((Component)ControllerManager.Instance.CurrentUiState.CurrentSelection).gameObject == (Object)(object)((Component)Rollover).gameObject)
		{
			Button.color = Color.white;
		}
		Rollover.OutColor = Color.white;
		Content.SetActive(true);
	}
}
