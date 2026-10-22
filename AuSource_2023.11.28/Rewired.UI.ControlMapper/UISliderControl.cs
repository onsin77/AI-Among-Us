using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class UISliderControl : UIControl
{
	public Image iconImage;

	public Slider slider;

	private bool _showIcon;

	private bool _showSlider;

	public bool showIcon
	{
		get
		{
			return _showIcon;
		}
		set
		{
			if (!((Object)(object)iconImage == (Object)null))
			{
				((Component)iconImage).gameObject.SetActive(value);
				_showIcon = value;
			}
		}
	}

	public bool showSlider
	{
		get
		{
			return _showSlider;
		}
		set
		{
			if (!((Object)(object)slider == (Object)null))
			{
				((Component)slider).gameObject.SetActive(value);
				_showSlider = value;
			}
		}
	}

	public override void SetCancelCallback(Action cancelCallback)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected Obj, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected Obj, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected Obj, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		base.SetCancelCallback(cancelCallback);
		if (cancelCallback == null || (Object)(object)slider == (Object)null)
		{
			return;
		}
		if (slider is ICustomSelectable)
		{
			(slider as ICustomSelectable).CancelEvent += () =>
			{
				cancelCallback();
			};
			return;
		}
		EventTrigger val = ((Component)slider).GetComponent<EventTrigger>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)slider).gameObject.AddComponent<EventTrigger>();
		}
		Entry val2 = new Entry();
		val2.callback = new TriggerEvent();
		val2.eventID = (EventTriggerType)16;
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)((BaseEventData data) =>
		{
			cancelCallback();
		}));
		if (val.triggers == null)
		{
			val.triggers = new List<Entry>();
		}
		val.triggers.Add(val2);
	}
}
