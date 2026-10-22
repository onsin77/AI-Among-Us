using PowerTools;
using UnityEngine;

public class SurvCamera : MonoBehaviour
{
	public string CamName;

	public StringNames NewName;

	public SpriteAnim[] Images;

	public float CamSize = 3f;

	public float CamAspect = 1f;

	public Vector3 Offset;

	public AnimationClip OnAnim;

	public AnimationClip OffAnim;

	public StringNames camNameString;

	public void Awake()
	{
		if (Images == null || Images.Length == 0)
		{
			Images = ((Component)this).GetComponents<SpriteAnim>();
		}
	}

	public virtual void SetAnimation(bool on)
	{
		SpriteAnim[] images = Images;
		for (int i = 0; i < images.Length; i++)
		{
			images[i].Play(on ? OnAnim : OffAnim);
		}
	}
}
