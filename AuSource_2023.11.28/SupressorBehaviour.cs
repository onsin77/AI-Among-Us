using PowerTools;
using UnityEngine;

public class SupressorBehaviour : MonoBehaviour
{
	public Sprite ActiveBase;

	public Sprite InactiveBase;

	public SpriteRenderer BaseImage;

	public AnimationClip ElectricActive;

	public AnimationClip ElectricInactive;

	public SpriteAnim Electric;

	public AnimationClip LightsActive;

	public AnimationClip LightsInactive;

	public SpriteAnim Lights;

	public void Activate()
	{
		BaseImage.sprite = ActiveBase;
		Electric.Play(ElectricActive);
		Lights.Play(LightsActive);
	}

	public void Deactivate()
	{
		BaseImage.sprite = InactiveBase;
		Electric.Play(ElectricInactive);
		Lights.Play(LightsInactive);
	}
}
