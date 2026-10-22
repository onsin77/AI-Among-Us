using UnityEngine;

public class HowToPlaySceneMovementPageDemoKeyboardStick : VirtualJoystick
{
	public SpriteRenderer UpKey;

	public SpriteRenderer DownKey;

	public SpriteRenderer LeftKey;

	public SpriteRenderer RightKey;

	protected override void FixedUpdate()
	{
	}

	public override void UpdateJoystick(HowToPlaySceneMovementPageFingerBehaviour finger, Vector2 velocity, bool syncFinger)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)UpKey).enabled = velocity.y > 0.1f;
		((Renderer)DownKey).enabled = velocity.y < -0.1f;
		((Renderer)RightKey).enabled = velocity.x > 0.1f;
		((Renderer)LeftKey).enabled = velocity.x < -0.1f;
	}
}
