using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class Asteroid : PoolableBehavior
{
	public Sprite[] AsteroidImages;

	public Sprite[] BrokenImages;

	private int imgIdx;

	public FloatRange MoveSpeed = new FloatRange(2f, 5f);

	public FloatRange RotateSpeed = new FloatRange(-10f, 10f);

	public SpriteRenderer Explosion;

	public Vector3 TargetPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		internal set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public void FixedUpdate()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		Quaternion localRotation = ((Component)this).transform.localRotation;
		transform.localRotation = Quaternion.Euler(0f, 0f, localRotation.eulerAngles.z + RotateSpeed.Last * Time.fixedDeltaTime);
		Vector3 val = TargetPosition - ((Component)this).transform.localPosition;
		if (val.sqrMagnitude > 0.05f)
		{
			val.Normalize();
			Transform transform2 = ((Component)this).transform;
			transform2.localPosition += val * MoveSpeed.Last * Time.fixedDeltaTime;
		}
		else
		{
			OwnerPool.Reclaim(this);
		}
	}

	public override void Reset()
	{
		((Behaviour)this).enabled = true;
		((Renderer)Explosion).enabled = false;
		SpriteRenderer component = ((Component)this).GetComponent<SpriteRenderer>();
		imgIdx = AsteroidImages.RandomIdx();
		component.sprite = AsteroidImages[imgIdx];
		((Renderer)component).enabled = true;
		ButtonBehavior component2 = ((Component)this).GetComponent<ButtonBehavior>();
		((Behaviour)component2).enabled = true;
		((UnityEventBase)component2.OnClick).RemoveAllListeners();
		((Component)this).transform.Rotate(0f, 0f, RotateSpeed.Next());
		MoveSpeed.Next();
		base.Reset();
	}

	public IEnumerator CoBreakApart()
	{
		((Behaviour)this).enabled = false;
		((Behaviour)((Component)this).GetComponent<ButtonBehavior>()).enabled = false;
		VibrationManager.Vibrate(0.5f, 0.5f, 0.35f, VibrationManager.VibrationFalloff.Linear);
		((Renderer)Explosion).enabled = true;
		yield return new WaitForLerp(0.1f, (float t) =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			((Component)Explosion).transform.localScale = new Vector3(t, t, t);
		});
		yield return (object)new WaitForSeconds(0.05f);
		yield return new WaitForLerp(0.05f, (float t) =>
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			((Component)Explosion).transform.localScale = new Vector3(1f - t, 1f - t, 1f - t);
		});
		SpriteRenderer rend = ((Component)this).GetComponent<SpriteRenderer>();
		yield return null;
		rend.sprite = BrokenImages[imgIdx];
		yield return (object)new WaitForSeconds(0.2f);
		OwnerPool.Reclaim(this);
	}
}
