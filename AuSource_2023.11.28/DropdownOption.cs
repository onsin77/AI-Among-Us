using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DropdownOption : PoolableBehavior
{
	[SerializeField]
	private TextMeshPro optionText;

	[SerializeField]
	private PassiveButton button;

	public UiElement ButtonUiElement => button;

	public void Initialize(string text, Action onClick, Collider2D clickMask, Vector3 position)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected Obj, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)optionText).text = text;
		((UnityEventBase)button.OnClick).RemoveAllListeners();
		((UnityEvent)button.OnClick).AddListener((UnityAction)(() =>
		{
			onClick();
		}));
		button.ClickMask = clickMask;
		((Component)this).transform.localPosition = position;
	}

	public override void Reset()
	{
		((TMP_Text)optionText).text = string.Empty;
		((UnityEventBase)button.OnClick).RemoveAllListeners();
		button.ClickMask = null;
	}
}
