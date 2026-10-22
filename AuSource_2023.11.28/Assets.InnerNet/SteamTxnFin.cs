using Newtonsoft.Json;

namespace Assets.InnerNet;

public class SteamTxnFin
{
	[JsonProperty("order_id")]
	public string orderId;

	public static Request<SteamTxnFin> Create(string orderId)
	{
		SteamTxnFin steamTxnFin = new SteamTxnFin();
		steamTxnFin.orderId = orderId;
		return new Request<SteamTxnFin>
		{
			Data = new Request<SteamTxnFin>.RequestData
			{
				Attributes = steamTxnFin,
				Type = "steam_txn_finalize"
			}
		};
	}
}
