using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseFriendWrapper
{
	[JsonProperty("friends")]
	public NestedResponseData<ResponseFriends>[] Friends;
}
