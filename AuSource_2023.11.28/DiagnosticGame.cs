using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiagnosticGame : Minigame
{
	public VerticalGauge Gauge;

	public SpriteRenderer StartButton;

	public float TimePerStep = 90f;

	public TextMeshPro Text;

	private int TargetNum = -1;

	public SpriteRenderer[] Targets;

	private Color goodBarColor = Color32.op_Implicit(new Color32((byte)100, (byte)193, byte.MaxValue, byte.MaxValue));

	public AudioClip StartSound;

	public AudioClip CorrectSound;

	public AudioClip TickSound;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private int lastPercent;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public override void Begin(PlayerTask task)
	{
		((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BeginDiagnostics);
		base.Begin(task);
		if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			((MonoBehaviour)this).StartCoroutine(BlinkButton());
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	private IEnumerator BlinkButton()
	{
		while (true)
		{
			StartButton.color = Color.red;
			yield return Effects.Wait(0.5f);
			StartButton.color = Color.white;
			yield return Effects.Wait(0.5f);
		}
	}

	public void PickAnomaly(int num)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing == CloseState.None && MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished && num == TargetNum)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(CorrectSound, loop: false);
			}
			Targets[TargetNum].color = goodBarColor;
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}

	public void StartDiagnostic()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
		{
			((Behaviour)((Component)StartButton).GetComponent<PassiveButton>()).enabled = false;
			((MonoBehaviour)this).StopAllCoroutines();
			StartButton.color = Color.white;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(StartSound, loop: false);
			}
			MyNormTask.TaskTimer = TimePerStep;
			MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
		}
	}

	public void Update()
	{
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		switch (MyNormTask.TimerStarted)
		{
		case NormalPlayerTask.TimerState.Started:
		{
			((Component)Gauge).gameObject.SetActive(true);
			Gauge.MaxValue = TimePerStep;
			Gauge.value = MyNormTask.TaskTimer;
			int num = (int)(100f * MyNormTask.TaskTimer / TimePerStep);
			if (num != lastPercent && Constants.ShouldPlaySfx())
			{
				lastPercent = num;
				SoundManager.Instance.PlaySound(TickSound, loop: false, 0.8f);
			}
			((TMP_Text)Text).text = num + "%";
			Targets.ForEach((SpriteRenderer f) =>
			{
				((Component)f).gameObject.SetActive(false);
			});
			break;
		}
		case NormalPlayerTask.TimerState.NotStarted:
			((Component)Gauge).gameObject.SetActive(false);
			Targets.ForEach((SpriteRenderer f) =>
			{
				((Component)f).gameObject.SetActive(false);
			});
			break;
		case NormalPlayerTask.TimerState.Finished:
			((Component)Gauge).gameObject.SetActive(true);
			Gauge.MaxValue = 1f;
			Gauge.value = 1f;
			if (TargetNum == -1)
			{
				((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PickAnomaly);
				Targets.ForEach((SpriteRenderer f) =>
				{
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					((Component)f).gameObject.SetActive(true);
					f.color = goodBarColor;
				});
				TargetNum = Targets.RandomIdx();
				Targets[TargetNum].color = Color.red;
			}
			break;
		}
	}

	public DiagnosticGame()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
	}
}
