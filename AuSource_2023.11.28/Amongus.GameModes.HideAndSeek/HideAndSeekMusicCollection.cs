using UnityEngine;

namespace Amongus.GameModes.HideAndSeek;

public class HideAndSeekMusicCollection : ScriptableObject
{
	[Header("Crewmate Music")]
	public AudioClip NormalMusic;

	public AudioClip TaskMusic;

	public AudioClip DangerLevel1Music;

	public AudioClip DangerLevel2Music;

	[Header("Impostor Music")]
	public AudioClip ImpostorShortMusic;

	public AudioClip ImpostorLongMusic;

	public AudioClip ImpostorRanchMusic;
}
