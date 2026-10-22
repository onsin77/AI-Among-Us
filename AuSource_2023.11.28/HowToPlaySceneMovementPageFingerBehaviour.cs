using System.Collections;
using UnityEngine;

public class HowToPlaySceneMovementPageFingerBehaviour : MonoBehaviour
{
	public static class Quadratic
	{
		public static float InOut(float k)
		{
			if (k < 0f)
			{
				k = 0f;
			}
			if (k > 1f)
			{
				k = 1f;
			}
			if ((k *= 2f) < 1f)
			{
				return 0.5f * k * k;
			}
			return -0.5f * (--k * (k - 2f) - 1f);
		}
	}

	public SpriteRenderer Finger;

	public SpriteRenderer Click;

	public float liftedAngle = -20f;

	public IEnumerator DoClick(float duration)
	{
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			if (num < 0.4f)
			{
				float num2 = num / 0.4f;
				num2 = num2 * 2f - 1f;
				if (num2 < 0f)
				{
					float fingerAngle = Mathf.Lerp(liftedAngle, liftedAngle * 2f, 1f + Mathf.Abs(num2));
					SetFingerAngle(fingerAngle);
				}
				else
				{
					float fingerAngle2 = Mathf.Lerp(liftedAngle * 2f, 0f, num2);
					SetFingerAngle(fingerAngle2);
				}
			}
			else if (num < 0.7f)
			{
				ClickOn();
			}
			else
			{
				float num3 = (num - 0.7f) / 0.3f;
				((Renderer)Click).enabled = false;
				float fingerAngle3 = Mathf.Lerp(0f, liftedAngle, num3);
				SetFingerAngle(fingerAngle3);
			}
			yield return null;
		}
		ClickOff();
	}

	private void SetFingerAngle(float angle)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Component)Finger).transform.localRotation = Quaternion.Euler(0f, 0f, angle);
	}

	public void ClickOff()
	{
		((Renderer)Click).enabled = false;
		SetFingerAngle(liftedAngle);
	}

	public void ClickOn()
	{
		((Renderer)Click).enabled = true;
		SetFingerAngle(0f);
	}

	public IEnumerator MoveTo(Vector2 target, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 startPos = ((Component)this).transform.position;
		Vector3 targetPos = Vector2.op_Implicit(target);
		targetPos.z = startPos.z;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			((Component)this).transform.position = Vector3.Lerp(startPos, targetPos, num);
			yield return null;
		}
		((Component)this).transform.position = targetPos;
	}
}
