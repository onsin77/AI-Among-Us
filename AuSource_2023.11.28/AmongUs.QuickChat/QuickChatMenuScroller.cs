using System;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatMenuScroller : MonoBehaviour
{
	public const float SCROLLBAR_WIDTH = 0.2f;

	[SerializeField]
	private Scroller scroller;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private SpriteRenderer track;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private ScrollToSelection scrollToSelection;

	[Header("Gradients")]
	[SerializeField]
	private SpriteRenderer topGradient;

	[SerializeField]
	private Transform topGradientContainer;

	[Space(10f)]
	[SerializeField]
	private SpriteRenderer bottomGradient;

	[SerializeField]
	private Transform bottomGradientContainer;

	private float width;

	private float height;

	private Color topGradientColor;

	private Color bottomGradientColor;

	public bool IsVisible => scrollBar.HandleDrag;

	private void Awake()
	{
		Scroller scroller = this.scroller;
		scroller.OnScrollYEvent = (Scroller.ScrollHandler)Delegate.Combine(scroller.OnScrollYEvent, new Scroller.ScrollHandler(OnScroll));
	}

	public void SetSize(float width, float height)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		this.width = width;
		this.height = height;
		Vector2 val = Vector2.op_Implicit(((Component)track).transform.localPosition);
		val.x = width;
		((Component)track).transform.localPosition = Vector2.op_Implicit(val);
		Vector3 localPosition = ((Component)scrollBar).transform.localPosition;
		localPosition.x = width;
		((Component)scrollBar).transform.localPosition = localPosition;
		Vector3 localPosition2 = ((Component)collider).transform.localPosition;
		localPosition2.x = width / 2f;
		localPosition2.y = height / 2f * -1f;
		((Component)collider).transform.localPosition = localPosition2;
		Vector3 localScale = ((Component)collider).transform.localScale;
		localScale.x = width + 0.2f;
		localScale.y = height;
		((Component)collider).transform.localScale = localScale;
		InitializeGradient();
		float y = height - 0.2f;
		Vector2 size = track.size;
		size.y = y;
		track.size = size;
		float num = height - 0.25f;
		scroller.ScrollbarYBounds = new FloatRange(Mathf.Min(-0.25f, 0f - num), Mathf.Max(-0.25f, 0f - num));
		scroller.ScrollToTop();
	}

	public void SetContentHeight(float contentHeight)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		float min = scroller.GetYBounds().min;
		scroller.SetBounds(new FloatRange(min, Mathf.Max(min, contentHeight + min - ((Component)collider).transform.localScale.y)), null);
		scroller.ScrollToTop();
		InitializeGradient();
	}

	private void InitializeGradient()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)topGradient != (Object)null && topGradientColor.a < 0.1f)
		{
			topGradientColor = topGradient.color;
		}
		if ((Object)(object)bottomGradient != (Object)null && bottomGradientColor.a < 0.1f)
		{
			bottomGradientColor = bottomGradient.color;
		}
		if ((Object)(object)topGradientContainer != (Object)null)
		{
			((Component)topGradientContainer).gameObject.SetActive(IsVisible);
			Vector3 localPosition = topGradientContainer.localPosition;
			localPosition.y = 0f;
			topGradientContainer.localPosition = localPosition;
			Vector3 localScale = topGradientContainer.localScale;
			localScale.x = width + Mathf.Abs(localPosition.x) * 2f;
			topGradientContainer.localScale = localScale;
		}
		if ((Object)(object)bottomGradientContainer != (Object)null)
		{
			((Component)bottomGradientContainer).gameObject.SetActive(IsVisible);
			Vector3 localPosition2 = bottomGradientContainer.localPosition;
			localPosition2.y = 0f - height;
			bottomGradientContainer.localPosition = localPosition2;
			Vector3 localScale2 = bottomGradientContainer.localScale;
			localScale2.x = width + Mathf.Abs(localPosition2.x) * 2f;
			bottomGradientContainer.localScale = localScale2;
		}
		UpdateGradientAlpha(0f);
	}

	private void UpdateGradientAlpha(float value)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)topGradient != (Object)null)
		{
			Color color = topGradientColor;
			color.a *= GetPercentage(0f, 0.4f, value);
			topGradient.color = color;
		}
		if ((Object)(object)bottomGradient != (Object)null)
		{
			Color color2 = bottomGradientColor;
			color2.a *= 1f - GetPercentage(0.6f, 1f, value);
			bottomGradient.color = color2;
		}
	}

	private static float GetPercentage(float min, float max, float input)
	{
		return Mathf.Clamp01((input - min) / (max - min));
	}

	private void OnScroll(float value)
	{
		UpdateGradientAlpha(value);
	}
}
