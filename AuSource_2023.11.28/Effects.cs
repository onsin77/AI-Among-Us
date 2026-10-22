using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Effects
{
	private static HashSet<Transform> activeShakes = new HashSet<Transform>();

	public static IEnumerator Action(Action todo)
	{
		todo();
		yield break;
	}

	public static IEnumerator ActionAfterDelay(float seconds, Action todo)
	{
		yield return Wait(seconds);
		todo();
	}

	public static IEnumerator Wait(float duration)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			yield return null;
		}
	}

	public static IEnumerator Sequence(params IEnumerator[] items)
	{
		int i = 0;
		while (i < items.Length)
		{
			yield return items[i];
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerator All(params IEnumerator[] items)
	{
		Stack<IEnumerator>[] enums = new Stack<IEnumerator>[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			enums[i] = new Stack<IEnumerator>();
			enums[i].Push(items[i]);
		}
		int cap = 0;
		while (cap < 100000)
		{
			bool flag = false;
			for (int j = 0; j < enums.Length; j++)
			{
				if (enums[j].Count <= 0)
				{
					continue;
				}
				flag = true;
				IEnumerator enumerator = enums[j].Peek();
				if (enumerator.MoveNext())
				{
					if (enumerator.Current is IEnumerator)
					{
						enums[j].Push((IEnumerator)enumerator.Current);
					}
				}
				else
				{
					enums[j].Pop();
				}
			}
			if (flag)
			{
				yield return null;
				int num = cap + 1;
				cap = num;
				continue;
			}
			break;
		}
	}

	internal static IEnumerator Lerp(float duration, Action<float> action)
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			action(t / duration);
			yield return null;
		}
		action(1f);
	}

	internal static IEnumerator Overlerp(float duration, Action<float> action, float overextend = 0.05f)
	{
		float d1 = duration * 0.95f;
		for (float t = 0f; t < d1; t += Time.deltaTime)
		{
			action(Mathf.Lerp(0f, 1f + overextend, t / d1));
			yield return null;
		}
		float d2 = duration * 0.050000012f;
		for (float t = 0f; t < d2; t += Time.deltaTime)
		{
			action(Mathf.Lerp(1f + overextend, 1f, t / d2));
			yield return null;
		}
		action(1f);
	}

	internal static IEnumerator ScaleIn(Transform self, float source, float target, float duration)
	{
		if (Object.op_Implicit((Object)(object)self))
		{
			Vector3 localScale = default;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				localScale.x = (localScale.y = (localScale.z = Mathf.SmoothStep(source, target, t / duration)));
				self.localScale = localScale;
				yield return null;
			}
			localScale.x = (localScale.y = (localScale.z = target));
			self.localScale = localScale;
		}
	}

	internal static IEnumerator CycleColors(SpriteRenderer self, Color source, Color target, float rate, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)self))
		{
			((Renderer)self).enabled = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				float num = Mathf.Sin(t * (float)Math.PI / rate) / 2f + 0.5f;
				self.color = Color.Lerp(source, target, num);
				yield return null;
			}
			self.color = source;
		}
	}

	internal static IEnumerator PulseColor(SpriteRenderer self, Color source, Color target, float duration = 0.5f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)self))
		{
			((Renderer)self).enabled = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				self.color = Color.Lerp(target, source, t / duration);
				yield return null;
			}
			self.color = source;
		}
	}

	internal static IEnumerator PulseColor(TextMeshPro self, Color source, Color target, float duration = 0.5f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)self))
		{
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				((Graphic)self).color = Color.Lerp(target, source, t / duration);
				yield return null;
			}
			((Graphic)self).color = source;
		}
	}

	public static IEnumerator ColorFade(TextMeshPro self, Color source, Color target, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)self))
		{
			((Behaviour)self).enabled = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				((Graphic)self).color = Color.Lerp(source, target, t / duration);
				yield return null;
			}
			((Graphic)self).color = target;
		}
	}

	public static IEnumerator ColorFade(SpriteRenderer self, Color source, Color target, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)self))
		{
			((Renderer)self).enabled = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				self.color = Color.Lerp(source, target, t / duration);
				yield return null;
			}
			self.color = target;
		}
	}

	public static IEnumerator Rotate2D(Transform target, float source, float dest, float duration = 0.75f)
	{
		Vector3 temp = target.localEulerAngles;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.z = Mathf.SmoothStep(source, dest, num);
			target.localEulerAngles = temp;
			yield return null;
		}
		temp.z = dest;
		target.localEulerAngles = temp;
	}

	public static IEnumerator Slide3D(Transform target, Vector3 source, Vector3 dest, float duration = 0.75f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = default;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			localPosition.x = Mathf.SmoothStep(source.x, dest.x, num);
			localPosition.y = Mathf.SmoothStep(source.y, dest.y, num);
			localPosition.z = Mathf.Lerp(source.z, dest.z, num);
			target.localPosition = localPosition;
			yield return null;
		}
		target.localPosition = dest;
	}

	public static IEnumerator Slide2D(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 temp = new Vector3
		{
			z = target.localPosition.z
		};
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, num);
			temp.y = Mathf.SmoothStep(source.y, dest.y, num);
			target.localPosition = temp;
			yield return null;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.localPosition = temp;
	}

	public static IEnumerator Slide2DWorld(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 temp = new Vector3
		{
			z = target.position.z
		};
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float num = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, num);
			temp.y = Mathf.SmoothStep(source.y, dest.y, num);
			if ((Object)(object)target != (Object)null)
			{
				target.position = temp;
				yield return null;
				continue;
			}
			Debug.LogWarning((object)"target transform was destroyed while moving");
			yield break;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.position = temp;
	}

	public static IEnumerator Bounce(Transform target, float duration = 0.3f, float height = 0.15f)
	{
		if (!Object.op_Implicit((Object)(object)target))
		{
			yield break;
		}
		Vector3 origin = target.localPosition;
		Vector3 temp = origin;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = timer / duration;
			float num2 = 1f - num;
			temp.y = origin.y + height * Mathf.Abs(Mathf.Sin(num * (float)Math.PI * 3f)) * num2;
			if (!Object.op_Implicit((Object)(object)target))
			{
				yield break;
			}
			target.localPosition = temp;
			yield return null;
		}
		if (Object.op_Implicit((Object)(object)target))
		{
			((Component)target).transform.localPosition = origin;
		}
	}

	public static IEnumerator Shake(Transform target, float duration, float halfWidth, bool taper, bool returnToOrigin = false)
	{
		Vector3 origin = target.localPosition;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = timer / duration;
			Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle) * halfWidth;
			if (taper)
			{
				val *= 1f - num;
			}
			target.localPosition += val;
			yield return null;
		}
		if (returnToOrigin)
		{
			target.localPosition = origin;
		}
	}

	public static IEnumerator ShakeForever(Transform target, float halfWidth)
	{
		Vector3 origin = target.localPosition;
		while (true)
		{
			Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle) * halfWidth;
			target.localPosition = origin + val;
			yield return null;
		}
	}

	public static IEnumerator SwayX(Transform target, float duration = 0.75f, float halfWidth = 0.25f)
	{
		if (activeShakes.Add(target))
		{
			Vector3 origin = target.localPosition;
			for (float timer = 0f; timer < duration; timer += Time.deltaTime)
			{
				float num = timer / duration;
				target.localPosition = origin + Vector3.right * (halfWidth * Mathf.Sin(num * 30f) * (1f - num));
				yield return null;
			}
			((Component)target).transform.localPosition = origin;
			activeShakes.Remove(target);
		}
	}

	public static IEnumerator CurvePositionY(Transform target, AnimationCurve curve, float duration = 1f, float addXPos = 0f)
	{
		Vector3 initPos = ((Component)target).transform.position;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = timer / duration;
			Vector3 position = initPos;
			position.y += curve.Evaluate(num);
			position.x = Mathf.SmoothStep(initPos.x, initPos.x + addXPos, num);
			((Component)target).transform.position = position;
			yield return null;
		}
	}

	public static IEnumerator Bloop(float delay, Transform target, float finalSize = 1f, float duration = 0.5f)
	{
		for (float t = 0f; t < delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 localScale = default;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float z = ElasticOut(t, duration) * finalSize;
			localScale.x = (localScale.y = (localScale.z = z));
			target.localScale = localScale;
			yield return null;
		}
		localScale.x = (localScale.y = (localScale.z = finalSize));
		target.localScale = localScale;
	}

	public static IEnumerator ArcSlide(float duration, Transform target, Vector2 sourcePos, Vector2 targetPos, float anchorDistance)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (targetPos - sourcePos) / 2f;
		Vector2 val2 = sourcePos + val;
		Vector2 val3 = val.Rotate(90f);
		Vector2 anchor = val2 + val3.normalized * anchorDistance;
		float z = target.localPosition.z;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			Vector3 localPosition = Vector2.op_Implicit(Bezier(timer / duration, sourcePos, targetPos, anchor));
			localPosition.z = z;
			target.localPosition = localPosition;
			yield return null;
		}
		((Component)target).transform.localPosition = Vector2.op_Implicit(targetPos);
	}

	public static Vector3 Bezier(float t, Vector3 src, Vector3 dest, Vector3 anchor)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		t = Mathf.Clamp(t, 0f, 1f);
		float num = 1f - t;
		return num * num * src + 2f * num * t * anchor + t * t * dest;
	}

	public static Vector2 Bezier(float t, Vector2 src, Vector2 dest, Vector2 anchor)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		t = Mathf.Clamp(t, 0f, 1f);
		float num = 1f - t;
		return num * num * src + 2f * num * t * anchor + t * t * dest;
	}

	private static float ElasticOut(float time, float duration)
	{
		time /= duration;
		float num = time * time;
		float num2 = num * time;
		return 33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * time;
	}

	public static float ExpOut(float t)
	{
		return Mathf.Clamp(1f - Mathf.Pow(2f, -10f * t), 0f, 1f);
	}
}
