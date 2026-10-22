using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FontLoader
{
	public static FontData FromBinary(TextAsset dataSrc, FontExtensionData eData)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		FontData fontData = new FontData();
		using (MemoryStream memoryStream = new MemoryStream(dataSrc.bytes))
		{
			using BinaryReader binaryReader = new BinaryReader(memoryStream);
			if (memoryStream.ReadByte() != 66 || memoryStream.ReadByte() != 77 || memoryStream.ReadByte() != 70 || memoryStream.ReadByte() != 3)
			{
				throw new InvalidDataException("Wrong format font.");
			}
			long num = -1L;
			while (memoryStream.Position < memoryStream.Length)
			{
				if (num == memoryStream.Position)
				{
					throw new InvalidDataException("Bad font.");
				}
				num = memoryStream.Position;
				byte b = binaryReader.ReadByte();
				int num2 = binaryReader.ReadInt32();
				long position = memoryStream.Position;
				switch (b)
				{
				default:
					memoryStream.Position += num2;
					continue;
				case 2:
					fontData.LineHeight = (int)binaryReader.ReadUInt16();
					memoryStream.Position += 2L;
					fontData.TextureSize = new Vector2((float)(int)binaryReader.ReadUInt16(), (float)(int)binaryReader.ReadUInt16());
					memoryStream.Position = position + num2;
					continue;
				case 4:
				{
					int num3 = num2 / 20;
					fontData.charMap = new Dictionary<int, int>(num3);
					fontData.bounds.Capacity = num3;
					fontData.offsets.Capacity = num3;
					fontData.Channels.Capacity = num3;
					fontData.kernings = new Dictionary<int, Dictionary<int, float>>(256);
					for (int i = 0; i < num3; i++)
					{
						int key = binaryReader.ReadInt32();
						int num4 = binaryReader.ReadUInt16();
						int num5 = binaryReader.ReadUInt16();
						int num6 = binaryReader.ReadUInt16();
						int num7 = binaryReader.ReadUInt16();
						int num8 = binaryReader.ReadInt16();
						int num9 = binaryReader.ReadInt16();
						int num10 = binaryReader.ReadInt16();
						binaryReader.ReadByte();
						int input = binaryReader.ReadByte();
						fontData.charMap.Add(key, fontData.bounds.Count);
						fontData.bounds.Add(new Vector4((float)num4, (float)num5, (float)num6, (float)num7));
						fontData.offsets.Add(new Vector3((float)num8, (float)num9, (float)num10));
						fontData.Channels.Add(IntToChannels(input));
					}
					continue;
				}
				case 5:
					break;
				}
				while (memoryStream.Position < position + num2)
				{
					int key2 = binaryReader.ReadInt32();
					int key3 = binaryReader.ReadInt32();
					int num11 = binaryReader.ReadInt16();
					if (!fontData.kernings.TryGetValue(key2, out var value))
					{
						fontData.kernings.Add(key2, value = new Dictionary<int, float>(256));
					}
					value.Add(key3, num11);
				}
			}
		}
		if ((Object)(object)eData != (Object)null)
		{
			eData.AdjustKernings(fontData);
			eData.AdjustOffsets(fontData);
		}
		return fontData;
	}

	private static Vector4 IntToChannels(int input)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector4 result = default;
		for (int i = 0; i < 4; i++)
		{
			if (((input >> i) & 1) == 1)
			{
				result[i] = 1f;
			}
		}
		return result;
	}
}
