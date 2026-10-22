using Newtonsoft.Json;

namespace Assets.InnerNet;

public class LobbyInvite
{
	[JsonProperty("sender_puid")]
	public string SenderId;

	[JsonProperty("recipient_puid")]
	public string RecipientId;

	[JsonProperty("room_code")]
	public string RoomCode;

	[JsonProperty("sender_friendcode")]
	public string SenderFriendCode;

	[JsonProperty("recipient_friendcode")]
	public string RecipientFriendCode;
}
