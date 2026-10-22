using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class AccessibilitySettingsData : AbstractSaveDataGroup
{
	public const bool DEFAULT_COLOR_BLIND_MODE = false;

	[JsonProperty("colorBlindMode")]
	private bool colorBlindMode;

	public bool ColorBlindMode
	{
		get
		{
			return colorBlindMode;
		}
		set
		{
			SetValue(ref colorBlindMode, value, OnColorBlindModeChanged);
		}
	}

	public event Action OnColorBlindModeChanged;
}
