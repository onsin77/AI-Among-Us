using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class GameplaySettingsData : AbstractSaveDataGroup
{
	public const bool DEFAULT_SCREEN_SHAKE = true;

	public const bool DEFAULT_STREAMER_MODE = false;

	[JsonProperty("screenShake")]
	private bool screenShake = true;

	[JsonProperty("streamerMode")]
	private bool streamerMode;

	public bool ScreenShake
	{
		get
		{
			return screenShake;
		}
		set
		{
			SetValue(ref screenShake, value, OnScreenShakeChanged);
		}
	}

	public bool StreamerMode
	{
		get
		{
			return streamerMode;
		}
		set
		{
			SetValue(ref streamerMode, value, OnStreamerModeChanged);
		}
	}

	public event Action OnScreenShakeChanged;

	public event Action OnStreamerModeChanged;
}
