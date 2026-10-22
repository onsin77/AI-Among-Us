using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Discord;
using InnerNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordManager : DestroyableSingleton<DiscordManager>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UpdateActivityHandler _003C_003E9__13_0;

		public static UpdateActivityHandler _003C_003E9__14_0;

		public static UpdateActivityHandler _003C_003E9__15_0;

		public static UpdateActivityHandler _003C_003E9__16_0;

		public static ClearActivityHandler _003C_003E9__17_0;

		public static UpdateActivityHandler _003C_003E9__18_0;

		public static SendRequestReplyHandler _003C_003E9__23_0;

		public static SendRequestReplyHandler _003C_003E9__24_0;

		internal void _003CSetInMenus_003Eb__13_0(Result r)
		{
		}

		internal void _003CSetPlayingGame_003Eb__14_0(Result r)
		{
		}

		internal void _003CSetHowToPlay_003Eb__15_0(Result r)
		{
		}

		internal void _003CSetInLobbyClient_003Eb__16_0(Result r)
		{
		}

		internal void _003CClearPresence_003Eb__17_0(Result r)
		{
		}

		internal void _003CSetInLobbyHost_003Eb__18_0(Result r)
		{
		}

		internal void _003CRequestRespondYes_003Eb__23_0(Result r)
		{
		}

		internal void _003CRequestRespondNo_003Eb__24_0(Result r)
		{
		}
	}

	private const long ClientId = 477175586805252107L;

	private const string DeeplinkScheme = "amongus";

	private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public GenericPopup discordPopup;

	[NonSerialized]
	private Discord presence;

	private DateTime? StartTime;

	public void Start()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected Obj, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		if ((Object)(object)DestroyableSingleton<DiscordManager>.Instance != (Object)(object)this)
		{
			return;
		}
		try
		{
			presence = new Discord(477175586805252107L, 1uL);
			ActivityManager activityManager = presence.GetActivityManager();
			activityManager.RegisterSteam(945360u);
			activityManager.OnActivityJoin += HandleJoinRequest;
			SetInMenus();
			SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
			{
				OnSceneChange(scene.name);
			};
		}
		catch
		{
			Debug.LogWarning((object)"DiscordManager: Discord messed up");
		}
	}

	private void OnSceneChange(string name)
	{
		switch (name)
		{
		case "MatchMaking":
		case "MMOnline":
		case "MainMenu":
			SetInMenus();
			break;
		}
	}

	public void FixedUpdate()
	{
		if (presence == null)
		{
			return;
		}
		try
		{
			presence.RunCallbacks();
		}
		catch (ResultException)
		{
			presence.Dispose();
			presence = null;
		}
	}

	public bool HasValidPartyID()
	{
		return false;
	}

	public bool CanLoginWithDiscord()
	{
		return false;
	}

	public void LoginWithDiscord()
	{
		Debug.Log((object)"DiscordManager: Logging in with discord");
	}

	public void Logout()
	{
	}

	public void SetInMenus()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected Obj, but got Unknown
		StartTime = null;
		if (presence != null)
		{
			ClearPresence();
			Activity val = new Activity
			{
				State = "In Menus",
				Assets = 
				{
					LargeImage = "icon"
				}
			};
			ActivityManager activityManager = presence.GetActivityManager();
			UpdateActivityHandler val2 = _003C_003Ec._003C_003E9__13_0;
			if (val2 == null)
			{
				UpdateActivityHandler val3 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__13_0 = val3;
				val2 = val3;
			}
			activityManager.UpdateActivity(val, val2);
		}
	}

	public void SetPlayingGame()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected Obj, but got Unknown
		if (!StartTime.HasValue)
		{
			StartTime = DateTime.UtcNow;
		}
		if (presence != null)
		{
			Activity val = new Activity
			{
				State = "In Game",
				Details = "Playing",
				Assets = 
				{
					LargeImage = "icon"
				},
				Timestamps = 
				{
					Start = ToUnixTime(StartTime.Value)
				}
			};
			ActivityManager activityManager = presence.GetActivityManager();
			UpdateActivityHandler val2 = _003C_003Ec._003C_003E9__14_0;
			if (val2 == null)
			{
				UpdateActivityHandler val3 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__14_0 = val3;
				val2 = val3;
			}
			activityManager.UpdateActivity(val, val2);
		}
	}

	public void SetHowToPlay()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected Obj, but got Unknown
		if (presence != null)
		{
			ClearPresence();
			Activity val = new Activity
			{
				State = "In Freeplay",
				Assets = 
				{
					LargeImage = "icon"
				}
			};
			ActivityManager activityManager = presence.GetActivityManager();
			UpdateActivityHandler val2 = _003C_003Ec._003C_003E9__15_0;
			if (val2 == null)
			{
				UpdateActivityHandler val3 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__15_0 = val3;
				val2 = val3;
			}
			activityManager.UpdateActivity(val, val2);
		}
	}

	public void SetInLobbyClient(int numPlayers, int maxPlayers, int gameId)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected Obj, but got Unknown
		if (!StartTime.HasValue)
		{
			StartTime = DateTime.UtcNow;
		}
		string id = GameCode.IntToGameName(gameId);
		if (presence != null)
		{
			ClearPresence();
			Activity val = new Activity
			{
				State = "In Lobby",
				Assets = 
				{
					LargeImage = "icon"
				},
				Timestamps = 
				{
					Start = ToUnixTime(StartTime.Value)
				},
				Party = 
				{
					Size = 
					{
						CurrentSize = numPlayers
					}
				},
				Party = 
				{
					Size = 
					{
						MaxSize = maxPlayers
					}
				},
				Party = 
				{
					Id = id
				}
			};
			ActivityManager activityManager = presence.GetActivityManager();
			UpdateActivityHandler val2 = _003C_003Ec._003C_003E9__16_0;
			if (val2 == null)
			{
				UpdateActivityHandler val3 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__16_0 = val3;
				val2 = val3;
			}
			activityManager.UpdateActivity(val, val2);
		}
	}

	private void ClearPresence()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		if (presence != null)
		{
			ActivityManager activityManager = presence.GetActivityManager();
			ClearActivityHandler val = _003C_003Ec._003C_003E9__17_0;
			if (val == null)
			{
				ClearActivityHandler val2 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__17_0 = val2;
				val = val2;
			}
			activityManager.ClearActivity(val);
		}
	}

	public void SetInLobbyHost(int numPlayers, int maxPlayers, int gameId)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected Obj, but got Unknown
		if (!StartTime.HasValue)
		{
			StartTime = DateTime.UtcNow;
		}
		string text = GameCode.IntToGameName(gameId);
		if (presence != null)
		{
			Activity val = new Activity
			{
				State = "In Lobby",
				Details = "Hosting a game",
				Assets = 
				{
					LargeImage = "icon"
				},
				Assets = 
				{
					LargeText = "Ask to play!"
				},
				Party = 
				{
					Size = 
					{
						CurrentSize = numPlayers
					}
				},
				Party = 
				{
					Size = 
					{
						MaxSize = maxPlayers
					}
				},
				Secrets = 
				{
					Join = "join" + ReverseString(text)
				},
				Secrets = 
				{
					Match = "match" + ReverseString(text)
				},
				Party = 
				{
					Id = text
				},
				SupportedPlatforms = 7u
			};
			ActivityManager activityManager = presence.GetActivityManager();
			UpdateActivityHandler val2 = _003C_003Ec._003C_003E9__18_0;
			if (val2 == null)
			{
				UpdateActivityHandler val3 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__18_0 = val3;
				val2 = val3;
			}
			activityManager.UpdateActivity(val, val2);
		}
	}

	public bool CanShareGameOnDiscord()
	{
		return false;
	}

	public void ShareGameOnDiscord()
	{
	}

	private void HandleJoinRequest(string joinSecret)
	{
		if (!joinSecret.StartsWith("join"))
		{
			Debug.LogWarning((object)("DiscordManager: Invalid join secret: " + joinSecret));
			return;
		}
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoJoinGame(joinSecret));
	}

	private IEnumerator CoJoinGame(string joinSecret)
	{
		if (DestroyableSingleton<StoreMenu>.InstanceExists)
		{
			DestroyableSingleton<StoreMenu>.Instance.CloseEntirely();
		}
		while (!DestroyableSingleton<EOSManager>.InstanceExists)
		{
			yield return null;
		}
		yield return DestroyableSingleton<EOSManager>.Instance.WaitForLoginFlow();
		while (!DestroyableSingleton<ServerManager>.InstanceExists)
		{
			yield return null;
		}
		yield return DestroyableSingleton<ServerManager>.Instance.WaitForServers();
		while (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			yield return null;
		}
		if (AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			Debug.LogWarning((object)"DiscordManager: Already connected");
			yield break;
		}
		Debug.Log((object)("DiscordManager: Joining game: " + joinSecret));
		int gameId = GameCode.GameNameToInt(ReverseString(joinSecret.Substring(4)));
		yield return AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameId);
	}

	public void RequestRespondYes(long userId)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected Obj, but got Unknown
		if (presence != null)
		{
			ActivityManager activityManager = presence.GetActivityManager();
			SendRequestReplyHandler val = _003C_003Ec._003C_003E9__23_0;
			if (val == null)
			{
				SendRequestReplyHandler val2 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__23_0 = val2;
				val = val2;
			}
			activityManager.SendRequestReply(userId, (ActivityJoinRequestReply)1, val);
		}
	}

	public void RequestRespondNo(long userId)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		if (presence != null)
		{
			Debug.Log((object)"DiscordManager: responding no to Ask to Join request");
			ActivityManager activityManager = presence.GetActivityManager();
			SendRequestReplyHandler val = _003C_003Ec._003C_003E9__24_0;
			if (val == null)
			{
				SendRequestReplyHandler val2 = (Result r) =>
				{
				};
				_003C_003Ec._003C_003E9__24_0 = val2;
				val = val2;
			}
			activityManager.SendRequestReply(userId, (ActivityJoinRequestReply)0, val);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (presence != null)
		{
			presence.Dispose();
		}
	}

	private static string ReverseString(string source)
	{
		char[] array = source.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	private static long ToUnixTime(DateTime time)
	{
		return (long)(time - epoch).TotalSeconds;
	}
}
