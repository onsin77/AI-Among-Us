using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;

public abstract class GameManager : InnerNetObject, IDisconnectHandler
{
	[SerializeField]
	protected DeadBody deadBodyPrefab;

	protected readonly Logger logger = new Logger("GameManager", Logger.Level.Info, Logger.Category.Gameplay);

	protected readonly List<GameLogicComponent> LogicComponents = new List<GameLogicComponent>();

	public override bool IsDirty
	{
		get
		{
			foreach (GameLogicComponent logicComponent in LogicComponents)
			{
				if (logicComponent.IsDirty)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static GameManager Instance { get; private set; }

	public bool ShouldCheckForGameEnd { get; set; }

	public bool GameHasStarted { get; set; }

	public LogicGameFlow LogicFlow { get; protected set; }

	public LogicMinigame LogicMinigame { get; protected set; }

	public LogicRoleSelection LogicRoleSelection { get; protected set; }

	public LogicUsables LogicUsables { get; protected set; }

	public LogicOptions LogicOptions { get; protected set; }

	public virtual DeadBody DeadBodyPrefab => deadBodyPrefab;

	public virtual bool RevealTeams => IsHideAndSeek();

	public void RpcEndGame(GameOverReason endReason, bool showAd)
	{
		ShouldCheckForGameEnd = false;
		logger.Info($"Endgame for {endReason}");
		MessageWriter val = AmongUsClient.Instance.StartEndGame();
		val.Write((byte)endReason);
		val.Write(showAd);
		AmongUsClient.Instance.FinishEndGame(val);
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.HandleRPC((RpcCalls)callId, reader);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		bool result = false;
		for (int i = 0; i < LogicComponents.Count; i++)
		{
			GameLogicComponent gameLogicComponent = LogicComponents[i];
			if (initialState || gameLogicComponent.IsDirty)
			{
				result = true;
				writer.StartMessage((byte)i);
				gameLogicComponent.Serialize(writer, initialState);
				writer.EndMessage();
				gameLogicComponent.ClearDirtyFlag();
			}
		}
		ClearDirtyBits();
		return result;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		while (reader.Position < reader.Length)
		{
			MessageReader val = reader.ReadMessage();
			int tag = val.Tag;
			if (tag < 0 || tag > LogicComponents.Count)
			{
				logger.Error("OUT OF BOUNDS IN DESERIALIZE!!");
			}
			else
			{
				LogicComponents[tag].Deserialize(val, initialState);
			}
		}
	}

	public abstract void InitComponents();

	public abstract PlayerBodyTypes GetBodyType(PlayerControl player);

	public virtual void StartGame()
	{
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.OnGameStart();
		}
		ShouldCheckForGameEnd = true;
		GameHasStarted = true;
		foreach (GameData.PlayerInfo allPlayer in GameData.Instance.AllPlayers)
		{
			allPlayer.OnGameStart();
		}
		DestroyableSingleton<HudManager>.Instance.OnGameStart();
	}

	public virtual void EndGame()
	{
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.OnGameEnd();
		}
		foreach (GameData.PlayerInfo allPlayer in GameData.Instance.AllPlayers)
		{
			allPlayer.OnGameEnd();
		}
		ShouldCheckForGameEnd = false;
		GameHasStarted = false;
	}

	public virtual void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
	{
		LogicRoleSelection.OnPlayerDeath(player, assignGhostRole);
	}

	public virtual void FinishTask(PlayerTask task)
	{
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			logger.Warning("There can be only one game manager! Destroying this one...");
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		InitComponents();
	}

	public bool CheckTaskCompletion()
	{
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin));
				ShipStatus.Instance.Begin();
			}
			return false;
		}
		return CheckEndGameViaTasks();
	}

	public bool CheckEndGameViaTasks()
	{
		GameData.Instance.RecomputeTaskCounts();
		if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
		{
			RpcEndGame(GameOverReason.HumansByTask, !DataManager.Player.Ads.HasPurchasedAdRemoval);
			return true;
		}
		return false;
	}

	public void ReviveEveryoneFreeplay()
	{
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			PlayerControl playerControl = GameData.Instance.AllPlayers[i].Object;
			if (playerControl.Data.IsDead)
			{
				if (playerControl.Data.Role.IsImpostor)
				{
					DestroyableSingleton<RoleManager>.Instance.SetRole(playerControl, RoleTypes.Impostor);
				}
				else
				{
					DestroyableSingleton<RoleManager>.Instance.SetRole(playerControl, RoleTypes.Crewmate);
				}
			}
		}
		Object.FindObjectsOfType<DeadBody>().ForEach((DeadBody b) =>
		{
			Object.Destroy((Object)(object)((Component)b).gameObject);
		});
	}

	protected T AddGameLogic<T>(T logic) where T : GameLogicComponent
	{
		LogicComponents.Add(logic);
		return logic;
	}

	[Obsolete("We really need to refactor IntroCutscene.cs, which is the file that needed this method to exist at all. Don't use this more.")]
	public GameLogicComponent GetLogicComponent<T>()
	{
		return LogicComponents.FirstOrDefault((GameLogicComponent x) => x is T);
	}

	protected virtual void FixedUpdate()
	{
		if (AmongUsClient.Instance.AmHost && ShouldCheckForGameEnd)
		{
			LogicFlow.CheckEndCriteria();
		}
		if (!GameHasStarted)
		{
			return;
		}
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.FixedUpdate();
		}
	}

	public static void DestroyInstance()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)Instance).gameObject);
			Instance = null;
		}
	}

	public override void OnDestroy()
	{
		AmongUsClient.Instance.DisconnectHandlers.Remove(this);
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.OnDestroy();
		}
		base.OnDestroy();
	}

	public void HandleDisconnect(PlayerControl pc, DisconnectReasons reason)
	{
		if (Object.op_Implicit((Object)(object)pc) && Object.op_Implicit((Object)(object)GameData.Instance))
		{
			OnPlayerDisconnect(pc);
		}
	}

	public void HandleDisconnect()
	{
	}

	protected virtual void OnPlayerDisconnect(PlayerControl pc)
	{
		foreach (GameLogicComponent logicComponent in LogicComponents)
		{
			logicComponent.OnPlayerDisconnect(pc);
		}
	}

	public virtual bool DidHumansWin(GameOverReason reason)
	{
		if (reason != GameOverReason.HumansByTask && reason != GameOverReason.HumansByVote)
		{
			return reason == GameOverReason.ImpostorDisconnect;
		}
		return true;
	}

	public virtual bool DidImpostorsWin(GameOverReason reason)
	{
		if (reason != GameOverReason.ImpostorByKill && reason != GameOverReason.ImpostorBySabotage && reason != GameOverReason.ImpostorByVote)
		{
			return reason == GameOverReason.HumansDisconnect;
		}
		return true;
	}

	public abstract MapOptions GetMapOptions();

	public virtual bool CanReportBodies()
	{
		return true;
	}

	public virtual bool SabotagesEnabled()
	{
		return true;
	}

	public virtual bool ShowCrewmatesKilled()
	{
		return IsHideAndSeek();
	}

	[Obsolete]
	public virtual bool IsNormal()
	{
		return false;
	}

	[Obsolete]
	public virtual bool IsHideAndSeek()
	{
		return false;
	}

	public virtual void SetSpecialCosmetics(PlayerControl p)
	{
	}
}
