using Hazel;
using UnityEngine;

public class MushroomWallDoor : OpenableDoor
{
	[SerializeField]
	private BoxCollider2D wallCollider;

	[SerializeField]
	private Collider2D shadowColl;

	[SerializeField]
	private MushroomWallMushroom[] mushrooms;

	[SerializeField]
	private AudioClip openSound;

	[SerializeField]
	private AudioClip closeSound;

	private bool open;

	private bool allowAudio;

	public override bool IsOpen => open;

	private void Start()
	{
		allowAudio = false;
		SetDoorway(open: true);
		allowAudio = true;
	}

	public override void SetDoorway(bool open)
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (this.open == open)
		{
			return;
		}
		this.open = open;
		((Collider2D)wallCollider).isTrigger = open;
		((Behaviour)shadowColl).enabled = !open;
		MushroomWallMushroom[] array = mushrooms;
		foreach (MushroomWallMushroom mushroomWallMushroom in array)
		{
			if (open)
			{
				mushroomWallMushroom.Hide();
			}
			else
			{
				mushroomWallMushroom.Show();
			}
		}
		if (open)
		{
			if (Constants.ShouldPlaySfx() && allowAudio)
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, openSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, openSound);
		}
		else
		{
			if (Constants.ShouldPlaySfx() && allowAudio)
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, closeSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, closeSound);
		}
	}

	private void DoorDynamics(AudioSource source, float dt)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(val, truePosition);
		if (num > 4f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 4f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
	}

	public override void Serialize(MessageWriter writer)
	{
		writer.Write(open);
	}

	public override void Deserialize(MessageReader reader)
	{
		SetDoorway(reader.ReadBoolean());
	}

	public override bool DoUpdate(float deltaTime)
	{
		return false;
	}
}
