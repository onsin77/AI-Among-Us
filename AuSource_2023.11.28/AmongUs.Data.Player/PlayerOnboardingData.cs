using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerOnboardingData : AbstractSaveDataGroup
{
	public const int DEFAULT_PRIVACY_POLICY_VERSION = 0;

	public const bool DEFAULT_VIEWED_ONLINE_HELP_MESSAGE = false;

	public const bool DEFAULT_VIEWED_MIN_PLAYER_WARNING = false;

	public const bool DEFAULT_ALWAYS_SHOW_MIN_PLAYER_WARNING = true;

	public const bool DEFAULT_VIEWED_HIDEANDSEEK_HOWTOPLAY = false;

	[JsonProperty("privacyPolicyVersion")]
	private int privacyPolicyVersion;

	[JsonProperty("viewedOnlineHelpMessage")]
	private bool viewedOnlineHelpMessage;

	[JsonProperty("viewedMinPlayerWarning")]
	private bool viewedMinPlayerWarning;

	[JsonProperty("alwaysShowMinPlayerWarning")]
	private bool alwaysShowMinPlayerWarning = true;

	[JsonProperty("viewedHideAndSeekHowToPlay")]
	private bool viewedHideAndSeekHowToPlay;

	public int LastAcceptedPrivacyPolicyVersion
	{
		get
		{
			return privacyPolicyVersion;
		}
		set
		{
			SetValue(ref privacyPolicyVersion, value, OnLastAcceptedPrivacyPolicyVersionChanged);
		}
	}

	public bool ViewedOnlineHelpMessage
	{
		get
		{
			return viewedOnlineHelpMessage;
		}
		set
		{
			SetValue(ref viewedOnlineHelpMessage, value, OnViewedOnlineHelpMessageChanged);
		}
	}

	public bool ViewedMinPlayerWarning
	{
		get
		{
			return viewedMinPlayerWarning;
		}
		set
		{
			SetValue(ref viewedMinPlayerWarning, value, OnViewedMinPlayerWarningChanged);
		}
	}

	public bool AlwaysShowMinPlayerWarning
	{
		get
		{
			return alwaysShowMinPlayerWarning;
		}
		set
		{
			SetValue(ref alwaysShowMinPlayerWarning, value, OnAlwaysShowMinPlayerWarningChanged);
		}
	}

	public bool ViewedHideAndSeekHowToPlay
	{
		get
		{
			return viewedHideAndSeekHowToPlay;
		}
		set
		{
			SetValue(ref viewedHideAndSeekHowToPlay, value, OnViewedHideAndSeekHowToPlay);
		}
	}

	public event Action OnLastAcceptedPrivacyPolicyVersionChanged;

	public event Action OnViewedOnlineHelpMessageChanged;

	public event Action OnViewedMinPlayerWarningChanged;

	public event Action OnAlwaysShowMinPlayerWarningChanged;

	public event Action OnViewedHideAndSeekHowToPlay;
}
