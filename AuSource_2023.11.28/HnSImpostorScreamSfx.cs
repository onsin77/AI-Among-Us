using UnityEngine;

public class HnSImpostorScreamSfx : MonoBehaviour
{
	[SerializeField]
	private AudioClip HnSLocalImpostorTransformSfx;

	[SerializeField]
	private AudioClip HnSOtherImpostorTransformSfx;

	[SerializeField]
	private AudioClip HnSLocalYeehawSfx;

	[SerializeField]
	private AudioClip HnSOtherYeehawSfx;

	public void LocalImpostorScream()
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			SoundManager.Instance.PlaySound(HnSLocalImpostorTransformSfx, loop: false);
		}
	}

	public void OtherImpostorScream()
	{
		if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			SoundManager.Instance.PlaySound(HnSOtherImpostorTransformSfx, loop: false);
		}
	}

	public void LocalImpostorYeehaw()
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			SoundManager.Instance.PlaySound(HnSLocalYeehawSfx, loop: false);
		}
	}

	public void OtherImpostorYeehaw()
	{
		if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			SoundManager.Instance.PlaySound(HnSOtherYeehawSfx, loop: false);
		}
	}
}
