using System;
using System.Collections;
using Hazel;
using UnityEngine;

public class DeconSystem : MonoBehaviour, ISystemType
{
	[Flags]
	public enum States : byte
	{
		Idle = 0,
		Enter = 1,
		Closed = 2,
		Exit = 4,
		HeadingUp = 8
	}

	private const byte HeadUpCmd = 1;

	private const byte HeadDownCmd = 2;

	private const byte HeadUpInsideCmd = 3;

	private const byte HeadDownInsideCmd = 4;

	public SomeKindaDoor UpperDoor;

	public SomeKindaDoor LowerDoor;

	public float DoorOpenTime = 3f;

	public float DeconTime = 3f;

	public AudioClip SpraySound;

	public ParticleSystem[] Particles;

	public SystemTypes TargetSystem = SystemTypes.Decontamination;

	private float timer;

	public Collider2D RoomArea;

	public DecontamNumController FloorText;

	private Coroutine sprayers;

	public States CurState { get; private set; }

	public bool IsDirty { get; private set; }

	public void Deteriorate(float dt)
	{
		if (sprayers == null && CurState.HasFlag(States.Closed))
		{
			sprayers = ((MonoBehaviour)this).StartCoroutine(CoRunSprayers());
		}
		int num = Mathf.CeilToInt(timer);
		timer = Mathf.Max(0f, timer - dt);
		int num2 = Mathf.CeilToInt(timer);
		if (num == num2)
		{
			return;
		}
		if (num2 == 0)
		{
			if (CurState.HasFlag(States.Enter))
			{
				CurState = (CurState & ~States.Enter) | States.Closed;
				timer = DeconTime;
			}
			else if (CurState.HasFlag(States.Closed))
			{
				CurState = (CurState & ~States.Closed) | States.Exit;
				timer = DoorOpenTime;
			}
			else if (CurState.HasFlag(States.Exit))
			{
				CurState = States.Idle;
			}
		}
		UpdateDoorsViaState();
		IsDirty = true;
	}

	private IEnumerator CoRunSprayers()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("DeconSpray", SpraySound, loop: false, SoundDynamics, SoundManager.Instance.SfxChannel);
		}
		Particles.ForEach((ParticleSystem p) =>
		{
			p.Play();
		});
		yield return Effects.Wait(DeconTime);
		sprayers = null;
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (sprayers == null || !Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			source.volume = 0f;
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		if (Object.op_Implicit((Object)(object)RoomArea) && RoomArea.OverlapPoint(truePosition))
		{
			float num = timer / DeconTime;
			if ((double)num > 0.5)
			{
				source.volume = 1f - (num - 0.5f) / 0.5f;
			}
			else
			{
				source.volume = 1f;
			}
			float num2 = source.volume * 0.075f;
			VibrationManager.Vibrate(num2, num2);
		}
		else
		{
			source.volume = 0f;
		}
	}

	public void OpenDoor(bool upper)
	{
		if (CurState == States.Idle)
		{
			ShipStatus.Instance.RpcUpdateSystem(TargetSystem, (byte)((!upper) ? 1 : 2));
		}
	}

	public void OpenFromInside(bool upper)
	{
		if (CurState == States.Idle)
		{
			ShipStatus.Instance.RpcUpdateSystem(TargetSystem, (byte)(upper ? 3 : 4));
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		if (CurState == States.Idle)
		{
			switch (msgReader.ReadByte())
			{
			case 1:
				CurState = States.Enter | States.HeadingUp;
				timer = DoorOpenTime;
				break;
			case 2:
				CurState = States.Enter;
				timer = DoorOpenTime;
				break;
			case 3:
				CurState = States.Exit | States.HeadingUp;
				timer = DoorOpenTime;
				break;
			case 4:
				CurState = States.Exit;
				timer = DoorOpenTime;
				break;
			}
			UpdateDoorsViaState();
			IsDirty = true;
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)Mathf.CeilToInt(timer));
		writer.Write((byte)CurState);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		timer = (int)reader.ReadByte();
		CurState = (States)reader.ReadByte();
		UpdateDoorsViaState();
	}

	private void UpdateDoorsViaState()
	{
		int num = Mathf.CeilToInt(timer);
		if (CurState.HasFlag(States.Enter))
		{
			bool flag = CurState.HasFlag(States.HeadingUp);
			LowerDoor.SetDoorway(flag);
			UpperDoor.SetDoorway(!flag);
			if (Object.op_Implicit((Object)(object)FloorText))
			{
				FloorText.SetSecond(num, DoorOpenTime);
			}
		}
		else if (CurState.HasFlag(States.Closed) || CurState == States.Idle)
		{
			LowerDoor.SetDoorway(open: false);
			UpperDoor.SetDoorway(open: false);
			if (Object.op_Implicit((Object)(object)FloorText))
			{
				FloorText.SetSecond(DeconTime - (float)num, DeconTime);
			}
		}
		else if (CurState.HasFlag(States.Exit))
		{
			bool flag2 = CurState.HasFlag(States.HeadingUp);
			LowerDoor.SetDoorway(!flag2);
			UpperDoor.SetDoorway(flag2);
			if (Object.op_Implicit((Object)(object)FloorText))
			{
				FloorText.SetSecond(num, DoorOpenTime);
			}
		}
		else
		{
			Debug.LogWarning((object)("What is this state: " + CurState));
		}
	}
}
