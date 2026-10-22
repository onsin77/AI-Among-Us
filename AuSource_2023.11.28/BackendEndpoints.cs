public static class BackendEndpoints
{
	public static readonly string BaseEndpoint = "https://backend.innersloth.com/api/";

	public static string RedeemCurrency => BaseEndpoint + "user/redeem-currency";

	public static string ValidateReceipt => BaseEndpoint + "user/validate-receipt";

	public static string ValidateReceiptBatch => BaseEndpoint + "user/validate-receipt-batch";

	public static string SteamTransactionInitialize => BaseEndpoint + "user/steam-transaction-initialize";

	public static string SteamTransactionFinalize => BaseEndpoint + "user/steam-transaction-finalize";

	public static string AccountDeletion => BaseEndpoint + "user/accounts/deletion";

	public static string MergeGuestAccount => BaseEndpoint + "merge-guest-account";

	public static string FriendEndpoint => BaseEndpoint + "user/friends";

	public static string FriendRequestsEndpoint => BaseEndpoint + "user/friends/received-requests";

	public static string FriendSendRequestEndpoint => BaseEndpoint + "user/friends/request";

	public static string FriendSendRequestByFriendcodeEndpoint => BaseEndpoint + "user/friends/request-by-friendcode";

	public static string LobbyInviteEndpoint => BaseEndpoint + "user/lobby/invites";

	public static string FriendcodeEndpoint => BaseEndpoint + "user/username";

	public static string BlocksEndpoint => BaseEndpoint + "user/blocks";

	public static string Announcements => BaseEndpoint + "announcements";

	public static string FriendDenyRequestEndpoint(string puid)
	{
		return BaseEndpoint + "user/friends/request?sender_puid=" + puid;
	}

	public static string RemoveFriendEndpoint(string puid)
	{
		return BaseEndpoint + "user/friends?friend_puid=" + puid;
	}

	public static string UnblockEndpoint(string puid)
	{
		return BaseEndpoint + "user/blocks?recipient_puid=" + puid;
	}

	public static string LobbyDenyInviteEndpoint(string puid)
	{
		return BaseEndpoint + "user/lobby/invites?sender_puid=" + puid;
	}
}
