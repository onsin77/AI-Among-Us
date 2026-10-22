using Assets.InnerNet;
using InnerNet;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineFriendBar : FriendsListBar
{
	public PassiveButton LobbyInviteButton;

	private bool canInvite = true;

	public override void SetUp(string puid, FriendsListUI parentUI, string friendCode, string playerInGameName)
	{
		base.SetUp(puid, parentUI, friendCode, playerInGameName);
		if (!string.IsNullOrWhiteSpace(playerInGameName))
		{
			((TMP_Text)SenderName).text = playerInGameName + "\r\n" + friendCode.Color("#666666") + " ";
		}
	}

	private void Awake()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (GameCode.IntToGameName(AmongUsClient.Instance.GameId) != null)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (!(activeScene.name != "OnlineGame"))
			{
				return;
			}
		}
		canInvite = false;
		((Component)LobbyInviteButton).GetComponent<SpriteRenderer>().color = Color.gray;
	}

	public void PlatformInviteToLobby()
	{
		if (canInvite && CanUseApi())
		{
			actionCooldown = 1f;
			DestroyableSingleton<FriendsListManager>.Instance.OpenConfirmationScreen(PlatformSendGameInvite, FriendsListConfirmMenu.ActionType.Lobby, friendCode);
		}
	}

	public void InviteToLobby()
	{
		if (canInvite && CanUseApi())
		{
			DestroyableSingleton<FriendsListManager>.Instance.OpenConfirmationScreen(SendGameInvite, FriendsListConfirmMenu.ActionType.Lobby, friendCode);
		}
	}

	public void CheckRemoveFriend()
	{
		if (CanUseApi())
		{
			actionCooldown = 1f;
			DestroyableSingleton<FriendsListManager>.Instance.OpenConfirmationScreen(RemoveFriend, FriendsListConfirmMenu.ActionType.Unfriend, friendCode);
		}
	}

	private void RemoveFriend()
	{
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.RemoveFriend(puid, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				Delete();
				parentUI.UpdateFriendBars();
			}
			else
			{
				GoActive();
			}
		});
	}

	public void CheckBlockPlayer()
	{
		if (CanUseApi())
		{
			actionCooldown = 1f;
			DestroyableSingleton<FriendsListManager>.Instance.OpenConfirmationScreen(BlockPlayer, FriendsListConfirmMenu.ActionType.Block, friendCode);
		}
	}

	private void BlockPlayer()
	{
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.BlockPlayer(puid, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers.Add(new ResponseBlockedPlayer
				{
					FriendPuid = puid,
					FriendCode = friendCode
				});
				RemoveFriend();
			}
			else
			{
				GoActive();
			}
		});
	}

	private void PlatformSendGameInvite()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		actionCooldown = 1f;
		SteamFriends.InviteUserToGame(PlatformFriendData.CSteamID, GameCode.IntToGameName(AmongUsClient.Instance.GameId));
		if (1 != 0)
		{
			DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameLobbyInviteSent), FriendListNotification.IconType.Sent);
		}
		else
		{
			DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameLobbyInviteSentFailed), FriendListNotification.IconType.Failed);
		}
	}

	private void SendGameInvite()
	{
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.SendGameInvite(puid, GameCode.IntToGameName(AmongUsClient.Instance.GameId), (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				GoActive();
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameLobbyInviteSent), FriendListNotification.IconType.Sent);
			}
			else
			{
				GoActive();
				DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameLobbyInviteSentFailed), FriendListNotification.IconType.Failed);
			}
		});
	}

	private void Delete()
	{
		DestroyableSingleton<FriendsListManager>.Instance.Friends.RemoveAll((ResponseFriends notif) => notif.FriendPuid == puid);
	}
}
