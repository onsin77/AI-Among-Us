using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ValidateReceipt
{
	public const string IapEntitlementType = "in-app-purchase";

	public const string DlcEntitlementType = "dlc";

	public const string InnerslothEntitlementType = "innersloth-receipt";

	[JsonProperty("entitlement_type")]
	public string EntitlementType;

	[JsonProperty("receipt")]
	public string Receipt;

	public static Request<ValidateReceipt> Create(string entitlementType, string receipt)
	{
		ValidateReceipt validateReceipt = new ValidateReceipt();
		validateReceipt.EntitlementType = entitlementType;
		validateReceipt.Receipt = receipt;
		return new Request<ValidateReceipt>
		{
			Data = new Request<ValidateReceipt>.RequestData
			{
				Attributes = validateReceipt,
				Type = "receipt_validation_request"
			}
		};
	}
}
