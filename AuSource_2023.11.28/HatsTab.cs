using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;

public class HatsTab : InventoryTab
{
	private HatData currentHat;

	private bool currentHatIsEquipped;

	public override void OnEnable()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected Obj, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected Obj, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected Obj, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected Obj, but got Unknown
		base.OnEnable();
		HatData[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
		currentHat = DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);
		for (int i = 0; i < unlockedHats.Length; i++)
		{
			HatData hat = unlockedHats[i];
			float num = XRange.Lerp((float)(i % NumPerRow) / ((float)NumPerRow - 1f));
			float num2 = YStart - (float)(i / NumPerRow) * YOffset;
			ColorChip colorChip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)colorChip).transform.localPosition = new Vector3(num, num2, -1f);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				colorChip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectHat(hat);
				}));
				colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectHat(DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat));
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
					SelectHat(hat);
				}));
			}
			colorChip.Button.ClickMask = scroller.Hitbox;
			colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
			UpdateMaterials(colorChip.Inner.FrontLayer, hat);
			hat.SetPreview(colorChip.Inner.FrontLayer, HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
			colorChip.Tag = hat;
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(false);
			ColorChips.Add(colorChip);
		}
		currentHatIsEquipped = true;
		SetScrollerBounds();
	}

	public void Update()
	{
		PlayerPreview.SetBodyColor(HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
		HatData hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);
		currentHatIsEquipped = DataManager.Player.Customization.Hat == currentHat.ProdId;
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive((Object)(object)hatById == (Object)(object)(HatData)colorChip.Tag);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive((Object)(object)currentHat == (Object)(object)(HatData)colorChip.Tag);
		}
	}

	private void SelectHat(HatData hat)
	{
		currentHat = hat;
		int color = (HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
		PlayerPreview.SetHat(currentHat.ProdId, color);
		PlayerCustomizationMenu.Instance.SetItemName(hat.GetItemName());
		if (hat.BlocksVisors)
		{
			PlayerPreview.SetVisor(string.Empty, 0);
		}
		else
		{
			PlayerPreview.SetVisor(HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.VisorId : DataManager.Player.Customization.Visor, color);
		}
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentHatIsEquipped;
	}

	public override void ClickEquip()
	{
		base.ClickEquip();
		DataManager.Player.Customization.Hat = currentHat.ProdId;
		if (HasLocalPlayer())
		{
			PlayerControl.LocalPlayer.RpcSetHat(currentHat.ProdId);
		}
		if (currentHat.BlocksVisors)
		{
			DataManager.Player.Customization.Visor = "";
			if (HasLocalPlayer())
			{
				PlayerControl.LocalPlayer.RpcSetVisor("");
			}
		}
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Equipped hat " + currentHat.ProdId);
	}

	public override ColorChip GetDefaultSelectable()
	{
		HatData hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);
		PlayerCustomizationMenu.Instance.SetItemName(hatById.GetItemName());
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			if ((Object)(object)hatById == (Object)(object)(HatData)colorChip.Tag)
			{
				return colorChip;
			}
		}
		return ColorChips[0];
	}
}
