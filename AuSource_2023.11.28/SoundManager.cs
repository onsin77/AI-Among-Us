using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
	private static SoundManager instance;

	[SerializeField]
	private AudioMixer mixer;

	[SerializeField]
	private AudioMixerGroup musicMixer;

	[SerializeField]
	private AudioMixerGroup ambienceMixer;

	[SerializeField]
	private AudioMixerGroup ambienceHighPassMixer;

	[SerializeField]
	private AudioMixerGroup sfxMixer;

	private Dictionary<AudioClip, AudioSource> allSources = new Dictionary<AudioClip, AudioSource>();

	private List<ISoundPlayer> soundPlayers = new List<ISoundPlayer>();

	private static float musicVolume = 1f;

	private static float sfxVolume = 1f;

	private static float ambienceVolume = 1f;

	public static SoundManager Instance
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)instance))
			{
				instance = Object.FindObjectOfType<SoundManager>() ?? new GameObject("SoundManager").AddComponent<SoundManager>();
			}
			return instance;
		}
	}

	public static float MusicVolume => musicVolume;

	public static float AmbienceVolume => ambienceVolume;

	public static float SfxVolume => sfxVolume;

	public AudioMixerGroup MusicChannel => musicMixer;

	public AudioMixerGroup AmbienceChannel => ambienceMixer;

	public AudioMixerGroup SfxChannel => sfxMixer;

	public AudioMixerGroup AmbienceHighPassChannel => ambienceHighPassMixer;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)instance) && (Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		UpdateChannelVolumes();
		DataManager.Settings.Audio.OnChangedEvent += OnAudioSettingsChanged;
	}

	private void OnDestroy()
	{
		DataManager.Settings.Audio.OnChangedEvent -= OnAudioSettingsChanged;
	}

	private void Update()
	{
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			soundPlayers[i].Update(Time.deltaTime);
		}
	}

	private void OnAudioSettingsChanged()
	{
		UpdateChannelVolumes();
	}

	private void UpdateChannelVolumes()
	{
		ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);
		ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
		ChangeSfxVolume(DataManager.Settings.Audio.SfxVolume);
	}

	private void ChangeMusicVolume(float volume)
	{
		SetChannelVolume(volume, ref musicVolume, "MusicVolume");
	}

	private void ChangeAmbienceVolume(float volume)
	{
		SetChannelVolume(volume, ref ambienceVolume, "AmbienceVolume");
	}

	private void ChangeSfxVolume(float volume)
	{
		SetChannelVolume(volume, ref sfxVolume, "SfxVolume");
	}

	private void SetChannelVolume(float volume, ref float value, string channelName)
	{
		value = ((volume <= 0f) ? (-80f) : GetCalculatedVolume(volume));
		mixer.SetFloat(channelName, value);
	}

	private static float GetCalculatedVolume(float volume)
	{
		return Mathf.Log10(volume) * 20f;
	}

	public bool HasNamedSound(string name)
	{
		foreach (ISoundPlayer soundPlayer in soundPlayers)
		{
			if (soundPlayer.Name.Equals(name))
			{
				return true;
			}
		}
		return false;
	}

	public void StopNamedSound(string name)
	{
		for (int num = soundPlayers.Count - 1; num >= 0; num--)
		{
			ISoundPlayer soundPlayer = soundPlayers[num];
			if (soundPlayer.Name.Equals(name))
			{
				Object.Destroy((Object)(object)soundPlayer.Player);
				soundPlayers.RemoveAt(num);
				break;
			}
		}
	}

	public void StopSound(AudioClip clip)
	{
		if (allSources.TryGetValue(clip, out var value))
		{
			allSources.Remove(clip);
			value.Stop();
			Object.Destroy((Object)(object)value);
		}
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = soundPlayers[i];
			if ((Object)(object)soundPlayer.Player.clip == (Object)(object)clip)
			{
				Object.Destroy((Object)(object)soundPlayer.Player);
				soundPlayers.RemoveAt(i);
				break;
			}
		}
	}

	public void StopAllSound()
	{
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			Object.Destroy((Object)(object)soundPlayers[i].Player);
		}
		soundPlayers.Clear();
		foreach (KeyValuePair<AudioClip, AudioSource> allSource in allSources)
		{
			AudioSource value = allSource.Value;
			value.volume = 0f;
			value.Stop();
			Object.Destroy((Object)(object)allSource.Value);
		}
		allSources.Clear();
	}

	public AudioSource PlayNamedSound(string name, AudioClip sound, bool loop, AudioMixerGroup channel)
	{
		return PlayDynamicSound(name, sound, loop, (AudioSource a, float b) =>
		{
		}, channel);
	}

	public AudioSource GetNamedSfxSource(string name)
	{
		return GetNamedAudioSource(name, SfxChannel);
	}

	public AudioSource GetNamedAudioSource(string name, AudioMixerGroup channel)
	{
		return PlayDynamicSound(name, null, loop: false, (AudioSource a, float b) =>
		{
		}, channel);
	}

	public AudioSource PlayDynamicSound(string name, AudioClip clip, bool loop, DynamicSound.GetDynamicsFunction volumeFunc, AudioMixerGroup channel)
	{
		DynamicSound dynamicSound = null;
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = soundPlayers[i];
			if (soundPlayer.Name == name && soundPlayer is DynamicSound)
			{
				dynamicSound = (DynamicSound)soundPlayer;
				break;
			}
		}
		if (dynamicSound == null)
		{
			dynamicSound = new DynamicSound();
			dynamicSound.Name = name;
			dynamicSound.Player = ((Component)this).gameObject.AddComponent<AudioSource>();
			dynamicSound.Player.outputAudioMixerGroup = channel;
			dynamicSound.Player.playOnAwake = false;
			soundPlayers.Add(dynamicSound);
		}
		dynamicSound.Player.loop = loop;
		dynamicSound.SetTarget(clip, volumeFunc);
		return dynamicSound.Player;
	}

	public void CrossFadeSound(string name, AudioClip clip, float maxVolume, float duration = 1.5f)
	{
		CrossFader crossFader = null;
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			ISoundPlayer soundPlayer = soundPlayers[i];
			if (soundPlayer.Name == name && soundPlayer is CrossFader)
			{
				crossFader = (CrossFader)soundPlayer;
				break;
			}
		}
		if (crossFader == null)
		{
			crossFader = new CrossFader();
			crossFader.Name = name;
			crossFader.Duration = duration;
			crossFader.MaxVolume = maxVolume;
			crossFader.Player = ((Component)this).gameObject.AddComponent<AudioSource>();
			crossFader.Player.outputAudioMixerGroup = MusicChannel;
			crossFader.Player.playOnAwake = false;
			crossFader.Player.loop = true;
			soundPlayers.Add(crossFader);
		}
		crossFader.SetTarget(clip);
	}

	public AudioSource PlaySoundImmediate(AudioClip clip, bool loop, float volume = 1f, float pitch = 1f, AudioMixerGroup audioMixer = null)
	{
		if ((Object)(object)clip == (Object)null)
		{
			Debug.LogWarning((object)"Missing audio clip");
			return null;
		}
		if ((Object)(object)audioMixer == (Object)null)
		{
			audioMixer = (loop ? MusicChannel : SfxChannel);
		}
		if (allSources.TryGetValue(clip, out var value))
		{
			value.pitch = pitch;
			value.loop = loop;
			value.outputAudioMixerGroup = audioMixer;
			value.Play();
		}
		else
		{
			value = ((Component)this).gameObject.AddComponent<AudioSource>();
			value.outputAudioMixerGroup = audioMixer;
			value.playOnAwake = false;
			value.volume = volume;
			value.pitch = pitch;
			value.loop = loop;
			value.clip = clip;
			value.Play();
			allSources.Add(clip, value);
		}
		return value;
	}

	public bool SoundIsPlaying(AudioClip clip)
	{
		if (allSources.TryGetValue(clip, out var value))
		{
			return !value.isPlaying;
		}
		return false;
	}

	public AudioSource PlaySound(AudioClip clip, bool loop, float volume = 1f, AudioMixerGroup audioMixer = null)
	{
		if ((Object)(object)clip == (Object)null)
		{
			Debug.LogWarning((object)"Missing audio clip");
			return null;
		}
		if ((Object)(object)audioMixer == (Object)null)
		{
			audioMixer = (loop ? MusicChannel : SfxChannel);
		}
		if (allSources.TryGetValue(clip, out var value))
		{
			if (!value.isPlaying)
			{
				value.volume = volume;
				value.outputAudioMixerGroup = audioMixer;
				value.loop = loop;
				value.Play();
			}
		}
		else
		{
			value = ((Component)this).gameObject.AddComponent<AudioSource>();
			value.outputAudioMixerGroup = audioMixer;
			value.playOnAwake = false;
			value.volume = volume;
			value.loop = loop;
			value.clip = clip;
			value.Play();
			allSources.Add(clip, value);
		}
		return value;
	}

	public static float GetSoundVolume(Vector2 soundPosition, Vector2 listenerPosition, float falloffStartDistance = 2f, float falloffEndDistance = 6f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(listenerPosition, soundPosition);
		if (num > falloffEndDistance)
		{
			return 0f;
		}
		if (num >= falloffStartDistance)
		{
			float num2 = (num - falloffStartDistance) / (falloffEndDistance - falloffStartDistance);
			return 1f - Mathf.Clamp01(Mathf.SmoothStep(0f, 1f, num2));
		}
		return 1f;
	}
}
