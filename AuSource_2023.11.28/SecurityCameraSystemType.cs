using System.Collections.Generic;
using Hazel;
using UnityEngine;

public class SecurityCameraSystemType : ISystemType
{
	public const byte IncrementOp = 1;

	public const byte DecrementOp = 2;

	private HashSet<byte> PlayersUsing = new HashSet<byte>();

	private HashSet<byte> ToRemove = new HashSet<byte>();

	public bool InUse => PlayersUsing.Count > 0;

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		foreach (byte item in PlayersUsing)
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(item);
			if (playerById == null || playerById.Disconnected)
			{
				ToRemove.Add(item);
			}
		}
		if (ToRemove.Count > 0)
		{
			PlayersUsing.ExceptWith(ToRemove);
			ToRemove.Clear();
			UpdateCameras();
			IsDirty = true;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		if (msgReader.ReadByte() == 1)
		{
			PlayersUsing.Add(player.PlayerId);
		}
		else
		{
			PlayersUsing.Remove(player.PlayerId);
		}
		IsDirty = true;
		UpdateCameras();
	}

	private void UpdateCameras()
	{
		for (int i = 0; i < ShipStatus.Instance.AllCameras.Length; i++)
		{
			ShipStatus.Instance.AllCameras[i].SetAnimation(InUse);
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(PlayersUsing.Count);
		foreach (byte item in PlayersUsing)
		{
			writer.Write(item);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		PlayersUsing.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			PlayersUsing.Add(reader.ReadByte());
		}
		UpdateCameras();
	}
}
