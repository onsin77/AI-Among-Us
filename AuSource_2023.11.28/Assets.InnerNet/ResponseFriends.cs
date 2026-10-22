using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseFriends
{
	[JsonProperty("friend_friendcode")]
	public string FriendCode;

	[JsonProperty("friend_puid")]
	public string FriendPuid;
}
