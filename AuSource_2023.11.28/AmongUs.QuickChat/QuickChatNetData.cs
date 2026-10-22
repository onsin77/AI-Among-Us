using Hazel;

namespace AmongUs.QuickChat;

public static class QuickChatNetData
{
	public static void Serialize(QuickChatPhraseBuilderResult data, MessageWriter writer)
	{
		QuickChatPhraseType type = data.RootPhrase.Type;
		writer.Write((byte)type);
		switch (type)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] Can't serialize data for {1}.{2}. Message will be empty!", "QuickChatNetData", "QuickChatPhraseType", type));
			break;
		case QuickChatPhraseType.Empty:
			QuickChatMenu.Logger.Error("[QuickChatNetData] Empty is not a valid message type. Message body will be empty. This should not be happening!");
			break;
		case QuickChatPhraseType.PlayerId:
			SerializePlayerID(data.RootPhrase.PlayerID, writer);
			break;
		case QuickChatPhraseType.SimplePhrase:
			SerializeSimplePhrase(data.RootPhrase.PhraseKey, writer);
			break;
		case QuickChatPhraseType.ComplexPhrase:
			SerializeComplexPhrase(data, writer);
			break;
		}
	}

	public static QuickChatPhraseBuilderResult Deserialize(MessageReader reader)
	{
		QuickChatPhraseType quickChatPhraseType = (QuickChatPhraseType)reader.ReadByte();
		switch (quickChatPhraseType)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}.{2} has not been configured. This should not be happening.", "QuickChatNetData", "QuickChatPhraseType", quickChatPhraseType));
			return null;
		case QuickChatPhraseType.Empty:
			QuickChatMenu.Logger.Error("[QuickChatNetData] Empty is not a valid message type. Message body will be empty. This should not be happening!");
			return null;
		case QuickChatPhraseType.PlayerId:
		{
			byte playerID = DeserializePlayerID(reader);
			return new QuickChatPhraseBuilderResult(QuickChatPhraseType.PlayerId, StringNames.None, playerID, null);
		}
		case QuickChatPhraseType.SimplePhrase:
		{
			StringNames rootPhrase = DeserializeSimplePhrase(reader);
			return new QuickChatPhraseBuilderResult(QuickChatPhraseType.SimplePhrase, rootPhrase, 0, null);
		}
		case QuickChatPhraseType.ComplexPhrase:
			return DeserializeComplexPhrase(reader);
		}
	}

	private static void SerializePlayerID(byte playerID, MessageWriter writer)
	{
		writer.Write(playerID);
	}

	private static byte DeserializePlayerID(MessageReader reader)
	{
		return reader.ReadByte();
	}

	private static void SerializeSimplePhrase(StringNames phraseKey, MessageWriter writer)
	{
		writer.Write((ushort)phraseKey);
	}

	private static StringNames DeserializeSimplePhrase(MessageReader reader)
	{
		return (StringNames)reader.ReadUInt16();
	}

	private static void SerializeComplexPhrase(QuickChatPhraseBuilderResult data, MessageWriter writer)
	{
		writer.Write((ushort)data.RootPhrase.PhraseKey);
		writer.Write((byte)data.SubPhrases.Length);
		for (int i = 0; i < data.SubPhrases.Length; i++)
		{
			QuickChatPhraseBuilderResult.Phrase phrase = data.SubPhrases[i];
			writer.Write((byte)phrase.Type);
			switch (phrase.Type)
			{
			default:
				QuickChatMenu.Logger.Error(string.Format("[{0}] Writing '{1}.{2}' has not been configured.", "QuickChatNetData", "QuickChatPhraseType", phrase.Type));
				break;
			case QuickChatPhraseType.Empty:
			case QuickChatPhraseType.ComplexPhrase:
				QuickChatMenu.Logger.Error(string.Format("[{0}] {1}.{2} is not a valid sub-phrase type!", "QuickChatNetData", "QuickChatPhraseType", phrase.Type));
				break;
			case QuickChatPhraseType.PlayerId:
				SerializePlayerID(phrase.PlayerID, writer);
				break;
			case QuickChatPhraseType.SimplePhrase:
				SerializeSimplePhrase(phrase.PhraseKey, writer);
				break;
			}
		}
	}

	private static QuickChatPhraseBuilderResult DeserializeComplexPhrase(MessageReader reader)
	{
		StringNames rootPhrase = (StringNames)reader.ReadUInt16();
		int num = reader.ReadByte();
		QuickChatPhrase[] array = new QuickChatPhrase[num];
		for (byte b = 0; b < num; b++)
		{
			QuickChatPhraseType quickChatPhraseType = (QuickChatPhraseType)reader.ReadByte();
			switch (quickChatPhraseType)
			{
			default:
				QuickChatMenu.Logger.Error(string.Format("[{0}] Reading '{1}.{2}' has not been configured.", "QuickChatNetData", "QuickChatPhraseType", quickChatPhraseType));
				break;
			case QuickChatPhraseType.Empty:
			case QuickChatPhraseType.ComplexPhrase:
				QuickChatMenu.Logger.Error(string.Format("[{0}] {1}.{2} is not a valid sub-phrase type!", "QuickChatNetData", "QuickChatPhraseType", quickChatPhraseType));
				break;
			case QuickChatPhraseType.PlayerId:
			{
				byte playerID = DeserializePlayerID(reader);
				array[b] = QuickChatPhrase.NewPlayerId(playerID);
				break;
			}
			case QuickChatPhraseType.SimplePhrase:
			{
				StringNames localizationKey = DeserializeSimplePhrase(reader);
				array[b] = QuickChatPhrase.NewSimplePhrase(localizationKey, isInput: true, isNegative: false);
				break;
			}
			}
		}
		return new QuickChatPhraseBuilderResult(QuickChatPhraseType.ComplexPhrase, rootPhrase, 0, array);
	}
}
