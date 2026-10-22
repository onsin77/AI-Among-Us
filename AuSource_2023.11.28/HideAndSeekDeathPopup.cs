using System.Collections;
using TMPro;
using UnityEngine;

public class HideAndSeekDeathPopup : MonoBehaviour
{
	[SerializeField]
	private HideAndSeekDeathPopupNameplate nameplate;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AudioClip sfx;

	[SerializeField]
	private TextMeshPro text;

	public void Show(PlayerControl player, int deathIndex)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Constants.ShouldHorseAround())
		{
			((Behaviour)text).enabled = false;
		}
		nameplate.SetPlayer(player);
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.z -= deathIndex;
		((Component)this).transform.localPosition = localPosition;
		((MonoBehaviour)this).StartCoroutine(AnimateCoroutine());
	}

	private void AnimEvent_PlaySound()
	{
		SoundManager.Instance.PlaySoundImmediate(sfx, loop: false);
	}

	private IEnumerator AnimateCoroutine()
	{
		while (true)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (!currentAnimatorStateInfo.IsName("Show"))
			{
				break;
			}
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
