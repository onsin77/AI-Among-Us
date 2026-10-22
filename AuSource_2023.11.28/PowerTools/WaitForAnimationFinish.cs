using System.Collections;
using UnityEngine;

namespace PowerTools;

public class WaitForAnimationFinish : IEnumerator
{
	private SpriteAnim animator;

	private AnimationClip clip;

	private bool first = true;

	private int stopFrame = -1;

	public object Current => null;

	public WaitForAnimationFinish(SpriteAnim animator, AnimationClip clip, bool restartIfAlreadyPlaying = true, int stopAtFrame = -1)
	{
		this.animator = animator;
		this.clip = clip;
		stopFrame = stopAtFrame;
		if (restartIfAlreadyPlaying)
		{
			this.animator.Play(this.clip);
			this.animator.Time = 0f;
		}
	}

	public bool MoveNext()
	{
		if (first)
		{
			first = false;
			return true;
		}
		try
		{
			if (stopFrame != -1 && animator.FrameTime >= stopFrame)
			{
				animator.Stop();
				return false;
			}
			return animator.IsPlaying(clip);
		}
		catch
		{
			return false;
		}
	}

	public void Reset()
	{
		first = true;
		animator.Play(clip);
	}
}
