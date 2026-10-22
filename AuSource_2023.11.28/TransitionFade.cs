using System;
using System.Collections;
using UnityEngine;

public class TransitionFade : DestroyableSingleton<TransitionFade>
{
	public SpriteRenderer overlay;

	public void DoTransitionFade(GameObject transitionFrom, GameObject transitionTo, Action callback)
	{
		if (transitionTo != null)
		{
			transitionTo.SetActive(false);
		}
		((MonoBehaviour)this).StartCoroutine(CoShow(transitionFrom, transitionTo, callback));
	}

	private IEnumerator CoShow(GameObject transitionFrom, GameObject transitionTo, Action callback)
	{
		yield return Effects.ColorFade(overlay, Color.clear, Color.black, 0.1f);
		if (Object.op_Implicit((Object)(object)transitionFrom) && Object.op_Implicit((Object)(object)transitionFrom.gameObject))
		{
			if (transitionFrom != null)
			{
				transitionFrom.gameObject.SetActive(false);
			}
		}
		if (Object.op_Implicit((Object)(object)transitionTo) && Object.op_Implicit((Object)(object)transitionTo.gameObject))
		{
			if (transitionTo != null)
			{
				transitionTo.gameObject.SetActive(true);
			}
		}
		yield return null;
		yield return Effects.ColorFade(overlay, Color.black, Color.clear, 0.1f);
		callback?.Invoke();
	}
}
