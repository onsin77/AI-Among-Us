using System.Collections;
using UnityEngine;

public class SpinAnimator : MonoBehaviour
{
	private enum States
	{
		Visible,
		Invisible,
		Spinning,
		Pulsing
	}

	public float Speed = 60f;

	public GameObject inputGlyph;

	private States curState;

	private void Update()
	{
		if (curState == States.Spinning)
		{
			((Component)this).transform.Rotate(0f, 0f, Speed * Time.deltaTime);
		}
	}

	public void Appear()
	{
		if (curState == States.Invisible)
		{
			curState = States.Visible;
			((Component)this).gameObject.SetActive(true);
			inputGlyph.SetActive(true);
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(Effects.ScaleIn(((Component)this).transform, 0f, 1f, 0.125f));
		}
	}

	public void Disappear()
	{
		if (curState != States.Invisible)
		{
			curState = States.Invisible;
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoDisappear());
		}
	}

	private IEnumerator CoDisappear()
	{
		yield return Effects.ScaleIn(((Component)this).transform, 1f, 0f, 0.125f);
		((Component)this).gameObject.SetActive(false);
		inputGlyph.SetActive(false);
	}

	public void StartPulse()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (curState != States.Pulsing)
		{
			curState = States.Pulsing;
			SpriteRenderer component = ((Component)this).GetComponent<SpriteRenderer>();
			((MonoBehaviour)this).StartCoroutine(Effects.CycleColors(component, Color.white, Color.green, 1f, float.MaxValue));
		}
	}

	internal void Play()
	{
		curState = States.Spinning;
		inputGlyph.SetActive(false);
	}
}
