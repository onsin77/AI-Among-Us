using AmongUs.Data.Legacy;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class SettingsData : AbstractUserSaveData
{
	[JsonProperty("gameplay")]
	private GameplaySettingsData gameplay = new GameplaySettingsData();

	[JsonProperty("accessibility")]
	private AccessibilitySettingsData accessibility = new AccessibilitySettingsData();

	[JsonProperty("audio")]
	private AudioSettingsData audio = new AudioSettingsData();

	[JsonProperty("video")]
	private VideoSettingsData video = new VideoSettingsData();

	[JsonProperty("language")]
	private LanguageSettingsData language = new LanguageSettingsData();

	[JsonProperty("input")]
	private InputSettingsData input = new InputSettingsData();

	[JsonProperty("multiplayer")]
	private MultiplayerSettingsData multiplayer = new MultiplayerSettingsData();

	protected override int CurrentVersion => 1;

	public GameplaySettingsData Gameplay => gameplay;

	public AccessibilitySettingsData Accessibility => accessibility;

	public AudioSettingsData Audio => audio;

	public VideoSettingsData Video => video;

	public InputSettingsData Input => input;

	public LanguageSettingsData Language => language;

	public MultiplayerSettingsData Multiplayer => multiplayer;

	protected override string FileName => "settings.amogus";

	protected override AbstractSaveDataGroup[] Groups => new AbstractSaveDataGroup[7] { Gameplay, Accessibility, Audio, Video, Language, Input, Multiplayer };

	protected override void HandleVersionMigration(int version)
	{
		base.HandleVersionMigration(version);
		if (version < 1)
		{
			if (LegacySaveManager.PlayerPrefsExists())
			{
				Gameplay.ScreenShake = LegacySaveManager.Screenshake;
				Gameplay.StreamerMode = LegacySaveManager.StreamerMode;
				Accessibility.ColorBlindMode = LegacySaveManager.ColorBlindMode;
				Audio.SfxVolume = LegacySaveManager.SfxVolume;
				Audio.MusicVolume = LegacySaveManager.MusicVolume;
				Video.VSync = LegacySaveManager.VSync;
				Language.CurrentLanguage = (SupportedLangs)LegacySaveManager.LastLanguage;
				Input.InputMode = LegacySaveManager.ControlMode;
				Input.MouseMovementEnabled = LegacySaveManager.EnableMouseMovement;
				Input.TouchJoystickSize = LegacySaveManager.JoystickSize;
				Multiplayer.CrossPlay = LegacySaveManager.CrossplayAllPlatforms;
				Multiplayer.AllowFriendInvites = LegacySaveManager.EnableFriendsListInvites;
				Multiplayer.CensorChat = LegacySaveManager.CensorChat;
				Multiplayer.ChatMode = LegacySaveManager.ChatModeType;
			}
			dataVersion = 1;
		}
	}
}
