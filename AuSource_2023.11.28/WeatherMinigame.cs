using System.Collections;
using TMPro;
using UnityEngine;

public class WeatherMinigame : Minigame
{
	public float Duration = 5f;

	public HorizontalGauge destGauge1;

	public HorizontalGauge destGauge2;

	public HorizontalGauge destGauge3;

	public PassiveButton StartButton;

	public TextMeshPro EtaText;

	public AudioClip StartSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void StartStopFill()
	{
		((Behaviour)StartButton).enabled = false;
		((MonoBehaviour)this).StartCoroutine(CoDoAnimation());
	}

	private IEnumerator CoDoAnimation()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(StartSound, loop: false);
		}
		yield return Effects.ScaleIn(((Component)StartButton).transform, 1f, 0f, 0.15f);
		((Component)EtaText).gameObject.SetActive(true);
		yield return Effects.ScaleIn(EtaText.transform, 0f, 1f, 0.15f);
		for (float timer = 0f; timer < Duration; timer += Time.deltaTime)
		{
			int num = Mathf.CeilToInt(Duration - timer);
			((TMP_Text)EtaText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WeatherEta, num);
			destGauge1.Value = Mathf.Lerp(0f, 1f, timer / Duration * 5f);
			destGauge2.Value = Mathf.Lerp(0f, 1f, timer / Duration * 3f);
			destGauge3.Value = Mathf.Lerp(0f, 1f, timer / Duration);
			yield return null;
		}
		((TMP_Text)EtaText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WeatherComplete);
		MyNormTask.NextStep();
		yield return CoStartClose();
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(StartSound);
		base.Close();
	}
}
