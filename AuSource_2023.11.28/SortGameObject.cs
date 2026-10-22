using System.Collections;
using UnityEngine;

public class SortGameObject : MonoBehaviour
{
	public enum ObjType
	{
		Plant,
		Mineral,
		Animal
	}

	public ObjType MyType;

	public Collider2D Collider;

	public SpriteRenderer Image;

	public SpriteRenderer Shadow;

	private const float ShadowTime = 0.15f;

	public IEnumerator CoShadowRise()
	{
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			float num = timer / 0.15f * 0.35f;
			Collider.offset = new Vector2(0f, 0f - num);
			((Component)Shadow).transform.localPosition = new Vector3(0f, num, -0.0001f);
			yield return null;
		}
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.z = 0f;
		((Component)this).transform.localPosition = localPosition;
	}

	public IEnumerator CoShadowFall(bool inBox, AudioClip dropSound)
	{
		if (inBox)
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			localPosition.z = 2.5f;
			((Component)this).transform.localPosition = localPosition;
		}
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			float num = (1f - timer / 0.15f) * 0.35f;
			Collider.offset = new Vector2(0f, 0f - num);
			((Component)Shadow).transform.localPosition = new Vector3(0f, num, -0.0001f);
			yield return null;
		}
		VibrationManager.Vibrate(0.2f, 0.2f, 0.05f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(dropSound, loop: false);
		}
		yield return Effects.Shake(((Component)this).transform, 0.075f, 0.05f, taper: false);
	}
}
