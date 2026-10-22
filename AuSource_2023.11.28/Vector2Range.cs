using System;
using UnityEngine;

[Serializable]
public struct Vector2Range
{
	public Vector2 min;

	public Vector2 max;

	public float Width => max.x - min.x;

	public float Height => max.y - min.y;

	public Vector2Range(Vector2 min, Vector2 max)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		this.min = min;
		this.max = max;
	}

	public void LerpUnclamped(ref Vector3 output, float t, float z)
	{
		output.Set(Mathf.LerpUnclamped(min.x, max.x, t), Mathf.LerpUnclamped(min.y, max.y, t), z);
	}

	public void Lerp(ref Vector3 output, float t, float z)
	{
		output.Set(Mathf.Lerp(min.x, max.x, t), Mathf.Lerp(min.y, max.y, t), z);
	}

	public Vector2 Next()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
	}

	public static Vector2 NextEdge()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.PI * 2f * Random.value;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector2(num2, num3);
	}
}
