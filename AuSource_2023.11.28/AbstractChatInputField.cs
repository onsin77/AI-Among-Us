using System;
using UnityEngine;

public abstract class AbstractChatInputField : MonoBehaviour
{
	protected const float BUTTON_MARGIN = 0.1f;

	private const float TEXT_FIELD_MARGIN = 0.12f;

	[SerializeField]
	private SpriteRenderer background;

	[SerializeField]
	private ChatInputFieldButton submitButton;

	private bool visible = true;

	private bool canSubmit = true;

	public SpriteRenderer Background => background;

	public bool Visible => visible;

	public bool CanSubmit => canSubmit;

	public event Action OnSubmitEvent;

	protected virtual void Awake()
	{
		submitButton.OnPressed += OnSubmitButtonPressed;
		submitButton.OnSizeChanged += OnSubmitButtonSizeChanged;
	}

	protected virtual void Start()
	{
		UpdateElements();
		Clear();
	}

	private void OnSubmitButtonPressed()
	{
		Submit();
	}

	private void OnSubmitButtonSizeChanged()
	{
		UpdateElements();
	}

	private void UpdateElements()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Vector3 localPosition = ((Component)submitButton).transform.localPosition;
		localPosition.x = bounds.max.x - submitButton.Size.x - 0.1f;
		((Component)submitButton).transform.localPosition = localPosition;
		Vector3 position = new Vector3(0f, 0f, -0.1f);
		position.x = bounds.min.x + 0.12f;
		Vector2 size = Vector2.op_Implicit(bounds.size);
		size.x = bounds.size.x - submitButton.Size.x - 0.1f - 0.24f;
		size.y = 0.5f;
		OnTextFieldTransformChanged(position, size);
	}

	protected virtual void OnTextFieldTransformChanged(Vector3 position, Vector2 size)
	{
	}

	protected Bounds GetBounds()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(Vector3.zero, Vector2.op_Implicit(background.size));
	}

	public virtual void SetVisible(bool visible)
	{
		this.visible = visible;
		((Component)this).gameObject.SetActive(visible);
	}

	public void SetCanSubmit(bool canSubmit)
	{
		this.canSubmit = canSubmit;
		((Behaviour)submitButton).enabled = canSubmit;
		SetButtonsEnabled(canSubmit);
	}

	protected virtual void SetButtonsEnabled(bool enabled)
	{
		submitButton.SetButtonEnabled(enabled);
	}

	public virtual void Clear()
	{
	}

	protected virtual void Submit()
	{
		OnSubmitEvent?.Invoke();
	}
}
