using Assets.InnerNet;
using UnityEngine;

public class FriendRequestBar : FriendsListBar
{
	public void PressAccept()
	{
		if (!CanUseApi())
		{
			return;
		}
		actionCooldown = 1f;
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.SendFriendRequestByUsername(friendCode, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				Delete();
				Debug.Log((object)"Friend request successful");
				DestroyableSingleton<FriendsListManager>.Instance.Friends.Add(new ResponseFriends
				{
					FriendPuid = puid,
					FriendCode = friendCode
				});
				parentUI.UpdateFriendBars();
				parentUI.RefreshNotifications();
			}
			else
			{
				GoActive();
				if (response.Errors != null && response.Errors.Length != 0 && DestroyableSingleton<FriendsListManager>.Instance.ErrorCodes.ContainsKey(response.Errors[0].Detail))
				{
					DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(DestroyableSingleton<FriendsListManager>.Instance.ErrorCodes[response.Errors[0].Detail]), FriendListNotification.IconType.Failed);
				}
				else
				{
					DestroyableSingleton<FriendsListManager>.Instance.FriendListNotification.SetUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Failed), FriendListNotification.IconType.Failed);
				}
			}
		});
	}

	public void DenyFriendRequest()
	{
		if (!CanUseApi())
		{
			return;
		}
		actionCooldown = 1f;
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.DenyFriendRequest(puid, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				Delete();
				Debug.Log((object)"Deny Friend request successful");
				parentUI.RefreshNotifications();
			}
			else
			{
				GoActive();
				Debug.Log((object)"Deny Friend request FAILED");
			}
		});
	}

	public void CheckBlockPlayer()
	{
		if (CanUseApi())
		{
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
				parentUI.UpdateFriendBars();
				DenyFriendRequest();
				Debug.Log((object)"Block successful");
			}
			else
			{
				GoActive();
				Debug.Log((object)"Block FAILED");
			}
		});
	}

	private void Delete()
	{
		DestroyableSingleton<FriendsListManager>.Instance.ReceivedRequests.RemoveAll((ResponseReceivedFriendRequest notif) => notif.SenderId == puid || notif.SenderFriendCode == friendCode);
	}
}
