using System;
using AmongUs.Data;
using Rewired;
using UnityEngine;

public class ActiveInputManager : DestroyableSingleton<ActiveInputManager>
{
	public enum InputType
	{
		Joystick,
		Keyboard,
		Touch
	}

	public InputType testCurrentControlType;

	public static InputType currentControlType = PlatformDefault;

	public double kChangeTime;

	public double mChangeTime;

	public double jChangeTime;

	public double tChangeTime;

	private Vector2 lastMousePosition = new Vector2(float.MinValue, float.MinValue);

	public static Action CurrentInputSourceChanged;

	private Controller lastUsedController;

	private static InputType PlatformDefault => InputType.Keyboard;

	public Controller CurrentController => lastUsedController;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		ReInput.players.GetPlayer(0).controllers.AddLastActiveControllerChangedDelegate((PlayerActiveControllerChangedDelegate)OnLastActiveControllerChanged);
	}

	private void Update()
	{
		UpdateActiveControlType();
	}

	private void OnLastActiveControllerChanged(Player player, Controller controller)
	{
		if (controller != null)
		{
			currentControlType = InputType.Joystick;
			if (DestroyableSingleton<HudManager>.InstanceExists && !(DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick))
			{
				DestroyableSingleton<HudManager>.Instance.SetTouchType(ControlTypes.Controller);
			}
		}
		else
		{
			currentControlType = PlatformDefault;
			if (DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick)
			{
				DestroyableSingleton<HudManager>.Instance.SetTouchType(DataManager.Settings.Input.InputMode);
			}
		}
		lastUsedController = controller;
		if (CurrentInputSourceChanged != null)
		{
			CurrentInputSourceChanged();
		}
	}

	public void UpdateActiveControlType()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		InputType inputType = currentControlType;
		if (ShouldEnableTouch())
		{
			Touch touch = Input.GetTouch(0);
			lastMousePosition = touch.position;
			currentControlType = InputType.Touch;
			tChangeTime = Math.Max(kChangeTime, Math.Max(mChangeTime, jChangeTime));
			return;
		}
		if (currentControlType != InputType.Keyboard)
		{
			if (player.controllers.hasKeyboard)
			{
				kChangeTime = ((Controller)player.controllers.Keyboard).GetLastTimeAnyElementChanged();
				if (kChangeTime > jChangeTime && kChangeTime > tChangeTime)
				{
					currentControlType = InputType.Keyboard;
				}
			}
			if (player.controllers.hasMouse)
			{
				Mouse mouse = player.controllers.Mouse;
				mChangeTime = ((Controller)mouse).GetLastTimeAnyElementChanged();
				if (Vector2.Distance(lastMousePosition, mouse.screenPosition) > 0.0001f && mChangeTime > jChangeTime && mChangeTime > tChangeTime)
				{
					lastMousePosition = mouse.screenPosition;
					currentControlType = InputType.Keyboard;
				}
			}
		}
		if (player.controllers.joystickCount > 0 && currentControlType != InputType.Joystick)
		{
			Controller lastActiveController = player.controllers.GetLastActiveController((ControllerType)2);
			if (lastActiveController != null)
			{
				jChangeTime = lastActiveController.GetLastTimeAnyElementChanged();
				if (jChangeTime > mChangeTime && jChangeTime > kChangeTime && jChangeTime > tChangeTime)
				{
					currentControlType = InputType.Joystick;
				}
			}
		}
		if (inputType == currentControlType)
		{
			return;
		}
		if (CurrentInputSourceChanged != null)
		{
			CurrentInputSourceChanged();
		}
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			return;
		}
		if (currentControlType == InputType.Joystick)
		{
			if (!(DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick))
			{
				DestroyableSingleton<HudManager>.Instance.SetTouchType(ControlTypes.Controller);
			}
		}
		else if (DestroyableSingleton<HudManager>.Instance.joystick is ConsoleJoystick)
		{
			DestroyableSingleton<HudManager>.Instance.SetTouchType(DataManager.Settings.Input.InputMode);
		}
	}

	private bool ShouldEnableTouch()
	{
		return Input.touchCount > 0;
	}

	public ActiveInputManager()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
