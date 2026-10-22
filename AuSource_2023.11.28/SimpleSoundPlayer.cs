using UnityEngine;

public class SimpleSoundPlayer : MonoBehaviour
{
	public AudioClip[] clips;

	private AudioSource soundSource;

	private void OnEnable()
	{
		soundSource = ((Component)this).GetComponent<AudioSource>();
	}

	public void PlaySound()
	{
		soundSource.Play();
	}

	public void PlaySpecificSound(int index)
	{
		soundSource.clip = clips[index];
		soundSource.Play();
	}
}
