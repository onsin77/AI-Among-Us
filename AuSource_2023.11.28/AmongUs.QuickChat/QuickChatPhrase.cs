using System;
using System.Linq;
using AmongUs.Data;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatPhrase
{
	private QuickChatPhraseType type;

	private bool isInput;

	private bool isNegative;

	private byte playerId;

	private StringNames phraseKey;

	private QuickChatPhraseInput[] phraseInputs;

	public bool IsInput
	{
		get
		{
			if (Type != QuickChatPhraseType.ComplexPhrase)
			{
				return isInput;
			}
			return false;
		}
	}

	public bool IsNegative => isNegative;

	public QuickChatPhraseType Type => type;

	public byte PlayerId => playerId;

	public StringNames PhraseKey => phraseKey;

	public QuickChatPhraseInput[] PhraseInputs => phraseInputs;

	public int ExpectedInputCount
	{
		get
		{
			if (PhraseInputs == null)
			{
				return 0;
			}
			return PhraseInputs.Length;
		}
	}

	public static QuickChatPhrase NewPlayerId(byte playerID)
	{
		return new QuickChatPhrase
		{
			isInput = true,
			isNegative = false,
			type = QuickChatPhraseType.PlayerId,
			playerId = playerID
		};
	}

	public static QuickChatPhrase NewSimplePhrase(StringNames localizationKey, bool isInput, bool isNegative)
	{
		return new QuickChatPhrase
		{
			isInput = isInput,
			isNegative = isNegative,
			type = QuickChatPhraseType.SimplePhrase,
			phraseKey = localizationKey,
			phraseInputs = null
		};
	}

	public static QuickChatPhrase NewComplexPhrase(StringNames localizationKey, QuickChatPhraseInput[] inputs, bool isNegative)
	{
		return new QuickChatPhrase
		{
			isInput = false,
			isNegative = isNegative,
			type = QuickChatPhraseType.ComplexPhrase,
			phraseKey = localizationKey,
			phraseInputs = inputs
		};
	}

	public string ToPreviewText(QuickChatPhrase[] inputs)
	{
		switch (type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2}.{3} has not been configured. This should not be happening. Returning empty string.", "QuickChatPhrase", "ToPreviewText", "QuickChatPhraseType", type));
			return "";
		case QuickChatPhraseType.PlayerId:
			return ToPlayerNamePreviewText();
		case QuickChatPhraseType.SimplePhrase:
			return ToSimplePhrasePreviewText();
		case QuickChatPhraseType.ComplexPhrase:
			return ToComplexPreviewText(inputs, useColor: false);
		}
	}

	public string ToButtonText()
	{
		switch (type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2}.{3} has not been configured. This should not be happening. Returning empty string.", "QuickChatPhrase", "ToButtonText", "QuickChatPhraseType", type));
			return "";
		case QuickChatPhraseType.PlayerId:
			return ToPlayerNameButtonText();
		case QuickChatPhraseType.SimplePhrase:
			return ToSimplePhraseButtonText();
		case QuickChatPhraseType.ComplexPhrase:
			return ToComplexPreviewText(null, useColor: true);
		}
	}

	public string ToComplexInputPreviewText()
	{
		switch (type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2}.{3} has not been configured. This should not be happening. Returning empty string.", "QuickChatPhrase", "ToComplexInputPreviewText", "QuickChatPhraseType", type));
			return "";
		case QuickChatPhraseType.ComplexPhrase:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2} cannot be converted to Complex Input Preview Text. Only non-complex phrase types are supported. Returning an empty string.", "QuickChatPhrase", "ToComplexInputPreviewText", QuickChatPhraseType.ComplexPhrase));
			return "";
		case QuickChatPhraseType.PlayerId:
			return ToPlayerNameInputPreviewText();
		case QuickChatPhraseType.SimplePhrase:
			return ToSimplePhraseInputPreviewText();
		}
	}

	public string ToLogString()
	{
		string text = $"{type}: ";
		switch (type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}() :: {2}.{3} has not been configured. This should not be happening. Returning empty string.", "QuickChatPhrase", "ToLogString", "QuickChatPhraseType", type));
			return text + "ERROR";
		case QuickChatPhraseType.PlayerId:
			return text + playerId;
		case QuickChatPhraseType.SimplePhrase:
		case QuickChatPhraseType.ComplexPhrase:
			return text + phraseKey;
		}
	}

	private string ToPlayerNameText(bool boldPlayerName)
	{
		if (PlayerId == byte.MaxValue)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhrase] ToPlayerNameText() :: 'playerId' was empty or uninitialized. Returning null string.");
			return null;
		}
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(PlayerId);
		if (!boldPlayerName)
		{
			return playerById.PlayerName;
		}
		return AddBoldTagsIfSupported(playerById.PlayerName);
	}

	private string ToPlayerColorText()
	{
		if (PlayerId == byte.MaxValue)
		{
			QuickChatMenu.Logger.Error("[QuickChatPhrase] ToPlayerColorText() :: 'playerId' was empty or uninitialized. Returning null string.");
			return null;
		}
		return GameData.Instance.GetPlayerById(PlayerId).GetPlayerColorString();
	}

	private string ToPlayerNamePreviewText()
	{
		return ToPlayerNameText(boldPlayerName: true) + " (" + ToPlayerColorText() + ")";
	}

	private string ToPlayerNameButtonText()
	{
		return ToPlayerNameText(boldPlayerName: true) + Environment.NewLine + ToPlayerColorText();
	}

	private string ToPlayerNameInputPreviewText()
	{
		return ToPlayerNameText(boldPlayerName: true) + " (" + ToPlayerColorText() + ")";
	}

	public Color ToPlayerColor()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerId == byte.MaxValue)
		{
			return Color.white;
		}
		return GameData.Instance.GetPlayerById(PlayerId).Color;
	}

	public Color ToPlayerBackgroundColor()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerId == byte.MaxValue)
		{
			return Color.white;
		}
		return GameData.Instance.GetPlayerById(PlayerId).ShadowColor;
	}

	private string ToSimplePhraseText()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(PhraseKey);
	}

	private string ToSimplePhrasePreviewText()
	{
		return ToSimplePhraseText();
	}

	private string ToSimplePhraseButtonText()
	{
		return ToSimplePhraseText();
	}

	private string ToSimplePhraseInputPreviewText()
	{
		return AddBoldTagsIfSupported(ToSimplePhraseText());
	}

	private string ToComplexPreviewText(QuickChatPhrase[] inputs, bool useColor)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(PhraseKey);
		for (int i = 0; i < 10; i++)
		{
			char c = (char)(65 + i);
			string newValue = $"({c})";
			if (inputs != null && inputs.Length > i)
			{
				newValue = inputs[i].ToComplexInputPreviewText();
			}
			else if (phraseInputs != null && phraseInputs.Length > i)
			{
				QuickChatPhraseInput quickChatPhraseInput = phraseInputs[i];
				if (quickChatPhraseInput != null && quickChatPhraseInput.Tags.Length != 0)
				{
					if (quickChatPhraseInput.Tags.Length > 1)
					{
						string text2 = string.Join("/", quickChatPhraseInput.Tags.Select((QuickChatTag x) =>
						{
							//IL_0012: Unknown result type (might be due to invalid IL or missing references)
							return GetConditionallyColoredString(x.LocalizedInputName.ToUpper(), useColor, x.Color);
						}));
						newValue = AddBoldTagsIfSupported("[" + text2 + "]");
					}
					else
					{
						QuickChatTag quickChatTag = quickChatPhraseInput.Tags[0];
						newValue = AddBoldTagsIfSupported(GetConditionallyColoredString("[" + quickChatTag.LocalizedInputName.ToUpper() + "]", useColor, quickChatTag.Color));
					}
				}
			}
			text = text.Replace($"{{{i}}}", newValue);
		}
		return text;
	}

	private static string GetConditionallyColoredString(string input, bool useColor, Color color)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string text2 = "";
		if (useColor)
		{
			text = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
			text2 = "</color>";
		}
		return text + input + text2;
	}

	private static string AddBoldTagsIfSupported(string input)
	{
		if (!TranslationController.DoesLanguageSupportBold(DataManager.Settings.Language.CurrentLanguage))
		{
			return input;
		}
		return "<b>" + input + "</b>";
	}
}
