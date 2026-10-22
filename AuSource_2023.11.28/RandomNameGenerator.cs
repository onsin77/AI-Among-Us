using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RandomNameGenerator : MonoBehaviour
{
	public TextAsset PatternConfig;

	private List<Pattern> Patterns = new List<Pattern>();

	private Dictionary<string, WordGroup> WordGroups = new Dictionary<string, WordGroup>(StringComparer.OrdinalIgnoreCase);

	private static Random randy = new Random();

	private IEnumerable<Pattern> UsablePatterns;

	private void Awake()
	{
		Parse(new StringReader(PatternConfig.text));
		UsablePatterns = Patterns;
	}

	public string GetName()
	{
		if (UsablePatterns.Count() == 0)
		{
			Reset();
		}
		Pattern pattern = UsablePatterns.Random();
		UsablePatterns = UsablePatterns.Where((Pattern p) => p != pattern);
		return pattern.FollowPattern(randy, WordGroups);
	}

	public bool ValidateName(string inName)
	{
		UsablePatterns = Patterns;
		foreach (Pattern usablePattern in UsablePatterns)
		{
			if (usablePattern.CheckPattern(inName, WordGroups))
			{
				return true;
			}
		}
		return false;
	}

	public void Reset()
	{
		UsablePatterns = Patterns;
	}

	public void Parse(TextReader reader)
	{
		string text;
		while ((text = reader.ReadLine()) != null)
		{
			if (!text.StartsWith("["))
			{
				continue;
			}
			string[] array = text.Split(new char[3] { ' ', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2 && array.Any((string w) => w.Equals("words", StringComparison.OrdinalIgnoreCase)))
			{
				string key = array[0];
				WordGroup value = WordGroup.Parse(reader);
				WordGroups.Add(key, value);
			}
			else if (array.Length == 1 && array[0].Equals("patterns", StringComparison.OrdinalIgnoreCase))
			{
				while (!(text = reader.ReadLine()).IsNullOrWhiteSpace())
				{
					Patterns.Add(Pattern.Parse(text));
				}
			}
		}
	}
}
