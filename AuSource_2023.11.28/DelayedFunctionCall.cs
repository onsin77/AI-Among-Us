using UnityEngine;
using UnityEngine.Events;

public class DelayedFunctionCall : MonoBehaviour
{
	public UnityEvent onTimerElapsed;

	public float delayDuration = 1f;

	private float t;

	private void Update()
	{
		if (t >= delayDuration)
		{
			((Behaviour)this).enabled = false;
			if (onTimerElapsed != null)
			{
				onTimerElapsed.Invoke();
			}
		}
		t += Time.deltaTime;
	}
}
