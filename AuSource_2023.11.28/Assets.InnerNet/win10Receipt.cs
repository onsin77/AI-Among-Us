using Newtonsoft.Json;

namespace Assets.InnerNet;

public struct win10Receipt
{
	[JsonProperty("item_id")]
	public string itemId;

	[JsonProperty("token")]
	public string xstsToken;
}
