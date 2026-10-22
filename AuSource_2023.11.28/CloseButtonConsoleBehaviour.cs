using System;
using UnityEngine;

public class CloseButtonConsoleBehaviour : MonoBehaviour
{
	public bool keepActiveOnConsoles;

	private void Start()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		OnInputMethodChanged();
	}

	private void SetActive(bool active)
	{
		((Component)this).gameObject.SetActive(active || keepActiveOnConsoles);
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void OnInputMethodChanged()
	{
		SetActive(ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick);
	}

	public void TryActivate()
	{
		OnInputMethodChanged();
	}
}
