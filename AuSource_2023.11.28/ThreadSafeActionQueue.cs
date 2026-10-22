using System;
using System.Collections.Generic;

public class ThreadSafeActionQueue
{
	private readonly Queue<Action> pendingCallbacks = new Queue<Action>();

	public void Enqueue(Action action)
	{
		lock (pendingCallbacks)
		{
			pendingCallbacks.Enqueue(action);
		}
	}

	public void Drain()
	{
		lock (pendingCallbacks)
		{
			while (pendingCallbacks.Count > 0)
			{
				pendingCallbacks.Dequeue()();
			}
		}
	}
}
