using UnityEngine;

public class TriggeredSound : MonoBehaviour
{
	public AudioClip[] SoundToPlay;

	public FloatRange PitchRange = new FloatRange(1f, 1f);

	private AudioSource Player;

	public float MaxVolume = 1f;

	public float MaxDist = 6f;

	public float HitModifier = 0.25f;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	public void Start()
	{
		Player = SoundManager.Instance.GetNamedSfxSource(((Object)this).name + ((Object)this).GetInstanceID());
		Player.playOnAwake = false;
		Player.loop = false;
	}

	public void PlaySound()
	{
		if (Object.op_Implicit((Object)(object)Player) && SoundToPlay != null && PitchRange != null)
		{
			Player.clip = SoundToPlay.Random();
			Player.pitch = PitchRange.Next();
			GetAmbientSoundVolume(Player);
			Player.Play();
		}
	}

	private void GetAmbientSoundVolume(AudioSource player)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			player.volume = 0f;
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(val, truePosition);
		if (num > MaxDist)
		{
			player.volume = 0f;
			return;
		}
		ContactFilter2D val2 = new ContactFilter2D
		{
			useTriggers = false,
			layerMask = LayerMask.op_Implicit(Constants.ShipOnlyMask),
			useLayerMask = true
		};
		Vector2 val3 = truePosition - val;
		int num2 = Physics2D.Raycast(val, val3, val2, volumeBuffer, num);
		float num3 = 1f - num / MaxDist - (float)num2 * HitModifier;
		player.volume = num3 * MaxVolume;
	}
}
