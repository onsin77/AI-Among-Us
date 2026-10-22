using System.Collections.Generic;
using UnityEngine;

namespace InnerNet.GizmoHelpers;

public class NetTransformLogger : INetTransformLogger
{
	private List<int> queue;

	private List<bool> slowbandLog;

	private Vector3 pos;

	public NetTransformLogger()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		queue = new List<int>();
		slowbandLog = new List<bool>();
		float num = (float)Screen.width * 0.1f;
		float num2 = (float)Screen.height * 1.2f;
		pos = Camera.main.ScreenToWorldPoint(new Vector3(num, num2, num2));
	}

	public void SetQueueLength(int i)
	{
		if (queue.Count > 30)
		{
			queue.RemoveAt(0);
		}
		queue.Add(i);
	}

	public void SetSlowbandingLog(bool isSlowbanding)
	{
		if (slowbandLog.Count > 30)
		{
			slowbandLog.RemoveAt(0);
		}
		slowbandLog.Add(isSlowbanding);
	}

	public void Render(Vector3 pos2, float rubberBandMod)
	{
	}
}
