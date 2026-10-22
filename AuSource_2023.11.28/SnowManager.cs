using System.Collections;
using UnityEngine;

public class SnowManager : MonoBehaviour
{
	public ParticleSystem particles;

	private ParticleSystemRenderer rend;

	private float timer;

	private void Start()
	{
		rend = ((Component)this).GetComponent<ParticleSystemRenderer>();
		((MonoBehaviour)this).StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		ContactFilter2D filter = new ContactFilter2D
		{
			layerMask = LayerMask.op_Implicit(Constants.ShipOnlyMask),
			useLayerMask = true,
			useTriggers = true
		};
		Collider2D[] buffer = new Collider2D[10];
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		while (true)
		{
			yield return wait;
			if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
			{
				continue;
			}
			bool flag = false;
			int num = PlayerControl.LocalPlayer.Collider.OverlapCollider(filter, buffer);
			for (int i = 0; i < num; i++)
			{
				if (((Component)buffer[i]).tag == "NoSnow")
				{
					flag = true;
				}
			}
			if (!particles.isPlaying)
			{
				if (!flag)
				{
					particles.Play();
				}
			}
			else if (flag)
			{
				timer = Mathf.Max(0f, timer - 0.2f);
			}
			else
			{
				timer = Mathf.Min(1f, timer + 0.2f);
			}
			SetPartAlpha();
		}
	}

	private void SetPartAlpha()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Color val = new Color(1f, 1f, 1f, timer);
		((Renderer)rend).material.SetColor("_Color", val);
	}
}
