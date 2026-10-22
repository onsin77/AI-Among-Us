using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class VideoSettingsData : AbstractSaveDataGroup
{
	public const bool DEFAULT_VSYNC = false;

	[JsonProperty("vsync")]
	private bool vsync;

	public bool VSync
	{
		get
		{
			return vsync;
		}
		set
		{
			SetValue(ref vsync, value, OnVSyncChanged);
		}
	}

	public event Action OnVSyncChanged;
}
