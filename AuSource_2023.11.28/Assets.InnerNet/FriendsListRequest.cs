using Newtonsoft.Json;

namespace Assets.InnerNet;

public class FriendsListRequest
{
	[JsonProperty("username")]
	public string Username;

	[JsonProperty("recipient_puid")]
	public string RecipientId;

	[JsonProperty("recipient_friendcode")]
	public string RecipientUsername;

	public static Request<FriendsListRequest> ChangeUsername(string username)
	{
		FriendsListRequest friendsListRequest = new FriendsListRequest();
		friendsListRequest.Username = username;
		return new Request<FriendsListRequest>
		{
			Data = new Request<FriendsListRequest>.RequestData
			{
				Attributes = friendsListRequest,
				Type = "change_username"
			}
		};
	}

	public static Request<FriendsListRequest> SendRequest(string recipientId, string type)
	{
		FriendsListRequest friendsListRequest = new FriendsListRequest();
		friendsListRequest.RecipientId = recipientId;
		return new Request<FriendsListRequest>
		{
			Data = new Request<FriendsListRequest>.RequestData
			{
				Attributes = friendsListRequest,
				Type = type
			}
		};
	}

	public static Request<FriendsListRequest> SendRequestByUsername(string recipientName)
	{
		FriendsListRequest friendsListRequest = new FriendsListRequest();
		friendsListRequest.RecipientUsername = recipientName;
		return new Request<FriendsListRequest>
		{
			Data = new Request<FriendsListRequest>.RequestData
			{
				Attributes = friendsListRequest,
				Type = "friend_request_by_friendcode"
			}
		};
	}
}
