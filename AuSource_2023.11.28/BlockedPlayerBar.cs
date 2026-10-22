using Assets.InnerNet;

public class BlockedPlayerBar : FriendsListBar
{
	public void CheckUnblockPlayer()
	{
		if (CanUseApi())
		{
			actionCooldown = 1f;
			DestroyableSingleton<FriendsListManager>.Instance.OpenConfirmationScreen(UnblockPlayer, FriendsListConfirmMenu.ActionType.Unblock, friendCode);
		}
	}

	public void UnblockPlayer()
	{
		GoInactive();
		DestroyableSingleton<FriendsListManager>.Instance.UnblockPlayer(puid, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
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

	private void Delete()
	{
		DestroyableSingleton<FriendsListManager>.Instance.BlockedPlayers.RemoveAll((ResponseBlockedPlayer notif) => notif.FriendPuid == puid);
	}
}
