using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using InnerNet;
using Newtonsoft.Json;

namespace AmongUs.Data.Settings;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class MultiplayerSettingsData : AbstractSaveDataGroup
{
	public const bool DEFAULT_CROSS_PLAY = true;

	public const bool DEFAULT_ALLOW_FRIEND_INVITES = true;

	public const bool DEFAULT_CENSOR_CHAT = true;

	public const QuickChatModes DEFAULT_CHAT_MODE = QuickChatModes.FreeChatOrQuickChat;

	public const GameModes DEFAULT_LAST_PLAYED_GAMEMODE = GameModes.Normal;

	public static readonly StringNames[] DEFAULT_QUICK_CHAT_FAVORITES = new StringNames[0];

	public static readonly byte[] DEFAULT_RAW_GAME_OPTIONS = new byte[0];

	public static readonly DateTime DEFAULT_FILTER_DATE = DateTime.MinValue;

	public static readonly string[] DEFAULT_FILTER_TAGS = new string[0];

	public static readonly GameFilterOptions DEFAULT_VALID_GAME_FILTER_OPTIONS = new GameFilterOptions();

	public static readonly GameFilterOptions DEFAULT_HOST_GAME_FILTER_OPTIONS = new GameFilterOptions();

	public static readonly GameFilterOptions DEFAULT_SEARCH_GAME_FILTER_OPTIONS = new GameFilterOptions();

	[JsonProperty("crossPlay")]
	private bool crossPlay = true;

	[JsonProperty("allowFriendInvites")]
	private bool allowFriendInvites = true;

	[JsonProperty("censorChat")]
	private bool censorChat = true;

	[JsonProperty("chatMode")]
	private QuickChatModes chatMode = QuickChatModes.FreeChatOrQuickChat;

	[JsonProperty("quickChatFavs")]
	private List<StringNames> quickChatFavorites = new List<StringNames>(DEFAULT_QUICK_CHAT_FAVORITES);

	[JsonProperty("lastPlayedGameMode")]
	private GameModes lastPlayedGameMode = GameModes.Normal;

	[JsonProperty("normalHostOptions")]
	private byte[] rawNormalHostOptions = DEFAULT_RAW_GAME_OPTIONS;

	[JsonProperty("normalSearchOptions")]
	private byte[] rawNormalSearchOptions = DEFAULT_RAW_GAME_OPTIONS;

	[JsonProperty("hideNSeekHostOptions")]
	private byte[] rawHideNSeekHostOptions = DEFAULT_RAW_GAME_OPTIONS;

	[JsonProperty("hideNSeekSearchOptions")]
	private byte[] rawHideNSeekSearchOptions = DEFAULT_RAW_GAME_OPTIONS;

	[JsonProperty("gameFilterRefreshDate")]
	private DateTime validGameFilterLastRefreshDate = DEFAULT_FILTER_DATE;

	[JsonProperty("gameFilterTags")]
	private string[] validGameFilterTags = DEFAULT_FILTER_TAGS;

	[JsonProperty("hostFilterTags")]
	private string[] hostGameFilterTags = DEFAULT_FILTER_TAGS;

	[JsonProperty("searchFilterTags")]
	private string[] searchGameFilterTags = DEFAULT_FILTER_TAGS;

	private GameFilterOptions validGameFilterOptions = DEFAULT_VALID_GAME_FILTER_OPTIONS;

	private GameFilterOptions hostGameFilterOptions = DEFAULT_HOST_GAME_FILTER_OPTIONS;

	private GameFilterOptions searchGameFilterOptions = DEFAULT_SEARCH_GAME_FILTER_OPTIONS;

	public bool CrossPlay
	{
		get
		{
			return true;
		}
		set
		{
			SetValue(ref crossPlay, value, OnCrossPlayChanged);
		}
	}

	public bool AllowFriendInvites
	{
		get
		{
			return allowFriendInvites;
		}
		set
		{
			SetValue(ref allowFriendInvites, value, OnAllowFriendInvitesChanged);
		}
	}

	public bool CensorChat
	{
		get
		{
			return censorChat;
		}
		set
		{
			SetValue(ref censorChat, value, OnCensorChatChanged);
		}
	}

	public QuickChatModes ChatMode
	{
		get
		{
			if (chatMode == 0)
			{
				chatMode = QuickChatModes.FreeChatOrQuickChat;
			}
			return chatMode;
		}
		set
		{
			SetValue(ref chatMode, value, OnChatModeChanged);
		}
	}

	public GameModes LastPlayedGameMode
	{
		get
		{
			return lastPlayedGameMode;
		}
		set
		{
			SetValue(ref lastPlayedGameMode, value, OnLastPlayedGameModeChanged);
		}
	}

	public byte[] RawNormalHostOptions
	{
		get
		{
			return rawNormalHostOptions;
		}
		set
		{
			SetValue(ref rawNormalHostOptions, value, OnNormalHostOptionsChanged);
		}
	}

	public byte[] RawNormalSearchOptions
	{
		get
		{
			return rawNormalSearchOptions;
		}
		set
		{
			SetValue(ref rawNormalSearchOptions, value, OnNormalSearchOptionsChanged);
		}
	}

	public byte[] RawHideNSeekHostOptions
	{
		get
		{
			return rawHideNSeekHostOptions;
		}
		set
		{
			SetValue(ref rawHideNSeekHostOptions, value, OnHideNSeekHostOptionsChanged);
		}
	}

