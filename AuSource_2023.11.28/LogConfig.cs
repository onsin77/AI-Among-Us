using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class LogConfig
{
	public const string ConfigFilePath = "Library/LogConfig.txt";

	public static bool FileExists => File.Exists("Library/LogConfig.txt");

	public static Dictionary<string, Logger.Level> GetOverrides()
	{
		Dictionary<string, Logger.Level> dictionary = new Dictionary<string, Logger.Level>();
		if (!FileExists)
		{
			return dictionary;
		}
		string[] array = File.ReadAllLines("Library/LogConfig.txt");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			string text = ((array2.Length != 0) ? array2[0] : null);
			string value = ((array2.Length > 1) ? array2[1] : null);
			if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(value) && Enum.TryParse<Logger.Level>(value, out var result))
			{
				dictionary[text] = result;
			}
		}
		return dictionary;
	}

	public static void WriteOverridesToFile(Dictionary<string, Logger.Level> newOverrides)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, Logger.Level> newOverride in newOverrides)
		{
			stringBuilder.AppendLine($"{newOverride.Key},{newOverride.Value}");
		}
		File.WriteAllText("Library/LogConfig.txt", stringBuilder.ToString());
	}
}
