using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class MedScanSystem : ISystemType
{
	public const byte Request = 128;

	public const byte Release = 64;

	public const byte NumMask = 31;

	public const byte NoPlayer = byte.MaxValue;

	public List<byte> UsersList = new List<byte>();

	public byte CurrentUser { get; private set; } = byte.MaxValue;

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		if (UsersList.Count > 0)
		{
			if (CurrentUser != UsersList[0])
			{
				if (CurrentUser != byte.MaxValue)
				{
					Debug.Log((object)("Released scanner from: " + CurrentUser));
				}
				CurrentUser = UsersList[0];
				Debug.Log((object)("Acquired scanner for: " + CurrentUser));
				IsDirty = true;
			}
		}
		else if (CurrentUser != byte.MaxValue)
		{
			Debug.Log((object)("Released scanner from: " + CurrentUser));
			CurrentUser = byte.MaxValue;
			IsDirty = true;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		byte playerId = (byte)(b & 0x1F);
		if ((b & 0x80) != 0)
		{
			if (!UsersList.Contains(playerId))
			{
				Debug.Log((object)("Added to queue: " + playerId));
				UsersList.Add(playerId);
			}
		}
		else if ((b & 0x40) != 0)
		{
			Debug.Log((object)("Removed from queue: " + playerId));
			UsersList.RemoveAll((byte v) => v == playerId);
		}
		IsDirty = true;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(UsersList.Count);
		for (int i = 0; i < UsersList.Count; i++)
		{
			writer.Write(UsersList[i]);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		UsersList.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			UsersList.Add(reader.ReadByte());
		}
	}
}
