using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerNavMenu : MonoBehaviour
{
	public bool openInOnEnable;

	public bool gridNavigation;

	public bool trySelectAny;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private bool isOpen;

	private void Start()
	{
		if (!openInOnEnable)
		{
			OpenMenu();
		}
	}

	private void OnEnable()
	{
		if (!openInOnEnable)
		{
			return;
		}
		foreach (UiElement item in ControllerSelectable)
		{
			if (!((Behaviour)item).isActiveAndEnabled && ((Behaviour)item).enabled && ((Component)item).gameObject.activeSelf)
			{
				((Component)item).gameObject.SetActive(false);
				((Component)item).gameObject.SetActive(true);
			}
		}
		OpenMenu();
	}

	private IEnumerator OpenCoroutine()
	{
		yield return null;
		OpenMenu();
	}

	private void OnDisable()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		CloseMenu();
	}

	public void OpenMenu(bool force = false)
	{
		if (!(!isOpen | force) || !Object.op_Implicit((Object)(object)ControllerManager.Instance))
		{
			return;
		}
		UiElement uiElement = DefaultButtonSelected;
		if ((!Object.op_Implicit((Object)(object)uiElement) || !((Behaviour)uiElement).isActiveAndEnabled) && trySelectAny)
		{
			foreach (UiElement item in ControllerSelectable)
			{
				if (Object.op_Implicit((Object)(object)item) && ((Behaviour)item).isActiveAndEnabled)
				{
					uiElement = item;
				}
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, BackButton, uiElement, ControllerSelectable, gridNavigation);
		isOpen = true;
	}

	public void CloseMenu()
	{
		if (isOpen)
		{
			ControllerManager.Instance.CloseOverlayMenu(((Object)((Component)this).gameObject).name);
			isOpen = false;
		}
	}
}
