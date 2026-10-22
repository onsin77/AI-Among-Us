using System;
using System.Collections;
using System.Collections.Generic;
using PowerTools;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
	[SerializeField]
	private SpriteAnim glowAnimator;

	[SerializeField]
	private SpriteRenderer glowSpriteRenderer;

	[SerializeField]
	private List<PlayerAnimationGroup> animationGroups;

	[SerializeField]
	[Header("Med Scan")]
	private SpriteAnim[] scannerAnims;

	[SerializeField]
	private SpriteRenderer[] scannersImages;

	private PlayerAnimationGroup group;

	private PlayerBodyTypes type;

	public float Time => Animator.Time;

	public float ShapeshiftScale => group.ShapeshiftScale;

	public Vector3 DefaultPlayerScale
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return group.defaultPlayerScale;
		}
	}

	private SpriteAnim Animator
	{
		get
		{
			if (group == null)
			{
				throw new NullReferenceException("PlayerAnimations have not been initialized properly before attempting usage.");
			}
			return group.SpriteAnimator;
		}
	}

	public void SetBodyType(PlayerBodyTypes bodyType, Vector3 flippedCosmeticOffset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		type = bodyType;
		foreach (PlayerAnimationGroup animationGroup in animationGroups)
		{
			if (animationGroup.BodyType == type)
			{
				group = animationGroup;
				group.SpriteAnimator.Initialize();
				animationGroup.NodeSyncs.ForEach((SpriteAnimNodeSync n) =>
				{
					//IL_0009: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					((Behaviour)n).enabled = true;
					n.flipOffset = flippedCosmeticOffset;
				});
			}
			else
			{
				animationGroup.NodeSyncs.ForEach((SpriteAnimNodeSync n) =>
				{
					((Behaviour)n).enabled = false;
				});
			}
		}
		if (group == null)
		{
			throw new NullReferenceException("Somehow, the animation group was still null after setting body type.");
		}
	}

	public OverlayKillAnimation[] GetKillAnimations()
	{
		return group.KillAnims;
	}

	public bool IsPlayingRunAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.RunAnim;
	}

	public bool IsPlayingSpawnAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.SpawnAnim;
	}

	public bool IsPlayingClimbAnimation()
	{
		if (!((Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.ClimbUpAnim))
		{
			return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.ClimbDownAnim;
		}
		return true;
	}

	public bool IsPlayingGuardianAngelIdleAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.GhostGuardianAngelAnim;
	}

	public bool IsPlayingGhostIdleAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.GhostIdleAnim;
	}

	public bool IsPlayingEnterVentAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.EnterVentAnim;
	}

	public bool IsPlayingAnyLadderAnimation()
	{
		if (!((Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.ClimbDownAnim))
		{
			return (Object)(object)Animator.GetCurrentAnimation() == (Object)(object)group.ClimbUpAnim;
		}
		return true;
	}

	public bool IsPlayingSomeAnimation()
	{
		return (Object)(object)Animator.GetCurrentAnimation() != (Object)null;
	}

	public void PlayRunAnimation()
	{
		Animator.Play(group.RunAnim);
		if (group.RunAnimTime > 0f)
		{
			Animator.Time = group.RunAnimTime;
		}
	}

	public void PlayIdleAnimation()
	{
		Animator.Play(group.IdleAnim);
	}

	public void PlayClimbAnimation(bool down)
	{
		Animator.Play(down ? group.ClimbDownAnim : group.ClimbUpAnim);
		Animator.Time = 0f;
	}

	public void PlayGuardianAngelIdleAnimation()
	{
		Animator.Play(group.GhostGuardianAngelAnim);
	}

	public void PlayGhostIdleAnimation()
	{
		Animator.Play(group.GhostIdleAnim);
	}

	public IEnumerator CoPlaySpawnAnimation(bool flipX)
	{
		glowSpriteRenderer.flipX = flipX;
		glowAnimator.Play(group.SpawnGlowAnim);
		yield return new WaitForAnimationFinish(Animator, group.SpawnAnim);
	}

	public IEnumerator CoPlayEnterVentAnimation(int numFramesUntilDisappearance)
	{
		Animator.Play(group.EnterVentAnim);
		if (numFramesUntilDisappearance > 0)
		{
			while (Animator.FrameTime < numFramesUntilDisappearance)
			{
				yield return null;
			}
			Animator.Stop();
		}
		else
		{
			while (Animator.IsPlaying())
			{
				yield return null;
			}
		}
	}

	public IEnumerator CoPlayExitVentAnimation()
	{
		Animator.Play(group.ExitVentAnim);
		while (Animator.IsPlaying())
		{
			yield return null;
		}
	}

	public IEnumerator CoPlayJumpAnimation()
	{
		yield return new WaitForAnimationFinish(Animator, group.EnterVentAnim, restartIfAlreadyPlaying: true, 5);
	}

	public IEnumerator CoPlayCustomAnimation(AnimationClip customAnim)
	{
		yield return new WaitForAnimationFinish(Animator, customAnim);
	}

	public void PlayScanner(bool on, bool isDead, bool flipX)
	{
		for (int i = 0; i < scannerAnims.Length; i++)
		{
			SpriteAnim spriteAnim = scannerAnims[i];
			if (on && !isDead)
			{
				((Component)spriteAnim).gameObject.SetActive(true);
				spriteAnim.Play();
				scannersImages[i].flipX = !flipX;
				continue;
			}
			if (((Behaviour)spriteAnim).isActiveAndEnabled)
			{
				spriteAnim.Stop();
			}
			((Component)spriteAnim).gameObject.SetActive(false);
		}
	}
}
