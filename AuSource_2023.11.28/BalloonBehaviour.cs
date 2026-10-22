using UnityEngine;

public class BalloonBehaviour : MonoBehaviour
{
	public Vector2 Origin;

	public float PeriodX = 4f;

	public float PeriodY = 4f;

	public float MagnitudeX = 3f;

	public float MagnitudeY = 3f;

	public void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = Vector2.op_Implicit(Origin + new Vector2(Mathf.PerlinNoise(Time.time * PeriodX, 1f) * MagnitudeX, Mathf.Sin(Time.time * PeriodY) * MagnitudeY));
	}
}
