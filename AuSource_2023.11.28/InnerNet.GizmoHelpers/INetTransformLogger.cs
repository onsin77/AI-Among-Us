using UnityEngine;

namespace InnerNet.GizmoHelpers;

public interface INetTransformLogger
{
	void SetQueueLength(int i);

	void Render(Vector3 pos2, float mod);

	void SetSlowbandingLog(bool isSlowbanding);
}
