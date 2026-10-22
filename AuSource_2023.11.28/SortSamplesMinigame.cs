using System;
using System.Collections;
using UnityEngine;

public class SortSamplesMinigame : Minigame
{
	[SerializeField]
	private SortableSample[] sortableSamples;

	[SerializeField]
	private Collider2D mushroomJar;

	[SerializeField]
	private Transform mushroomJarDropSpot;

	[SerializeField]
	private Collider2D crystalJar;

	[SerializeField]
	private Transform crystalJarDropSpot;

	[SerializeField]
	private Transform hand;

	[SerializeField]
	private SpriteRenderer handOpenSprite;

	[SerializeField]
	private SpriteRenderer handClosedSprite;

	private readonly Controller controller = new Controller();

	private SortableSample currentSample;

	private int samplesSorted;

	private bool IsComplete => samplesSorted >= sortableSamples.Length;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private void Update()
	{
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (MyNormTask.IsComplete)
		{
			return;
		}
		controller.Update();
		SortableSample[] array = sortableSamples;
		foreach (SortableSample sortableSample in array)
		{
			switch (controller.CheckDrag(sortableSample.Collider))
			{
			case DragState.TouchStart:
				sortableSample.StartPickUp();
				break;
			case DragState.Holding:
			case DragState.Dragging:
			{
				currentSample = sortableSample;
				Vector3 localPosition = ((Component)sortableSample).transform.localPosition;
				Vector2 val = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
				localPosition.x = val.x;
				localPosition.y = val.y;
				sortableSample.UpdatePosition(localPosition);
				UpdateCursorVisibility();
				break;
			}
			case DragState.Released:
				currentSample = null;
				if (sortableSample.IsMushroom && mushroomJar.OverlapPoint(Vector2.op_Implicit(((Component)sortableSample).transform.position)))
				{
					samplesSorted++;
					sortableSample.DropIntoContainer(mushroomJarDropSpot);
				}
				else if (!sortableSample.IsMushroom && crystalJar.OverlapPoint(Vector2.op_Implicit(((Component)sortableSample).transform.position)))
				{
					samplesSorted++;
					sortableSample.DropIntoContainer(crystalJarDropSpot);
				}
				else
				{
					sortableSample.SnapBackToOriginalPosition();
				}
				UpdateCursorVisibility();
				break;
			}
		}
		hand.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		hand.SetLocalZ(-0.5f);
		if (IsComplete)
		{
			UpdateCursorVisibility();
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoWaitBeforeClose());
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SetupInput(disableCursor: false);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		SpriteRenderer[] componentsInChildren = ((Component)hand).GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private IEnumerator CoWaitBeforeClose()
	{
		yield return Effects.Wait(1.5f);
		((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}

	private void UpdateCursorVisibility()
	{
		((Component)hand).gameObject.SetActive(!IsComplete && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
		((Renderer)handClosedSprite).enabled = (Object)(object)currentSample != (Object)null;
		((Renderer)handOpenSprite).enabled = (Object)(object)currentSample == (Object)null;
	}
}
