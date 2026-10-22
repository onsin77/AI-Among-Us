using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectSamplesMinigame : Minigame
{
	private const int SAMPLE_COUNT = 3;

	[SerializeField]
	private Transform[] spawnPoints;

	[SerializeField]
	private CollectSamplesMinigameCollectableSample[] collectableSamples;

	[SerializeField]
	private Transform hand;

	private Controller controller = new Controller();

	private bool IsComplete => SamplesCollected == 3;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private int SamplesCollected
	{
		get
		{
			return MyNormTask.Data[1];
		}
		set
		{
			MyNormTask.Data[1] = (byte)value;
		}
	}

	private void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		hand.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		hand.SetLocalZ(-0.5f);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: false);
		InitializeSaveState();
		SpriteRenderer[] componentsInChildren = ((Component)hand).GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		List<int> list = new List<int>(Enumerable.Range(0, spawnPoints.Length));
		List<int> list2 = new List<int>(Enumerable.Range(0, collectableSamples.Length));
		for (int j = 0; j < 3 - SamplesCollected; j++)
		{
			int num = list2.Random();
			list2.Remove(num);
			CollectSamplesMinigameCollectableSample collectSamplesMinigameCollectableSample = collectableSamples[num];
			int num2 = list.Random();
			list.Remove(num2);
			CollectSamplesMinigameCollectableSample collectable = Object.Instantiate<CollectSamplesMinigameCollectableSample>(collectSamplesMinigameCollectableSample, spawnPoints[num2], false);
			collectable.OnPressEvent += (CollectSamplesMinigameCollectableSample x) =>
			{
				((MonoBehaviour)this).StartCoroutine(CoCollectSample(collectable));
			};
		}
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void InitializeSaveState()
	{
		if (MyNormTask.Data.Length == 1)
		{
			MyNormTask.Data = new byte[2]
			{
				MyNormTask.Data[0],
				0
			};
		}
	}

	private IEnumerator CoCollectSample(CollectSamplesMinigameCollectableSample collectable)
	{
		if (MyNormTask.taskStep <= 0)
		{
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			yield return Effects.ScaleIn(((Component)collectable).transform, 1f, 0f, 0.3f);
			SamplesCollected++;
			if (IsComplete)
			{
				UpdateCursorVisibility();
				MyNormTask.NextStep();
				yield return Effects.Wait(0.5f);
				((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
			}
		}
	}

	private void UpdateCursorVisibility()
	{
		((Component)hand).gameObject.SetActive(!IsComplete && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}
}
