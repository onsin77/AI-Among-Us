using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BlockedWords
{
	private class LengthCompare : IComparer<string>
	{
		public static readonly LengthCompare Instance = new LengthCompare();

		public int Compare(string x, string y)
		{
			return -x.Length.CompareTo(y.Length);
		}
	}

	public static readonly HashSet<char> SymbolChars;

	private static LetterTree SkipList;

	static BlockedWords()
	{
		SymbolChars = new HashSet<char>
		{
			'?', '!', ',', '.', '\'', ':', ';', '(', ')', '/',
			'\\', '%', '^', '&', '-', '='
		};
	}

	public static void SetLanguage(TranslatedImageSet newLang)
	{
		Debug.Log((object)("Censor set language = " + newLang.Name));
		SkipList = new LetterTree();
		if (!Object.op_Implicit((Object)(object)newLang) || !Object.op_Implicit((Object)(object)newLang.CensorSet))
		{
			return;
		}
		using StringReader stringReader = new StringReader(newLang.CensorSet.text);
		string text;
		while ((text = stringReader.ReadLine()) != null)
		{
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				SkipList.AddWord(text);
			}
		}
	}

	public static bool ContainsWord(string chatText)
	{
		char[] input = chatText.ToCharArray();
		for (int i = 0; i < chatText.Length; i++)
		{
			if (SkipList.Search(input, i) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static string CensorWords(string chatText, bool skipConsoleAPI = false)
	{
		char[] array = chatText.ToCharArray();
		for (int i = 0; i < chatText.Length; i++)
		{
			int num = SkipList.Search(array, i);
			if (num > 0)
			{
				for (int j = 0; j < num; j++)
				{
					array[i + j] = '*';
				}
				i = i + num - 1;
			}
		}
		return new string(array);
	}

	public static void CensorWordsAsync(string chatText, Action<string> onWordsCensoredCallback)
	{
		((MonoBehaviour)DestroyableSingleton<ActiveInputManager>.Instance).StartCoroutine(CensorWordsCo(chatText, onWordsCensoredCallback));
	}

	public static IEnumerator CensorWordsCo(string chatText, Action<string> onWordsCensoredCallback)
	{
		char[] array = chatText.ToCharArray();
		for (int i = 0; i < chatText.Length; i++)
		{
			int num = SkipList.Search(array, i);
			if (num > 0)
			{
				for (int j = 0; j < num; j++)
				{
					array[i + j] = '*';
				}
				i = i + num - 1;
			}
		}
		string obj = new string(array);
		onWordsCensoredCallback(obj);
		yield return null;
	}

	private static bool IsLetter(char letter)
	{
		switch (letter)
		{
		case ' ':
			return false;
		case '\r':
			return false;
		case '\n':
			return false;
		default:
			if (SymbolChars.Contains(letter))
			{
				return false;
			}
			return true;
		}
	}
}
