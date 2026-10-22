using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class InputSettingsData : AbstractSaveDataGroup
{
	public const ControlTypes DEFAULT_INPUT_MODE = ControlTypes.Keyboard;

	public const bool DEFAULT_MOUSE_MOVEMENT_ENABLED = false;

	public const float DEFAULT_TOUCH_JOYSTICK_SIZE = 1f;

	[JsonProperty("inputMode")]
	private ControlTypes inputMode = ControlTypes.Keyboard;

	[JsonProperty("mouseMovementEnabled")]
	private bool mouseMovementEnabled;

	[JsonProperty("touchJoystickSize")]
	private float touchJoystickSize = 1f;

	public ControlTypes InputMode
	{
		get
		{
			return inputMode;
		}
		set
		{
			SetValue(ref inputMode, ValidateInputMode(value), OnInputModeChanged);
		}
	}

	public bool MouseMovementEnabled
	{
		get
		{
			return mouseMovementEnabled;
		}
		set
		{
			SetValue(ref mouseMovementEnabled, value, OnMouseMovementEnabledChanged);
		}
	}

	public float TouchJoystickSize
	{
		get
		{
			return touchJoystickSize;
		}
		set
		{
			SetValue(ref touchJoystickSize, value, OnTouchJoystickSizeChanged);
		}
	}

	public event Action OnInputModeChanged;

	public event Action OnMouseMovementEnabledChanged;

	public event Action OnTouchJoystickSizeChanged;

	protected override void OnLoadComplete()
	{
		inputMode = ValidateInputMode(inputMode);
	}

	private static ControlTypes ValidateInputMode(ControlTypes mode)
	{
		if (Application.isConsolePlatform)
		{
			return ControlTypes.Controller;
		}
		if (Application.isMobilePlatform && (mode == ControlTypes.Keyboard || mode == ControlTypes.Controller))
		{
			mode = ControlTypes.VirtualJoystick;
		}
		return mode;
	}
}
