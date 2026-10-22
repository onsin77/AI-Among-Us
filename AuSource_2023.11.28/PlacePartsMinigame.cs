using System;
using Rewired;
using UnityEngine;

public class PlacePartsMinigame : Minigame
{
	[SerializeField]
	private DraggablePart[] draggableParts;

	[SerializeField]
	private DraggablePartFixed[] draggableFixedParts;

	[SerializeField]
	private SpriteRenderer hand;

	[SerializeField]
	private Sprite openHand;

	[SerializeField]
	private Sprite closeHand;

	private readonly Controller controller = new Controller();

	[SerializeField]
	[Header("Audio")]
	private AudioClip[] pickUpPartSfx;

	[SerializeField]
	private AudioClip[] resetPartSfx;

	[SerializeField]
	private AudioClip[] scrapOldPartSfx;

	[SerializeField]
	private AudioClip[] attachPartSfx;

	private DraggablePart selectedPart;

	private DraggablePartFixed selectedFixedPart;

	private int placedParts;

	private Player gamepad;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		((Component)hand).transform.position = new Vector3(VirtualCursor.currentPosition.x, VirtualCursor.currentPosition.y, ((Component)hand).transform.position.z);
		DraggablePart[] array = draggableParts;
		foreach (DraggablePart draggablePart in array)
		{
			switch (controller.CheckDrag(draggablePart.GetCollider()))
			{
			case DragState.TouchStart:
				selectedPart = draggablePart;
				SetHand(isGrabbing: true);
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(pickUpPartSfx.Random(), loop: false);
				}
				break;
			case DragState.Dragging:
				if ((Object)(object)selectedPart == (Object)(object)draggablePart)
				{
					selectedPart.UpdatePartPosition(controller.DragPosition);
				}
				break;
			case DragState.Released:
				if ((Object)(object)selectedPart == (Object)(object)draggablePart)
				{
					SetHand(isGrabbing: false);
					selectedPart.SnapIntoPlaceOrReset();
					selectedPart = null;
				}
				break;
			}
		}
		DraggablePartFixed[] array2 = draggableFixedParts;
		foreach (DraggablePartFixed draggablePartFixed in array2)
		{
			switch (controller.CheckDrag(draggablePartFixed.GetCollider()))
			{
			case DragState.TouchStart:
				draggablePartFixed.StartDrag();
				selectedFixedPart = draggablePartFixed;
				SetHand(isGrabbing: true);
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(pickUpPartSfx.Random(), loop: false);
				}
				break;
			case DragState.Dragging:
				if ((Object)(object)selectedFixedPart == (Object)(object)draggablePartFixed)
				{
					selectedFixedPart.UpdatePartPosition(controller.DragPosition);
				}
				break;
			case DragState.Released:
				if ((Object)(object)selectedFixedPart == (Object)(object)draggablePartFixed)
				{
					SetHand(isGrabbing: false);
					selectedFixedPart.SnapIntoPlaceOrReset();
					selectedFixedPart = null;
				}
				break;
			}
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		gamepad = ReInput.players.GetPlayer(0);
		SetupInput(disableCursor: false);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)hand);
		DraggablePart[] array = draggableParts;
		foreach (DraggablePart obj in array)
		{
			obj.Initialize();
			obj.OnNewPositionSavedEvent += OnOldPartRemoved;
			obj.OnPartPositionResetEvent += OnOldPartReset;
		}
		DraggablePartFixed[] array2 = draggableFixedParts;
		foreach (DraggablePartFixed obj2 in array2)
		{
			obj2.Initialize();
			obj2.OnNewPositionSavedEvent += OnNewPartPlaced;
			obj2.OnPartPositionResetEvent += OnNewPartReset;
		}
		placedParts = 0;
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private void SetHand(bool isGrabbing)
	{
		if (((Component)hand).gameObject.activeInHierarchy)
		{
			hand.sprite = (isGrabbing ? closeHand : openHand);
		}
	}

	private void OnNewPartPlaced()
	{
		placedParts++;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(attachPartSfx.Random(), loop: false);
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
		if (placedParts >= draggableParts.Length)
		{
			MyNormTask.NextStep();
			((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		}
	}

	private void OnNewPartReset()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(resetPartSfx.Random(), loop: false);
		}
	}

	private void OnOldPartRemoved()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(scrapOldPartSfx.Random(), loop: false);
		}
		VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
	}

	private void OnOldPartReset()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(resetPartSfx.Random(), loop: false);
		}
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}

	private void UpdateCursorVisibility()
	{
		((Component)hand).gameObject.SetActive(!MyNormTask.IsComplete && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}
}
