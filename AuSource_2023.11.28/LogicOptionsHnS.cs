using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

public class LogicOptionsHnS : LogicOptions
{
	private const float CommonShortTaskTime = 5f;

	private const float LongTaskTime = 10f;

	private const float TaskTimePlayerDelta = 1f;

	private HideNSeekGameOptionsV07 GameOptions;

	private HideAndSeekManager HnSManager;

	public float PlayerSpeedBase => GameOptions.PlayerSpeedMod;

	protected override IGameOptions currentGameOptions => GameOptions;

	public LogicOptionsHnS(HideAndSeekManager manager)
		: base(manager)
	{
		HnSManager = manager;
		if (AmongUsClient.Instance.AmHost)
		{
			GameOptions = GameOptionsManager.Instance.CurrentGameOptions as HideNSeekGameOptionsV07;
			GameOptions.ImpostorPlayerID = -1;
		}
	}

	public override float GetPlayerSpeedMod(PlayerControl pc)
	{
		if ((Object)(object)pc == (Object)null || pc.Data == null || (Object)(object)pc.Data.Role == (Object)null)
		{
			return base.GetPlayerSpeedMod(pc);
		}
		if (pc.Data.IsDead)
		{
			return GameOptions.PlayerSpeedMod + 1f;
		}
		if (!pc.Data.Role.IsImpostor)
		{
			return GameOptions.PlayerSpeedMod;
		}
		float num = GameOptions.PlayerSpeedMod + GameOptions.PlayerSpeedMod * 0.25f;
		if (HnSManager.LogicFlowHnS.IsFinalCountdown)
		{
			num *= GameOptions.SeekerFinalSpeed;
		}
		return num;
	}

	public override float GetKillDistance()
	{
		return HideNSeekGameOptionsV07.KillDistances[Mathf.Clamp(GameOptions.KillDistance, 0, HideNSeekGameOptionsV07.KillDistances.Length - 1)];
	}

	public override float GetEngineerCooldown()
	{
		return GetCrewmateVentCooldown();
	}

	public override float GetEngineerInVentTime()
	{
		return GetCrewmateInVentTime();
	}

	public int GetCrewmateLeadTime()
	{
		return 10;
	}

	public float GetEscapeTime()
	{
		return GameOptions.EscapeTime;
	}

	public float GetFinalCountdownTime()
	{
		return GameOptions.FinalCountdownTime;
	}

	public int GetCrewmateVentUses()
	{
		return GameOptions.CrewmateVentUses;
	}

	public float GetScaryMusicDistance()
	{
		return 55f;
	}

	public float GetVeryScaryMusicDistance()
	{
		return 15f;
	}

	public float GetCrewmateInVentTime()
	{
		return GameOptions.CrewmateTimeInVent;
	}

	public float GetCrewmateVentCooldown()
	{
		return 1f;
	}

	public float GetCommonTaskTimeValue()
	{
		return 10f - (float)GameData.Instance.PlayerCount * 1f / 2f;
	}

	public float GetShortTaskTimeValue()
	{
		return GetCommonTaskTimeValue();
	}

	public float GetLongTaskTimeValue()
	{
		return 20f - (float)GameData.Instance.PlayerCount * 1f;
	}

	public bool GetSeekerFinalMap()
	{
		return GameOptions.SeekerFinalMap;
	}

	public int ImpostorPlayerID()
	{
		return GameOptions.ImpostorPlayerID;
	}

	public bool HasImpostorPlayerID()
	{
		return GameOptions.ImpostorPlayerID > -1;
	}

	public bool ValidateImpostorPlayerID(List<GameData.PlayerInfo> players)
	{
		if (!HasImpostorPlayerID())
		{
			return false;
		}
		return players.Find((GameData.PlayerInfo p) => p.PlayerId == ImpostorPlayerID()) != null;
	}

	public bool GetSeekerPings()
	{
		return GameOptions.SeekerPings;
	}

	public float GetMaxPingTime()
	{
		return GameOptions.MaxPingTime;
	}

	public float GetShowPingTime()
	{
		return 2f;
	}

	public override bool GetShowCrewmateNames()
	{
		return GameOptions.ShowCrewmateNames;
	}

	public override TaskBarMode GetTaskBarMode()
	{
		return TaskBarMode.Normal;
	}

	public override int GetEmergencyCooldown()
	{
		return 0;
	}

	public override int GetNumEmergencyMeetings()
	{
		return 0;
	}

	public override bool GetVisualTasks()
	{
		return false;
	}

	public override bool GetGhostsDoTasks()
	{
		return false;
	}

	public override void OnGameStart()
	{
	}

	public override void OnGameEnd()
	{
	}

	public override void FixedUpdate()
	{
	}

	public override void OnDestroy()
	{
	}

	protected override void SetGameOptions(IGameOptions newOptions)
	{
		GameOptions = newOptions as HideNSeekGameOptionsV07;
		SetDirty();
	}
}
