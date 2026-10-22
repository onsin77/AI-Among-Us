using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class CustomButton : Button, ICustomSelectable, ICancelHandler, IEventSystemHandler
{
	[SerializeField]
	private Sprite _disabledHighlightedSprite;

	[SerializeField]
	private Color _disabledHighlightedColor;

	[SerializeField]
	private string _disabledHighlightedTrigger;

	[SerializeField]
	private bool _autoNavUp = true;

	[SerializeField]
	private bool _autoNavDown = true;

	[SerializeField]
	private bool _autoNavLeft = true;

	[SerializeField]
	private bool _autoNavRight = true;

	private bool isHighlightDisabled;

	[CompilerGenerated]
	private UnityAction m__CancelEvent;

	public Sprite disabledHighlightedSprite
	{
		get
		{
			return _disabledHighlightedSprite;
		}
		set
		{
			_disabledHighlightedSprite = value;
		}
	}

	public Color disabledHighlightedColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _disabledHighlightedColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_disabledHighlightedColor = value;
		}
	}

	public string disabledHighlightedTrigger
	{
		get
		{
			return _disabledHighlightedTrigger;
		}
		set
		{
			_disabledHighlightedTrigger = value;
		}
	}

	public bool autoNavUp
	{
		get
		{
			return _autoNavUp;
		}
		set
		{
			_autoNavUp = value;
		}
	}

	public bool autoNavDown
	{
		get
		{
			return _autoNavDown;
		}
		set
		{
			_autoNavDown = value;
		}
	}

	public bool autoNavLeft
	{
		get
		{
			return _autoNavLeft;
		}
		set
		{
			_autoNavLeft = value;
		}
	}

	public bool autoNavRight
	{
		get
		{
			return _autoNavRight;
		}
		set
		{
			_autoNavRight = value;
		}
	}

	private bool isDisabled => !((Selectable)this).IsInteractable();

	private event UnityAction _CancelEvent
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			UnityAction val = this.m__CancelEvent;
			UnityAction val2;
			do
			{
				val2 = val;
				UnityAction value2 = (UnityAction)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m__CancelEvent, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			UnityAction val = this.m__CancelEvent;
			UnityAction val2;
			do
			{
				val2 = val;
				UnityAction value2 = (UnityAction)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m__CancelEvent, value2, val2);
			}
			while (val != val2);
		}
	}

	public event UnityAction CancelEvent
	{
		add
		{
			_CancelEvent += value;
		}
		remove
		{
			_CancelEvent -= value;
		}
	}

	public override Selectable FindSelectableOnLeft()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)this).navigation;
		if ((navigation.mode & 1) != 0 || _autoNavLeft)
		{
			return UISelectionUtility.FindNextSelectable((Selectable)(object)this, ((Component)this).transform, Vector3.left);
		}
		return ((Selectable)this).FindSelectableOnLeft();
	}

	public override Selectable FindSelectableOnRight()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)this).navigation;
		if ((navigation.mode & 1) != 0 || _autoNavRight)
		{
			return UISelectionUtility.FindNextSelectable((Selectable)(object)this, ((Component)this).transform, Vector3.right);
		}
		return ((Selectable)this).FindSelectableOnRight();
	}

	public override Selectable FindSelectableOnUp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)this).navigation;
		if ((navigation.mode & 2) != 0 || _autoNavUp)
		{
			return UISelectionUtility.FindNextSelectable((Selectable)(object)this, ((Component)this).transform, Vector3.up);
		}
		return ((Selectable)this).FindSelectableOnUp();
	}

	public override Selectable FindSelectableOnDown()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = ((Selectable)this).navigation;
		if ((navigation.mode & 2) != 0 || _autoNavDown)
		{
			return UISelectionUtility.FindNextSelectable((Selectable)(object)this, ((Component)this).transform, Vector3.down);
		}
		return ((Selectable)this).FindSelectableOnDown();
	}

	protected override void OnCanvasGroupChanged()
	{
		((Selectable)this).OnCanvasGroupChanged();
		if (!((Object)(object)EventSystem.current == (Object)null))
		{
			EvaluateHightlightDisabled((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)this).gameObject);
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected I4, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (isHighlightDisabled)
		{
			Color val = _disabledHighlightedColor;
			Sprite newSprite = _disabledHighlightedSprite;
			string triggername = _disabledHighlightedTrigger;
			if (((Component)this).gameObject.activeInHierarchy)
			{
				Transition transition = ((Selectable)this).transition;
				switch (transition - 1)
				{
				case 0:
				{
					ColorBlock colors = ((Selectable)this).colors;
					StartColorTween(val * colors.colorMultiplier, instant);
					break;
				}
				case 1:
					DoSpriteSwap(newSprite);
					break;
				case 2:
					TriggerAnimation(triggername);
					break;
				}
			}
		}
		else
		{
			((Selectable)this).DoStateTransition(state, instant);
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((Selectable)this).targetGraphic == (Object)null))
		{
			Graphic targetGraphic = ((Selectable)this).targetGraphic;
			float num;
			if (!instant)
			{
				ColorBlock colors = ((Selectable)this).colors;
				num = colors.fadeDuration;
			}
			else
			{
				num = 0f;
			}
			targetGraphic.CrossFadeColor(targetColor, num, true, true);
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!((Object)(object)((Selectable)this).image == (Object)null))
		{
			((Selectable)this).image.overrideSprite = newSprite;
		}
	}

	private void TriggerAnimation(string triggername)
	{
		if (!((Object)(object)((Selectable)this).animator == (Object)null) && ((Behaviour)((Selectable)this).animator).enabled && ((Behaviour)((Selectable)this).animator).isActiveAndEnabled && !((Object)(object)((Selectable)this).animator.runtimeAnimatorController == (Object)null) && !string.IsNullOrEmpty(triggername))
		{
			((Selectable)this).animator.ResetTrigger(_disabledHighlightedTrigger);
			((Selectable)this).animator.SetTrigger(triggername);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		EvaluateHightlightDisabled(isSelected: true);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		((Selectable)this).OnDeselect(eventData);
		EvaluateHightlightDisabled(isSelected: false);
	}

	private void Press()
	{
		if (((UIBehaviour)this).IsActive() && ((Selectable)this).IsInteractable())
		{
			((UnityEvent)((Button)this).onClick).Invoke();
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (((UIBehaviour)this).IsActive() && ((Selectable)this).IsInteractable() && (int)eventData.button == 0)
		{
			Press();
			if (!((UIBehaviour)this).IsActive() || !((Selectable)this).IsInteractable())
			{
				isHighlightDisabled = true;
				((Selectable)this).DoStateTransition((SelectionState)4, false);
			}
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		Press();
		if (!((UIBehaviour)this).IsActive() || !((Selectable)this).IsInteractable())
		{
			isHighlightDisabled = true;
			((Selectable)this).DoStateTransition((SelectionState)4, false);
		}
		else
		{
			((Selectable)this).DoStateTransition((SelectionState)2, false);
			((MonoBehaviour)this).StartCoroutine(OnFinishSubmit());
		}
	}

	private IEnumerator OnFinishSubmit()
	{
		ColorBlock colors = ((Selectable)this).colors;
		float fadeTime = colors.fadeDuration;
		float elapsedTime = 0f;
		while (elapsedTime < fadeTime)
		{
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}
		((Selectable)this).DoStateTransition(((Selectable)this).currentSelectionState, false);
	}

	private void EvaluateHightlightDisabled(bool isSelected)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!isSelected)
		{
			if (isHighlightDisabled)
			{
				isHighlightDisabled = false;
				SelectionState val = (SelectionState)(isDisabled ? 4 : ((int)((Selectable)this).currentSelectionState));
				((Selectable)this).DoStateTransition(val, false);
			}
		}
		else if (isDisabled)
		{
			isHighlightDisabled = true;
			((Selectable)this).DoStateTransition((SelectionState)4, false);
		}
	}

	public void OnCancel(BaseEventData eventData)
	{
		if (this.m__CancelEvent != null)
		{
			this.m__CancelEvent.Invoke();
		}
	}
}
