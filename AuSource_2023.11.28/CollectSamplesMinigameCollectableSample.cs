using System;
using UnityEngine;

public class CollectSamplesMinigameCollectableSample : MonoBehaviour
{
	[SerializeField]
	private PassiveButton button;

	[SerializeField]
	private AudioClip collectSfx;

	public event Action<CollectSamplesMinigameCollectableSample> OnPressEvent;

	private void Awake()
	{
		button.AddOnClickListeners(OnButtonPressed);
	}

	private void OnButtonPressed()
	{
		button.SetButtonEnableState(enabled: false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySoundImmediate(collectSfx, loop: false);
		}
		OnPressEvent?.Invoke(this);
	}
}
