using System;
using System.Linq;
using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatPreviewField : AbstractChatInputField
{
	[SerializeField]
	[Space(10f)]
	private ChatInputFieldButton clearButton;

	[SerializeField]
	private ChatInputFieldButton undoButton;

	[SerializeField]
	private TextMeshPro warningText;

	[SerializeField]
	private TextMeshPro text;

	[SerializeField]
	private TextMeshPro placeholderText;

	public event Action OnClearPressedEvent;

	public event Action OnUndoPressedEvent;

	protected override void Awake()
	{
		base.Awake();
		clearButton.OnPressed += OnClearButtonPressed;
		clearButton.OnSizeChanged += OnClearButtonSizeChanged;
		undoButton.OnPressed += OnUndoButtonPressed;
		undoButton.OnSizeChanged += OnUndoButtonSizeChanged;
		((TMP_Text)warningText).text = GetQuickChatChatWarningStr();
	}

	public override void Clear()
	{
		base.Clear();
		UpdatePreview(null);
	}

	public void UpdatePreview(QuickChatPhraseBuilder builder)
	{
		((TMP_Text)text).text = "";
		((Component)text).gameObject.SetActive(false);
		((Component)placeholderText).gameObject.SetActive(true);
		SetCanSubmit(canSubmit: false);
		((Component)clearButton).gameObject.SetActive(false);
		((Component)undoButton).gameObject.SetActive(false);
		if (builder == null || builder.IsEmpty)
		{
			return;
		}
		QuickChatPhrase[] array = builder.ToPreviewData();
		if (array == null || array.Length < 1)
		{
			((Component)clearButton).gameObject.SetActive(false);
			return;
		}
		((Component)text).gameObject.SetActive(true);
		((Component)placeholderText).gameObject.SetActive(false);
		((Component)clearButton).gameObject.SetActive(true);
		((Component)clearButton).gameObject.SetActive(true);
		((Component)undoButton).gameObject.SetActive(true);
		QuickChatPhrase rootPhrase = array[0];
		((TMP_Text)text).text = rootPhrase.ToPreviewText(null);
		if (array[0].Type != QuickChatPhraseType.ComplexPhrase)
		{
			SetCanSubmit(canSubmit: true);
			return;
		}
		((TMP_Text)text).text = rootPhrase.ToPreviewText(array.Where((QuickChatPhrase x) => x != rootPhrase).ToArray());
		if (builder.IsComplete())
		{
			SetCanSubmit(canSubmit: true);
		}
	}

	public void AnimateInvalidSend()
	{
		Debug.LogWarning((object)"TODO: implement 'invalid send' animation for when trying to send a QC message before the builder is complete");
	}

	protected override void OnTextFieldTransformChanged(Vector3 position, Vector2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		base.OnTextFieldTransformChanged(position, size);
		((Transform)((TMP_Text)text).rectTransform).localPosition = position;
		((TMP_Text)text).rectTransform.sizeDelta = size;
		((Transform)((TMP_Text)placeholderText).rectTransform).localPosition = position;
		((TMP_Text)placeholderText).rectTransform.sizeDelta = size;
	}

	private void OnClearButtonPressed()
	{
		OnClearPressedEvent?.Invoke();
	}

	private void OnClearButtonSizeChanged()
	{
		UpdateClearButtonTransform();
	}

	private void UpdateClearButtonTransform()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)clearButton).transform.localPosition;
		localPosition.x = ((Component)undoButton).transform.localPosition.x - 0.1f - clearButton.Size.x * ((Component)clearButton).transform.localScale.x;
		((Component)clearButton).transform.localPosition = localPosition;
	}

	private void OnUndoButtonPressed()
	{
		OnUndoPressedEvent?.Invoke();
	}

	private void OnUndoButtonSizeChanged()
	{
		UpdateUndoButtonTransform();
	}

	private void UpdateUndoButtonTransform()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Vector3 localPosition = ((Component)undoButton).transform.localPosition;
		localPosition.x = bounds.max.x - undoButton.Size.x * ((Component)undoButton).transform.localScale.x;
		((Component)undoButton).transform.localPosition = localPosition;
		UpdateClearButtonTransform();
	}

	private string GetQuickChatChatWarningStr()
	{
		string text = "";
		if (DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsStart) + " ";
			if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn && DestroyableSingleton<EOSManager>.Instance.IsMinorOrWaiting() && !DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
			{
				text += DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsChild);
			}
		}
		return text;
	}
}
