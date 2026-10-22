using AmongUs.Data;
using Rewired;
using UnityEngine;

public class KeyboardJoystick : MonoBehaviour, IVirtualJoystick
{
	private Vector2 del;

	private static Player player;

	private Controller myController = new Controller();

	private Collider2D[] hitBuffer = new Collider2D[20];

	private int touchId = -1;

	public Vector2 DeltaL
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return del;
		}
	}

	public Vector2 DeltaR
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.zero;
		}
	}

	public void ToggleVisuals(bool on)
	{
	}

	private void UpdateTouchMovement()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		if (touchId > -1)
		{
			Controller.TouchState touchState = myController.Touches[touchId];
			if (touchState.IsDown)
			{
				Vector2 val = Vector2.op_Implicit(Camera.main.ViewportToWorldPoint(Vector2.op_Implicit(new Vector2(0.5f, 0.5f))));
				Vector2 val2 = touchState.Position - val;
				del = val2.normalized;
			}
			else
			{
				touchId = -1;
				del = Vector2.zero;
			}
			return;
		}
		for (int i = 0; i < myController.Touches.Length; i++)
		{
			Controller.TouchState touchState2 = myController.Touches[i];
			if (!touchState2.TouchStart)
			{
				continue;
			}
			bool flag = false;
			int num = Physics2D.OverlapPointNonAlloc(touchState2.Position, hitBuffer, Constants.NotShipMask);
			for (int j = 0; j < num; j++)
			{
				Collider2D val3 = hitBuffer[j];
				if (Object.op_Implicit((Object)(object)((Component)val3).GetComponent<ButtonBehavior>()) || Object.op_Implicit((Object)(object)((Component)val3).GetComponent<PassiveButton>()))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				touchId = i;
				break;
			}
		}
	}

	private void Update()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		if (player == null)
		{
			player = ReInput.players.GetPlayer(0);
		}
		del.x = (del.y = 0f);
		if (DataManager.Settings.Input.MouseMovementEnabled)
		{
			UpdateTouchMovement();
		}
		if (del == Vector2.zero)
		{
			if (player.GetButton(40))
			{
				del.x++;
			}
			if (player.GetButton(39))
			{
				del.x--;
			}
			if (player.GetButton(44))
			{
				del.y++;
			}
			if (player.GetButton(42))
			{
				del.y--;
			}
		}
		HandleHud();
		if (Input.GetKeyDown((KeyCode)27))
		{
			if (Object.op_Implicit((Object)(object)Minigame.Instance))
			{
				Minigame.Instance.Close();
			}
			else if (DestroyableSingleton<HudManager>.InstanceExists && Object.op_Implicit((Object)(object)MapBehaviour.Instance) && MapBehaviour.Instance.IsOpen)
			{
				MapBehaviour.Instance.Close();
			}
			else if (Object.op_Implicit((Object)(object)PlayerCustomizationMenu.Instance))
			{
				PlayerCustomizationMenu.Instance.Close(canMove: true);
			}
			else if (Object.op_Implicit((Object)(object)GameSettingMenu.Instance))
			{
				GameSettingMenu.Instance.Close();
			}
		}
		del.Normalize();
	}

	private static void HandleHud()
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			return;
		}
		if (player.GetButtonDown(7))
		{
			DestroyableSingleton<HudManager>.Instance.ReportButton.DoClick();
		}
		if (player.GetButtonDown(6))
		{
			DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
		}
		if (player.GetButtonDown(4) && !DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(GameManager.Instance.GetMapOptions());
		}
		if (player.GetButtonDown(49))
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.DoClick();
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
}
