using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.Data;
using Assets.InnerNet;
using InnerNet;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public class FriendsListManager : DestroyableSingleton<FriendsListManager>
{
	public class PlatformFriend
	{
		public string Username;

		public ulong GUID;

		public CSteamID CSteamID;

		public PlatformFriend(string name, CSteamID CSteamID)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Username = name;
			this.CSteamID = CSteamID;
		}
	}

	public static CSteamID myId;

	private Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequested;

	public JoinGameButton joinGameButton;

	public List<ResponseFriends> Friends = new List<ResponseFriends>();

	public List<PlatformFriend> PlatformFriends = new List<PlatformFriend>();

	public List<ResponseBlockedPlayer> BlockedPlayers = new List<ResponseBlockedPlayer>();

	public List<ResponseBlockedBy> BlockedByPlayers = new List<ResponseBlockedBy>();

	public List<ResponseReceivedFriendRequest> ReceivedRequests = new List<ResponseReceivedFriendRequest>();

	public List<LobbyInvite> ReceivedLobbyInvites = new List<LobbyInvite>();

	public List<GameData.PlayerInfo> RecentlyPlayedWith = new List<GameData.PlayerInfo>();

	public FriendListNotification FriendListNotification;

	public FriendsListConfirmMenu ConfirmationScreen;

	public FriendsListUI Ui;

	public FriendsListButton FriendsListButton;

	public SpriteRenderer LoadScreen;

	public bool FriendCodeHidden = true;

	public Sprite GlobalPlayerIcon;

	[HideInInspector]
	public Dictionary<int, StringNames> ErrorCodes = new Dictionary<int, StringNames>
	{
		{
			1003,
			StringNames.ErrorBadUsername
		},
		{
			1201,
			StringNames.AlreadyBlocked
		},
		{
			1209,
			StringNames.ErrorThisIsYou
		},
		{
			1300,
			StringNames.ErrorUserNotFound
		},
		{
			3500,
			StringNames.ErrorFriendRequestExists
		},
		{
			3501,
			StringNames.ErrorAlreadyFriends
		},
		{
			3512,
			StringNames.ErrorPlayerBlockedYou
		},
		{
			3513,
			StringNames.ErrorRecipientMaxFriendRequests
		},
		{
			3515,
			StringNames.ErrorMaxFriends
		},
		{
			3516,
			StringNames.ErrorRecipientMaxFriends
		},
		{
			1337420,
			StringNames.UserIdTokenError
		}
	};

	private readonly Logger logger = new Logger("FriendsListManager", Logger.Level.Info, Logger.Category.Network);

	private Coroutine pollingRoutine;

	private bool gotFriendRequests;

	private bool gotLobbyInvites;

	[SerializeField]
	private FriendsListConfirmMenu ConfirmationScreenPrefab;

	[SerializeField]
	private FriendsListUI UiPrefab;

	[SerializeField]
	private FriendsListButton FriendsListButtonPrefab;

	private void OnAwake()
	{
		if (SteamManager.Initialized)
		{
			m_GameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create((DispatchDelegate<GameRichPresenceJoinRequested_t>)HandleGameInvite);
		}
	}

	private void HandleGameInvite(GameRichPresenceJoinRequested_t param)
	{
		if (SteamManager.Initialized)
		{
			int gameId = GameCode.GameNameToInt(param.m_rgchConnect);
			((MonoBehaviour)this).StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameId));
			logger.Info("Got steam invite with code: " + param.m_rgchConnect);
		}
	}

	public IEnumerator PopulatePlatformFriendList()
	{
		yield return null;
		if (SteamManager.Initialized)
		{
			List<PlatformFriend> list = new List<PlatformFriend>();
			int friendCount = SteamFriends.GetFriendCount((EFriendFlags)4);
			myId = SteamUser.GetSteamID();
			for (int i = 0; i < friendCount; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, (EFriendFlags)4);
				list.Add(new PlatformFriend(SteamFriends.GetFriendPersonaName(friendByIndex), friendByIndex));
			}
			list.OrderBy((PlatformFriend a) => a.Username).ToList();
			PlatformFriends = list;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if ((Object)(object)DestroyableSingleton<FriendsListManager>.Instance == (Object)(object)this)
		{
			FriendsListUI.Instance = Ui;
		}
		DataManager.Settings.Multiplayer.OnAllowFriendInvitesChanged += OnAllowFriendInvitesChanged;
		OnAwake();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		DataManager.Settings.Multiplayer.OnAllowFriendInvitesChanged -= OnAllowFriendInvitesChanged;
	}

	private void OnAllowFriendInvitesChanged()
	{
		if (DataManager.Settings.Multiplayer.AllowFriendInvites)
		{
			StartPolling();
		}
		else
		{
			StopPolling();
		}
	}

	private bool SetRequestHeaders(UnityWebRequest request)
	{
		request.SetRequestHeader("Content-Type", "application/vnd.api+json");
		request.SetRequestHeader("Accept", "application/vnd.api+json");
		if (string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.UserIDToken))
		{
			logger.Error("Cannot make friend list request, UserIDToken does not exist");
			return false;
		}
		request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		return true;
	}

	public void OnSignIn()
	{
		((MonoBehaviour)this).StartCoroutine(RefreshFriendsList(null, checkPSNBlocked: false));
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			StartPolling();
		}
	}

	public bool HasPlayerBlockedMeUsername(string friendcode)
	{
		if (BlockedByPlayers.Find((ResponseBlockedBy p) => p.FriendCode == friendcode) != null)
		{
			return true;
		}
		return false;
	}

	public bool HasPlayerBlockedMe(string puid)
	{
		if (BlockedByPlayers.Find((ResponseBlockedBy p) => p.SenderId == puid) != null)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerBlocked(string puid)
	{
		if (BlockedPlayers.Find((ResponseBlockedPlayer p) => p.FriendPuid == puid) != null)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerFriend(string puid)
	{
		if (Friends.Find((ResponseFriends p) => p.FriendPuid == puid) != null)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerBlockedUsername(string friendcode)
	{
		if (BlockedPlayers.Find((ResponseBlockedPlayer p) => p.FriendCode == friendcode) != null)
		{
			return true;
		}
		return false;
	}

	public IEnumerator RefreshFriendsList(Action cb, bool checkPSNBlocked = true)
	{
		bool gotBlocks = false;
		bool gotFriends = false;
		DestroyableSingleton<FriendsListManager>.Instance.GetBlockedPlayers((ResponseState responseState, Response<ResponseBlockedWrapper> response) =>
		{
			if (responseState == ResponseState.Success)
			{
				BlockedPlayers = response.Data?.Attributes?.Blocking.Select((NestedResponseData<ResponseBlockedPlayer> f) => f.Attributes).ToList();
				BlockedByPlayers = response.Data?.Attributes?.BlockedBy.Select((NestedResponseData<ResponseBlockedBy> f) => f.Attributes).ToList();
				if (BlockedPlayers == null)
				{
					BlockedPlayers = new List<ResponseBlockedPlayer>();
				}
			}
			gotBlocks = true;
		});
		DestroyableSingleton<FriendsListManager>.Instance.GetFriends((ResponseState responseState, Response<ResponseFriendWrapper> response) =>
		{
			if (responseState == ResponseState.Success)
			{
				Friends = response.Data?.Attributes?.Friends.Select((NestedResponseData<ResponseFriends> f) => f.Attributes).OrderBy((ResponseFriends x) => x.FriendCode).ToList();
			}
			gotFriends = true;
		});
		yield return ((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.PopulatePlatformFriendList());
		while (!gotBlocks || !gotFriends)
		{
			yield return null;
		}
		cb?.Invoke();
	}

	public void OnSignOut()
	{
		StopPolling();
	}

	private void StartPolling()
	{
		StopPolling();
		pollingRoutine = ((MonoBehaviour)this).StartCoroutine(PollForRequests());
	}

	private void StopPolling()
	{
		if (pollingRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(pollingRoutine);
			pollingRoutine = null;
		}
	}

	public void SetRecentlyPlayed(List<GameData.PlayerInfo> players)
	{
		RecentlyPlayedWith = players;
		UpdateFriendsDataWithPlayers(players);
	}

	public void UpdateFriendsDataWithPlayers(List<GameData.PlayerInfo> players)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		if (players == null || players.Count == 0)
		{
			return;
		}
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder(string.Format("Updating {0} with player info from {1} players!)", "PlayerFriendData", players.Count));
		foreach (GameData.PlayerInfo player in players)
		{
			if (player == null || string.IsNullOrWhiteSpace(player.PlayerName) || string.IsNullOrWhiteSpace(player.Puid) || player.IsIncomplete)
			{
				stringBuilder.AppendLine(" - Player " + "[Uninitialized]".Color(Palette.LogErrorColor));
				continue;
			}
			stringBuilder.AppendLine(" - " + player.PlayerName + " (PUID: " + player.Puid + ") ");
			if (IsPlayerFriend(player.Puid))
			{
				stringBuilder.Append("[Friend] ".Color(Palette.LogSuccessColor));
				string cachedName = DataManager.Player.Friends.GetCachedName(player.Puid);
				if (string.IsNullOrWhiteSpace(cachedName) || cachedName != player.PlayerName)
				{
					stringBuilder.Append("[Cached new name]".Color(Palette.LogWarningColor));
					DataManager.Player.Friends.UpdateCachedName(player.Puid, player.PlayerName);
					flag = true;
				}
			}
		}
		if (flag)
		{
			logger.Info(stringBuilder);
			DataManager.Player.Save();
		}
	}

	public void ReparentUI()
	{
		if ((Object)(object)FriendsListButton == (Object)null)
		{
			FriendsListButton = Object.Instantiate<FriendsListButton>(FriendsListButtonPrefab, ((Component)this).transform);
		}
		if ((Object)(object)Ui == (Object)null)
		{
			Ui = Object.Instantiate<FriendsListUI>(UiPrefab, ((Component)this).transform);
		}
		if ((Object)(object)ConfirmationScreen == (Object)null)
		{
			ConfirmationScreen = Object.Instantiate<FriendsListConfirmMenu>(ConfirmationScreenPrefab, ((Component)this).transform);
		}
		((Component)FriendsListButton).gameObject.transform.SetParent(((Component)DestroyableSingleton<FriendsListManager>.Instance).transform);
		((Component)Ui).gameObject.transform.SetParent(((Component)DestroyableSingleton<FriendsListManager>.Instance).transform);
		((Component)ConfirmationScreen).gameObject.transform.SetParent(((Component)DestroyableSingleton<FriendsListManager>.Instance).transform);
	}

	public void OpenUi()
	{
		Ui.Open();
	}

	public IEnumerator CheckFriendCodeOnLogin()
	{
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.GetFriendCode((ResponseState cb, Response<ResponseFriendCode> response) =>
			{
				if (cb == ResponseState.Success)
				{
					DestroyableSingleton<EOSManager>.Instance.FriendCode = response.Data.Attributes.Username + "#" + response.Data.Attributes.Discriminator;
				}
				else
				{
					if (response != null)
					{
						ResponseError[] errors = response.Errors;
						if (errors != null && errors[0].Detail == 3506)
						{
							((Component)DestroyableSingleton<EOSManager>.Instance.editAccountUsername).gameObject.SetActive(true);
							return;
						}
					}
					logger.Error("Problem getting Friend Code: " + ((response != null) ? response.Errors[0] : null));
					DestroyableSingleton<AccountManager>.Instance.SignInFail(EOSManager.EOS_ERRORS.FriendCodeFail, DestroyableSingleton<EOSManager>.Instance.ContinueInOfflineMode);
				}
			}));
			while (DestroyableSingleton<EOSManager>.Instance.FriendCode == null || DestroyableSingleton<EOSManager>.Instance.FriendCode == string.Empty || ((Component)DestroyableSingleton<EOSManager>.Instance.editAccountUsername).gameObject.activeSelf)
			{
				yield return null;
			}
		}
		else
		{
			DestroyableSingleton<EOSManager>.Instance.FriendCode = null;
		}
	}

	public IEnumerator GetFriendCode(Action<ResponseState, Response<ResponseFriendCode>> resultCallback)
	{
		string friendcodeEndpoint = BackendEndpoints.FriendcodeEndpoint;
		yield return ((MonoBehaviour)this).StartCoroutine(Get(friendcodeEndpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendCode> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendCode>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to retrieve friend code: " + text);
			}
		}));
	}

	public void SetFriendCode(string username, Action<ResponseState, Response<ResponseFriendCode>> resultCallback)
	{
		string friendcodeEndpoint = BackendEndpoints.FriendcodeEndpoint;
		string jsonContent = JsonConvert.SerializeObject((object)FriendsListRequest.ChangeUsername(username));
		((MonoBehaviour)this).StartCoroutine(Post(friendcodeEndpoint, jsonContent, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendCode> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendCode>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to set friend code: " + text);
			}
		}));
	}

	public void OpenConfirmationScreen(Action confirm, FriendsListConfirmMenu.ActionType type, string recipientFriendCode)
	{
		((MonoBehaviour)this).StartCoroutine(ConfirmationScreen.WaitForConfirm(confirm, type, recipientFriendCode));
	}

	private IEnumerator PollForRequests()
	{
		while (true)
		{
			while (!DataManager.Settings.Multiplayer.AllowFriendInvites)
			{
				yield return null;
			}
			gotFriendRequests = false;
			gotLobbyInvites = false;
			float pollingInterval = 5f;
			if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed() && !Object.op_Implicit((Object)(object)ShipStatus.Instance))
			{
				DestroyableSingleton<FriendsListManager>.Instance.GetReceivedFriendRequests((ResponseState cb, Response<ResponseReceivedFriendRequestWrapper> response) =>
				{
					if (cb == ResponseState.Success && DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
					{
						List<ResponseReceivedFriendRequest> list = response.Data?.Attributes?.Senders.Select((NestedResponseData<ResponseReceivedFriendRequest> f) => f.Attributes).ToList();
						list.RemoveAll((ResponseReceivedFriendRequest f) => IsPlayerBlocked(f.SenderId));
						List<string> list2 = list.Select((ResponseReceivedFriendRequest inv) => inv.SenderFriendCode).Except(ReceivedRequests.Select((ResponseReceivedFriendRequest inv) => inv.SenderFriendCode)).ToList();
						ReceivedRequests = list;
						if (list2.Count > 0)
						{
							DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NewRequests, list2.Count), FriendListNotification.IconType.Friend);
						}
					}
					gotFriendRequests = true;
				});
				DestroyableSingleton<FriendsListManager>.Instance.GetLobbyInvites((ResponseState cb, Response<ResponseGameInvites> response) =>
				{
					if (cb == ResponseState.Success && DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
					{
						pollingInterval = response.Data?.Attributes?.NextPollSeconds ?? 5f;
						List<LobbyInvite> list = response.Data?.Attributes?.Invites.Select((NestedResponseData<LobbyInvite> f) => f.Attributes).ToList();
						list.RemoveAll((LobbyInvite f) => IsPlayerBlocked(f.SenderId));
						List<string> list2 = list.Select((LobbyInvite inv) => inv.RoomCode).Except(ReceivedLobbyInvites.Select((LobbyInvite inv) => inv.RoomCode)).ToList();
						ReceivedLobbyInvites = list;
						if (list2.Count > 0)
						{
							DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NewRequests, list2.Count), FriendListNotification.IconType.LobbyInvite);
						}
					}
					gotLobbyInvites = true;
				});
				while (!gotFriendRequests || !gotLobbyInvites)
				{
					yield return null;
				}
				FriendsListButton.CheckNotifCount();
				if (DestroyableSingleton<FriendsListManager>.Instance.Ui.IsOpen && DestroyableSingleton<FriendsListManager>.Instance.Ui.CurrentTab != FriendsListUI.FriendsListTab.Notifications)
				{
					DestroyableSingleton<FriendsListManager>.Instance.Ui.RefreshNotifications();
				}
			}
			pollingInterval = Mathf.Clamp(pollingInterval, 3f, 60f);
			yield return (object)new WaitForSecondsRealtime(pollingInterval);
		}
	}

	public void GetFriends(Action<ResponseState, Response<ResponseFriendWrapper>> resultCallback)
	{
		string friendEndpoint = BackendEndpoints.FriendEndpoint;
		((MonoBehaviour)this).StartCoroutine(Get(friendEndpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendWrapper> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendWrapper>>(response);
			if (response2 == null)
			{
				response2 = new Response<ResponseFriendWrapper>();
			}
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to retrieve friends: " + text);
			}
		}));
	}

	public void GetReceivedFriendRequests(Action<ResponseState, Response<ResponseReceivedFriendRequestWrapper>> resultCallback)
	{
		string friendRequestsEndpoint = BackendEndpoints.FriendRequestsEndpoint;
		((MonoBehaviour)this).StartCoroutine(Get(friendRequestsEndpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseReceivedFriendRequestWrapper> response2 = JsonConvert.DeserializeObject<Response<ResponseReceivedFriendRequestWrapper>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to retrieve friend request: " + text);
			}
		}));
	}

	public void SendFriendRequest(string recipientPuid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string friendSendRequestEndpoint = BackendEndpoints.FriendSendRequestEndpoint;
		string jsonContent = JsonConvert.SerializeObject((object)FriendsListRequest.SendRequest(recipientPuid, "friend_request"));
		((MonoBehaviour)this).StartCoroutine(Post(friendSendRequestEndpoint, jsonContent, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to send friend request by puid: " + text);
			}
			else
			{
				logger.Info("Sent friend request successfully by puid");
			}
		}));
	}

	public void SendFriendRequestByUsername(string username, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string friendSendRequestByFriendcodeEndpoint = BackendEndpoints.FriendSendRequestByFriendcodeEndpoint;
		string jsonContent = JsonConvert.SerializeObject((object)FriendsListRequest.SendRequestByUsername(username));
		((MonoBehaviour)this).StartCoroutine(Post(friendSendRequestByFriendcodeEndpoint, jsonContent, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to send friend request by username: " + text);
			}
			else
			{
				logger.Info("Sent friend request successfully by username");
			}
		}));
	}

	public void DenyFriendRequest(string puid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string endpoint = BackendEndpoints.FriendDenyRequestEndpoint(puid);
		((MonoBehaviour)this).StartCoroutine(Delete(endpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Info("Failed to delete friend request: " + text);
			}
		}));
	}

	public void RemoveFriend(string recipientPuid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string endpoint = BackendEndpoints.RemoveFriendEndpoint(recipientPuid);
		((MonoBehaviour)this).StartCoroutine(Delete(endpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to remove friend: " + text);
			}
			else
			{
				logger.Info("Removed friend successfully");
			}
		}));
	}

	public void BlockPlayer(string recipientPuid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string blocksEndpoint = BackendEndpoints.BlocksEndpoint;
		string jsonContent = JsonConvert.SerializeObject((object)FriendsListRequest.SendRequest(recipientPuid, "block_request"));
		((MonoBehaviour)this).StartCoroutine(Post(blocksEndpoint, jsonContent, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to block player: " + text);
			}
			else
			{
				logger.Info("Blocked player successfully");
			}
		}));
	}

	public void UnblockPlayer(string recipientPuid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string endpoint = BackendEndpoints.UnblockEndpoint(recipientPuid);
		((MonoBehaviour)this).StartCoroutine(Delete(endpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to unblock player: " + text);
			}
			else
			{
				logger.Info("Unblocked player successfully");
			}
		}));
	}

	public void GetBlockedPlayers(Action<ResponseState, Response<ResponseBlockedWrapper>> resultCallback)
	{
		string blocksEndpoint = BackendEndpoints.BlocksEndpoint;
		((MonoBehaviour)this).StartCoroutine(Get(blocksEndpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseBlockedWrapper> response2 = JsonConvert.DeserializeObject<Response<ResponseBlockedWrapper>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to retrieve blocked players: " + text);
			}
		}));
	}

	public void SendGameInvite(string recipientPuid, string roomCode, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		if (HasPlayerBlockedMe(recipientPuid))
		{
			return;
		}
		string lobbyInviteEndpoint = BackendEndpoints.LobbyInviteEndpoint;
		string jsonContent = JsonConvert.SerializeObject((object)GameInviteRequest.SendGameInvite(recipientPuid, roomCode));
		((MonoBehaviour)this).StartCoroutine(Post(lobbyInviteEndpoint, jsonContent, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to send game invite: " + text);
			}
			else
			{
				logger.Info("Sent game invite successfully");
			}
		}));
	}

	public void GetLobbyInvites(Action<ResponseState, Response<ResponseGameInvites>> resultCallback)
	{
		string lobbyInviteEndpoint = BackendEndpoints.LobbyInviteEndpoint;
		((MonoBehaviour)this).StartCoroutine(Get(lobbyInviteEndpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseGameInvites> response2 = JsonConvert.DeserializeObject<Response<ResponseGameInvites>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to retrieve game invites: " + text);
			}
		}));
	}

	public void DenyLobbyInvite(string recipientPuid, Action<ResponseState, Response<ResponseFriendsListRequest>> resultCallback)
	{
		string endpoint = BackendEndpoints.LobbyDenyInviteEndpoint(recipientPuid);
		((MonoBehaviour)this).StartCoroutine(Delete(endpoint, (ResponseState cb, string response) =>
		{
			Response<ResponseFriendsListRequest> response2 = JsonConvert.DeserializeObject<Response<ResponseFriendsListRequest>>(response);
			resultCallback(cb, response2);
			if (cb != ResponseState.Success)
			{
				string text = "null";
				if (response2?.Errors != null)
				{
					text = string.Join("\r\n", response2.Errors.Select((ResponseError s) => s.ToString()));
				}
				logger.Error("Failed to delete lobby invite: " + text);
			}
		}));
	}

	private IEnumerator Get(string endpoint, Action<ResponseState, string> resultCallback)
	{
		UnityWebRequest request = UnityWebRequest.Get(endpoint);
		if (!SetRequestHeaders(request))
		{
			logger.Error("Failed to call GET friends API, probably because the UserIdToken does not exist");
			resultCallback(ResponseState.Failed, Constants.UserIdTokenError);
			yield break;
		}
		yield return request.SendWebRequest();
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			resultCallback(ResponseState.Failed, request.downloadHandler.text);
		}
		else
		{
			resultCallback(ResponseState.Success, request.downloadHandler.text);
		}
	}

	private IEnumerator Delete(string endpoint, Action<ResponseState, string> resultCallback)
	{
		UnityWebRequest request = UnityWebRequest.Delete(endpoint);
		if (!SetRequestHeaders(request))
		{
			logger.Error("Failed to call DELETE friends API, probably because the UserIdToken does not exist");
			resultCallback(ResponseState.Failed, Constants.UserIdTokenError);
			yield break;
		}
		yield return request.SendWebRequest();
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			string arg = string.Empty;
			if (request.downloadHandler != null)
			{
				arg = request.downloadHandler.text;
			}
			resultCallback(ResponseState.Failed, arg);
		}
		else
		{
			string arg2 = string.Empty;
			if (request.downloadHandler != null)
			{
				arg2 = request.downloadHandler.text;
			}
			resultCallback(ResponseState.Success, arg2);
		}
	}

	private IEnumerator Post(string endpoint, string jsonContent, Action<ResponseState, string> resultCallback)
	{
		UnityWebRequest request = UnityWebRequest.Post(endpoint, jsonContent);
		if (!SetRequestHeaders(request))
		{
			logger.Error("Failed to call POST friends API, probably because the UserIdToken does not exist");
			resultCallback(ResponseState.Failed, Constants.UserIdTokenError);
			yield break;
		}
		UploadHandlerRaw val = new UploadHandlerRaw(Encoding.ASCII.GetBytes(jsonContent));
		((UploadHandler)val).contentType = "application/vnd.api+json";
		request.uploadHandler = (UploadHandler)(object)val;
		yield return request.SendWebRequest();
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			resultCallback(ResponseState.Failed, request.downloadHandler.text);
		}
		else
		{
			resultCallback(ResponseState.Success, request.downloadHandler.text);
		}
	}

	public void SetFriendButtonColor(bool isGrayedOut)
	{
		FriendsListButton.SetGlyphColor(isGrayedOut);
	}
}
