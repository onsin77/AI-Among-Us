using System.IO;
using Hazel;

namespace AmongUs.GameOptions;

public class GameOptionsData : IGameOptions
{
	private static readonly int[] RecommendedKillCooldown = new int[16]
	{
		0, 0, 0, 0, 45, 30, 15, 35, 30, 25,
		20, 20, 20, 20, 20, 20
	};

	private static readonly int[] RecommendedImpostors = new int[16]
	{
		0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
		2, 2, 3, 3, 3, 3
	};

	private static readonly int[] MaxImpostors = new int[16]
	{
		0, 0, 0, 0, 1, 1, 1, 2, 2, 3,
		3, 3, 3, 3, 3, 3
	};

	private static readonly float[] KillDistances = new float[3] { 1f, 1.8f, 2.5f };

	private static readonly string[] KillDistanceStrings = new string[3] { "Short", "Normal", "Long" };

	private static readonly int[] MinPlayers = new int[4] { 4, 4, 7, 9 };

	private const byte EmergencyCooldownSupportVersion = 1;

	private const byte VisualTaskOptionsSupportVersion = 2;

	private const byte ExtraVotingOptionsSupportVersion = 3;

	private const byte RolesSupportVersion = 4;

	public const byte ServerVersion = 2;

	public const byte NewestVersion = 5;

	public float PlayerSpeedMod = 1f;

	public float CrewLightMod = 1f;

	public float ImpostorLightMod = 1.5f;

	public float KillCooldown = 15f;

	public int NumCommonTasks = 1;

	public int NumLongTasks = 1;

	public int NumShortTasks = 2;

	public int NumEmergencyMeetings = 1;

	public int EmergencyCooldown;

	public bool GhostsDoTasks = true;

	public int KillDistance = 1;

	public int DiscussionTime = 15;

	public int VotingTime = 120;

	public bool ConfirmImpostor = true;

	public bool VisualTasks = true;

	public bool AnonymousVotes;

	public TaskBarMode TaskBarMode;

	private readonly ILogger logger;

	private RoleOptionsData roleOptions = new RoleOptionsData();

	public byte Version { get; private set; } = 6;

	public GameModes GameMode => GameModes.Normal;

	public int MaxPlayers { get; private set; } = 10;

	public GameKeywords Keywords { get; private set; } = GameKeywords.English;

	public byte MapId { get; private set; }

	public IRoleOptionsCollection RoleOptions => roleOptions;

	public int NumImpostors { get; private set; } = 1;

	public bool IsDefaults { get; private set; } = true;

	public int TotalTaskCount => NumCommonTasks + NumLongTasks + NumShortTasks;

	public GameOptionsData(ILogger logger)
	{
		this.logger = logger;
	}

	public static void Serialize(BinaryWriter writer, GameOptionsData gameOptionsData)
	{
		writer.Write(gameOptionsData.Version);
		writer.Write((byte)gameOptionsData.MaxPlayers);
		writer.Write((uint)gameOptionsData.Keywords);
		writer.Write(gameOptionsData.MapId);
		writer.Write(gameOptionsData.PlayerSpeedMod);
		writer.Write(gameOptionsData.CrewLightMod);
		writer.Write(gameOptionsData.ImpostorLightMod);
		writer.Write(gameOptionsData.KillCooldown);
		writer.Write((byte)gameOptionsData.NumCommonTasks);
		writer.Write((byte)gameOptionsData.NumLongTasks);
		writer.Write((byte)gameOptionsData.NumShortTasks);
		writer.Write(gameOptionsData.NumEmergencyMeetings);
		writer.Write((byte)gameOptionsData.NumImpostors);
		writer.Write((byte)gameOptionsData.KillDistance);
		writer.Write(gameOptionsData.DiscussionTime);
		writer.Write(gameOptionsData.VotingTime);
		writer.Write(gameOptionsData.IsDefaults);
		if (gameOptionsData.Version > 1)
		{
			writer.Write((byte)gameOptionsData.EmergencyCooldown);
		}
		if (gameOptionsData.Version > 2)
		{
			writer.Write(gameOptionsData.ConfirmImpostor);
			writer.Write(gameOptionsData.VisualTasks);
		}
		if (gameOptionsData.Version > 3)
		{
			writer.Write(gameOptionsData.AnonymousVotes);
			writer.Write((byte)gameOptionsData.TaskBarMode);
		}
		if (gameOptionsData.Version > 4)
		{
			RoleOptionsData.Serialize(writer, gameOptionsData.roleOptions);
		}
	}

