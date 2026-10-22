using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseBlockedWrapper
{
	[JsonProperty("blocked_by")]
	public NestedResponseData<ResponseBlockedBy>[] BlockedBy;

	[JsonProperty("blocking")]
	public NestedResponseData<ResponseBlockedPlayer>[] Blocking;
}
