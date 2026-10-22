using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseReceivedFriendRequestWrapper
{
	[JsonProperty("senders")]
	public NestedResponseData<ResponseReceivedFriendRequest>[] Senders;
}
