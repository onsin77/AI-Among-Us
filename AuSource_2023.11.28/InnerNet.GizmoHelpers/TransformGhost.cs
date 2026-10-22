using System.Collections.Generic;
using UnityEngine;

namespace InnerNet.GizmoHelpers;

public class TransformGhost : ITransformGhost
{
	private const int MaxCount = 30;

	private const float HalfSize = 0.05f;

	private List<Vector2> positions = new List<Vector2>();

	public void AddPt(Vector2 pt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		positions.Add(pt);
		if (positions.Count >= 30)
		{
			positions.RemoveAt(0);
		}
	}

	public void Render(Color c, bool crosses)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = c;
		for (int i = 0; i < positions.Count; i++)
		{
			Vector3 val = Vector2.op_Implicit(positions[i]);
			if (crosses)
			{
				val.z = -101f;
				Gizmos.DrawLine(val + Vector3.left * 0.05f, val + Vector3.right * 0.05f);
				Gizmos.DrawLine(val + Vector3.up * 0.05f, val + Vector3.down * 0.05f);
			}
			else
			{
				val.z = -100f;
				Gizmos.DrawCube(val, Vector3.one * 0.05f * 2f);
			}
		}
		Gizmos.color = Color.white;
	}
}
