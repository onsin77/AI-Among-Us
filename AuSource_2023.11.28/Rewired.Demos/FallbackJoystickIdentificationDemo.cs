using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class FallbackJoystickIdentificationDemo : MonoBehaviour
{
	private const float windowWidth = 250f;

	private const float windowHeight = 250f;

	private const float inputDelay = 1f;

	private bool identifyRequired;

	private Queue<Joystick> joysticksToIdentify;

	private float nextInputAllowedTime;

	private GUIStyle style;

	private void Awake()
	{
		if (ReInput.unityJoystickIdentificationRequired)
		{
			ReInput.ControllerConnectedEvent += JoystickConnected;
			ReInput.ControllerDisconnectedEvent += JoystickDisconnected;
			IdentifyAllJoysticks();
		}
	}

	private void JoystickConnected(ControllerStatusChangedEventArgs args)
	{
		IdentifyAllJoysticks();
	}

	private void JoystickDisconnected(ControllerStatusChangedEventArgs args)
	{
		IdentifyAllJoysticks();
	}

	public void IdentifyAllJoysticks()
	{
		Reset();
		if (ReInput.controllers.joystickCount != 0)
		{
			Joystick[] joysticks = ReInput.controllers.GetJoysticks();
			if (joysticks != null)
			{
				identifyRequired = true;
				joysticksToIdentify = new Queue<Joystick>(joysticks);
				SetInputDelay();
			}
		}
	}

	private void SetInputDelay()
	{
		nextInputAllowedTime = Time.time + 1f;
	}

	private void OnGUI()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected Obj, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (!identifyRequired)
		{
			return;
		}
		if (joysticksToIdentify == null || joysticksToIdentify.Count == 0)
		{
			Reset();
			return;
		}
		Rect val = new Rect((float)Screen.width * 0.5f - 125f, (float)Screen.height * 0.5f - 125f, 250f, 250f);
		GUILayout.Window(0, val, (WindowFunction)DrawDialogWindow, "Joystick Identification Required", Array.Empty<GUILayoutOption>());
		GUI.FocusWindow(0);
		if (!(Time.time < nextInputAllowedTime) && ReInput.controllers.SetUnityJoystickIdFromAnyButtonOrAxisPress(((Controller)joysticksToIdentify.Peek()).id, 0.8f, false))
		{
			joysticksToIdentify.Dequeue();
			SetInputDelay();
			if (joysticksToIdentify.Count == 0)
			{
				Reset();
			}
		}
	}

	private void DrawDialogWindow(int windowId)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		if (identifyRequired)
		{
			if (style == null)
			{
				style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
			}
			GUILayout.Space(15f);
			GUILayout.Label("A joystick has been attached or removed. You will need to identify each joystick by pressing a button on the controller listed below:", style, Array.Empty<GUILayoutOption>());
			Joystick val = joysticksToIdentify.Peek();
			GUILayout.Label("Press any button on \"" + ((Controller)val).name + "\" now.", style, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Skip", Array.Empty<GUILayoutOption>()))
			{
				joysticksToIdentify.Dequeue();
			}
		}
	}

	private void Reset()
	{
		joysticksToIdentify = null;
		identifyRequired = false;
	}
}
