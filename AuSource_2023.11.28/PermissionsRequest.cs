using AmongUs.Data;
using UnityEngine;

public class PermissionsRequest : MonoBehaviour
{
	public EmailTextBehaviour emailText;

	public EmailTextBehaviour emailConfirmText;

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	public void ContinueWithout()
	{
		DestroyableSingleton<EOSManager>.Instance.GoOfflineFromPermissionsWindow();
		Close();
	}

	public void SendEmail()
	{
		if (!emailText.ShakeIfInvalid() && !emailConfirmText.ShakeIfInvalid() && !emailConfirmText.ShakeIfDoesntMatch(emailText.nameSource.text, emailConfirmText.nameSource.text))
		{
			string emailValidEmail = emailText.GetEmailValidEmail();
			DataManager.Player.Account.GuardianEmail = emailValidEmail;
			DataManager.Player.Save();
			DestroyableSingleton<AccountManager>.Instance.UpdateGuardianEmailDisplay();
			DestroyableSingleton<EOSManager>.Instance.CreateKWSUer();
			Close();
		}
	}
}
