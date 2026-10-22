using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Stats;
using Steamworks;
using UnityEngine;

public class AchievementManager : DestroyableSingleton<AchievementManager>
{
	private struct AchievementProgressValue(string achievementName, int achievementMaxProgress)
	{
		public string name = achievementName;

		public int maxProgress = achievementMaxProgress;
	}

	private const string TasksCompleteEasyKey = "task_complete_easy";

	private const int TasksCompleteEasy = 10;

	private const string TasksCompleteMediumKey = "task_complete_medium";

	private const int TasksCompleteMedium = 100;

	private const string TasksCompleteHardKey = "task_complete_hard";

	private const int TasksCompleteHard = 500;

	private const string CardSwipeFirstTryKey = "card_first_try";

	private const string KillDuringLightsKey = "kill_during_lights";

	private const string NoVentsImpostorWinKey = "no_vents_impostor_win";

	private const string FirstKillKey = "kills_first";

	private const int KillsFirst = 1;

	private const string KillsEasyKey = "kills_easy";

	private const int KillsEasy = 5;

	private const string KillsMediumKey = "kills_medium";

	private const int KillsMedium = 50;

	private const string KillsHardKey = "kills_hard";

	private const int KillsHard = 150;

	private const int MapWins = 3;

	private const string SkeldWinsKey = "wins_skeld";

	private const string MiraWinsKey = "wins_mira";

	private const string PolusWinsKey = "wins_polus";

	private const string AirshipWinsKey = "wins_airship";

	private const string TwoImpsSurviveKey = "survive_two_impostors";

	private const string TasksWinKey = "win_all_tasks";

	private const string SabotageWinKey = "win_sabotage";

	private const string ImpostorKillWinKey = "win_kills";

	private const string ImpostorVoteWinKey = "win_impostor_vote";

	private const string CrewmateSurviveWinKey = "survive_crewmate";

	private const string DieDuringMedScanKey = "die_during_medscan";

	private const string FixOwnSabotageKey = "fix_own_sabotage";

	private const string ThreeKillsBeforeMeetingKey = "three_kills_before_meeting";

	private const string WinAlwaysCorrectVotesKey = "win_always_correct_votes";

	private const string KillWhileDisguisedKey = "kill_while_shifted";

	private const string BlockKillAsGuardianAngelKey = "block_kill_as_guardian";

	private const string StatNameImpostorKills = "impostorKills";

	private const string StatNameTasksCompleted = "tasksCompleted";

	private const string StatNameMapWinsSkeld = "MapWinsSkeld";

	private const string StatNameMapWinsPolus = "MapWinsPolus";

	private const string StatNameMapWinsMira = "MapWinsMira";

	private const string StatNameMapWinsAirship = "MapWinsAirship";

	private const string HnSCrewmateSurviveWinKey = "hns_survive_crewmate";

	private const string HnSImpostorKillAllKey = "hns_impostor_kill_all";

	private const string HnSCompleteAllTasksKey = "hns_complete_all_tasks";

	private const string HnSCrewmateWinsEasyKey = "hns_crewmate_wins_easy";

	private const int HnSCrewmateWinsEasy = 10;

	private const string HnSCrewmateWinsHardKey = "hns_crewmate_wins_hard";

	private const int HnSCrewmateWinsHard = 50;

	private const string HnSImpostorKillsEasyKey = "hns_impostor_kills_easy";

	private const int HnSImpostorKillsEasy = 100;

	private const string HnSImpostorKillsHardKey = "hns_impostor_kills_hard";

	private const int HnSImpostorKillsHard = 200;

	private const string StatNameHnSCrewmateWin = "hnsCrewmateWin";

	private const string StatNameHnSImpostorKill = "hnsImpostorKills";

