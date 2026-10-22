using UnityEngine;

public class AnimationOffsetter : MonoBehaviour
{
	public Animator anim;

	private void Start()
	{
		anim.speed = FloatRange.Next(0.9f, 1.1f);
	}
}
