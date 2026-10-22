using UnityEngine;

public class PlayerParticle : PoolableBehavior
{
	public SpriteRenderer myRend;

	public float maxDistance = 6f;

	public Vector2 velocity;

	public float angularVelocity;

	private Vector3 lastCamera;

	public Camera FollowCamera { get; set; }

	public void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.localPosition;
		float sqrMagnitude = val.sqrMagnitude;
		if (Object.op_Implicit((Object)(object)FollowCamera))
		{
			Vector3 position = ((Component)FollowCamera).transform.position;
			position.z = 0f;
			val += (position - lastCamera) * (1f - ((Component)this).transform.localScale.x);
			lastCamera = position;
			Vector2 val2 = Vector2.op_Implicit(val - position);
			sqrMagnitude = val2.sqrMagnitude;
		}
		if (sqrMagnitude > maxDistance * maxDistance)
		{
			OwnerPool.Reclaim(this);
			return;
		}
		val += Vector2.op_Implicit(velocity * Time.deltaTime);
		((Component)this).transform.localPosition = val;
		((Component)this).transform.Rotate(0f, 0f, Time.deltaTime * angularVelocity);
	}
}
