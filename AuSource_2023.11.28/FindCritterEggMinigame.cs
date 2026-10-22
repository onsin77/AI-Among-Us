using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FindCritterEggMinigame : Minigame
{
	[Header("Egg")]
	[SerializeField]
	private Transform[] spawnPoints;

	[SerializeField]
	private Transform egg;

	[SerializeField]
	private PassiveButton eggButton;

	[SerializeField]
	[Header("Leaves")]
	private EggCoveringLeaf[] leaves;

	[SerializeField]
	[Header("Cursor")]
	private Transform hand;

	[Header("Audio")]
	[SerializeField]
	private AudioClip eggGetSfx;

	[Range(0f, 1f)]
	[SerializeField]
	[Space(10f)]
	private float leafSfxPitchRange = 0.2f;

	[SerializeField]
	private AudioClip[] leafSfx;

	private Controller controller = new Controller();

	private bool completed;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		hand.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		hand.SetLocalZ(0f);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected Obj, but got Unknown
		base.Begin(task);
		SetupInput(disableCursor: false);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		SpriteRenderer[] componentsInChildren = ((Component)hand).GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		egg.position = spawnPoints.Random().position;
		((UnityEvent)eggButton.OnClick).AddListener((UnityAction)GetEgg);
		EggCoveringLeaf[] array = leaves;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnCollectedEvent += OnLeafCollected;
		}
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}

	private void UpdateCursorVisibility()
	{
		((Component)hand).gameObject.SetActive(!completed && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}

	private void GetEgg()
	{
		if (!completed)
		{
			((MonoBehaviour)this).StartCoroutine(CoGetEgg());
		}
	}

	private IEnumerator CoGetEgg()
	{
		completed = true;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(eggGetSfx, loop: false);
		}
		UpdateCursorVisibility();
		eggButton.SetButtonEnableState(enabled: false);
		MyNormTask.NextStep();
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		yield return Effects.Bounce(egg, 0.75f);
		yield return Effects.Wait(0.25f);
		((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
	}

	private void OnLeafCollected()
	{
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(leafSfx.Random(), loop: false, 1f, Random.Range(1f - leafSfxPitchRange, 1f + leafSfxPitchRange));
		}
	}
}
