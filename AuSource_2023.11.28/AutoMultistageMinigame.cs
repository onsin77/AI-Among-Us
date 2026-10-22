using System.Collections;
using UnityEngine;

public class AutoMultistageMinigame : Minigame
{
	public Minigame[] Stages;

	private Minigame stage;

	public override void Begin(PlayerTask task)
	{
		NormalPlayerTask normalPlayerTask = task as NormalPlayerTask;
		for (int i = 0; i < Stages.Length; i++)
		{
			((Component)Stages[i]).gameObject.SetActive(i == normalPlayerTask.taskStep);
		}
		stage = Stages[normalPlayerTask.taskStep];
		stage.Console = Console;
		stage.Begin(task);
		Minigame.Instance = this;
		((MonoBehaviour)this).StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		while (Object.op_Implicit((Object)(object)stage))
		{
			yield return null;
		}
		Close();
	}

	public override void Close()
	{
		Minigame.Instance = null;
		if (Object.op_Implicit((Object)(object)stage))
		{
			stage.Close();
		}
		base.Close();
	}
}
