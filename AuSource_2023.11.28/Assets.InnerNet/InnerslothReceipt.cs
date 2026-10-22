using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.InnerNet;

public class InnerslothReceipt
{
	[JsonProperty("items")]
	public string[] Items;

	public InnerslothReceipt()
	{
	}

	public InnerslothReceipt(string[] items)
	{
		Items = items;
	}

	public InnerslothReceipt(List<string> items)
	{
		Items = items.ToArray();
	}
}
