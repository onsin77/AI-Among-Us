using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FreeChatInputField : AbstractChatInputField
{
	[SerializeField]
	[Space(10f)]
	private TextBoxTMP textArea;

	[SerializeField]
	private PassiveButton fieldButton;

	[SerializeField]
	private TextMeshPro charCountText;

	public string Text => textArea.text;

	public event Action OnChangedEvent;

	protected override void Awake()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		base.Awake();
		fieldButton.AddOnClickListeners(OnFieldPressed);
		((UnityEvent)textArea.OnChange).AddListener((UnityAction)OnFieldChanged);
		((UnityEvent)textArea.OnEnter).AddListener((UnityAction)OnFieldSubmit);
		textArea.characterLimit = 100;
		UpdateState();
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		((Behaviour)textArea).enabled = visible;
	}

	public override void Clear()
	{
		base.Clear();
		textArea.Clear();
		UpdateState();
	}

	private void OnFieldChanged()
	{
		UpdateCharCount();
		UpdateState();
		OnChangedEvent?.Invoke();
	}

	protected override void OnTextFieldTransformChanged(Vector3 position, Vector2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.OnTextFieldTransformChanged(position, size);
		((Transform)((TMP_Text)textArea.outputText).rectTransform).localPosition = position;
		((TMP_Text)textArea.outputText).rectTransform.sizeDelta = size;
	}

	protected override void Submit()
	{
		base.Submit();
		UpdateState();
	}

	public void Focus()
	{
		textArea.GiveFocus();
		UpdateState();
	}

	public void Unfocus()
	{
		textArea.LoseFocus();
		UpdateState();
	}

	public void ForceKeyboardClose()
	{
		textArea.ForceKeyboardClose();
	}

	private void OnFieldPressed()
	{
		Focus();
	}

	private void OnFieldSubmit()
	{
		Submit();
	}

	public void UpdateCharCount()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = Background.size;
		size.y = Math.Max(0.62f, textArea.TextHeight + 0.2f);
		Background.size = size;
		Vector3 localPosition = ((Component)Background).transform.localPosition;
		localPosition.y = (0.62f - size.y) / 2f;
		((Component)Background).transform.localPosition = localPosition;
		Vector3 localPosition2 = ((Component)this).transform.localPosition;
		localPosition2.y = -2.08f - localPosition.y * 2f;
		((Component)this).transform.localPosition = localPosition2;
		int length = textArea.text.Length;
		((TMP_Text)charCountText).text = $"{length}/100";
		if (length < 75)
		{
			((Graphic)charCountText).color = Color.black;
		}
		else if (length < 100)
		{
			((Graphic)charCountText).color = new Color(1f, 1f, 0f, 1f);
		}
		else
		{
			((Graphic)charCountText).color = Color.red;
		}
	}

	private void UpdateState()
	{
		bool flag = false;
		if (Text.Length > 0)
		{
			flag = true;
		}
		SetCanSubmit(flag);
	}
}
