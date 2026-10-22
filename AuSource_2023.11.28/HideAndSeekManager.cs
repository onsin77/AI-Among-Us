using Amongus.GameModes.HideAndSeek;
using TMPro;
using UnityEngine;

public class HideAndSeekManager : GameManager
{
	public LogicHnSMusic LogicMusic;

	public LogicGameFlowHnS LogicFlowHnS;

	public LogicOptionsHnS LogicOptionsHnS;

	public LogicHnSDangerLevel LogicDangerLevel;

	public LogicPingsHnS LogicPing;

	public LogicHnSDeathPopup LogicDeathPopup;

	public AudioClip FinalHideAlertSFX;

	public AudioClip FinalHideCountdownSFX;

	[SerializeField]
	private HideAndSeekTimerBar TimerBarPrefab;

	[SerializeField]
	private HideAndSeekDeathPopup DeathPopupPrefab;

	[SerializeField]
	private HideAndSeekMusicCollection MusicCollection;

	[SerializeField]
	private AudioClip TaskFinishedSound;

	[SerializeField]
	private ObjectPoolBehavior PingPool;

	[SerializeField]
	private Sprite BrokenEmergencyButton;

	[SerializeField]
	private DeadBody horseBody;

	private TextMeshPro someoneJustDiedText;

	private GameData.PlayerOutfit horseWranglerOutfit = new GameData.PlayerOutfit
	{
		HatId = "hat_Starless",
		SkinId = "skin_MilitaryDesert",
		VisorId = "visor_WinstonStache"
	};

	public override DeadBody DeadBodyPrefab
	{
		get
		{
			if (!Constants.ShouldHorseAround())
			{
				return deadBodyPrefab;
			}
			return horseBody;
		}
	}

	public override void InitComponents()
	{
		LogicMusic = AddGameLogic(new LogicHnSMusic(this, MusicCollection));
		LogicMinigame = AddGameLogic(new LogicMinigameHnS(this));
		LogicFlowHnS = new LogicGameFlowHnS(this, TimerBarPrefab);
		LogicFlow = AddGameLogic(LogicFlowHnS);
		LogicUsables = AddGameLogic(new LogicUsablesHnS(this));
		LogicRoleSelection = AddGameLogic(new LogicRoleSelectionHnS(this));
		LogicOptionsHnS = new LogicOptionsHnS(this);
		LogicOptions = AddGameLogic(LogicOptionsHnS);
		LogicDangerLevel = AddGameLogic(new LogicHnSDangerLevel(this));
		LogicPing = AddGameLogic(new LogicPingsHnS(this, PingPool));
		LogicDeathPopup = AddGameLogic(new LogicHnSDeathPopup(this, DeathPopupPrefab));
	}

	public override void StartGame()
	{
		base.StartGame();
		if ((Object)(object)someoneJustDiedText != (Object)null)
		{
			Object.Destroy((Object)(object)someoneJustDiedText);
		}
		ShipStatus.Instance.BreakEmergencyButton();
	}

	public override PlayerBodyTypes GetBodyType(PlayerControl player)
	{
		if ((Object)(object)player == (Object)null || player.Data == null || (Object)(object)player.Data.Role == (Object)null)
		{
			return PlayerBodyTypes.Normal;
		}
		if (Constants.ShouldHorseAround())
		{
			if (player.Data.Role.IsImpostor)
			{
				return PlayerBodyTypes.Normal;
			}
			return PlayerBodyTypes.Horse;
		}
		if (player.Data.Role.IsImpostor)
		{
			return PlayerBodyTypes.Seeker;
		}
		return PlayerBodyTypes.Normal;
	}

