using UnityEngine;

public class FullScreenScaler : MonoBehaviour
{
	public SpriteRenderer OptionalTarget;

	public bool ScaleWidth = true;

	public bool ScaleHeight = true;

	public void OnEnable()
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		float num = main.orthographicSize * 2f;
		float num2 = num * main.aspect;
		if (Object.op_Implicit((Object)(object)OptionalTarget))
		{
			if ((int)OptionalTarget.drawMode != 0)
			{
				Vector3 val = Vector2.op_Implicit(OptionalTarget.size);
				if (ScaleWidth)
				{
					val.x = num2;
				}
				if (ScaleHeight)
				{
					val.y = num;
				}
				OptionalTarget.size = Vector2.op_Implicit(val);
				return;
			}
			Vector3 localScale = ((Component)this).transform.localScale;
			if (ScaleWidth)
			{
				localScale.x = num2;
			}
			if (ScaleHeight)
			{
				localScale.y = num;
			}
			if (Object.op_Implicit((Object)(object)OptionalTarget.sprite))
			{
				Bounds bounds = OptionalTarget.sprite.bounds;
				Vector3 size = bounds.size;
				if (ScaleWidth)
				{
					localScale.x = num2 / size.x;
				}
				if (ScaleHeight)
				{
					localScale.y = num / size.y;
				}
			}
			((Component)this).transform.localScale = localScale;
		}
		else
		{
			Vector3 localScale2 = ((Component)this).transform.localScale;
			if (ScaleWidth)
			{
				localScale2.x = num2;
			}
			if (ScaleHeight)
			{
				localScale2.y = num;
			}
			((Component)this).transform.localScale = localScale2;
		}
	}
}
