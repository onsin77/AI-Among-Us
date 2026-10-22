using System.Collections;
using PowerTools;
using UnityEngine;

public class HandZiplinePoolable : PoolableBehavior
{
	[SerializeField]
	private SpriteRenderer handRenderer;

	[SerializeField]
	private SpriteAnim spriteAnim;

	[SerializeField]
	private AnimationClip DownIntroAnim;

	[SerializeField]
	private AnimationClip DownLoopAnim;

	[SerializeField]
	private AnimationClip DownOutroAnim;

	[SerializeField]
	private AnimationClip UpIntroAnim;

	[SerializeField]
	private AnimationClip UpLoopAnim;

	[SerializeField]
	private AnimationClip UpOutroAnim;

	public void StartUpAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(CoUpAnimation());
	}

	public void StartUpOutroAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(CoUpOutroAnimation());
	}

	public void StartDownAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(CoDownAnimation());
	}

	public void StartDownOutroAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(CoDownOutroAnimation());
	}

	public void SetPlayerColor(GameData.PlayerOutfit outfit, PlayerMaterial.MaskType maskType)
	{
		((Renderer)handRenderer).sharedMaterial = CosmeticsLayer.GetBodyMaterial(maskType);
		PlayerMaterial.SetColors(outfit.ColorId, (Renderer)(object)handRenderer);
	}

	private IEnumerator CoDownAnimation()
	{
		spriteAnim.Play(DownIntroAnim);
		yield return new WaitForAnimationFinish(spriteAnim, DownIntroAnim);
		spriteAnim.Play(DownLoopAnim);
	}

	private IEnumerator CoDownOutroAnimation()
	{
		spriteAnim.Play(DownOutroAnim);
		yield return new WaitForAnimationFinish(spriteAnim, DownOutroAnim);
		((Component)this).gameObject.SetActive(false);
	}

	private IEnumerator CoUpAnimation()
	{
		spriteAnim.Play(UpIntroAnim);
		yield return new WaitForAnimationFinish(spriteAnim, UpIntroAnim);
		spriteAnim.Play(UpLoopAnim);
	}

	private IEnumerator CoUpOutroAnimation()
	{
		spriteAnim.Play(UpOutroAnim);
		yield return new WaitForAnimationFinish(spriteAnim, UpOutroAnim);
		((Component)this).gameObject.SetActive(false);
	}
}
