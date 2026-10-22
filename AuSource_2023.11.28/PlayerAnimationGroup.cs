using System;
using System.Collections.Generic;
using PowerTools;
using UnityEngine;

[Serializable]
public class PlayerAnimationGroup
{
	[Header("Other")]
	public PlayerBodyTypes BodyType;

	public SpriteAnim SpriteAnimator;

	public List<SpriteAnimNodeSync> NodeSyncs;

	public OverlayKillAnimation[] KillAnims;

	[Header("Animation Clips")]
	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	public AnimationClip GhostIdleAnim;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public AnimationClip SpawnAnim;

	public AnimationClip SpawnGlowAnim;

	public AnimationClip ClimbUpAnim;

	public AnimationClip ClimbDownAnim;

	public AnimationClip GhostGuardianAngelAnim;

	[Header("Animation Settings")]
	public float RunAnimTime;

	public float ShapeshiftScale;

	public Vector3 defaultPlayerScale = new Vector3(0.5f, 0.5f, 1f);

	public PlayerAnimationGroup()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
