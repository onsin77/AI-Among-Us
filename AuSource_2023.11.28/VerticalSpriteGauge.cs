using UnityEngine;

public class VerticalSpriteGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public FloatRange YRange;

	public SpriteRenderer Mask;

	private float lastValue = float.MinValue;

	public float TopY { get; private set; }

	public void Update()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (MaxValue != 0f && lastValue != Value)
		{
			lastValue = Value;
			Vector3 localPosition = ((Component)Mask).transform.localPosition;
			YRange.Lerp(lastValue / MaxValue);
			Vector3 localScale = ((Component)Mask).transform.localScale;
			localScale.y = lastValue / MaxValue * YRange.Width;
			((Component)Mask).transform.localScale = localScale;
			localPosition.y = YRange.min + localScale.y / 2f;
			((Component)Mask).transform.localPosition = localPosition;
			TopY = YRange.min + localScale.y;
		}
	}
}
