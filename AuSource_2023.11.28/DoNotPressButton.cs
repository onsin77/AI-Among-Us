using UnityEngine;

public class DoNotPressButton : MonoBehaviour
{
	[SerializeField]
	private PlayerParticles particleManager;

	[SerializeField]
	private AudioClip ejectSound;

	[SerializeField]
	private SpriteRenderer pressedSprite;

	[SerializeField]
	private SpriteRenderer unpressedSprite;

	public void EjectCrewmate()
	{
		SoundManager.Instance.PlaySound(ejectSound, loop: false);
	}
}
