using UnityEngine;

public class EngineBehaviour : MonoBehaviour
{
	public AudioClip ElectricSound;

	public AudioClip SteamSound;

	public float SoundDistance = 5f;

	public void PlayElectricSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("EngineShock" + ((Object)this).name, ElectricSound, loop: false, GetSoundDistance, SoundManager.Instance.SfxChannel);
		}
	}

	public void PlaySteamSound()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Constants.ShouldPlaySfx())
		{
			float pitch = FloatRange.Next(0.7f, 1.1f);
			SoundManager.Instance.PlayDynamicSound("EngineSteam" + ((Object)this).name, SteamSound, loop: false, (AudioSource p, float d) =>
			{
				GetSoundDistance(p, d, pitch);
			}, SoundManager.Instance.SfxChannel);
			VibrationManager.Vibrate(1f, Vector2.op_Implicit(((Component)this).transform.position), SoundDistance, 0f, VibrationManager.VibrationFalloff.None, SteamSound);
		}
	}

	private void GetSoundDistance(AudioSource player, float dt)
	{
		GetSoundDistance(player, dt, 1f);
	}

	private void GetSoundDistance(AudioSource player, float dt, float pitch)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			player.volume = 0f;
			return;
		}
		float num = Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), PlayerControl.LocalPlayer.GetTruePosition());
		float num2 = 1f - num / SoundDistance;
		player.volume = num2 * 0.8f;
		player.pitch = pitch;
	}
}
