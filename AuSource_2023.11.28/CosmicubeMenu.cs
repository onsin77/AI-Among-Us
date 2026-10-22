using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using Assets.InnerNet;
using Innersloth.Assets;
using Rewired;
using TMPro;
using UnityEngine;

public class CosmicubeMenu : MonoBehaviour
{
	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public GameObject zoomInGlyph;

	public GameObject zoomOutGlyph;

	public TextMeshPro cubeTitle;

	public TextMeshPro percentCompletedText;

	public TextMeshPro selectedItemName;

	public TextMeshPro selectedItemCategory;

	public TextMeshPro[] selectedItemPrice;

	public TextMeshPro confirmPurchaseText;

	public GameObject confirmWindow;

	public GameObject equipButton;

	public GameObject equippedText;

	public SpriteRenderer podConfirmIcon;

	public CubeCellPurchaseButton purchaseButton;

	public GameObject lockedButton;

	public PoolablePlayer dressMeUp;

	public PlayerVoteArea nameplatePreview;

	public StoreMannequin storeMannequin;

	public SpriteRenderer nameplateConfirmPreview;

	public SpriteRenderer podIcon;

	public TextMeshPro podAmount;

	public Scroller scroller;

	public GameObject cubeHolder;

	public SpriteRenderer zoomOutButton;

	public SpriteRenderer zoomInButton;

	public AudioClip cubeCellUnlockSound;

	public UpdateProductTimerDisplay limitedTimeText;

	public PurchasePopUp plsWaitModal;

	public static int CellMaskLayer = 61;

	private float zoomLevel = 1f;

	private float minZoom = 0.5f;

	private float maxZoom = 1.5f;

	private bool previewMode;

	private bool animatingPurchase;

	private Cosmicube currentCube;

	private CosmicubeData currentCubeData;

	private CosmicubeCell currentCell;

