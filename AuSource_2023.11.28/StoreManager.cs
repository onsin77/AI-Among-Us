using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class StoreManager : DestroyableSingleton<StoreManager>, IStoreListener
{
	public CompletionToken<PurchaseStates> starPurchaseStateQuery;

	private Product[] allProducts;

	public bool FinishedInitializationFlow;

	private readonly Logger logger = new Logger("StoreManager", Logger.Level.Warning, Logger.Category.Network);

	private IStoreController controller;

	private IExtensionProvider extensions;

	private bool initialized;

	public void Initialize()
	{
		logger.Info("Initializing store");
		ConfigurationBuilder val = ConfigurationBuilder.Instance((IPurchasingModule)(object)new SteamPurchasingModule(), Array.Empty<IPurchasingModule>());
		foreach (BundleData allBundle in DestroyableSingleton<HatManager>.Instance.AllBundles)
		{
			val.AddProduct(allBundle.GetPurchasingProductId(), (ProductType)1);
		}
		foreach (StarBundle allStarBundle in DestroyableSingleton<HatManager>.Instance.AllStarBundles)
		{
			val.AddProduct(allStarBundle.GetPurchasingProductId(), (ProductType)0);
		}
		UnityPurchasing.Initialize((IStoreListener)(object)this, val);
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		this.controller = controller;
		this.extensions = extensions;
		if (this.controller == null || this.controller.products == null)
		{
			logger.Error("Failed to initialize store");
			return;
		}
		allProducts = controller.products.all;
		initialized = true;
		FinishedInitializationFlow = true;
		logger.Info("Successfully initialized store");
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		FinishedInitializationFlow = true;
		logger.Warning("Failed to init store:\r\n" + ((object)error/*cast due to constrained. prefix*/).ToString());
	}

	public void ProcessOldReceiptsRestore()
	{
		if (!initialized)
		{
			logger.Warning("Tried to process old receipts before store was finished initializing");
			DestroyableSingleton<EOSManager>.Instance.validateOldDLC.MarkComplete(PurchaseStates.Fail);
		}
		else if (extensions == null)
		{
			logger.Error("Somehow StoreManager was initialized, but no IExtensionProvider (this.extensions) was set.");
			DestroyableSingleton<EOSManager>.Instance.validateOldDLC.MarkComplete(PurchaseStates.Fail);
		}
		else
		{
			ProcessOldReceipts();
		}
	}

	public void ProcessOldReceipts()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		if (!initialized)
		{
			logger.Warning("Tried to process old receipts before store was finished initializing");
			DestroyableSingleton<EOSManager>.Instance.validateOldDLC.MarkComplete(PurchaseStates.Fail);
			return;
		}
		List<string> list = new List<string>();
		Product[] array = allProducts;
		foreach (Product val in array)
		{
			if (val != null && (int)val.definition.type == 1)
			{
				Debug.Log((object)$"receipts: {val.hasReceipt}:{val.receipt} - {val.definition.storeSpecificId} - {val.definition.id} - {val.metadata.localizedTitle} - {val.hasReceipt} - {val.transactionID}");
				if (val != null && val.hasReceipt)
				{
					list.Add(val.receipt);
				}
			}
		}
		CompletionToken<PurchaseStates> completionToken = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		if (list.Count != 0)
		{
			DestroyableSingleton<InventoryManager>.Instance.ValidateIapReceipt(list, completionToken);
			DestroyableSingleton<InventoryManager>.Instance.WaitForRedemptionCompletion(completionToken);
		}
		else
		{
			DestroyableSingleton<EOSManager>.Instance.validateOldDLC?.MarkComplete(PurchaseStates.Success);
		}
	}

	public void InitiateStorePurchaseStar(PurchaseButton button)
	{
		starPurchaseStateQuery = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		if (button.Product is StarBundle)
		{
			StarBundle starBundle = (StarBundle)button.Product;
			Debug.Log((object)starBundle.GetPurchasingProductId());
			controller.InitiatePurchase(starBundle.GetPurchasingProductId());
			((MonoBehaviour)this).StartCoroutine(CoWaitForStarPurchaseValidation(starPurchaseStateQuery, button));
		}
		else
		{
			DestroyableSingleton<StoreMenu>.Instance.DisplayFailed("This isn't a star");
		}
	}

	private void InitiateStoreHeadlessStarPurchase(PurchaseEventArgs purchaseEventArgs)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		starPurchaseStateQuery = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		if (purchaseEventArgs.purchasedProduct != null && (int)purchaseEventArgs.purchasedProduct.definition.type == 0)
		{
			((MonoBehaviour)this).StartCoroutine(CoWaitForHeadlessStarPurchaseValidation(starPurchaseStateQuery));
		}
		else
		{
			logger.Warning($"Tried to finish a store purchase that wasn't a star: {purchaseEventArgs}");
		}
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Debug.Log((object)$"Process purchase - transaction ID: {e.purchasedProduct.transactionID} - {e.purchasedProduct.hasReceipt} - id: {e.purchasedProduct.definition.id}");
			if ((int)e.purchasedProduct.definition.type == 1)
			{
				return (PurchaseProcessingResult)0;
			}
		}
		catch (IAPSecurityException)
		{
			Debug.Log((object)("Failed Validation: " + e.purchasedProduct.definition.id));
			return (PurchaseProcessingResult)1;
		}
		Debug.Log((object)("Completed Purchase: " + e.purchasedProduct.definition.id));
		if ((int)e.purchasedProduct.definition.type != 0)
		{
			return (PurchaseProcessingResult)0;
		}
		return (PurchaseProcessingResult)1;
	}

	private IEnumerator CoWaitForStarPurchaseValidation(CompletionToken<PurchaseStates> queryDone, PurchaseButton button)
	{
		yield return queryDone.WaitForCompletion();
		if (queryDone.Value == PurchaseStates.Success)
		{
			if (button.Product is StarBundle)
			{
				StarBundle starBundle = (StarBundle)button.Product;
				Debug.Log((object)starBundle.starAmount);
				DestroyableSingleton<InventoryManager>.Instance.UpdateStarCount(starBundle.starAmount);
				Product val = allProducts.FirstOrDefault((Product x) =>
				{
					object obj;
					if (x == null)
					{
						obj = null;
					}
					else
					{
						ProductDefinition definition = x.definition;
						obj = ((definition != null) ? definition.id : null);
					}
					return (string)obj == starBundle.GetPurchasingProductId();
				});
				if (val != null && (int)val.definition.type == 0)
				{
					Debug.Log((object)("validating prod " + val.definition.id));
					controller.ConfirmPendingPurchase(val);
				}
			}
			DestroyableSingleton<StoreMenu>.Instance.DisplaySuccess();
			DestroyableSingleton<StoreMenu>.Instance.CloseConfirm();
		}
		else if (queryDone.Value == PurchaseStates.Fail)
		{
			DestroyableSingleton<StoreMenu>.Instance.DisplayFailed(queryDone.errorStr);
		}
		else
		{
			DestroyableSingleton<StoreMenu>.Instance.CloseConfirm();
			DestroyableSingleton<StoreMenu>.Instance.CloseWaitModal();
		}
	}

	private IEnumerator CoWaitForHeadlessStarPurchaseValidation(CompletionToken<PurchaseStates> queryDone)
	{
		yield return queryDone.WaitForCompletion();
		if (queryDone.Value == PurchaseStates.Success)
		{
			DestroyableSingleton<EOSManager>.Instance.UpdateInventory();
		}
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason error)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)error == 4)
		{
			DestroyableSingleton<StoreMenu>.Instance.CloseWaitModal();
			DestroyableSingleton<StoreMenu>.Instance.CloseConfirm();
		}
		else
		{
			starPurchaseStateQuery?.MarkComplete(PurchaseStates.Fail, ((object)error/*cast due to constrained. prefix*/).ToString());
			Debug.LogError((object)("Failed: " + ((object)error/*cast due to constrained. prefix*/).ToString()));
		}
	}
}
