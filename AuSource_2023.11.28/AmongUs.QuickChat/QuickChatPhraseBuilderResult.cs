using AmongUs.Data;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatPhraseBuilderResult
{
	public class Phrase
	{
		public readonly QuickChatPhraseType Type;

		public readonly StringNames PhraseKey;

		public readonly byte PlayerID;

		public Phrase(QuickChatPhrase phrase)
		{
			Type = phrase.Type;
			switch (phrase.Type)
			{
			default:
				QuickChatMenu.Logger.Error($"[{GetType().FullName}] Cannot be created. {phrase.Type} has not yet been configured.");
				break;
			case QuickChatPhraseType.SimplePhrase:
			case QuickChatPhraseType.ComplexPhrase:
				PhraseKey = phrase.PhraseKey;
				break;
			case QuickChatPhraseType.PlayerId:
				PlayerID = phrase.PlayerId;
				break;
			}
		}

		public Phrase(QuickChatPhraseType type, StringNames key, byte playerID)
		{
			Type = type;
			PhraseKey = key;
			PlayerID = playerID;
		}
	}

	public readonly Phrase RootPhrase;

	public readonly Phrase[] SubPhrases;

	public QuickChatPhraseBuilderResult(QuickChatPhraseType type, StringNames rootPhrase, byte playerID, QuickChatPhrase[] subPhrases)
	{
		RootPhrase = new Phrase(type, rootPhrase, playerID);
		SubPhrases = ConvertSubPhrases(subPhrases);
	}

	public QuickChatPhraseBuilderResult(QuickChatPhrase rootPhrase, QuickChatPhrase[] subPhrases)
		: this(rootPhrase.Type, rootPhrase.PhraseKey, rootPhrase.PlayerId, subPhrases)
	{
	}

	public bool IsValid()
	{
		if (RootPhrase == null)
		{
			return false;
		}
		Phrase[] subPhrases = SubPhrases;
		foreach (Phrase phrase in subPhrases)
		{
			if (phrase == null || phrase.Type == QuickChatPhraseType.ComplexPhrase)
			{
				return false;
			}
		}
		return true;
	}

	public string ToChatText()
	{
		switch (RootPhrase.Type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] Type {1} has not yet been configured.", "QuickChatPhraseBuilderResult", RootPhrase.Type));
			return "";
		case QuickChatPhraseType.PlayerId:
			return ToPlayerNameChatText(RootPhrase.PlayerID);
		case QuickChatPhraseType.SimplePhrase:
			return ToSimpleChatText(RootPhrase.PhraseKey);
		case QuickChatPhraseType.ComplexPhrase:
			return ToComplexChatText();
		}
	}

	private static string ToPlayerNameChatText(byte playerID)
	{
		if ((Object)(object)GameData.Instance == (Object)null)
		{
			QuickChatMenu.Logger.Warning("[QuickChatPhraseBuilderResult] ToPlayerNameChatText() GameData.Instance is NULL");
			return "ERROR";
		}
		if (playerID == byte.MaxValue)
		{
			QuickChatMenu.Logger.Warning("[QuickChatPhraseBuilderResult] ToPlayerNameChatText() Player ID is empty.");
			return "ERROR";
		}
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerID);
		if (playerById == null)
		{
			QuickChatMenu.Logger.Warning(string.Format("[{0}] ToPlayerNameChatText() Player with associated ID '{1}' was not found.", "QuickChatPhraseBuilderResult", playerID));
			return "ERROR";
		}
		if (DataManager.Settings.Accessibility.ColorBlindMode)
		{
			return playerById.PlayerName + " (" + playerById.GetPlayerColorString() + ")";
		}
		return playerById.PlayerName;
	}

	private static string ToSimpleChatText(StringNames key)
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(key);
	}

	private string ToComplexChatText()
	{
		string[] array = new string[SubPhrases.Length];
		for (int i = 0; i < SubPhrases.Length; i++)
		{
			Phrase phrase = SubPhrases[i];
			array[i] = "-";
			if (phrase != null)
			{
				switch (phrase.Type)
				{
				default:
					QuickChatMenu.Logger.Error(string.Format("[{0}] Type {1} has not yet been configured.", "QuickChatPhraseBuilderResult", RootPhrase.Type));
					break;
				case QuickChatPhraseType.ComplexPhrase:
					QuickChatMenu.Logger.Error(string.Format("[{0}] Phrases of type {1} are not supported as sub-phrases!", "QuickChatPhraseBuilderResult", QuickChatPhraseType.ComplexPhrase));
					break;
				case QuickChatPhraseType.PlayerId:
					array[i] = ToPlayerNameChatText(phrase.PlayerID);
					break;
				case QuickChatPhraseType.SimplePhrase:
					array[i] = ToSimpleChatText(phrase.PhraseKey);
					break;
				}
			}
		}
		string format = DestroyableSingleton<TranslationController>.Instance.GetString(RootPhrase.PhraseKey);
		object[] args = array;
		return string.Format(format, args);
	}

	private static Phrase[] ConvertSubPhrases(QuickChatPhrase[] input)
	{
		int num = input?.Length ?? 0;
		Phrase[] array = new Phrase[num];
		for (int i = 0; i < num; i++)
		{
			if (input[i] != null)
			{
				if (input[i].Type == QuickChatPhraseType.ComplexPhrase)
				{
					QuickChatMenu.Logger.Error(string.Format("[{0}] Cannot create input. Inputs of type {1} are not supported.", "QuickChatPhraseBuilderResult", QuickChatPhraseType.ComplexPhrase));
				}
				else
				{
					array[i] = new Phrase(input[i]);
				}
			}
		}
		return array;
	}
}
