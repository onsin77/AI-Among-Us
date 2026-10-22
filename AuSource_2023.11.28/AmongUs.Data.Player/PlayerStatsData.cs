using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerStatsData : AbstractSaveDataGroup
{
	public const uint DEFAULT_LEVEL = 0u;

	public const uint DEFAULT_XP = 0u;

	public const uint DEFAULT_XP_FOR_NEXT_LEVEL = 0u;

	[JsonProperty("level")]
	private uint level;

	[JsonProperty("xp")]
	private uint xp;

	[JsonProperty("xpForNextLevel")]
	private uint xpForNextLevel;

	public uint Level
	{
		get
		{
			return level;
		}
		set
		{
			SetValue(ref level, value, OnLevelChanged);
		}
	}

	public uint Xp
	{
		get
		{
			return xp;
		}
		set
		{
			SetValue(ref xp, value, OnXpChanged);
		}
	}

	public uint XpForNextLevel
	{
		get
		{
			return xpForNextLevel;
		}
		set
		{
			SetValue(ref xpForNextLevel, value, OnXpForNextLevelChanged);
		}
	}

	public event Action OnLevelChanged;

	public event Action OnXpChanged;

	public event Action OnXpForNextLevelChanged;
}
