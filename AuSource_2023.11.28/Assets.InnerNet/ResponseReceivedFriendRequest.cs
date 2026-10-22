using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseReceivedFriendRequest
{
	[JsonProperty("sender_puid")]
	public string SenderId;

	[JsonProperty("sender_friendcode")]
	public string SenderFriendCode;
}
