using System.Collections;
using TMPro;
using UnityEngine;

public class DiscussBehaviour : MonoBehaviour
{
	public SpriteRenderer LeftPlayer;

	public SpriteRenderer RightPlayer;

	public TextMeshPro Text;

	public FloatRange RotateRange = new FloatRange(-5f, 5f);

	public Vector2Range TextTarget;

	public AnimationCurve TextEasing;

	public float Delay = 0.1f;

	public float TextDuration = 0.5f;

	public float HoldDuration = 2f;

	private Vector3 vec;

	public IEnumerator PlayAnimation()
	{
		Text.transform.localPosition = Vector2.op_Implicit(TextTarget.min);
		yield return AnimateText();
		yield return ShhhBehaviour.WaitWithInterrupt(HoldDuration);
	}

	public void Update()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		vec.Set(0f, 0f, RotateRange.Lerp(Mathf.PerlinNoise(1f, Time.time * 8f)));
		((Component)LeftPlayer).transform.eulerAngles = vec;
		vec.Set(0f, 0f, RotateRange.Lerp(Mathf.PerlinNoise(2f, Time.time * 8f)));
		((Component)RightPlayer).transform.eulerAngles = vec;
	}

	private IEnumerator AnimateText()
	{
		for (float t = 0f; t < Delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 vec = default;
		for (float t = 0f; t < TextDuration; t += Time.deltaTime)
		{
			float num = t / TextDuration;
			UpdateText(ref vec, TextEasing.Evaluate(num));
			yield return null;
		}
		UpdateText(ref vec, 1f);
	}

	private void UpdateText(ref Vector3 vec, float p)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		TextTarget.LerpUnclamped(ref vec, p, -7f);
		Text.transform.localPosition = vec;
	}
}
