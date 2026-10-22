using System;
using System.Collections;
using UnityEngine;

public class CoroutineHelper : DestroyableSingleton<CoroutineHelper>
{
	public void DelayBySeconds(float seconds, Action action)
	{
		((MonoBehaviour)this).StartCoroutine(CoDelayBySeconds(seconds, action));
	}

	public void DelayByFrame(Action action)
	{
		DelayByFrames(1, action);
	}

	public void DelayByFrames(int frames, Action action)
	{
		((MonoBehaviour)this).StartCoroutine(CoDelayByFrames(frames, action));
	}

	public void DelayUntilEndOfFrame(Action action)
	{
		((MonoBehaviour)this).StartCoroutine(CoDelayUntilEndOfFrame(action));
	}

	private IEnumerator CoDelayBySeconds(float seconds, Action action)
	{
		yield return (object)new WaitForSeconds(seconds);
		action();
	}

	private IEnumerator CoDelayByFrames(int frames, Action action)
	{
		for (int i = 0; i < frames; i++)
		{
			yield return null;
		}
		action();
	}

	private IEnumerator CoDelayUntilEndOfFrame(Action action)
	{
		yield return (object)new WaitForEndOfFrame();
		action();
	}
}
