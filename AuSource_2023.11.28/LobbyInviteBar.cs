using System.Collections;
using Assets.InnerNet;
using InnerNet;
using UnityEngine;

public class LobbyInviteBar : FriendsListBar
{
	private string roomCode;

	private bool hasAccepted;

	public void SetUp(string puid, FriendsListUI parentUI, string roomCode, string friendCode, string playerName)
	{
		SetUp(puid, parentUI, friendCode, playerName);
		this.roomCode = roomCode;
		hasAccepted = false;
	}

	public void PressAccept()
	{
		if (!hasAccepted)
		{
			hasAccepted = true;
			((MonoBehaviour)this).StartCoroutine(JoinGame());
		}
	}

	private IEnumerator JoinGame()
	{
		((Renderer)DestroyableSingleton<FriendsListManager>.Instance.LoadScreen).enabled = true;
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && AmongUsClient.Instance.AmClient)
		{
			AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		}
		int gameId = GameCode.GameNameToInt(roomCode);
		yield return AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameId);
		((Renderer)DestroyableSingleton<FriendsListManager>.Instance.LoadScreen).enabled = false;
		parentUI.Close(force: true);
		DenyLobbyInvite();
	}

	public void DenyLobbyInvite()
	{
		DestroyableSingleton<FriendsListManager>.Instance.DenyLobbyInvite(puid, (ResponseState cb, Response<ResponseFriendsListRequest> response) =>
		{
			if (cb == ResponseState.Success)
			{
				Debug.Log((object)"Deny lobby invite successful");
			}
			else
			{
				Debug.Log((object)"Deny lobby invite FAILED");
			}
		});
		parentUI.RefreshFriends();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
