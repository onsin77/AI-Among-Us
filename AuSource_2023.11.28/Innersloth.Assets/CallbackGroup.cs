using System;
using System.Collections.Generic;

namespace Innersloth.Assets;

public class CallbackGroup
{
	private List<Action> callbacks = new List<Action>();

	public void AddCallback(Action callback)
	{
		if (callback != null)
		{
			callbacks.Add(callback);
		}
	}

	public void Call()
	{
		foreach (Action callback in callbacks)
		{
			callback();
		}
	}

	public void Clear()
	{
		callbacks?.Clear();
	}

	public void CallAndClear()
	{
		Call();
		Clear();
	}

	public void Destroy()
	{
		callbacks = null;
	}
}
