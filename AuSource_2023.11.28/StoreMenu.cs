using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using Assets.InnerNet;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class StoreMenu : DestroyableSingleton<StoreMenu>
{
	public const string BoughtAdsProductId = "bought_ads";

	private const float MaxItemsPerRow = 4f;

	private const float NumRowsVisible = 2f;

	private const float DefaultAspectRatio = 1.7777778f;

	private HatData[] allHats;

	private SkinData[] allSkins;

	private VisorData[] allVisors;

	private NamePlateData[] allPlates;

	private BundleData[] allBundles;

	private CosmicubeData[] allCubes;

	private PetData[] allPets;

	private CosmeticData[] allFeaturedItems;

	private CosmicubeData[] allFeaturedCubes;

	private BundleData[] allLargeFeaturedBundles;

	private BundleData[] allFeaturedBundles;

	private StarBundle[] allStars;

	private Product[] allProducts;

	[Header("Store Menu")]
	public StoreTabButton[] Tabs;

	public GameObject normalMenu;

	public GameObject featuredMenu;

	public TextMeshPro LoadingText;

	public TextMeshPro RestorePurchasesButton;

	public GameObject RestorePurchasesObj;

	public UiElement OpenAllInEShopButton;

	public PurchaseButton PurchasablePrefab;

	public PurchaseButton PurchasableNameplatePrefab;

	public BundlePurchaseButton BundlePurchasablePrefab;

	public BundlePurchaseButton LargeBundlePurchasablePrefab;

	public TextMeshPro beanAmount;

	public TextMeshPro starAmount;

	public GameObject comingSoonText;

	public TextMeshPro tabHeader;

	[Header("Confirm Menu")]
	public PurchasePopUp plsWaitModal;

	public GameObject confirmMenu;

	public CubeCellPurchaseButton priceButton;

	public CubeCellPurchaseButton priceButtonCube;

	public GameObject starConfirmMenu;

	public TextMeshPro StarBundleName;

	public TextMeshPro StarPriceText;

	public PassiveButton starBuyButton;

	public SpriteRenderer starPreviewImage;

	public CosmicubeMenu cosmicubePreviewMenu;

	public PoolablePlayer dressMeUp;

	public PoolablePlayer dressMeUpCube;

	public PlayerVoteArea NameplatePreviewArea;

	public TextMeshPro ItemName;

	public TextMeshPro ItemNameCube;

	public TextMeshPro BundleName;

	public TextMeshPro CosmicubeName;

	public SpriteRenderer PurchaseBackground;

	public TextMeshPro PriceText;

	public TextMeshPro PriceTextCube;

	public StoreChip PreviewPrefab;

	public Scroller ConfirmMenuScroller;

	public UpdateProductTimerDisplay itemLimitedTimeText;

	public FloatRange BundleRange = new FloatRange(-3.5f, 2.9f);

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public FloatRange CubesRange = new FloatRange(-3.7f, 2.7f);

	public float YStart = 1.1f;

	public float YOffset = 0.8f;

	public int NumPerRow = 4;

	public Vector3 ConfirmCosmeticPosition;

	public Sprite starIcon;

	public Sprite beanIcon;

	[HideInInspector]
	public List<ColorChip> ColorChips = new List<ColorChip>();

	private RefreshTimer[] refreshTimers;

	private StoreTab selectedTab;

	private StoreTabButton selectedTabObject;

	private const float NormalHeight = -1.95f;

	private const float BoxHeight = -1.95f;

	public TextMeshPro starDisclaimerText;

	private List<TextMeshPro> BundleTitles;

	private Logger logger = new Logger("StoreMenu", Logger.Level.Info, Logger.Category.UI);

	public Action OnOpen;

	public Action OnClose;

	private StoreTab[] controllerSelectableTabs = new StoreTab[9]
	{
		StoreTab.Home,
		StoreTab.Hats,
		StoreTab.Skins,
		StoreTab.Pets,
		StoreTab.Visors,
		StoreTab.Nameplates,
		StoreTab.Bundles,
		StoreTab.Cubes,
		StoreTab.Stars
	};

	[Header("Store Menu")]
	public Scroller Scroller;

	public Scroller FeatureTabScroller;

	private MainMenuManager manager;

	private PurchaseButton CurrentButton;

	private List<GameObject> AllObjects = new List<GameObject>();

	[Header("Console Controller Navigation")]
	private ControllerNavMenu controllerNavMenu;

	public GameObject glyphL;

	public GameObject glyphR;

	private bool initialized;

	public PurchaseStates PurchaseState { get; private set; }

	public bool Initialized => initialized;

	public void CloseConfirm()
	{
		confirmMenu.SetActive(false);
		starConfirmMenu.SetActive(false);
		((Component)cosmicubePreviewMenu).gameObject.SetActive(false);
		if (selectedTab == StoreTab.Bundles)
		{
			featuredMenu.SetActive(true);
		}
		else
		{
			normalMenu.SetActive(true);
		}
	}

	public void CloseWaitModal()
	{
		((Component)plsWaitModal).gameObject.SetActive(false);
	}

	public void DisplayFailed(string error)
	{
		plsWaitModal.SetResponse(ResponseState.Failed, error);
	}

	public void DisplaySuccess()
	{
		plsWaitModal.SetResponse(ResponseState.Success, "");
	}

	public void UpdateStarDisplay()
	{
		if ((Object)(object)starAmount != (Object)null)
		{
			((TMP_Text)starAmount).text = DestroyableSingleton<InventoryManager>.Instance.UnusedStars.ToString();
		}
	}

	public void Start()
	{
		BundleTitles = new List<TextMeshPro>();
		((Component)this).gameObject.SetActive(false);
		if ((Object)(object)OpenAllInEShopButton != (Object)null)
		{
			((Component)OpenAllInEShopButton).gameObject.SetActive(false);
		}
		StoreTabButton[] tabs = Tabs;
		foreach (StoreTabButton storeTabButton in tabs)
		{
			if (DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions.ContainsKey(storeTabButton.storeTab) && Constants.ParseDate(DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[storeTabButton.storeTab]) <= DataManager.Player.Store.GetStoreTabViewDate(storeTabButton.storeTab))
			{
				storeTabButton.NewItemIcon.SetActive(false);
			}
		}
	}

	protected virtual void OnDisable()
	{
		DataManager.Player.Save();
	}

	public void OpenTab(StoreMenuTabButton Butt)
	{
		OpenTab(Butt.Tab);
	}

	public void OpenTab(StoreTab tab)
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		selectedTab = tab;
		for (int i = 0; i < Tabs.Length; i++)
		{
			StoreTabButton storeTabButton = Tabs[i];
			if (tab == storeTabButton.storeTab)
			{
				selectedTabObject = storeTabButton;
				((Renderer)selectedTabObject.Button).enabled = true;
				if (DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions.ContainsKey(selectedTabObject.storeTab))
				{
					selectedTabObject.NewItemIcon.SetActive(false);
					DataManager.Player.Store.SetStoreTabViewDate(selectedTabObject.storeTab, Constants.ParseDate(DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[selectedTabObject.storeTab]));
				}
				if (Object.op_Implicit((Object)(object)selectedTabObject.tabText))
				{
					((TMP_Text)selectedTabObject.tabText).outlineColor = Color32.op_Implicit(Palette.InventorySelectedTextColor);
				}
				ShowTab();
			}
			else
			{
				((Renderer)storeTabButton.Button).enabled = false;
				if (Object.op_Implicit((Object)(object)storeTabButton.tabText))
				{
					((TMP_Text)storeTabButton.tabText).outlineColor = Color32.op_Implicit(Palette.InventoryTextColor);
				}
			}
		}
		bool flag = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
		glyphL.SetActive(flag && selectedTab != controllerSelectableTabs[0]);
		glyphR.SetActive(flag && selectedTab != controllerSelectableTabs[controllerSelectableTabs.Length - 1]);
	}

	public void ShowConfirm(PurchaseButton butt)
	{
		if (butt.Product is StarBundle)
		{
			if (DataManager.Player.Account.LoginStatus != EOSManager.AccountLoginStatus.LoggedIn)
			{
				((TMP_Text)starDisclaimerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LoggedInErrorStarPurchase);
				starBuyButton.SetButtonEnableState(enabled: false);
			}
			else
			{
				((TMP_Text)starDisclaimerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.StarDisclaimer);
				starBuyButton.SetButtonEnableState(enabled: true);
			}
			starConfirmMenu.gameObject.SetActive(true);
			starPreviewImage.sprite = (butt.Product as StarBundle).StoreImage;
		}
		else if (butt.Product is CosmicubeData)
		{
			((Component)cosmicubePreviewMenu).gameObject.SetActive(true);
			cosmicubePreviewMenu.Show(butt.ProductId, previewMode: true);
		}
		else
		{
			confirmMenu.SetActive(true);
		}
		priceButton.podIcon.sprite = ((butt.Product.StarCost > 0) ? starIcon : beanIcon);
		priceButtonCube.podIcon.sprite = ((butt.Product.StarCost > 0) ? starIcon : beanIcon);
		normalMenu.SetActive(false);
		featuredMenu.SetActive(false);
		SetProduct(butt);
		logger.Info($"Showing confirmation menu for product {butt.Product} {butt.ProductId}");
	}

	public void StarPurchaseFinished()
	{
		CloseWaitModal();
	}

	public void Initialize()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			initialized = true;
			controllerNavMenu = ((Component)this).GetComponent<ControllerNavMenu>();
			((Component)this).gameObject.SetActive(false);
			dressMeUp.TogglePet(active: false);
			dressMeUp.ToggleName(active: false);
			dressMeUpCube.TogglePet(active: false);
			dressMeUpCube.ToggleName(active: false);
			PurchaseBackground.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			((Graphic)PriceText).color = new Color(0.8f, 0.8f, 0.8f, 1f);
			((TMP_Text)PriceText).text = "";
			((Graphic)PriceTextCube).color = new Color(0.8f, 0.8f, 0.8f, 1f);
			((TMP_Text)PriceTextCube).text = "";
			LoadCosmeticData();
		}
	}

	public void Update()
	{
		HandleRefreshTimers();
		if ((normalMenu.activeSelf || featuredMenu.activeSelf) && ControllerManager.Instance.CurrentUiState.MenuName == ((Object)this).name)
		{
			Player player = ReInput.players.GetPlayer(0);
			bool flag = false;
			int num = Array.IndexOf(controllerSelectableTabs, selectedTab);
			if (num > 0 && player.GetButtonDown(35))
			{
				num--;
				flag = true;
			}
			if (num < controllerSelectableTabs.Length - 1 && player.GetButtonDown(34))
			{
				num++;
				flag = true;
			}
			if (flag)
			{
				selectedTab = controllerSelectableTabs[num];
				OpenTab(selectedTab);
			}
		}
	}

	public void Open()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		CloseWaitModal();
		CloseConfirm();
		if (Object.op_Implicit((Object)(object)controllerNavMenu))
		{
			controllerNavMenu.OpenMenu();
		}
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			Vector3 position = ((Component)DestroyableSingleton<HudManager>.Instance).transform.position;
			position.z -= 100f;
			((Component)this).transform.position = position;
		}
		else
		{
			((Component)this).transform.position = new Vector3(0f, 0f, -100f);
		}
		((TMP_Text)beanAmount).text = DestroyableSingleton<InventoryManager>.Instance.UnusedBeans.ToString();
		((TMP_Text)starAmount).text = DestroyableSingleton<InventoryManager>.Instance.UnusedStars.ToString();
		OnOpen?.Invoke();
		OpenTab(StoreTab.Home);
	}

	public void OpenGeneralShop()
	{
	}

	private void DestroySliderObjects()
	{
		for (int i = 0; i < AllObjects.Count; i++)
		{
			Object.Destroy((Object)(object)AllObjects[i]);
		}
		AllObjects.Clear();
		controllerNavMenu.DefaultButtonSelected = null;
		controllerNavMenu.ControllerSelectable.Clear();
	}

	public void PreviewItem(StoreChip storeChip, CosmeticData product)
	{
		((TMP_Text)ItemName).text = product.GetItemName();
		((TMP_Text)ItemNameCube).text = product.GetItemName();
		storeChip.ShowSelected();
		if (product is NamePlateData)
		{
			((Component)NameplatePreviewArea).gameObject.SetActive(true);
			((Component)dressMeUp).gameObject.SetActive(false);
			((Component)dressMeUpCube).gameObject.SetActive(false);
			NameplatePreviewArea.PreviewNameplate(product.ProdId);
		}
		else if (product != null)
		{
			((Component)NameplatePreviewArea).gameObject.SetActive(false);
			((Component)dressMeUp).gameObject.SetActive(true);
			((Component)dressMeUpCube).gameObject.SetActive(true);
			product.PreviewOnPlayer(((Component)dressMeUp).gameObject.activeSelf ? dressMeUp : dressMeUpCube, 2);
		}
	}

	public void SetProduct(PurchaseButton button)
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected Obj, but got Unknown
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Expected Obj, but got Unknown
		if (PurchaseState == PurchaseStates.Started || !Object.op_Implicit((Object)(object)button) || button.Product == null)
		{
			return;
		}
		for (int i = 0; i < ColorChips.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)ColorChips[i]).gameObject);
		}
		ColorChips.Clear();
		ConfirmMenuScroller.SetBoundsMax(0f, 0f);
		ConfirmMenuScroller.ScrollToTop();
		ControllerManager.Instance.ClearDestroyedSelectableUiElements();
		if (button.Product is CosmicubeData)
		{
			dressMeUpCube.ResetCosmetics();
		}
		else
		{
			itemLimitedTimeText.SetProduct(button.Product);
			((Component)itemLimitedTimeText).gameObject.SetActive(button.Product.LimitedTimeAvailable.HasEndDateAndAvailable());
			dressMeUp.ResetCosmetics();
		}
		try
		{
			CurrentButton = button;
			if (CurrentButton.Product is CosmeticData)
			{
				CosmeticData prod = (CosmeticData)CurrentButton.Product;
				((TMP_Text)BundleName).text = prod.GetItemName();
				StoreChip chip = Object.Instantiate<StoreChip>(PreviewPrefab, ConfirmMenuScroller.Inner);
				((Component)chip).transform.localPosition = ConfirmCosmeticPosition;
				chip.Button.ClickMask = ConfirmMenuScroller.Hitbox;
				chip.SetData(prod);
				PreviewItem(chip, prod);
				((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					PreviewItem(chip, prod);
				}));
				ColorChips.Add(chip);
			}
			else if (CurrentButton.Product is CosmicubeData)
			{
				CosmicubeData cosmicubeData = (CosmicubeData)CurrentButton.Product;
				((TMP_Text)BundleName).text = cosmicubeData.GetCosmicubeName();
				((TMP_Text)ItemName).text = cosmicubeData.GetCosmicubeName();
				((TMP_Text)ItemNameCube).text = cosmicubeData.GetCosmicubeName();
			}
			else if (CurrentButton.Product is StarBundle)
			{
				StarBundle starBundle = (StarBundle)CurrentButton.Product;
				((TMP_Text)StarBundleName).text = starBundle.GetItemName();
			}
			else if (CurrentButton.Product is BundleData)
			{
				BundleData bundleData = (BundleData)CurrentButton.Product;
				((TMP_Text)BundleName).text = bundleData.GetItemName();
				CosmeticData[] cosmetics = bundleData.cosmetics;
				for (int num = 0; num < cosmetics.Length; num++)
				{
					CosmeticData cosmeticData = cosmetics[num];
					float num2 = XRange.Lerp((float)(num % NumPerRow) / ((float)NumPerRow - 1f));
					float num3 = YStart - (float)(num / NumPerRow) * YOffset;
					StoreChip chip2 = Object.Instantiate<StoreChip>(PreviewPrefab, ConfirmMenuScroller.Inner);
					chip2.Button.ClickMask = ConfirmMenuScroller.Hitbox;
					((Component)chip2).transform.localPosition = new Vector3(num2, num3, -60f);
					chip2.SetData(cosmeticData);
					((UnityEvent)chip2.Button.OnClick).AddListener((UnityAction)(() =>
					{
						DeselectAllItems();
						PreviewItem(chip2, cosmeticData);
					}));
					if (num == 0)
					{
						PreviewItem(chip2, cosmeticData);
					}
					ColorChips.Add(chip2);
					ControllerManager.Instance.AddSelectableUiElement(chip2.Button);
				}
				ConfirmMenuScroller.SetBoundsMax(Mathf.Max(0f, (float)Mathf.CeilToInt(((float)cosmetics.Length - 8f) / 4f) * YOffset), 0f);
			}
			else
			{
				((TMP_Text)BundleName).text = "Remove All Ads";
			}
			if (button.Product is StarBundle)
			{
				((TMP_Text)StarPriceText).text = "$" + ((StarBundle)button.Product).price;
				return;
			}
			if (button.Purchased)
			{
				PurchaseBackground.color = new Color(0.5f, 0.5f, 0.5f, 1f);
				((Graphic)PriceText).color = new Color(0.8f, 0.8f, 0.8f, 1f);
				((TMP_Text)PriceText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Owned);
				((Graphic)PriceTextCube).color = new Color(0.8f, 0.8f, 0.8f, 1f);
				((TMP_Text)PriceTextCube).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Owned);
				return;
			}
			((TMP_Text)PriceText).text = button.Price;
			((TMP_Text)PriceTextCube).text = button.Price;
			if ((button.Product.StarCost > 0 && DestroyableSingleton<InventoryManager>.Instance.UnusedStars < button.Product.StarCost) || (button.Product.BeanCost > 0 && DestroyableSingleton<InventoryManager>.Instance.UnusedBeans < button.Product.BeanCost))
			{
				priceButton.SetInactive();
				((Behaviour)((Component)priceButton).GetComponent<ControllerButtonBehavior>()).enabled = false;
				priceButtonCube.SetInactive();
				((Behaviour)((Component)priceButtonCube).GetComponent<ControllerButtonBehavior>()).enabled = false;
			}
			else
			{
				priceButton.SetUseable();
				((Behaviour)((Component)priceButton).GetComponent<ControllerButtonBehavior>()).enabled = true;
				priceButtonCube.SetUseable();
				((Behaviour)((Component)priceButtonCube).GetComponent<ControllerButtonBehavior>()).enabled = true;
			}
		}
		catch (Exception ex)
		{
			logger.Error("Could not set product: " + button.ProductId);
			throw ex;
		}
	}

	private void DeselectAllItems()
	{
		ColorChips.ForEach((ColorChip c) =>
		{
			c.Deselect();
		});
	}

	public void BuyProduct()
	{
		if (!Object.op_Implicit((Object)(object)CurrentButton) || CurrentButton.Purchased || ((Component)plsWaitModal).gameObject.activeSelf)
		{
			logger.Error("Buy product Failed.");
			return;
		}
		((Component)plsWaitModal).gameObject.SetActive(true);
		plsWaitModal.Show();
		CurrencyTypes currency = CurrencyTypes.Stars;
		if (CurrentButton.Product.BeanCost > 0)
		{
			currency = CurrencyTypes.Beans;
		}
		((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<InventoryManager>.Instance.CoPurchaseItem(CurrentButton.Product.ProdId, currency, (ResponseState cb, Response<ResponsePurchase> response) =>
		{
			((TMP_Text)beanAmount).text = DestroyableSingleton<InventoryManager>.Instance.UnusedBeans.ToString();
			((TMP_Text)starAmount).text = DestroyableSingleton<InventoryManager>.Instance.UnusedStars.ToString();
			plsWaitModal.SetResponse(cb, response?.Errors);
			if (cb == ResponseState.Success)
			{
				CloseConfirm();
				ReSortCosmeticTabs();
				OpenTab(selectedTab);
			}
		}));
	}

	public void Close()
	{
		if (((Component)plsWaitModal).gameObject.activeSelf)
		{
			CloseWaitModal();
			CloseConfirm();
		}
		else if (normalMenu.activeSelf || featuredMenu.activeSelf)
		{
			HatsTab hatsTab = Object.FindObjectOfType<HatsTab>();
			if (Object.op_Implicit((Object)(object)hatsTab))
			{
				hatsTab.OnDisable();
				hatsTab.OnEnable();
			}
			OnClose?.Invoke();
			DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(((Component)this).gameObject, null, null);
			if ((Object)(object)manager == (Object)null)
			{
				manager = Object.FindObjectOfType<MainMenuManager>();
			}
			if (Object.op_Implicit((Object)(object)manager))
			{
				manager.CheckNewItems();
			}
		}
		else
		{
			CloseWaitModal();
			CloseConfirm();
		}
		logger.Info("Closing store");
	}

	public void CloseEntirely()
	{
		CloseWaitModal();
		CloseConfirm();
		Close();
	}

	private void LoadCosmeticData()
	{
		string text = "";
		try
		{
			SetRefreshTimers();
			ReSortCosmeticTabs();
			try
			{
				((Component)LoadingText).gameObject.SetActive(false);
			}
			catch
			{
			}
		}
		catch (Exception ex)
		{
			logger.Error("Exception: " + text + ": " + ex);
			DestroySliderObjects();
			((TMP_Text)LoadingText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LoadingFailed) + "\r\n" + text;
			((Component)LoadingText).gameObject.SetActive(true);
		}
	}

	public void OnSaveDataChanged()
	{
		ReSortCosmeticTabs();
	}

	private void ReSortCosmeticTabs()
	{
		allHats = GetSortedStoreCosmetics(DestroyableSingleton<HatManager>.Instance.AllHats);
		allSkins = GetSortedStoreCosmetics(DestroyableSingleton<HatManager>.Instance.AllSkins);
		allVisors = GetSortedStoreCosmetics(DestroyableSingleton<HatManager>.Instance.AllVisors);
		allPlates = GetSortedStoreCosmetics(DestroyableSingleton<HatManager>.Instance.AllNamePlates);
		allPets = GetSortedStoreCosmetics(DestroyableSingleton<HatManager>.Instance.AllPets);
		allCubes = (from p in DestroyableSingleton<CosmicubeManager>.Instance.GetAvailableCubeData()
			orderby DataManager.Player.Purchases.GetPurchase(p.ProdId, ""), p.GetDateCreated() descending
			select p).ToArray();
		allStars = DestroyableSingleton<HatManager>.Instance.AllStarBundles.OrderBy((StarBundle p) => p.price).ToArray();
		allBundles = DestroyableSingleton<HatManager>.Instance.AllBundles.Where((BundleData h) => !h.Free && !h.NotInStore).ToArray();
		DestroyableSingleton<HatManager>.Instance.RefreshFeatured();
		allFeaturedItems = DestroyableSingleton<HatManager>.Instance.AllFeaturedItems.ToArray();
		allFeaturedCubes = DestroyableSingleton<HatManager>.Instance.AllFeaturedCubes.Where((CosmicubeData x) => x.LimitedTimeAvailable.IsAvailable()).ToArray();
		allFeaturedBundles = DestroyableSingleton<HatManager>.Instance.AllFeaturedBundles.Where((BundleData x) => x.BundleSize == BundleSizes.Wide).ToArray();
		allLargeFeaturedBundles = DestroyableSingleton<HatManager>.Instance.AllFeaturedBundles.Where((BundleData x) => x.BundleSize == BundleSizes.Large).ToArray();
	}

	private void SetRefreshTimers()
	{
		Dictionary<DateTime, List<StoreTab>> dictionary = new Dictionary<DateTime, List<StoreTab>>();
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllHats, StoreTab.Hats);
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllSkins, StoreTab.Skins);
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllVisors, StoreTab.Visors);
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllNamePlates, StoreTab.Nameplates);
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllPets, StoreTab.Pets);
		AddRefreshTimers(dictionary, DestroyableSingleton<CosmicubeManager>.Instance.GetAllCubeData(), default(StoreTab));
		AddRefreshTimers(dictionary, DestroyableSingleton<HatManager>.Instance.AllBundles, StoreTab.Bundles);
		AddRefreshTimers(dictionary, DestroyableSingleton<ReferenceDataManager>.Instance.GetAllFeaturedItems());
		List<RefreshTimer> list = new List<RefreshTimer>();
		foreach (DateTime key in dictionary.Keys)
		{
			if (!(key <= DateTime.UtcNow))
			{
				list.Add(new RefreshTimer(key, dictionary[key].ToArray()));
			}
		}
		refreshTimers = list.OrderBy((RefreshTimer x) => x.time).ToArray();
		DebugRefreshTimers();
	}

	private void HandleRefreshTimers()
	{
		int num = -1;
		for (int i = 0; i < refreshTimers.Length && !(refreshTimers[i].time > DateTime.UtcNow); i++)
		{
			Logger.GlobalInstance.Info(string.Format("{0} passed: {1}", "RefreshTimer", refreshTimers[i].time));
			num = i;
		}
		if (num < 0)
		{
			return;
		}
		bool flag = false;
		for (int j = 0; j <= num; j++)
		{
			if (refreshTimers[j].tabs.Contains(selectedTab))
			{
				flag = true;
				break;
			}
		}
		Logger.GlobalInstance.Info("Re-sorting cosmetics");
		ReSortCosmeticTabs();
		if (flag)
		{
			Logger.GlobalInstance.Info($"Refreshing current store tab: {selectedTab}");
			OpenTab(selectedTab);
		}
		refreshTimers = refreshTimers.Skip(num + 1).ToArray();
		DebugRefreshTimers();
	}

	private void UpdateRefreshTimers()
	{
		List<RefreshTimer> list = new List<RefreshTimer>(refreshTimers);
		DateTime utcNow = DateTime.UtcNow;
		RefreshTimer[] array = refreshTimers;
		foreach (RefreshTimer refreshTimer in array)
		{
			if (refreshTimer.time < utcNow)
			{
				list.Remove(refreshTimer);
			}
		}
		refreshTimers = list.ToArray();
	}

	private void AddRefreshTimers(Dictionary<DateTime, List<StoreTab>> dictionary, IEnumerable<CosmeticData> cosmetics, params StoreTab[] tabs)
	{
		foreach (CosmeticData cosmetic in cosmetics)
		{
			if (cosmetic.LimitedTimeAvailable.IsLimitedTime())
			{
				DateTime dateTime = cosmetic.LimitedTimeAvailable.timeStart.GetDateTime();
				DateTime dateTime2 = cosmetic.LimitedTimeAvailable.timeEnd.GetDateTime();
				AddRefreshTimers(dictionary, dateTime, dateTime2, tabs);
			}
		}
	}

	private void AddRefreshTimers(Dictionary<DateTime, List<StoreTab>> dictionary, IEnumerable<CosmicubeData> cosmicubes, params StoreTab[] tabs)
	{
		foreach (CosmicubeData cosmicube in cosmicubes)
		{
			if (!cosmicube.IsAlwaysAvailable)
			{
				DateTime dateTime = cosmicube.LimitedTimeAvailable.timeStart.GetDateTime();
				DateTime dateTime2 = cosmicube.LimitedTimeAvailable.timeEnd.GetDateTime();
				AddRefreshTimers(dictionary, dateTime, dateTime2, tabs);
			}
		}
	}

	private void AddRefreshTimers(Dictionary<DateTime, List<StoreTab>> dictionary, IEnumerable<BundleData> bundles, params StoreTab[] tabs)
	{
		foreach (BundleData bundle in bundles)
		{
			if (bundle.LimitedTimeAvailable.IsLimitedTime())
			{
				DateTime dateTime = bundle.LimitedTimeAvailable.timeStart.GetDateTime();
				DateTime dateTime2 = bundle.LimitedTimeAvailable.timeEnd.GetDateTime();
				AddRefreshTimers(dictionary, dateTime, dateTime2, tabs);
			}
		}
	}

	private void AddRefreshTimers(Dictionary<DateTime, List<StoreTab>> dictionary, IEnumerable<FeaturedItemGroup> features)
	{
		foreach (FeaturedItemGroup feature in features)
		{
			if (feature.LimitedTimeAvailable.IsLimitedTime())
			{
				DateTime dateTime = feature.LimitedTimeAvailable.timeStart.GetDateTime();
				DateTime dateTime2 = feature.LimitedTimeAvailable.timeEnd.GetDateTime();
				AddRefreshTimers(dictionary, dateTime, dateTime2, StoreTab.Home);
				AddRefreshTimers(dictionary, feature.featuredBundles, StoreTab.Home, StoreTab.Bundles);
				AddRefreshTimers(dictionary, feature.featuredCubes, StoreTab.Home, StoreTab.Cubes);
				AddRefreshTimers(dictionary, feature.featuredItems, StoreTab.Home, StoreTab.Hats, StoreTab.Visors, StoreTab.Skins, StoreTab.Pets, StoreTab.Nameplates);
			}
		}
	}

	private void AddRefreshTimers(Dictionary<DateTime, List<StoreTab>> dictionary, DateTime start, DateTime end, params StoreTab[] tabs)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (start > utcNow)
		{
			AddRefreshTimer(dictionary, start, tabs);
		}
		if (end > utcNow)
		{
			AddRefreshTimer(dictionary, end, tabs);
		}
	}

	private void AddRefreshTimer(Dictionary<DateTime, List<StoreTab>> dictionary, DateTime time, params StoreTab[] tabs)
	{
		if (!dictionary.ContainsKey(time))
		{
			dictionary.Add(time, new List<StoreTab>());
		}
		if (dictionary[time] == null)
		{
			dictionary[time] = new List<StoreTab>();
		}
		foreach (StoreTab item in tabs)
		{
			if (!dictionary[time].Contains(item))
			{
				dictionary[time].Add(item);
			}
		}
	}

	private void DebugRefreshTimers()
	{
	}

	private T[] GetSortedStoreCosmetics<T>(IEnumerable<T> all) where T : CosmeticData
	{
		return (from p in all
			where !p.Free && !p.NotInStore && (!p.LimitedTimeAvailable.IsLimitedTime() || p.LimitedTimeAvailable.IsLimitedAndAvailable()) && string.IsNullOrWhiteSpace(p.BundleId)
			orderby DataManager.Player.Purchases.GetPurchase(p.ProductId, p.BundleId)
			select p).ToArray();
	}

	private void ShowTab()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_079b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0808: Unknown result type (might be due to invalid IL or missing references)
		//IL_080f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_071e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		DestroySliderObjects();
		string text = "";
		List<PurchaseButton> list = new List<PurchaseButton>();
		CloseConfirm();
		StoreTab storeTab = selectedTab;
		logger.Info($"Showing tab {storeTab}");
		try
		{
			text = "Couldn't place products";
			Vector3 val = new Vector3(selectedTabObject.XRange.Lerp(0.5f), selectedTabObject.ItemListStartY);
			((Component)OpenAllInEShopButton).gameObject.SetActive(false);
			RestorePurchasesObj.gameObject.SetActive(false);
			if (((Component)OpenAllInEShopButton).gameObject.activeSelf)
			{
				val.y += -1.95f;
			}
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PetFailFetchData);
			val.y += -1.95f;
			float num = Mathf.Min(1f, Camera.main.aspect / 1.7777778f);
			featuredMenu.SetActive(false);
			normalMenu.SetActive(true);
			BundleTitles.ForEach((TextMeshPro t) =>
			{
				Object.Destroy((Object)(object)((Component)t).gameObject);
			});
			BundleTitles.Clear();
			Scroller.allowX = true;
			Scroller.allowY = true;
			Scroller.showX = true;
			Scroller.showY = true;
			switch (storeTab)
			{
			case StoreTab.Cubes:
			{
				val.y += -0.975f;
				Vector3 position6 = val;
				IBuyable[] products = allCubes;
				val = AddProductFromList(position6, products, list, -3.9f, selectedTabObject.NumPerRow, CubesRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Cosmicubes);
				break;
			}
			case StoreTab.Bundles:
				foreach (BundlePurchaseButton item in featuredMenu.GetComponentsInChildren<BundlePurchaseButton>(true).ToList())
				{
					item.UpdateBundle();
					item.PassiveButton.ClickMask = FeatureTabScroller.Hitbox;
					list.Add(item);
				}
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bundles);
				FeatureTabScroller.Inner.localScale = Vector3.one * num;
				featuredMenu.SetActive(true);
				normalMenu.SetActive(false);
				break;
			case StoreTab.Pets:
			{
				Vector3 position11 = val;
				IBuyable[] products = allPets;
				val = AddProductFromList(position11, products, list, -1.95f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PetLabel);
				break;
			}
			case StoreTab.Hats:
			{
				Vector3 position10 = val;
				IBuyable[] products = allHats;
				val = AddProductFromList(position10, products, list, -1.95f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HatLabel);
				break;
			}
			case StoreTab.Visors:
			{
				Vector3 position9 = val;
				IBuyable[] products = allVisors;
				val = AddProductFromList(position9, products, list, -1.95f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visors);
				break;
			}
			case StoreTab.Nameplates:
			{
				Vector3 position7 = val;
				IBuyable[] products = allPlates;
				val = AddProductFromList(position7, products, list, -1.95f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NamePlates);
				break;
			}
			case StoreTab.Skins:
			{
				Vector3 position8 = val;
				IBuyable[] products = allSkins;
				val = AddProductFromList(position8, products, list, -1.95f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SkinLabel);
				break;
			}
			case StoreTab.Home:
				((TMP_Text)tabHeader).text = string.Empty;
				if (allLargeFeaturedBundles.Length != 0 || allFeaturedBundles.Length != 0)
				{
					TextMeshPro val2 = Object.Instantiate<TextMeshPro>(tabHeader, Scroller.Inner);
					((Behaviour)((Component)val2).GetComponent<AspectPosition>()).enabled = false;
					((TMP_Text)val2).horizontalAlignment = (HorizontalAlignmentOptions)2;
					val2.transform.position = Vector3.zero;
					val2.transform.localPosition = new Vector3(0f, val.y);
					((TMP_Text)val2).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FeaturedBundles);
					val.y += -1.95f;
					BundleTitles.Add(val2);
					if (allLargeFeaturedBundles.Length != 0)
					{
						Vector3 position2 = val;
						IBuyable[] products = allLargeFeaturedBundles;
						val = AddProductFromList(position2, products, list, -3.9f, 3, BundleRange, 0.75f, centerRemainder: true);
					}
					if (allFeaturedBundles.Length != 0)
					{
						Vector3 position3 = val;
						IBuyable[] products = allFeaturedBundles;
						val = AddProductFromList(position3, products, list, -1.95f, 3, BundleRange);
					}
				}
				if (allFeaturedCubes.Length != 0)
				{
					TextMeshPro val3 = Object.Instantiate<TextMeshPro>(tabHeader, Scroller.Inner);
					((Behaviour)((Component)val3).GetComponent<AspectPosition>()).enabled = false;
					((TMP_Text)val3).horizontalAlignment = (HorizontalAlignmentOptions)2;
					val3.transform.position = Vector3.zero;
					val3.transform.localPosition = new Vector3(0f, val.y);
					((TMP_Text)val3).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FeaturedCubes);
					val.y += -1.95f;
					BundleTitles.Add(val3);
					Vector3 position4 = val;
					IBuyable[] products = allFeaturedCubes;
					val = AddProductFromList(position4, products, list, -3.9f, 3, CubesRange, 0.75f, centerRemainder: true);
				}
				if (allFeaturedItems.Length != 0)
				{
					TextMeshPro val4 = Object.Instantiate<TextMeshPro>(tabHeader, Scroller.Inner);
					((Behaviour)((Component)val4).GetComponent<AspectPosition>()).enabled = false;
					((TMP_Text)val4).horizontalAlignment = (HorizontalAlignmentOptions)2;
					val4.transform.position = Vector3.zero;
					val4.transform.localPosition = new Vector3(0f, val.y);
					((TMP_Text)val4).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FeaturedItems);
					val.y += -0.975f;
					BundleTitles.Add(val4);
					Vector3 position5 = val;
					IBuyable[] products = allFeaturedItems;
					val = AddProductFromList(position5, products, list, -1.95f, 6, Tabs[1].XRange, 1f, centerRemainder: true);
				}
				Scroller.allowX = false;
				Scroller.allowY = false;
				Scroller.showX = false;
				Scroller.showY = false;
				break;
			case StoreTab.Stars:
			{
				val.y += -0.975f;
				Vector3 position = val;
				IBuyable[] products = allStars;
				val = AddProductFromList(position, products, list, -3.9f, selectedTabObject.NumPerRow, selectedTabObject.XRange);
				((TMP_Text)tabHeader).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Stars);
				break;
			}
			}
			text = "Couldn't finalize menu";
			float num2 = -4f * num + 2.9f;
			Scroller.SetBounds(new FloatRange(num2, Mathf.Max(num2, (0f - val.y) * num - 2.5f)), null);
			Scroller.Inner.localScale = Vector3.one * num;
			Scroller.ScrollToTop();
		}
		catch (Exception ex)
		{
			logger.Error("Exception: " + text + ": " + ex);
			DestroySliderObjects();
			((TMP_Text)LoadingText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LoadingFailed) + "\r\n" + text;
			((Component)LoadingText).gameObject.SetActive(true);
		}
		ControllerUiElementsState menu = ControllerManager.Instance.GetMenu(((Object)this).name);
		if (menu != null)
		{
			ControllerManager.Instance.ClearDestroyedSelectableUiElements(menu);
			foreach (PurchaseButton item2 in list)
			{
				ControllerManager.Instance.AddSelectableUiElement(item2.PassiveButton, menu);
			}
		}
		comingSoonText.SetActive(list.Count == 0);
	}

	private Vector3 AddProductFromList(Vector3 position, IBuyable[] products, List<PurchaseButton> allBtns, float yOffset, int numPerRow, FloatRange xRange, float spacingMultiplier = 1f, bool centerRemainder = false)
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = products.Length % numPerRow;
		int num3 = products.Length;
		foreach (IBuyable item in products)
		{
			if ((num3 <= num2) & centerRemainder)
			{
				if (num3 == num2 && products.Length > numPerRow)
				{
					position.y += yOffset;
				}
				int num4 = num % numPerRow;
				int num5 = (numPerRow - num2) % numPerRow;
				position.x = xRange.Lerp((float)num4 / ((float)numPerRow - 1f));
				position.x += Mathf.Abs(xRange.Lerp(0f) - xRange.Lerp((float)num5 / ((float)numPerRow - 1f))) / 2f;
			}
			else
			{
				int num6 = num % numPerRow;
				position.x = xRange.Lerp((float)num6 / ((float)numPerRow - 1f));
				if (num6 == 0 && num > 1)
				{
					position.y += yOffset;
				}
			}
			allBtns.Add(InsertProduct(position, item));
			num++;
			num3--;
		}
		position.y += yOffset * spacingMultiplier;
		return position;
	}

	private PurchaseButton InsertProduct(Vector3 position, IBuyable item)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		PurchaseButton purchaseButton;
		if (!(item is BundleData))
		{
			purchaseButton = ((!(item is NamePlateData)) ? Object.Instantiate<PurchaseButton>(PurchasablePrefab, Scroller.Inner) : Object.Instantiate<PurchaseButton>(PurchasableNameplatePrefab, Scroller.Inner));
		}
		else
		{
			BundleSizes bundleSize = (item as BundleData).BundleSize;
			purchaseButton = ((bundleSize == BundleSizes.Wide || bundleSize != BundleSizes.Large) ? Object.Instantiate<BundlePurchaseButton>(BundlePurchasablePrefab, Scroller.Inner) : Object.Instantiate<BundlePurchaseButton>(LargeBundlePurchasablePrefab, Scroller.Inner));
		}
		AllObjects.Add(((Component)purchaseButton).gameObject);
		((Component)purchaseButton).transform.localPosition = position;
		purchaseButton.PassiveButton.ClickMask = Scroller.Hitbox;
		purchaseButton.Parent = this;
		string text = "";
		string text2 = "";
		bool flag = false;
		string text3 = "";
		CurrencyTypes currency = CurrencyTypes.Stars;
		if (item is StarBundle)
		{
			text = ((StarBundle)item).GetItemName();
			text2 = ((StarBundle)item).SteamPrice;
			flag = false;
			text3 = ((StarBundle)item).ProdId;
		}
		else if (item is BundleData)
		{
			BundleData bundleData = (BundleData)item;
			text3 = bundleData.ProdId;
			text = bundleData.GetItemName();
			flag = DataManager.Player.Purchases.GetPurchase(bundleData.ProdId, "");
			text2 = bundleData.starCost.ToString();
		}
		else if (item is CosmicubeData)
		{
			CosmicubeData cosmicubeData = (CosmicubeData)item;
			text3 = cosmicubeData.ProdId;
			text = cosmicubeData.GetCosmicubeName();
			flag = DataManager.Player.Purchases.GetPurchase(cosmicubeData.ProdId, "");
			if (item.StarCost > 0)
			{
				text2 = item.StarCost.ToString();
			}
			else
			{
				text2 = item.BeanCost.ToString();
				currency = CurrencyTypes.Beans;
			}
		}
		else
		{
			CosmeticData cosmeticData = (CosmeticData)item;
			text3 = cosmeticData.ProductId;
			text = cosmeticData.GetItemName();
			flag = DataManager.Player.Purchases.GetPurchase(cosmeticData.ProductId, cosmeticData.BundleId);
			if (item.StarCost > 0)
			{
				text2 = item.StarCost.ToString();
			}
			else
			{
				text2 = item.BeanCost.ToString();
				currency = CurrencyTypes.Beans;
			}
		}
		purchaseButton.SetItem(item, text3, text, text2, flag, currency);
		UiElement component = ((Component)purchaseButton).GetComponent<UiElement>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (!Object.op_Implicit((Object)(object)controllerNavMenu.DefaultButtonSelected))
			{
				controllerNavMenu.DefaultButtonSelected = component;
			}
			controllerNavMenu.ControllerSelectable.Add(component);
		}
		return purchaseButton;
	}

	public void BuyStars()
	{
		if (Object.op_Implicit((Object)(object)CurrentButton) && !CurrentButton.Purchased && PurchaseState != PurchaseStates.Started)
		{
			((Component)plsWaitModal).gameObject.SetActive(true);
			plsWaitModal.Show();
			if (DataManager.Player.Account.LoginStatus != EOSManager.AccountLoginStatus.LoggedIn)
			{
				DisplayFailed(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LoggedInErrorStarPurchase));
			}
			else
			{
				DestroyableSingleton<StoreManager>.Instance.InitiateStorePurchaseStar(CurrentButton);
			}
		}
	}

	public void ResetPurchaseState()
	{
		PurchaseState = PurchaseStates.NotStarted;
	}
}
