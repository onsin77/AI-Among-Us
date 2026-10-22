using System;
using System.Collections.Generic;
using AmongUs.Data;
using Epic.OnlineServices;
using InnerNet;
using TMPro;
using UnityEngine;

public class AccountTab : MonoBehaviour
{
	public TextMeshPro userName;

	public TextMeshPro friendCode;

	public TextMeshPro friendCodeTitle;

	public PoolablePlayer playerImage;

	public GameObject friendCodeObject;

	public SpriteRenderer FriendCodeHiddenIcon;

	public GameObject offlineMode;

	public GameObject guestMode;

	public FullAccount loggedInMode;

	public GameObject waitingForGuardian;

	public TextMeshPro guardianEmailText;

	public EditName editNameScreen;

	public GameObject idCard;

	public GameObject resendEmailButton;

	public TextMeshPro levelText;

	public ProgressBar xpProgressBar;

	public TextMeshPro veryBadErrorText;

	public Collider2D clickToCloseCollider;

	public TextMeshPro accountIDDisplayText;

	public GameObject showAccountIDButton;

	public SpriteRenderer SpaceBean;

	public SpriteRenderer SpaceHorse;

	public InfoTextBox InfoTextBoxDisplay;

	[SerializeField]
	private SignInStatusComponent signInStatusComponent;

	private bool showAccountID;

	private string friendCodeHiddenText = "XXXXXXXXXX";

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public List<UiElement> PotentialDefaultSelections;

	public List<UiElement> selectableObjects;

	private UiElement DefaultSelection
	{
		get
		{
			foreach (UiElement potentialDefaultSelection in PotentialDefaultSelections)
			{
				if (((Behaviour)potentialDefaultSelection).isActiveAndEnabled)
				{
					return potentialDefaultSelection;
				}
			}
			return null;
		}
	}

	private void Awake()
	{
		((Renderer)SpaceHorse).enabled = Constants.ShouldHorseAround();
		((Renderer)SpaceBean).enabled = !Constants.ShouldHorseAround();
	}

	public void TurnAllSectionsOff()
	{
		((Component)loggedInMode).gameObject.SetActive(false);
		waitingForGuardian.SetActive(false);
		((Component)veryBadErrorText).gameObject.SetActive(false);
		offlineMode.gameObject.SetActive(false);
		guestMode.gameObject.SetActive(false);
	}

	public void UpdateKidAccountCanChangeName()
	{
		loggedInMode.CanSetCustomName(DestroyableSingleton<AccountManager>.Instance.CanMinorSetCustomDisplayName());
	}

	public void SignIn()
	{
		DestroyableSingleton<EOSManager>.Instance.LoginFromAccountTab();
	}

	public void RandomizeName()
	{
		DestroyableSingleton<AccountManager>.Instance.RandomizeName();
	}

