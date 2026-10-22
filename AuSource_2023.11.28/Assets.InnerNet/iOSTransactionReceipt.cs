using Newtonsoft.Json;

namespace Assets.InnerNet;

public class iOSTransactionReceipt
{
	[JsonProperty("Store")]
	public string Store;

	[JsonProperty("TransactionID")]
	public string TransactionID;

	[JsonProperty("Payload")]
	public string Payload;
}
