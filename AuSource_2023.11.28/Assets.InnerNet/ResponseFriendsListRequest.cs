using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseFriendsListRequest
{
	[JsonProperty("recipient_puid")]
	public string RecipientId;
}
