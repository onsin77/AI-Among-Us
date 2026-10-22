using System;
using System.Collections.Generic;
using AmongUs.Data;
using Rewired;
using TMPro;
using UnityEngine;

public class PlayerCustomizationMenu : MonoBehaviour
{
	public static PlayerCustomizationMenu Instance;

	public GameObject cosmicubeMenuTab;

	public CubesTab cubesTab;

	public TabButton[] Tabs;

	private int selectedTab;

	private bool cubeTabSelected;

	public PoolablePlayer PreviewArea;

	public GameObject equipButton;

	public GameObject equippedText;

	public GameObject nameplateMaskArea;

	public GameObject cubeArea;

	public TextMeshPro itemName;

	public Action OnClose;

	public Action OnOpen;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public GameObject glyphL;

	public GameObject glyphR;

	private readonly Logger logger = new Logger("PlayerCustomizationMenu", Logger.Level.Info, Logger.Category.UI);

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		DataManager.Player.Save();
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.PlayerCam.OverrideScreenShakeEnabled = true;
		}
	}

	public void Start()
	{
		if (Object.op_Implicit((Object)(object)Instance) && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
		OpenTab(Tabs[0].Tab);
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.PlayerCam.OverrideScreenShakeEnabled = false;
		}
	}

	public void OpenTab(InventoryTab tab)
	{
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		cosmicubeMenuTab.SetActive(false);
		for (int i = 0; i < Tabs.Length; i++)
		{
			TabButton tabButton = Tabs[i];
			if ((Object)(object)tabButton.Tab == (Object)(object)tab)
			{
				selectedTab = i;
				((Component)tabButton.Tab).gameObject.SetActive(true);
				((Renderer)tabButton.Button).enabled = true;
				if (Object.op_Implicit((Object)(object)tabButton.tabText))
				{
					((TMP_Text)tabButton.tabText).outlineColor = Color32.op_Implicit(Palette.InventorySelectedTextColor);
				}
				List<ColorChip> list = new List<ColorChip>();
				ColorChip colorChip = null;
				((Component)PreviewArea).gameObject.SetActive(false);
				cubeArea.SetActive(false);
				cubeTabSelected = false;
				PlayerTab component = ((Component)tabButton.Tab).GetComponent<PlayerTab>();
				if ((Object)(object)component != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(true);
					list = component.ColorChips;
					colorChip = component.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				HatsTab component2 = ((Component)tabButton.Tab).GetComponent<HatsTab>();
				if ((Object)(object)component2 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(true);
					list = component2.ColorChips;
					colorChip = component2.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				PetsTab component3 = ((Component)tabButton.Tab).GetComponent<PetsTab>();
				if ((Object)(object)component3 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(true);
					list = component3.ColorChips;
					colorChip = component3.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				SkinsTab component4 = ((Component)tabButton.Tab).GetComponent<SkinsTab>();
				if ((Object)(object)component4 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(true);
					list = component4.ColorChips;
					colorChip = component4.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				VisorsTab component5 = ((Component)tabButton.Tab).GetComponent<VisorsTab>();
				if ((Object)(object)component5 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(true);
					list = component5.ColorChips;
					colorChip = component5.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				CubesTab component6 = ((Component)tabButton.Tab).GetComponent<CubesTab>();
				if ((Object)(object)component6 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(false);
					list = component6.ColorChips;
					colorChip = component6.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					cubeTabSelected = true;
					equipButton.SetActive(false);
					cubeArea.SetActive(true);
					nameplateMaskArea.gameObject.SetActive(false);
				}
				NameplatesTab component7 = ((Component)tabButton.Tab).GetComponent<NameplatesTab>();
				if ((Object)(object)component7 != (Object)null)
				{
					((Component)PreviewArea).gameObject.SetActive(false);
					list = component7.ColorChips;
					colorChip = component7.GetDefaultSelectable();
					HandleSelection(list, colorChip);
					nameplateMaskArea.gameObject.SetActive(true);
				}
				((Component)itemName).gameObject.SetActive(!cubeTabSelected);
				logger.Info("Opened tab " + ((Object)tabButton.Tab).name);
			}
			else
			{
				((Component)tabButton.Tab).gameObject.SetActive(false);
				((Renderer)tabButton.Button).enabled = false;
				if (Object.op_Implicit((Object)(object)tabButton.tabText))
				{
					((TMP_Text)tabButton.tabText).outlineColor = Color32.op_Implicit(Palette.InventoryTextColor);
				}
			}
		}
		bool flag = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
		glyphL.SetActive(flag && selectedTab != 0);
		glyphR.SetActive(flag && selectedTab < Tabs.Length - 1);
		ControllerManager.Instance.ClearDestroyedSelectableUiElements();
		PreviewArea.UpdateFromDataManager(PlayerMaterial.MaskType.None);
		PreviewArea.ToggleName(active: false);
	}

	private void HandleSelection(List<ColorChip> buttons, ColorChip selectedChip)
	{
		foreach (ColorChip button in buttons)
		{
			ControllerManager.Instance.AddSelectableUiElement(button.Button);
		}
		if (Object.op_Implicit((Object)(object)selectedChip))
		{
			ControllerManager.Instance.SetCurrentSelected(selectedChip.Button);
			selectedChip.Button.ReceiveMouseOver();
		}
		else if (buttons.Count > 0)
		{
			ControllerManager.Instance.SetCurrentSelected(buttons[0].Button);
		}
		else
		{
			ControllerManager.Instance.PickTopSelectable();
		}
	}

	public void Open()
	{
		DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, ((Component)this).gameObject, () =>
		{
			((Component)this).gameObject.SetActive(true);
			OnOpen?.Invoke();
		});
	}

	public void Close(bool canMove)
	{
		OnClose?.Invoke();
		logger.Info("Closing");
		DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(((Component)this).gameObject, null, DestroyObj);
	}

	private void DestroyObj()
	{
		OnClose?.Invoke();
		OnClose = null;
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void Update()
	{
		if (!(ControllerManager.Instance.CurrentUiState.MenuName != ((Object)this).name))
		{
			Player player = ReInput.players.GetPlayer(0);
			if (selectedTab > 0 && player.GetButtonDown(35))
			{
				selectedTab--;
				OpenTab(Tabs[selectedTab].Tab);
			}
			if (selectedTab < Tabs.Length - 1 && player.GetButtonDown(34))
			{
				selectedTab++;
				OpenTab(Tabs[selectedTab].Tab);
			}
			bool flag = !Tabs[selectedTab].Tab.IsSelectedItemEquipped();
			equipButton.SetActive(flag && !cubeTabSelected);
			equippedText.SetActive(!flag && !cubeTabSelected);
			if ((flag || cubeTabSelected) && player.GetButtonDown(11))
			{
				ClickEquip();
			}
			if (player.GetButtonDown(29) && cubeTabSelected)
			{
				ViewCube();
			}
		}
	}

	public void ViewCube()
	{
		cosmicubeMenuTab.gameObject.SetActive(true);
		cubesTab.ViewCube();
	}

	public void SetItemName(string text)
	{
		((TMP_Text)itemName).text = text;
	}

	public void ClickEquip()
	{
		equipButton.SetActive(false);
		((Component)Tabs[selectedTab].Tab).GetComponent<InventoryTab>().ClickEquip();
	}
}
