using System;
using System.Collections.Generic;
using System.IO;
using AmongUs.GameOptions;
using Innersloth.IO;
using UnityEngine;

public class StatsManager
{
	public class Stats
	{
		public float banPoints;

		public long lastGameStarted;

		public float FastestCrewmateWin_HideAndSeek = float.NaN;

		public float FastestImpostorWin_HideAndSeek = float.NaN;

		public readonly Dictionary<StringNames, uint> gameplayStats = new Dictionary<StringNames, uint>();

		private readonly Dictionary<GameOverReason, uint> winReasons = new Dictionary<GameOverReason, uint>();

		private readonly Dictionary<GameOverReason, uint> loseReasons = new Dictionary<GameOverReason, uint>();

		private readonly Dictionary<GameOverReason, uint> drawReasons = new Dictionary<GameOverReason, uint>();

		private readonly Dictionary<MapNames, uint> mapWins = new Dictionary<MapNames, uint>();

		private readonly Dictionary<RoleTypes, uint> roleWins = new Dictionary<RoleTypes, uint>();

		private static readonly List<StringNames> SimpleStats = new List<StringNames>
		{
			StringNames.StatsBodiesReported,
			StringNames.StatsEmergenciesCalled,
			StringNames.StatsTasksCompleted,
			StringNames.StatsAllTasksCompleted,
			StringNames.StatsSabotagesFixed,
			StringNames.StatsImpostorKills,
			StringNames.StatsTimesMurdered,
			StringNames.StatsTimesEjected,
			StringNames.StatsCrewmateStreak,
			StringNames.StatsGamesImpostor,
			StringNames.StatsGamesCrewmate,
			StringNames.StatsGamesStarted,
			StringNames.StatsGamesFinished,
			StringNames.StatsEngineerVents,
			StringNames.StatsGuardianAngelCrewmatesProtected,
			StringNames.StatsScientistChargesGained,
			StringNames.StatsShapeshifterShiftedKills,
			StringNames.StatsHidenSeekGamesCrewmateSurvived,
			StringNames.StatsHidenSeekTimesVented,
			StringNames.StatsTimesPettedPet,
			StringNames.StatsImpostorKills_HideAndSeek
		};

		public void AddDrawReason(GameOverReason reason)
		{
			if (!drawReasons.ContainsKey(reason))
			{
				drawReasons[reason] = 0u;
			}
			drawReasons[reason]++;
		}

		public void AddWinReason(GameOverReason reason)
		{
			if (!winReasons.ContainsKey(reason))
			{
				winReasons[reason] = 0u;
			}
			winReasons[reason]++;
		}

		public uint GetWinReason(GameOverReason reason)
		{
			if (!winReasons.ContainsKey(reason))
			{
				winReasons[reason] = 0u;
			}
			return winReasons[reason];
		}

		public void AddLoseReason(GameOverReason reason)
		{
			if (!loseReasons.ContainsKey(reason))
			{
				loseReasons[reason] = 0u;
			}
			loseReasons[reason]++;
		}

		public uint GetLoseReason(GameOverReason reason)
		{
			if (!loseReasons.ContainsKey(reason))
			{
				loseReasons[reason] = 0u;
			}
			return loseReasons[reason];
		}

		public void AddMapWin(int mapIdx)
		{
			if (!mapWins.ContainsKey((MapNames)mapIdx))
			{
				mapWins[(MapNames)mapIdx] = 0u;
			}
			mapWins[(MapNames)mapIdx]++;
		}

		public void AddRoleWin(RoleTypes role)
		{
			if (!roleWins.ContainsKey(role))
			{
				roleWins[role] = 0u;
			}
			roleWins[role]++;
		}

		public uint GetMapWinCount(MapNames mapName)
		{
			if (!mapWins.ContainsKey(mapName))
			{
				mapWins[mapName] = 0u;
			}
			return mapWins[mapName];
		}

