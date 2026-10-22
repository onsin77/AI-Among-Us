using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseBlockedPlayer
{
	[JsonProperty("recipient_friendcode")]
	public string FriendCode;

	[JsonProperty("recipient_puid")]
	public string FriendPuid;
}