	public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
	{
		base.OnPlayerDeath(player, assignGhostRole);
		DestroyableSingleton<HudManager>.Instance.NotifyOfDeath();
		if (player.AmOwner)
		{
			LogicMusic.ResetMusic();
		}
		LogicDeathPopup.OnPlayerDeath(player);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	protected override void OnPlayerDisconnect(PlayerControl pc)
	{
		base.OnPlayerDisconnect(pc);
		DestroyableSingleton<HudManager>.Instance.NotifyOfDisconnect(pc);
	}

	public override void FinishTask(PlayerTask task)
	{
		if (task is NormalPlayerTask normalPlayerTask)
		{
			switch (normalPlayerTask.Length)
			{
			case NormalPlayerTask.TaskLength.None:
			case NormalPlayerTask.TaskLength.Common:
				LogicFlowHnS.OnTaskComplete(LogicOptionsHnS.GetCommonTaskTimeValue());
				break;
			case NormalPlayerTask.TaskLength.Short:
				LogicFlowHnS.OnTaskComplete(LogicOptionsHnS.GetShortTaskTimeValue());
				break;
			case NormalPlayerTask.TaskLength.Long:
				LogicFlowHnS.OnTaskComplete(LogicOptionsHnS.GetLongTaskTimeValue());
				break;
			}
			SoundManager.Instance.PlaySoundImmediate(TaskFinishedSound, loop: false);
		}
	}

	public override void EndGame()
	{
		base.EndGame();
		CheckUpdateSurvivedStat();
		CheckUpdateGameTimeStats();
		if ((Object)(object)someoneJustDiedText != (Object)null)
		{
			Object.Destroy((Object)(object)someoneJustDiedText);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((Object)(object)someoneJustDiedText != (Object)null)
		{
			Object.Destroy((Object)(object)someoneJustDiedText);
		}
	}

	public override bool DidHumansWin(GameOverReason reason)
	{
		if (reason != GameOverReason.HideAndSeek_ByTimer)
		{
			return reason == GameOverReason.ImpostorDisconnect;
		}
		return true;
	}

	public override bool DidImpostorsWin(GameOverReason reason)
	{
		if (reason != GameOverReason.HideAndSeek_ByKills)
		{
			return reason == GameOverReason.HumansDisconnect;
		}
		return true;
	}

	public override MapOptions GetMapOptions()
	{
		MapOptions mapOptions = new MapOptions
		{
			Mode = MapOptions.Modes.Normal
		};
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && LogicFlowHnS.SeekerAdminMapEnabled(PlayerControl.LocalPlayer))
		{
			mapOptions.Mode = MapOptions.Modes.CountOverlay;
			mapOptions.AllowMovementWhileMapOpen = true;
			mapOptions.IncludeDeadBodies = false;
			mapOptions.ShowLivePlayerPosition = false;
		}
		return mapOptions;
	}

	private void CheckUpdateSurvivedStat()
	{
		if (!PlayerControl.LocalPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsHidenSeekGamesCrewmateSurvived);
		}
	}

	private void CheckUpdateGameTimeStats()
	{
		float roundTimeElapsed = LogicFlowHnS.GetRoundTimeElapsed();
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && DidImpostorsWin(TempData.EndReason))
		{
			StatsManager.Instance.UpdateHideAndSeekImpostorWinTimer(roundTimeElapsed);
		}
		else if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor && DidHumansWin(TempData.EndReason))
		{
			StatsManager.Instance.UpdateHideAndSeekCrewmateWinTimer(roundTimeElapsed);
		}
	}

	public override bool CanReportBodies()
	{
		return false;
	}

	public override bool SabotagesEnabled()
	{
		return false;
	}

	public override bool IsHideAndSeek()
	{
		return true;
	}

	public override void SetSpecialCosmetics(PlayerControl p)
	{
		if (Constants.ShouldHorseAround())
		{
			horseWranglerOutfit.ColorId = p.CurrentOutfit.ColorId;
			horseWranglerOutfit.PlayerName = p.CurrentOutfit.PlayerName;
			p.SetOutfit(horseWranglerOutfit, PlayerOutfitType.HorseWrangler);
		}
	}
}
