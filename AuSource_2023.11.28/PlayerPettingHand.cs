using System.Collections;
using PowerTools;
using UnityEngine;

public class PlayerPettingHand : MonoBehaviour
{
	public SpriteRenderer HandSprite;

	public SpriteAnim HandSpriteAnim;

	public AnimationClip PetClip;

	private const int PET_ANIMATION_START_FRAME = 6;

	private const float ZPOS_PETTING = 1E-05f;

	public void StartPet(PetBehaviour pet)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.position = new Vector3(pet.PettingHandPosition.x, pet.PettingHandPosition.y, ((Component)pet).transform.position.z - 1E-05f);
		HandSpriteAnim.Play(PetClip);
		((MonoBehaviour)this).StartCoroutine(WaitForPetAnimStart(pet));
	}

	public IEnumerator WaitForPetAnimStart(PetBehaviour pet)
	{
		while (HandSpriteAnim.FrameTime < 6)
		{
			yield return null;
		}
		pet.StartPetAnim();
	}

	public IEnumerator WaitUntilFinishedPetting()
	{
		yield return new WaitForAnimationFinish(HandSpriteAnim, PetClip);
		StopPetting();
	}

	public void StopPetting()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
