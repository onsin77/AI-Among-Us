using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class ElectricalDoors : MonoBehaviour, ISystemType
{
	[Serializable]
	public class DoorSet
	{
		public string Name;

		public StaticDoor[] Doors;

		public override string ToString()
		{
			return Name;
		}
	}

	public StaticDoor[] Doors;

	public DoorSet[] Rooms;

	public DoorSet LeftExits;

	public bool IsDirty { get; private set; }

	[ContextMenu("Shuffle Doors")]
	public void Initialize()
	{
		HashSet<DoorSet> hashSet = new HashSet<DoorSet>();
		StaticDoor[] doors = Doors;
		for (int i = 0; i < doors.Length; i++)
		{
			doors[i].SetOpen(isOpen: false);
		}
		DoorSet room = Rooms[0];
		int num = 0;
		while (hashSet.Count < Rooms.Length && num++ < 10000)
		{
			StaticDoor door = room.Doors.Random();
			DoorSet doorSet = Rooms.First((DoorSet r) => r != room && r.Doors.Contains(door));
			if (hashSet.Add(doorSet))
			{
				door.SetOpen(isOpen: true);
			}
			if (BoolRange.Next())
			{
				hashSet.Add(room);
				room = doorSet;
			}
		}
		bool flag = BoolRange.Next();
		LeftExits.Doors[0].SetOpen(flag);
		LeftExits.Doors[1].SetOpen(!flag);
		IsDirty = true;
	}

	public void Deteriorate(float deltaTime)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		uint num = 0u;
		for (int i = 0; i < Doors.Length; i++)
		{
			num |= (uint)((Doors[i].IsOpen ? 1 : 0) << i);
		}
		writer.Write(num);
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		uint num = reader.ReadUInt32();
		for (int i = 0; i < Doors.Length; i++)
		{
			Doors[i].SetOpen((num & (1 << i)) != 0);
		}
	}
}
