using System;
using System.IO;
using Innersloth.IO;
using UnityEngine;

namespace AmongUs.Data.Legacy;

public class LegacySecureDataFile
{
	private string filePath;

	public bool Loaded { get; private set; }

	public LegacySecureDataFile(string filePath)
	{
		this.filePath = filePath;
	}

	public void LoadData(Action<BinaryReader> performRead)
	{
		Loaded = true;
		Debug.Log((object)("Loading secure: " + filePath));
		if (!FileIO.Exists(filePath))
		{
			return;
		}
		byte[] array;
		try
		{
			array = FileIO.ReadAllBytes(filePath);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] ^= (byte)(i % 212);
			}
		}
		catch
		{
			Debug.Log((object)"Couldn't read secure file");
			Delete();
			return;
		}
		try
		{
			using MemoryStream input = new MemoryStream(array);
			using BinaryReader binaryReader = new BinaryReader(input);
			if (binaryReader.ReadString() != SystemInfo.deviceUniqueIdentifier)
			{
				Debug.Log((object)"Invalid secure file");
				Delete();
			}
			else
			{
				performRead(binaryReader);
			}
		}
		catch
		{
			Debug.Log((object)"Deleted corrupt secure file inner");
			Delete();
		}
	}

	public void Delete()
	{
		try
		{
			FileIO.Delete(filePath);
		}
		catch
		{
		}
	}
}
