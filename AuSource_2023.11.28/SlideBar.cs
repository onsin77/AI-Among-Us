using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlideBar : PassiveUiElement
{
	[Space(20f)]
	public TextMeshPro Title;

	public SpriteRenderer Bar;

	public SpriteRenderer Dot;

	public FloatRange Range;

	public bool Vertical;

	public float Value;

	public UnityEvent OnValueChange;

	private float sliderSegmentIncrement = 0.1f;

	public override bool HandleDrag => true;

	protected override void OnEnable()
	{
		base.OnEnable();
		SetEnabledColors();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SetDisabledColors();
	}

	public override void ReceiveClickDrag(Vector2 dragDelta)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Dot).transform.localPosition;
		Vector2 val = DestroyableSingleton<PassiveButtonManager>.Instance.controller.DragPosition - Vector2.op_Implicit(((Component)Bar).transform.position);
		if (Vertical)
		{
			localPosition.y = Range.Clamp(val.y);
			Value = Range.ReverseLerp(localPosition.y);
		}
		else
		{
			localPosition.x = Range.Clamp(val.x);
			Value = Range.ReverseLerp(localPosition.x);
		}
		UpdateValue();
		OnValueChange.Invoke();
	}

	public void UpdateValue()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Dot).transform.localPosition;
		if (Vertical)
		{
			localPosition.y = Range.Lerp(Value);
		}
		else
		{
			localPosition.x = Range.Lerp(Value);
		}
		((Component)Dot).transform.localPosition = localPosition;
	}

	public void SetValue(float newValue)
	{
		Value = Mathf.Clamp01(newValue);
		UpdateValue();
	}

	public void ControllerIncrease()
	{
		float num = Value + sliderSegmentIncrement;
		Value = Mathf.Clamp(num, 0f, 1f);
		Debug.Log((object)("ControllerIncrease: " + Value));
		UpdateValue();
		OnValueChange.Invoke();
	}

	public void ControllerDecrease()
	{
		float num = Value - sliderSegmentIncrement;
		Value = Mathf.Clamp(num, 0f, 1f);
		Debug.Log((object)("ControllerDecrease: " + Value));
		UpdateValue();
		OnValueChange.Invoke();
	}

	private void OnValidate()
	{
		UpdateValue();
	}

	public void SetEnabledColors()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Title))
		{
			((Graphic)Title).color = Color.white;
		}
		if (Object.op_Implicit((Object)(object)Bar))
		{
			Bar.color = Color.white;
		}
		Dot.color = Color.white;
		UpdateValue();
	}

	public void SetDisabledColors()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Title))
		{
			((Graphic)Title).color = Color.gray;
		}
		if (Object.op_Implicit((Object)(object)Bar))
		{
			Bar.color = Color.gray;
		}
		Dot.color = Color.gray;
	}
}
