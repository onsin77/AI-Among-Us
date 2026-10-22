using System;
using UnityEngine;

public class ChainBehaviour : MonoBehaviour
{
	public FloatRange SwingRange = new FloatRange(0f, 30f);

	public float SwingPeriod = 2f;

	public float swingTime;

	private Vector3 vec;

	public void Awake()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		swingTime = FloatRange.Next(0f, SwingPeriod);
		vec.z = SwingRange.Lerp(Mathf.Sin(swingTime));
		((Component)this).transform.eulerAngles = vec;
	}

	public void Update()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		swingTime += Time.deltaTime / SwingPeriod;
		vec.z = SwingRange.Lerp(Mathf.Sin(swingTime * (float)Math.PI) / 2f + 0.5f);
		((Component)this).transform.eulerAngles = vec;
	}
}
