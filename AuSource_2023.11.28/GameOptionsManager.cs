using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;

public class GameOptionsManager
{
	private const int CurrentGameOptionsVersion = 7;

	private readonly Dictionary<int, IGameOptionsMigrator> optionsMigrators;

	private readonly GameOptionsFactory gameOptionsFactory;

	private readonly ILogger logger;

	private GameModes currentGameMode;

	private IGameOptions currentGameOptions;

	private NormalGameOptionsV07 currentNormalGameOptions;

	private HideNSeekGameOptionsV07 currentHideNSeekGameOptions;

	private IGameOptions currentHostOptions;

	private NormalGameOptionsV07 normalGameHostOptions;

	private HideNSeekGameOptionsV07 hideNSeekGameHostOptions;

	private IGameOptions currentSearchOptions;

	private NormalGameOptionsV07 normalGameSearchOptions;

	private HideNSeekGameOptionsV07 hideNSeekGameSearchOptions;

	public static GameOptionsManager Instance { get; private set; }

	public bool HasOptions => currentGameOptions != null;

	public IGameOptions CurrentGameOptions
	{
		get
		{
			if (currentGameOptions == null)
			{
				logger.WriteError("CurrentGameOptions was null on get");
				CurrentGameOptions = GameHostOptions;
			}
			return currentGameOptions;
		}
		set
		{
			GameModes gameMode = value.GameMode;
			SwitchGameMode(gameMode);
			switch (gameMode)
			{
			case GameModes.Normal:
				currentNormalGameOptions = value as NormalGameOptionsV07;
				if (currentNormalGameOptions == null)
				{
					logger.WriteError($"Setting current game mode as {gameMode} but options data type is not of type {typeof(NormalGameOptionsV07)}");
				}
				break;
			case GameModes.HideNSeek:
				currentHideNSeekGameOptions = value as HideNSeekGameOptionsV07;
				if (currentHideNSeekGameOptions == null)
				{
					logger.WriteError($"Setting current game mode as {gameMode} but options data type is not of type {typeof(HideNSeekGameOptionsV07)}");
				}
				break;
			default:
				logger.WriteError($"Cannot cache settings for game mode {gameMode}");
				break;
			}
			currentGameOptions = value;
		}
	}

	public IGameOptions GameHostOptions
	{
		get
		{
			return currentHostOptions;
		}
		set
		{
			GameModes gameMode = value.GameMode;
			SwitchGameMode(gameMode);
			switch (gameMode)
			{
			case GameModes.Normal:
				normalGameHostOptions = value as NormalGameOptionsV07;
				if (normalGameHostOptions == null)
				{
					logger.WriteError($"Setting host options game mode as {gameMode} but options data type is not of type {typeof(NormalGameOptionsV07)}");
				}
				else
				{
					SaveNormalHostOptions();
				}
				break;
			case GameModes.HideNSeek:
				hideNSeekGameHostOptions = value as HideNSeekGameOptionsV07;
				if (hideNSeekGameHostOptions == null)
				{
					logger.WriteError($"Setting host options game mode as {gameMode} but options data type is not of type {typeof(HideNSeekGameOptionsV07)}");
				}
				else
				{
					SaveHideNSeekHostOptions();
				}
				break;
			default:
				logger.WriteError($"Cannot cache settings for host options game mode {gameMode}");
				break;
			}
			currentHostOptions = value;
		}
	}

	public IGameOptions GameSearchOptions
	{
		get
		{
			return currentSearchOptions;
		}
		set
		{
			GameModes gameMode = value.GameMode;
			SwitchGameMode(gameMode);
			switch (gameMode)
			{
			case GameModes.Normal:
				normalGameSearchOptions = value as NormalGameOptionsV07;
				if (normalGameSearchOptions == null)
				{
					logger.WriteError($"Setting search options game mode as {gameMode} but options data type is not of type {typeof(NormalGameOptionsV07)}");
				}
				else
				{
					SaveNormalSearchOptions();
				}
				break;
			case GameModes.HideNSeek:
				hideNSeekGameSearchOptions = value as HideNSeekGameOptionsV07;
				if (hideNSeekGameSearchOptions == null)
				{
					logger.WriteError($"Setting search options game mode as {gameMode} but options data type is not of type {typeof(HideNSeekGameOptionsV07)}");
				}
				else
				{
					SaveHideNSeekSearchOptions();
				}
				break;
			default:
				logger.WriteError($"Cannot cache settings for search options game mode {gameMode}");
				break;
			}
			currentSearchOptions = value;
		}
	}

	public GameOptionsManager(ILogger logger)
	{
		if (Instance != null)
		{
			throw new Exception("CANNOT HAVE MORE THAN ONE OPTIONS MANAGER INSTANCE!");
		}
		Instance = this;
		gameOptionsFactory = new GameOptionsFactory(logger);
		this.logger = logger;
		optionsMigrators = new Dictionary<int, IGameOptionsMigrator> { 
		{
			6,
			new GameOptionsMigratorV06toV07(logger)
		} };
	}

