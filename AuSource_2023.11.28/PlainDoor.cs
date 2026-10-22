using System.Collections;
using Hazel;
using PowerTools;
using UnityEngine;

public class PlainDoor : OpenableDoor
{
	public bool Open;

	public BoxCollider2D myCollider;

	public Collider2D shadowCollider;

	public SpriteAnim animator;

	public AnimationClip OpenDoorAnim;

	public AnimationClip CloseDoorAnim;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private float size;

	public override bool IsOpen => Open;

	protected virtual void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = myCollider.size;
		size = ((val.x > val.y) ? val.y : val.x);
		Open = ((Collider2D)myCollider).isTrigger;
		animator.Play(Open ? OpenDoorAnim : CloseDoorAnim, 1000f);
		UpdateShadow();
	}

	public override void SetDoorway(bool open)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (Open == open)
		{
			return;
		}
		Open = open;
		((Collider2D)myCollider).isTrigger = open;
		UpdateShadow();
		animator.Play(open ? OpenDoorAnim : CloseDoorAnim);
		((MonoBehaviour)this).StopAllCoroutines();
		if (!open)
		{
			Vector2 val = myCollider.size;
			((MonoBehaviour)this).StartCoroutine(CoCloseDoorway(val.x > val.y));
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, CloseSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, CloseSound);
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound(((Object)this).name, OpenSound, loop: false, DoorDynamics, SoundManager.Instance.SfxChannel);
			}
			VibrationManager.Vibrate(2.5f, Vector2.op_Implicit(((Component)this).transform.position), 3f, 0f, VibrationManager.VibrationFalloff.None, OpenSound);
		}
	}

	private void UpdateShadow()
	{
		if ((Object)(object)shadowCollider != (Object)null)
		{
			((Behaviour)shadowCollider).enabled = !Open;
		}
	}

	private IEnumerator CoCloseDoorway(bool isHort)
	{
		Vector2 s = myCollider.size;
		float i = 0f;
		if (isHort)
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.y = Mathf.Lerp(0.0001f, size, i / 0.1f);
				myCollider.size = s;
				yield return null;
			}
		}
		else
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.x = Mathf.Lerp(0.0001f, size, i / 0.1f);
				myCollider.size = s;
				yield return null;
			}
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
		writer.Write(Open);
	}

	public override void Deserialize(MessageReader reader)
	{
		SetDoorway(reader.ReadBoolean());
	}

	public override bool DoUpdate(float dt)
	{
		return false;
	}
}
