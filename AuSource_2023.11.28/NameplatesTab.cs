using System;
using System.Collections;
using AmongUs.Data;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.Events;

public class NameplatesTab : InventoryTab
{
	public PlayerVoteArea previewArea;

	private string plateId;

	private bool currentNameplateIsEquipped;

	private void Awake()
	{
		NumRowsVisible = 4;
	}

	public override void OnEnable()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected Obj, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected Obj, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected Obj, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected Obj, but got Unknown
		base.OnEnable();
		((Component)PlayerPreview).gameObject.SetActive(false);
		((MonoBehaviour)this).StartCoroutine(CoLoadNameplatePreview());
		NamePlateData[] unlockedNamePlates = DestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates();
		for (int i = 0; i < unlockedNamePlates.Length; i++)
		{
			NamePlateData plate = unlockedNamePlates[i];
			float num = XRange.Lerp((float)(i % NumPerRow) / ((float)NumPerRow - 1f));
			float num2 = YStart - (float)(i / NumPerRow) * YOffset;
			ColorChip chip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)chip).transform.localPosition = new Vector3(num, num2, -1f);
			chip.Button.ClickMask = scroller.Hitbox;
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				chip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectNameplate(plate);
				}));
				chip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectNameplate(DestroyableSingleton<HatManager>.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate));
				}));
				((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					ClickEquip();
				}));
			}
			else
			{
				((UnityEvent)chip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					SelectNameplate(plate);
				}));
			}
			((MonoBehaviour)this).StartCoroutine(AddressableAssetExtensions.CoLoadAssetAsync<NamePlateViewData>((MonoBehaviour)(object)this, (IAddressableAssetProvider<NamePlateViewData>)plate, (Action<NamePlateViewData>)((NamePlateViewData viewData) =>
			{
				(chip as NameplateChip).image.sprite = viewData?.Image;
			})));
			(chip as NameplateChip).ProductId = plate.ProdId;
			ColorChips.Add(chip);
		}
		if (unlockedNamePlates.Length != 0)
		{
			GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
		}
		plateId = DataManager.Player.Customization.NamePlate;
		currentNameplateIsEquipped = true;
		SetScrollerBounds();
	}

	private IEnumerator CoLoadNameplatePreview()
	{
		yield return (object)new WaitForEndOfFrame();
		previewArea.PreviewNameplate(DataManager.Player.Customization.NamePlate);
	}

	public void Update()
	{
		NamePlateData namePlateById = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate);
		NamePlateData namePlateById2 = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(plateId);
		currentNameplateIsEquipped = DataManager.Player.Customization.NamePlate == plateId;
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive(namePlateById.ProdId == (colorChip as NameplateChip).ProductId);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(namePlateById2.ProdId == (colorChip as NameplateChip).ProductId);
		}
	}

	public override void ClickEquip()
	{
		base.ClickEquip();
		DataManager.Player.Customization.NamePlate = plateId;
		if (HasLocalPlayer())
		{
			PlayerControl.LocalPlayer.RpcSetNamePlate(plateId);
		}
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Equipped nameplate " + plateId);
	}

	private void SelectNameplate(NamePlateData plate)
	{
		plateId = plate.ProdId;
		currentNameplateIsEquipped = DataManager.Player.Customization.NamePlate == plate.ProdId;
		previewArea.PreviewNameplate(plateId);
		PlayerCustomizationMenu.Instance.SetItemName(plate.GetItemName());
	}

	public override ColorChip GetDefaultSelectable()
	{
		if (ColorChips.Count == 0)
		{
			return null;
		}
		NamePlateData namePlateById = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(DataManager.Player.Customization.NamePlate);
		PlayerCustomizationMenu.Instance.SetItemName(namePlateById.GetItemName());
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			if (namePlateById.ProdId == (colorChip as NameplateChip).ProductId)
			{
				return colorChip;
			}
		}
		return ColorChips[0];
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentNameplateIsEquipped;
	}
}
