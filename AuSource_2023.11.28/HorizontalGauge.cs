using UnityEngine;

public class HorizontalGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteMask Mask;

	public SpriteRenderer MaskArea;

	private float lastValue = float.MinValue;

	public void Update()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (MaxValue != 0f && lastValue != Value)
		{
			lastValue = Value;
			float num = lastValue / MaxValue * maskScale;
			if (Object.op_Implicit((Object)(object)Mask))
			{
				((Component)Mask).transform.localScale = new Vector3(num, ((Component)Mask).transform.localScale.y, 1f);
				Transform transform = ((Component)Mask).transform;
				Bounds bounds = Mask.sprite.bounds;
				transform.localPosition = new Vector3((0f - bounds.size.x) * (maskScale - num) / 2f, 0f, 0f);
			}
			if (Object.op_Implicit((Object)(object)MaskArea))
			{
				((Component)MaskArea).transform.localScale = new Vector3(num, ((Component)MaskArea).transform.localScale.y, 1f);
				((Component)MaskArea).transform.localPosition = new Vector3((0f - MaskArea.size.x) * (maskScale - num) / 2f, 0f, 0f);
			}
		}
	}
}