	public static GameOptionsData Deserialize(BinaryReader reader, byte version, ILogger logger)
	{
		try
		{
			GameOptionsData gameOptionsData = new GameOptionsData(logger);
			gameOptionsData.Version = reader.ReadByte();
			gameOptionsData.MaxPlayers = reader.ReadByte();
			gameOptionsData.Keywords = (GameKeywords)reader.ReadUInt32();
			gameOptionsData.MapId = reader.ReadByte();
			gameOptionsData.PlayerSpeedMod = reader.ReadSingle();
			gameOptionsData.CrewLightMod = reader.ReadSingle();
			gameOptionsData.ImpostorLightMod = reader.ReadSingle();
			gameOptionsData.KillCooldown = reader.ReadSingle();
			gameOptionsData.NumCommonTasks = reader.ReadByte();
			gameOptionsData.NumLongTasks = reader.ReadByte();
			gameOptionsData.NumShortTasks = reader.ReadByte();
			gameOptionsData.NumEmergencyMeetings = reader.ReadInt32();
			gameOptionsData.NumImpostors = reader.ReadByte();
			gameOptionsData.KillDistance = reader.ReadByte();
			gameOptionsData.DiscussionTime = reader.ReadInt32();
			gameOptionsData.VotingTime = reader.ReadInt32();
			gameOptionsData.IsDefaults = reader.ReadBoolean();
			try
			{
				if (version > 1)
				{
					gameOptionsData.EmergencyCooldown = reader.ReadByte();
				}
				if (version > 2)
				{
					gameOptionsData.ConfirmImpostor = reader.ReadBoolean();
					gameOptionsData.VisualTasks = reader.ReadBoolean();
				}
				if (version > 3)
				{
					gameOptionsData.AnonymousVotes = reader.ReadBoolean();
					gameOptionsData.TaskBarMode = (TaskBarMode)reader.ReadByte();
				}
				if (version > 4)
				{
					gameOptionsData.roleOptions = RoleOptionsData.Deserialize(reader);
				}
			}
			catch
			{
			}
			return gameOptionsData;
		}
		catch
		{
		}
		return null;
	}

	public bool AreInvalid(int maxExpectedPlayers)
	{
		if (MaxPlayers <= maxExpectedPlayers && NumImpostors >= 1 && NumImpostors <= 3 && KillDistance >= 0 && KillDistance <= 2 && !(PlayerSpeedMod <= 0f))
		{
			return PlayerSpeedMod > 3f;
		}
		return true;
	}

	public void SetRecommendations(int numPlayers, bool isOnline)
	{
		PlayerSpeedMod = 1f;
		CrewLightMod = 1f;
		ImpostorLightMod = 1.5f;
		KillCooldown = RecommendedKillCooldown[numPlayers];
		NumCommonTasks = 1;
		NumLongTasks = 1;
		NumShortTasks = 2;
		NumEmergencyMeetings = 1;
		if (!isOnline)
		{
			NumImpostors = RecommendedImpostors[numPlayers];
		}
		KillDistance = 1;
		DiscussionTime = 15;
		VotingTime = 120;
		IsDefaults = true;
		ConfirmImpostor = true;
		VisualTasks = true;
		EmergencyCooldown = (isOnline ? 15 : 0);
		roleOptions.ShapeshifterCooldown = 10f;
		roleOptions.ShapeshifterDuration = 30f;
		roleOptions.ShapeshifterLeaveSkin = false;
		roleOptions.ImpostorsCanSeeProtect = false;
		roleOptions.ScientistCooldown = 15f;
		roleOptions.ScientistBatteryCharge = 5f;
		roleOptions.GuardianAngelCooldown = 60f;
		roleOptions.ProtectionDurationSeconds = 10f;
		roleOptions.EngineerCooldown = 30f;
		roleOptions.EngineerInVentMaxTime = 15f;
	}

	public bool Validate(int numPlayers)
	{
		bool result = false;
		if (NumCommonTasks + NumLongTasks + NumShortTasks == 0)
		{
			NumShortTasks = 1;
			result = true;
		}
		int num = MaxImpostors[numPlayers];
		if (NumImpostors > num)
		{
			NumImpostors = num;
			result = true;
		}
		return result;
	}

