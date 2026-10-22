using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class MainMenuManager : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<StoreTabButton, bool> _003C_003E9__37_0;

		public static Action _003C_003E9__38_0;

		public static UnityAction _003C_003E9__47_0;

		internal bool _003CCheckNewItems_003Eb__37_0(StoreTabButton t)
		{
			if (DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions.ContainsKey(t.storeTab))
			{
				return DataManager.Player.Store.GetStoreTabViewDate(t.storeTab) < Constants.ParseDate(DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[t.storeTab]);
			}
			return false;
		}

		internal void _003CRunStartUp_003Eb__38_0()
		{
			DestroyableSingleton<EOSManager>.Instance.announcementsVisible = false;
		}

		internal void _003CConnectMainMenuScreenButtonEvents_003Eb__47_0()
		{
			DestroyableSingleton<AccountManager>.Instance.OpenAccountWindow();
		}
	}

	public AdDataCollectScreen AdsPolicy;

	public PassiveButton PlayOnlineButton;

	public HatManager HatManagerRef;

	public CosmicubeManager CosmicubeManagerRef;

	public PlayerCustomizationMenu playerCustomizationPrefab;

	[SerializeField]
	private AnnouncementPopUp announcementPopUp;

	[SerializeField]
	private GameObject gameModeButtons;

	[SerializeField]
	private GameObject accountButtons;

	[SerializeField]
	private SpriteMask screenMask;

	[SerializeField]
	private GameObject mainMenuUI;

	[SerializeField]
	private GameObject rightPanelMask;

	[SerializeField]
	private SpriteRenderer screenTint;

	[SerializeField]
	private PassiveButton freePlayButton;

	[Header("Left panel buttons")]
	[SerializeField]
	private PassiveButton playButton;

	[SerializeField]
	private PassiveButton inventoryButton;

	[SerializeField]
	private PassiveButton shopButton;

	[SerializeField]
	private PassiveButton myAccountButton;

	[SerializeField]
	private PassiveButton newsButton;

	[SerializeField]
	private PassiveButton settingsButton;

	[SerializeField]
	private PassiveButton creditsButton;

	[SerializeField]
	private PassiveButton quitButton;

	[Header("Default right panel buttons")]
	[SerializeField]
	private PassiveButton playLocalButton;

	[SerializeField]
	private PassiveButton howToPlayButton;

	[SerializeField]
	private PassiveButton accountCTAButton;

	private List<PassiveButton> mainButtons;

	[SerializeField]
	private GameObject creditsScreen;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public List<PassiveButton> disableOnStartup = new List<PassiveButton>();

	[SerializeField]
	private GameObject NewStoreItemsIcon;

	private CosmicubeManager cosmicubeManager;

	private void Awake()
	{
		mainButtons = new List<PassiveButton> { playButton, inventoryButton, shopButton, newsButton, myAccountButton, settingsButton };
		SetUpControllerNav();
		ControllerManager.Instance.NewScene(((Object)this).name, null, DefaultButtonSelected, ControllerSelectable);
		CheckNewItems();
		GameOptionsManager.Instance.Initialize();
		rightPanelMask.gameObject.SetActive(false);
		ResetScreen();
		ConnectMainMenuScreenButtonEvents();
	}

	public void Start()
	{
		ChatLanguageSet.Instance.Load();
		((MonoBehaviour)this).StartCoroutine(RunStartUp());
		QualitySettings.vSyncCount = (DataManager.Settings.Video.VSync ? 1 : 0);
		announcementPopUp.ActionOnDisable += () =>
		{
			((Renderer)screenMask).enabled = true;
		};
		announcementPopUp.ActionOnEnable += () =>
		{
			((Renderer)screenMask).enabled = false;
		};
		announcementPopUp.SetMainMenuManager(this);
	}

	public void ResetScreen()
	{
		if (DestroyableSingleton<AccountManager>.InstanceExists)
		{
			DestroyableSingleton<AccountManager>.Instance.CloseAccountWindow();
		}
		for (int i = 0; i < mainButtons.Count; i++)
		{
			mainButtons[i].ControllerNav.selectOnRight = null;
		}
		creditsScreen.SetActive(false);
		gameModeButtons.SetActive(false);
		accountButtons.SetActive(false);
		((Renderer)screenTint).enabled = false;
	}

	public void OpenGameModeMenu()
	{
		ResetScreen();
		gameModeButtons.SetActive(true);
		((Renderer)screenTint).enabled = true;
		for (int i = 0; i < mainButtons.Count; i++)
		{
			if (((Component)playLocalButton).gameObject.activeSelf)
			{
				mainButtons[i].ControllerNav.selectOnRight = playLocalButton;
			}
			else if (((Behaviour)PlayOnlineButton).enabled)
			{
				mainButtons[i].ControllerNav.selectOnRight = PlayOnlineButton;
			}
			else
			{
				mainButtons[i].ControllerNav.selectOnRight = howToPlayButton;
			}
		}
	}

	public void OpenAccountMenu()
	{
		ResetScreen();
		accountButtons.SetActive(true);
		((Renderer)screenTint).enabled = true;
		for (int i = 0; i < mainButtons.Count; i++)
		{
			mainButtons[i].ControllerNav.selectOnRight = accountCTAButton;
		}
	}

	public void OpenCredits()
	{
		ResetScreen();
		creditsScreen.SetActive(true);
		((Renderer)screenTint).enabled = true;
	}

	public void CheckNewItems()
	{
		NewStoreItemsIcon.SetActive(DestroyableSingleton<StoreMenu>.Instance.Tabs.Any((StoreTabButton t) => DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions.ContainsKey(t.storeTab) && DataManager.Player.Store.GetStoreTabViewDate(t.storeTab) < Constants.ParseDate(DestroyableSingleton<ReferenceDataManager>.Instance.StoreTabVersions[t.storeTab])));
	}

	private IEnumerator RunStartUp()
	{
		Logger.GlobalInstance.Info("MainMenuManager.RunStartUp beginning");
		Constants.SetMainThread();
		DestroyableSingleton<EOSManager>.Instance.FinishedAssets = true;
		Object.Instantiate<HatManager>(HatManagerRef).Initialize();
		cosmicubeManager = Object.Instantiate<CosmicubeManager>(CosmicubeManagerRef);
		cosmicubeManager.Initialize();
		yield return DestroyableSingleton<EOSManager>.Instance.RunLogin();
		rightPanelMask.gameObject.SetActive(true);
		if (!DestroyableSingleton<StoreManager>.InstanceExists || !DestroyableSingleton<StoreManager>.Instance.FinishedInitializationFlow)
		{
			DestroyableSingleton<StoreManager>.Instance.Initialize();
		}
		if (!DestroyableSingleton<StoreMenu>.InstanceExists || !DestroyableSingleton<StoreMenu>.Instance.Initialized)
		{
			DestroyableSingleton<StoreMenu>.Instance.Initialize();
		}
		while (!DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow() || DestroyableSingleton<AccountManager>.Instance.signInScreen.IsOpen())
		{
			yield return (object)new WaitForSeconds(0.3f);
		}
		DestroyableSingleton<EOSManager>.Instance.announcementsVisible = true;
		if (DataManager.Player.Account.LoginStatus != EOSManager.AccountLoginStatus.Offline)
		{
			((MonoBehaviour)this).StartCoroutine(announcementPopUp.ShowIfNew(() =>
			{
				DestroyableSingleton<EOSManager>.Instance.announcementsVisible = false;
			}));
		}
		CheckAddOns();
		DestroyableSingleton<StoreMenu>.Instance.OnSaveDataChanged();
		if (!DataManager.Settings.DoesFileExist())
		{
			DataManager.Settings.ForceSave();
		}
		Logger.GlobalInstance.Info("MainMenuManager.RunStartUp finished");
		yield return null;
	}

	private void CheckAddOns()
	{
	}

	private void LateUpdate()
	{
		if (!ControllerManager.Instance.IsUiControllerActive && DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
		{
			Debug.Log((object)"No current valid menu active, opening main menu scene");
			ControllerManager.Instance.NewScene(((Object)this).name, null, DefaultButtonSelected, ControllerSelectable);
			ControllerManager.Instance.CurrentUiState.CurrentSelection = DefaultButtonSelected;
		}
	}

	public void TransitionToInventory()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		ResetScreen();
		PlayerCustomizationMenu playerCustomizationMenu = Object.Instantiate<PlayerCustomizationMenu>(playerCustomizationPrefab);
		playerCustomizationMenu.Open();
		playerCustomizationMenu.OnClose = (Action)Delegate.Combine(playerCustomizationMenu.OnClose, new Action(ActivateMainMenuUI));
		playerCustomizationMenu.OnOpen = (Action)Delegate.Combine(playerCustomizationMenu.OnOpen, new Action(DeactivateMainMenuUI));
		((Component)playerCustomizationMenu).transform.SetParent(((Component)Camera.main).transform, false);
		((Component)playerCustomizationMenu).transform.localPosition = new Vector3(0f, 0f, -20f);
	}

	public void TransitionToShop()
	{
		ResetScreen();
		StoreMenu instance = DestroyableSingleton<StoreMenu>.Instance;
		instance.OnClose = (Action)Delegate.Combine(instance.OnClose, new Action(ActivateMainMenuUI));
		instance.OnOpen = (Action)Delegate.Combine(instance.OnOpen, new Action(DeactivateMainMenuUI));
		DestroyableSingleton<StoreMenu>.Instance.Open();
	}

	public bool IsMainMenuActive()
	{
		return mainMenuUI.activeSelf;
	}

	private void ActivateMainMenuUI()
	{
		mainMenuUI.SetActive(true);
	}

	private void DeactivateMainMenuUI()
	{
		mainMenuUI.SetActive(false);
		if (((Component)announcementPopUp).gameObject.activeSelf)
		{
			announcementPopUp.Close();
		}
	}

	private void SetUpControllerNav()
	{
		for (int i = 0; i < mainButtons.Count; i++)
		{
			mainButtons[i].ControllerNav.mode = ControllerNavigation.Mode.Explicit;
			if (i < mainButtons.Count - 1)
			{
				mainButtons[i].ControllerNav.selectOnDown = mainButtons[i + 1];
			}
			else
			{
				mainButtons[i].ControllerNav.selectOnDown = creditsButton;
			}
			if (i > 0)
			{
				mainButtons[i].ControllerNav.selectOnUp = mainButtons[i - 1];
			}
		}
		creditsButton.ControllerNav.selectOnRight = quitButton;
		creditsButton.ControllerNav.selectOnUp = settingsButton;
		quitButton.ControllerNav.selectOnUp = settingsButton;
		quitButton.ControllerNav.selectOnLeft = creditsButton;
	}

	private void ConnectMainMenuScreenButtonEvents()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		((UnityEventBase)accountCTAButton.OnClick).RemoveAllListeners();
		ButtonClickedEvent onClick = accountCTAButton.OnClick;
		UnityAction val = _003C_003Ec._003C_003E9__47_0;
		if (val == null)
		{
			UnityAction val2 = () =>
			{
				DestroyableSingleton<AccountManager>.Instance.OpenAccountWindow();
			};
			_003C_003Ec._003C_003E9__47_0 = val2;
			val = val2;
		}
		((UnityEvent)onClick).AddListener(val);
	}

	private void OnDestroy()
	{
	}
}