		public uint GetRoleWinCount(RoleTypes role)
		{
			if (!roleWins.ContainsKey(role))
			{
				roleWins[role] = 0u;
			}
			return roleWins[role];
		}

		public bool CheckUpdateCrewmateMinVictoryTime(float timeInSeconds)
		{
			if (float.IsNaN(FastestCrewmateWin_HideAndSeek) || FastestCrewmateWin_HideAndSeek > timeInSeconds)
			{
				FastestCrewmateWin_HideAndSeek = timeInSeconds;
				return true;
			}
			return false;
		}

		public bool CheckUpdateImpostorMinVictoryTime(float timeInSeconds)
		{
			if (float.IsNaN(FastestImpostorWin_HideAndSeek) || FastestImpostorWin_HideAndSeek > timeInSeconds)
			{
				FastestImpostorWin_HideAndSeek = timeInSeconds;
				return true;
			}
			return false;
		}

		public void MigrateFromOldFile(BinaryReader reader)
		{
			byte b = reader.ReadByte();
			gameplayStats[StringNames.StatsBodiesReported] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsEmergenciesCalled] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsTasksCompleted] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsAllTasksCompleted] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsSabotagesFixed] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsImpostorKills] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsTimesMurdered] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsTimesEjected] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsCrewmateStreak] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsGamesImpostor] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsGamesCrewmate] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsGamesStarted] = reader.ReadUInt32();
			gameplayStats[StringNames.StatsGamesFinished] = reader.ReadUInt32();
			for (int i = 0; i < 7; i++)
			{
				winReasons[(GameOverReason)i] = reader.ReadUInt32();
			}
			for (int j = 0; j < 7; j++)
			{
				loseReasons[(GameOverReason)j] = reader.ReadUInt32();
			}
			if (b > 1)
			{
				for (int k = 0; k < 7; k++)
				{
					drawReasons[(GameOverReason)k] = reader.ReadUInt32();
				}
			}
			if (b > 2)
			{
				banPoints = reader.ReadSingle();
				lastGameStarted = reader.ReadInt64();
			}
			if (b > 3)
			{
				int num = reader.ReadInt32();
				for (int l = 0; l < num; l++)
				{
					mapWins[(MapNames)l] = reader.ReadUInt32();
				}
			}
		}

		public void Reset()
		{
			gameplayStats.Clear();
			winReasons.Clear();
			loseReasons.Clear();
			drawReasons.Clear();
			mapWins.Clear();
			banPoints = 0f;
			lastGameStarted = 0L;
			FastestImpostorWin_HideAndSeek = float.NaN;
			FastestCrewmateWin_HideAndSeek = float.NaN;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write((byte)5);
			uint count = (uint)SimpleStats.Count;
			writer.Write(count);
			foreach (StringNames simpleStat in SimpleStats)
			{
				if (gameplayStats.TryGetValue(simpleStat, out var value))
				{
					writer.Write(value);
				}
				else
				{
					writer.Write(0u);
				}
			}
			WriteEnumDict(winReasons, writer);
			WriteEnumDict(loseReasons, writer);
			WriteEnumDict(drawReasons, writer);
			WriteEnumDict(mapWins, writer);
			WriteEnumDict(roleWins, writer);
			writer.Write(FastestCrewmateWin_HideAndSeek);
			writer.Write(FastestImpostorWin_HideAndSeek);
		}

		public void Deserialize(BinaryReader reader)
		{
			reader.ReadByte();
			int num = (int)reader.ReadUInt32();
			for (int i = 0; i < num; i++)
			{
				StringNames key = SimpleStats[i];
				gameplayStats[key] = reader.ReadUInt32();
			}
			for (int j = num; j < SimpleStats.Count; j++)
			{
				StringNames key2 = SimpleStats[j];
				gameplayStats[key2] = 0u;
			}
			ReadEnumDict(winReasons, reader);
			ReadEnumDict(loseReasons, reader);
			ReadEnumDict(drawReasons, reader);
			ReadEnumDict(mapWins, reader);
			ReadEnumDict(roleWins, reader);
			TryReadFloat(ref FastestCrewmateWin_HideAndSeek, reader);
			TryReadFloat(ref FastestImpostorWin_HideAndSeek, reader);
		}

		private void WriteEnumDict<T>(Dictionary<T, uint> enumDict, BinaryWriter writer) where T : Enum
		{
			uint num = (uint)Enum.GetNames(typeof(T)).Length;
			writer.Write(num);
			for (int i = 0; i < num; i++)
			{
				T key = (T)Enum.Parse(typeof(T), i.ToString());
				if (enumDict.TryGetValue(key, out var value))
				{
					writer.Write(value);
				}
				else
				{
					writer.Write(0);
				}
			}
		}

		private void ReadEnumDict<T>(Dictionary<T, uint> enumDict, BinaryReader reader) where T : Enum
		{
			uint num = (uint)Enum.GetNames(typeof(T)).Length;
			int num2 = (int)reader.ReadUInt32();
			for (int i = 0; i < num2; i++)
			{
				T key = (T)Enum.Parse(typeof(T), i.ToString());
				enumDict[key] = reader.ReadUInt32();
			}
			for (int j = num2; j < num; j++)
			{
				T key2 = (T)Enum.Parse(typeof(T), j.ToString());
				enumDict[key2] = 0u;
			}
		}

		private void TryReadFloat(ref float value, BinaryReader reader)
		{
			if (reader.BaseStream.Position >= reader.BaseStream.Length)
			{
				return;
			}
			try
			{
				value = reader.ReadSingle();
			}
			catch
			{
			}
		}
	}

	public static StatsManager Instance = new StatsManager();

	private const string SaveFileName = "playerStats3";

	private const int PointsUntilBanStarts = 2;

	private const int MinutesPerBanPoint = 5;

	private const byte StatsVersion = 5;

	protected Stats stats;

	private bool loadedStats;

	private readonly Logger logger = new Logger("StatsManager", Logger.Level.Info, Logger.Category.Gameplay);

	private Stats TheStats
	{
		get
		{
			LoadStats();
			return stats;
		}
	}

	public float BanPoints
	{
		get
		{
			LoadStats();
			return TheStats.banPoints;
		}
		set
		{
			LoadStats();
			if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
			{
				TheStats.banPoints = Mathf.Max(0f, value);
			}
			SaveStats();
		}
	}

	public DateTime LastGameStarted
	{
		get
		{
			LoadStats();
			return new DateTime(TheStats.lastGameStarted);
		}
		set
		{
			LoadStats();
			TheStats.lastGameStarted = value.Ticks;
			SaveStats();
		}
	}

	public float BanMinutes => Mathf.Max(BanPoints - 2f, 0f) * 5f;

	public bool AmBanned => BanMinutesLeft > 0;

	public int BanMinutesLeft
	{
		get
		{
			TimeSpan timeSpan = DateTime.UtcNow - LastGameStarted;
			int num = Mathf.CeilToInt(BanMinutes - (float)timeSpan.TotalMinutes);
			if (num > 1440 || timeSpan.TotalDays > 1.0)
			{
				BanPoints = 0f;
				return 0;
			}
			return num;
		}
	}

	public void IncrementStat(StringNames statName)
	{
		if (!TheStats.gameplayStats.ContainsKey(statName))
		{
			TheStats.gameplayStats[statName] = 0u;
		}
		logger.Info($"Increasing stat {statName} by 1");
		TheStats.gameplayStats[statName]++;
		SaveStats();
	}

	public void ResetStat(StringNames statName)
	{
		if (TheStats.gameplayStats.ContainsKey(statName))
		{
			logger.Info($"Resetting stat {statName} to 0");
			TheStats.gameplayStats[statName] = 0u;
		}
		SaveStats();
	}

	public uint GetStat(StringNames statName)
	{
		if (!TheStats.gameplayStats.TryGetValue(statName, out var value))
		{
			return 0u;
		}
		return value;
	}

	public void AddDrawReason(GameOverReason reason)
	{
		LoadStats();
		TheStats.AddDrawReason(reason);
		SaveStats();
	}

	public void AddWinReason(GameOverReason reason, int mapIdx, RoleTypes role)
	{
		LoadStats();
		TheStats.AddWinReason(reason);
		TheStats.AddMapWin(mapIdx);
		TheStats.AddRoleWin(role);
		SaveStats();
	}

	public uint GetWinReason(GameOverReason reason)
	{
		LoadStats();
		return TheStats.GetWinReason(reason);
	}

	public uint GetMapWinCount(MapNames map)
	{
		LoadStats();
		return stats.GetMapWinCount(map);
	}

	public uint GetRoleWinCount(RoleTypes role)
	{
		LoadStats();
		return stats.GetRoleWinCount(role);
	}

	public void AddLoseReason(GameOverReason reason)
	{
		LoadStats();
		TheStats.AddLoseReason(reason);
		SaveStats();
	}

	public uint GetLoseReason(GameOverReason reason)
	{
		LoadStats();
		return TheStats.GetLoseReason(reason);
	}

	public void UpdateHideAndSeekImpostorWinTimer(float timeInSeconds)
	{
		LoadStats();
		if (TheStats.CheckUpdateImpostorMinVictoryTime(timeInSeconds))
		{
			logger.Info($"Updating hns impostor win time record to {timeInSeconds}");
			SaveStats();
		}
	}

	public float GetFastestHideAndSeekImpostorWin()
	{
		LoadStats();
		return TheStats.FastestImpostorWin_HideAndSeek;
	}

	public float GetFastestHideAndSeekCrewmateWin()
	{
		LoadStats();
		return TheStats.FastestCrewmateWin_HideAndSeek;
	}

	public void UpdateHideAndSeekCrewmateWinTimer(float timeInSeconds)
	{
		LoadStats();
		if (TheStats.CheckUpdateCrewmateMinVictoryTime(timeInSeconds))
		{
			logger.Info($"Updating hns crewmate win time record to {timeInSeconds}");
			SaveStats();
		}
	}

	public void ForceLoadStats()
	{
		loadedStats = false;
	}

	protected virtual void LoadStats()
	{
		if (loadedStats)
		{
			return;
		}
		loadedStats = true;
		logger.Info("LoadStats");
		stats = new Stats();
		string path = Path.Combine(PlatformPaths.persistentDataPath, "playerStats2");
		string path2 = Path.Combine(PlatformPaths.persistentDataPath, "playerStats3");
		if (FileIO.Exists(path))
		{
			try
			{
				using BinaryReader reader = new BinaryReader(File.OpenRead(path));
				stats = new Stats();
				stats.MigrateFromOldFile(reader);
				SaveStats();
				logger.Info("Stats migration successful, deleting old file");
				FileIO.Delete(path);
				return;
			}
			catch
			{
				logger.Error("Deleting corrupted stats file");
				FileIO.Delete(path);
				return;
			}
		}
		if (FileIO.Exists(path2))
		{
			try
			{
				using BinaryReader reader2 = new BinaryReader(File.OpenRead(path2));
				stats.Deserialize(reader2);
				return;
			}
			catch
			{
				logger.Error("Deleting corrupted stats file");
				FileIO.Delete(path2);
				return;
			}
		}
		stats = new Stats();
		ResetStatDisplay();
	}

	protected virtual void SaveStats()
	{
		logger.Info("SaveStats");
		try
		{
			using BinaryWriter writer = new BinaryWriter(File.OpenWrite(Path.Combine(PlatformPaths.persistentDataPath, "playerStats3")));
			TheStats.Serialize(writer);
		}
		catch (Exception ex)
		{
			logger.Error("Failed to write out stats: " + ex.Message);
		}
	}

	private void ResetStatDisplay()
	{
		TheStats.Reset();
	}
}
