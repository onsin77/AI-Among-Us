using System;
using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;

public class NameTextBehaviour : MonoBehaviour
{
	public static readonly HashSet<char> SymbolChars = new HashSet<char>
	{
		'?', '!', ',', '.', '\'', ':', ';', '(', ')', '/',
		'\\', '%', '^', '&', '-', '=', '\r', '\n', '[', ']'
	};

	public static NameTextBehaviour Instance;

	public TextBoxTMP nameSource;

	public void Start()
	{
		Instance = this;
		if (!IsValidName(DataManager.Player.Customization.Name))
		{
			nameSource.SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName));
		}
		else
		{
			nameSource.SetText(DataManager.Player.Customization.Name);
		}
	}

	public void UpdateName()
	{
		if (!ShakeIfInvalid())
		{
			DataManager.Player.Customization.Name = nameSource.text;
			DataManager.Player.Save();
		}
	}

	public static bool IsValidName(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		if (text.Equals(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName), StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		foreach (char c in text)
		{
			if (c < ' ')
			{
				return false;
			}
			if (SymbolChars.Contains(c))
			{
				return false;
			}
		}
		if (BlockedWords.CensorWords(text) != text)
		{
			return false;
		}
		return true;
	}

	public bool ShakeIfInvalid()
	{
		if (!IsValidName(nameSource.text))
		{
			((MonoBehaviour)this).StartCoroutine(Effects.SwayX(((Component)nameSource).transform));
			return true;
		}
		return false;
	}
}
