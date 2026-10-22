using System;
using System.Text;
using Newtonsoft.Json;

namespace Assets.InnerNet;

public class RedeemCurrency
{
	[JsonProperty("item_id")]
	public string ItemId;

	[JsonProperty("currency_type")]
	public string CurrencyType;

	[JsonProperty("platform")]
	public string Platform;

	public static Request<RedeemCurrency> ForNonCubePurchase(string productId, CurrencyTypes currencyType)
	{
		string purchasingPlatformType = Constants.GetPurchasingPlatformType();
		RedeemCurrency redeemCurrency = new RedeemCurrency();
		redeemCurrency.ItemId = ProductIdToItemId(productId);
		redeemCurrency.Platform = purchasingPlatformType.ToString().ToLower();
		switch (currencyType)
		{
		case CurrencyTypes.Beans:
			redeemCurrency.CurrencyType = "/beans/normal";
			break;
		case CurrencyTypes.Stars:
			if (Constants.GetPlatformType() == Platforms.Switch)
			{
				redeemCurrency.CurrencyType = "/stars/switch";
			}
			else
			{
				redeemCurrency.CurrencyType = "/stars/normal";
			}
			break;
		case CurrencyTypes.Pods:
			throw new InvalidOperationException("Use ForCubePurchase instead");
		}
		return new Request<RedeemCurrency>
		{
			Data = new Request<RedeemCurrency>.RequestData
			{
				Attributes = redeemCurrency,
				Type = "redeem_currency"
			}
		};
	}

	public static Request<RedeemCurrency> ForCubePurchase(string productId, string cubeName)
	{
		RedeemCurrency redeemCurrency = new RedeemCurrency();
		redeemCurrency.CurrencyType = "/pods/" + cubeName.Replace("cube_", "");
		redeemCurrency.ItemId = ProductIdToItemId(productId);
		redeemCurrency.Platform = Constants.GetPlatformType().ToString().ToLower();
		return new Request<RedeemCurrency>
		{
			Data = new Request<RedeemCurrency>.RequestData
			{
				Attributes = redeemCurrency,
				Type = "redeem_currency"
			}
		};
	}

	public static string ProductIdToItemId(string productId)
	{
		productId = productId.Replace("map", "bundle");
		int length = productId.IndexOf('_');
		string text = productId.Substring(0, length);
		text += (text.EndsWith("s") ? "" : "s");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("/");
		stringBuilder.Append(text);
		stringBuilder.Append("/unlocked/");
		stringBuilder.Append(productId);
		return stringBuilder.ToString();
	}
}
