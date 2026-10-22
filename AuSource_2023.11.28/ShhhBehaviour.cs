using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShhhBehaviour : MonoBehaviour
{
	public SpriteRenderer Background;

	public SpriteRenderer Body;

	public SpriteRenderer Hand;

	public TextMeshPro TextImage;

	public float RotateSpeed = 15f;

	public Vector2Range HandTarget;

	public AnimationCurve PositionEasing;

	public FloatRange HandRotate;

	public AnimationCurve RotationEasing;

	public Vector2Range TextTarget;

	public AnimationCurve TextEasing;

	public float Duration = 0.5f;

	public float Delay = 0.1f;

	public float TextDuration = 0.5f;

	public float PulseDuration = 0.1f;

	public float PulseSize = 0.1f;

	public float HoldDuration = 2f;

	public bool Autoplay;

	public void OnEnable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Autoplay)
		{
			Vector3 vec = default;
			UpdateHand(ref vec, 1f);
			UpdateText(ref vec, 1f);
			vec.Set(1f, 1f, 1f);
			((Component)Body).transform.localScale = vec;
			((Graphic)TextImage).color = Color.white;
		}
	}

	public IEnumerator PlayAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(AnimateHand());
		yield return AnimateText();
		yield return WaitWithInterrupt(HoldDuration);
	}

	public void Update()
	{
		((Component)Background).transform.Rotate(0f, 0f, Time.deltaTime * RotateSpeed);
	}

	private IEnumerator AnimateText()
	{
		((Graphic)TextImage).color = Palette.ClearWhite;
		for (float t = 0f; t < Delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 vec = default;
		for (float t = 0f; t < PulseDuration; t += Time.deltaTime)
		{
			float num = t / PulseDuration;
			float num2 = 1f + Mathf.Sin((float)Math.PI * num) * PulseSize;
			vec.Set(num2, num2, 1f);
			((Component)Body).transform.localScale = vec;
			((Graphic)TextImage).color = Color.Lerp(Palette.ClearWhite, Palette.White, num * 2f);
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		((Component)Body).transform.localScale = vec;
		((Graphic)TextImage).color = Color.white;
	}

	private IEnumerator AnimateHand()
	{
		((Component)Hand).transform.localPosition = Vector2.op_Implicit(HandTarget.min);
		Vector3 vec = default;
		for (float t = 0f; t < Duration; t += Time.deltaTime)
		{
			float p = t / Duration;
			UpdateHand(ref vec, p);
			yield return null;
		}
		UpdateHand(ref vec, 1f);
	}

	private void UpdateHand(ref Vector3 vec, float p)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		HandTarget.LerpUnclamped(ref vec, PositionEasing.Evaluate(p), -1f);
		((Component)Hand).transform.localPosition = vec;
		vec.Set(0f, 0f, HandRotate.LerpUnclamped(RotationEasing.Evaluate(p)));
		((Component)Hand).transform.eulerAngles = vec;
	}

	private void UpdateText(ref Vector3 vec, float p)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		TextTarget.LerpUnclamped(ref vec, p, -2f);
		TextImage.transform.localPosition = vec;
	}

	public static IEnumerator WaitWithInterrupt(float duration)
	{
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			if (CheckForInterrupt())
			{
				break;
			}
			yield return null;
		}
	}

	public static bool CheckForInterrupt()
	{
		return Input.anyKeyDown;
	}
}
