using UnityEngine;

[RequireComponent(typeof(Scroller))]
public class ScrollToSelection : MonoBehaviour
{
	[SerializeField]
	private float scrollOffset;

	private Scroller scrollRect;

	private bool manuallyScrolling;

	private UIScrollbarHelper[] childElements;

	public Vector2 wantedValue;

	private GameObject lastSelectedObject;

	public bool cursorEnabled;

	public bool killScroll;

	public bool onePage;

	private void Awake()
	{
		scrollRect = ((Component)this).GetComponent<Scroller>();
	}

	private void OnEnable()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		childElements = ((Component)this).GetComponentsInChildren<UIScrollbarHelper>(false);
		scrollRect = ((Component)this).GetComponent<Scroller>();
		wantedValue = Vector2.zero;
		ClearSelections();
		for (int i = 0; i < childElements.Length; i++)
		{
			childElements[i].index = i;
			childElements[i].isDirectChild = (Object)(object)((Component)childElements[i]).transform.parent == (Object)(object)((Component)this).transform;
		}
	}

	private void Start()
	{
		childElements = ((Component)this).GetComponentsInChildren<UIScrollbarHelper>(false);
		for (int i = 0; i < childElements.Length; i++)
		{
			childElements[i].index = i;
			childElements[i].isDirectChild = (Object)(object)((Component)childElements[i]).transform.parent == (Object)(object)scrollRect.Inner;
		}
	}

	public void Toggle(bool on)
	{
		manuallyScrolling = !on;
	}

	public void ClearSelections()
	{
		lastSelectedObject = null;
	}

	private void LateUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		if (!((Behaviour)scrollRect).enabled)
		{
			onePage = true;
			return;
		}
		cursorEnabled = Controller.currentTouchType != Controller.TouchType.Joystick;
		if (cursorEnabled)
		{
			wantedValue = Vector2.op_Implicit(scrollRect.Inner.localPosition);
			return;
		}
		onePage = false;
		GameObject val = (Object.op_Implicit((Object)(object)ControllerManager.Instance.CurrentUiState.CurrentSelection) ? ((Component)ControllerManager.Instance.CurrentUiState.CurrentSelection).gameObject : null);
		if ((Object)(object)lastSelectedObject != (Object)null && (Object)(object)lastSelectedObject != (Object)(object)val && manuallyScrolling)
		{
			Toggle(on: true);
		}
		if ((Object)(object)lastSelectedObject != (Object)(object)val)
		{
			if (Object.op_Implicit((Object)(object)val) && val.transform.IsChildOf(((Component)this).transform))
			{
				ScrollToRect(val.transform);
			}
			lastSelectedObject = val;
			if (!Object.op_Implicit((Object)(object)val))
			{
				killScroll = true;
			}
		}
		if (manuallyScrolling)
		{
			return;
		}
		if (scrollRect.allowY)
		{
			if (!killScroll && scrollRect.Inner.localPosition.y != wantedValue.y)
			{
				Vector3 localPosition = scrollRect.Inner.localPosition;
				localPosition.y = Mathf.Lerp(localPosition.y, wantedValue.y, Time.unscaledDeltaTime * 3f);
				scrollRect.Inner.localPosition = localPosition;
				scrollRect.UpdateScrollBars();
			}
			else if (killScroll)
			{
				wantedValue.y = scrollRect.Inner.localPosition.y;
			}
		}
		if (scrollRect.allowX)
		{
			if (!killScroll && scrollRect.Inner.localPosition.x != wantedValue.x)
			{
				Vector3 localPosition2 = scrollRect.Inner.localPosition;
				localPosition2.x = Mathf.Lerp(localPosition2.x, wantedValue.x, Time.unscaledDeltaTime * 3f);
				scrollRect.Inner.localPosition = localPosition2;
				scrollRect.UpdateScrollBars();
			}
			else if (killScroll)
			{
				wantedValue.x = scrollRect.Inner.localPosition.x;
			}
		}
	}

	private void ScrollToRect(Transform targetRectTransform)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		if (manuallyScrolling || (!scrollRect.allowY && !scrollRect.allowX))
		{
			return;
		}
		UIScrollbarHelper uIScrollbarHelper = ((Component)targetRectTransform).GetComponent<UIScrollbarHelper>();
		if ((Object)(object)uIScrollbarHelper == (Object)null)
		{
			uIScrollbarHelper = ((Component)targetRectTransform).GetComponentInParent<UIScrollbarHelper>();
		}
		Vector3 val = Vector3.zero;
		if (scrollRect.Colliders.Length != 0)
		{
			Transform transform = ((Component)scrollRect.Colliders[0]).transform;
			if ((Object)(object)transform.parent == (Object)(object)((Component)this).transform)
			{
				val = ((Component)scrollRect.Colliders[0]).transform.localPosition;
			}
			else if ((Object)(object)transform != (Object)(object)((Component)scrollRect).transform)
			{
				Vector3 position = ((Component)scrollRect.Colliders[0]).transform.position;
				val = ((Component)this).transform.InverseTransformPoint(position);
			}
		}
		if ((Object)(object)uIScrollbarHelper == (Object)null)
		{
			wantedValue = Vector2.op_Implicit(-((Component)scrollRect.Inner).transform.localPosition + val);
			killScroll = true;
			return;
		}
		if (childElements.Length == 0)
		{
			childElements = ((Component)this).GetComponentsInChildren<UIScrollbarHelper>(false);
			for (int i = 0; i < childElements.Length; i++)
			{
				childElements[i].index = i;
			}
			if (childElements.Length == 0)
			{
				return;
			}
		}
		killScroll = false;
		Vector2 localPosition = uIScrollbarHelper.GetLocalPosition(scrollRect);
		if (scrollRect.allowY)
		{
			wantedValue.y = Mathf.Clamp(0f - localPosition.y + val.y, scrollRect.GetYBounds().min, scrollRect.GetYBounds().max) + scrollOffset;
		}
		if (scrollRect.allowX)
		{
			wantedValue.x = Mathf.Clamp(0f - localPosition.x + val.x, scrollRect.GetXBounds().min, scrollRect.GetXBounds().max);
		}
	}
}
