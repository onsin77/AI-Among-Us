using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using AmongUs.Data;
using Epic.OnlineServices;
using Epic.OnlineServices.KWS;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccountManager : DestroyableSingleton<AccountManager>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__46_0;

		internal void _003CPlatformSignInFail_003Eb__46_0()
		{
			DestroyableSingleton<EOSManager>.Instance.StartTempAccountFlow();
		}
	}

	private static readonly string[] ACCOUNT_TAB_SCENES = new string[3] { "MainMenu", "MMOnline", "MatchMaking" };

	[SerializeField]
	private AccountTab accountTab;

	[SerializeField]
	private GameObject accountWindow;

	[SerializeField]
	private PermissionsRequest enterGuardianEmailWindow;

	[SerializeField]
	private UpdateGuardianEmail updateGuardianEmailWindow;

	[SerializeField]
	private InfoTextBox guardianEmailConfirmWindow;

	[SerializeField]
	private InfoTextBox genericInfoDisplayBox;

	[SerializeField]
	private AgeGateScreen enterDateOfBirthScreen;

	public GameObject waitingText;

	public GameObject postLoadWaiting;

	public GameObject privacyPolicyBg;

	public SignInScreen signInScreen;

	public PrivacyPolicyScreen PrivacyPolicy;

	private IChatModeDisplay chatModeMenuScreen;

	public KWSPermissionStatus freeChatAllowed = (KWSPermissionStatus)1;

	public KWSPermissionStatus customDisplayName = (KWSPermissionStatus)1;

	public KWSPermissionStatus friendsListAllowed = (KWSPermissionStatus)1;

	public KWSPermissionStatus accountLinkingAllowed = (KWSPermissionStatus)1;

	public Action OnLoggedInStatusChange;

	private EOSManager.AccountLoginStatus prevLoggedInStatus;

	[SerializeField]
	private TextMeshPro accountDeletionText;

	private bool isAccountWaitingForDeletion;

	public bool IsAccountWaitingForDeletion => isAccountWaitingForDeletion;

	public bool AccountTabActive => ((Behaviour)accountTab).isActiveAndEnabled;

	public void UpdateKidAccountDisplay()
	{
		if (!Object.op_Implicit((Object)(object)AmongUsClient.Instance) || !AmongUsClient.Instance.AmClient)
		{
			accountTab.UpdateKidAccountCanChangeName();
			if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
			{
				DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.QuickChatOnly;
			}
			if (chatModeMenuScreen != null)
			{
				chatModeMenuScreen.UpdateDisplay();
			}
		}
	}

	public void SetChatModeButtonForUpdates(IChatModeDisplay modeCycle)
	{
		chatModeMenuScreen = modeCycle;
	}

	public void UpdateAccountInfoDisplays()
	{
		accountTab.UpdateNameDisplay();
	}

	public void ShowGuardianEmailSentConfirm()
	{
		((Component)guardianEmailConfirmWindow).gameObject.SetActive(true);
	}

	public void ShowPermissionsRequestForm()
	{
		enterGuardianEmailWindow.Show();
	}

	public void CreateAccount()
	{
		DestroyableSingleton<EOSManager>.Instance.CreateAccountWithPlatformAuth();
	}

	public IEnumerator EditGuardianEmail()
	{
		yield return updateGuardianEmailWindow.Show();
		UpdateGuardianEmailDisplay();
	}

	public void UpdateGuardianEmailDisplay()
	{
		accountTab.UpdateGuardianEmailText();
	}

	public IEnumerator ShowAgeGate()
	{
		yield return enterDateOfBirthScreen.Show();
	}

	public void CheckAndRegenerateName()
	{
		if (!ValidateRandomName())
		{
			RandomizeName();
		}
	}

	public bool CanMinorSetCustomDisplayName()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)customDisplayName == 0)
		{
			return DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn;
		}
		return false;
	}

	public bool HasMinorsGuardianEverUpdatedAnything()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if ((int)freeChatAllowed == 2 && (int)customDisplayName == 2 && (int)friendsListAllowed == 2)
		{
			return (int)accountLinkingAllowed != 2;
		}
		return true;
	}

	public void RandomizeName()
	{
		DataManager.Player.Customization.Name = GetRandomName();
		DataManager.Player.Save();
		accountTab.UpdateNameDisplay();
	}

	public string GetRandomName()
	{
		string name;
		do
		{
			name = ((Component)this).GetComponent<RandomNameGenerator>().GetName();
		}
		while (name.Length > 10 || BlockedWords.ContainsWord(name));
		return name;
	}

	private bool ValidateRandomName()
	{
		return ((Component)this).GetComponent<RandomNameGenerator>().ValidateName(DataManager.Player.Customization.Name);
	}

	public override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		accountWindow.SetActive(false);
		((Component)accountTab).gameObject.SetActive(ACCOUNT_TAB_SCENES.Contains(scene.name));
		if (((Component)accountTab).gameObject.activeSelf)
		{
			UpdateAccountDeletionStatus(isAccountWaitingForDeletion);
		}
		else
		{
			((Component)accountDeletionText).gameObject.SetActive(false);
		}
	}

	public void SignInSuccess(Action callback)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected Obj, but got Unknown
		genericInfoDisplayBox.SetOneButton();
		((TMP_Text)genericInfoDisplayBox.titleTexxt).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Success);
		((TMP_Text)genericInfoDisplayBox.bodyText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SuccessLogIn);
		((TMP_Text)genericInfoDisplayBox.button1Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Close);
		((UnityEventBase)genericInfoDisplayBox.button1.OnClick).RemoveAllListeners();
		((UnityEvent)genericInfoDisplayBox.button1.OnClick).AddListener((UnityAction)(() =>
		{
			callback();
		}));
		((Component)genericInfoDisplayBox).gameObject.SetActive(true);
		if (signInScreen.IsOpen())
		{
			signInScreen.OnClose();
		}
	}

	public void SignInFail(EOSManager.EOS_ERRORS error, Action callback)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected Obj, but got Unknown
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Sign in fail {error}");
		genericInfoDisplayBox.SetOneButton();
		((TMP_Text)genericInfoDisplayBox.titleTexxt).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Failed);
		((TMP_Text)genericInfoDisplayBox.bodyText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorLogIn) + $"\n(Error {error})";
		((TMP_Text)genericInfoDisplayBox.button1Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Close);
		((Component)genericInfoDisplayBox).gameObject.SetActive(true);
		((UnityEventBase)genericInfoDisplayBox.button1.OnClick).RemoveAllListeners();
		((UnityEvent)genericInfoDisplayBox.button1.OnClick).AddListener((UnityAction)(() =>
		{
			callback();
		}));
	}

	public void PlatformSignInFail()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected Obj, but got Unknown
		if (DataManager.Player.Account.LoginStatus != EOSManager.AccountLoginStatus.TempAccount)
		{
			string text = "";
			genericInfoDisplayBox.SetOneButton();
			((TMP_Text)genericInfoDisplayBox.titleTexxt).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SignInIssueTitle, Constants.GetCurrentPlatformName());
			((TMP_Text)genericInfoDisplayBox.bodyText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SignInIssueText, text);
			((TMP_Text)genericInfoDisplayBox.button1Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Continue);
			((UnityEventBase)genericInfoDisplayBox.button1.OnClick).RemoveAllListeners();
			ButtonClickedEvent onClick = genericInfoDisplayBox.button1.OnClick;
			UnityAction val = _003C_003Ec._003C_003E9__46_0;
			if (val == null)
			{
				UnityAction val2 = () =>
				{
					DestroyableSingleton<EOSManager>.Instance.StartTempAccountFlow();
				};
				_003C_003Ec._003C_003E9__46_0 = val2;
				val = val2;
			}
			((UnityEvent)onClick).AddListener(val);
			((Component)genericInfoDisplayBox).gameObject.SetActive(true);
			if (signInScreen.IsOpen())
			{
				signInScreen.OnClose();
			}
		}
		else
		{
			DestroyableSingleton<EOSManager>.Instance.StartTempAccountFlow();
		}
	}

	public void SetDLLErrorMode()
	{
		DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.Offline;
		DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.QuickChatOnly;
		accountTab.SetDLLErrorMode();
	}

	public void UpdateVisuals()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (prevLoggedInStatus != DataManager.Player.Account.LoginStatus)
		{
			OnLoggedInStatusChange?.Invoke();
		}
		prevLoggedInStatus = DataManager.Player.Account.LoginStatus;
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (activeScene.name.Equals("MainMenu"))
			{
				accountTab.TurnAllSectionsOff();
				((MonoBehaviour)this).StartCoroutine(CheckFriendCodeAndUpdateVisuals());
				return;
			}
		}
		accountTab.UpdateVisuals();
	}

	private IEnumerator CheckFriendCodeAndUpdateVisuals()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.CheckFriendCodeOnLogin());
		accountTab.UpdateVisuals();
	}

	public void UpdatePlayerCustomizationVisuals()
	{
		accountTab.UpdatePlayerCosmetics();
	}

	public bool CanPlayOnline()
	{
		if ((Object)(object)DestroyableSingleton<EOSManager>.Instance == (Object)null)
		{
			return false;
		}
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline)
		{
			return false;
		}
		if ((Handle)(object)DestroyableSingleton<EOSManager>.Instance.PlatformInterface == (Handle)null)
		{
			return false;
		}
		return true;
	}

	public void GetDeletionInfo()
	{
		((MonoBehaviour)this).StartCoroutine(GetInfoRequest());
	}

	private IEnumerator GetInfoRequest()
	{
		string accountDeletion = BackendEndpoints.AccountDeletion;
		UnityWebRequest request = UnityWebRequest.Get(accountDeletion);
		request.SetRequestHeader("Content-Type", "application/vnd.api+json");
		request.SetRequestHeader("Accept", "application/vnd.api+json");
		request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		yield return request.SendWebRequest();
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			_ = string.Empty;
			if (request.downloadHandler != null)
			{
				_ = request.downloadHandler.text;
			}
			UpdateAccountDeletionStatus(shouldDelete: false);
		}
		else
		{
			_ = string.Empty;
			if (request.downloadHandler != null)
			{
				_ = request.downloadHandler.text;
				Debug.Log((object)("GET account deletion request output " + request.downloadHandler.text));
			}
			UpdateAccountDeletionStatus(shouldDelete: true);
		}
		Debug.Log((object)$"GET account deletion info {request.responseCode}");
	}

	public void UpdateAccountDeletionStatus(bool shouldDelete)
	{
		isAccountWaitingForDeletion = shouldDelete;
		((Component)accountDeletionText).gameObject.SetActive(IsAccountWaitingForDeletion);
	}

	public void OpenAccountWindow()
	{
		accountWindow.SetActive(true);
		accountTab.Open();
	}

	public void CloseAccountWindow()
	{
		accountTab.Close();
		accountWindow.SetActive(false);
	}

	public AccountManager()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
	}
}
