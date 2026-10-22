using System;
using UnityEngine;
using UnityEngine.Events;

public class TakePartsMinigame : Minigame
{
	[SerializeField]
	private PassiveButton[] parts;

	[SerializeField]
	private GameObject[] inventoryParts;

	[SerializeField]
	private PassiveButton boxLid;

	[SerializeField]
	private GameObject boxOpen;

	[SerializeField]
	private GameObject hand;

	[Header("Audio")]
	[SerializeField]
	private AudioClip openToolboxSfx;

	[SerializeField]
	private AudioClip[] pickUpSfx;

	private readonly Controller controller = new Controller();

	private int partsTaken;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		hand.transform.position = new Vector3(VirtualCursor.currentPosition.x, VirtualCursor.currentPosition.y, hand.transform.position.z);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected Obj, but got Unknown
		base.Begin(task);
		SetupInput(disableCursor: false);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		PassiveButton[] array = parts;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = false;
		}
		SpriteRenderer[] componentsInChildren = hand.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		partsTaken = 0;
		((UnityEvent)boxLid.OnClick).AddListener((UnityAction)OnToolboxOpen);
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void OnToolboxOpen()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected Obj, but got Unknown
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(openToolboxSfx, loop: false);
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		((Component)boxLid).gameObject.SetActive(false);
		boxOpen.gameObject.SetActive(true);
		PassiveButton[] array = parts;
		foreach (PassiveButton part in array)
		{
			((Behaviour)part).enabled = true;
			((UnityEvent)part.OnClick).AddListener((UnityAction)(() =>
			{
				int num = parts.IndexOf(part);
				inventoryParts[num].SetActive(true);
				SavePart();
				((Component)part).gameObject.SetActive(false);
			}));
		}
	}

	private void SavePart()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(pickUpSfx.Random(), loop: false);
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		partsTaken++;
		if (partsTaken >= parts.Length)
		{
			MyNormTask.NextStep();
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}

	private void UpdateCursorVisibility()
	{
		hand.gameObject.SetActive(!MyNormTask.IsComplete && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}
}
