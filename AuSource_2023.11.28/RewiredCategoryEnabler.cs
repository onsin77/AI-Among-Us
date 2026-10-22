using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class RewiredCategoryEnabler : MonoBehaviour
{
	[Serializable]
	public class RewiredCategoryState
	{
		public string name;

		public bool enabled;
	}

	public bool applyDefaultMapState;

	public RewiredCategoryState[] defaultStates;

	private void OnEnable()
	{
		ReInput.ControllerConnectedEvent += ReInput_ControllerConnectedEvent;
	}

	private void OnDisable()
	{
		ReInput.ControllerConnectedEvent -= ReInput_ControllerConnectedEvent;
	}

	private void ReInput_ControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
	{
		if (applyDefaultMapState)
		{
			ApplyDefaultMapState();
		}
	}

	public void ApplyDefaultMapState()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		Debug.Log((object)"RewiredCategoryEnabler - Applying default map state to all players");
		int num = 0;
		foreach (Player item in allPlayers)
		{
			foreach (Joystick joystick in item.controllers.Joysticks)
			{
				num++;
				RewiredCategoryState[] array = defaultStates;
				foreach (RewiredCategoryState rewiredCategoryState in array)
				{
					item.controllers.maps.GetMap((Controller)(object)joystick, rewiredCategoryState.name, "Default").enabled = rewiredCategoryState.enabled;
				}
			}
		}
		Debug.Log((object)("Applied default map state to " + num + " joysticks"));
		if (ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Menu)
		{
			ConsoleJoystick.ReapplyCurrentInputState();
		}
	}
}
