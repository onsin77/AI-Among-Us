using UnityEngine;

public class Scrollbar : PassiveUiElement
{
	[SerializeField]
	private Scroller parent;

	[SerializeField]
	private SpriteRenderer graphic;

	[SerializeField]
	private SpriteRenderer trackGraphic;

	[SerializeField]
	private bool horizontal;

	[SerializeField]
	private float dragSpeed;

	private bool dragable = true;

	public override bool HandleDrag => dragable;

	public void Toggle(bool on)
	{
		((Renderer)graphic).enabled = on;
		if ((Object)(object)trackGraphic != (Object)null)
		{
			((Renderer)trackGraphic).enabled = on;
		}
		dragable = on;
	}

	public void UpdatePosition(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Component)graphic).transform.localPosition = position;
	}

	public override void ReceiveClickDrag(Vector2 dragDelta)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (horizontal)
		{
			dragDelta.y = 0f;
			dragDelta.x = 0f - dragDelta.x;
			parent.ScrollToScrollbarPositionX(((Component)graphic).transform.localPosition - new Vector3(dragDelta.x, dragDelta.y));
		}
		else
		{
			dragDelta.x = 0f;
			dragDelta.y = 0f - dragDelta.y;
			parent.ScrollToScrollbarPositionY(((Component)graphic).transform.localPosition - new Vector3(dragDelta.x, dragDelta.y));
		}
	}
}
