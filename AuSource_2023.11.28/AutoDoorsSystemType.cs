using System.Linq;
using Hazel;

public class AutoDoorsSystemType : ISystemType, IActivatable, RunTimer, IDoorSystem
{
	private uint dirtyBits;

	private float initialCooldown;

	public bool IsActive => ShipStatus.Instance.AllDoors.Any((OpenableDoor b) => !b.IsOpen);

	public bool IsDirty => dirtyBits != 0;

	public void Deteriorate(float deltaTime)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			if (ShipStatus.Instance.AllDoors[i].DoUpdate(deltaTime))
			{
				dirtyBits |= (uint)(1 << i);
			}
		}
		if (initialCooldown > 0f)
		{
			initialCooldown -= deltaTime;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
				ShipStatus.Instance.AllDoors[i].Serialize(writer);
			}
			return;
		}
		writer.WritePacked(dirtyBits);
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((dirtyBits & (uint)(1 << j)) != 0)
			{
				ShipStatus.Instance.AllDoors[j].Serialize(writer);
			}
		}
		dirtyBits = 0u;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
				ShipStatus.Instance.AllDoors[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((num & (uint)(1 << j)) != 0)
			{
				ShipStatus.Instance.AllDoors[j].Deserialize(reader);
			}
		}
	}

	public void SetDoor(AutoOpenDoor door, bool open)
	{
		door.SetDoorway(open);
		dirtyBits |= (uint)(1 << ShipStatus.Instance.AllDoors.IndexOf(door));
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors[i];
			if (openableDoor.Room == room)
			{
				openableDoor.SetDoorway(open: false);
				dirtyBits |= (uint)(1 << i);
			}
		}
	}

	public float GetTimer(SystemTypes room)
	{
		if (initialCooldown > 0f)
		{
			return initialCooldown / 10f;
		}
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors[i];
			if (openableDoor.Room == room)
			{
				return ((AutoOpenDoor)openableDoor).CooldownTimer / 30f;
			}
		}
		return 0f;
	}

	public void SetInitialSabotageCooldown()
	{
		initialCooldown = 10f;
	}
}
