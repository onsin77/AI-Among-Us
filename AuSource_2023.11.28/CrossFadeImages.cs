using System;
using UnityEngine;

public class CrossFadeImages : MonoBehaviour
{
	public SpriteRenderer Image1;

	public SpriteRenderer Image2;

	public float Period = 5f;

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		white.a = Mathf.Clamp((Mathf.Sin((float)Math.PI * Time.time / Period) + 0.75f) * 0.75f, 0f, 1f);
		Image1.color = white;
		white.a = 1f - white.a;
		Image2.color = white;
	}
}