	public static readonly Dictionary<string, List<GameModes>> AchievementGameModeKey = new Dictionary<string, List<GameModes>>
	{
		{
			"kills_first",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"kills_easy",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"kills_medium",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"kills_hard",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"task_complete_easy",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"task_complete_medium",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"task_complete_hard",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"wins_skeld",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"wins_mira",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"wins_polus",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"wins_airship",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"survive_two_impostors",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"win_all_tasks",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"win_sabotage",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"win_kills",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"win_impostor_vote",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"survive_crewmate",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"card_first_try",
			new List<GameModes>
			{
				GameModes.Normal,
				GameModes.HideNSeek
			}
		},
		{
			"kill_during_lights",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"die_during_medscan",
			new List<GameModes>
			{
				GameModes.Normal,
				GameModes.HideNSeek
			}
		},
		{
			"fix_own_sabotage",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"three_kills_before_meeting",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"win_always_correct_votes",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"no_vents_impostor_win",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"kill_while_shifted",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"block_kill_as_guardian",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"hns_survive_crewmate",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_impostor_kill_all",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_complete_all_tasks",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_crewmate_wins_easy",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_crewmate_wins_hard",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_impostor_kills_easy",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hns_impostor_kills_hard",
			new List<GameModes> { GameModes.HideNSeek }
		}
	};

	public static readonly Dictionary<string, List<GameModes>> StatsGameModeKey = new Dictionary<string, List<GameModes>>
	{
		{
			"impostorKills",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"tasksCompleted",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"MapWinsSkeld",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"MapWinsPolus",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"MapWinsMira",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"MapWinsAirship",
			new List<GameModes> { GameModes.Normal }
		},
		{
			"hnsCrewmateWin",
			new List<GameModes> { GameModes.HideNSeek }
		},
		{
			"hnsImpostorKills",
			new List<GameModes> { GameModes.HideNSeek }
		}
	};

	private readonly Logger logger = new Logger("AchievementManager", Logger.Level.Info, Logger.Category.Gameplay);

	private bool gameStarted;

	private RoleTypes myRole;

	private int cardSwipesThisMatch;

	private int ventsUsedThisMatch;

	private bool meetingCalledThisMatch;

	private int killsThisMatch;

	private bool sabotageCalledLocally;

	private int timesVoted;

	private int timesVotedForImp;

	private int winMap = -1;

	private Dictionary<string, List<AchievementProgressValue>> statName2AchievementProgressValues;

	public void UpdateAchievementProgressImpl(string key, int progress, int total)
	{
		if (progress >= total)
		{
			UnlockAchievementImpl(key);
		}
	}

	public void UnlockAchievementImpl(string key)
	{
		Debug.Log((object)("on achievement unlocked callback " + key));
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			if (!SteamManager.Initialized)
			{
				Debug.Log((object)"Steam not initialized");
				return;
			}
			SteamUserStats.SetAchievement(key);
			SteamUserStats.StoreStats();
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	private void InitializeAchievementProgressDictionary()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		statName2AchievementProgressValues = new Dictionary<string, List<AchievementProgressValue>>();
		uint achievementDefinitionCount = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().GetAchievementDefinitionCount(new GetAchievementDefinitionCountOptions());
		DefinitionV2 val2 = default;
		for (uint num = 0u; num < achievementDefinitionCount; num++)
		{
			CopyAchievementDefinitionV2ByIndexOptions val = new CopyAchievementDefinitionV2ByIndexOptions
			{
				AchievementIndex = num
			};
			if ((int)DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().CopyAchievementDefinitionV2ByIndex(val, ref val2) != 0 || val2.StatThresholds == null)
			{
				continue;
			}
			StatThresholds[] statThresholds = val2.StatThresholds;
			foreach (StatThresholds val3 in statThresholds)
			{
				if (statName2AchievementProgressValues.ContainsKey(val3.Name))
				{
					statName2AchievementProgressValues[val3.Name].Add(new AchievementProgressValue(val2.AchievementId, val3.Threshold));
					continue;
				}
				statName2AchievementProgressValues.Add(val3.Name, new List<AchievementProgressValue>
				{
					new AchievementProgressValue(val2.AchievementId, val3.Threshold)
				});
			}
		}
	}

