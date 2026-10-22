using Hazel;
using UnityEngine;

namespace InnerNet;

public static class MessageExtensions
{
	public static void WriteNetObject(this MessageWriter self, InnerNetObject obj)
	{
		if (!Object.op_Implicit((Object)(object)obj))
		{
			self.Write(0);
		}
		else
		{
			self.WritePacked(obj.NetId);
		}
	}

	public static T ReadNetObject<T>(this MessageReader self) where T : InnerNetObject
	{
		uint netId = self.ReadPackedUInt32();
		return AmongUsClient.Instance.FindObjectByNetId<T>(netId);
	}
}
