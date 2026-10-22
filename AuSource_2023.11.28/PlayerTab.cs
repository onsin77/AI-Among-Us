using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTab : InventoryTab
{
	private const int Columns = 4;

	public Transform ColorTabArea;

	private HashSet<int> AvailableColors = new HashSet<int>();

	private bool currentColorIsEquipped;

	private int currentColor;

	public override void OnEnable()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected Obj, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected Obj, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected Obj, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		_ = (float)Palette.PlayerColors.Length / 4f;
		for (int i = 0; i < Palette.PlayerColors.Length; i++)
		{
			float num = XRange.Lerp((float)(i % 4) / 3f);
			float num2 = YStart - (float)(i / 4) * YOffset;
			ColorChip colorChip = Object.Instantiate<ColorChip>(ColorTabPrefab);
			((Component)colorChip).transform.SetParent(ColorTabArea);
			((Component)colorChip).transform.localPosition = new Vector3(num, num2, -1f);
			int j = i;
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				colorChip.Button.OnMouseOver.AddListener((UnityAction)(() =>
				{
					SelectColor(j);
				}));
				colorChip.Button.OnMouseOut.AddListener((UnityAction)(() =>
				{
					SelectColor(DataManager.Player.Customization.Color);
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
					SelectColor(j);
				}));
			}
			colorChip.Inner.SpriteColor = Color32.op_Implicit(Palette.PlayerColors[i]);
			colorChip.Tag = j;
			ColorChips.Add(colorChip);
		}
		currentColor = DataManager.Player.Customization.Color;
	}

	public void Update()
	{
		UpdateAvailableColors();
		for (int i = 0; i < ColorChips.Count; i++)
		{
			ColorChip colorChip = ColorChips[i];
			colorChip.InUseForeground.SetActive(!AvailableColors.Contains(i));
			((Renderer)colorChip.SelectionHighlight).enabled = currentColor == i;
			colorChip.PlayerEquippedForeground.SetActive(GetCurrentColorId() == i);
		}
		currentColorIsEquipped = DataManager.Player.Customization.Color == currentColor;
	}

	private void SelectColor(int colorId)
	{
		UpdateAvailableColors();
		string colorName = Palette.GetColorName(colorId);
		PlayerCustomizationMenu.Instance.SetItemName(colorName);
		currentColor = colorId;
		PlayerPreview.SetBodyColor(colorId);
		PlayerPreview.SetPetColor(colorId);
		PlayerPreview.SetSkin(DataManager.Player.Customization.Skin, colorId);
		PlayerPreview.SetHat(DataManager.Player.Customization.Hat, colorId);
		PlayerPreview.SetVisor(DataManager.Player.Customization.Visor, colorId);
	}

	public override ColorChip GetDefaultSelectable()
	{
		int currentColorId = GetCurrentColorId();
		PlayerCustomizationMenu.Instance.SetItemName(Palette.GetColorName(currentColorId));
		return ColorChips[currentColorId];
	}

	public override void ClickEquip()
	{
		if (AvailableColors.Remove(currentColor))
		{
			DataManager.Player.Customization.Color = (byte)currentColor;
			if (HasLocalPlayer())
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)currentColor);
			}
			string colorName = Palette.GetColorName(currentColor);
			Logger.GlobalInstance.Info("PlayerCustomizationMenu::Set player color " + colorName);
		}
	}

	public override bool IsSelectedItemEquipped()
	{
		return currentColorIsEquipped;
	}

	private void UpdateAvailableColors()
	{
		for (int i = 0; i < Palette.PlayerColors.Length; i++)
		{
			AvailableColors.Add(i);
		}
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
			for (int j = 0; j < allPlayers.Count; j++)
			{
				GameData.PlayerInfo playerInfo = allPlayers[j];
				AvailableColors.Remove(playerInfo.DefaultOutfit.ColorId);
			}
		}
	}

	private int GetCurrentColorId()
	{
		if (HasLocalPlayer())
		{
			return PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId;
		}
		return DataManager.Player.Customization.Color;
	}
}