	private readonly Logger logger = new Logger("CosmicubeMenu", Logger.Level.Info, Logger.Category.UI);

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		currentCell = null;
		currentCube = null;
	}

	public void ZoomIn()
	{
		AdjustZoom(0.5f);
	}

	public void ZoomOut()
	{
		AdjustZoom(-0.5f);
	}

	public void Show(string cubeID, bool previewMode = false)
	{
		this.previewMode = previewMode;
		animatingPurchase = false;
		((Component)this).gameObject.SetActive(true);
		if (!previewMode)
		{
			dressMeUp.ResetCosmetics();
			dressMeUp.ToggleName(active: false);
		}
		dressMeUp.SetBodyColor(DataManager.Player.Customization.Color);
		currentCube = ((Component)this).GetComponentInChildren<Cosmicube>();
		if (Object.op_Implicit((Object)(object)currentCube))
		{
			Object.Destroy((Object)(object)((Component)currentCube).gameObject);
		}
		CellMaskLayer = 61;
		currentCubeData = DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(cubeID);
		((MonoBehaviour)this).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync<Sprite>(currentCubeData.podIconRef, (Action<Sprite>)((Sprite icon) =>
		{
			podIcon.sprite = icon;
			podConfirmIcon.sprite = icon;
		})));
		((MonoBehaviour)this).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync<Cosmicube>(currentCubeData.CosmicubePrefabRef, (Action<Cosmicube>)((Cosmicube cube) =>
		{
			currentCube = Object.Instantiate<Cosmicube>(cube, cubeHolder.transform);
			currentCube.SetMenu(this);
			LoadCube();
			((TMP_Text)podAmount).text = DestroyableSingleton<InventoryManager>.Instance.GetPodCount(currentCubeData.podId).ToString();
			PassiveButton firstCell = currentCube.GetFirstCell();
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, firstCell, new List<UiElement>(), gridNav: true);
			foreach (PassiveButton allButton in currentCube.GetAllButtons())
			{
				allButton.ClickMask = scroller.Hitbox;
				ControllerManager.Instance.AddSelectableUiElement(allButton);
			}
			scroller.UpdateScrollBars();
			((TMP_Text)selectedItemName).text = string.Empty;
			((TMP_Text)selectedItemCategory).text = string.Empty;
			currentCell = null;
			((Component)purchaseButton).gameObject.SetActive(false);
			equipButton.SetActive(false);
			equippedText.SetActive(false);
		})));
		if ((Object)(object)limitedTimeText != (Object)null)
		{
			limitedTimeText.SetProduct(currentCubeData);
			((Component)limitedTimeText).gameObject.SetActive(GetCurrentProductLimitedTime().IsLimitedAndAvailable());
		}
		UpdatePercentComplete();
	}

	private void UpdatePercentComplete()
	{
		if (!((Object)(object)percentCompletedText == (Object)null))
		{
			float completionProgress = DestroyableSingleton<CosmicubeManager>.Instance.GetCompletionProgress(currentCubeData);
			((TMP_Text)percentCompletedText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CosmicubeProgression, (int)(completionProgress * 100f));
		}
	}

	public LimitedTimeStartEnd GetCurrentProductLimitedTime()
	{
		if ((Object)(object)currentCubeData == (Object)null)
		{
			return default;
		}
		return currentCubeData.LimitedTimeAvailable;
	}

	public void Close()
	{
		if (((Component)plsWaitModal).gameObject.activeSelf)
		{
			((Component)plsWaitModal).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).GetComponent<TransitionOpen>().Close();
		}
	}

	public void EquipSelectedItem()
	{
		if ((Object)(object)currentCell == (Object)null || !currentCell.unlocked || currentCell.CanBePurchased())
		{
			logger.Error("Somehow tried to equip an item that's not available to equip");
			return;
		}
		PlayerCustomization.Equip(currentCell.CosmeticData.unlockableCosmetic);
		equipButton.SetActive(false);
		equippedText.SetActive(true);
		logger.Info("Equipped " + currentCell.CosmeticData.GetProdId());
	}

	public void SelectItem(CosmicubeCell cell)
	{
		if (animatingPurchase)
		{
			return;
		}
		currentCell?.SetDeselected();
		currentCell = cell;
		currentCell.SetSelected();
		((Component)storeMannequin).gameObject.SetActive(!(cell.CosmeticData.unlockableCosmetic is NamePlateData));
		((Component)nameplateConfirmPreview).gameObject.SetActive(cell.CosmeticData.unlockableCosmetic is NamePlateData);
		((Component)dressMeUp).gameObject.SetActive(!(cell.CosmeticData.unlockableCosmetic is NamePlateData));
		((Component)nameplatePreview).gameObject.SetActive(cell.CosmeticData.unlockableCosmetic is NamePlateData);
		((Component)nameplatePreview.MaskArea).gameObject.SetActive(cell.CosmeticData.unlockableCosmetic is NamePlateData);
		if (cell.CosmeticData.unlockableCosmetic is NamePlateData)
		{
			NamePlateData namePlateData = cell.CosmeticData.unlockableCosmetic as NamePlateData;
			nameplatePreview.PreviewNameplate(namePlateData.ProdId);
			((MonoBehaviour)this).StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<NamePlateViewData>)namePlateData, (Action<NamePlateViewData>)((NamePlateViewData viewData) =>
			{
				nameplateConfirmPreview.sprite = viewData?.Image;
			})));
		}
		else if (cell.CosmeticData.unlockableCosmetic is PetData)
		{
			dressMeUp.ResetCosmetics();
			dressMeUp.ToggleName(active: false);
			PetData petData = cell.CosmeticData.unlockableCosmetic as PetData;
			dressMeUp.SetPetIdle(petData, DataManager.Player.Customization.Color);
		}
		else
		{
			cell.CosmeticData.unlockableCosmetic.PreviewOnPlayer(dressMeUp, DataManager.Player.Customization.Color);
		}
		((TMP_Text)selectedItemName).text = cell.CosmeticData.unlockableCosmetic.GetItemName();
		((TMP_Text)selectedItemCategory).text = cell.CosmeticData.unlockableCosmetic.GetItemCategory();
		if (previewMode)
		{
			((Behaviour)selectedItemName).enabled = false;
			((Behaviour)selectedItemCategory).enabled = false;
		}
		selectedItemPrice.ForEach((TextMeshPro t) =>
		{
			((TMP_Text)t).text = cell.CosmeticData.cost.ToString();
		});
		bool flag = false;
		bool flag2 = cell.unlocked && !previewMode;
		if (flag2)
		{
			flag = PlayerCustomization.IsEquipped(cell.CosmeticData.unlockableCosmetic);
		}
		equipButton.SetActive(flag2 && !flag);
		equippedText.SetActive(flag2 & flag);
		((Component)purchaseButton).gameObject.SetActive(cell.CanBePurchased() && !previewMode);
		lockedButton.SetActive(!cell.CanBePurchased() && !cell.unlocked && !previewMode);
		if (((Component)purchaseButton).gameObject.activeSelf)
		{
			purchaseButton.podIcon.sprite = podIcon.sprite;
			if (DestroyableSingleton<InventoryManager>.Instance.GetPodCount(currentCubeData.podId) < cell.CosmeticData.cost)
			{
				purchaseButton.SetInactive();
			}
			else
			{
				purchaseButton.SetUseable();
			}
		}
	}

	private void LoadCube()
	{
		((TMP_Text)cubeTitle).text = currentCubeData.GetCosmicubeName();
		((TMP_Text)confirmPurchaseText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ConfirmPurchaseText, currentCubeData.GetCosmicubeName());
		scroller.SetBounds(currentCubeData.YRange, currentCubeData.XRange);
	}

	public void ClickPurchase()
	{
		if (previewMode || animatingPurchase || DestroyableSingleton<InventoryManager>.Instance.GetPodCount(currentCubeData.podId) < currentCell.CosmeticData.cost)
		{
			return;
		}
		((Component)plsWaitModal).gameObject.SetActive(true);
		plsWaitModal.Show();
		DestroyableSingleton<InventoryManager>.Instance.UnlockCubeItem(currentCell.CosmeticData.GetProdId(), currentCubeData.ProdId, (ResponseState cb, Response<ResponsePurchase> response) =>
		{
			if (cb == ResponseState.Success)
			{
				((MonoBehaviour)this).StartCoroutine(CoAnimatePurchase());
			}
			else
			{
				plsWaitModal.SetResponse(cb, response.Errors);
			}
		});
	}

	private IEnumerator CoAnimatePurchase()
	{
		animatingPurchase = true;
		SoundManager.Instance.PlaySoundImmediate(cubeCellUnlockSound, loop: false, 0.5f);
		((Component)plsWaitModal).gameObject.SetActive(false);
		((TMP_Text)podAmount).text = DestroyableSingleton<InventoryManager>.Instance.GetPodCount(currentCube.Data.podId).ToString();
		yield return currentCell.CoPurchase();
		animatingPurchase = false;
		UpdatePercentComplete();
		SelectItem(currentCell);
	}

	public void ShowConfirmPurchase()
	{
		if ((Object)(object)currentCell != (Object)null && currentCell.CanBePurchased() && DestroyableSingleton<InventoryManager>.Instance.GetPodCount(currentCube.Data.podId) >= currentCell.CosmeticData.cost)
		{
			confirmWindow.SetActive(true);
			if (currentCell.CosmeticData.unlockableCosmetic is PetData)
			{
				PetData product = currentCell.CosmeticData.unlockableCosmetic as PetData;
				storeMannequin.WearThing(product, PlayerMaterial.MaskType.None);
			}
			else
			{
				storeMannequin.WearThing(currentCell.CosmeticData.unlockableCosmetic, PlayerMaterial.MaskType.ScrollingUI, DataManager.Player.Customization.Color);
			}
			logger.Info("Showing confirm window for " + currentCell.cosmetic.GetProdId());
		}
	}

	private void Update()
	{
		if (confirmWindow.activeSelf)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		if (player.GetButtonDown(35))
		{
			ZoomOut();
		}
		if (player.GetButtonDown(34))
		{
			ZoomIn();
		}
		if ((Object)(object)currentCell != (Object)null && player.GetButtonDown(29))
		{
			if (currentCell.CanBePurchased())
			{
				ShowConfirmPurchase();
			}
			else
			{
				EquipSelectedItem();
			}
		}
	}

	private void AdjustZoom(float amount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		zoomInButton.color = Palette.White;
		zoomOutButton.color = Palette.White;
		zoomLevel += amount;
		if (zoomLevel <= minZoom)
		{
			zoomLevel = minZoom;
			zoomOutButton.color = Palette.DisabledGrey;
		}
		if (zoomLevel >= maxZoom)
		{
			zoomLevel = maxZoom;
			zoomInButton.color = Palette.DisabledGrey;
		}
		scroller.DragScrollSpeed = 0.3f / zoomLevel;
		((Component)scroller).transform.localScale = Vector3.one * zoomLevel;
		bool active = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && zoomLevel > minZoom;
		zoomOutGlyph.SetActive(active);
		bool active2 = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && zoomLevel < maxZoom;
		zoomInGlyph.SetActive(active2);
	}

	private void OnDestroy()
	{
		Object.op_Implicit((Object)(object)currentCube);
	}
}
