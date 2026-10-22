using System.Collections.Generic;
using UnityEngine;

public class RaycastAmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;

	public float AmbientVolume = 1f;

	public float AmbientMaxDist = 8f;

	public float HitModifier = 0.25f;

	public bool ShouldVibrate = true;

	public static List<RaycastAmbientSoundPlayer> players = new List<RaycastAmbientSoundPlayer>();

	public float ambientVolume;

	public float t;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	private void OnEnable()
	{
		players.Add(this);
	}

	private void OnDisable()
	{
		players.Remove(this);
	}

	public void Start()
	{
		if (Constants.ShouldPlaySfx() && Object.op_Implicit((Object)(object)AmbientSound))
		{
			SoundManager.Instance.PlayDynamicSound("Amb " + ((Object)this).name, AmbientSound, loop: true, (AudioSource player, float dt) =>
			{
				GetAmbientSoundVolume(player, dt);
			}, SoundManager.Instance.AmbienceChannel);
		}
	}

	private void GetAmbientSoundVolume(AudioSource player, float dt)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		ambientVolume = 0f;
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			player.volume = 0f;
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(val, truePosition);
		if (num > AmbientMaxDist)
		{
			player.volume = 0f;
			return;
		}
		Vector2 val2 = truePosition - val;
		int num2 = Physics2D.RaycastNonAlloc(val, val2, volumeBuffer, num, Constants.ShipOnlyMask);
		float num3 = 1f - num / AmbientMaxDist - (float)num2 * HitModifier;
		player.volume = Mathf.Lerp(player.volume, num3 * AmbientVolume, dt);
		ambientVolume = player.volume;
	}
}
