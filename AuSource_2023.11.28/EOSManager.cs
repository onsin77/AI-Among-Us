using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.KWS;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using InnerNet;
using Newtonsoft.Json;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class EOSManager : DestroyableSingleton<EOSManager>
{
	public enum AccountLoginStatus
	{
		Offline,
		LoggedIn,
		WaitingForParent,
		TempAccount
	}

	public enum EOS_ERRORS
	{
		FailedEpicAuthToken,
		UnsupportedPlatform,
		LinkAccountFail,
		SteamworksAppTicketFail,
		SteamworksAuthFail,
		iOSAuthFail,
		GoogleAuthFail,
		GoogleAuthNoToken,
		NullContinuanceToken,
		MismatchedProductUserIDs,
		GenericLoginError,
		XboxUserAddError,
		XboxGetTokenError,
		AccountCreationFail,
		InterfaceInitFail,
		InvalidParentEmail,
		GuestModeAuthFail,
		PlatformNotSupported,
		NintendoAuthFailed,
		PlayStationAuthFailed,
		AccountLinkFail,
		NoConnectionError,
		ItchIOAuthFail,
		UnlinkAccountFail,
		FriendCodeFail,
		TempAccountError
	}

	private CallResult<EncryptedAppTicketResponse_t> OnEncryptedAppTicketResponseCallResult;

	private OnLoginCallback successCallback;

	private Action onLoginFailedCallback;

	[SerializeField]
	private string productName;

	[SerializeField]
	private string productVersion;

	[SerializeField]
	private string productId;

	[SerializeField]
	private string sandboxId;

	private string deploymentId = "503cd077a7804777aee5a6eeb5cfe62d";

	[SerializeField]
	private string clientId;

	[SerializeField]
	private string clientSecret;

	[SerializeField]
	private string friendCode;

	private bool hasRunLoginFlow;

	private const float platformTickInterval = 0.1f;

	private float platformTickTimer;

	private bool platformInitialized;

	private bool loginFlowFinished;

	private bool tryingToLogin;

	private bool stopTimeOutCheck;

	public GameObject TimeOutPopup;

	public float TimeOutTime = 30f;

	public bool FinishedAssets;

	private PlatformInterface platformInterface;

	private ProductUserId userId;

	private ProductUserId deviceIDuserID;

	public bool announcementsVisible;

	private bool attemptAuthAgain = true;

	private uint numLinkedAccounts;

	public List<ExternalAccountInfo> linkedExternalAccounts = new List<ExternalAccountInfo>();

	public EditAccountUsername editAccountUsername;

	public AskToMergeGuest askToMergeAccount;

	private string freeChatKey = "freeChat";

	private string customNameKey = "customDisplayName";

	private string friendsListKey = "friendsList";

	private string accountLinkKey = "accountLinking";

	[SerializeField]
	private List<TextMeshProUGUI> watermark;

	[SerializeField]
	private GameObject watermarkCanvas;

	private readonly Logger logger = new Logger("EOSManager", Logger.Level.Warning, Logger.Category.Network);

	private int ageOfConsent;

	private string kwsUserId;

	private bool isKWSMinor;

	private ContinuanceToken continuanceToken;

	public string exchangeToken = "";

	private string platformAuthToken = "";

	private bool authExpiredCallbackTriggered;

	private bool silentLoginFailed;

	public CompletionToken<PurchaseStates> validateOldDLC;

	public bool isRedeemingDLC;

	private PlatformInterface s_eosPlatformInterface;

	public string ProductName => productName;

	public string ProductVersion => productVersion;

	public string ProductId => productId;

	public string SandboxId => sandboxId;

	public string DeploymentId => deploymentId;

	public string ClientId => clientId;

	public string ClientSecret => clientSecret;

	public string FriendCode
	{
		get
		{
			return friendCode;
		}
		set
		{
			friendCode = value;
		}
	}

	public PlatformInterface PlatformInterface => platformInterface;

	public string ProductUserId
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			string result = null;
			ProductUserId val = userId;
			if (val != null)
			{
				val.ToString(ref result);
			}
			return result;
		}
	}

	public string UserIDToken
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			CopyIdTokenOptions val = new CopyIdTokenOptions
			{
				LocalUserId = ProductUserId.FromString(ProductUserId)
			};
			IdToken val2 = default;
			PlatformInterface.GetConnectInterface().CopyIdToken(val, ref val2);
			return ((val2 != null) ? val2.JsonWebToken : null) ?? "";
		}
	}

	public string DeviceIDUserIDToken
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			CopyIdTokenOptions val = new CopyIdTokenOptions
			{
				LocalUserId = ProductUserId.FromString(DeviceIDProductUserId)
			};
			IdToken val2 = default;
			PlatformInterface.GetConnectInterface().CopyIdToken(val, ref val2);
			return ((val2 != null) ? val2.JsonWebToken : null) ?? "";
		}
	}

	public string DeviceIDProductUserId
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			string result = null;
			ProductUserId val = deviceIDuserID;
			if (val != null)
			{
				val.ToString(ref result);
			}
			return result;
		}
	}

	public void StartTempAccountFlow()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		CreateDeviceIdOptions val = new CreateDeviceIdOptions
		{
			DeviceModel = SystemInfo.deviceUniqueIdentifier
		};
		PlatformInterface.GetConnectInterface().CreateDeviceId(val, (object)null, (OnCreateDeviceIdCallback)((CreateDeviceIdCallbackInfo createDeviceIDCallbackInfo) =>
		{
			LoginWithDeviceID();
		}));
	}

	private void LoginWithDeviceID()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_0040: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		LoginOptions val = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = null,
				Type = (ExternalCredentialType)10
			},
			UserLoginInfo = new UserLoginInfo
			{
				DisplayName = DataManager.Player.Customization.Name
			}
		};
		PlatformInterface.GetConnectInterface().Login(val, (object)null, (OnLoginCallback)((LoginCallbackInfo data) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected Obj, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected Obj, but got Unknown
			if ((int)data.ResultCode != 0)
			{
				if ((Handle)(object)data.ContinuanceToken != (Handle)null)
				{
					CreateUserOptions val2 = new CreateUserOptions
					{
						ContinuanceToken = continuanceToken
					};
					PlatformInterface.GetConnectInterface().CreateUser(val2, (object)null, (OnCreateUserCallback)((CreateUserCallbackInfo info) =>
					{
						LoginWithDeviceID();
					}));
				}
				else
				{
					DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.TempAccountError, ContinueInOfflineMode);
				}
			}
			else
			{
				DataManager.Player.Account.LoginStatus = AccountLoginStatus.TempAccount;
				DataManager.Player.Save();
				userId = data.LocalUserId;
				UpdateInventory();
			}
		}));
	}

	private void AuthWithCorrectPlatformImpl()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		logger.Info("Auth with Steam");
		if (!SteamManager.Initialized)
		{
			PlatformAuthReturn();
			return;
		}
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Steam ID {SteamUser.GetSteamID()}");
		OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create((APIDispatchDelegate<EncryptedAppTicketResponse_t>)OnSteamEncryptedAppTicketLoginCallback);
		SteamAPICall_t val = SteamUser.RequestEncryptedAppTicket((byte[])null, 0);
		OnEncryptedAppTicketResponseCallResult.Set(val, (APIDispatchDelegate<EncryptedAppTicketResponse_t>)null);
	}

	private void OnSteamEncryptedAppTicketLoginCallback(EncryptedAppTicketResponse_t pCallback, bool bIOFailure)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		logger.Debug("[" + 154 + " - EncryptedAppTicketResponse] - " + ((object)pCallback.m_eResult/*cast due to constrained. prefix*/).ToString());
		if ((int)pCallback.m_eResult != 1)
		{
			logger.Error($"EOS Failed to get Steamworks auth: {pCallback.m_eResult}");
			PlatformAuthReturn();
			return;
		}
		byte[] array = new byte[1024];
		uint num = default;
		if (SteamUser.GetEncryptedAppTicket(array, 1024, ref num))
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0L, array2, 0L, num);
			platformAuthToken = Common.ToString(array2);
			PlatformAuthReturn();
		}
		else
		{
			logger.Error($"EOS Failed to get Steamworks app ticket: {pCallback.m_eResult}");
			PlatformAuthReturn();
		}
	}

	private void LoginWithCorrectPlatformImpl(OnLoginCallback successCallback, Action onLoginFailed = null)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected Obj, but got Unknown
		//IL_0093: Expected Obj, but got Unknown
		logger.Info("Login with Steam");
		if (string.IsNullOrEmpty(platformAuthToken))
		{
			if (attemptAuthAgain)
			{
				attemptAuthAgain = false;
				RetryAuthAndLoginImpl(successCallback, onLoginFailed);
				return;
			}
			attemptAuthAgain = true;
			logger.Error("EOS Failed to get Steamworks auth: token empty");
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.SteamworksAuthFail, ContinueInOfflineMode);
			onLoginFailed?.Invoke();
		}
		else
		{
			LoginOptions val = new LoginOptions
			{
				Credentials = new Credentials
				{
					Token = platformAuthToken,
					Type = (ExternalCredentialType)1
				}
			};
			PlatformInterface.GetConnectInterface().Login(val, (object)null, successCallback);
			stopTimeOutCheck = true;
		}
	}

	public void LoginSilentlyImpl()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		//IL_0047: Expected Obj, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected Obj, but got Unknown
		stopTimeOutCheck = true;
		if (string.IsNullOrEmpty(platformAuthToken))
		{
			authExpiredCallbackTriggered = false;
			silentLoginFailed = true;
			return;
		}
		LoginOptions val = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = platformAuthToken,
				Type = (ExternalCredentialType)1
			}
		};
		platformInterface.GetConnectInterface().Login(val, (object)null, (OnLoginCallback)EOSConnectPlatformSilentLoginCallback);
	}

	private void RetryAuthAndLoginImpl(OnLoginCallback successCallback, Action onLoginFailed = null)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		logger.Info("Retry with Steam");
		this.successCallback = successCallback;
		onLoginFailedCallback = onLoginFailed;
		if (!SteamManager.Initialized)
		{
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.SteamworksAuthFail, ContinueInOfflineMode);
			return;
		}
		OnEncryptedAppTicketResponseCallResult = CallResult<EncryptedAppTicketResponse_t>.Create((APIDispatchDelegate<EncryptedAppTicketResponse_t>)OnSteamEncryptedAppTicketLoginCallbackRetry);
		SteamAPICall_t val = SteamUser.RequestEncryptedAppTicket((byte[])null, 0);
		OnEncryptedAppTicketResponseCallResult.Set(val, (APIDispatchDelegate<EncryptedAppTicketResponse_t>)null);
	}

	private void OnSteamEncryptedAppTicketLoginCallbackRetry(EncryptedAppTicketResponse_t pCallback, bool bIOFailure)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("[" + 154 + " - EncryptedAppTicketResponse] - " + ((object)pCallback.m_eResult/*cast due to constrained. prefix*/).ToString()));
		if ((int)pCallback.m_eResult != 1)
		{
			Debug.LogError((object)$"EOS Failed to get Steamworks auth: {pCallback.m_eResult}");
			onLoginFailedCallback?.Invoke();
			onLoginFailedCallback = null;
			return;
		}
		byte[] array = new byte[1024];
		uint num = default;
		if (SteamUser.GetEncryptedAppTicket(array, 1024, ref num))
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0L, array2, 0L, num);
			platformAuthToken = Common.ToString(array2);
			LoginWithCorrectPlatformImpl(successCallback, onLoginFailedCallback);
		}
		else
		{
			Debug.LogError((object)$"EOS Failed to get Steamworks app ticket: {pCallback.m_eResult}");
			onLoginFailedCallback?.Invoke();
			onLoginFailedCallback = null;
		}
	}

	public override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += OnSceneLoaded;
		if (DestroyableSingleton<EOSManager>.InstanceExists && !((Object)(object)DestroyableSingleton<EOSManager>.Instance != (Object)(object)this))
		{
			InitializePlatformInterface();
		}
	}

	public void InitializePlatformInterface()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected Obj, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected Obj, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected Obj, but got Unknown
		//IL_0191: Expected Obj, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected Obj, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected Obj, but got Unknown
		InitializeOptions val = new InitializeOptions
		{
			ProductName = productName,
			ProductVersion = productVersion
		};
		Result val2 = (Result)18;
		try
		{
			val2 = PlatformInterface.Initialize(val);
		}
		catch (DllNotFoundException ex)
		{
			logger.Error("DLL Not Found: " + ex.Message);
			GameObject val3 = FindPlayOnlineButton();
			if ((Object)(object)val3 != (Object)null)
			{
				val3.GetComponent<ButtonRolloverHandler>()?.SetDisabledColors();
				((Behaviour)val3.GetComponent<PassiveButton>()).enabled = false;
			}
		}
		bool flag = Application.isEditor && (int)val2 == 15;
		if ((int)val2 != 0 && !flag)
		{
			throw new Exception("Failed to initialize platform: " + ((object)val2/*cast due to constrained. prefix*/).ToString());
		}
		LoggingInterface.SetLogLevel((LogCategory)int.MaxValue, (LogLevel)300);
		LoggingInterface.SetCallback((LogMessageFunc)HandleEosLogging);
		string cacheDirectory = Path.Combine(PlatformPaths.temporaryCachePath, "EOS");
		Options val4 = new Options
		{
			ProductId = productId,
			SandboxId = sandboxId,
			DeploymentId = deploymentId,
			Flags = (PlatformFlags)2,
			ClientCredentials = new ClientCredentials
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			},
			EncryptionKey = "AAAABBBBCCCCDDDDEEEFFFF1111222233334444555566667777888899990000",
			CacheDirectory = cacheDirectory
		};
		platformInterface = PlatformInterface.Create(val4);
		if ((Handle)(object)platformInterface == (Handle)null)
		{
			throw new Exception("Failed to create platform interface");
		}
		PlatformInterface.GetConnectInterface().AddNotifyAuthExpiration(new AddNotifyAuthExpirationOptions(), (object)null, (OnAuthExpirationCallback)OnAuthExpirationCallback);
		AddNotifyPermissionsUpdateReceivedOptions val5 = new AddNotifyPermissionsUpdateReceivedOptions();
		PlatformInterface.GetKWSInterface().AddNotifyPermissionsUpdateReceived(val5, (object)null, (OnPermissionsUpdateReceivedCallback)KWSPermissionsUpdatedCallback);
		platformInitialized = true;
	}

	private void KWSPermissionsUpdatedCallback(PermissionsUpdateReceivedCallbackInfo permissionsCallbackInfo)
	{
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.LoggedIn || DataManager.Player.Account.LoginStatus == AccountLoginStatus.WaitingForParent)
		{
			UpdatePermissionKeys(DestroyableSingleton<AccountManager>.Instance.UpdateVisuals);
		}
	}

	private void OnAuthExpirationCallback(AuthExpirationCallbackInfo data)
	{
		if (!authExpiredCallbackTriggered)
		{
			authExpiredCallbackTriggered = true;
			LoginSilentlyImpl();
		}
	}

	public void ShowTimeout()
	{
		if (!TimeOutPopup.activeSelf)
		{
			TimeOutPopup.SetActive(true);
		}
	}

	public void HideTimeout()
	{
		if (TimeOutPopup.activeSelf)
		{
			TimeOutPopup.SetActive(false);
		}
	}

	public void PlayOffline()
	{
		ContinueInOfflineMode();
	}

	public IEnumerator RunLogin()
	{
		announcementsVisible = false;
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.Offline)
		{
			IsAllowedOnline(canOnline: false);
		}
		if (hasRunLoginFlow)
		{
			DestroyableSingleton<AccountManager>.Instance.privacyPolicyBg.gameObject.SetActive(false);
			DestroyableSingleton<AccountManager>.Instance.waitingText.gameObject.SetActive(false);
			if (DataManager.Player.Account.LoginStatus != AccountLoginStatus.Offline)
			{
				IsAllowedOnline(canOnline: true);
			}
			yield break;
		}
		hasRunLoginFlow = true;
		loginFlowFinished = false;
		yield return DestroyableSingleton<AccountManager>.Instance.PrivacyPolicy.Show();
		DestroyableSingleton<AccountManager>.Instance.privacyPolicyBg.gameObject.SetActive(false);
		if (platformInitialized)
		{
			StartInitialLoginFlow();
		}
		else
		{
			DestroyableSingleton<AccountManager>.Instance.SetDLLErrorMode();
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.InterfaceInitFail, ContinueInOfflineMode);
		}
		while (!HasFinishedLoginFlow())
		{
			yield return null;
		}
	}

	public void StartInitialLoginFlow()
	{
		if (!platformInitialized)
		{
			CloseStartupWaitScreen();
			DestroyableSingleton<AccountManager>.Instance.SetDLLErrorMode();
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.InterfaceInitFail, BeginFinalPartsOfLoginFlow);
		}
		else
		{
			BeginLoginFlowWithDeviceID();
		}
	}

	private void BeginLoginFlowWithDeviceID()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_0040: Expected Obj, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected Obj, but got Unknown
		LoginOptions val = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = null,
				Type = (ExternalCredentialType)10
			},
			UserLoginInfo = new UserLoginInfo
			{
				DisplayName = DataManager.Player.Customization.Name
			}
		};
		PlatformInterface.GetConnectInterface().Login(val, (object)null, (OnLoginCallback)LogInToDeviceIDOnStartupCallback);
	}

	private void LogInToDeviceIDOnStartupCallback(LoginCallbackInfo loginCallbackInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)loginCallbackInfo.ResultCode == 0)
		{
			deviceIDuserID = loginCallbackInfo.LocalUserId;
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Successfully Logged into Device ID Account {deviceIDuserID}");
		}
		else
		{
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"No Device ID account found on this device Error: {loginCallbackInfo.ResultCode}");
		}
		BeginLoginFlowWithPlatform();
	}

	private void BeginLoginFlowWithPlatform()
	{
		authExpiredCallbackTriggered = false;
		silentLoginFailed = false;
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Start Platform Authentication");
		AuthWithCorrectPlatformImpl();
	}

	private void PlatformAuthReturn()
	{
		if (silentLoginFailed)
		{
			LoginSilentlyImpl();
			return;
		}
		CloseStartupWaitScreen();
		GetAgeOfConsentFromKWS();
	}

	private void AbortToMainMenuThrowError()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		ClearAuthToken();
		silentLoginFailed = false;
		authExpiredCallbackTriggered = false;
		Scene activeScene = SceneManager.GetActiveScene();
		if (!activeScene.name.Equals("MainMenu"))
		{
			SceneManager.LoadScene("MainMenu");
		}
		DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.GenericLoginError, ContinueInOfflineMode);
	}

	private void GetAgeOfConsentFromKWS()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		QueryAgeGateOptions val = new QueryAgeGateOptions();
		ShowCallbackWaitAnim();
		PlatformInterface.GetKWSInterface().QueryAgeGate(val, (object)null, (OnQueryAgeGateCallback)KWSQueryAgeGateCallback);
	}

	private void KWSQueryAgeGateCallback(QueryAgeGateCallbackInfo ageGateCallbackInfo)
	{
		if (!((Object)(object)AmongUsClient.Instance != (Object)null) || !AmongUsClient.Instance.IsInGame)
		{
			logger.Info($"EOSManager::Age gate country {ageGateCallbackInfo.CountryCode}, age of consent is {ageGateCallbackInfo.AgeOfConsent}");
			ageOfConsent = (int)ageGateCallbackInfo.AgeOfConsent;
			UpdateGuardianEmailDisplayAndLoginToPlatform();
		}
	}

	private void UpdateGuardianEmailDisplayAndLoginToPlatform()
	{
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.WaitingForParent)
		{
			DestroyableSingleton<AccountManager>.Instance.UpdateGuardianEmailDisplay();
		}
		tryingToLogin = false;
		LoginWithCorrectPlatform();
	}

	public void LoginWithCorrectPlatform()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected Obj, but got Unknown
		if (!tryingToLogin)
		{
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Log in with Platform " + Constants.GetCurrentPlatformName());
			loginFlowFinished = false;
			tryingToLogin = true;
			ShowCallbackWaitAnim();
			LoginWithCorrectPlatformImpl(EOSConnectPlatformLoginCallback);
		}
	}

	private void EOSConnectPlatformLoginCallback(LoginCallbackInfo loginCallbackInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Invalid comparison between Unknown and I4
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Invalid comparison between Unknown and I4
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Invalid comparison between Unknown and I4
		if ((int)loginCallbackInfo.ResultCode == 19 && DataManager.Player.Account.LoginStatus == AccountLoginStatus.Offline)
		{
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Platform Account Retry {loginCallbackInfo.ResultCode}");
			HideCallbackWaitAnim();
			CloseStartupWaitScreen();
			return;
		}
		if ((int)loginCallbackInfo.ResultCode == 0)
		{
			logger.Info("Platform Login is success");
			userId = loginCallbackInfo.LocalUserId;
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Successfully Logged into Platform Account {Constants.GetCurrentPlatformName()}/{userId}");
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.LoggedIn;
			DataManager.Player.Save();
			EndLoginFlowWithPlatform();
			return;
		}
		logger.Debug("EOS Connect platform Login Callback " + ((object)loginCallbackInfo.ResultCode/*cast due to constrained. prefix*/).ToString());
		if ((int)loginCallbackInfo.ResultCode == 1)
		{
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.NoConnectionError, ContinueInOfflineMode);
			return;
		}
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Login to Platform Account {Constants.GetCurrentPlatformName()} Failed {loginCallbackInfo.ResultCode}");
		if ((int)loginCallbackInfo.ResultCode == 7000 || (int)loginCallbackInfo.ResultCode == 7 || (int)loginCallbackInfo.ResultCode == 1004 || (int)loginCallbackInfo.ResultCode == 1006 || (int)loginCallbackInfo.ResultCode == 1003 || (int)loginCallbackInfo.ResultCode == 7004)
		{
			ClearAuthToken();
		}
		if ((Handle)(object)continuanceToken == (Handle)null)
		{
			continuanceToken = loginCallbackInfo.ContinuanceToken;
		}
		if ((Handle)(object)continuanceToken == (Handle)null)
		{
			tryingToLogin = false;
			LoginWithCorrectPlatform();
		}
		else
		{
			DataManager.Player.Age.DateOfBirth = PlayerAgeData.DEFAULT_DATE_OF_BIRTH;
			DataManager.Player.Save();
			CheckAndShowAgePromptIfNeeded(ShowSignInScreen);
		}
	}

	private IEnumerator ShowSignInWindow()
	{
		while (announcementsVisible)
		{
			yield return (object)new WaitForSeconds(0.3f);
		}
		DestroyableSingleton<AccountManager>.Instance.signInScreen.Open();
	}

	private void ShowSignInScreen()
	{
		((MonoBehaviour)this).StartCoroutine(ShowSignInWindow());
	}

	public void CreateAccountWithPlatformAuth()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected Obj, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected Obj, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected Obj, but got Unknown
		ShowCallbackWaitAnim();
		if ((Handle)(object)continuanceToken == (Handle)null)
		{
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Tried to create new " + Constants.GetCurrentPlatformName() + " account with no continuance token");
			RetryAuthAndLoginImpl(EOSConnectPlatformLoginCallback);
			return;
		}
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Create new " + Constants.GetCurrentPlatformName() + " account");
		CreateUserOptions val = new CreateUserOptions
		{
			ContinuanceToken = continuanceToken
		};
		PlatformInterface.GetConnectInterface().CreateUser(val, (object)null, (OnCreateUserCallback)EOSConnectCreateUserCallback);
	}

	private void EOSConnectCreateUserCallback(CreateUserCallbackInfo createUserCallbackInfo)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Create {Constants.GetCurrentPlatformName()} account result code {createUserCallbackInfo.ResultCode}");
		if ((int)createUserCallbackInfo.ResultCode == 0)
		{
			userId = createUserCallbackInfo.LocalUserId;
			EndLoginFlowWithPlatform();
		}
		else if ((int)createUserCallbackInfo.ResultCode == 7001)
		{
			logger.Warning("Account exists already - why are we here...just try to log in again");
			tryingToLogin = false;
			LoginWithCorrectPlatform();
		}
		else
		{
			logger.Error($"EOS Failed to create account: {createUserCallbackInfo.ResultCode}");
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.AccountCreationFail, ContinueInOfflineMode);
		}
	}

	private void EndLoginFlowWithPlatform()
	{
		((MonoBehaviour)this).StartCoroutine(RefreshPlayerStorageManager(BeginKWSPermissionsFlow));
	}

	private IEnumerator RefreshPlayerStorageManager(Action callback)
	{
		DestroyableSingleton<PlayerStorageManager>.Instance.RefreshAll(PlatformInterface);
		while (DestroyableSingleton<PlayerStorageManager>.Instance.PlayerStorageLoadState != UpdateState.Success)
		{
			yield return null;
		}
		callback();
	}

	private void BeginKWSPermissionsFlow()
	{
		QueryKWSPermissions();
	}

	private void QueryKWSPermissions()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		QueryPermissionsOptions val = new QueryPermissionsOptions
		{
			LocalUserId = userId
		};
		ShowCallbackWaitAnim();
		PlatformInterface.GetKWSInterface().QueryPermissions(val, (object)null, (OnQueryPermissionsCallback)KWSQueryPermissionsOnPlatformLoginCallback);
	}

	private void KWSQueryPermissionsOnPlatformLoginCallback(QueryPermissionsCallbackInfo permissionsCallbackInfo)
	{
		kwsUserId = permissionsCallbackInfo.KWSUserId;
		if (!string.IsNullOrEmpty(kwsUserId))
		{
			string[] array = permissionsCallbackInfo.DateOfBirth.Split('-');
			int.TryParse(array[0], out var result);
			int.TryParse(array[1], out var result2);
			int.TryParse(array[2], out var result3);
			DataManager.Player.Age.DateOfBirth = new DateTime(result, result2, result3);
			DataManager.Player.Save();
			isKWSMinor = IsMinorByDate(result, result2, result3);
			if (isKWSMinor)
			{
				UpdatePermissionKeys(FinishInitialLoginFlowAndDisplayLoginSuccess);
			}
			else
			{
				AdultPermissionsFlow();
			}
		}
		else
		{
			AdultPermissionsFlow();
		}
	}

	private void AdultPermissionsFlow()
	{
		PlayerStorageManager.CloudPlayerPrefs playerPrefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
		logger.Debug($"isAdult flag {playerPrefs.IsAdult}");
		if (playerPrefs.IsAdult == 1)
		{
			FinishInitialLoginFlowAndDisplayLoginSuccess();
		}
		else if (DataManager.Player.Age.DateOfBirth == PlayerAgeData.DEFAULT_DATE_OF_BIRTH)
		{
			CheckAndShowAgePromptIfNeeded(QueryKWSPermissions);
		}
		else if (IsMinorByDate(DataManager.Player.Age.DateOfBirth))
		{
			DateTime latestSetDate = DataManager.Player.Age.LatestSetDate;
			DateTime dateTime = latestSetDate.AddHours(2.0);
			Debug.Log((object)$"DoB set time {latestSetDate.Year}:{latestSetDate.Month}:{latestSetDate.Day}:{latestSetDate.Hour}:{latestSetDate.Minute}:{latestSetDate.Second}");
			if (dateTime < DateTime.UtcNow)
			{
				CheckAndShowAgePromptIfNeeded(QueryKWSPermissions);
				return;
			}
			isKWSMinor = true;
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.WaitingForParent;
			DataManager.Player.Save();
			DestroyableSingleton<AccountManager>.Instance.ShowPermissionsRequestForm();
			logger.Debug($"DoB set time {latestSetDate.Year}:{latestSetDate.Month}:{latestSetDate.Day}:{latestSetDate.Hour}:{latestSetDate.Minute}:{latestSetDate.Second}");
		}
		else
		{
			playerPrefs.IsAdult = 1u;
			DestroyableSingleton<PlayerStorageManager>.Instance.SaveChanges(playerPrefs);
			FinishInitialLoginFlowAndDisplayLoginSuccess();
		}
	}

	public void CreateKWSUer()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected Obj, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected Obj, but got Unknown
		if (!string.IsNullOrEmpty(DataManager.Player.Account.GuardianEmail))
		{
			DateTime dateOfBirth = DataManager.Player.Age.DateOfBirth;
			CreateUserOptions val = new CreateUserOptions();
			val.LocalUserId = userId;
			val.DateOfBirth = dateOfBirth.Year + "-" + dateOfBirth.Month.ToString().PadLeft(2, '0') + "-" + dateOfBirth.Day.ToString().PadLeft(2, '0');
			val.ParentEmail = DataManager.Player.Account.GuardianEmail;
			CreateUserOptions val2 = val;
			ShowCallbackWaitAnim();
			logger.Info("Create KWS User, email: " + EmailUtils.CensorEmail(DataManager.Player.Account.GuardianEmail));
			PlatformInterface.GetKWSInterface().CreateUser(val2, (object)null, (OnCreateUserCallback)CreateKWSUserCallback);
		}
		else
		{
			DestroyableSingleton<AccountManager>.Instance.SignInFail(EOS_ERRORS.InvalidParentEmail, ContinueInOfflineMode);
		}
	}

	private void CreateKWSUserCallback(CreateUserCallbackInfo createUserCallbackInfo)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		logger.Info($"Create KWS User Result: {createUserCallbackInfo.ResultCode}");
		if ((int)createUserCallbackInfo.ResultCode == 0)
		{
			logger.Info("KWS User Created Successfully!");
			DataManager.Player.Age.DateOfBirth = PlayerAgeData.DEFAULT_DATE_OF_BIRTH;
			DataManager.Player.Save();
		}
		UpdatePermissionKeys(FinishInitialLoginFlowAndDisplayLoginSuccess);
	}

	private void UpdatePermissionKeys(Action callback)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected Obj, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected Obj, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected Obj, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		logger.Debug("KWS User ID " + kwsUserId);
		GetPermissionByKeyOptions val = new GetPermissionByKeyOptions
		{
			LocalUserId = userId,
			Key = freeChatKey
		};
		Result permissionByKey = PlatformInterface.GetKWSInterface().GetPermissionByKey(val, ref DestroyableSingleton<AccountManager>.Instance.freeChatAllowed);
		if ((int)permissionByKey != 0)
		{
			logger.Debug("freeChat Update: " + ((object)permissionByKey/*cast due to constrained. prefix*/).ToString());
		}
		GetPermissionByKeyOptions val2 = new GetPermissionByKeyOptions
		{
			LocalUserId = userId,
			Key = customNameKey
		};
		permissionByKey = PlatformInterface.GetKWSInterface().GetPermissionByKey(val2, ref DestroyableSingleton<AccountManager>.Instance.customDisplayName);
		if ((int)permissionByKey != 0)
		{
			logger.Debug("custom display name Update: " + ((object)permissionByKey/*cast due to constrained. prefix*/).ToString());
		}
		GetPermissionByKeyOptions val3 = new GetPermissionByKeyOptions
		{
			LocalUserId = userId,
			Key = friendsListKey
		};
		permissionByKey = PlatformInterface.GetKWSInterface().GetPermissionByKey(val3, ref DestroyableSingleton<AccountManager>.Instance.friendsListAllowed);
		if ((int)permissionByKey != 0)
		{
			logger.Debug("friends list permission Update: " + ((object)permissionByKey/*cast due to constrained. prefix*/).ToString());
		}
		GetPermissionByKeyOptions val4 = new GetPermissionByKeyOptions
		{
			LocalUserId = userId,
			Key = accountLinkKey
		};
		permissionByKey = PlatformInterface.GetKWSInterface().GetPermissionByKey(val4, ref DestroyableSingleton<AccountManager>.Instance.accountLinkingAllowed);
		if ((int)permissionByKey != 0)
		{
			Debug.Log((object)("account linking permission Update: " + ((object)permissionByKey/*cast due to constrained. prefix*/).ToString()));
		}
		DestroyableSingleton<AccountManager>.Instance.UpdateKidAccountDisplay();
		if (DestroyableSingleton<FriendsListManager>.Instance.Ui.IsOpen)
		{
			DestroyableSingleton<FriendsListManager>.Instance.Ui.Close();
		}
		if (!DestroyableSingleton<AccountManager>.Instance.HasMinorsGuardianEverUpdatedAnything())
		{
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.WaitingForParent;
		}
		else if (DataManager.Player.Account.LoginStatus != AccountLoginStatus.LoggedIn)
		{
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.LoggedIn;
		}
		DataManager.Player.Save();
		callback();
	}

	private void FinishInitialLoginFlowAndDisplayLoginSuccess()
	{
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.WaitingForParent)
		{
			EndKWSPermissionsFlow();
		}
		else if (DataManager.Player.Account.LoginStatus != AccountLoginStatus.LoggedIn)
		{
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.LoggedIn;
			if (IsFreechatAllowed())
			{
				DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
			}
			DestroyableSingleton<AccountManager>.Instance.SignInSuccess(EndKWSPermissionsFlow);
		}
		else
		{
			DataManager.Player.Account.LoginStatus = AccountLoginStatus.LoggedIn;
			EndKWSPermissionsFlow();
		}
		DataManager.Player.Save();
	}

	private void EndKWSPermissionsFlow()
	{
		BeginMergeGuestAccountFlow();
	}

	private void BeginMergeGuestAccountFlow()
	{
		CheckGuestAccountMigrationStuff();
	}

	private void CheckGuestAccountMigrationStuff()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected Obj, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected Obj, but got Unknown
		if (DeviceIDProductUserId == null)
		{
			logger.Debug("There's no device ID, which is great, just move on // external account " + ProductUserId);
			EndMergeGuestAccountFlow();
		}
		else
		{
			logger.Debug("Here's the two accounts Device ID " + DeviceIDProductUserId + " // external account " + ProductUserId);
			DataManager.Player.Account.DeleteDeviceIDFailed = false;
			if (string.Compare(DeviceIDProductUserId, ProductUserId) != 0)
			{
				QueryProductUserIdMappingsOptions val = new QueryProductUserIdMappingsOptions();
				val.LocalUserId = ProductUserId.FromString(ProductUserId);
				val.ProductUserIds = new ProductUserId[1] { ProductUserId.FromString(DeviceIDProductUserId) };
				QueryProductUserIdMappingsOptions val2 = val;
				DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetConnectInterface().QueryProductUserIdMappings(val2, (object)null, (OnQueryProductUserIdMappingsCallback)((QueryProductUserIdMappingsCallbackInfo data) =>
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					//IL_0017: Expected Obj, but got Unknown
					//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
					//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
					//IL_00ea: Expected Obj, but got Unknown
					//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
					//IL_0116: Unknown result type (might be due to invalid IL or missing references)
					GetProductUserExternalAccountCountOptions val3 = new GetProductUserExternalAccountCountOptions
					{
						TargetUserId = ProductUserId.FromString(DeviceIDProductUserId)
					};
					numLinkedAccounts = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetConnectInterface().GetProductUserExternalAccountCount(val3);
					if (numLinkedAccounts == 0)
					{
						logger.Debug("No external accounts linked to the device ID, so ask the user if they want to merge");
						askToMergeAccount.GoAheadButton.AddOnClickListeners(MergeGuestAccountIntoPlatform);
						askToMergeAccount.NotRightNowButton.AddOnClickListeners(EndMergeGuestAccountFlow);
						((Component)askToMergeAccount).gameObject.SetActive(true);
					}
					else
					{
						logger.Debug($"Found {numLinkedAccounts} external accounts on this device ID");
						ExternalAccountInfo val5 = default;
						for (uint num = 0u; num < numLinkedAccounts; num++)
						{
							CopyProductUserExternalAccountByIndexOptions val4 = new CopyProductUserExternalAccountByIndexOptions
							{
								TargetUserId = ProductUserId.FromString(ProductUserId),
								ExternalAccountInfoIndex = num
							};
							if ((int)PlatformInterface.GetConnectInterface().CopyProductUserExternalAccountByIndex(val4, ref val5) == 0)
							{
								DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Device ID account {DeviceIDProductUserId} has an external account {num}:{val5.AccountIdType}");
							}
						}
						DeleteDeviceID(EndMergeGuestAccountFlow);
					}
				}));
			}
			else
			{
				logger.Info("Device ID and external account are the same!");
				DeleteDeviceID(EndMergeGuestAccountFlow);
			}
		}
		DataManager.Player.Save();
	}

	private void DeleteDeviceID(Action callback)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		//IL_0036: Expected Obj, but got Unknown
		PlatformInterface.GetConnectInterface().DeleteDeviceId(new DeleteDeviceIdOptions(), (object)null, (OnDeleteDeviceIdCallback)((DeleteDeviceIdCallbackInfo data) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if ((int)data.ResultCode != 0 && !DataManager.Player.Account.DeleteDeviceIDFailed)
			{
				DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Device ID {DeviceIDProductUserId} failed to delete. Retry: {data.ResultCode}");
				DataManager.Player.Account.DeleteDeviceIDFailed = true;
				DataManager.Player.Save();
				DeleteDeviceID(callback);
			}
			else
			{
				logger.Info($"Device ID {DeviceIDProductUserId} deleted because it matched the PUID of the authenticated external account {ProductUserId} : {data.ResultCode}");
				DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Device ID {DeviceIDProductUserId} deleted because it matched the PUID of the authenticated external account {ProductUserId} : {data.ResultCode}");
				callback();
			}
		}));
	}

	private void MergeGuestAccountIntoPlatform()
	{
		((MonoBehaviour)this).StartCoroutine(RequestMergeGuestAccount(MergeGuestAccount.ToMergeGuestAccount(UserIDToken, DeviceIDUserIDToken), (ResponseState state) =>
		{
			logger.Info($"Merge Guest Account result: {state}");
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Merge Guest Account result: {state}");
			if (state == ResponseState.Success)
			{
				DeleteDeviceID(EndMergeGuestAccountFlow);
				EndMergeGuestAccountFlow();
			}
			else
			{
				EndMergeGuestAccountFlow();
			}
		}));
	}

	private void EndMergeGuestAccountFlow()
	{
		BeginFinalPartsOfLoginFlow();
	}

	private void BeginFinalPartsOfLoginFlow()
	{
		((MonoBehaviour)this).StartCoroutine(WaitForStorageToSave());
	}

	private IEnumerator WaitForStorageToSave()
	{
		while (DestroyableSingleton<PlayerStorageManager>.Instance.saveRoutine != null)
		{
			yield return null;
		}
		logger.Info("finishing flow, login status " + DataManager.Player.Account.LoginStatus);
		if (DataManager.Player.Account.LoginStatus != AccountLoginStatus.Offline)
		{
			IsAllowedOnline(canOnline: true);
		}
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog($"Finish Login Flow. Guest: {deviceIDuserID}//Logged in({Constants.GetCurrentPlatformName()}): {userId}");
		((MonoBehaviour)this).StartCoroutine(UpdateFriendCodeAndInventory());
	}

	private IEnumerator UpdateFriendCodeAndInventory()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			yield return ((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<FriendsListManager>.Instance.CheckFriendCodeOnLogin());
		}
		yield return ((MonoBehaviour)this).StartCoroutine(InitializeStore());
		yield return ((MonoBehaviour)this).StartCoroutine(CoUpdateInventoryOrValidateReceipts());
		CloseStartupWaitScreen();
	}

	private IEnumerator InitializeStore()
	{
		DestroyableSingleton<StoreManager>.Instance.Initialize();
		while (!DestroyableSingleton<StoreManager>.Instance.FinishedInitializationFlow)
		{
			yield return null;
		}
		DestroyableSingleton<StoreMenu>.Instance.Initialize();
		yield return null;
	}

	private IEnumerator CoUpdateInventoryOrValidateReceipts()
	{
		isRedeemingDLC = false;
		_ = DataManager.Player.Account.LoginStatus;
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.LoggedIn)
		{
			isRedeemingDLC = true;
			validateOldDLC = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
			DestroyableSingleton<InventoryManager>.Instance.ValidatePreviousReceipts();
			yield return validateOldDLC.WaitForCompletion();
			isRedeemingDLC = false;
			PlayerStorageManager.CloudPlayerPrefs prefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
			uint platformFlag = (uint)(1 << (int)Constants.GetPlatformType());
			if ((prefs.PlatformsLimitedItemsValidated & platformFlag) == 0)
			{
				string[] array = new string[6] { "hat_geoff", "pet_test", "hat_traffic_purple", "hats_newyears2018", "hat_holiday2018", "hats_halloween2019" }.Where((string x) => DataManager.Player.Purchases.GetPurchase(x, x)).ToArray();
				if (array.Length != 0)
				{
					List<string> list = new List<string>();
					for (int num = 0; num < array.Length; num++)
					{
						list.Add(RedeemCurrency.ProductIdToItemId(array[num]));
					}
					CompletionToken<PurchaseStates> validateSpecialItems = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
					yield return DestroyableSingleton<InventoryManager>.Instance.CoValidateTrustedReceipt(list.ToArray(), validateSpecialItems);
					if (validateSpecialItems.Value != PurchaseStates.Success)
					{
						logger.Error("Redemption of special items failed. Error " + validateSpecialItems.errorStr);
					}
				}
				prefs.PlatformsLimitedItemsValidated |= platformFlag;
				DestroyableSingleton<PlayerStorageManager>.Instance.SaveChanges(prefs);
			}
			UpdateInventory();
		}
		else
		{
			UpdateInventory();
		}
	}

	private void HandleEosLogging(LogMessage msg)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		LogLevel level = msg.Level;
		if ((int)level != 200)
		{
			if ((int)level != 300)
			{
				_ = 400;
				logger.Info(msg.Message);
			}
			else
			{
				logger.Warning(msg.Message);
			}
		}
		else
		{
			logger.Error(msg.Message);
		}
	}

	public void ContinueInOfflineMode()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		DestroyableSingleton<FriendsListManager>.Instance.OnSignOut();
		logger.Warning("Continuing in offline mode");
		DataManager.Player.Account.LoginStatus = AccountLoginStatus.Offline;
		DataManager.Player.Save();
		userId = new ProductUserId();
		EndFinalPartsOfLoginFlowOffline();
	}

	private void IsAllowedOnline(bool canOnline)
	{
		GameObject val = FindPlayOnlineButton();
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		PlayOnlineButtonSprite component = val.GetComponent<PlayOnlineButtonSprite>();
		if ((Object)(object)component != (Object)null)
		{
			component.SetState(canOnline);
		}
		PassiveButton component2 = val.GetComponent<PassiveButton>();
		component2?.SetButtonEnableState(canOnline);
		if (!canOnline || !Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		ControllerUiElementsState menu = ControllerManager.Instance.GetMenu("MainMenuManager");
		if (menu != null)
		{
			if (ControllerManager.Instance.CurrentUiState == menu && (Object)(object)menu.CurrentSelection != (Object)(object)component2)
			{
				menu.CurrentSelection.ReceiveMouseOut();
			}
			menu.CurrentSelection = component2;
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
			{
				ButtonRolloverHandler component3 = val.GetComponent<ButtonRolloverHandler>();
				AudioClip hoverSound = component3.HoverSound;
				component3.HoverSound = null;
				component2.ReceiveMouseOver();
				component3.HoverSound = hoverSound;
			}
		}
	}

	private void CheckAndShowAgePromptIfNeeded(Action callback)
	{
		DateTime dateOfBirth = DataManager.Player.Age.DateOfBirth;
		bool flag = false;
		if (dateOfBirth == PlayerAgeData.DEFAULT_DATE_OF_BIRTH)
		{
			flag = true;
		}
		if (dateOfBirth.Year == 0 || dateOfBirth.Year == PlayerAgeData.DEFAULT_DATE_OF_BIRTH.Year)
		{
			flag = true;
		}
		if (dateOfBirth.Year == DateTime.UtcNow.Year)
		{
			flag = true;
		}
		if (IsMinorByDate(dateOfBirth))
		{
			flag = true;
		}
		if (flag)
		{
			((MonoBehaviour)this).StartCoroutine(ShowAgePrompt(callback));
		}
		else
		{
			callback();
		}
	}

	private IEnumerator ShowAgePrompt(Action callback)
	{
		HideCallbackWaitAnim();
		DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Query DOB");
		while (!DestroyableSingleton<AccountManager>.Instance.AccountTabActive || announcementsVisible)
		{
			yield return null;
		}
		yield return DestroyableSingleton<AccountManager>.Instance.ShowAgeGate();
		ShowCallbackWaitAnim();
		callback();
	}

	public void LoginFromAccountTab()
	{
		LoginWithCorrectPlatform();
	}

	private void EOSConnectPlatformSilentLoginCallback(LoginCallbackInfo loginCallbackInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)loginCallbackInfo.ResultCode != 0)
		{
			if (silentLoginFailed)
			{
				DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("Silent login failed at least twice in a row!");
			}
			silentLoginFailed = true;
		}
		else
		{
			silentLoginFailed = false;
		}
		authExpiredCallbackTriggered = false;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		if (scene.name == "EndGame" && silentLoginFailed)
		{
			ClearAuthToken();
			authExpiredCallbackTriggered = false;
			LoginWithCorrectPlatformImpl(EOSConnectPlatformSilentLoginCallback, AbortToMainMenuThrowError);
		}
	}

	private GameObject FindPlayOnlineButton()
	{
		MainMenuManager mainMenuManager = Object.FindObjectOfType<MainMenuManager>();
		if (Object.op_Implicit((Object)(object)mainMenuManager))
		{
			return ((Component)mainMenuManager.PlayOnlineButton).gameObject;
		}
		return null;
	}

	public void GoOfflineFromPermissionsWindow()
	{
		ContinueInOfflineMode();
	}

	public void GoOfflineFromPlatformSignout()
	{
		ContinueInOfflineMode();
	}

	public void GetExternalAccountInfo(string targetPUID, Platforms targetAccountPlatform, Action<ExternalAccountInfo> callback)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected Obj, but got Unknown
		ProductUserId eosPUID = ProductUserId.FromString(targetPUID);
		QueryProductUserIdMappingsOptions val = new QueryProductUserIdMappingsOptions();
		val.ProductUserIds = new ProductUserId[1] { eosPUID };
		val.LocalUserId = ProductUserId.FromString(ProductUserId);
		QueryProductUserIdMappingsOptions val2 = val;
		PlatformInterface.GetConnectInterface().QueryProductUserIdMappings(val2, (object)null, (OnQueryProductUserIdMappingsCallback)((QueryProductUserIdMappingsCallbackInfo res) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			if ((int)res.ResultCode == 0)
			{
				ExternalAccountType val3 = (ExternalAccountType)0;
				switch (targetAccountPlatform)
				{
				case Platforms.Android:
					val3 = (ExternalAccountType)10;
					break;
				case Platforms.StandaloneMac:
				case Platforms.IPhone:
					val3 = (ExternalAccountType)9;
					break;
				case Platforms.Playstation:
					val3 = (ExternalAccountType)2;
					break;
				case Platforms.StandaloneEpicPC:
					val3 = (ExternalAccountType)0;
					break;
				case Platforms.StandaloneItch:
					val3 = (ExternalAccountType)12;
					break;
				case Platforms.StandaloneSteamPC:
					val3 = (ExternalAccountType)1;
					break;
				case Platforms.StandaloneWin10:
				case Platforms.Xbox:
					val3 = (ExternalAccountType)3;
					break;
				case Platforms.Switch:
					val3 = (ExternalAccountType)6;
					break;
				case Platforms.Unknown:
					logger.Error("Unknown platform provided for EOSManager.GetExternalAccountInfo()");
					break;
				}
				if ((int)val3 == 0)
				{
					callback(null);
				}
				else
				{
					callback(GetExternalAccountInfo(eosPUID, val3));
				}
			}
		}));
	}

	private ExternalAccountInfo GetExternalAccountInfo(ProductUserId targetPUID, ExternalAccountType targetAccountType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		CopyProductUserExternalAccountByAccountTypeOptions val = new CopyProductUserExternalAccountByAccountTypeOptions
		{
			TargetUserId = targetPUID,
			AccountIdType = targetAccountType
		};
		ExternalAccountInfo result = default;
		if ((int)PlatformInterface.GetConnectInterface().CopyProductUserExternalAccountByAccountType(val, ref result) == 0)
		{
			return result;
		}
		return null;
	}

	public void UpdateInventory()
	{
		DestroyableSingleton<InventoryManager>.Instance.RefreshAll(PlatformInterface);
	}

	public void EndFinalPartsOfLoginFlow()
	{
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.TempAccount)
		{
			EndFinalPartsOfLoginFlowTempAccount();
		}
		else if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.Offline)
		{
			EndFinalPartsOfLoginFlowOffline();
		}
		else
		{
			EndFinalPartsOfLoginFlowFullAccount();
		}
	}

	public void EndFinalPartsOfLoginFlowFullAccount()
	{
		DestroyableSingleton<AchievementManager>.Instance.UpdateAchievementsAndStats();
		DestroyableSingleton<AccountManager>.Instance.UpdateVisuals();
		HideCallbackWaitAnim();
		loginFlowFinished = true;
		tryingToLogin = false;
		if (!DestroyableSingleton<AccountManager>.Instance.signInScreen.IsOpen())
		{
			DestroyableSingleton<AccountManager>.Instance.signInScreen.OnClose();
		}
		Debug.Log((object)("finishing flow, login status " + DataManager.Player.Account.LoginStatus));
		if (DataManager.Player.Account.LoginStatus != AccountLoginStatus.Offline)
		{
			DestroyableSingleton<FriendsListManager>.Instance.OnSignIn();
			IsAllowedOnline(canOnline: true);
		}
		HideTimeout();
	}

	public void EndFinalPartsOfLoginFlowTempAccount()
	{
		DestroyableSingleton<AccountManager>.Instance.UpdateVisuals();
		HideCallbackWaitAnim();
		loginFlowFinished = true;
		tryingToLogin = false;
		if (!DestroyableSingleton<AccountManager>.Instance.signInScreen.IsOpen())
		{
			DestroyableSingleton<AccountManager>.Instance.signInScreen.OnClose();
		}
		Debug.Log((object)("finishing generic flow, login status " + DataManager.Player.Account.LoginStatus));
		IsAllowedOnline(canOnline: true);
		HideTimeout();
	}

	public void EndFinalPartsOfLoginFlowOffline()
	{
		DestroyableSingleton<AccountManager>.Instance.UpdateVisuals();
		HideCallbackWaitAnim();
		loginFlowFinished = true;
		tryingToLogin = false;
		if (!DestroyableSingleton<AccountManager>.Instance.signInScreen.IsOpen())
		{
			DestroyableSingleton<AccountManager>.Instance.signInScreen.OnClose();
		}
		Debug.Log((object)("finishing generic flow, login status " + DataManager.Player.Account.LoginStatus));
		IsAllowedOnline(canOnline: false);
		CloseStartupWaitScreen();
		HideTimeout();
	}

	private IEnumerator RequestMergeGuestAccount(Request<MergeGuestAccount> requestData, Action<ResponseState> resultCallback)
	{
		string mergeGuestAccount = BackendEndpoints.MergeGuestAccount;
		string text = JsonConvert.SerializeObject((object)requestData);
		logger.Debug("Guest Merge json content " + text);
		UnityWebRequest request = UnityWebRequest.Post(mergeGuestAccount, text);
		request.SetRequestHeader("Content-Type", "application/vnd.api+json");
		request.SetRequestHeader("Accept", "application/vnd.api+json");
		if (string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.UserIDToken))
		{
			logger.Error("Cannot merge account, UserIDToken does not exist");
			resultCallback(ResponseState.Failed);
			yield break;
		}
		request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		UploadHandlerRaw val = new UploadHandlerRaw(Encoding.ASCII.GetBytes(text));
		((UploadHandler)val).contentType = "application/vnd.api+json";
		request.uploadHandler = (UploadHandler)(object)val;
		logger.Debug("Sent request guest merge request");
		yield return request.SendWebRequest();
		logger.Debug("guest merge Request done");
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			logger.Error("Request failed, " + request.responseCode + ": " + (HttpStatusCode)request.responseCode/*cast due to constrained. prefix*/);
			logger.Error(request.error);
			string text2 = request.downloadHandler.text;
			logger.Info("Failed to merge account: " + text2);
			resultCallback(ResponseState.Failed);
		}
		else
		{
			_ = request.downloadHandler.text;
			resultCallback(ResponseState.Success);
		}
	}

	public void ClearAuthToken()
	{
		platformAuthToken = "";
	}

	public string GetPlatformAuthToken()
	{
		return platformAuthToken;
	}

	public void UpdateGuardianEmail()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected Obj, but got Unknown
		UpdateParentEmailOptions val = new UpdateParentEmailOptions
		{
			LocalUserId = userId,
			ParentEmail = DataManager.Player.Account.GuardianEmail
		};
		logger.Info("Update Guardian Email: " + EmailUtils.CensorEmail(DataManager.Player.Account.GuardianEmail));
		PlatformInterface.GetKWSInterface().UpdateParentEmail(val, (object)null, (OnUpdateParentEmailCallback)UpdateGuardianEmailSettingsCallback);
	}

	public void ResendGuardianEmail()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected Obj, but got Unknown
		RequestPermissionsOptions val = new RequestPermissionsOptions();
		val.LocalUserId = userId;
		val.PermissionKeys = new string[3] { friendsListKey, customNameKey, freeChatKey };
		RequestPermissionsOptions val2 = val;
		logger.Info("Resend Guardian Email: " + EmailUtils.CensorEmail(DataManager.Player.Account.GuardianEmail));
		PlatformInterface.GetKWSInterface().RequestPermissions(val2, (object)null, (OnRequestPermissionsCallback)RequestPermissionsCallback);
	}

	private void RequestPermissionsCallback(RequestPermissionsCallbackInfo requestPermissionsCallbackInfo)
	{
		logger.Info("Successfully resent guardian email");
		BeginFinalPartsOfLoginFlow();
	}

	private void UpdateGuardianEmailSettingsCallback(UpdateParentEmailCallbackInfo updateParentEmailCallbackInfo)
	{
		logger.Info("Successfully updated guardian email");
		DataManager.Player.Account.LoginStatus = AccountLoginStatus.WaitingForParent;
		DataManager.Player.Save();
		BeginFinalPartsOfLoginFlow();
	}

	public bool IsMinorByDate(DateTime date)
	{
		return IsMinorByDate(date.Year, date.Month, date.Day);
	}

	public bool IsMinorByDate(int year, int month, int day)
	{
		if (year == PlayerAgeData.DEFAULT_DATE_OF_BIRTH.Year)
		{
			return false;
		}
		if (year < 1 || month < 1 || month > 12)
		{
			return false;
		}
		DateTime dateTime = new DateTime(year, month, day).AddYears(ageOfConsent);
		if (dateTime > DateTime.UtcNow)
		{
			return true;
		}
		return false;
	}

	public bool IsMinorOrWaiting()
	{
		if (isKWSMinor)
		{
			return true;
		}
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.WaitingForParent)
		{
			return true;
		}
		return false;
	}

	public bool IsFriendsListAllowed()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.LoggedIn)
		{
			if (!IsMinorOrWaiting())
			{
				return true;
			}
			if (IsMinorOrWaiting() && (int)DestroyableSingleton<AccountManager>.Instance.friendsListAllowed == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFreechatAllowed()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (DataManager.Player.Account.LoginStatus == AccountLoginStatus.LoggedIn)
		{
			if (!IsMinorOrWaiting())
			{
				return true;
			}
			if (IsMinorOrWaiting() && (int)DestroyableSingleton<AccountManager>.Instance.freeChatAllowed == 0)
			{
				return true;
			}
		}
		DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.QuickChatOnly;
		return false;
	}

	public IEnumerator WaitForLoginFlow()
	{
		while (!HasFinishedLoginFlow())
		{
			yield return null;
		}
	}

	public bool HasFinishedLoginFlow()
	{
		return loginFlowFinished;
	}

	private void CloseStartupWaitScreen()
	{
		DestroyableSingleton<AccountManager>.Instance.waitingText.gameObject.SetActive(false);
	}

	private void ShowCallbackWaitAnim()
	{
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(true);
	}

	private void HideCallbackWaitAnim()
	{
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(false);
	}

	private void OnEnable()
	{
		_ = platformInitialized;
	}

	private new void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (platformInitialized)
		{
			DoShutdown();
		}
	}

	private void Update()
	{
		DumpEGSPluginLogs();
		if (!platformInitialized)
		{
			return;
		}
		watermarkCanvas.gameObject.SetActive(false);
		if ((Handle)(object)PlatformInterface != (Handle)null)
		{
			platformTickTimer += Time.deltaTime;
			if (platformTickTimer >= 0.1f)
			{
				platformTickTimer = 0f;
				PlatformInterface.Tick();
			}
		}
	}

	[ContextMenu("Shutdown")]
	private void DoShutdown()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)PlatformInterface != (Handle)null)
		{
			PlatformInterface.Release();
			platformInterface = null;
		}
		PlatformInterface.Shutdown();
	}

	private void DumpEGSPluginLogs()
	{
	}
}
