using UnityEngine;

public class HorizontalSpriteGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteRenderer Mask;

	private float lastValue = float.MinValue;

	public void Update()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (MaxValue != 0f && lastValue != Value)
		{
			lastValue = Value;
			float num = lastValue / MaxValue * maskScale;
			Vector3 localScale = ((Component)Mask).transform.localScale;
			localScale.x = num;
			((Component)Mask).transform.localScale = localScale;
			Vector3 localPosition = ((Component)Mask).transform.localPosition;
			Bounds bounds = Mask.sprite.bounds;
			localPosition.x = (0f - bounds.size.x) * (maskScale - num) / 2f;
			((Component)Mask).transform.localPosition = localPosition;
		}
	}
}
