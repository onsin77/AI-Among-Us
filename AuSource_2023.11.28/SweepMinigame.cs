using System.Collections.Generic;
using UnityEngine;

public class SweepMinigame : Minigame
{
	public SpriteRenderer[] Spinners;

	public SpriteRenderer[] Shadows;

	public SpriteRenderer[] Lights;

	public HorizontalGauge[] Gauges;

	private int spinnerIdx;

	private float timer;

	public float SpinRate = 45f;

	private float initialTimer;

	public AudioClip SpinningSound;

	public AudioClip AcceptSound;

	public AudioClip RejectSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		ResetGauges();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(SpinningSound, loop: true);
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable, gridNav: true);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(SpinningSound);
		base.Close();
	}

	public void FixedUpdate()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(2f - timer / 30f, 1f, 2f);
		timer += Time.fixedDeltaTime * num;
		if (spinnerIdx < Spinners.Length)
		{
			float num2 = CalcXPerc();
			Gauges[spinnerIdx].Value = ((num2 < 13f) ? 0.9f : 0.1f);
			Quaternion localRotation = Quaternion.Euler(0f, 0f, timer * SpinRate);
			((Component)Spinners[spinnerIdx]).transform.localRotation = localRotation;
			((Component)Shadows[spinnerIdx]).transform.localRotation = localRotation;
			((Renderer)Lights[spinnerIdx]).enabled = num2 < 13f;
		}
		for (int i = 0; i < Gauges.Length; i++)
		{
			HorizontalGauge horizontalGauge = Gauges[i];
			if (i < spinnerIdx)
			{
				horizontalGauge.Value = 0.95f;
			}
			if (i > spinnerIdx)
			{
				horizontalGauge.Value = 0.05f;
			}
			horizontalGauge.Value += (Mathf.PerlinNoise((float)i, Time.time * 51f) - 0.5f) * 0.025f;
		}
	}

	private float CalcXPerc()
	{
		int num = (int)(timer * SpinRate) % 360;
		return Mathf.Min(360 - num, num);
	}

	public void HitButton(int i)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (i != spinnerIdx)
		{
			return;
		}
		if (CalcXPerc() < 13f)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(AcceptSound, loop: false);
			}
			((Component)Spinners[spinnerIdx]).transform.localRotation = Quaternion.identity;
			((Component)Shadows[spinnerIdx]).transform.localRotation = Quaternion.identity;
			spinnerIdx++;
			timer = initialTimer;
			if (spinnerIdx >= Gauges.Length)
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(RejectSound, loop: false);
			}
			ResetGauges();
		}
	}

	private void ResetGauges()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		spinnerIdx = 0;
		timer = FloatRange.Next(1f, 3f);
		initialTimer = timer;
		for (int i = 0; i < Gauges.Length; i++)
		{
			((Renderer)Lights[i]).enabled = false;
			((Component)Spinners[i]).transform.localRotation = Quaternion.Euler(0f, 0f, timer * SpinRate);
			((Component)Shadows[i]).transform.localRotation = Quaternion.Euler(0f, 0f, timer * SpinRate);
		}
	}
}
