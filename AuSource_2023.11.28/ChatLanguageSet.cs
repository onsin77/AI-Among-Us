using System.Collections.Generic;
using System.IO;

public class ChatLanguageSet
{
	public static readonly ChatLanguageSet Instance = new ChatLanguageSet();

	public readonly Dictionary<string, uint> Languages = new Dictionary<string, uint>
	{
		{ "English", 256u },
		{ "Español (Latinoamérica)", 2u },
		{ "Português (Brasil)", 2048u },
		{ "Português", 16u },
		{ "한국어", 4u },
		{ "Pусский", 8u },
		{ "Nederlands", 4096u },
		{ "Bisaya", 64u },
		{ "Français", 8192u },
		{ "Deutsch", 16384u },
		{ "Italiano", 32768u },
		{ "日本語", 512u },
		{ "Español", 1024u },
		{ "Al Arabiya", 32u },
		{ "Polskie", 128u },
		{ "简体中文", 65536u },
		{ "繁體中文", 131072u },
		{ "Gaeilge", 262144u },
		{ "Other", 1u }
	};

	public void Load()
	{
		string path = Path.Combine(PlatformPaths.persistentDataPath, "languageFilter");
		try
		{
			if (!File.Exists(path))
			{
				return;
			}
			using StreamReader streamReader = new StreamReader(path);
			while (!streamReader.EndOfStream)
			{
				string[] array = streamReader.ReadLine().Split(',');
				try
				{
					Languages[array[0]] = uint.Parse(array[1]);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	public void Save()
	{
		try
		{
			string text = Path.Combine(PlatformPaths.persistentDataPath, "languageFilterTemp");
			using (StreamWriter streamWriter = new StreamWriter(text, append: false))
			{
				foreach (KeyValuePair<string, uint> language in Languages)
				{
					streamWriter.Write(language.Key);
					streamWriter.Write(",");
					streamWriter.WriteLine(language.Value);
				}
			}
			string text2 = Path.Combine(PlatformPaths.persistentDataPath, "languageFilter");
			File.Delete(text2);
			File.Move(text, text2);
		}
		catch
		{
		}
	}

	public string GetString(uint flag)
	{
		foreach (KeyValuePair<string, uint> language in Languages)
		{
			if (language.Value == flag)
			{
				return language.Key;
			}
		}
		return "???";
	}
}
