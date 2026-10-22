using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class DoorsSystemType : ISystemType, IActivatable, RunTimer, IDoorSystem
{
	public const byte CloseDoors = 128;

	public const byte OpenDoor = 64;

	private const byte ActionMask = 192;

	private const byte IdMask = 31;

	private readonly Dictionary<SystemTypes, float> timers = new Dictionary<SystemTypes, float>();

	private float initialCooldown;

	public bool IsActive => false;

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		for (int i = 0; i < SystemTypeHelpers.AllTypes.Length; i++)
		{
			SystemTypes key = SystemTypeHelpers.AllTypes[i];
			if (timers.TryGetValue(key, out var value))
			{
				timers[key] = Mathf.Clamp(value - deltaTime, 0f, 30f);
			}
		}
		if (initialCooldown > 0f)
		{
			initialCooldown -= deltaTime;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		int id = b & 0x1F;
		switch (b & 0xC0)
		{
		case 64:
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors.First((OpenableDoor d) => d.Id == id);
			if ((Object)(object)openableDoor == (Object)null)
			{
				Debug.LogWarning((object)$"Couldn't find door {id}");
			}
			else
			{
				openableDoor.SetDoorway(open: true);
			}
			break;
		}
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)timers.Count);
		foreach (KeyValuePair<SystemTypes, float> timer in timers)
		{
			writer.Write((byte)timer.Key);
			writer.Write(timer.Value);
		}
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			ShipStatus.Instance.AllDoors[i].Serialize(writer);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			SystemTypes key = (SystemTypes)reader.ReadByte();
			float value = reader.ReadSingle();
			timers[key] = value;
		}
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			ShipStatus.Instance.AllDoors[j].Deserialize(reader);
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		timers[room] = 30f;
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors[i];
			if (openableDoor.Room == room)
			{
				openableDoor.SetDoorway(open: false);
			}
		}
		IsDirty = true;
	}

	public float GetTimer(SystemTypes room)
	{
		if (initialCooldown > 0f)
		{
			return initialCooldown / 10f;
		}
		if (timers.TryGetValue(room, out var value))
		{
			return value / 30f;
		}
		return 0f;
	}

	public void SetInitialSabotageCooldown()
	{
		initialCooldown = 10f;
	}
}