	public void Initialize()
	{
		LoadOrCreateNormalGameHostOptions();
		LoadOrCreateNormalGameSearchOptions();
		LoadOrCreateHideNSeekGameHostOptions();
		LoadOrCreateHideNSeekGameSearchOptions();
		SwitchGameMode(GameModes.Normal);
	}

	public void SwitchGameMode(GameModes gameMode)
	{
		switch (gameMode)
		{
		case GameModes.Normal:
			currentHostOptions = normalGameHostOptions;
			currentSearchOptions = normalGameSearchOptions;
			currentGameOptions = currentNormalGameOptions;
			break;
		case GameModes.HideNSeek:
			currentHostOptions = hideNSeekGameHostOptions;
			currentSearchOptions = hideNSeekGameSearchOptions;
			currentGameOptions = currentHideNSeekGameOptions;
			break;
		default:
			logger.WriteError($"Cannot switch game mode options to {gameMode} - mode does not exist");
			return;
		}
		currentGameMode = gameMode;
		logger.WriteInfo($"Game options game mode switched to {gameMode}");
	}

	private void LoadOrCreateNormalGameHostOptions()
	{
		byte[] rawNormalHostOptions = DataManager.Settings.Multiplayer.RawNormalHostOptions;
		IGameOptions legacyGameOptions = null;
		if (rawNormalHostOptions == null || rawNormalHostOptions.Length == 0)
		{
			if (!TryLoadLegacyOptionsFile("gameHostOptions", out legacyGameOptions))
			{
				normalGameHostOptions = new NormalGameOptionsV07(logger);
				SaveNormalHostOptions();
				return;
			}
		}
		else
		{
			legacyGameOptions = gameOptionsFactory.FromBytes(rawNormalHostOptions);
		}
		if (legacyGameOptions.Version < 7)
		{
			normalGameHostOptions = MigrateNormalGameOptions(legacyGameOptions);
			SaveNormalHostOptions();
		}
		else
		{
			normalGameHostOptions = legacyGameOptions as NormalGameOptionsV07;
		}
	}

	private void LoadOrCreateHideNSeekGameHostOptions()
	{
		byte[] rawHideNSeekHostOptions = DataManager.Settings.Multiplayer.RawHideNSeekHostOptions;
		if (rawHideNSeekHostOptions == null || rawHideNSeekHostOptions.Length == 0)
		{
			logger.WriteInfo("No saved HideNSeek game host options found, initializing to default values");
			hideNSeekGameHostOptions = new HideNSeekGameOptionsV07(logger);
			SaveHideNSeekHostOptions();
			return;
		}
		IGameOptions gameOptions = gameOptionsFactory.FromBytes(rawHideNSeekHostOptions);
		if (gameOptions == null)
		{
			logger.WriteInfo("Data was null after trying to deserialize- initializing to default values");
			hideNSeekGameHostOptions = new HideNSeekGameOptionsV07(logger);
			SaveHideNSeekHostOptions();
		}
		else if (gameOptions.Version < 7)
		{
			hideNSeekGameHostOptions = MigrateHideNSeekGameOptions(gameOptions);
			SaveHideNSeekHostOptions();
		}
		else
		{
			hideNSeekGameHostOptions = gameOptions as HideNSeekGameOptionsV07;
		}
	}

	private void LoadOrCreateNormalGameSearchOptions()
	{
		byte[] rawNormalSearchOptions = DataManager.Settings.Multiplayer.RawNormalSearchOptions;
		IGameOptions legacyGameOptions;
		if (rawNormalSearchOptions == null || rawNormalSearchOptions.Length == 0)
		{
			if (!TryLoadLegacyOptionsFile("gameSearchOptions", out legacyGameOptions))
			{
				logger.WriteInfo("No saved Normal game search options found, initializing to default values");
				normalGameSearchOptions = new NormalGameOptionsV07(logger);
				SaveNormalSearchOptions();
				return;
			}
		}
		else
		{
			legacyGameOptions = gameOptionsFactory.FromBytes(rawNormalSearchOptions);
		}
		if (legacyGameOptions.Version < 7)
		{
			normalGameSearchOptions = MigrateNormalGameOptions(legacyGameOptions);
			SaveNormalSearchOptions();
		}
		else
		{
			normalGameSearchOptions = legacyGameOptions as NormalGameOptionsV07;
		}
	}

