using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
	public Sprite[] Images;

	public FloatRange SpinSpeed = new FloatRange(-45f, 45f);

	public Vector2Range StartVel;

	public float AccelRate = 30f;

	[HideInInspector]
	public LeafMinigame Parent;

	public bool Held;

	private static RandomFill<Sprite> ImageFiller = new RandomFill<Sprite>();

	[HideInInspector]
	public Rigidbody2D body;

	public void Start()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		ImageFiller.Set(Images);
		Sprite val = ImageFiller.Get();
		if (!Object.op_Implicit((Object)(object)val))
		{
			ImageFiller = new RandomFill<Sprite>();
			ImageFiller.Set(Images);
			val = ImageFiller.Get();
		}
		((Component)this).GetComponent<SpriteRenderer>().sprite = val;
		body = ((Component)this).GetComponent<Rigidbody2D>();
		body.angularVelocity = SpinSpeed.Next();
		body.velocity = StartVel.Next();
	}

	public void FixedUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!Held && (double)((Component)this).transform.localPosition.x < -2.5)
		{
			Parent.LeafDone(this);
		}
	}
}
