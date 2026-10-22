using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.InnerNet;

public class ValidateReceiptBatch
{
	[JsonProperty("receipts")]
	public List<Request<ValidateReceipt>.RequestData> receipts;

	public static Request<ValidateReceiptBatch> Create(string entitlementType, List<string> receiptStrings)
	{
		List<Request<ValidateReceipt>.RequestData> list = new List<Request<ValidateReceipt>.RequestData>();
		foreach (string receiptString in receiptStrings)
		{
			ValidateReceipt validateReceipt = new ValidateReceipt();
			validateReceipt.EntitlementType = entitlementType;
			validateReceipt.Receipt = receiptString;
			Request<ValidateReceipt> request = new Request<ValidateReceipt>
			{
				Data = new Request<ValidateReceipt>.RequestData
				{
					Attributes = validateReceipt,
					Type = "receipt_validation_request"
				}
			};
			list.Add(request.Data);
		}
		ValidateReceiptBatch validateReceiptBatch = new ValidateReceiptBatch();
		validateReceiptBatch.receipts = list;
		return new Request<ValidateReceiptBatch>
		{
			Data = new Request<ValidateReceiptBatch>.RequestData
			{
				Attributes = validateReceiptBatch,
				Type = "batch_receipt_validation_request"
			}
		};
	}
}