	private void LoadOrCreateHideNSeekGameSearchOptions()
	{
		byte[] rawHideNSeekSearchOptions = DataManager.Settings.Multiplayer.RawHideNSeekSearchOptions;
		if (rawHideNSeekSearchOptions == null || rawHideNSeekSearchOptions.Length == 0)
		{
			logger.WriteInfo("No saved HideNSeek game search options found, initializing to default values");
			hideNSeekGameSearchOptions = new HideNSeekGameOptionsV07(logger);
			SaveHideNSeekSearchOptions();
			return;
		}
		IGameOptions gameOptions = gameOptionsFactory.FromBytes(rawHideNSeekSearchOptions);
		if (gameOptions == null)
		{
			logger.WriteInfo("Data was null after trying to deserialize- initializing to default values");
			hideNSeekGameSearchOptions = new HideNSeekGameOptionsV07(logger);
			SaveHideNSeekSearchOptions();
		}
		else if (gameOptions.Version < 7)
		{
			hideNSeekGameSearchOptions = MigrateHideNSeekGameOptions(gameOptions);
			SaveHideNSeekSearchOptions();
		}
		else
		{
			hideNSeekGameSearchOptions = gameOptions as HideNSeekGameOptionsV07;
		}
	}

	private bool TryLoadLegacyOptionsFile(string filename, out IGameOptions legacyGameOptions)
	{
		legacyGameOptions = null;
		string path = Path.Combine(PlatformPaths.persistentDataPath, filename);
		if (!File.Exists(path))
		{
			logger.WriteInfo("No legacy game options " + filename + " options found, initializing to default values");
			return false;
		}
		using (FileStream input = File.OpenRead(path))
		{
			using BinaryReader reader = new BinaryReader(input);
			legacyGameOptions = GameOptionsData.Deserialize(reader, 6, logger);
		}
		File.Delete(path);
		return true;
	}

	private void SaveNormalHostOptions()
	{
		byte[] rawNormalHostOptions = gameOptionsFactory.ToBytes(normalGameHostOptions);
		DataManager.Settings.Multiplayer.RawNormalHostOptions = rawNormalHostOptions;
		DataManager.Settings.Save();
		logger.WriteInfo($"Saved host options for game mode {normalGameHostOptions.GameMode} with version {normalGameHostOptions.Version}");
	}

	private void SaveNormalSearchOptions()
	{
		byte[] rawNormalSearchOptions = gameOptionsFactory.ToBytes(normalGameSearchOptions);
		DataManager.Settings.Multiplayer.RawNormalSearchOptions = rawNormalSearchOptions;
		DataManager.Settings.Save();
		logger.WriteInfo($"Saved search options for game mode {normalGameSearchOptions.GameMode} with version {normalGameSearchOptions.Version}");
	}

	private void SaveHideNSeekHostOptions()
	{
		byte[] rawHideNSeekHostOptions = gameOptionsFactory.ToBytes(hideNSeekGameHostOptions);
		DataManager.Settings.Multiplayer.RawHideNSeekHostOptions = rawHideNSeekHostOptions;
		DataManager.Settings.Save();
		logger.WriteInfo($"Saved host options for game mode {hideNSeekGameHostOptions.GameMode} with version {hideNSeekGameHostOptions.Version}");
	}

	private void SaveHideNSeekSearchOptions()
	{
		byte[] rawHideNSeekSearchOptions = gameOptionsFactory.ToBytes(hideNSeekGameSearchOptions);
		DataManager.Settings.Multiplayer.RawHideNSeekSearchOptions = rawHideNSeekSearchOptions;
		DataManager.Settings.Save();
		logger.WriteInfo($"Saved search options for game mode {hideNSeekGameSearchOptions.GameMode} with version {hideNSeekGameSearchOptions.Version}");
	}

	private NormalGameOptionsV07 MigrateNormalGameOptions(IGameOptions gameOptions)
	{
		logger.WriteInfo($"Migrating game options for mode {gameOptions.GameMode}");
		while (gameOptions.Version != 7)
		{
			logger.WriteInfo($"Attempting to migrating game options for mode {gameOptions.GameMode} from version {gameOptions.Version} to {gameOptions.Version + 1}");
			if (!optionsMigrators.TryGetValue(gameOptions.Version, out var value))
			{
				logger.WriteError($"Failed to migrating game options for mode {gameOptions.GameMode} from version {gameOptions.Version} to {gameOptions.Version + 1} - no migrator available");
				return new NormalGameOptionsV07(logger);
			}
			if (!value.TryMigrate(gameOptions as GameOptionsData, out var updatedVersion))
			{
				logger.WriteError($"Failed to migrating game options for mode {gameOptions.GameMode} from version {gameOptions.Version} to {gameOptions.Version + 1} - conversion failed");
				return new NormalGameOptionsV07(logger);
			}
			gameOptions = updatedVersion;
		}
		return gameOptions as NormalGameOptionsV07;
	}

	private HideNSeekGameOptionsV07 MigrateHideNSeekGameOptions(IGameOptions gameOptions)
	{
		logger.WriteError($"Failed to migrating game options for mode {gameOptions.GameMode} from version {gameOptions.Version} to {gameOptions.Version + 1} - hideNseek options only have 1 version");
		return new HideNSeekGameOptionsV07(logger);
	}
}
