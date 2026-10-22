using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class PressAnyButtonToJoinExample_Assigner : MonoBehaviour
{
	private void Update()
	{
		if (ReInput.isReady)
		{
			AssignJoysticksToPlayers();
		}
	}

	private void AssignJoysticksToPlayers()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			Joystick val = joysticks[i];
			if (!ReInput.controllers.IsControllerAssigned(((Controller)val).type, ((Controller)val).id) && ((Controller)val).GetAnyButtonDown())
			{
				Player val2 = FindPlayerWithoutJoystick();
				if (val2 == null)
				{
					return;
				}
				val2.controllers.AddController((Controller)(object)val, false);
			}
		}
		if (DoAllPlayersHaveJoysticks())
		{
			ReInput.configuration.autoAssignJoysticks = true;
			((Behaviour)this).enabled = false;
		}
	}

	private Player FindPlayerWithoutJoystick()
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i].controllers.joystickCount <= 0)
			{
				return players[i];
			}
		}
		return null;
	}

	private bool DoAllPlayersHaveJoysticks()
	{
		return FindPlayerWithoutJoystick() == null;
	}
}
