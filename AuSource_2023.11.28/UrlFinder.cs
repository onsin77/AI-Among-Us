public static class UrlFinder
{
	private static readonly string[] Prefixes = new string[2] { "http", "ftp" };

	private static readonly string[] Suffixes = new string[11]
	{
		".com", ".net", ".org", ".info", ".uk", ".jp", ".io", ".gg", ".co", ".ru",
		".ca"
	};

	public static bool TryFindUrl(char[] sourceText, out int startCensor, out int endCensor)
	{
		if (CheckPrefixes(sourceText, out startCensor, out endCensor))
		{
			return true;
		}
		if (CheckSuffixes(sourceText, 0, out startCensor, out endCensor))
		{
			return true;
		}
		return false;
	}

	private static bool CheckPrefixes(char[] sourceText, out int startCensor, out int endCensor)
	{
		for (int i = 0; i < Prefixes.Length; i++)
		{
			string word = Prefixes[i];
			startCensor = FindWord(sourceText, word);
			if (startCensor >= 0)
			{
				endCensor = FindWordEnd(sourceText, startCensor);
				return true;
			}
		}
		startCensor = (endCensor = 0);
		return false;
	}

	private static bool CheckSuffixes(char[] sourceText, int startAt, out int startCensor, out int endCensor)
	{
		string[] suffixes = Suffixes;
		foreach (string text in suffixes)
		{
			int num = FindWord(sourceText, text, startAt);
			if (num >= 0)
			{
				endCensor = num + text.Length;
				if (endCensor >= sourceText.Length || !char.IsLetter(sourceText[endCensor]))
				{
					startCensor = FindWordStart(sourceText, num);
					return true;
				}
				if (CheckSuffixes(sourceText, endCensor, out startCensor, out endCensor))
				{
					return true;
				}
			}
		}
		startCensor = (endCensor = 0);
		return false;
	}

	private static int FindWord(char[] sourceText, string word, int startAt = 0)
	{
		int num = sourceText.Length - word.Length + 1;
		for (int i = startAt; i < num; i++)
		{
			bool flag = true;
			for (int j = 0; j < word.Length; j++)
			{
				if (char.ToLower(sourceText[i + j]) != word[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	private static int FindWordStart(char[] sourceText, int startAt)
	{
		for (int num = startAt; num >= 0; num--)
		{
			if (char.IsWhiteSpace(sourceText[num]))
			{
				return num + 1;
			}
		}
		return 0;
	}

	private static int FindWordEnd(char[] sourceText, int startAt)
	{
		for (int i = startAt; i < sourceText.Length; i++)
		{
			if (char.IsWhiteSpace(sourceText[i]))
			{
				return i;
			}
		}
		return sourceText.Length;
	}
}