	public void SetByte(ByteOptionNames optionName, byte value)
	{
		if (optionName == ByteOptionNames.MapId)
		{
			MapId = value;
		}
		else
		{
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} could not set byte named {optionName}");
		}
	}

	public void SetFloat(FloatOptionNames optionName, float value)
	{
		switch (optionName)
		{
		case FloatOptionNames.PlayerSpeedMod:
			PlayerSpeedMod = value;
			break;
		case FloatOptionNames.CrewLightMod:
			CrewLightMod = value;
			break;
		case FloatOptionNames.ImpostorLightMod:
			ImpostorLightMod = value;
			break;
		case FloatOptionNames.KillCooldown:
			KillCooldown = value;
			break;
		case FloatOptionNames.ShapeshifterCooldown:
			roleOptions.ShapeshifterCooldown = value;
			break;
		case FloatOptionNames.ShapeshifterDuration:
			roleOptions.ShapeshifterDuration = value;
			break;
		case FloatOptionNames.ScientistBatteryCharge:
			roleOptions.ScientistBatteryCharge = value;
			break;
		case FloatOptionNames.ScientistCooldown:
			roleOptions.ScientistCooldown = value;
			break;
		case FloatOptionNames.GuardianAngelCooldown:
			roleOptions.GuardianAngelCooldown = value;
			break;
		case FloatOptionNames.ProtectionDurationSeconds:
			roleOptions.ProtectionDurationSeconds = value;
			break;
		case FloatOptionNames.EngineerCooldown:
			roleOptions.EngineerCooldown = value;
			break;
		case FloatOptionNames.EngineerInVentMaxTime:
			roleOptions.EngineerInVentMaxTime = value;
			break;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} could not set float named {optionName}");
			break;
		}
	}

	public void SetBool(BoolOptionNames optionName, bool value)
	{
		switch (optionName)
		{
		case BoolOptionNames.VisualTasks:
			VisualTasks = value;
			break;
		case BoolOptionNames.ConfirmImpostor:
			ConfirmImpostor = value;
			break;
		case BoolOptionNames.AnonymousVotes:
			AnonymousVotes = value;
			break;
		case BoolOptionNames.IsDefaults:
			IsDefaults = value;
			break;
		case BoolOptionNames.ShapeshifterLeaveSkin:
			roleOptions.ShapeshifterLeaveSkin = value;
			break;
		case BoolOptionNames.ImpostorsCanSeeProtect:
			roleOptions.ImpostorsCanSeeProtect = value;
			break;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} could not set bool named {optionName}");
			break;
		}
	}

	public void SetInt(Int32OptionNames optionName, int value)
	{
		switch (optionName)
		{
		case Int32OptionNames.KillDistance:
			KillDistance = value;
			break;
		case Int32OptionNames.EmergencyCooldown:
			EmergencyCooldown = value;
			break;
		case Int32OptionNames.DiscussionTime:
			DiscussionTime = value;
			break;
		case Int32OptionNames.VotingTime:
			VotingTime = value;
			break;
		case Int32OptionNames.NumImpostors:
			NumImpostors = value;
			break;
		case Int32OptionNames.NumCommonTasks:
			NumCommonTasks = value;
			break;
		case Int32OptionNames.NumShortTasks:
			NumShortTasks = value;
			break;
		case Int32OptionNames.NumLongTasks:
			NumLongTasks = value;
			break;
		case Int32OptionNames.NumEmergencyMeetings:
			NumEmergencyMeetings = value;
			break;
		case Int32OptionNames.TaskBarMode:
			TaskBarMode = (TaskBarMode)value;
			break;
		case Int32OptionNames.MaxPlayers:
			MaxPlayers = value;
			break;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} could not set int named {optionName}");
			break;
		}
	}

	public void SetUInt(UInt32OptionNames optionName, uint value)
	{
		if (optionName == UInt32OptionNames.Keywords)
		{
			Keywords = (GameKeywords)value;
		}
		else
		{
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} could not set uint named {optionName}");
		}
	}

	public byte GetByte(ByteOptionNames optionName)
	{
		if (TryGetByte(optionName, out var value))
		{
			return value;
		}
		return 0;
	}

	public float GetFloat(FloatOptionNames optionName)
	{
		if (TryGetFloat(optionName, out var value))
		{
			return value;
		}
		return 0f;
	}

	public bool GetBool(BoolOptionNames optionName)
	{
		if (TryGetBool(optionName, out var value))
		{
			return value;
		}
		return false;
	}

	public int GetInt(Int32OptionNames optionName)
	{
		if (TryGetInt(optionName, out var value))
		{
			return value;
		}
		return 0;
	}

	public int[] GetIntArray(Int32ArrayOptionNames optionName)
	{
		if (TryGetIntArray(optionName, out var value))
		{
			return value;
		}
		return null;
	}

	public float[] GetFloatArray(FloatArrayOptionNames optionName)
	{
		if (TryGetFloatArray(optionName, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TryGetByte(ByteOptionNames optionName, out byte value)
	{
		value = 0;
		if (optionName == ByteOptionNames.MapId)
		{
			value = MapId;
			return true;
		}
		logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have a byte named {optionName}");
		return false;
	}

	public bool TryGetFloat(FloatOptionNames optionName, out float value)
	{
		value = 0f;
		switch (optionName)
		{
		case FloatOptionNames.KillCooldown:
			value = KillCooldown;
			return true;
		case FloatOptionNames.PlayerSpeedMod:
			value = PlayerSpeedMod;
			return true;
		case FloatOptionNames.ImpostorLightMod:
			value = ImpostorLightMod;
			return true;
		case FloatOptionNames.CrewLightMod:
			value = CrewLightMod;
			return true;
		case FloatOptionNames.ShapeshifterCooldown:
			value = roleOptions.ShapeshifterCooldown;
			return true;
		case FloatOptionNames.ShapeshifterDuration:
			value = roleOptions.ShapeshifterDuration;
			return true;
		case FloatOptionNames.ScientistCooldown:
			value = roleOptions.ScientistCooldown;
			return true;
		case FloatOptionNames.ScientistBatteryCharge:
			value = roleOptions.ScientistBatteryCharge;
			return true;
		case FloatOptionNames.ProtectionDurationSeconds:
			value = roleOptions.ProtectionDurationSeconds;
			return true;
		case FloatOptionNames.GuardianAngelCooldown:
			value = roleOptions.GuardianAngelCooldown;
			return true;
		case FloatOptionNames.EngineerCooldown:
			value = roleOptions.EngineerCooldown;
			return true;
		case FloatOptionNames.EngineerInVentMaxTime:
			value = roleOptions.EngineerInVentMaxTime;
			return true;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have a float named {optionName}");
			return false;
		}
	}

	public bool TryGetBool(BoolOptionNames optionName, out bool value)
	{
		value = false;
		switch (optionName)
		{
		case BoolOptionNames.VisualTasks:
			value = VisualTasks;
			return true;
		case BoolOptionNames.ConfirmImpostor:
			value = ConfirmImpostor;
			return true;
		case BoolOptionNames.AnonymousVotes:
			value = AnonymousVotes;
			return true;
		case BoolOptionNames.GhostsDoTasks:
			value = GhostsDoTasks;
			return true;
		case BoolOptionNames.IsDefaults:
			value = IsDefaults;
			return true;
		case BoolOptionNames.ImpostorsCanSeeProtect:
			value = roleOptions.ImpostorsCanSeeProtect;
			return true;
		case BoolOptionNames.ShapeshifterLeaveSkin:
			value = roleOptions.ShapeshifterLeaveSkin;
			return true;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have a bool named {optionName}");
			return false;
		}
	}

	public bool TryGetInt(Int32OptionNames optionName, out int value)
	{
		value = 0;
		switch (optionName)
		{
		case Int32OptionNames.KillDistance:
			value = KillDistance;
			return true;
		case Int32OptionNames.NumEmergencyMeetings:
			value = NumEmergencyMeetings;
			return true;
		case Int32OptionNames.EmergencyCooldown:
			value = EmergencyCooldown;
			return true;
		case Int32OptionNames.DiscussionTime:
			value = DiscussionTime;
			return true;
		case Int32OptionNames.VotingTime:
			value = VotingTime;
			return true;
		case Int32OptionNames.NumCommonTasks:
			value = NumCommonTasks;
			return true;
		case Int32OptionNames.NumShortTasks:
			value = NumShortTasks;
			return true;
		case Int32OptionNames.NumLongTasks:
			value = NumLongTasks;
			return true;
		case Int32OptionNames.NumImpostors:
			value = NumImpostors;
			return true;
		case Int32OptionNames.TaskBarMode:
			value = (int)TaskBarMode;
			return true;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have an int (int32) named {optionName}");
			return false;
		}
	}

	public bool TryGetIntArray(Int32ArrayOptionNames optionName, out int[] value)
	{
		value = null;
		switch (optionName)
		{
		case Int32ArrayOptionNames.MaxImpostors:
			value = MaxImpostors;
			return true;
		case Int32ArrayOptionNames.MinPlayers:
			value = MinPlayers;
			return true;
		default:
			logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have an int array (int32) named {optionName}");
			return false;
		}
	}

	public bool TryGetFloatArray(FloatArrayOptionNames optionName, out float[] value)
	{
		value = null;
		if (optionName == FloatArrayOptionNames.KillDistances)
		{
			value = KillDistances;
			return true;
		}
		logger.WriteError($"Options for Mode {GameMode}, Version {Version} does not have an float array named {optionName}");
		return false;
	}
}
