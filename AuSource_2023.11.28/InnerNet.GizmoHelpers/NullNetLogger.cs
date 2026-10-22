using UnityEngine;

namespace InnerNet.GizmoHelpers;

public class NullNetLogger : INetTransformLogger
{
	public void SetQueueLength(int i)
	{
	}

	public void Render(Vector3 pos2, float mod)
	{
	}

	public void SetSlowbandingLog(bool isSlowbanding)
	{
	}
}
