using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseFriendCode : ResponseFriendsListRequest
{
	[JsonProperty("username")]
	public string Username;

	[JsonProperty("discriminator")]
	public string Discriminator;
}
