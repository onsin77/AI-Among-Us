using System;
using UnityEngine;

public class DisableScriptsForJoystick : MonoBehaviour
{
	public MonoBehaviour[] scripts;

	private void Start()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		OnInputMethodChanged();
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void OnInputMethodChanged()
	{
		MonoBehaviour[] array = scripts;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick;
		}
	}
}
