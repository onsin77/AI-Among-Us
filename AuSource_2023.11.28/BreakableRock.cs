using System;
using System.Collections;
using UnityEngine;

public class BreakableRock : MonoBehaviour
{
	private const float FALL_DURATION = 0.3f;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private Sprite[] rockSprites;

	[SerializeField]
	private PassiveButton passiveButton;

	[SerializeField]
	private AnimationCurve animCurve;

	[SerializeField]
	private CollectableOre myOre;

	[SerializeField]
	private Collider2D col;

	private Action<CollectableOre, Vector3> powerMeterAction;

	private Controller controller;

	public bool CanInteract { get; private set; } = true;

	private void Update()
	{
		if (CanInteract && controller.CheckDrag(col) == DragState.TouchStart)
		{
			TouchRock();
		}
	}

	public void Initialize(Action<CollectableOre, Vector3> powerAction, Controller control)
	{
		controller = control;
		powerMeterAction = (Action<CollectableOre, Vector3>)Delegate.Combine(powerMeterAction, powerAction);
		spriteRenderer.sprite = rockSprites.Random();
	}

	private void TouchRock()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (CanInteract && !IsUsingController())
		{
			powerMeterAction?.Invoke(myOre, Vector2.op_Implicit(controller.HoverPosition));
		}
	}

	public CollectableOre GetOre()
	{
		return myOre;
	}

	public void SendPower()
	{
		((MonoBehaviour)this).StartCoroutine(FallRockAnim());
	}

	public void SetInput(bool isEnabled)
	{
		CanInteract = isEnabled;
	}

	private IEnumerator FallRockAnim()
	{
		Vector3 position = ((Component)this).transform.position;
		float num = ((Random.Range(0, 2) == 0) ? Random.Range(-0.5f, -1.5f) : Random.Range(0.5f, 1.5f));
		position.x += num;
		((MonoBehaviour)this).StartCoroutine(Effects.Sequence(Effects.Wait(0.15f), Effects.ColorFade(spriteRenderer, Color.white, Color.clear, 0.15f)));
		yield return Effects.CurvePositionY(((Component)this).transform, animCurve, 0.3f, num);
		((Component)this).gameObject.SetActive(false);
	}

	private bool IsUsingController()
	{
		return ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
	}
}
