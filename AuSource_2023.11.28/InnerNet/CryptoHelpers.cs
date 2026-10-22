using System;
using System.Collections.Generic;

namespace InnerNet;

public static class CryptoHelpers
{
	public static byte[] DecodePEM(string pemData)
	{
		List<byte> list = new List<byte>();
		pemData = pemData.Replace("\r", "");
		string[] array = pemData.Split('\n');
		foreach (string text in array)
		{
			if (!text.StartsWith("-----"))
			{
				byte[] collection = Convert.FromBase64String(text);
				list.AddRange(collection);
			}
		}
		return list.ToArray();
	}
}
