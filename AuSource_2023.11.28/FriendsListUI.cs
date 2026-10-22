using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using Assets.InnerNet;
using InnerNet;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FriendsListUI : MonoBehaviour
{
	[Serializable]
	public class FriendsListTabButton
	{
		public FriendsListTab tab;

		public GameObject tabObject;

		public List<SpriteRenderer> selectedSprites;

		public bool selectable = true;
	}

	[Serializable]
	public enum FriendsListTab
	{
		LobbyPlayers,
		RecentPlayers,
		AmongUsFriends,
		Blocked,
		Notifications,
		AddFriend
	}

	public static FriendsListUI Instance;

	public TextMeshPro FriendCodeText;

	public SpriteRenderer FriendCodeHiddenIcon;

	public GameObject FriendCodeHideToggleObject;

	public FriendsListTabButton[] Tabs;

	public GameObject NotifArea;

	public GameObject BlockedArea;

	public GameObject FriendArea;

	public GameObject PlatformFriendArea;

	public GameObject LobbyPlayerArea;

	public GameObject RecentlyPlayedArea;

	public GameObject LobbyPlayersTab;

	public GameObject LobbyPlayersInactiveTab;

	public GameObject PlatformFriendsButton;

	public FriendRequestBar FriendRequestBar;

	public BlockedPlayerBar BlockedPlayerBar;

	public OnlineFriendBar OnlineFriendBar;

	public OnlineFriendBar PlatformFriendBar;

	public LobbyPlayerBar LobbyPlayerBar;

	public LobbyInviteBar LobbyInviteBar;

	public Scroller FriendsScroller;

	public Scroller PlatformFriendsScroller;

	public Scroller BlockedScroller;

	public Scroller NotifScroller;

	public Scroller LobbyScroller;

	public Scroller RecentlyPlayedScroller;

	public SpriteRenderer InactiveAllFriends;

	public SpriteRenderer InactivePlatformFriends;

	public TextBoxTMP AddFriendArea;

	public List<FriendsListGuestWarning> guestAccountWarnings;

	public float YStart;

	public float YOffset;

	public SpriteRenderer ViewRequestsButton;

	public TextMeshPro ViewRequestsText;

	public Color NewRequestColor;

	public Color NoRequestsColor;

	public GameObject AddFriendObjects;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public GameObject glyphL;

	public GameObject glyphR;

	private int selectedTab;

	private List<FriendsListBar> lobbyBars;

	private List<FriendsListBar> friendBars;

	private List<FriendsListBar> platformFriendBars;

	private List<FriendsListBar> notifBars;

	private string friendCodeHiddenText = "XXXXXXXXXX";

	private bool viewingAllFriends = true;

	private string currentSceneName;

	private float addFriendCooldown;

	public FriendsListTab CurrentTab => Tabs[selectedTab].tab;

	public bool IsOpen => ((Component)this).gameObject.activeSelf;

	public void Open()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeSelf || currentSceneName == "")
		{
			Close();
			return;
		}
		FriendsListBar[] componentsInChildren = ((Component)this).GetComponentsInChildren<FriendsListBar>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)componentsInChildren[i]).gameObject);
			}
		}
		Scene activeScene = SceneManager.GetActiveScene();
		currentSceneName = activeScene.name;
		UpdateFriendCodeUI();
		if ((DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening) || (Object)(object)ShipStatus.Instance != (Object)null)
		{
			return;
		}
		friendBars = new List<FriendsListBar>();
		lobbyBars = new List<FriendsListBar>();
		notifBars = new List<FriendsListBar>();
		platformFriendBars = new List<FriendsListBar>();
		viewingAllFriends = true;
		((Component)this).gameObject.SetActive(true);
		guestAccountWarnings.ForEach((FriendsListGuestWarning t) =>
		{
			((Component)t).gameObject.SetActive(false);
		});
		ViewRequestsButton.color = NoRequestsColor;
		((TMP_Text)ViewRequestsText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoNewRequests);
		((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.RefreshFriendsList(() =>
		{
			ClearNotifs();
			if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
			{
				AddFriendObjects.SetActive(true);
				RefreshBlockedPlayers();
				RefreshFriends();
				RefreshNotifications();
			}
			else
			{
				AddFriendObjects.SetActive(false);
				guestAccountWarnings.ForEach((FriendsListGuestWarning g) =>
				{
					g.SetUp();
				});
			}
			RefreshRecentlyPlayed();
			RefreshPlatformFriends();
			foreach (FriendsListBar friendBar in friendBars)
			{
				foreach (PassiveButton item in friendBar.ControllerSelectable)
				{
					ControllerManager.Instance.AddSelectableUiElement(item);
				}
			}
			foreach (FriendsListBar platformFriendBar in platformFriendBars)
			{
				foreach (PassiveButton item2 in platformFriendBar.ControllerSelectable)
				{
					ControllerManager.Instance.AddSelectableUiElement(item2);
				}
			}
			foreach (FriendsListBar notifBar in notifBars)
			{
				foreach (PassiveButton item3 in notifBar.ControllerSelectable)
				{
					ControllerManager.Instance.AddSelectableUiElement(item3);
				}
			}
			foreach (FriendsListBar lobbyBar in lobbyBars)
			{
				foreach (PassiveButton item4 in lobbyBar.ControllerSelectable)
				{
					ControllerManager.Instance.AddSelectableUiElement(item4);
				}
			}
			ControllerManager.Instance.PickTopSelectable();
		}));
		if (currentSceneName == "OnlineGame")
		{
			RefreshLobbyPlayers();
			LobbyPlayersTab.SetActive(true);
			LobbyPlayersInactiveTab.SetActive(false);
			OpenTab(0);
		}
		else
		{
			LobbyPlayersInactiveTab.SetActive(true);
			LobbyPlayersTab.SetActive(false);
			OpenTab(2);
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
	}

	public void OpenTab(int tabType)
	{
		CloseTab();
		FriendsListTabButton friendsListTabButton = Tabs.First((FriendsListTabButton t) => t.tab == (FriendsListTab)tabType);
		friendsListTabButton.tabObject.SetActive(true);
		Logger.GlobalInstance.Info("FriendsListUI::Opening friends list tab " + ((Object)friendsListTabButton.tabObject).name);
		selectedTab = tabType;
		if (friendsListTabButton.selectedSprites != null)
		{
			friendsListTabButton.selectedSprites.ForEach((SpriteRenderer s) =>
			{
				((Renderer)s).enabled = true;
			});
		}
		ControllerManager.Instance.ClearDestroyedSelectableUiElements();
	}

	public void Close(bool force = false)
	{
		if (!force && ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && (Tabs[selectedTab].tab == FriendsListTab.AddFriend || Tabs[selectedTab].tab == FriendsListTab.Notifications))
		{
			OpenTab(2);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public virtual void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void Update()
	{
		if ((Object)(object)ShipStatus.Instance != (Object)null)
		{
			Close();
		}
		else
		{
			if (ControllerManager.Instance.CurrentUiState.MenuName != ((Object)this).name)
			{
				return;
			}
			if (addFriendCooldown > 0f)
			{
				addFriendCooldown -= Time.unscaledDeltaTime;
			}
			Player player = ReInput.players.GetPlayer(0);
			if (!Tabs[selectedTab].selectable)
			{
				return;
			}
			int num = ((!LobbyPlayersTab.activeSelf) ? 1 : 0);
			if (selectedTab > num && player.GetButtonDown(35))
			{
				CloseTab();
				selectedTab--;
				OpenTab(selectedTab);
			}
			if (selectedTab < Tabs.Length - 1 && player.GetButtonDown(34) && Tabs[selectedTab + 1].selectable)
			{
				CloseTab();
				selectedTab++;
				OpenTab(selectedTab);
			}
			if (PlatformFriendsButton.activeSelf && selectedTab == 2 && player.GetButtonDown(29))
			{
				if (viewingAllFriends)
				{
					ViewPlatformFriends();
				}
				else
				{
					ViewAllFriends();
				}
			}
		}
	}

	private void CloseTab()
	{
		if (Tabs[selectedTab] == null)
		{
			return;
		}
		if (Tabs[selectedTab].selectedSprites != null)
		{
			Tabs[selectedTab].selectedSprites.ForEach((SpriteRenderer s) =>
			{
				((Renderer)s).enabled = false;
			});
		}
		Tabs[selectedTab].tabObject.SetActive(false);
	}

	public void AddFriend()
	{
		if (AddFriendArea.text.Trim() == string.Empty || addFriendCooldown > 0f)
		{
			return;
		}
		addFriendCooldown = 1f;
		DestroyableSingleton<FriendsListManager>.Instance.SendFriendRequestByUsername(AddFriendArea.text.Trim(), (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FriendRequestSent), FriendListNotification.IconType.Sent);
			}
			else if (response == null)
			{
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FriendRequestSentFailed), FriendListNotification.IconType.Failed);
			}
			else if (response.Errors != null && response.Errors.Length != 0 && DestroyableSingleton<FriendsListManager>.Instance.ErrorCodes.ContainsKey(response.Errors[0].Detail))
			{
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(DestroyableSingleton<FriendsListManager>.Instance.ErrorCodes[response.Errors[0].Detail]), FriendListNotification.IconType.Failed);
			}
			else
			{
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FriendRequestSentFailed), FriendListNotification.IconType.Failed);
			}
		});
	}

	public void RefreshFriends()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < DestroyableSingleton<FriendsListManager>.Instance.Friends.Count; i++)
		{
			float num = YStart - (float)i * YOffset;
			string friendPuid = DestroyableSingleton<FriendsListManager>.Instance.Friends[i].FriendPuid;
			OnlineFriendBar onlineFriendBar = Object.Instantiate<OnlineFriendBar>(OnlineFriendBar, FriendArea.transform);
			onlineFriendBar.SetUp(friendPuid, this, DestroyableSingleton<FriendsListManager>.Instance.Friends[i].FriendCode, DataManager.Player.Friends.GetCachedName(friendPuid));
			((Component)onlineFriendBar).transform.localPosition = new Vector3(-0.29f, num, -1f);
			onlineFriendBar.Buttons.ForEach((PassiveButton b) =>
			{
				b.ClickMask = FriendsScroller.Hitbox;
			});
			onlineFriendBar.GetAndSetPlatform();
			friendBars.Add(onlineFriendBar);
		}
		foreach (FriendsListBar friendBar in friendBars)
		{
			foreach (PassiveButton item in friendBar.ControllerSelectable)
			{
				ControllerManager.Instance.AddSelectableUiElement(item);
			}
		}
		FriendsScroller.SetYBoundsMax(0f - (YStart - (float)DestroyableSingleton<FriendsListManager>.Instance.Friends.Count * YOffset));
	}

	public void RefreshPlatformFriends()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < DestroyableSingleton<FriendsListManager>.Instance.PlatformFriends.Count; i++)
		{
			float num = YStart - (float)i * YOffset;
			OnlineFriendBar onlineFriendBar = Object.Instantiate<OnlineFriendBar>(PlatformFriendBar, PlatformFriendArea.transform);
			onlineFriendBar.SetUp(null, this, DestroyableSingleton<FriendsListManager>.Instance.PlatformFriends[i].Username, null);
			onlineFriendBar.PlatformFriendData = DestroyableSingleton<FriendsListManager>.Instance.PlatformFriends[i];
			((Component)onlineFriendBar).transform.localPosition = new Vector3(-0.29f, num, -1f);
			onlineFriendBar.Buttons.ForEach((PassiveButton b) =>
			{
				b.ClickMask = PlatformFriendsScroller.Hitbox;
			});
			onlineFriendBar.SetPlatform(Constants.GetPlatformType(), "");
			platformFriendBars.Add(onlineFriendBar);
		}
		foreach (FriendsListBar platformFriendBar in platformFriendBars)
		{
			foreach (PassiveButton item in platformFriendBar.ControllerSelectable)
			{
				ControllerManager.Instance.AddSelectableUiElement(item);
			}
		}
		PlatformFriendsScroller.SetYBoundsMax(0f - (YStart - (float)DestroyableSingleton<FriendsListManager>.Instance.PlatformFriends.Count * YOffset));
	}

	public void RefreshBlockedPlayers()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers.Count; i++)
		{
			float num = YStart - (float)i * YOffset;
			BlockedPlayerBar blockedPlayerBar = Object.Instantiate<BlockedPlayerBar>(BlockedPlayerBar, BlockedArea.transform);
			blockedPlayerBar.SetUp(DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers[i].FriendPuid, this, DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers[i].FriendCode, null);
			((Component)blockedPlayerBar).transform.localPosition = new Vector3(-0.29f, num, -1f);
			blockedPlayerBar.Buttons.ForEach((PassiveButton b) =>
			{
				b.ClickMask = BlockedScroller.Hitbox;
			});
			blockedPlayerBar.GetAndSetPlatform();
			friendBars.Add(blockedPlayerBar);
		}
		BlockedScroller.SetYBoundsMax(0f - (YStart - (float)DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers.Count * YOffset));
	}

	private void ClearNotifs()
	{
		foreach (FriendsListBar notifBar in notifBars)
		{
			if ((Object)(object)notifBar == (Object)null)
			{
				continue;
			}
			foreach (PassiveButton item in notifBar.ControllerSelectable)
			{
				ControllerManager.Instance.RemoveSelectableUiElement(item);
			}
			Object.DestroyImmediate((Object)(object)((Component)notifBar).gameObject);
		}
		notifBars.Clear();
	}

	public void RefreshNotifications()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		ClearNotifs();
		if (!DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			return;
		}
		for (int i = 0; i < DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.Count; i++)
		{
			float num = YStart - (float)i * YOffset;
			FriendRequestBar friendRequestBar = Object.Instantiate<FriendRequestBar>(FriendRequestBar, NotifArea.transform);
			friendRequestBar.SetUp(DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests[i].SenderId, this, DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests[i].SenderFriendCode, null);
			((Component)friendRequestBar).transform.localPosition = new Vector3(-0.29f, num, -1f);
			friendRequestBar.Buttons.ForEach((PassiveButton b) =>
			{
				b.ClickMask = NotifScroller.Hitbox;
			});
			notifBars.Add(friendRequestBar);
		}
		for (int num2 = 0; num2 < DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites.Count; num2++)
		{
			float num3 = YStart - (float)(num2 + DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.Count) * YOffset;
			LobbyInviteBar lobbyInviteBar = Object.Instantiate<LobbyInviteBar>(LobbyInviteBar, NotifArea.transform);
			lobbyInviteBar.SetUp(DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites[num2].SenderId, this, DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites[num2].RoomCode, DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites[num2].SenderFriendCode, null);
			((Component)lobbyInviteBar).transform.localPosition = new Vector3(-0.29f, num3, -1f);
			lobbyInviteBar.Buttons.ForEach((PassiveButton b) =>
			{
				b.ClickMask = NotifScroller.Hitbox;
			});
			notifBars.Add(lobbyInviteBar);
		}
		int num4 = DestroyableSingleton<FriendsListManager>.Instance.ReceivedLobbyInvites.Count + DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.Count;
		if (num4 > 0)
		{
			ViewRequestsButton.color = NewRequestColor;
			((TMP_Text)ViewRequestsText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NewRequests, num4);
		}
		else
		{
			ViewRequestsButton.color = NoRequestsColor;
			((TMP_Text)ViewRequestsText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoNewRequests);
		}
		NotifScroller.SetYBoundsMax(0f - (YStart - (float)num4 * YOffset));
		foreach (FriendsListBar notifBar in notifBars)
		{
			foreach (PassiveButton item in notifBar.ControllerSelectable)
			{
				ControllerManager.Instance.AddSelectableUiElement(item);
			}
		}
	}

	public void RefreshLobbyPlayers()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GameData.Instance == (Object)null || GameData.Instance.AllPlayers.Count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			GameData.PlayerInfo player = GameData.Instance.AllPlayers[i];
			if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
			{
				float num2 = YStart - (float)num * YOffset;
				LobbyPlayerBar lobbyPlayerBar = Object.Instantiate<LobbyPlayerBar>(LobbyPlayerBar, LobbyPlayerArea.transform);
				lobbyPlayerBar.SetUp(player.Puid, this, player.FriendCode, player.PlayerName);
				((Component)lobbyPlayerBar).transform.localPosition = new Vector3(-0.29f, num2, -1f);
				lobbyPlayerBar.Buttons.ForEach((PassiveButton b) =>
				{
					b.ClickMask = LobbyScroller.Hitbox;
				});
				PlayerControl playerControl = PlayerControl.AllPlayerControls.Find((PlayerControl pc) => pc.PlayerId == player.PlayerId);
				if (Object.op_Implicit((Object)(object)playerControl))
				{
					ClientData client = AmongUsClient.Instance.GetClient(playerControl.OwnerId);
					lobbyPlayerBar.SetInfoFromClient(client);
				}
				else
				{
					lobbyPlayerBar.SetInfoFromClient(null);
				}
				lobbyBars.Add(lobbyPlayerBar);
				num++;
			}
		}
		foreach (FriendsListBar lobbyBar in lobbyBars)
		{
			foreach (PassiveButton item in lobbyBar.ControllerSelectable)
			{
				ControllerManager.Instance.AddSelectableUiElement(item);
			}
		}
		LobbyScroller.SetYBoundsMax(0f - (YStart - (float)GameData.Instance.AllPlayers.Count * YOffset));
	}

	public void RefreshRecentlyPlayed()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < DestroyableSingleton<FriendsListManager>.Instance.RecentlyPlayedWith.Count; i++)
		{
			GameData.PlayerInfo playerInfo = DestroyableSingleton<FriendsListManager>.Instance.RecentlyPlayedWith[i];
			if (!(playerInfo.PlayerName == PlayerControl.LocalPlayer.Data.PlayerName))
			{
				float num2 = YStart - (float)num * YOffset;
				LobbyPlayerBar lobbyPlayerBar = Object.Instantiate<LobbyPlayerBar>(LobbyPlayerBar, RecentlyPlayedArea.transform);
				lobbyPlayerBar.SetUp(playerInfo.Puid, this, playerInfo.FriendCode, playerInfo.PlayerName);
				((Component)lobbyPlayerBar).transform.localPosition = new Vector3(-0.29f, num2, -1f);
				lobbyPlayerBar.Buttons.ForEach((PassiveButton b) =>
				{
					b.ClickMask = RecentlyPlayedScroller.Hitbox;
				});
				lobbyPlayerBar.GetAndSetPlatform();
				lobbyBars.Add(lobbyPlayerBar);
				num++;
			}
		}
		RecentlyPlayedScroller.SetYBoundsMax(0f - (YStart - (float)DestroyableSingleton<FriendsListManager>.Instance.RecentlyPlayedWith.Count * YOffset));
	}

	public void UpdateFriendBars()
	{
		foreach (FriendsListBar friendBar in friendBars)
		{
			if ((Object)(object)friendBar == (Object)null)
			{
				continue;
			}
			foreach (PassiveButton item in friendBar.ControllerSelectable)
			{
				ControllerManager.Instance.RemoveSelectableUiElement(item);
			}
			Object.DestroyImmediate((Object)(object)((Component)friendBar).gameObject);
		}
		friendBars.Clear();
		RefreshBlockedPlayers();
		RefreshFriends();
	}

	public void GiveFocus()
	{
		AddFriendArea.GiveFocus();
	}

	public void ToggleFriendCode()
	{
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden = !DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
			UpdateFriendCodeUI();
		}
	}

	public void UpdateFriendCodeUI()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			FriendCodeHideToggleObject.SetActive(true);
			((Behaviour)FriendCodeText).enabled = true;
			((Renderer)FriendCodeHiddenIcon).enabled = DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
			if (DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden)
			{
				((TMP_Text)FriendCodeText).text = friendCodeHiddenText;
			}
			else
			{
				((TMP_Text)FriendCodeText).text = DestroyableSingleton<EOSManager>.Instance.FriendCode;
			}
		}
		else
		{
			((Behaviour)FriendCodeText).enabled = false;
			FriendCodeHideToggleObject.SetActive(false);
		}
	}

	public void ViewAllFriends()
	{
		((Renderer)InactiveAllFriends).enabled = false;
		((Renderer)InactivePlatformFriends).enabled = true;
		viewingAllFriends = true;
		PlatformFriendArea.SetActive(false);
		FriendArea.SetActive(true);
		((Component)FriendsScroller).gameObject.SetActive(true);
		((Component)PlatformFriendsScroller).gameObject.SetActive(false);
		if (!((Behaviour)ControllerManager.Instance.CurrentUiState.CurrentSelection).isActiveAndEnabled && friendBars.Count > 0 && friendBars[0].Buttons.Count > 0)
		{
			ControllerManager.Instance.SetCurrentSelected(friendBars[0].Buttons[0]);
		}
	}

	public void ViewPlatformFriends()
	{
		((Renderer)InactiveAllFriends).enabled = true;
		((Renderer)InactivePlatformFriends).enabled = false;
		viewingAllFriends = false;
		PlatformFriendArea.SetActive(true);
		FriendArea.SetActive(false);
		((Component)FriendsScroller).gameObject.SetActive(false);
		((Component)PlatformFriendsScroller).gameObject.SetActive(true);
		if (!((Behaviour)ControllerManager.Instance.CurrentUiState.CurrentSelection).isActiveAndEnabled && platformFriendBars.Count > 0 && platformFriendBars[0].Buttons.Count > 0)
		{
			ControllerManager.Instance.SetCurrentSelected(platformFriendBars[0].Buttons[0]);
		}
	}

	public void ResendGuardianEmail()
	{
		DestroyableSingleton<EOSManager>.Instance.ResendGuardianEmail();
	}

	public void SortSamePlatformToTop(List<FriendsListBar> friendBars)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Constants.GetPlatformType();
		friendBars.Sort(ComparePreferSamePlatform);
		int num = 0;
		for (int i = 0; i < friendBars.Count; i++)
		{
			float num2 = YStart - (float)num * YOffset;
			((Component)friendBars[num]).transform.localPosition = new Vector3(-0.29f, num2, -1f);
			num++;
		}
		static int ComparePreferSamePlatform(FriendsListBar a, FriendsListBar b)
		{
			bool flag = a.PlatformFriendData != null;
			bool flag2 = b.PlatformFriendData != null;
			if (flag)
			{
				if (flag2)
				{
					return 0;
				}
				return -1;
			}
			if (flag2)
			{
				return 1;
			}
			return 0;
		}
	}
}
