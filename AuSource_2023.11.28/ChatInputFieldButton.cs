using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatInputFieldButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro text;

	[SerializeField]
	private TextTranslatorTMP translator;

	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private Transform inputGlyph;

	[SerializeField]
	private SpriteRenderer[] backgroundSprites;

	[SerializeField]
	private SpriteRenderer[] iconSprites;

	public Vector2 Size
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return collider.size;
		}
	}

	public event Action OnPressed;

	public event Action OnSizeChanged;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		((UnityEvent)button.OnClick).AddListener((UnityAction)OnButtonClicked);
		translator.OnTranslate.AddListener((UnityAction)OnTextTranslationChanged);
		UpdateSize();
	}

	public void SetButtonEnabled(bool enabled)
	{
		button.SetButtonEnableState(enabled);
	}

	private void OnButtonClicked()
	{
		OnPressed?.Invoke();
	}

	private void OnTextTranslationChanged()
	{
		UpdateSize();
	}

	private void UpdateSize()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)text).autoSizeTextContainer = true;
		((Graphic)text).Rebuild((CanvasUpdate)0);
		float x = ((TMP_Text)text).rectTransform.sizeDelta.x;
		Vector2 size = Size;
		Vector2 val = new Vector2(size.x * 0.5f, (0f - size.y) * 0.5f);
		float[] array = new float[backgroundSprites.Length];
		for (int i = 0; i < backgroundSprites.Length; i++)
		{
			SpriteRenderer val2 = backgroundSprites[i];
			array[i] = Mathf.Abs(val2.size.x * 0.5f - val.x);
		}
		size.x = x;
		val.x = size.x * 0.5f;
		if (backgroundSprites != null && backgroundSprites.Length != 0)
		{
			for (int j = 0; j < backgroundSprites.Length; j++)
			{
				SpriteRenderer obj = backgroundSprites[j];
				Vector3 localPosition = ((Component)obj).transform.localPosition;
				float num = array[j];
				localPosition.x = val.x;
				((Component)obj).transform.localPosition = localPosition;
				Vector2 size2 = obj.size;
				size2.x = size.x + num * 2f;
				obj.size = size2;
			}
		}
		collider.size = size;
		((Collider2D)collider).offset = new Vector2(val.x, 0f);
		Vector3 localPosition2 = inputGlyph.localPosition;
		localPosition2.x = Size.x;
		inputGlyph.localPosition = localPosition2;
		OnSizeChanged?.Invoke();
	}
}
