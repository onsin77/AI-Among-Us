using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class ButtonAnimRolloverHandler : MonoBehaviour
{
	public Sprite StaticOutImage;

	public AnimationClip RolloverAnim;

	public AudioClip HoverSound;

	private SpriteRenderer target;

	private SpriteAnim animTarget;

	public void Start()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected Obj, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected Obj, but got Unknown
		target = ((Component)this).GetComponent<SpriteRenderer>();
		animTarget = ((Component)this).GetComponent<SpriteAnim>();
		PassiveButton component = ((Component)this).GetComponent<PassiveButton>();
		component.OnMouseOver.AddListener((UnityAction)DoMouseOver);
		component.OnMouseOut.AddListener((UnityAction)DoMouseOut);
	}

	public void DoMouseOver()
	{
		animTarget.Play(RolloverAnim);
		if (Object.op_Implicit((Object)(object)HoverSound))
		{
			SoundManager.Instance.PlaySound(HoverSound, loop: false);
		}
	}

	public void DoMouseOut()
	{
		animTarget.Stop();
		target.sprite = StaticOutImage;
	}
}
