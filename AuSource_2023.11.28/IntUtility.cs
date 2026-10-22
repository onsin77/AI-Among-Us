using System;

public static class IntUtility
{
	public static byte[] IntToByteArray(int value)
	{
		return new byte[4]
		{
			(byte)(value & 0xFF),
			(byte)((value >> 8) & 0xFF),
			(byte)((value >> 16) & 0xFF),
			(byte)((value >> 24) & 0xFF)
		};
	}

	public static int ByteArrayToInt(byte[] data)
	{
		return data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
	}

	public static ulong ConvertHexToUint64(string hexStr)
	{
		ulong num = 1uL;
		ulong num2 = 0uL;
		for (int num3 = hexStr.Length - 1; num3 >= 0; num3--)
		{
			ulong num4 = 0uL;
			switch (hexStr[num3])
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				num4 = (ulong)(hexStr[num3] - 48);
				break;
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
				num4 = (ulong)(hexStr[num3] - 97) + 10uL;
				break;
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
				num4 = (ulong)(hexStr[num3] - 65) + 10uL;
				break;
			default:
				throw new Exception($"Error converting \"{hexStr}\" to decimal: Invalid digit '{hexStr[num3]}'");
			}
			num2 += num4 * num;
			num *= 16;
		}
		return num2;
	}
}
