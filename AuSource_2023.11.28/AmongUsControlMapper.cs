using System.Collections.Generic;
using UnityEngine;

public class AmongUsControlMapper : MonoBehaviour
{
	public UiElement backButton;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, backButton, backButton, new List<UiElement> { backButton });
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}
}
