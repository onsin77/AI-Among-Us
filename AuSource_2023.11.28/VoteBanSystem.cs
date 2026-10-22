using System;
using System.Collections.Generic;
using Hazel;
using InnerNet;

public class VoteBanSystem : InnerNetObject
{
	public static VoteBanSystem Instance;

	public Dictionary<int, int[]> Votes = new Dictionary<int, int[]>();

	public void Awake()
	{
		Instance = this;
	}

	public void CmdAddVote(int clientId)
	{
		AddVote(AmongUsClient.Instance.ClientId, clientId);
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 26, (SendOption)1);
		val.Write(AmongUsClient.Instance.ClientId);
		val.Write(clientId);
		val.EndMessage();
	}

	private void AddVote(int srcClient, int clientId)
	{
		if (!Votes.TryGetValue(clientId, out var value))
		{
			int[] array = (Votes[clientId] = new int[3]);
			value = array;
		}
		int num = -1;
		for (int i = 0; i < value.Length; i++)
		{
			int num2 = value[i];
			if (num2 == srcClient)
			{
				break;
			}
			if (num2 == 0)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			value[num] = srcClient;
			SetDirtyBit(1u);
			if (num == value.Length - 1)
			{
				AmongUsClient.Instance.KickPlayer(clientId, ban: false);
			}
		}
	}

	public bool HasMyVote(int clientId)
	{
		if (Votes.TryGetValue(clientId, out var value))
		{
			return Array.IndexOf(value, AmongUsClient.Instance.ClientId) != -1;
		}
		return false;
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId == 26)
		{
			int srcClient = reader.ReadInt32();
			int clientId = reader.ReadInt32();
			AddVote(srcClient, clientId);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)Votes.Count);
		foreach (KeyValuePair<int, int[]> vote in Votes)
		{
			writer.Write(vote.Key);
			for (int i = 0; i < 3; i++)
			{
				writer.WritePacked(vote.Value[i]);
			}
		}
		ClearDirtyBits();
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		int num = reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			int key = reader.ReadInt32();
			if (!Votes.TryGetValue(key, out var value))
			{
				int[] array = (Votes[key] = new int[3]);
				value = array;
			}
			for (int j = 0; j < 3; j++)
			{
				value[j] = reader.ReadPackedInt32();
			}
		}
	}
}
