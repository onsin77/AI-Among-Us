using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;

public class VisorsTab : InventoryTab
{
	private string visorId;

	private bool currentVisorIsEquipped;

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
		VisorData[] unlockedVisors = DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
		for (int i = 0; i < unlockedVisors.Length; i++)
		{
			VisorData visor = unlockedVisors[i];
			float num = XRange.Lerp((float)(i % NumPerRow) / ((float)NumPerRow - 1f));
			float num2 = YStart - (float)(i / NumPerRow) * YOffset;
			ColorChip colorChip = Object.Instantiate<ColorChip>(ColorTabPrefab, scroller.Inner);
			((Component)colorChip).transform.localPosition = new Vector3(num, num2, -1f);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				colorChip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectVisor(visor);
				}));
				colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
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
					SelectVisor(visor);
				}));
			}
			colorChip.Button.ClickMask = scroller.Hitbox;
			colorChip.ProductId = visor.ProductId;
			UpdateMaterials(colorChip.Inner.FrontLayer, visor);
			visor.SetPreview(colorChip.Inner.FrontLayer, HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
			colorChip.Tag = visor.ProdId;
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(false);
			ColorChips.Add(colorChip);
		}
		if (unlockedVisors.Length != 0)
		{
			GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
		}
		visorId = DataManager.Player.Customization.Visor;
		currentVisorIsEquipped = true;
		SetScrollerBounds();
	}

	public void Update()
	{
		VisorData visorById = DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor);
		VisorData visorById2 = DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId);
		currentVisorIsEquipped = DataManager.Player.Customization.Visor == visorId;
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.PlayerEquippedForeground.SetActive(visorById.ProductId == colorChip.ProductId);
			((Component)colorChip.SelectionHighlight).gameObject.SetActive(visorById2.ProductId == colorChip.ProductId);
		}
	}

	public override void ClickEquip()
	{
		base.ClickEquip();
		DataManager.Player.Customization.Visor = visorId;
		if (HasLocalPlayer())
		{
			PlayerControl.LocalPlayer.RpcSetVisor(visorId);
		}
		if (DestroyableSingleton<HatManager>.Instance.GetHatById(HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.HatId : DataManager.Player.Customization.Hat).BlocksVisors)
		{
			DataManager.Player.Customization.Hat = "";
			if (HasLocalPlayer())
			{
				PlayerControl.LocalPlayer.RpcSetHat("");
			}
		}
		Logger.GlobalInstance.Info("PlayerCustomizationMenu::Equipped visor " + visorId);
	}

	public override ColorChip GetDefaultSelectable()
	{
		if (ColorChips.Count == 0)
		{
			return null;
		}
		VisorData visorById = DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor);
		PlayerCustomizationMenu.Instance.SetItemName(visorById.GetItemName());
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			if (visorById.ProductId == colorChip.ProductId)
			{
				return colorChip;
			}
		}
		return ColorChips[0];
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentVisorIsEquipped;
	}

	private void SelectVisor(VisorData visor)
	{
		int color = (HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
		visorId = visor.ProdId;
		PlayerPreview.SetVisor(visor.ProdId, color);
		PlayerCustomizationMenu.Instance.SetItemName(visor.GetItemName());
		HatData hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.HatId : DataManager.Player.Customization.Hat);
		if (hatById.BlocksVisors && !visor.IsEmpty)
		{
			PlayerPreview.SetHat("", color);
		}
		else
		{
			PlayerPreview.SetHat(hatById.ProdId, color);
		}
	}
}
