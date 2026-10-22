using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VentDirt : PoolableBehavior
{
	public Sprite[] DirtImages;

	public ParticleSystem CleanedEffect;

	private int imgIdx;

	public override void Reset()
	{
		((Behaviour)this).enabled = true;
		SpriteRenderer component = ((Component)this).GetComponent<SpriteRenderer>();
		imgIdx = DirtImages.RandomIdx();
		component.sprite = DirtImages[imgIdx];
		((Renderer)component).enabled = true;
		ButtonBehavior component2 = ((Component)this).GetComponent<ButtonBehavior>();
		((Behaviour)component2).enabled = true;
		((UnityEventBase)component2.OnClick).RemoveAllListeners();
		base.Reset();
	}

	public IEnumerator CoDisappear()
	{
		if (((Behaviour)this).enabled)
		{
			((Behaviour)this).enabled = false;
			((Behaviour)((Component)this).GetComponent<ButtonBehavior>()).enabled = false;
			VibrationManager.Vibrate(0.5f, 0.5f, 0.35f, VibrationManager.VibrationFalloff.Linear);
			CleanedEffect.Play();
			SpriteRenderer rend = ((Component)this).GetComponent<SpriteRenderer>();
			yield return Effects.Lerp(0.2f, (float t) =>
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				((Component)rend).transform.localScale = new Vector3(1f - t, 1f - t, 1f - t);
			});
			yield return Effects.Wait(0.1f);
			rend.sprite = null;
			OwnerPool.Reclaim(this);
		}
	}
}
