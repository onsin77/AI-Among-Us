using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControllerButtonBehaviourComplex : MonoBehaviour
{
	public enum ActionTriggerType
	{
		Any,
		All
	}

	[Serializable]
	public struct ActionTrigger
	{
		public enum ActionType
		{
			Axis_GEqual,
			Axis_LEqual,
			Button_Down
		}

		public RewiredConstsEnum.Action action;

		public ActionType actionType;

		public float axisComparisonValue;

		public bool ConditionMet(Player player)
		{
			return actionType switch
			{
				ActionType.Button_Down => player.GetButtonDown((int)action), 
				ActionType.Axis_GEqual => player.GetAxisRaw((int)action) >= axisComparisonValue, 
				ActionType.Axis_LEqual => player.GetAxisRaw((int)action) <= axisComparisonValue, 
				_ => false, 
			};
		}
	}

	public ActionTriggerType actionTriggerType;

	public ActionTrigger[] actionTriggers;

	public GameObject requiredMenuObject;

	private Player player;

	private void Start()
	{
		player = ReInput.players.GetPlayer(0);
	}

	private bool ConditionMet()
	{
		ActionTrigger[] array;
		if (actionTriggerType == ActionTriggerType.All)
		{
			array = actionTriggers;
			foreach (ActionTrigger actionTrigger in array)
			{
				if (!actionTrigger.ConditionMet(player))
				{
					return false;
				}
			}
			return true;
		}
		array = actionTriggers;
		foreach (ActionTrigger actionTrigger2 in array)
		{
			if (actionTrigger2.ConditionMet(player))
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		if (!ConditionMet() || (Object.op_Implicit((Object)(object)requiredMenuObject) && ControllerManager.Instance.CurrentUiState.MenuName != ((Object)requiredMenuObject).name))
		{
			return;
		}
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
