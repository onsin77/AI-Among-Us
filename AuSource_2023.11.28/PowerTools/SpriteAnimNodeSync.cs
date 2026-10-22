using UnityEngine;

namespace PowerTools;

[ExecuteInEditMode]
public class SpriteAnimNodeSync : MonoBehaviour
{
	public int NodeId;

	public SpriteAnimNodes Parent;

	public SpriteRenderer ParentRenderer;

	public SpriteRenderer Renderer;

	[HideInInspector]
	public Vector3 flipOffset = Vector3.zero;

	public void LateUpdate()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (Object.op_Implicit((Object)(object)ParentRenderer))
		{
			flag = ParentRenderer.flipX;
			if (Object.op_Implicit((Object)(object)Renderer))
			{
				Renderer.flipX = ParentRenderer.flipX;
			}
		}
		else if (Object.op_Implicit((Object)(object)Renderer))
		{
			flag = Renderer.flipX;
		}
		Vector3 localPosition = ((Component)this).transform.localPosition;
		Vector3 localPosition2 = Parent.GetLocalPosition(NodeId);
		localPosition.x = localPosition2.x;
		localPosition.y = localPosition2.y;
		((Component)this).transform.localPosition = localPosition;
		float angle = Parent.GetAngle(NodeId);
		if (flag)
		{
			((Component)this).transform.eulerAngles = new Vector3(0f, 0f, 0f - angle);
			Transform transform = ((Component)this).transform;
			transform.localPosition += flipOffset;
		}
		else
		{
			((Component)this).transform.eulerAngles = new Vector3(0f, 0f, angle);
		}
	}

	public SpriteAnimNodeSync()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