	public void UpdateAchievementsAndStats()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected Obj, but got Unknown
		//IL_003d: Expected Obj, but got Unknown
		ProductUserId localUserId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().QueryDefinitions(new QueryDefinitionsOptions
		{
			LocalUserId = localUserId
		}, (object)null, (OnQueryDefinitionsCompleteCallback)((OnQueryDefinitionsCompleteCallbackInfo data) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			logger.Info($"Achievements def queried {data.ResultCode}");
			InitializeAchievementProgressDictionary();
		}));
		QueryStats(null);
	}

	public void QueryStats(Action callback)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected Obj, but got Unknown
		//IL_0058: Expected Obj, but got Unknown
		ProductUserId val = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetStatsInterface().QueryStats(new QueryStatsOptions
		{
			TargetUserId = val,
			LocalUserId = val
		}, (object)null, (OnQueryStatsCompleteCallback)((OnQueryStatsCompleteCallbackInfo data) =>
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			logger.Info($"Stats queried {data.ResultCode}");
			if (callback != null)
			{
				callback();
			}
		}));
	}

	public void OnMatchStart(RoleTypes myRole)
	{
		logger.Info("Match started");
		gameStarted = true;
		this.myRole = myRole;
		cardSwipesThisMatch = 0;
		ventsUsedThisMatch = 0;
		meetingCalledThisMatch = false;
		killsThisMatch = 0;
		sabotageCalledLocally = false;
		timesVoted = 0;
		timesVotedForImp = 0;
		winMap = -1;
	}

	public void SetWinMap(int mapIndex)
	{
		winMap = mapIndex;
	}

	public void OnMatchExited()
	{
		logger.Info("Match exited");
		gameStarted = false;
	}

	public void OnMatchEnd(GameOverReason reason, bool didWin)
	{
		if (!gameStarted)
		{
			return;
		}
		logger.Info("Match ended");
		gameStarted = false;
		bool flag = PlayerControl.LocalPlayer.Data?.Role.IsImpostor ?? false;
		if (flag)
		{
			if (ventsUsedThisMatch == 0 && GameManager.Instance.DidImpostorsWin(reason))
			{
				UnlockAchievement("no_vents_impostor_win");
			}
			IEnumerable<GameData.PlayerInfo> source = GameData.Instance.AllPlayers.Where((GameData.PlayerInfo p) => p.Role.IsImpostor);
			int num = source.Count();
			if ((num == 2 && num == source.Count((GameData.PlayerInfo p) => !p.IsDead && !p.Disconnected)) & didWin)
			{
				UnlockAchievement("survive_two_impostors");
			}
			if (reason == GameOverReason.ImpostorByKill)
			{
				UnlockAchievement("win_kills");
			}
			if (reason == GameOverReason.ImpostorBySabotage)
			{
				UnlockAchievement("win_sabotage");
			}
			if (reason == GameOverReason.ImpostorByVote)
			{
				UnlockAchievement("win_impostor_vote");
			}
			if (reason == GameOverReason.HideAndSeek_ByKills)
			{
				UnlockAchievement("hns_impostor_kill_all");
			}
		}
		else
		{
			if (GameManager.Instance.DidHumansWin(reason))
			{
				UpdateAchievementProgress("hnsCrewmateWin", 1);
				GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
				if (data != null && !data.IsDead)
				{
					UnlockAchievement("survive_crewmate");
					UnlockAchievement("hns_survive_crewmate");
				}
			}
			if (reason == GameOverReason.HumansByTask)
			{
				UnlockAchievement("win_all_tasks");
			}
		}
		switch (winMap)
		{
		case 0:
			UpdateAchievementProgress("MapWinsSkeld", 1);
			break;
		case 1:
			UpdateAchievementProgress("MapWinsMira", 1);
			break;
		case 2:
			UpdateAchievementProgress("MapWinsPolus", 1);
			break;
		case 4:
			UpdateAchievementProgress("MapWinsAirship", 1);
			break;
		}
		if (didWin && !flag && timesVoted > 0 && timesVotedForImp == timesVoted)
		{
			UnlockAchievement("win_always_correct_votes");
		}
	}

	public void OnConsoleUse(IUsable console)
	{
		if (gameStarted && console is Vent)
		{
			ventsUsedThisMatch++;
		}
	}

	public void SabotageCalledLocally()
	{
		sabotageCalledLocally = true;
	}

	public void OnSabotageCleared()
	{
		if (gameStarted)
		{
			sabotageCalledLocally = false;
		}
	}

	public void OnSabotageFixed(bool didContribute)
	{
		if (gameStarted && (sabotageCalledLocally & didContribute))
		{
			UnlockAchievement("fix_own_sabotage");
		}
	}

	public void OnMurder(bool amKiller, bool amVictim, bool isShapeShifted, int shapeshiftTargetId, int victimId)
	{
		if (!gameStarted)
		{
			return;
		}
		if (amVictim)
		{
			if (IsTaskOpen(TaskTypes.SubmitScan))
			{
				UnlockAchievement("die_during_medscan");
			}
		}
		else if (amKiller)
		{
			killsThisMatch++;
			if (TryGetSystemOfType<SwitchSystem>(SystemTypes.Electrical, out var output) && output.IsActive)
			{
				UnlockAchievement("kill_during_lights");
			}
			UpdateAchievementProgress("impostorKills", 1);
			UpdateAchievementProgress("hnsImpostorKills", 1);
			if (isShapeShifted && shapeshiftTargetId == victimId)
			{
				UnlockAchievement("kill_while_shifted");
			}
			if (killsThisMatch >= 3 && !meetingCalledThisMatch)
			{
				UnlockAchievement("three_kills_before_meeting");
			}
		}
	}

	public void OnMeetingCalled()
	{
		if (gameStarted)
		{
			meetingCalledThisMatch = true;
		}
	}

	public void OnProtectACrewmate()
	{
		UnlockAchievement("block_kill_as_guardian");
	}

	public void OnMeetingVote(GameData.PlayerInfo self, GameData.PlayerInfo target)
	{
		if (gameStarted && target != null)
		{
			timesVoted++;
			if (target.Role.IsImpostor)
			{
				timesVotedForImp++;
			}
		}
	}

	public void OnTaskComplete(TaskTypes taskType)
	{
		if (!gameStarted)
		{
			return;
		}
		UpdateAchievementProgress("tasksCompleted", 1);
		if (taskType == TaskTypes.SwipeCard)
		{
			DestroyableSingleton<UnityTelemetry>.Instance.CardSwipeComplete(cardSwipesThisMatch);
			if (cardSwipesThisMatch == 0)
			{
				UnlockAchievement("card_first_try");
			}
		}
		else
		{
			DestroyableSingleton<UnityTelemetry>.Instance.WriteCompleteTask(taskType);
		}
		if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
		{
			UnlockAchievement("hns_complete_all_tasks");
		}
	}

	public void OnTaskFailure(TaskTypes taskType)
	{
		if (gameStarted && taskType == TaskTypes.SwipeCard)
		{
			cardSwipesThisMatch++;
		}
	}

	public void QueryAndLogAchievementProgress(string achievementKey = null)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected Obj, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		ProductUserId userId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		QueryPlayerAchievementsOptions val = new QueryPlayerAchievementsOptions
		{
			LocalUserId = userId,
			TargetUserId = userId
		};
		DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().QueryPlayerAchievements(val, (object)null, (OnQueryPlayerAchievementsCompleteCallback)((OnQueryPlayerAchievementsCompleteCallbackInfo data) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Debug.Log((object)$"Query Achievement Results {data.ResultCode}");
			if (achievementKey == null)
			{
				foreach (KeyValuePair<string, List<GameModes>> item in AchievementGameModeKey)
				{
					CopyAndLogPlayerAchievementProgress(item.Key, userId);
				}
				return;
			}
			CopyAndLogPlayerAchievementProgress(achievementKey, userId);
		}));
	}

	private bool IsTaskOpen(TaskTypes taskType)
	{
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			return Minigame.Instance.TaskType == taskType;
		}
		return false;
	}

	private bool TryGetSystemOfType<T>(SystemTypes sysType, out T output) where T : class, ISystemType
	{
		output = null;
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			return false;
		}
		if (ShipStatus.Instance.Systems.TryGetValue(sysType, out var value))
		{
			output = value as T;
			return output != null;
		}
		return false;
	}

	private void CopyAndLogPlayerAchievementProgress(string key, ProductUserId userId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		CopyPlayerAchievementByAchievementIdOptions val = new CopyPlayerAchievementByAchievementIdOptions
		{
			AchievementId = key,
			LocalUserId = userId,
			TargetUserId = userId
		};
		PlayerAchievement val3 = default;
		Result val2 = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().CopyPlayerAchievementByAchievementId(val, ref val3);
		if ((int)val2 == 0 && val3 != null)
		{
			PlayerStatInfo[] statInfo = val3.StatInfo;
			string text = ((statInfo != null && statInfo.Length != 0) ? $"{val3.Progress}/{val3.StatInfo[0].ThresholdValue}" : $"{val3.Progress}");
			Debug.Log((object)(key + " | Display Name: " + val3.DisplayName + " | Description: " + val3.Description + " | Progress: " + text));
		}
		else
		{
			Debug.Log((object)$"Failed to find achievement with key {key} result was {val2}");
		}
	}

	private void UpdateAchievementProgress(string statName, int progress)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected Obj, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected Obj, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected Obj, but got Unknown
		if (!StatsGameModeKey.ContainsKey(statName) || !StatsGameModeKey[statName].Contains(GameOptionsManager.Instance.CurrentGameOptions.GameMode))
		{
			logger.Warning($"Stat Key {statName} not available for game mode {GameOptionsManager.Instance.CurrentGameOptions.GameMode}");
			return;
		}
		logger.Info($"Ingest Info {statName}: {progress}");
		ProductUserId userId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		IngestStatOptions val = new IngestStatOptions();
		val.TargetUserId = userId;
		val.LocalUserId = userId;
		val.Stats = new IngestData[1]
		{
			new IngestData
			{
				StatName = statName,
				IngestAmount = progress
			}
		};
		IngestStatOptions val2 = val;
		DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetStatsInterface().IngestStat(val2, (object)null, (OnIngestStatCompleteCallback)((IngestStatCompleteCallbackInfo data) =>
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			logger.Info($"Ingest Results {data.ResultCode}");
			QueryStats(() =>
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Expected Obj, but got Unknown
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				if (statName2AchievementProgressValues == null)
				{
					InitializeAchievementProgressDictionary();
				}
				CopyStatByNameOptions val3 = new CopyStatByNameOptions
				{
					TargetUserId = userId,
					Name = statName
				};
				Stat val5 = default;
				Result val4 = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetStatsInterface().CopyStatByName(val3, ref val5);
				if (statName2AchievementProgressValues.TryGetValue(statName, out var value) && (int)val4 == 0)
				{
					foreach (AchievementProgressValue item in value)
					{
						UpdateAchievementProgressImpl(item.name, val5.Value, item.maxProgress);
					}
				}
			});
		}));
	}

	private void UnlockAchievement(string key)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected Obj, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected Obj, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected Obj, but got Unknown
		if (!AchievementGameModeKey.ContainsKey(key) || !AchievementGameModeKey[key].Contains(GameOptionsManager.Instance.CurrentGameOptions.GameMode))
		{
			logger.Warning($"Achievement Key {key} not available for game mode {GameOptionsManager.Instance.CurrentGameOptions.GameMode}");
			return;
		}
		ProductUserId userId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId);
		logger.Info("Try to Unlock " + key);
		CopyAchievementDefinitionV2ByAchievementIdOptions val = new CopyAchievementDefinitionV2ByAchievementIdOptions
		{
			AchievementId = key
		};
		DefinitionV2 playerAchievement = default;
		Result val2 = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().CopyAchievementDefinitionV2ByAchievementId(val, ref playerAchievement);
		if ((int)val2 == 0)
		{
			UnlockAchievementsOptions val3 = new UnlockAchievementsOptions();
			val3.UserId = userId;
			val3.AchievementIds = new string[1] { playerAchievement.AchievementId };
			UnlockAchievementsOptions val4 = val3;
			DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetAchievementsInterface().UnlockAchievements(val4, (object)null, (OnUnlockAchievementsCompleteCallback)((OnUnlockAchievementsCompleteCallbackInfo data) =>
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				logger.Info($"Unlocked Achievement {playerAchievement.AchievementId} Result {data.ResultCode}");
				UnlockAchievementImpl(key);
			}));
		}
		else
		{
			logger.Error($"Achievement {key} Unlock Error {val2}");
		}
	}
}