	public void ManageAccount()
	{
		((TMP_Text)InfoTextBoxDisplay.titleTexxt).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ManageAccountTitle);
		((TMP_Text)InfoTextBoxDisplay.button1Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Okay);
		InfoTextBoxDisplay.SetOneButton();
		((Component)InfoTextBoxDisplay).gameObject.SetActive(true);
		InfoTextBoxDisplay.SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ManageAccountText));
	}

	public void UpdateNameDisplay()
	{
		((TMP_Text)userName).text = DataManager.Player.Customization.Name;
		if (!string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.FriendCode))
		{
			((Behaviour)friendCodeTitle).enabled = true;
			((Behaviour)friendCode).enabled = true;
		}
		else
		{
			((Behaviour)friendCodeTitle).enabled = false;
			((Behaviour)friendCode).enabled = false;
		}
	}

	public void UpdatePlayerCosmetics()
	{
		playerImage.UpdateFromDataManager(PlayerMaterial.MaskType.SimpleUI);
		playerImage.ToggleName(active: false);
	}

	public void ChangeName()
	{
		((Component)editNameScreen).gameObject.SetActive(true);
	}

	public void ResendEmail()
	{
		DestroyableSingleton<EOSManager>.Instance.ResendGuardianEmail();
		DestroyableSingleton<AccountManager>.Instance.ShowGuardianEmailSentConfirm();
	}

	public void EditGuardianEmail()
	{
		((MonoBehaviour)this).StartCoroutine(DestroyableSingleton<AccountManager>.Instance.EditGuardianEmail());
	}

	public void UpdateGuardianEmailText()
	{
		((TMP_Text)guardianEmailText).text = DataManager.Player.Account.GuardianEmail;
		resendEmailButton.gameObject.SetActive(!string.IsNullOrEmpty(DataManager.Player.Account.GuardianEmail));
	}

	public void SetDLLErrorMode()
	{
		TurnAllSectionsOff();
		DestroyableSingleton<AccountManager>.Instance.RandomizeName();
		idCard.SetActive(true);
		((Component)veryBadErrorText).gameObject.SetActive(true);
		((TMP_Text)veryBadErrorText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DLLNotFoundAccountError);
	}

	public void UpdateVisuals()
	{
		TurnAllSectionsOff();
		UpdateNameDisplay();
		UpdateLevelDisplay();
		if (string.IsNullOrEmpty(((TMP_Text)userName).text))
		{
			DestroyableSingleton<AccountManager>.Instance.RandomizeName();
		}
		showAccountID = false;
		UpdateAccountIDDisplay();
		UpdateFriendCodeUI();
		switch (DataManager.Player.Account.LoginStatus)
		{
		case EOSManager.AccountLoginStatus.LoggedIn:
			Debug.Log((object)"*******************updating to LOGGED IN MODE");
			signInStatusComponent.SetOnline();
			if (DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
			{
				resendEmailButton.gameObject.SetActive(!string.IsNullOrEmpty(DataManager.Player.Account.GuardianEmail));
			}
			idCard.SetActive(true);
			((Component)accountIDDisplayText).gameObject.SetActive(true);
			((Component)loggedInMode).gameObject.SetActive(true);
			loggedInMode.UpdateLoggedInAccountVisuals();
			DestroyableSingleton<AccountManager>.Instance.GetDeletionInfo();
			break;
		case EOSManager.AccountLoginStatus.Offline:
			Debug.Log((object)"*******************updating to OFFLINE MODE");
			signInStatusComponent.SetOffline();
			idCard.SetActive(true);
			offlineMode.gameObject.SetActive(true);
			((Component)accountIDDisplayText).gameObject.SetActive(false);
			if ((Handle)(object)DestroyableSingleton<EOSManager>.Instance.PlatformInterface == (Handle)null)
			{
				SetDLLErrorMode();
			}
			break;
		case EOSManager.AccountLoginStatus.WaitingForParent:
			Debug.Log((object)"*******************updating to WAITING FOR PARENT MODE");
			signInStatusComponent.SetPending();
			((TMP_Text)guardianEmailText).text = (string.IsNullOrEmpty(DataManager.Player.Account.GuardianEmail) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GuardianEmail) : DataManager.Player.Account.GuardianEmail);
			idCard.SetActive(false);
			((Component)accountIDDisplayText).gameObject.SetActive(true);
			waitingForGuardian.gameObject.SetActive(true);
			DestroyableSingleton<AccountManager>.Instance.GetDeletionInfo();
			break;
		case EOSManager.AccountLoginStatus.TempAccount:
			Debug.Log((object)"*******************updating to ERROR MODE");
			signInStatusComponent.SetGuest();
			idCard.SetActive(true);
			((Component)accountIDDisplayText).gameObject.SetActive(true);
			((Component)loggedInMode).gameObject.SetActive(false);
			guestMode.gameObject.SetActive(true);
			loggedInMode.UpdateLoggedInAccountVisuals();
			DestroyableSingleton<AccountManager>.Instance.GetDeletionInfo();
			break;
		}
	}

	private void UpdateAccountIDDisplay()
	{
		if (!string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.ProductUserId))
		{
			if (showAccountID)
			{
				((TMP_Text)accountIDDisplayText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AccountIDDisplay, DestroyableSingleton<EOSManager>.Instance.ProductUserId);
				showAccountIDButton.gameObject.SetActive(false);
			}
			else
			{
				showAccountIDButton.gameObject.SetActive(true);
				((TMP_Text)accountIDDisplayText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AccountIDDisplay, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HiddenAccountID));
			}
		}
		else
		{
			showAccountID = false;
			showAccountIDButton.gameObject.SetActive(false);
			((TMP_Text)accountIDDisplayText).text = "";
		}
	}

	public void HideAccountIDDisplay()
	{
		showAccountID = false;
		UpdateAccountIDDisplay();
	}

	public void ShowAccountIDDisplay()
	{
		showAccountID = true;
		UpdateAccountIDDisplay();
	}

	public void ToggleFriendCodeDisplay()
	{
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn)
		{
			DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden = !DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
			UpdateFriendCodeUI();
		}
	}

	public void UpdateFriendCodeUI()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsFriendsListAllowed())
		{
			friendCodeObject.SetActive(true);
			((Renderer)FriendCodeHiddenIcon).enabled = DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden;
			if (DestroyableSingleton<FriendsListManager>.Instance.FriendCodeHidden)
			{
				((TMP_Text)friendCode).text = friendCodeHiddenText;
			}
			else
			{
				((TMP_Text)friendCode).text = DestroyableSingleton<EOSManager>.Instance.FriendCode;
			}
		}
		else
		{
			friendCodeObject.SetActive(false);
		}
	}

	public void Toggle()
	{
		SlideOpen component = ((Component)this).GetComponent<SlideOpen>();
		if (!((Object)(object)component == (Object)null))
		{
			if (component.isOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	public void Close()
	{
		((Behaviour)clickToCloseCollider).enabled = false;
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Open()
	{
		HideAccountIDDisplay();
		((Behaviour)clickToCloseCollider).enabled = true;
		UpdateVisuals();
		UpdatePlayerCosmetics();
		UpdateFriendCodeUI();
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultSelection, selectableObjects);
	}

	private void UpdateLevelDisplay()
	{
		((TMP_Text)levelText).text = ProgressionManager.Instance.CurrentVisualLevel;
		xpProgressBar.MaxValue = Math.Max(1u, DataManager.Player.Stats.XpForNextLevel);
		xpProgressBar.Value = Mathf.Clamp((float)DataManager.Player.Stats.Xp, 0f, xpProgressBar.MaxValue);
		xpProgressBar.CapValue = xpProgressBar.Value;
		xpProgressBar.GlowAlpha = 0f;
	}
}
