using System.Collections;
using TMPro;
using UnityEngine;

public class PlaceholderMinigame : Minigame
{
	[SerializeField]
	private TextMeshPro waitText;

	[SerializeField]
	private TextMeshPro taskNameText;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		((TMP_Text)taskNameText).text = DestroyableSingleton<TranslationController>.Instance.GetString(task.TaskType);
		if (MyNormTask.MaxStep > 1)
		{
			TextMeshPro val = taskNameText;
			((TMP_Text)val).text = ((TMP_Text)val).text + $" Step {MyNormTask.taskStep + 1}/{MyNormTask.MaxStep}";
		}
		((MonoBehaviour)this).StartCoroutine(CoCompleteAfterABit());
	}

	private IEnumerator CoCompleteAfterABit()
	{
		float startTime = Time.time;
		while (Time.time - startTime < 5f)
		{
			((TMP_Text)waitText).text = $"This task will auto-complete in {Mathf.CeilToInt(5f - (Time.time - startTime))} seconds";
			yield return null;
		}
		MyNormTask.NextStep();
		if ((Object)(object)((Component)this).transform.parent != (Object)null && Object.op_Implicit((Object)(object)((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>()))
		{
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
		else
		{
			yield return CoStartClose();
		}
	}
}
