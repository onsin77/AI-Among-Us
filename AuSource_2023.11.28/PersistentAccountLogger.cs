using System;
using System.Globalization;
using System.IO;
using Innersloth.IO;
using UnityEngine;

public class PersistentAccountLogger : DestroyableSingleton<PersistentAccountLogger>
{
	private StreamWriter sw;

	private void Start()
	{
		string path = Path.Combine(PlatformPaths.persistentDataPath, "persLog.log");
		try
		{
			if (!FileIO.Exists(path))
			{
				sw = File.CreateText(path);
			}
			else
			{
				sw = File.AppendText(path);
			}
			sw.WriteLine("----------------------------------------------------------");
			AddStringToLog("App Start");
		}
		catch (Exception arg)
		{
			Debug.Log((object)$"Could not create persistent log file, error = {arg}");
		}
	}

	public void AddStringToLog(string str)
	{
		if (sw != null)
		{
			string text = DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss", CultureInfo.InvariantCulture);
			sw.WriteLine(text + ": " + str);
		}
	}

	private void OnApplicationQuit()
	{
		sw.Close();
	}
}
