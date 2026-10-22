using TMPro;
using UnityEngine;

public class FungleTimer : MonoBehaviour
{
	private const string DONE_TIMER_TEXT = "!!";

	[SerializeField]
	private TextMeshPro timerText;

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SetValue(float taskTimer)
	{
		((TMP_Text)timerText).text = Mathf.CeilToInt(taskTimer).ToString();
	}

	public void SetDone()
	{
		((TMP_Text)timerText).text = "!!";
	}

	public void Shake()
	{
		((MonoBehaviour)this).StartCoroutine(Effects.Shake(((Component)this).transform, 0.5f, 0.1f, taper: true, returnToOrigin: true));
	}
}
