using System.Collections;
using PowerTools;
using UnityEngine;

public class MushroomWallMushroom : MonoBehaviour
{
	[SerializeField]
	private SpriteAnim animator;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private AnimationClip showClip;

	[SerializeField]
	private AnimationClip hideClip;

	private Coroutine hideCoroutine;

	public void Show()
	{
		((Renderer)sprite).enabled = true;
		if (hideCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideCoroutine);
		}
		animator.Play(showClip);
	}

	public void Hide()
	{
		hideCoroutine = ((MonoBehaviour)this).StartCoroutine(CoHideAndThenDisableSprite());
	}

	private IEnumerator CoHideAndThenDisableSprite()
	{
		yield return new WaitForAnimationFinish(animator, hideClip);
		((Renderer)sprite).enabled = false;
		hideCoroutine = null;
	}
}
