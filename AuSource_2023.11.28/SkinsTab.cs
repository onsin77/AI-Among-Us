using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;

public class SkinsTab : InventoryTab
{
	private string skinId;

	private bool currentSkinIsEquipped;

	public override void OnEnable()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected Obj, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected Obj, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected Obj, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected Obj, but got Unknown
		base.OnEnable();
		SkinData[] unlockedSkins = DestroyableSingleton<HatManager>.Instance.GetUnlockedSkins();
		for (int i = 0; i < unlockedSkins.Length; i++)
		{
			SkinData skin = unlockedSkins[i];
			float num = XRange.Lerp((float)(i % NumPerRow) / ((float)NumPerRow - 1f));
			float num2 = YStart - (float)(i / NumPerRow) * YOffset;
			ColorChip colorChip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)colorChip).transform.localPosition = new Vector3(num, num2, -1f);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				colorChip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectSkin(skin);
				}));
				colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectSkin(DestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin));
				}));
				((UnityEvent)colorChip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					ClickEquip();
				}));
			}
			else
			{
				((UnityEvent)colorChip.Button.OnClick).AddListener((UnityAction)(() =>
				{
					SelectSkin(skin);
				}));
			}
			colorChip.Button.ClickMask = scroller.Hitbox;
			colorChip.ProductId = skin.ProductId;
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(false);
			UpdateMaterials(colorChip.Inner.FrontLayer, skin);
			skin.SetPreview(colorChip.Inner.FrontLayer, HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
			ColorChips.Add(colorChip);
		}
		skinId = DataManager.Player.Customization.Skin;
		currentSkinIsEquipped = true;
		SetScrollerBounds();
	}

	public void Update()
	{
		PlayerPreview.SetBodyColor(GetDisplayColor());
		SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin);
		SkinData skinById2 = DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId);
		currentSkinIsEquipped = DataManager.Player.Customization.Skin == skinId;
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive(skinById.ProductId == colorChip.ProductId);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(skinById2.ProductId == colorChip.ProductId);
		}
	}

	public override void ClickEquip()
	{
		base.ClickEquip();
		DataManager.Player.Customization.Skin = skinId;
		if (HasLocalPlayer())
		{
			PlayerControl.LocalPlayer.RpcSetSkin(skinId);
		}
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Equipped skin " + skinId);
	}

	private void SelectSkin(SkinData skin)
	{
		skinId = skin.ProdId;
		PlayerPreview.SetSkin(skinId, GetDisplayColor());
		PlayerCustomizationMenu.Instance.SetItemName(skin.GetItemName());
	}

	public override ColorChip GetDefaultSelectable()
	{
		SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin);
		PlayerCustomizationMenu.Instance.SetItemName(skinById.GetItemName());
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			if (skinById.ProductId == colorChip.ProductId)
			{
				return colorChip;
			}
		}
		return ColorChips[0];
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentSkinIsEquipped;
	}
}
