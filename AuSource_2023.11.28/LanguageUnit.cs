using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LanguageUnit
{
	private static StringBuilder builder = new StringBuilder(512);

	public bool IsEnglish;

	public SupportedLangs languageID;

	private Dictionary<string, string> AllStrings = new Dictionary<string, string>();

	private Dictionary<ImageNames, Sprite> AllImages = new Dictionary<ImageNames, Sprite>();

	public LanguageUnit(TranslatedImageSet locSet)
	{
		languageID = locSet.languageID;
		ParseTSV(locSet.Data.text, AllStrings);
		Array values = Enum.GetValues(typeof(StringNames));
		List<StringNames> list = new List<StringNames>();
		foreach (StringNames item in values)
		{
			if (!AllStrings.ContainsKey(item.ToString()) && locSet.languageID == SupportedLangs.English)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			string arg = string.Join("\n", list);
			Debug.LogWarning((object)$"No translation found in {SupportedLangs.English} for the following StringNames:\n{arg}");
		}
	}

	public static bool TryParseTsvLine(string line, out string key, out string text)
	{
		key = null;
		text = null;
		if (line.Length == 0)
		{
			return false;
		}
		int num = line.IndexOf('\t');
		if (num < 0)
		{
			Debug.LogWarning((object)("Couldn't parse: " + line + " because there was no tab"));
			return false;
		}
		key = line.Substring(0, num);
		text = UnescapeCodes(line, num + 1);
		if (key.Length == 0 && text.Length == 0)
		{
			return false;
		}
		return true;
	}

	public static void ParseTSV(string tsvText, Dictionary<string, string> allStrings)
	{
		using StringReader stringReader = new StringReader(tsvText);
		for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
		{
			if (!TryParseTsvLine(text, out var key, out var text2))
			{
				continue;
			}
			string text3;
			if (key.StartsWith("Cosmetic.") || key.StartsWith("Bundle.") || key.StartsWith("Bundle."))
			{
				text3 = key;
			}
			else
			{
				if (!Enum.TryParse<StringNames>(key, out var result) && !string.IsNullOrEmpty(key))
				{
					continue;
				}
				text3 = result.ToString();
			}
			if (allStrings.ContainsKey(text3))
			{
				if (text2 != allStrings[text3])
				{
					Debug.LogWarning((object)("Duplicate translation for " + text3 + ": '" + text2 + "' and '" + allStrings[text3] + "'"));
				}
			}
			else
			{
				allStrings.Add(text3, text2);
			}
		}
	}

	public static string UnescapeCodes(string src, int startAt)
	{
		builder.Clear();
		for (int i = startAt; i < src.Length; i++)
		{
			char c = src[i];
			if (c == '\\')
			{
				switch (src[++i])
				{
				case 'n':
					builder.Append('\n');
					break;
				case 't':
					builder.Append('\t');
					break;
				}
			}
			else
			{
				builder.Append(c);
			}
		}
		return builder.ToString();
	}

	public string GetString(string idString, params object[] parts)
	{
		if (AllStrings.TryGetValue(idString, out var value) && !value.IsNullOrWhiteSpace())
		{
			if (parts.Length != 0)
			{
				return string.Format(value, parts);
			}
			return value;
		}
		return string.Empty;
	}

	public Sprite GetImage(ImageNames id)
	{
		AllImages.TryGetValue(id, out var value);
		return value;
	}
}
