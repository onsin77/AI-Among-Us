using Rewired.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
[RequireComponent(typeof(Selectable))]
public class ScrollRectSelectableChild : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public bool useCustomEdgePadding;

	public float customEdgePadding = 50f;

	private ScrollRect parentScrollRect;

	private Selectable _selectable;

	private RectTransform parentScrollRectContentTransform => parentScrollRect.content;

	private Selectable selectable => _selectable ?? (_selectable = ((Component)this).GetComponent<Selectable>());

	private RectTransform rectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private void Start()
	{
		parentScrollRect = ((Component)((Component)this).transform).GetComponentInParent<ScrollRect>();
		if ((Object)(object)parentScrollRect == (Object)null)
		{
			Debug.LogError((object)"Rewired Control Mapper: No ScrollRect found! This component must be a child of a ScrollRect!");
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)parentScrollRect == (Object)null) && eventData is AxisEventData)
		{
			Transform transform = ((Component)parentScrollRect).transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			Rect val2 = MathTools.TransformRect(rectTransform.rect, (Transform)(object)rectTransform, (Transform)(object)val);
			Rect rect = val.rect;
			Rect rect2 = val.rect;
			float num = ((!useCustomEdgePadding) ? val2.height : customEdgePadding);
			rect2.yMax -= num;
			rect2.yMin += num;
			Vector2 val3 = default;
			if (!MathTools.RectContains(rect2, val2) && MathTools.GetOffsetToContainRect(rect2, val2, ref val3))
			{
				Vector2 anchoredPosition = parentScrollRectContentTransform.anchoredPosition;
				anchoredPosition.x = Mathf.Clamp(anchoredPosition.x + val3.x, 0f, Mathf.Abs(rect.width - parentScrollRectContentTransform.sizeDelta.x));
				anchoredPosition.y = Mathf.Clamp(anchoredPosition.y + val3.y, 0f, Mathf.Abs(rect.height - parentScrollRectContentTransform.sizeDelta.y));
				parentScrollRectContentTransform.anchoredPosition = anchoredPosition;
			}
		}
	}
}
