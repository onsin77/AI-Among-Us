using System.Collections;
using AmongUs.Data;
using UnityEngine;

public class UpdateGuardianEmail : MonoBehaviour
{
	public EmailTextBehaviour emailText;

	public EmailTextBehaviour emailConfirmText;

	public IEnumerator Show()
	{
		((Component)this).gameObject.SetActive(true);
		while (((Component)this).gameObject.activeSelf)
		{
			yield return null;
		}
	}

	public void Close()
	{
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	public void SendUpdateGuardianEmail()
	{
		if (!emailText.ShakeIfInvalid() && !emailConfirmText.ShakeIfInvalid() && !emailConfirmText.ShakeIfDoesntMatch(emailText.nameSource.text, emailConfirmText.nameSource.text))
		{
			string emailValidEmail = emailText.GetEmailValidEmail();
			DataManager.Player.Account.GuardianEmail = emailValidEmail;
			DataManager.Player.Save();
			DestroyableSingleton<EOSManager>.Instance.UpdateGuardianEmail();
			DestroyableSingleton<AccountManager>.Instance.ShowGuardianEmailSentConfirm();
			Close();
		}
	}
}
