using TMPro;
using UnityEngine;

public class TimerTextTMP : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro text;

	private string format = "";

	private float duration;

	private float startTime;

	private int seconds;

	private void OnEnable()
	{
		UpdateText();
	}

	private void Update()
	{
		Tick();
	}

	public void SetTimer(float duration, string format)
	{
		this.duration = duration;
		startTime = Time.unscaledTime;
		seconds = Mathf.CeilToInt(duration);
		this.format = format;
		UpdateText();
	}

	private int GetSecondsRemaining()
	{
		return Mathf.CeilToInt(duration - Mathf.Clamp(Time.unscaledTime - startTime, 0f, duration));
	}

	private string GetTextString()
	{
		return string.Format(format, GetSecondsRemaining());
	}

	private void Tick()
	{
		int secondsRemaining = GetSecondsRemaining();
		if (secondsRemaining != seconds)
		{
			UpdateText();
		}
		seconds = secondsRemaining;
	}

	private void UpdateText()
	{
		((TMP_Text)text).text = GetTextString();
	}
}
