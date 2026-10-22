using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Pattern
{
	private string pattern;

	public string FollowPattern(Random randy, Dictionary<string, WordGroup> words)
	{
		bool flag = false;
		bool flag2 = true;
		StringBuilder stringBuilder = new StringBuilder();
		string text = pattern;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '^')
			{
				flag = true;
			}
			else if (char.IsUpper(c))
			{
				if (words.TryGetValue(c.ToString(), out var value))
				{
					string text2 = value[randy.Next(value.Count)];
					if (flag2)
					{
						flag2 = flag;
						stringBuilder.AppendFormat("{0}{1}", text2.Substring(0, 1).ToUpperInvariant(), text2.Substring(1).ToLowerInvariant());
					}
					else
					{
						stringBuilder.Append(text2);
					}
				}
			}
			else
			{
				if (flag2)
				{
					flag2 = flag;
					stringBuilder.Append(char.ToUpperInvariant(c));
				}
				else
				{
					stringBuilder.Append(c);
				}
				if (char.IsWhiteSpace(c))
				{
					flag2 = true;
				}
			}
		}
		return stringBuilder.Replace(" Of", " of").Replace(" The", " the").ToString();
	}

	public static Pattern Parse(string line)
	{
		return new Pattern
		{
			pattern = line
		};
	}

	public bool CheckPattern(string nameToCheck, Dictionary<string, WordGroup> words)
	{
		bool flag = false;
		bool flag2 = true;
		new StringBuilder();
		nameToCheck = nameToCheck.ToLower();
		string text = pattern;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '^')
			{
				flag = true;
			}
			else if (char.IsUpper(c))
			{
				if (!words.TryGetValue(c.ToString(), out var value))
				{
					continue;
				}
				if (flag2)
				{
					flag2 = flag;
					string text2 = value.FirstOrDefault((string x) => nameToCheck.StartsWith(x.ToLower()));
					if (!string.IsNullOrEmpty(text2))
					{
						nameToCheck = nameToCheck.Replace(text2.ToLower(), "");
					}
				}
				else
				{
					string text3 = value.FirstOrDefault((string x) => nameToCheck.Equals(x.ToLower()));
					if (!string.IsNullOrEmpty(text3))
					{
						nameToCheck = nameToCheck.Replace(text3.ToLower(), "");
					}
				}
			}
			else
			{
				if (flag2)
				{
					flag2 = flag;
					nameToCheck = nameToCheck.Replace(c.ToString().ToLower(), "");
				}
				else
				{
					nameToCheck = nameToCheck.Replace(c.ToString().ToLower(), "");
				}
				if (char.IsWhiteSpace(c))
				{
					flag2 = true;
				}
			}
		}
		nameToCheck = nameToCheck.Replace(" of", "").Replace(" the", "").Trim()
			.ToString();
		if (string.IsNullOrEmpty(nameToCheck))
		{
			return true;
		}
		return false;
	}
}
