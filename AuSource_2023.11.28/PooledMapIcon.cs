using System;
using UnityEngine;

public class PooledMapIcon : PoolableBehavior
{
	public float NormalSize = 0.3f;

	public int lastMapTaskStep = -1;

	public SpriteRenderer rend;

	public AlphaPulse alphaPulse;

	public void Update()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)alphaPulse).enabled)
		{
			float num = Mathf.Abs(Mathf.Cos((alphaPulse.Offset + Time.time) * (float)Math.PI / alphaPulse.Duration));
			if ((double)num > 0.9)
			{
				num -= 0.9f;
				num = NormalSize + num;
				((Component)this).transform.localScale = new Vector3(num, num, num);
			}
		}
	}

	public override void Reset()
	{
		lastMapTaskStep = -1;
		((Behaviour)alphaPulse).enabled = false;
		((Renderer)rend).material.SetFloat("_Outline", 0f);
		base.Reset();
	}
}
