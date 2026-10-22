using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(RectTransform))]
public sealed class UIPointer : UIBehaviour
{
	[SerializeField]
	[Tooltip("Should the hardware pointer be hidden?")]
	private bool _hideHardwarePointer = true;

	[SerializeField]
	[Tooltip("Sets the pointer to the last sibling in the parent hierarchy. Do not enable this on multiple UIPointers under the same parent transform or they will constantly fight each other for dominance.")]
	private bool _autoSort = true;

	private Canvas _canvas;

	public bool autoSort
	{
		get
		{
			return _autoSort;
		}
		set
		{
			if (value != _autoSort)
			{
				_autoSort = value;
				if (value)
				{
					((Component)this).transform.SetAsLastSibling();
				}
			}
		}
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		Graphic[] componentsInChildren = ((Component)this).GetComponentsInChildren<Graphic>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].raycastTarget = false;
		}
		if (_hideHardwarePointer)
		{
			Cursor.visible = false;
		}
		if (_autoSort)
		{
			((Component)this).transform.SetAsLastSibling();
		}
		GetDependencies();
	}

	private void Update()
	{
		if (_autoSort && ((Component)this).transform.GetSiblingIndex() < ((Component)this).transform.parent.childCount - 1)
		{
			((Component)this).transform.SetAsLastSibling();
		}
	}

	protected override void OnTransformParentChanged()
	{
		((UIBehaviour)this).OnTransformParentChanged();
		GetDependencies();
	}

	protected override void OnCanvasGroupChanged()
	{
		((UIBehaviour)this).OnCanvasGroupChanged();
		GetDependencies();
	}

	public void OnScreenPositionChanged(Vector2 screenPosition)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (!((Object)(object)_canvas == (Object)null))
		{
			Camera val = null;
			RenderMode renderMode = _canvas.renderMode;
			if ((int)renderMode != 0 && renderMode - 1 <= 1)
			{
				val = _canvas.worldCamera;
			}
			Transform parent = ((Component)this).transform.parent;
			Vector2 val2 = default;
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((parent is RectTransform) ? parent : null), screenPosition, val, ref val2);
			((Component)this).transform.localPosition = new Vector3(val2.x, val2.y, ((Component)this).transform.localPosition.z);
		}
	}

	private void GetDependencies()
	{
		_canvas = ((Component)((Component)this).transform.root).GetComponentInChildren<Canvas>();
	}
}
