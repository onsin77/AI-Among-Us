using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
	public PlayerParticleInfo[] Sprites;

	public PlayerParticleInfo[] HorseSprites;

	public FloatRange velocity;

	public FloatRange scale;

	public ObjectPoolBehavior pool;

	public float StartRadius;

	public PoolableBehavior HorsePrefab;

	public Camera FollowCamera;

	private RandomFill<PlayerParticleInfo> fill;

	public void Start()
	{
		fill = new RandomFill<PlayerParticleInfo>();
		if (Constants.ShouldHorseAround())
		{
			fill.Set(HorseSprites);
			pool.Prefab = HorsePrefab;
		}
		else
		{
			fill.Set(Sprites);
		}
		int num = 0;
		while (pool.NotInUse > 0)
		{
			PlayerParticle playerParticle = pool.Get<PlayerParticle>();
			PlayerMaterial.SetColors(num++, (Renderer)(object)playerParticle.myRend);
			PlacePlayer(playerParticle, initial: true);
		}
	}

	public void Update()
	{
		while (pool.NotInUse > 0)
		{
			PlayerParticle part = pool.Get<PlayerParticle>();
			PlacePlayer(part, initial: false);
		}
	}

	private void PlacePlayer(PlayerParticle part, bool initial)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(Random.insideUnitCircle);
		if (!initial)
		{
			val.Normalize();
		}
		val *= StartRadius;
		float num = scale.Next();
		if (Constants.ShouldHorseAround())
		{
			num *= 0.7f;
		}
		((Component)part).transform.localScale = new Vector3(num, num, num);
		val.z = (0f - num) * 0.001f;
		if (Object.op_Implicit((Object)(object)FollowCamera))
		{
			Vector3 position = ((Component)FollowCamera).transform.position;
			position.z = 0f;
			val += position;
			part.FollowCamera = FollowCamera;
		}
		((Component)part).transform.localPosition = val;
		PlayerParticleInfo playerParticleInfo = fill.Get();
		part.myRend.sprite = playerParticleInfo.image;
		part.myRend.flipX = BoolRange.Next();
		Vector2 self = Vector2.op_Implicit(-val.normalized);
		self = self.Rotate(FloatRange.Next(-45f, 45f));
		part.velocity = self * velocity.Next();
		part.angularVelocity = playerParticleInfo.angularVel.Next();
		if (playerParticleInfo.alignToVel)
		{
			((Component)part).transform.localEulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(self));
		}
	}
}
