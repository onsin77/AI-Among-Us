using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Scroller : PassiveUiElement
{
	public delegate void ScrollHandler(float value);

	public Transform Inner;

	public bool allowY;

	public bool showY = true;

	[SerializeField]
	[FormerlySerializedAs("YBounds")]
	private FloatRange ContentYBounds = new FloatRange(-10f, 10f);

	[FormerlySerializedAs("ScrollerYRange")]
	public FloatRange ScrollbarYBounds;

	[FormerlySerializedAs("ScrollerY")]
	public Scrollbar ScrollbarY;

	public bool allowX;

	public bool showX = true;

	[FormerlySerializedAs("XBounds")]
	[SerializeField]
	private FloatRange ContentXBounds = new FloatRange(-10f, 10f);

	[FormerlySerializedAs("ScrollerXRange")]
	public FloatRange ScrollbarXBounds;

	[FormerlySerializedAs("ScrollerX")]
	public Scrollbar ScrollbarX;

	public float DragScrollSpeed = 1f;

	public float ScrollWheelSpeed = 0.25f;

	public bool MouseMustBeOverToScroll;

	private Vector2 velocity;

	private bool active;

	private bool mouseOver;

	public ScrollHandler OnScrollXEvent;

	public ScrollHandler OnScrollYEvent;

	public override bool HandleUp => true;

	public override bool HandleDown => true;

	public override bool HandleDrag => true;

	public override bool HandleOverOut => MouseMustBeOverToScroll;

	public bool AtTop
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Inner.localPosition.y <= ContentYBounds.min + 0.25f;
		}
	}

	public bool AtBottom
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Inner.localPosition.y >= ContentYBounds.max - 0.25f;
		}
	}

	public bool AtLeft
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Inner.localPosition.x <= ContentXBounds.min + 0.25f;
		}
	}

	public bool AtRight
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Inner.localPosition.x >= ContentXBounds.max - 0.25f;
		}
	}

	public Collider2D Hitbox => Colliders[0];

	public void SetBounds(FloatRange yBounds, FloatRange xBounds)
	{
		if (yBounds != null)
		{
			ContentYBounds = yBounds;
		}
		if (xBounds != null)
		{
			ContentXBounds = xBounds;
		}
		UpdateScrollBars();
	}

	public void CalculateAndSetYBounds(float amount, float numPerRow, float numRowsVisible, float spacing)
	{
		ContentYBounds.max = Mathf.Max(0f, (float)Mathf.CeilToInt((amount - numPerRow * numRowsVisible) / numPerRow) * spacing);
		UpdateScrollBars();
	}

	public void SetBoundsMax(float yMax, float xMax)
	{
		ContentYBounds.max = yMax;
		ContentXBounds.max = xMax;
		UpdateScrollBars();
	}

	public void SetYBoundsMax(float yMax)
	{
		ContentYBounds.max = yMax;
		UpdateScrollBars();
	}

	public void SetBoundsMin(float yMin, float xMin)
	{
		ContentYBounds.min = yMin;
		ContentXBounds.min = xMin;
		UpdateScrollBars();
	}

	public void SetYBoundsMin(float yMin)
	{
		ContentYBounds.min = yMin;
		UpdateScrollBars();
	}

	public FloatRange GetYBounds()
	{
		return ContentYBounds;
	}

	public FloatRange GetXBounds()
	{
		return ContentXBounds;
	}

	protected override void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!active && velocity.sqrMagnitude > 0.01f)
		{
			velocity = Vector2.ClampMagnitude(velocity, velocity.magnitude - 10f * Time.deltaTime);
			ScrollRelative(velocity * Time.deltaTime);
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)Inner))
			{
				return;
			}
			if (MouseMustBeOverToScroll && Object.op_Implicit((Object)(object)ClickMask))
			{
				Vector2 position = DestroyableSingleton<PassiveButtonManager>.Instance.controller.GetTouch(0).Position;
				mouseOver = ClickMask.OverlapPoint(position);
			}
			if (!MouseMustBeOverToScroll || mouseOver)
			{
				Vector2 val = Input.mouseScrollDelta * ScrollWheelSpeed;
				if (val.y != 0f)
				{
					val.y = 0f - val.y;
					ScrollRelative(val);
				}
				if (val.x != 0f)
				{
					val.x = 0f - val.x;
					ScrollRelative(val);
				}
			}
		}
	}

	public void ScrollDown()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Collider2D val = Colliders.First();
		Bounds bounds = val.bounds;
		float y = bounds.max.y;
		bounds = val.bounds;
		float num = y - bounds.min.y;
		ScrollRelative(new Vector2(0f, num * 0.75f));
	}

	public void ScrollUp()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Collider2D val = Colliders.First();
		Bounds bounds = val.bounds;
		float y = bounds.max.y;
		bounds = val.bounds;
		float num = y - bounds.min.y;
		ScrollRelative(new Vector2(0f, num * -0.75f));
	}

	public float GetScrollPercY()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (ContentYBounds.Width < 0.0001f)
		{
			return 1f;
		}
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		return ContentYBounds.ReverseLerp(localPosition.y);
	}

	public float GetScrollPercX()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (ContentXBounds.Width < 0.0001f)
		{
			return 1f;
		}
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		return ContentXBounds.ReverseLerp(localPosition.x);
	}

	public void ScrollPercentY(float p)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		localPosition.y = ContentYBounds.Lerp(p);
		((Component)Inner).transform.localPosition = localPosition;
		UpdateScrollBars();
	}

	public void ScrollPercentX(float p)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		localPosition.x = ContentXBounds.Lerp(p);
		((Component)Inner).transform.localPosition = localPosition;
		UpdateScrollBars();
	}

	public override void ReceiveClickDown()
	{
		active = true;
	}

	public override void ReceiveClickUp()
	{
		active = false;
	}

	public override void ReceiveClickDrag(Vector2 dragDelta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		velocity = dragDelta / Time.deltaTime * 0.9f;
		ScrollRelative(dragDelta);
	}

	public void ScrollToScrollbarPositionY(Vector3 newScrollbarPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		newScrollbarPosition.y = ScrollbarYBounds.Clamp(newScrollbarPosition.y);
		float p = 1f - ScrollbarYBounds.ReverseLerp(newScrollbarPosition.y);
		ScrollPercentY(p);
	}

	public void ScrollToScrollbarPositionX(Vector3 newScrollbarPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		newScrollbarPosition.x = ScrollbarXBounds.Clamp(newScrollbarPosition.x);
		float p = 1f - ScrollbarXBounds.ReverseLerp(newScrollbarPosition.x);
		ScrollPercentX(p);
	}

	public void ScrollRelative(Vector2 dragDelta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		dragDelta *= DragScrollSpeed;
		if (!allowX)
		{
			dragDelta.x = 0f;
		}
		if (!allowY)
		{
			dragDelta.y = 0f;
		}
		Vector3 val = ((Component)Inner).transform.localPosition + Vector2.op_Implicit(dragDelta);
		float num = Mathf.Max(ContentYBounds.min, ContentYBounds.max);
		val.y = Mathf.Clamp(val.y, ContentYBounds.min, num);
		float num2 = Mathf.Max(ContentXBounds.min, ContentXBounds.max);
		val.x = Mathf.Clamp(val.x, ContentXBounds.min, num2);
		((Component)Inner).transform.localPosition = val;
		UpdateScrollBars();
	}

	public void UpdateScrollBars()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ScrollbarY))
		{
			if (!showY || ContentYBounds.min >= ContentYBounds.max)
			{
				ScrollbarY.Toggle(on: false);
			}
			else
			{
				ScrollbarY.Toggle(on: true);
				float num = ContentYBounds.ReverseLerp(((Component)Inner).transform.localPosition.y);
				Vector3 localPosition = ((Component)ScrollbarY).transform.localPosition;
				localPosition.y = ScrollbarYBounds.Lerp(1f - num);
				ScrollbarY.UpdatePosition(localPosition);
			}
			OnScrollYEvent?.Invoke(GetScrollPercY());
		}
		if (Object.op_Implicit((Object)(object)ScrollbarX))
		{
			if (!showX || ContentXBounds.min >= ContentXBounds.max)
			{
				ScrollbarX.Toggle(on: false);
			}
			else
			{
				ScrollbarX.Toggle(on: true);
				float num2 = ContentXBounds.ReverseLerp(((Component)Inner).transform.localPosition.x);
				Vector3 localPosition2 = ((Component)ScrollbarX).transform.localPosition;
				localPosition2.x = ScrollbarXBounds.Lerp(1f - num2);
				ScrollbarX.UpdatePosition(localPosition2);
			}
			OnScrollXEvent?.Invoke(GetScrollPercX());
		}
	}

	public void ScrollToTop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector2.zero;
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		localPosition.y = ContentYBounds.min;
		((Component)Inner).transform.localPosition = localPosition;
		UpdateScrollBars();
	}
}
