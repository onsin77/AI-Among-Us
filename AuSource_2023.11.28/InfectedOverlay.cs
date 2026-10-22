using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class InfectedOverlay : MonoBehaviour
{
	public MapRoom[] rooms;

	private IActivatable doors;

	private SabotageSystemType sabSystem;

	public ButtonBehavior[] allButtons;

	public ButtonBehavior selectedButton;

	private const float selectCooldown = 0.5f;

	private float currentCooldown;

	public bool CanUseDoors
	{
		get
		{
			if (ShipStatus.Instance.Type != ShipStatus.MapType.Pb)
			{
				return !sabSystem.AnyActive;
			}
			return true;
		}
	}

	public bool CanUseSabotage
	{
		get
		{
			if (GameManager.Instance.SabotagesEnabled() && sabSystem.Timer <= 0f && !sabSystem.AnyActive)
			{
				return !DoorsPreventingSabotage;
			}
			return false;
		}
	}

	private bool DoorsPreventingSabotage
	{
		get
		{
			if (doors != null)
			{
				return doors.IsActive;
			}
			return false;
		}
	}

	public void Start()
	{
		for (int i = 0; i < rooms.Length; i++)
		{
			rooms[i].Parent = this;
		}
		sabSystem = (SabotageSystemType)ShipStatus.Instance.Systems[SystemTypes.Sabotage];
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out var value))
		{
			doors = (IActivatable)value;
		}
	}

	private void FixedUpdate()
	{
		float specialActive = (DoorsPreventingSabotage ? 1f : sabSystem.PercentCool);
		for (int i = 0; i < rooms.Length; i++)
		{
			rooms[i].SetSpecialActive(specialActive);
			rooms[i].DoorsUpdate();
		}
	}

	private void OnEnable()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		allButtons = ((Component)this).GetComponentsInChildren<ButtonBehavior>(true);
		if (!Object.op_Implicit((Object)(object)selectedButton) && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			SelectClosestButton(Vector2.op_Implicit(((Component)this).transform.position));
		}
		else if (Object.op_Implicit((Object)(object)selectedButton) && ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
		{
			DeselectCurrent();
		}
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnControlTypeChanged));
	}

	private void OnDisable()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnControlTypeChanged));
	}

	public void OnControlTypeChanged()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			SelectClosestButton(Vector2.op_Implicit(((Component)this).transform.position));
		}
		else
		{
			DeselectCurrent();
		}
	}

	private void DeselectCurrent()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)selectedButton) && Object.op_Implicit((Object)(object)selectedButton.spriteRenderer))
		{
			selectedButton.spriteRenderer.color = Color.white;
			selectedButton = null;
		}
	}

	private void Select(ButtonBehavior newSelected)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)selectedButton))
		{
			DeselectCurrent();
		}
		selectedButton = newSelected;
		if (Object.op_Implicit((Object)(object)selectedButton) && Object.op_Implicit((Object)(object)selectedButton.spriteRenderer))
		{
			selectedButton.spriteRenderer.color = Color.green;
		}
	}

	private void SelectClosestButton(Vector2 anchorSpot)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		ButtonBehavior buttonBehavior = null;
		float num = float.PositiveInfinity;
		ButtonBehavior[] array = allButtons;
		foreach (ButtonBehavior buttonBehavior2 in array)
		{
			Vector2 val = Vector2.op_Implicit(((Component)buttonBehavior2).transform.position);
			Vector2 val2 = anchorSpot - val;
			float sqrMagnitude = val2.sqrMagnitude;
			if (sqrMagnitude < num || (Object)(object)buttonBehavior == (Object)null)
			{
				num = sqrMagnitude;
				buttonBehavior = buttonBehavior2;
			}
		}
		if ((Object)(object)buttonBehavior != (Object)null)
		{
			if (!Object.op_Implicit((Object)(object)buttonBehavior.spriteRenderer))
			{
				buttonBehavior.spriteRenderer = ((Component)buttonBehavior).GetComponent<SpriteRenderer>();
			}
			Select(buttonBehavior);
		}
	}

	private void Update()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		Vector2 val = new Vector2(player.GetAxis(16), player.GetAxis(17));
		float magnitude = val.magnitude;
		if (magnitude > 0.9f)
		{
			if (currentCooldown > 0f)
			{
				currentCooldown -= Time.deltaTime;
			}
			else
			{
				ButtonBehavior buttonBehavior = null;
				float num = float.PositiveInfinity;
				Vector2 val2 = Vector2.op_Implicit(((Component)selectedButton).transform.position);
				val /= magnitude;
				Debug.DrawRay(Vector2.op_Implicit(val2), Vector2.op_Implicit(val), Color.green, 5f);
				ButtonBehavior[] array = allButtons;
				foreach (ButtonBehavior buttonBehavior2 in array)
				{
					if ((Object)(object)buttonBehavior2 != (Object)(object)selectedButton)
					{
						Vector2 val3 = Vector2.op_Implicit(((Component)buttonBehavior2).transform.position);
						Vector2 val4 = val3 - val2;
						float magnitude2 = val4.magnitude;
						val4 /= magnitude2;
						float num2 = Vector2.Dot(val, val4);
						float num3 = num2 / magnitude2;
						if (num2 > 0.7f && ((Object)(object)buttonBehavior == (Object)null || num3 > num))
						{
							num = num3;
							buttonBehavior = buttonBehavior2;
							Debug.DrawLine(Vector2.op_Implicit(val2), Vector2.op_Implicit(val3), Color.cyan, 5f);
						}
						else
						{
							Debug.DrawLine(Vector2.op_Implicit(val2), Vector2.op_Implicit(val3), Color.red, 5f);
						}
					}
				}
				if ((Object)(object)buttonBehavior != (Object)null)
				{
					DeselectCurrent();
					Select(buttonBehavior);
					currentCooldown = 0.5f;
				}
			}
		}
		else
		{
			currentCooldown = 0f;
		}
		if (Object.op_Implicit((Object)(object)selectedButton) && player.GetButtonDown(11))
		{
			((UnityEvent)selectedButton.OnClick).Invoke();
		}
	}
}
