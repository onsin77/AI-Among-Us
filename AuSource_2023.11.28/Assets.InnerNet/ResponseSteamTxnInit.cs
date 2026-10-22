using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponseSteamTxnInit
{
	[JsonProperty("order_id")]
	public string orderID;

	[JsonProperty("steam_id")]
	public string steamID;
}
