using System;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class CustomControllerDemo : MonoBehaviour
{
	public int playerId;

	public string controllerTag;

	public bool useUpdateCallbacks;

	private int buttonCount;

	private int axisCount;

	private float[] axisValues;

	private bool[] buttonValues;

	private TouchJoystickExample[] joysticks;

	private TouchButtonExample[] buttons;

	private CustomController controller;

	[NonSerialized]
	private bool initialized;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		ScreenOrientation val = (ScreenOrientation)3;
		if ((int)SystemInfo.deviceType == 1 && Screen.orientation != val)
		{
			Screen.orientation = val;
		}
		Initialize();
	}

	private void Initialize()
	{
		ReInput.InputSourceUpdateEvent += OnInputSourceUpdate;
		joysticks = ((Component)this).GetComponentsInChildren<TouchJoystickExample>();
		buttons = ((Component)this).GetComponentsInChildren<TouchButtonExample>();
		axisCount = joysticks.Length * 2;
		buttonCount = buttons.Length;
		axisValues = new float[axisCount];
		buttonValues = new bool[buttonCount];
		Player player = ReInput.players.GetPlayer(playerId);
		controller = player.controllers.GetControllerWithTag<CustomController>(controllerTag);
		if (controller == null)
		{
			Debug.LogError((object)("A matching controller was not found for tag \"" + controllerTag + "\""));
		}
		if (((Controller)controller).buttonCount != buttonValues.Length || ((ControllerWithAxes)controller).axisCount != axisValues.Length)
		{
			Debug.LogError((object)"Controller has wrong number of elements!");
		}
		if (useUpdateCallbacks && controller != null)
		{
			controller.SetAxisUpdateCallback((Func<int, float>)GetAxisValueCallback);
			controller.SetButtonUpdateCallback((Func<int, bool>)GetButtonValueCallback);
		}
		initialized = true;
	}

	private void Update()
	{
		if (ReInput.isReady && !initialized)
		{
			Initialize();
		}
	}

	private void OnInputSourceUpdate()
	{
		GetSourceAxisValues();
		GetSourceButtonValues();
		if (!useUpdateCallbacks)
		{
			SetControllerAxisValues();
			SetControllerButtonValues();
		}
	}

	private void GetSourceAxisValues()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < axisValues.Length; i++)
		{
			if (i % 2 != 0)
			{
				axisValues[i] = joysticks[i / 2].position.y;
			}
			else
			{
				axisValues[i] = joysticks[i / 2].position.x;
			}
		}
	}

	private void GetSourceButtonValues()
	{
		for (int i = 0; i < buttonValues.Length; i++)
		{
			buttonValues[i] = buttons[i].isPressed;
		}
	}

	private void SetControllerAxisValues()
	{
		for (int i = 0; i < axisValues.Length; i++)
		{
			controller.SetAxisValue(i, axisValues[i]);
		}
	}

	private void SetControllerButtonValues()
	{
		for (int i = 0; i < buttonValues.Length; i++)
		{
			controller.SetButtonValue(i, buttonValues[i]);
		}
	}

	private float GetAxisValueCallback(int index)
	{
		if (index >= axisValues.Length)
		{
			return 0f;
		}
		return axisValues[index];
	}

	private bool GetButtonValueCallback(int index)
	{
		if (index >= buttonValues.Length)
		{
			return false;
		}
		return buttonValues[index];
	}
}
