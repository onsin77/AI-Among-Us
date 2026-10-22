using System.Collections.Generic;
using Amongus.GameModes.HideAndSeek;
using Hazel;
using UnityEngine;

public class LogicHnSMusic : GameLogicComponent
{
	public enum HideAndSeekMusicTrack
	{
		None,
		Normal,
		Task,
		DangerLevel1,
		DangerLevel2
	}

	private HideAndSeekMusicCollection musicCollection;

	private float lastMusicSyncTime;

	private bool isDoingTask;

	private float normalVolume;

	private float taskVolume;

	private float dangerLevel1Volume;

	private float dangerLevel2Volume;

	private AudioSource normalSource;

	private AudioSource taskSource;

	private AudioSource dangerLevel1Source;

	private AudioSource dangerLevel2Source;

	private float musicLerpSpeed = 5f;

	private readonly Dictionary<HideAndSeekMusicTrack, string> musicNames = new Dictionary<HideAndSeekMusicTrack, string>
	{
		{
			HideAndSeekMusicTrack.Normal,
			"HnS_Music_Normal"
		},
		{
			HideAndSeekMusicTrack.Task,
			"HnS_Music_Task"
		},
		{
			HideAndSeekMusicTrack.DangerLevel1,
			"HnS_Music_DangerLevel1"
		},
		{
			HideAndSeekMusicTrack.DangerLevel2,
			"HnS_Music_DangerLevel2"
		}
	};

	public LogicHnSMusic(HideAndSeekManager manager, HideAndSeekMusicCollection music)
		: base(manager)
	{
		musicCollection = music;
	}

	public override void OnGameStart()
	{
		InitMusic();
		ResetMusic();
	}

	public override void OnGameEnd()
	{
		ResetMusic();
	}

	public override void OnDestroy()
	{
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	private void InitMusic()
	{
		if (normalSource == null)
		{
			normalSource = SoundManager.Instance.GetNamedSfxSource(musicNames[HideAndSeekMusicTrack.Normal]);
		}
		normalSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
		normalSource.clip = musicCollection.NormalMusic;
		normalSource.loop = true;
		if (taskSource == null)
		{
			taskSource = SoundManager.Instance.GetNamedSfxSource(musicNames[HideAndSeekMusicTrack.Task]);
		}
		taskSource.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
		taskSource.volume = 0f;
		taskSource.clip = musicCollection.TaskMusic;
		taskSource.loop = true;
		if (dangerLevel1Source == null)
		{
			dangerLevel1Source = SoundManager.Instance.GetNamedSfxSource(musicNames[HideAndSeekMusicTrack.DangerLevel1]);
		}
		dangerLevel1Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
		dangerLevel1Source.volume = 0f;
		dangerLevel1Source.clip = musicCollection.DangerLevel1Music;
		dangerLevel1Source.loop = true;
		if (dangerLevel2Source == null)
		{
			dangerLevel2Source = SoundManager.Instance.GetNamedSfxSource(musicNames[HideAndSeekMusicTrack.DangerLevel2]);
		}
		dangerLevel2Source.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;
		dangerLevel2Source.volume = 0f;
		dangerLevel2Source.clip = musicCollection.DangerLevel2Music;
		dangerLevel2Source.loop = true;
		normalSource.Play();
		taskSource.Play();
		dangerLevel1Source.Play();
		dangerLevel2Source.Play();
		SyncMusic();
	}

	public void StartMusicWithIntro()
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
		{
			AudioClip clip = (((Manager as HideAndSeekManager).LogicOptionsHnS.GetEscapeTime() <= 180f) ? musicCollection.ImpostorShortMusic : musicCollection.ImpostorLongMusic);
			if (Constants.ShouldHorseAround())
			{
				clip = musicCollection.ImpostorRanchMusic;
			}
			SoundManager.Instance.PlaySound(clip, loop: true, 1f, SoundManager.Instance.MusicChannel);
		}
	}

	public void SetTaskState(bool isDoingTask)
	{
		this.isDoingTask = isDoingTask;
	}

	public override void FixedUpdate()
	{
		if (!((Object)(object)normalSource == (Object)null) && !((Object)(object)taskSource == (Object)null) && !((Object)(object)dangerLevel1Source == (Object)null) && !((Object)(object)dangerLevel2Source == (Object)null))
		{
			if (Time.unscaledTime > lastMusicSyncTime + 1f)
			{
				SyncMusic();
			}
			normalSource.volume = Mathf.Lerp(normalSource.volume, normalVolume, Time.fixedDeltaTime * musicLerpSpeed);
			taskSource.volume = Mathf.Lerp(taskSource.volume, taskVolume, Time.fixedDeltaTime * musicLerpSpeed);
			dangerLevel1Source.volume = Mathf.Lerp(dangerLevel1Source.volume, dangerLevel1Volume, Time.fixedDeltaTime * musicLerpSpeed);
			dangerLevel2Source.volume = Mathf.Lerp(dangerLevel2Source.volume, dangerLevel2Volume, Time.fixedDeltaTime * musicLerpSpeed);
		}
	}

	private void SyncMusic()
	{
		taskSource.timeSamples = normalSource.timeSamples;
		dangerLevel1Source.timeSamples = normalSource.timeSamples;
		dangerLevel2Source.timeSamples = normalSource.timeSamples;
		lastMusicSyncTime = Time.unscaledTime;
	}

	public void ResetMusic()
	{
		SetMusicValues(0f, 0f);
	}

	public void SetMusicCrossfadeSpeed(float lerpSpeed)
	{
		musicLerpSpeed = lerpSpeed;
	}

	public void SetMusicValues(float dangerLevel1, float dangerLevel2)
	{
		if (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (Object)(object)normalSource == (Object)null || (Object)(object)taskSource == (Object)null || (Object)(object)dangerLevel1Source == (Object)null || (Object)(object)dangerLevel2Source == (Object)null)
		{
			return;
		}
		normalVolume = (isDoingTask ? 0f : 1f);
		taskVolume = (isDoingTask ? 1f : 0f);
		dangerLevel1Volume = 0f;
		dangerLevel2Volume = 0f;
		if (dangerLevel1 > 0f)
		{
			dangerLevel1Volume = dangerLevel1;
			if (isDoingTask)
			{
				taskVolume = 1f - dangerLevel1;
			}
			else
			{
				normalVolume = 1f - dangerLevel1;
			}
		}
		if (dangerLevel2 > 0f)
		{
			dangerLevel2Volume = dangerLevel2;
			dangerLevel1Volume = 1f - dangerLevel2;
		}
	}
}
