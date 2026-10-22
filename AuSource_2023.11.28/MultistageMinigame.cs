using System.Collections.Generic;
using UnityEngine;

public class MultistageMinigame : Minigame
{
	[Header("Multistage Minigame")]
	public Minigame[] Stages;

	private Minigame stage;

	private bool hasOverlayMenu;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public List<UiElement> ControllerSelectable;

	public override void Begin(PlayerTask task)
	{
		NormalPlayerTask normalPlayerTask = task as NormalPlayerTask;
		if (normalPlayerTask.TaskType == TaskTypes.FuelEngines || normalPlayerTask.TaskType == TaskTypes.ExtractFuel)
		{
			stage = Stages[normalPlayerTask.Data[1]];
		}
		else if (normalPlayerTask.TaskType == TaskTypes.CatchFish)
		{
			if (normalPlayerTask.TaskStep == 0 || normalPlayerTask.MaxStep == 3)
			{
				stage = Stages[0];
			}
			else
			{
				stage = Stages[normalPlayerTask.taskStep];
			}
		}
		else
		{
			stage = Stages[normalPlayerTask.taskStep];
		}
		((Component)stage).gameObject.SetActive(true);
		stage.Begin(task);
		Minigame.Instance = this;
		UiElement defaultSelection = null;
		foreach (UiElement item in ControllerSelectable)
		{
			if (((Behaviour)item).isActiveAndEnabled)
			{
				defaultSelection = item;
				break;
			}
		}
		if (!stage.SkipMultistageOverlayMenuSetup)
		{
			hasOverlayMenu = true;
			ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, BackButton, defaultSelection, ControllerSelectable);
		}
	}

	public override void Close()
	{
		Minigame.Instance = null;
		stage.Close();
		base.Close();
		if (hasOverlayMenu)
		{
			hasOverlayMenu = false;
			ControllerManager.Instance.CloseOverlayMenu(((Object)((Component)this).gameObject).name);
		}
	}
}
