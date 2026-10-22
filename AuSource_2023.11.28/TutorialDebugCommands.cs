using System.Collections.Generic;
using Innersloth.DebugTool;
using UnityEngine;

public class TutorialDebugCommands : MonoBehaviour
{
	private static bool AutoResetTask;

	private void Update()
	{
		if (!AutoResetTask)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		foreach (PlayerTask item in new List<PlayerTask>(localPlayer.myTasks))
		{
			if (!(item is SabotageTask) && item.IsComplete)
			{
				localPlayer.RemoveTask(item);
				TaskAddButton.AddTaskToPlayer(ShipStatus.Instance.GetTaskById((byte)item.Index));
			}
		}
	}

	public static void AddCommands(GameObject parent)
	{
	}

	public static void RemoveCommands(GameObject parent)
	{
	}

	[DebugCommand("Game/Toggle Auto-Reset Task After Complete", "", true)]
	public static void GameToggleAutoResetTask()
	{
		AutoResetTask = !AutoResetTask;
	}
}
