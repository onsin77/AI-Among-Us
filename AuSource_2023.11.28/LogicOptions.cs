using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;

public abstract class LogicOptions : GameLogicComponent
{
	private readonly GameOptionsFactory gameOptionsFactory = new GameOptionsFactory((ILogger)(object)new UnityLogger());

	protected abstract IGameOptions currentGameOptions { get; }

	public byte MapId => currentGameOptions.MapId;

	public int MaxPlayers => currentGameOptions.MaxPlayers;

	public int NumImpostors => currentGameOptions.NumImpostors;

	public bool IsDefaults => currentGameOptions.IsDefaults;

	public LogicOptions(GameManager manager)
		: base(manager)
	{
	}

	public virtual int GetAdjustedNumImpostors(int playerCount)
	{
		return currentGameOptions.GetAdjustedNumImpostors(playerCount);
	}

	public virtual void SetRecommendations(int numPlayers, NetworkModes networkMode)
	{
		currentGameOptions.SetRecommendations(numPlayers, networkMode);
		SyncOptions();
	}

	public void SyncOptions()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			SetDirty();
			if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
			{
				PlayerControl.LocalPlayer.RpcSyncSettings(gameOptionsFactory.ToBytes(currentGameOptions));
			}
		}
	}

	protected abstract void SetGameOptions(IGameOptions newOptions);

	public virtual bool GetGhostsDoTasks()
	{
		if (!currentGameOptions.TryGetBool(BoolOptionNames.GhostsDoTasks, out var value))
		{
			return false;
		}
		return value;
	}

	public virtual float GetEngineerCooldown()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.EngineerCooldown, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetEngineerInVentTime()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.EngineerInVentMaxTime, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetGuardianAngelCooldown()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.GuardianAngelCooldown, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetShapeshifterDuration()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.ShapeshifterDuration, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetShapeshifterCooldown()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.ShapeshifterCooldown, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual bool GetShapeshifterLeaveSkin()
	{
		if (!currentGameOptions.TryGetBool(BoolOptionNames.ShapeshifterLeaveSkin, out var value))
		{
			return false;
		}
		return value;
	}

	public virtual float GetScientistCooldown()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.ScientistCooldown, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetScientistBatteryCharge()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.ScientistBatteryCharge, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetKillCooldown()
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.KillCooldown, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual float GetKillDistance()
	{
		float[] floatArray = currentGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances);
		int num = currentGameOptions.GetInt(Int32OptionNames.KillDistance);
		return floatArray[Mathf.Clamp(num, 0, floatArray.Length - 1)];
	}

	public virtual float GetPlayerSpeedMod(PlayerControl pc)
	{
		if (!currentGameOptions.TryGetFloat(FloatOptionNames.PlayerSpeedMod, out var value))
		{
			return 1f;
		}
		return value;
	}

	public virtual bool GetConfirmImpostor()
	{
		if (!currentGameOptions.TryGetBool(BoolOptionNames.ConfirmImpostor, out var value))
		{
			return false;
		}
		return value;
	}

	public virtual int GetEmergencyCooldown()
	{
		if (!currentGameOptions.TryGetInt(Int32OptionNames.EmergencyCooldown, out var value))
		{
			return 1;
		}
		return value;
	}

	public virtual int GetNumEmergencyMeetings()
	{
		if (!currentGameOptions.TryGetInt(Int32OptionNames.NumEmergencyMeetings, out var value))
		{
			return 0;
		}
		return value;
	}

	public virtual bool GetVisualTasks()
	{
		if (!currentGameOptions.TryGetBool(BoolOptionNames.VisualTasks, out var value))
		{
			return false;
		}
		return value;
	}

	public virtual bool GetAnonymousVotes()
	{
		if (!currentGameOptions.TryGetBool(BoolOptionNames.AnonymousVotes, out var value))
		{
			return false;
		}
		return value;
	}

	public virtual TaskBarMode GetTaskBarMode()
	{
		if (!currentGameOptions.TryGetInt(Int32OptionNames.TaskBarMode, out var value))
		{
			return TaskBarMode.Normal;
		}
		return (TaskBarMode)value;
	}

	public virtual bool GetShowCrewmateNames()
	{
		return true;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.WriteBytesAndSize(gameOptionsFactory.ToBytes(currentGameOptions));
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		IGameOptions gameOptions = gameOptionsFactory.FromNetworkMessageWithSize(reader);
		SetGameOptions(gameOptions);
		GameOptionsManager.Instance.CurrentGameOptions = gameOptions;
	}
}
