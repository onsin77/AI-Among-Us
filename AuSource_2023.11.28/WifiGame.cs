using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

public class WifiGame : Minigame
{
	private const int WaitDuration = 60;

	public SlideBar Slider;

	public TextMeshPro StatusText;

	public SpriteRenderer[] Lights;

	public Sprite LightOn;

	public Sprite LightOff;

	public AudioClip SliderClick;

	private bool WifiOff;

	private TouchpadBehavior touchpad;

	private float initialSlider;

	private Controller controller = new Controller();

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			TurnOn(first: true);
		}
		else
		{
			TurnOff(first: true);
		}
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.IsComplete)
		{
			return;
		}
		controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			float num = 0f;
			num = player.GetAxis(17);
			if (touchpad.IsTouching())
			{
				num = touchpad.GetTouchVector().y;
			}
			if (!WifiOff && MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
			{
				float value;
				if (touchpad.IsTouching())
				{
					if (touchpad.IsFirstTouch())
					{
						initialSlider = Slider.Value;
					}
					value = initialSlider + num;
				}
				else
				{
					value = Slider.Value + num * Time.deltaTime * 3f;
				}
				Slider.SetValue(value);
			}
			else if (WifiOff && MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished)
			{
				float value2 = Slider.Value + num * Time.deltaTime * 3f;
				Slider.SetValue(value2);
			}
		}
		if (WifiOff)
		{
			if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished)
			{
				((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleasePowerOn);
			}
			else if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Started)
			{
				((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseReturnIn, Mathf.CeilToInt(MyNormTask.TaskTimer));
			}
		}
		if (!WifiOff && (double)Slider.Value < 0.1)
		{
			TurnOff();
		}
		else if (WifiOff && (double)Slider.Value > 0.9)
		{
			TurnOn();
		}
	}

	private void TurnOn(bool first = false)
	{
		if (Constants.ShouldPlaySfx() && !first)
		{
			SoundManager.Instance.PlaySound(SliderClick, loop: false);
		}
		Slider.Value = 1f;
		Slider.UpdateValue();
		WifiOff = false;
		if (!first)
		{
			((MonoBehaviour)this).StopAllCoroutines();
		}
		((MonoBehaviour)this).StartCoroutine(RunLights(MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished));
	}

	private void TurnOff(bool first = false)
	{
		if (Constants.ShouldPlaySfx() && !first)
		{
			SoundManager.Instance.PlaySound(SliderClick, loop: false);
		}
		Slider.Value = 0f;
		Slider.UpdateValue();
		WifiOff = true;
		if (!first)
		{
			((MonoBehaviour)this).StopAllCoroutines();
		}
		Lights.ForEach((SpriteRenderer s) =>
		{
			s.sprite = LightOff;
		});
		if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseReturnIn) + " " + 60;
			MyNormTask.TaskTimer = 60f;
			MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		}
	}

	private IEnumerator RunLights(bool finishing)
	{
		if (!finishing)
		{
			((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiRebootRequired);
			Lights.ForEach((SpriteRenderer s) =>
			{
				s.sprite = LightOn;
			});
			Lights[1].sprite = LightOff;
			Lights[3].sprite = LightOff;
			Lights[4].sprite = LightOff;
			Lights[6].sprite = LightOff;
			yield return Effects.All(CoBlinkLight(Lights[2], 0.3f));
			yield break;
		}
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiPleaseWait);
		for (float timer = 0f; timer < 3f; timer += Time.deltaTime)
		{
			float num = timer / 3f;
			for (int num2 = 0; num2 < Lights.Length; num2++)
			{
				float num3 = 0.75f * (float)num2 / (float)Lights.Length;
				float num4 = 0.75f * (float)(num2 + 1) / (float)Lights.Length;
				Lights[num2].sprite = ((num > num3 && num < num4) ? LightOn : LightOff);
			}
			yield return null;
		}
		((TMP_Text)StatusText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WifiRebootComplete);
		Lights.ForEach((SpriteRenderer s) =>
		{
			s.sprite = LightOn;
		});
		((MonoBehaviour)this).StartCoroutine(Effects.All(CoBlinkLight(Lights[3], 0.1f), CoBlinkLight(Lights[4], 0.09f), CoBlinkLight(Lights[5], 0.1f), CoBlinkLight(Lights[6], 0.05f), CoBlinkLight(Lights[7], 0.5f)));
		MyNormTask.NextStep();
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}

	private IEnumerator CoBlinkLight(SpriteRenderer light, float delay)
	{
		while (true)
		{
			light.sprite = LightOn;
			yield return Effects.Wait(delay);
			light.sprite = LightOff;
			yield return Effects.Wait(delay);
		}
	}
}
