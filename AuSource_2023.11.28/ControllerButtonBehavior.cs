using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControllerButtonBehavior : MonoBehaviour
{
	public RewiredConstsEnum.Action Action;

	public GameObject requiredMenuObject;

	public List<string> requiredMenuNames = new List<string>();

	private Player player;

	public event Action OnPressedEvent;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)requiredMenuObject))
		{
			requiredMenuNames.Add(((Object)requiredMenuObject).name);
		}
		player = ReInput.players.GetPlayer(0);
	}

	private void Update()
	{
		if (!player.GetButtonDown((int)Action) || (requiredMenuNames.Count > 0 && !requiredMenuNames.Contains(ControllerManager.Instance.CurrentUiState.MenuName)))
		{
			return;
		}
		OnPressedEvent?.Invoke();
		ButtonBehavior component = ((Component)this).GetComponent<ButtonBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			ButtonClickedEvent onClick = component.OnClick;
			if (onClick != null)
			{
				((UnityEvent)onClick).Invoke();
			}
			return;
		}
		PassiveButton component2 = ((Component)this).GetComponent<PassiveButton>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			ButtonClickedEvent onClick2 = component2.OnClick;
			if (onClick2 != null)
			{
				((UnityEvent)onClick2).Invoke();
			}
		}
	}
}
