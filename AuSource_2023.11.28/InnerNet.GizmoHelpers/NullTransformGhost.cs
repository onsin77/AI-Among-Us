using UnityEngine;

namespace InnerNet.GizmoHelpers;

public class NullTransformGhost : ITransformGhost
{
	public void AddPt(Vector2 pt)
	{
	}

	public void Render(Color c, bool crosses)
	{
	}
}
