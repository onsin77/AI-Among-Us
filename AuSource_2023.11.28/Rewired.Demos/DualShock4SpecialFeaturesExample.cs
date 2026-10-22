using System;
using System.Collections.Generic;
using Rewired.ControllerExtensions;
using Rewired.Interfaces;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class DualShock4SpecialFeaturesExample : MonoBehaviour
{
	private class Touch
	{
		public GameObject go;

		public int touchId = -1;
	}

	private const int maxTouches = 2;

	public int playerId;

	public Transform touchpadTransform;

	public GameObject lightObject;

	public Transform accelerometerTransform;

	private List<Touch> touches;

	private Queue<Touch> unusedTouches;

	private bool isFlashing;

	private GUIStyle textStyle;

	private Player player => ReInput.players.GetPlayer(playerId);

	private void Awake()
	{
		InitializeTouchObjects();
	}

	private void Update()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!ReInput.isReady)
		{
			return;
		}
		IDualShock4Extension firstDS = GetFirstDS4(player);
		if (firstDS != null)
		{
			((Component)this).transform.rotation = firstDS.GetOrientation();
			HandleTouchpad(firstDS);
			Vector3 accelerometerValue = firstDS.GetAccelerometerValue();
			accelerometerTransform.LookAt(accelerometerTransform.position + accelerometerValue);
		}
		if (player.GetButtonDown("CycleLight"))
		{
			SetRandomLightColor();
		}
		if (player.GetButtonDown("ResetOrientation"))
		{
			ResetOrientation();
		}
		if (player.GetButtonDown("ToggleLightFlash"))
		{
			if (isFlashing)
			{
				StopLightFlash();
			}
			else
			{
				StartLightFlash();
			}
			isFlashing = !isFlashing;
		}
		if (player.GetButtonDown("VibrateLeft"))
		{
			((IControllerVibrator)firstDS).SetVibration(0, 1f, 1f);
		}
		if (player.GetButtonDown("VibrateRight"))
		{
			((IControllerVibrator)firstDS).SetVibration(1, 1f, 1f);
		}
	}

	private void OnGUI()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (textStyle == null)
		{
			textStyle = new GUIStyle(GUI.skin.label);
			textStyle.fontSize = 20;
			textStyle.wordWrap = true;
		}
		if (GetFirstDS4(player) != null)
		{
			GUILayout.BeginArea(new Rect(200f, 100f, (float)Screen.width - 400f, (float)Screen.height - 200f));
			GUILayout.Label("Rotate the Dual Shock 4 to see the model rotate in sync.", textStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Touch the touchpad to see them appear on the model.", textStyle, Array.Empty<GUILayoutOption>());
			ActionElementMap firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, "ResetOrientation", true);
			if (firstElementMapWithAction != null)
			{
				GUILayout.Label("Press " + firstElementMapWithAction.elementIdentifierName + " to reset the orientation. Hold the gamepad facing the screen with sticks pointing up and press the button.", textStyle, Array.Empty<GUILayoutOption>());
			}
			firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, "CycleLight", true);
			if (firstElementMapWithAction != null)
			{
				GUILayout.Label("Press " + firstElementMapWithAction.elementIdentifierName + " to change the light color.", textStyle, Array.Empty<GUILayoutOption>());
			}
			firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, "ToggleLightFlash", true);
			if (firstElementMapWithAction != null)
			{
				GUILayout.Label("Press " + firstElementMapWithAction.elementIdentifierName + " to start or stop the light flashing.", textStyle, Array.Empty<GUILayoutOption>());
			}
			firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, "VibrateLeft", true);
			if (firstElementMapWithAction != null)
			{
				GUILayout.Label("Press " + firstElementMapWithAction.elementIdentifierName + " vibrate the left motor.", textStyle, Array.Empty<GUILayoutOption>());
			}
			firstElementMapWithAction = player.controllers.maps.GetFirstElementMapWithAction((ControllerType)2, "VibrateRight", true);
			if (firstElementMapWithAction != null)
			{
				GUILayout.Label("Press " + firstElementMapWithAction.elementIdentifierName + " vibrate the right motor.", textStyle, Array.Empty<GUILayoutOption>());
			}
			GUILayout.EndArea();
		}
	}

	private void ResetOrientation()
	{
		IDualShock4Extension firstDS = GetFirstDS4(player);
		if (firstDS != null)
		{
			firstDS.ResetOrientation();
		}
	}

	private void SetRandomLightColor()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		IDualShock4Extension firstDS = GetFirstDS4(player);
		if (firstDS != null)
		{
			Color val = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
			firstDS.SetLightColor(val);
			((Renderer)lightObject.GetComponent<MeshRenderer>()).material.color = val;
		}
	}

	private void StartLightFlash()
	{
		IDualShock4Extension firstDS = GetFirstDS4(player);
		DualShock4Extension val = (DualShock4Extension)(object)((firstDS is DualShock4Extension) ? firstDS : null);
		if (val != null)
		{
			val.SetLightFlash(0.5f, 0.5f);
		}
	}

	private void StopLightFlash()
	{
		IDualShock4Extension firstDS = GetFirstDS4(player);
		DualShock4Extension val = (DualShock4Extension)(object)((firstDS is DualShock4Extension) ? firstDS : null);
		if (val != null)
		{
			val.StopLightFlash();
		}
	}

	private IDualShock4Extension GetFirstDS4(Player player)
	{
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			IDualShock4Extension extension = ((Controller)joystick).GetExtension<IDualShock4Extension>();
			if (extension != null)
			{
				return extension;
			}
		}
		return null;
	}

	private void InitializeTouchObjects()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		touches = new List<Touch>(2);
		unusedTouches = new Queue<Touch>(2);
		for (int i = 0; i < 2; i++)
		{
			Touch touch = new Touch();
			touch.go = GameObject.CreatePrimitive((PrimitiveType)0);
			touch.go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			touch.go.transform.SetParent(touchpadTransform, true);
			((Renderer)touch.go.GetComponent<MeshRenderer>()).material.color = ((i == 0) ? Color.red : Color.green);
			touch.go.SetActive(false);
			unusedTouches.Enqueue(touch);
		}
	}

	private void HandleTouchpad(IDualShock4Extension ds4)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		for (int num = touches.Count - 1; num >= 0; num--)
		{
			Touch touch = touches[num];
			if (!ds4.IsTouchingByTouchId(touch.touchId))
			{
				touch.go.SetActive(false);
				unusedTouches.Enqueue(touch);
				touches.RemoveAt(num);
			}
		}
		Vector2 val = default;
		for (int i = 0; i < ds4.maxTouches; i++)
		{
			if (ds4.IsTouching(i))
			{
				int touchId = ds4.GetTouchId(i);
				Touch touch2 = touches.Find((Touch x) => x.touchId == touchId);
				if (touch2 == null)
				{
					touch2 = unusedTouches.Dequeue();
					touches.Add(touch2);
				}
				touch2.touchId = touchId;
				touch2.go.SetActive(true);
				ds4.GetTouchPosition(i, ref val);
				touch2.go.transform.localPosition = new Vector3(val.x - 0.5f, 0.5f + touch2.go.transform.localScale.y * 0.5f, val.y - 0.5f);
			}
		}
	}
}
