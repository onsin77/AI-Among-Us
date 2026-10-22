using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ResponsePurchase
{
	[JsonProperty("item_id")]
	public string ItemId;

	[JsonProperty("currency_type")]
	public string CurrencyType;

	[JsonProperty("platform")]
	public string Platform;

	[JsonProperty("item_cost")]
	public int ItemCost;
}
