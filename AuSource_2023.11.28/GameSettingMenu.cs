using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Rewired;
using TMPro;
using UnityEngine;

public class GameSettingMenu : MonoBehaviour
{
	public static GameSettingMenu Instance;

	public RolesSettingsMenu RolesSettings;

	public GameObject RegularGameSettings;

	public GameObject HideNSeekSettings;

	public GameObject Tabs;

	public TextMeshPro RoleName;

	public TextMeshPro RoleBlurb;

	public SpriteRenderer RoleIcon;

	public SpriteRenderer GameSettingsHightlight;

	public SpriteRenderer RolesSettingsHightlight;

	public Transform[] AllItems;

	public Transform[] AllHideAndSeekItems;

	public Scroller HideAndSeekScroller;

	public Scroller Scroller;

	public float YStart;

	public float YOffset;

	public Transform[] HideForOnline;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement AdvancedSettingsBackButton;

	public List<UiElement> DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public List<UiElement> ControllerSelectableHidenSeek;

	public void Close()
	{
		Instance = null;
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void ToggleRoles()
	{
		bool activeSelf = RegularGameSettings.activeSelf;
		RegularGameSettings.SetActive(!activeSelf);
		((Component)RolesSettings).gameObject.SetActive(activeSelf);
		((Renderer)GameSettingsHightlight).enabled = !activeSelf;
		((Renderer)RolesSettingsHightlight).enabled = activeSelf;
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)Instance) && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
		DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(false);
		ShowSettingsByGameType();
	}

	private void ShowSettingsByGameType()
	{
		if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek)
		{
			InitializeHideAndSeekOptions();
			HideNSeekSettings.SetActive(true);
			((Component)RolesSettings).gameObject.SetActive(false);
			RegularGameSettings.SetActive(false);
			Tabs.SetActive(false);
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected[1], ControllerSelectableHidenSeek);
		}
		else
		{
			InitializeNormalOptions();
			((Component)RolesSettings).gameObject.SetActive(false);
			RegularGameSettings.SetActive(true);
			HideNSeekSettings.SetActive(false);
			Tabs.SetActive(true);
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected[0], ControllerSelectable);
		}
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal && (player.GetButtonDown(35) || player.GetButtonDown(34)))
		{
			ToggleRoles();
		}
	}

	private void OnEnable()
	{
		ShowSettingsByGameType();
	}

	private void OnDisable()
	{
		DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(true);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void InitializeHideAndSeekOptions()
	{
		InitializeOptions(AllHideAndSeekItems, HideAndSeekScroller);
	}

	private void InitializeNormalOptions()
	{
		InitializeOptions(AllItems, Scroller);
	}

	private void InitializeOptions(Transform[] items, Scroller scroller)
	{
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			Transform val = items[i];
			if (!((Component)val).gameObject.activeSelf)
			{
				continue;
			}
			if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && HideForOnline.IndexOf(val) != -1)
			{
				((Component)val).gameObject.SetActive(false);
				continue;
			}
			if (((Object)val).name.Equals("MapName", StringComparison.OrdinalIgnoreCase))
			{
				int num2 = 0;
				List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
				for (int j = 0; j < Constants.MapNames.Length; j++)
				{
					if (j != 3 && j < AmongUsClient.Instance.ShipPrefabs.Count)
					{
						int num3 = ((j == 0 && Constants.ShouldFlipSkeld()) ? 3 : j);
						string key = Constants.MapNames[num3];
						list.Add(new KeyValuePair<string, int>(key, j));
						num2++;
					}
				}
				((Component)val).GetComponent<KeyValueOption>().Values = list;
				if (num2 == 1)
				{
					((Component)val).gameObject.SetActive(false);
					continue;
				}
			}
			UiElement[] array = null;
			UiElement[] array2 = null;
			if (i - 1 >= 0)
			{
				array = ((Component)items[i - 1]).GetComponentsInChildren<UiElement>(true);
			}
			if (i + 1 < items.Length)
			{
				array2 = ((Component)items[i + 1]).GetComponentsInChildren<UiElement>(true);
			}
			UiElement[] componentsInChildren = ((Component)val).GetComponentsInChildren<UiElement>(true);
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				UiElement uiElement = componentsInChildren[k];
				uiElement.ControllerNav.mode = ControllerNavigation.Mode.Explicit;
				if (array != null && array.Length != 0)
				{
					if (k < array.Length)
					{
						uiElement.ControllerNav.selectOnUp = array[k];
					}
					else
					{
						uiElement.ControllerNav.selectOnUp = array[0];
					}
				}
				else
				{
					uiElement.ControllerNav.selectOnUp = null;
				}
				if (array2 != null && array2.Length != 0)
				{
					if (k < array2.Length)
					{
						uiElement.ControllerNav.selectOnDown = array2[k];
					}
					else
					{
						uiElement.ControllerNav.selectOnDown = array2[0];
					}
				}
				else
				{
					uiElement.ControllerNav.selectOnDown = null;
				}
			}
			Vector3 localPosition = val.localPosition;
			localPosition.y = YStart - (float)num * YOffset;
			val.localPosition = localPosition;
			num++;
		}
		Scroller.SetBoundsMax((float)num * YOffset - 2f * YStart - 0.1f, 0f);
	}

	public void ShowAdvancedRoleOptions(RoleBehaviour r)
	{
		RolesSettings.ShowAdvancedSettings(r);
		ControllerManager.Instance.CurrentUiState.BackButton = AdvancedSettingsBackButton;
	}

	public void CloseAdvancedSettings()
	{
		RolesSettings.CloseAdvancedSettings();
		ControllerManager.Instance.CurrentUiState.BackButton = BackButton;
	}
}
