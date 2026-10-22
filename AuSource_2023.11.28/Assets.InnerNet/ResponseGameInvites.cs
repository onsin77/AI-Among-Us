using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseGameInvites
{
	[JsonProperty("next_poll_seconds")]
	public float NextPollSeconds;

	[JsonProperty("invites")]
	public NestedResponseData<LobbyInvite>[] Invites;
}
