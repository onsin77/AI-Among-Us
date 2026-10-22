using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TransitionOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public ButtonClickedEvent OnClose = new ButtonClickedEvent();

	public void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(AnimateOpen());
	}

	public void Toggle()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		if (((Behaviour)this).isActiveAndEnabled)
		{
			Close();
			return;
		}
		((Component)this).gameObject.SetActive(true);
		OnEnable();
	}

	public void Close()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			((MonoBehaviour)this).StartCoroutine(AnimateClose());
		}
	}

	private IEnumerator AnimateClose()
	{
		Vector3 vec = default;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			float num2 = Mathf.SmoothStep(1f, 0f, num);
			vec.Set(num2, num2, num2);
			((Component)this).transform.localScale = vec;
			yield return null;
		}
		vec.Set(0f, 0f, 0f);
		((Component)this).transform.localScale = vec;
		((UnityEvent)OnClose).Invoke();
	}

	private IEnumerator AnimateOpen()
	{
		Vector3 vec = default;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			vec.Set(num2, num2, num2);
			((Component)this).transform.localScale = vec;
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		((Component)this).transform.localScale = vec;
	}

	public TransitionOpen()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}
