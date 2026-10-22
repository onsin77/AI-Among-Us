using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class ConsoleJoystick : MonoBehaviour, IVirtualJoystick
{
	public enum ConsoleInputState
	{
		Gameplay,
		Menu,
		Task,
		Sabotage,
		Vent,
		QuickChat,
		None
	}

	private Vector2 delta;

	private Vector2 deltaR;

	private static Player player;

	private static Controller controller;

	private static Controller prevController;

	public static ConsoleInputState inputState = ConsoleInputState.Menu;

	private static ConsoleInputState oldInputState = ConsoleInputState.Sabotage;

	private static int highlightedVentIndex = 0;

	public Vector2 DeltaL
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return delta;
		}
	}

	public Vector2 DeltaR
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return deltaR;
		}
	}

	public void ToggleVisuals(bool on)
	{
	}

	private void OnEnable()
	{
		RewiredControllerSupport.onPostControllersAssigned = (Action)Delegate.Combine(RewiredControllerSupport.onPostControllersAssigned, new Action(ReapplyCurrentInputState));
		if (inputState == ConsoleInputState.Menu)
		{
			SetMode_Gameplay();
		}
		else
		{
			ReapplyCurrentInputState();
		}
	}

	private void OnDisable()
	{
		RewiredControllerSupport.onPostControllersAssigned = (Action)Delegate.Remove(RewiredControllerSupport.onPostControllersAssigned, new Action(ReapplyCurrentInputState));
	}

	public static void ReapplyCurrentInputState()
	{
		Debug.Log((object)"New controller connected, updating current input state");
		switch (inputState)
		{
		case ConsoleInputState.Gameplay:
			SetMode_Gameplay();
			break;
		case ConsoleInputState.Menu:
			SetMode_Menu();
			break;
		case ConsoleInputState.Sabotage:
			SetMode_Sabotage();
			break;
		case ConsoleInputState.Task:
			SetMode_Task();
			break;
		case ConsoleInputState.Vent:
			SetMode_Vent();
			break;
		case ConsoleInputState.QuickChat:
			SetMode_QuickChat();
			break;
		case ConsoleInputState.None:
			SetMode_None();
			break;
		}
	}

	public static void SetMode_MenuAdditive()
	{
		player = ReInput.players.GetPlayer(0);
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(2, enableMap: true);
		}
	}

	public static void SetMode_Gameplay()
	{
		inputState = ConsoleInputState.Gameplay;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance))
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: true);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: false);
			SetMapEnabled(4, enableMap: false);
		}
	}

	public static void SetMode_Menu()
	{
		inputState = ConsoleInputState.Menu;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance) && Object.op_Implicit((Object)(object)ControllerManager.Instance) && !ControllerManager.Instance.IsUiControllerActive)
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: false);
			SetMapEnabled(2, enableMap: true);
			SetMapEnabled(3, enableMap: false);
			SetMapEnabled(4, enableMap: false);
		}
		VirtualCursor.horizontalAxis = 9;
		VirtualCursor.verticalAxis = 10;
	}

	public static void SetMode_Task()
	{
		inputState = ConsoleInputState.Task;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance) && !ControllerManager.Instance.IsUiControllerActive)
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: false);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: true);
			SetMapEnabled(4, enableMap: false);
		}
		VirtualCursor.horizontalAxis = 13;
		VirtualCursor.verticalAxis = 14;
	}

	public static void SetMode_Sabotage()
	{
		inputState = ConsoleInputState.Sabotage;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance))
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: true);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: true);
			SetMapEnabled(4, enableMap: false);
		}
		VirtualCursor.horizontalAxis = 16;
		VirtualCursor.verticalAxis = 17;
	}

	public static void SetMode_Vent()
	{
		inputState = ConsoleInputState.Vent;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance) && !ControllerManager.Instance.IsUiControllerActive)
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: true);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: false);
			SetMapEnabled(4, enableMap: false);
		}
		highlightedVentIndex = -1;
		VirtualCursor.horizontalAxis = 16;
		VirtualCursor.verticalAxis = 17;
	}

	public static void SetMode_QuickChat()
	{
		inputState = ConsoleInputState.QuickChat;
		player = ReInput.players.GetPlayer(0);
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance) && !ControllerManager.Instance.IsUiControllerActive)
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(true);
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: false);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: false);
			SetMapEnabled(4, enableMap: true);
		}
		VirtualCursor.horizontalAxis = 13;
		VirtualCursor.verticalAxis = 14;
	}

	public static void SetMode_None()
	{
		inputState = ConsoleInputState.None;
		player = ReInput.players.GetPlayer(0);
		IList<Joystick> joysticks = player.controllers.Joysticks;
		for (int i = 0; i < player.controllers.joystickCount; i++)
		{
			controller = (Controller)(object)joysticks[i];
			SetMapEnabled(1, enableMap: false);
			SetMapEnabled(2, enableMap: false);
			SetMapEnabled(3, enableMap: false);
			SetMapEnabled(4, enableMap: false);
		}
	}

	private static void SetMapEnabled(int rewiredCategoryIndex, bool enableMap)
	{
		if (controller != null)
		{
			player.controllers.maps.GetMap(controller, rewiredCategoryIndex, 0).enabled = enableMap;
		}
	}

	private void Update()
	{
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		if (oldInputState != inputState)
		{
			if (DestroyableSingleton<HudManager>.Instance.consoleUIObjects != null && DestroyableSingleton<HudManager>.Instance.consoleUIObjects.Length != 0)
			{
				if (oldInputState != ConsoleInputState.None && (Object)(object)DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)oldInputState] != (Object)null)
				{
					DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)oldInputState].SetActive(false);
				}
				if (inputState != ConsoleInputState.None && (Object)(object)DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)inputState] != (Object)null)
				{
					DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)inputState].SetActive(true);
				}
			}
			oldInputState = inputState;
		}
		if (inputState == ConsoleInputState.Menu && ControllerManager.Instance.IsUiControllerActive != DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)inputState].activeSelf)
		{
			DestroyableSingleton<HudManager>.Instance.consoleUIObjects[(int)inputState].SetActive(ControllerManager.Instance.IsUiControllerActive);
		}
		delta.x = player.GetAxis(2);
		delta.y = player.GetAxis(3);
		deltaR.x = player.GetAxis(54);
		deltaR.y = player.GetAxis(55);
		if (delta.sqrMagnitude > 1f)
		{
			delta = delta.normalized;
		}
		HandleHUD();
	}

	private void HandleHUD()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			bool canMove = PlayerControl.LocalPlayer.CanMove;
			if (!canMove && inputState == ConsoleInputState.Gameplay)
			{
				SetMode_Menu();
				return;
			}
			if (canMove && inputState != ConsoleInputState.Sabotage && inputState != ConsoleInputState.Gameplay)
			{
				SetMode_Gameplay();
				return;
			}
		}
		if (inputState == ConsoleInputState.Vent)
		{
			if (!ControllerManager.Instance.IsUiControllerActive)
			{
				if (!Object.op_Implicit((Object)(object)Vent.currentVent))
				{
					SetMode_Gameplay();
				}
				else
				{
					bool flag = false;
					if (delta.sqrMagnitude > 0.25f)
					{
						flag = true;
						Vector2 normalized = delta.normalized;
						Vector2 val = Vector2.zero;
						float num = float.NegativeInfinity;
						int num2 = -1;
						for (int i = 0; i < Vent.currentVent.Buttons.Length; i++)
						{
							if (((Behaviour)Vent.currentVent.Buttons[i]).isActiveAndEnabled)
							{
								val = Vector2.op_Implicit(((Component)Vent.currentVent.Buttons[i]).transform.localPosition);
								val = val.normalized;
								float num3 = Vector2.Dot(normalized, val);
								if (num2 == -1 || num3 > num)
								{
									num = num3;
									num2 = i;
								}
							}
						}
						if (num > 0.7f)
						{
							if (highlightedVentIndex != -1 && highlightedVentIndex < Vent.currentVent.Buttons.Length)
							{
								Vent.currentVent.Buttons[highlightedVentIndex].spriteRenderer.color = Color.white;
								highlightedVentIndex = -1;
							}
							highlightedVentIndex = num2;
							Vent.currentVent.Buttons[highlightedVentIndex].spriteRenderer.color = Color.red;
						}
						else if (highlightedVentIndex != -1 && highlightedVentIndex < Vent.currentVent.Buttons.Length)
						{
							Vent.currentVent.Buttons[highlightedVentIndex].spriteRenderer.color = Color.white;
							highlightedVentIndex = -1;
						}
					}
					else if (highlightedVentIndex != -1 && highlightedVentIndex < Vent.currentVent.Buttons.Length)
					{
						Vent.currentVent.Buttons[highlightedVentIndex].spriteRenderer.color = Color.white;
						highlightedVentIndex = -1;
					}
					if (!flag)
					{
						if (player.GetButtonDown(6))
						{
							DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
						}
						if (player.GetButtonDown(49))
						{
							DestroyableSingleton<HudManager>.Instance.AbilityButton.DoClick();
						}
						if (PlayerControl.LocalPlayer.Data != null && (Object)(object)PlayerControl.LocalPlayer.Data.Role != (Object)null && PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.GetButtonDown(50))
						{
							DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
						}
					}
					else if (highlightedVentIndex != -1 && player.GetButtonDown(6))
					{
						Vent.currentVent.Buttons[highlightedVentIndex].spriteRenderer.color = Color.white;
						((UnityEvent)Vent.currentVent.Buttons[highlightedVentIndex].OnClick).Invoke();
						highlightedVentIndex = -1;
					}
				}
			}
		}
		else
		{
			if (player.GetButtonDown(49))
			{
				DestroyableSingleton<HudManager>.Instance.AbilityButton.DoClick();
			}
			if (player.GetButtonDown(7))
			{
				DestroyableSingleton<HudManager>.Instance.ReportButton.DoClick();
			}
			if (player.GetButtonDown(6))
			{
				DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
			}
			if (PlayerControl.LocalPlayer.Data != null && (Object)(object)PlayerControl.LocalPlayer.Data.Role != (Object)null)
			{
				if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.GetButtonDown(8))
				{
					DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
				}
				if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.GetButtonDown(50))
				{
					DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
				}
			}
		}
		if (player.GetButtonDown(5) && DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.TaskPanel.ToggleOpen();
		}
		if (player.GetButtonDown(1))
		{
			if (DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Close();
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Open();
			}
		}
		if (player.GetButtonDown(4))
		{
			if (inputState == ConsoleInputState.Sabotage)
			{
				SetMode_Gameplay();
				if (Object.op_Implicit((Object)(object)MapBehaviour.Instance))
				{
					MapBehaviour.Instance.Close();
				}
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(GameManager.Instance.GetMapOptions());
			}
		}
		if (player.GetButtonDown(35) && inputState == ConsoleInputState.Menu && Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
			{
				Mode = MapOptions.Modes.Normal
			});
		}
		if (player.GetButtonDown(12) && !ControllerManager.Instance.IsUiControllerActive)
		{
			if (Object.op_Implicit((Object)(object)Minigame.Instance))
			{
				Minigame.Instance.Close();
			}
			else if (Object.op_Implicit((Object)(object)MapBehaviour.Instance))
			{
				MapBehaviour.Instance.Close();
			}
			if (Object.op_Implicit((Object)(object)Vent.currentVent))
			{
				SetMode_Vent();
			}
			else if (inputState != ConsoleInputState.Gameplay)
			{
				SetMode_Gameplay();
			}
		}
	}
}
