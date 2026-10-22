using System.IO;
using Hazel;

namespace AmongUs.GameOptions;

public class GameOptionsFactory
{
	private const byte ModularOptionsDataVersion = 7;

	private readonly ILogger logger;

	public GameOptionsFactory(ILogger logger)
	{
		this.logger = logger;
	}

	public byte[] ToBytes(IGameOptions data)
	{
		byte version = data.Version;
		if (version < 7)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				if (!(data is GameOptionsData gameOptionsData))
				{
					logger.WriteError($"Could not serialize OG game options data for version {version}");
					return null;
				}
				GameOptionsData.Serialize(binaryWriter, gameOptionsData);
				binaryWriter.Flush();
				memoryStream.Position = 0L;
				return memoryStream.ToArray();
			}
		}
		GameModes gameMode = data.GameMode;
		MessageWriter val = MessageWriter.Get((SendOption)0);
		val.Write(data.Version);
		val.StartMessage((byte)0);
		val.Write((byte)gameMode);
		switch (gameMode)
		{
		case GameModes.Normal:
			NormalModeToWriter(val, data);
			break;
		case GameModes.HideNSeek:
			HideNSeekModeToWriter(val, data);
			break;
		}
		val.EndMessage();
		byte[] result = val.ToByteArray(false);
		val.Recycle();
		return result;
	}

	public void ToNetworkMessageWithSize(MessageWriter writer, IGameOptions data)
	{
		byte[] array = ToBytes(data);
		writer.WriteBytesAndSize(array);
	}

	public IGameOptions FromBytes(byte[] rawBytes)
	{
		int num = rawBytes.Length;
		int num2 = 0;
		MessageReader sized = MessageReader.GetSized(num);
		sized.Length = num;
		rawBytes.CopyTo(sized.Buffer, 0);
		sized.Offset = num2;
		sized.Length = num - num2;
		sized.Position = 0;
		byte version = sized.ReadByte();
		IGameOptions result = CheckDeserializationVersion(sized, version);
		sized.Recycle();
		return result;
	}

	public IGameOptions FromNetworkMessageWithSize(MessageReader reader)
	{
		byte b = reader.ReadByte();
		byte b2 = reader.ReadByte();
		IGameOptions result = CheckDeserializationVersion(reader, b2);
		if (b2 < 7)
		{
			reader.Position = reader.Position + b - 1;
		}
		return result;
	}

	private IGameOptions CheckDeserializationVersion(MessageReader reader, byte version)
	{
		if (version < 7)
		{
			using (MemoryStream input = new MemoryStream(reader.Buffer, reader.Position + reader.Offset - 1, reader.BytesRemaining + 1))
			{
				using BinaryReader reader2 = new BinaryReader(input);
				return GameOptionsData.Deserialize(reader2, version, logger);
			}
		}
		return FromInternalMessage(reader, version);
	}

	private IGameOptions FromInternalMessage(MessageReader reader, byte version)
	{
		MessageReader val = reader.ReadMessage();
		GameModes gameModes = (GameModes)val.ReadByte();
		switch (gameModes)
		{
		case GameModes.Normal:
			return NormalModeFromBytes(val, version);
		case GameModes.HideNSeek:
			return HideNSeekModeFromBytes(val, version);
		default:
			logger.WriteError($"Could not parse options data for game mode {gameModes}");
			return null;
		}
	}

	private void NormalModeToWriter(MessageWriter writer, IGameOptions optionsData)
	{
		if (optionsData.Version == 7)
		{
			NormalGameOptionsV07.Serialize(writer, optionsData as NormalGameOptionsV07);
		}
		else
		{
			logger.WriteError($"Could not serialize normal options data for version {optionsData.Version}");
		}
	}

	private void HideNSeekModeToWriter(MessageWriter writer, IGameOptions optionsData)
	{
		if (optionsData.Version == 7)
		{
			HideNSeekGameOptionsV07.Serialize(writer, optionsData as HideNSeekGameOptionsV07);
		}
		else
		{
			logger.WriteError($"Could not serialize hidenseek options data for version {optionsData.Version}");
		}
	}

	private IGameOptions NormalModeFromBytes(MessageReader reader, byte version)
	{
		if (version == 7)
		{
			return NormalGameOptionsV07.Deserialize(reader, logger);
		}
		logger.WriteError($"Could not parse normal game options for version {version}");
		return null;
	}

	private IGameOptions HideNSeekModeFromBytes(MessageReader reader, byte version)
	{
		if (version == 7)
		{
			return HideNSeekGameOptionsV07.Deserialize(reader, logger);
		}
		logger.WriteError($"Could not parse hidenseek game options for version {version}");
		return null;
	}
}
