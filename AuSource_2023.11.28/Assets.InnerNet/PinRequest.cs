using Newtonsoft.Json;

namespace Assets.InnerNet;

public class PinRequest
{
	[JsonProperty("continuation_token")]
	public string ContinuanceToken;

	[JsonProperty("language")]
	public string Language;

	public static PinRequest RequestPin(string continuanceToken)
	{
		return new PinRequest
		{
			ContinuanceToken = continuanceToken,
			Language = "en-US"
		};
	}
}
