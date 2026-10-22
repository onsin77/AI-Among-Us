using Newtonsoft.Json;

namespace Assets.InnerNet;

public class GameInviteRequest
{
	[JsonProperty("recipient_puid")]
	public string RecipientId;

	[JsonProperty("room_code")]
	public string RoomCode;

	public static Request<GameInviteRequest> SendGameInvite(string recipientId, string roomCode)
	{
		GameInviteRequest gameInviteRequest = new GameInviteRequest();
		gameInviteRequest.RoomCode = roomCode;
		gameInviteRequest.RecipientId = recipientId;
		return new Request<GameInviteRequest>
		{
			Data = new Request<GameInviteRequest>.RequestData
			{
				Attributes = gameInviteRequest,
				Type = "lobby_invite"
			}
		};
	}
}
