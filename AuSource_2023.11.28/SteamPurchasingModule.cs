using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AmongUs.Data;
using Assets.InnerNet;
using Steamworks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

internal class SteamPurchasingModule : IPurchasingModule, IStore
{
	private const string Name = "Steam";

	private IStoreCallback storeCallback;

	private bool steamOverlayOpen;

	private uint steamAppId = 945360u;

	private Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private CompletionToken<PurchaseStates> initializeQueryDone;

	private CompletionToken<PurchaseStates> finalizeQueryDone;

	public void Configure(IPurchasingBinder binder)
	{
		binder.RegisterStore("Steam", (IStore)(object)this);
	}

	public void FinishTransaction(ProductDefinition product, string transactionId)
	{
		Debug.LogError((object)"SteamPurchasingModule.FinishTransaction: Nothing");
	}

	public void Initialize(IStoreCallback callback)
	{
		storeCallback = callback;
		if (!SteamManager.Initialized || !SteamUtils.IsOverlayEnabled())
		{
			storeCallback.OnSetupFailed((InitializationFailureReason)0);
		}
		m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create((DispatchDelegate<GameOverlayActivated_t>)HandleOverlayActivate);
	}

	public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected Obj, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected Obj, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected Obj, but got Unknown
		List<ProductDescription> list = new List<ProductDescription>(products.Count);
		for (int i = 0; i < products.Count; i++)
		{
			ProductDefinition product = products[i];
			IBuyable buyable = DestroyableSingleton<HatManager>.Instance.AllBundles.FirstOrDefault((BundleData x) => x.productId == product.id);
			if (buyable == null)
			{
				buyable = DestroyableSingleton<HatManager>.Instance.AllStarBundles.First((StarBundle x) => x.SteamAppId.ToString() == product.id);
			}
			if (buyable is BundleData)
			{
				list.Add(new ProductDescription(product.id, new ProductMetadata(buyable.StarCost.ToString(), "", "", "usd", 1m), (string)null, (string)null));
			}
			else if (buyable is StarBundle)
			{
				StarBundle starBundle = (StarBundle)buyable;
				list.Add(new ProductDescription(product.id, new ProductMetadata(starBundle.price.ToString(), "", "", "usd", 1m), (string)null, (string)null));
			}
		}
		storeCallback.OnProductsRetrieved(list);
	}

	private void HandleOverlayActivate(GameOverlayActivated_t param)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		steamOverlayOpen = param.m_bActive != 0;
	}

	public void Purchase(ProductDefinition product, string developerPayload)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			DestroyableSingleton<StoreManager>.Instance.starPurchaseStateQuery.MarkComplete(PurchaseStates.Fail, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SteamNotInitialized));
			return;
		}
		if (!SteamUtils.IsOverlayEnabled())
		{
			DestroyableSingleton<StoreManager>.Instance.starPurchaseStateQuery.MarkComplete(PurchaseStates.Fail, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SteamOverlayDisabled));
			return;
		}
		Debug.Log((object)("Purchase product " + product.id + " - " + ((object)product.type/*cast due to constrained. prefix*/).ToString()));
		CSteamID steamID = SteamUser.GetSteamID();
		int result = 0;
		int.TryParse(product.id, out result);
		SteamTxnInit payloadData = new SteamTxnInit
		{
			steamId = ((object)steamID/*cast due to constrained. prefix*/).ToString(),
			languageCode = (uint)DataManager.Settings.Language.CurrentLanguage,
			itemId = result
		};
		initializeQueryDone = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		finalizeQueryDone = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		Callback<MicroTxnAuthorizationResponse_t>.Create((DispatchDelegate<MicroTxnAuthorizationResponse_t>)OnSteamMicroTxnAuth);
		((MonoBehaviour)DestroyableSingleton<StoreManager>.Instance).StartCoroutine(WaitForPurchase(product));
		DestroyableSingleton<InventoryManager>.Instance.InitSteamMicroTxn(SteamTxnInit.Create(payloadData), initializeQueryDone);
	}

	private void OnSteamMicroTxnAuth(MicroTxnAuthorizationResponse_t pCallback)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"MicroTxnAuthorizationResponse_t: app ID: {pCallback.m_unAppID} - order ID: {pCallback.m_ulOrderID} - authorized: {pCallback.m_bAuthorized}");
		if (pCallback.m_bAuthorized == 1)
		{
			DestroyableSingleton<InventoryManager>.Instance.FinalizeSteamMicroTxn(SteamTxnFin.Create(pCallback.m_ulOrderID.ToString()), finalizeQueryDone);
			return;
		}
		initializeQueryDone.MarkComplete(PurchaseStates.NotStarted);
		finalizeQueryDone.MarkComplete(PurchaseStates.NotStarted);
	}

	private IEnumerator WaitForPurchase(ProductDefinition product)
	{
		while (!steamOverlayOpen)
		{
			SteamAPI.RunCallbacks();
			yield return null;
		}
		while (steamOverlayOpen)
		{
			SteamAPI.RunCallbacks();
			yield return null;
		}
		yield return initializeQueryDone.WaitForCompletion();
		if (initializeQueryDone.Value != PurchaseStates.Success)
		{
			Debug.Log((object)("Steam initialize purchase error " + initializeQueryDone.errorStr));
			DestroyableSingleton<StoreManager>.Instance.starPurchaseStateQuery.MarkComplete(initializeQueryDone.Value, initializeQueryDone.errorStr);
			yield break;
		}
		yield return finalizeQueryDone.WaitForCompletion();
		Debug.Log((object)("Steam finalize purchase result " + finalizeQueryDone.errorStr));
		DestroyableSingleton<StoreManager>.Instance.starPurchaseStateQuery.MarkComplete(finalizeQueryDone.Value, finalizeQueryDone.errorStr);
		storeCallback.OnPurchaseSucceeded(product.id, "FakeReceipt", Random.value.ToString());
	}
}
