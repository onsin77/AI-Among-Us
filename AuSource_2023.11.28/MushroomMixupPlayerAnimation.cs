using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class MushroomMixupPlayerAnimation : MonoBehaviour, IPlayerVisibleItem
{
	private const float MIN_ANIMATION_DURATION_SECONDS = 1f;

	[SerializeField]
	private SpriteAnim spriteAnim;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private AnimationClip spawnAnimation;

	[SerializeField]
	private AnimationClip idleAnimation;

	[SerializeField]
	private AnimationClip despawnAnimation;

	private PlayerControl player;

	public bool Visible
	{
		get
		{
			return ((Renderer)sprite).enabled;
		}
		set
		{
			((Renderer)sprite).enabled = value;
		}
	}

	public void StartAnimation(IEnumerator doWhileAnimating, Action doInsideCloud)
	{
		((MonoBehaviour)this).StartCoroutine(CoPlay(doWhileAnimating, doInsideCloud));
	}

	private IEnumerator CoPlay(IEnumerator doWhileAnimating, Action doInsideCloud)
	{
		float startTime = Time.time;
		((MonoBehaviour)this).StartCoroutine(CoAnimateCloud());
		if (doWhileAnimating != null)
		{
			yield return doWhileAnimating;
		}
		float num = Time.time - startTime;
		if (num < 1f)
		{
			yield return Effects.Wait(1f - num);
		}
		doInsideCloud();
		yield return new WaitForAnimationFinish(spriteAnim, despawnAnimation);
		player.UnregisterVisibilityItem(this);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private IEnumerator CoAnimateCloud()
	{
		yield return new WaitForAnimationFinish(spriteAnim, spawnAnimation);
		spriteAnim.Play(idleAnimation);
	}

	public void SetPlayer(PlayerControl player)
	{
		this.player = player;
		player.RegisterVisibilityItem(this);
		PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(sprite, player.AmOwner);
	}
}
