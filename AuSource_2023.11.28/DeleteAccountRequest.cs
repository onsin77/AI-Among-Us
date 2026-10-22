using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DeleteAccountRequest : MonoBehaviour, ITranslatedText
{
	public TextMeshPro buttonText;

	public DeleteAccountPopupBox popupConfirmation;

	public InfoTextBox popupConfirmationUndelete;

	public InfoTextBox somethingWentWrong;

	private void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		UpdateText();
		((UnityEvent)popupConfirmation.button1.OnClick).AddListener((UnityAction)(() =>
		{
			DeleteAccount();
		}));
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
	}

	private void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			try
			{
				DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
			}
			catch
			{
			}
		}
	}

	public void ShowConfirmationScreen()
	{
		if (DestroyableSingleton<AccountManager>.Instance.IsAccountWaitingForDeletion)
		{
			((Component)popupConfirmationUndelete).gameObject.SetActive(true);
			popupConfirmationUndelete.SetTwoButtons();
		}
		else
		{
			((Component)popupConfirmation).gameObject.SetActive(true);
		}
	}

	public void ResetText()
	{
		UpdateText();
	}

	private void SomethingWentWrong(string error)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected Obj, but got Unknown
		((TMP_Text)somethingWentWrong.bodyText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherDownloadError, error), true);
		((Component)somethingWentWrong).gameObject.SetActive(true);
		somethingWentWrong.SetOneButton();
		((TMP_Text)somethingWentWrong.button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Okay), true);
		((UnityEventBase)somethingWentWrong.button1.OnClick).RemoveAllListeners();
		((UnityEvent)somethingWentWrong.button1.OnClick).AddListener((UnityAction)(() =>
		{
			UpdateText();
		}));
	}

	private void DeleteAccount()
	{
		((MonoBehaviour)this).StartCoroutine(AddDeleteRequest());
	}

	private void UnDeleteAccount()
	{
		((MonoBehaviour)this).StartCoroutine(RemoveDeleteRequest());
	}

	private IEnumerator AddDeleteRequest()
	{
		string accountDeletion = BackendEndpoints.AccountDeletion;
		UnityWebRequest request = UnityWebRequest.Post(accountDeletion, "");
		request.SetRequestHeader("Content-Type", "application/vnd.api+json");
		request.SetRequestHeader("Accept", "application/vnd.api+json");
		request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		yield return request.SendWebRequest();
		Logger.GlobalInstance.Info($"POST account deletion info {request.responseCode}");
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			_ = string.Empty;
			if (request.downloadHandler != null)
			{
				_ = request.downloadHandler.text;
			}
			SomethingWentWrong($"POST ERROR {request.responseCode}");
		}
		else
		{
			DestroyableSingleton<AccountManager>.Instance.UpdateAccountDeletionStatus(shouldDelete: true);
			UpdateText();
		}
	}

	private IEnumerator RemoveDeleteRequest()
	{
		string accountDeletion = BackendEndpoints.AccountDeletion;
		UnityWebRequest request = UnityWebRequest.Delete(accountDeletion);
		request.SetRequestHeader("Content-Type", "application/vnd.api+json");
		request.SetRequestHeader("Accept", "application/vnd.api+json");
		request.SetRequestHeader("Authorization", "Bearer " + DestroyableSingleton<EOSManager>.Instance.UserIDToken);
		yield return request.SendWebRequest();
		Logger.GlobalInstance.Info($"DELETE account deletion info {request.responseCode}");
		if (!HttpUtils.IsSuccess(request.responseCode))
		{
			_ = string.Empty;
			if (request.downloadHandler != null)
			{
				_ = request.downloadHandler.text;
			}
			SomethingWentWrong($"DELETE ERROR {request.responseCode}");
		}
		else
		{
			DestroyableSingleton<AccountManager>.Instance.UpdateAccountDeletionStatus(shouldDelete: false);
			UpdateText();
		}
	}

	private void UpdateText()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected Obj, but got Unknown
		if (!DestroyableSingleton<AccountManager>.Instance.IsAccountWaitingForDeletion)
		{
			((TMP_Text)buttonText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DeleteAccount), true);
			return;
		}
		((TMP_Text)popupConfirmationUndelete.button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Confirm), true);
		((TMP_Text)popupConfirmationUndelete.button2Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Cancel), true);
		((TMP_Text)popupConfirmationUndelete.titleTexxt).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DoNotDeleteAccount), true);
		((TMP_Text)popupConfirmationUndelete.bodyText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AccountUnDeleteHelp), true);
		((TMP_Text)buttonText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DoNotDeleteAccount), true);
		((UnityEventBase)popupConfirmationUndelete.button1.OnClick).RemoveAllListeners();
		((UnityEvent)popupConfirmationUndelete.button1.OnClick).AddListener((UnityAction)(() =>
		{
			UnDeleteAccount();
		}));
	}
}
