using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class UIControlSet : MonoBehaviour
{
	[SerializeField]
	private TMP_Text title;

	private Dictionary<int, UIControl> _controls;

	private Dictionary<int, UIControl> controls => _controls ?? (_controls = new Dictionary<int, UIControl>());

	public void SetTitle(string text)
	{
		if (!((Object)(object)title == (Object)null))
		{
			title.text = text;
		}
	}

	public T GetControl<T>(int uniqueId) where T : UIControl
	{
		controls.TryGetValue(uniqueId, out var value);
		return value as T;
	}

	public UISliderControl CreateSlider(GameObject prefab, Sprite icon, float minValue, float maxValue, Action<int, float> valueChangedCallback, Action<int> cancelCallback)
	{
		GameObject val = Object.Instantiate<GameObject>(prefab);
		UISliderControl control = val.GetComponent<UISliderControl>();
		if ((Object)(object)control == (Object)null)
		{
			Object.Destroy((Object)(object)val);
			Debug.LogError((object)"Prefab missing UISliderControl component!");
			return null;
		}
		val.transform.SetParent(((Component)this).transform, false);
		if ((Object)(object)control.iconImage != (Object)null)
		{
			control.iconImage.sprite = icon;
		}
		if ((Object)(object)control.slider != (Object)null)
		{
			control.slider.minValue = minValue;
			control.slider.maxValue = maxValue;
			if (valueChangedCallback != null)
			{
				((UnityEvent<float>)(object)control.slider.onValueChanged).AddListener((UnityAction<float>)((float value) =>
				{
					valueChangedCallback(control.id, value);
				}));
			}
			if (cancelCallback != null)
			{
				control.SetCancelCallback(() =>
				{
					cancelCallback(control.id);
				});
			}
		}
		controls.Add(control.id, control);
		return control;
	}
}
