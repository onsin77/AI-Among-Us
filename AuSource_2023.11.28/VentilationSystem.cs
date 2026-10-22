using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class VentilationSystem : ISystemType
{
	public struct VentMoveInfo
	{
		public Operation Op;

		public byte VentId;

		public byte PlayerId;
	}

	public enum Operation
	{
		StartCleaning,
		StopCleaning,
		Enter,
		Exit,
		Move,
		BootImpostors
	}

	private const ushort OP_ID_START = 0;

	private readonly Dictionary<byte, SequenceBuffer<VentMoveInfo>> SeqBuffers = new Dictionary<byte, SequenceBuffer<VentMoveInfo>>();

	private readonly Dictionary<byte, byte> PlayersCleaningVents = new Dictionary<byte, byte>();

	private readonly Dictionary<byte, byte> PlayersInsideVents = new Dictionary<byte, byte>();

	private readonly HashSet<byte> ToRemove = new HashSet<byte>();

	public bool IsDirty { get; private set; }

	public static void Update(Operation op, int ventId)
	{
		if (ShipStatus.Instance.Systems[SystemTypes.Ventilation] is VentilationSystem ventilationSystem)
		{
			byte playerId = PlayerControl.LocalPlayer.PlayerId;
			SequenceBuffer<VentMoveInfo> valueOrSetDefault = ventilationSystem.SeqBuffers.GetValueOrSetDefault(playerId, () => new SequenceBuffer<VentMoveInfo>(0));
			ushort num = (ushort)(valueOrSetDefault.LastSid + 1);
			MessageWriter val = MessageWriter.Get((SendOption)1);
			val.Write(num);
			val.Write((byte)op);
			val.Write((byte)ventId);
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Ventilation, val);
			val.Recycle();
			valueOrSetDefault.LastSid = num;
		}
	}

	public void Deteriorate(float deltaTime)
	{
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		foreach (KeyValuePair<byte, byte> playersCleaningVent in PlayersCleaningVents)
		{
			byte key = playersCleaningVent.Key;
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(key);
			if (playerById == null || playerById.Disconnected)
			{
				ToRemove.Add(key);
			}
		}
		if (ToRemove.Count <= 0)
		{
			return;
		}
		foreach (byte item in ToRemove)
		{
			PlayersCleaningVents.Remove(item);
			PlayersInsideVents.Remove(item);
		}
		ToRemove.Clear();
		UpdateVentArrows();
		IsDirty = true;
	}

	public bool IsVentCurrentlyBeingCleaned(int id)
	{
		foreach (KeyValuePair<byte, byte> playersCleaningVent in PlayersCleaningVents)
		{
			if (playersCleaningVent.Value == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsImpostorInsideVent(int ventId)
	{
		foreach (KeyValuePair<byte, byte> playersInsideVent in PlayersInsideVents)
		{
			if (playersInsideVent.Value == ventId)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		ushort opId = msgReader.ReadUInt16();
		Operation operation = (Operation)msgReader.ReadByte();
		byte b = msgReader.ReadByte();
		byte playerId = player.PlayerId;
		SequenceBuffer<VentMoveInfo> valueOrSetDefault = SeqBuffers.GetValueOrSetDefault(playerId, () => new SequenceBuffer<VentMoveInfo>((ushort)(opId - 1)));
		if (valueOrSetDefault.IsInvalidSid(opId))
		{
			Debug.LogError((object)$"Invalid Sid {opId} received; Op {operation} on vent {b} by player {playerId}. Last Sid was: {valueOrSetDefault.LastSid}");
			return;
		}
		if (valueOrSetDefault.IsNextSid(opId))
		{
			PerformVentOp(playerId, operation, b, valueOrSetDefault);
		}
		else
		{
			valueOrSetDefault.Add(opId, new VentMoveInfo
			{
				Op = operation,
				VentId = b,
				PlayerId = playerId
			});
		}
		foreach (VentMoveInfo item in valueOrSetDefault.SubsequentObjs())
		{
			PerformVentOp(item.PlayerId, item.Op, item.VentId, valueOrSetDefault);
		}
	}

	private void PerformVentOp(byte playerId, Operation op, byte ventId, SequenceBuffer<VentMoveInfo> seqBuffer)
	{
		seqBuffer.BumpSid();
		switch (op)
		{
		case Operation.StartCleaning:
			BootImpostorsFromVent(ventId);
			PlayersCleaningVents[playerId] = ventId;
			break;
		case Operation.StopCleaning:
		{
			KeyValuePair<byte, byte>[] array = PlayersCleaningVents.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<byte, byte> keyValuePair = array[i];
				if (keyValuePair.Value == ventId)
				{
					PlayersCleaningVents.Remove(keyValuePair.Key);
				}
			}
			break;
		}
		case Operation.Enter:
			PlayersInsideVents[playerId] = ventId;
			break;
		case Operation.Exit:
			PlayersInsideVents.Remove(playerId);
			break;
		case Operation.Move:
			PlayersInsideVents[playerId] = ventId;
			break;
		case Operation.BootImpostors:
			BootImpostorsFromVent(ventId);
			break;
		}
		IsDirty = true;
		UpdateVentArrows();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(PlayersCleaningVents.Count);
		foreach (KeyValuePair<byte, byte> playersCleaningVent in PlayersCleaningVents)
		{
			writer.Write(playersCleaningVent.Key);
			writer.Write(playersCleaningVent.Value);
		}
		writer.WritePacked(PlayersInsideVents.Count);
		foreach (KeyValuePair<byte, byte> playersInsideVent in PlayersInsideVents)
		{
			writer.Write(playersInsideVent.Key);
			writer.Write(playersInsideVent.Value);
		}
		IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		PlayersCleaningVents.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			byte key = reader.ReadByte();
			byte value = reader.ReadByte();
			PlayersCleaningVents[key] = value;
		}
		PlayersInsideVents.Clear();
		int num2 = reader.ReadPackedInt32();
		for (int j = 0; j < num2; j++)
		{
			byte key2 = reader.ReadByte();
			byte value2 = reader.ReadByte();
			PlayersInsideVents[key2] = value2;
		}
		UpdateVentArrows();
	}

	private void BootImpostorsFromVent(int ventId)
	{
		KeyValuePair<byte, byte>[] array = PlayersInsideVents.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<byte, byte> keyValuePair = array[i];
			if (keyValuePair.Value == ventId)
			{
				byte key = keyValuePair.Key;
				PlayersInsideVents.Remove(key);
				BootImpostorFromVent(ventId, key);
				IsDirty = true;
			}
		}
		if (IsDirty)
		{
			UpdateVentArrows();
		}
	}

	private void BootImpostorFromVent(int ventId, byte playerId)
	{
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (allPlayerControl.PlayerId == playerId)
			{
				allPlayerControl.MyPhysics.RpcBootFromVent(ventId);
				break;
			}
		}
	}

	private void UpdateVentArrows()
	{
		Vent[] allVents = ShipStatus.Instance.AllVents;
		for (int i = 0; i < allVents.Length; i++)
		{
			allVents[i].UpdateArrows(this);
		}
	}
}
