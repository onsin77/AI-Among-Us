using System.Collections;
using UnityEngine;

public class CrystalBehaviour : UiElement
{
	public Transform TargetPosition;

	public SpriteRenderer Renderer;

	public BoxCollider2D Collider;

	public bool CanMove = true;

	public FloatRange Padding;

	private const float Speed = 15f;

	public float XFloatMag = 0.01f;

	private const float FloatMag = 0.05f;

	private const float FloatSpeed = 0.35f;

	public float PieceIndex;

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)TargetPosition))
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			if ((double)Vector2.Distance(Vector2.op_Implicit(TargetPosition.localPosition), Vector2.op_Implicit(((Component)this).transform.localPosition)) > 0.01)
			{
				localPosition = Vector3.Lerp(TargetPosition.localPosition, ((Component)this).transform.localPosition, Time.deltaTime);
			}
			float num = Time.time * 0.35f;
			localPosition.x += (Mathf.PerlinNoise(num, PieceIndex * 100f) * 2f - 1f) * XFloatMag;
			localPosition.y += (Mathf.PerlinNoise(PieceIndex * 100f, num) * 2f - 1f) * 0.05f;
			((Component)this).transform.localPosition = localPosition;
		}
	}

	public void Flash(float delay = 0f)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(Flash(this, delay));
	}

	private static IEnumerator Flash(CrystalBehaviour c, float delay)
	{
		for (float time = 0f; time < delay; time += Time.deltaTime)
		{
			yield return null;
		}
		Color col = Color.clear;
		for (float time = 0f; time < 0.1f; time += Time.deltaTime)
		{
			float num = time / 0.1f;
			col.r = (col.g = (col.b = Mathf.Lerp(0f, 1f, num)));
			((Renderer)c.Renderer).material.SetColor("_AddColor", col);
			yield return null;
		}
		for (float time = 0f; time < 0.1f; time += Time.deltaTime)
		{
			float num2 = time / 0.1f;
			col.r = (col.g = (col.b = Mathf.Lerp(1f, 0f, num2)));
			((Renderer)c.Renderer).material.SetColor("_AddColor", col);
			yield return null;
		}
		col.r = (col.g = (col.b = 0f));
		((Renderer)c.Renderer).material.SetColor("_AddColor", col);
	}
}