	public byte[] RawHideNSeekSearchOptions
	{
		get
		{
			return rawHideNSeekSearchOptions;
		}
		set
		{
			SetValue(ref rawHideNSeekSearchOptions, value, OnHideNSeekSearchOptionsChanged);
		}
	}

	public GameFilterOptions ValidGameFilterOptions
	{
		get
		{
			return validGameFilterOptions;
		}
		set
		{
			SetValue(ref validGameFilterOptions, value, OnValidGameFilterOptionsChanged);
		}
	}

	public GameFilterOptions HostGameFilterOptions
	{
		get
		{
			return hostGameFilterOptions;
		}
		set
		{
			SetValue(ref hostGameFilterOptions, value, OnHostGameFilterOptionsChanged);
		}
	}

	public GameFilterOptions SearchGameFilterOptions
	{
		get
		{
			return searchGameFilterOptions;
		}
		set
		{
			SetValue(ref searchGameFilterOptions, value, OnSearchGameFilterOptionsChanged);
		}
	}

	public event Action OnCrossPlayChanged;

	public event Action OnAllowFriendInvitesChanged;

	public event Action OnCensorChatChanged;

	public event Action OnChatModeChanged;

	public event Action OnQuickChatFavoritesChanged;

	public event Action OnLastPlayedGameModeChanged;

	public event Action OnNormalHostOptionsChanged;

	public event Action OnNormalSearchOptionsChanged;

	public event Action OnHideNSeekHostOptionsChanged;

	public event Action OnHideNSeekSearchOptionsChanged;

	public event Action OnValidGameFilterOptionsChanged;

	public event Action OnHostGameFilterOptionsChanged;

	public event Action OnSearchGameFilterOptionsChanged;

	protected override void OnSaveStart()
	{
		ValidateFilterTags();
		validGameFilterLastRefreshDate = ValidGameFilterOptions.LastRefreshTime;
		validGameFilterTags = ValidGameFilterOptions.FilterTags.ToArray();
		hostGameFilterTags = HostGameFilterOptions.FilterTags.ToArray();
		searchGameFilterTags = SearchGameFilterOptions.FilterTags.ToArray();
		base.OnSaveStart();
	}

	protected override void OnLoadComplete()
	{
		ValidGameFilterOptions.LastRefreshTime = validGameFilterLastRefreshDate;
		ValidGameFilterOptions.FilterTags = new HashSet<string>(validGameFilterTags);
		HostGameFilterOptions.FilterTags = new HashSet<string>(hostGameFilterTags);
		SearchGameFilterOptions.FilterTags = new HashSet<string>(searchGameFilterTags);
	}

	public bool HasQuickChatFavorite(StringNames key)
	{
		if (quickChatFavorites == null || quickChatFavorites.Count < 1)
		{
			return false;
		}
		return quickChatFavorites.Contains(key);
	}

	public void AddQuickChatFavorite(StringNames key)
	{
		if (!quickChatFavorites.Contains(key))
		{
			quickChatFavorites.Add(key);
			OnQuickChatFavoritesChanged?.Invoke();
			HandleChange();
		}
	}

	public void RemoveQuickChatFavorite(StringNames key)
	{
		if (quickChatFavorites.Contains(key))
		{
			quickChatFavorites.RemoveAll((StringNames x) => x == key);
			OnQuickChatFavoritesChanged?.Invoke();
			HandleChange();
		}
	}

	public void ValidateFilterTags()
	{
		if (ValidGameFilterOptions == null)
		{
			validGameFilterOptions = DEFAULT_VALID_GAME_FILTER_OPTIONS;
		}
		if (HostGameFilterOptions == null)
		{
			hostGameFilterOptions = DEFAULT_HOST_GAME_FILTER_OPTIONS;
		}
		if (SearchGameFilterOptions == null)
		{
			searchGameFilterOptions = DEFAULT_SEARCH_GAME_FILTER_OPTIONS;
		}
		if (ValidGameFilterOptions.FilterTags.Contains("STRMISS"))
		{
			ValidGameFilterOptions.FilterTags.Remove("STRMISS");
		}
		if (HostGameFilterOptions.FilterTags.Contains("STRMISS"))
		{
			HostGameFilterOptions.FilterTags.Remove("STRMISS");
		}
		if (SearchGameFilterOptions.FilterTags.Contains("STRMISS"))
		{
			SearchGameFilterOptions.FilterTags.Remove("STRMISS");
		}
		if (ValidGameFilterOptions.FilterTags.Count == 0)
		{
			validGameFilterOptions.ResetToDefaults(DateTime.MinValue);
		}
		HostGameFilterOptions.FilterTags.IntersectWith(validGameFilterOptions.FilterTags);
		if (HostGameFilterOptions.FilterTags.Count == 0)
		{
			HostGameFilterOptions.FilterTags.Add(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DefaultFilterTag_FirstTime));
		}
		SearchGameFilterOptions.FilterTags.IntersectWith(validGameFilterOptions.FilterTags);
		if (SearchGameFilterOptions.FilterTags.Count == 0)
		{
			SearchGameFilterOptions.FilterTags.Add(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DefaultFilterTag_FirstTime));
		}
	}
}
