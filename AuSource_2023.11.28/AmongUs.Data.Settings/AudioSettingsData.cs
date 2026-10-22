using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class AudioSettingsData : AbstractSaveDataGroup
{
	public const float DEFAULT_MUSIC_VOLUME = 0.6f;

	public const float DEFAULT_AMBIENCE_VOLUME = 0.6f;

	public const float DEFAULT_SFX_VOLUME = 0.6f;

	[JsonProperty("musicVolume")]
	private float musicVolume = 0.6f;

	[JsonProperty("ambienceVolume")]
	private float ambienceVolume = 0.6f;

	[JsonProperty("sfxVolume")]
	private float sfxVolume = 0.6f;

	public float MusicVolume
	{
		get
		{
			return musicVolume;
		}
		set
		{
			SetValue(ref musicVolume, value, OnMusicVolumeChanged);
		}
	}

	public float AmbienceVolume
	{
		get
		{
			return ambienceVolume;
		}
		set
		{
			SetValue(ref ambienceVolume, value, OnAmbienceVolumeChanged);
		}
	}

	public float SfxVolume
	{
		get
		{
			return sfxVolume;
		}
		set
		{
			SetValue(ref sfxVolume, value, OnSfxVolumeChanged);
		}
	}

	public event Action OnMusicVolumeChanged;

	public event Action OnAmbienceVolumeChanged;

	public event Action OnSfxVolumeChanged;
}
