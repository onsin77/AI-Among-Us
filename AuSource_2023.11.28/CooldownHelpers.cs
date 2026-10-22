using UnityEngine;

public static class CooldownHelpers
{
	public static void SetCooldownNormalizedUvs(this SpriteRenderer myRend)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)myRend.sprite))
		{
			return;
		}
		Vector2[] uv = myRend.sprite.uv;
		Vector4 val = new Vector4(2f, -1f, 2f, -1f);
		for (int i = 0; i < uv.Length; i++)
		{
			if (val.x > uv[i].x)
			{
				val.x = uv[i].x;
			}
			if (val.y < uv[i].x)
			{
				val.y = uv[i].x;
			}
			if (val.z > uv[i].y)
			{
				val.z = uv[i].y;
			}
			if (val.w < uv[i].y)
			{
				val.w = uv[i].y;
			}
		}
		val.y -= val.x;
		val.w -= val.z;
		((Renderer)myRend).material.SetVector("_NormalizedUvs", val);
	}
}
