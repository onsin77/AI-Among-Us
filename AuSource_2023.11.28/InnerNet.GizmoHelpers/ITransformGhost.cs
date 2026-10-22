using UnityEngine;

namespace InnerNet.GizmoHelpers;

public interface ITransformGhost
{
	void AddPt(Vector2 pt);

	void Render(Color c, bool crosses);
}
