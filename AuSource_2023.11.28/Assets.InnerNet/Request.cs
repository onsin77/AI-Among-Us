using Newtonsoft.Json;

namespace Assets.InnerNet;

public class Request<T>
{
	public class RequestData
	{
		[JsonProperty("attributes")]
		public T Attributes;

		[JsonProperty("type")]
		public string Type;
	}

	[JsonProperty("data")]
	public RequestData Data;
}
