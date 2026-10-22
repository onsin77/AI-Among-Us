using System;
using UnityEngine;

public class TumbleBoxBehaviour : MonoBehaviour
{
	public FloatRange BoxHeight;

	public FloatRange shadowScale;

	public SpriteRenderer Shadow;

	public SpriteRenderer Box;

	public void FixedUpdate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time * 15f;
		float v = Mathf.Cos(Time.time * (float)Math.PI / 10f) / 2f + 0.5f;
		float num2 = shadowScale.Lerp(v);
		((Component)Shadow).transform.localScale = new Vector3(num2, num2, num2);
		float num3 = BoxHeight.Lerp(v);
		((Component)Box).transform.localPosition = new Vector3(0f, num3, -0.01f);
		((Component)Box).transform.eulerAngles = new Vector3(0f, 0f, num);
	}
}
