using System.Collections;
using PowerTools;
using UnityEngine;

public class HowToPlaySceneMovementPagePlayerAnimator : MonoBehaviour
{
	public float Speed = 2.5f;

	public VirtualJoystick joystick;

	public SpriteRenderer UseButton;

	public HowToPlaySceneMovementPageFingerBehaviour finger;

	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	private Vector2 velocity;

	[HideInInspector]
	private SpriteAnim Animator;

	[HideInInspector]
	private SpriteRenderer rend;

	public int NearbyConsoles;

	private void Start()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Animator = ((Component)this).GetComponent<SpriteAnim>();
		rend = ((Component)this).GetComponent<SpriteRenderer>();
		((Renderer)rend).material.SetColor("_BackColor", Color32.op_Implicit(Palette.ShadowColors[0]));
		((Renderer)rend).material.SetColor("_BodyColor", Color32.op_Implicit(Palette.PlayerColors[0]));
		((Renderer)rend).material.SetColor("_VisorColor", Color32.op_Implicit(Palette.VisorColor));
	}

	public void FixedUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Translate(Vector2.op_Implicit(velocity * Time.fixedDeltaTime));
		((Renderer)UseButton).enabled = NearbyConsoles > 0;
	}

	public void LateUpdate()
	{
		if (velocity.sqrMagnitude >= 0.1f)
		{
			if ((Object)(object)Animator.GetCurrentAnimation() != (Object)(object)RunAnim)
			{
				Animator.Play(RunAnim);
			}
			rend.flipX = velocity.x < 0f;
		}
		else if ((Object)(object)Animator.GetCurrentAnimation() == (Object)(object)RunAnim)
		{
			Animator.Play(IdleAnim);
		}
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, bool relax, float tolerance = 0.01f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		worldPos.y += 0.3636f;
		if (!(joystick is HowToPlaySceneMovementPageDemoKeyboardStick))
		{
			finger.ClickOn();
		}
		while (true)
		{
			Vector2 val2;
			Vector2 val = (val2 = worldPos - Vector2.op_Implicit(((Component)this).transform.position));
			if (!(val.sqrMagnitude > tolerance))
			{
				break;
			}
			float num = Mathf.Clamp(val2.magnitude * 2f, 0.01f, 1f);
			velocity = val2.normalized * Speed * num;
			joystick.UpdateJoystick(finger, velocity, syncFinger: true);
			yield return null;
		}
		if (relax)
		{
			finger.ClickOff();
			velocity = Vector2.zero;
			joystick.UpdateJoystick(finger, velocity, syncFinger: false);
		}
	}
}
