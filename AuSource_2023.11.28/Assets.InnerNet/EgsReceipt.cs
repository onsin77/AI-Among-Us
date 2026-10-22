using Newtonsoft.Json;

namespace Assets.InnerNet;

public struct EgsReceipt
{
	[JsonProperty("catalog_item_id")]
	public string catalogItemId;

	[JsonProperty("entitlement_id")]
	public string entitlementId;
}
