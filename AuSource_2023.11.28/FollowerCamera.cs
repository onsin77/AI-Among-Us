using System;
using System.Collections;
using AmongUs.Data;
using UnityEngine;

public class FollowerCamera : MonoBehaviour
{
	public MonoBehaviour Target;

	public Vector2 Offset;

	public bool Locked;

	public float shakeAmount;

	public float shakePeriod = 1f;

	[NonSerialized]
	public bool OverrideScreenShakeEnabled = true;

	private Vector2 centerPosition;

	public void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Target) && !Locked)
		{
			centerPosition = Vector2.op_Implicit(Vector3.Lerp(Vector2.op_Implicit(centerPosition), ((Component)Target).transform.position + Vector2.op_Implicit(Offset), 5f * Time.deltaTime));
			Vector2 val = centerPosition;
			if (shakeAmount > 0f && DataManager.Settings.Gameplay.ScreenShake && OverrideScreenShakeEnabled)
			{
				float num = Time.fixedTime * shakePeriod;
				float num2 = Mathf.PerlinNoise(0.5f, num) * 2f - 1f;
				float num3 = Mathf.PerlinNoise(num, 0.5f) * 2f - 1f;
				val.x += num2 * shakeAmount;
				val.y += num3 * shakeAmount;
			}
			((Component)this).transform.position = Vector2.op_Implicit(val);
		}
	}

	public void ShakeScreen(float duration, float severity)
	{
		if (DataManager.Settings.Gameplay.ScreenShake)
		{
			((MonoBehaviour)this).StartCoroutine(CoShakeScreen(duration, severity));
		}
	}

	private IEnumerator CoShakeScreen(float duration, float severity)
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		for (float t = duration; t > 0f; t -= Time.fixedDeltaTime)
		{
			float num = t / duration;
			Offset = Random.insideUnitCircle * num * severity;
			yield return wait;
		}
		Offset = Vector2.zero;
	}

	internal void SetTarget(MonoBehaviour target)
	{
		Target = target;
		SnapToTarget();
	}

	public void SnapToTarget()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ((Component)Target).transform.position + Vector2.op_Implicit(Offset);
	}
}
