using System.Collections;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
	private const float CurrentCapScale = 1f;

	public float Value = 0.5f;

	public float CapValue = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteRenderer Mask;

	[Header("Cap")]
	public SpriteRenderer cap;

	public SpriteRenderer capGlow;

	public float capScale = 1f;

	public float capGlowSizePadding = 0.2f;

	private float lastValue = float.MinValue;

	private float lastCapDiff = float.MinValue;

	public float GlowAlpha
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return cap.color.a;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			cap.color = cap.color.SetAlpha(value);
			capGlow.color = cap.color.SetAlpha(value);
		}
	}

	public IEnumerator ScaleDownY(float duration)
	{
		yield return Effects.All(Effects.Lerp(duration, (float t) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			Transform transform = ((Component)cap).transform;
			Vector3 localPosition = transform.localPosition;
			localPosition = new Vector3(localPosition.x, Mathf.Lerp(0f, (0f - cap.size.y) / 2f, Effects.ExpOut(t)), localPosition.z);
			transform.localPosition = localPosition;
			Vector3 localScale = transform.localScale;
			localScale = new Vector3(localScale.x, Mathf.Lerp(1f, 0f, Effects.ExpOut(t)), localScale.z);
			transform.localScale = localScale;
		}));
	}

	public void ResetScale()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Component)cap).transform.localScale = Vector3.one;
	}

	private void Update()
	{
		if (MaxValue != 0f)
		{
			float num = Value / MaxValue * maskScale;
			SetFillWidth(num);
			SetCapWidth(num);
		}
	}

	private void SetFillWidth(float xScale)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (lastValue != Value)
		{
			lastValue = Value;
			Transform transform = ((Component)Mask).transform;
			Vector3 localScale = transform.localScale;
			localScale.x = xScale;
			transform.localScale = localScale;
			Vector3 localPosition = transform.localPosition;
			Bounds bounds = Mask.sprite.bounds;
			localPosition.x = (0f - bounds.size.x) * (maskScale - xScale) / 2f;
			transform.localPosition = localPosition;
		}
	}

	private void SetCapWidth(float xScale)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		CapValue = Mathf.Max(CapValue, lastValue);
		if (!Mathf.Approximately(lastCapDiff, CapValue - Value) && Object.op_Implicit((Object)(object)cap))
		{
			lastCapDiff = CapValue - Value;
			Transform transform = ((Component)cap).transform;
			Vector2 val = new Vector2(lastCapDiff / MaxValue * capScale, cap.size.y);
			cap.size = val;
			capGlow.size = new Vector2(val.x + capGlowSizePadding, capGlow.size.y);
			if (val.x > 0f)
			{
				Bounds bounds = Mask.sprite.bounds;
				float num = xScale * bounds.size.x;
				float num2 = val.x * (capScale / val.x - 1f);
				transform.localPosition = new Vector3(num - num2 / 2f, 0f, 0f);
			}
		}
	}
}
