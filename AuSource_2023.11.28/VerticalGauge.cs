using UnityEngine;

public class VerticalGauge : MonoBehaviour
{
	public float value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteMask Mask;

	private float lastValue = float.MinValue;

	public void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (lastValue != value)
		{
			lastValue = value;
			float num = Mathf.Clamp(lastValue / MaxValue, 0f, 1f) * maskScale;
			Vector3 localScale = ((Component)Mask).transform.localScale;
			localScale.y = num;
			((Component)Mask).transform.localScale = localScale;
			Transform transform = ((Component)Mask).transform;
			Bounds bounds = Mask.sprite.bounds;
			transform.localPosition = new Vector3(0f, (0f - bounds.size.y) * (maskScale - num) / 2f, 0f);
		}
	}
}
