using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseBlockedBy
{
	[JsonProperty("sender_friendcode")]
	public string FriendCode;

	[JsonProperty("sender_puid")]
	public string SenderId;
}
