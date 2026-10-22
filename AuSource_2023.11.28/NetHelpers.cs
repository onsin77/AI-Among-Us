using Hazel;
using UnityEngine;

public static class NetHelpers
{
	private static readonly FloatRange XRange = new FloatRange(-50f, 50f);

	private static readonly FloatRange YRange = new FloatRange(-50f, 50f);

	public static bool SidGreaterThan(ushort newSid, ushort prevSid)
	{
		ushort num = (ushort)(prevSid + 32767);
		if (prevSid < num)
		{
			if (newSid > prevSid)
			{
				return newSid <= num;
			}
			return false;
		}
		if (newSid <= prevSid)
		{
			return newSid <= num;
		}
		return true;
	}

	public static bool SidGreaterThan(byte newSid, byte prevSid)
	{
		byte b = (byte)(prevSid + 127);
		if (prevSid < b)
		{
			if (newSid > prevSid)
			{
				return newSid <= b;
			}
			return false;
		}
		if (newSid <= prevSid)
		{
			return newSid <= b;
		}
		return true;
	}

	public static void WriteVector2(Vector2 vec, MessageWriter writer)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ushort num = (ushort)(XRange.ReverseLerp(vec.x) * 65535f);
		ushort num2 = (ushort)(YRange.ReverseLerp(vec.y) * 65535f);
		writer.Write(num);
		writer.Write(num2);
	}

	public static Vector2 ReadVector2(MessageReader reader)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float v = (float)(int)reader.ReadUInt16() / 65535f;
		float v2 = (float)(int)reader.ReadUInt16() / 65535f;
		return new Vector2(XRange.Lerp(v), YRange.Lerp(v2));
	}
}
