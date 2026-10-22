using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoastMarshmallowStickMinigame : Minigame
{
	private enum State
	{
		Ready,
		Ending,
		Ended
	}

	[Header("Stick Minigame")]
	[SerializeField]
	private BoxCollider2D[] spawnRegions;

	[SerializeField]
	private Transform stickTransform;

	[SerializeField]
	private GameObject stickSprite;

	[SerializeField]
	private ParticleSystem stickParticles;

	[SerializeField]
	private Transform[] otherRandomObjects;

	[SerializeField]
	private PassiveButton stickButton;

	[SerializeField]
	private Transform hand;

	private State state;

	public override bool SkipMultistageOverlayMenuSetup => true;

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		hand.position = Vector2.op_Implicit(VirtualCursor.currentPosition);
		hand.SetLocalZ(0.5f);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected Obj, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		SetupInput(disableCursor: false);
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
		UpdateCursorVisibility();
		SpriteRenderer[] componentsInChildren = ((Component)hand).GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer playerMaterialColors in componentsInChildren)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		state = State.Ready;
		((UnityEvent)stickButton.OnClick).AddListener((UnityAction)OnStickPressed);
		List<BoxCollider2D> list = new List<BoxCollider2D>(spawnRegions);
		list.Shuffle();
		Transform val = stickTransform;
		Bounds bounds = ((Collider2D)list[0]).bounds;
		float x = bounds.min.x;
		bounds = ((Collider2D)list[0]).bounds;
		float num = Random.Range(x, bounds.max.x);
		bounds = ((Collider2D)list[0]).bounds;
		float y = bounds.min.y;
		bounds = ((Collider2D)list[0]).bounds;
		val.position = new Vector3(num, Random.Range(y, bounds.max.y), stickTransform.position.z);
		list.RemoveAt(0);
		for (int j = 0; j < otherRandomObjects.Length; j++)
		{
			BoxCollider2D val2 = ((j >= list.Count) ? spawnRegions.Random() : list[j]);
			Transform val3 = otherRandomObjects[j];
			bounds = ((Collider2D)val2).bounds;
			float x2 = bounds.min.x;
			bounds = ((Collider2D)val2).bounds;
			float num2 = Random.Range(x2, bounds.max.x);
			bounds = ((Collider2D)val2).bounds;
			float y2 = bounds.min.y;
			bounds = ((Collider2D)val2).bounds;
			val3.position = new Vector3(num2, Random.Range(y2, bounds.max.y), val3.position.z);
		}
	}

	public override void Close()
	{
		base.Close();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputMethodChanged));
	}

	private IEnumerator CoComplete()
	{
		state = State.Ending;
		stickParticles.Play();
		stickSprite.SetActive(false);
		UpdateCursorVisibility();
		yield return (object)new WaitForSeconds(1f);
		MyNormTask.NextStep();
		((Component)((Component)this).transform.parent).GetComponent<MultistageMinigame>().Close();
		state = State.Ended;
	}

	private void OnStickPressed()
	{
		if (state == State.Ready)
		{
			VibrationManager.Vibrate(0.3f, 0.3f, 0.1f);
			((MonoBehaviour)this).StartCoroutine(CoComplete());
		}
	}

	private void OnInputMethodChanged()
	{
		UpdateCursorVisibility();
	}

	private void UpdateCursorVisibility()
	{
		((Component)hand).gameObject.SetActive(state == State.Ready && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
	}
}
