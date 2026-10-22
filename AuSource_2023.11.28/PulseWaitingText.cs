using System.Collections;
using UnityEngine;

public class PulseWaitingText : MonoBehaviour
{
	public Transform textSource;

	private float duration = 1f;

	private void Awake()
	{
		((MonoBehaviour)this).StartCoroutine(GetBig());
	}

	private IEnumerator GetBig()
	{
		Vector3 vec = default;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			float num2 = Mathf.SmoothStep(1f, 1.25f, num);
			vec.Set(num2, num2, num2);
			((Component)this).transform.localScale = vec;
			yield return null;
		}
		vec.Set(0f, 0f, 0f);
		((Component)this).transform.localScale = vec;
		((MonoBehaviour)this).StartCoroutine(GetSmall());
	}

	private IEnumerator GetSmall()
	{
		Vector3 vec = default;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			float num2 = Mathf.SmoothStep(1.25f, 1f, num);
			vec.Set(num2, num2, num2);
			((Component)this).transform.localScale = vec;
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		((Component)this).transform.localScale = vec;
		((MonoBehaviour)this).StartCoroutine(GetBig());
	}
}
