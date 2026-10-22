using AmongUs.Data;
using TMPro;
using UnityEngine;

public class ShowPUID : MonoBehaviour
{
	public int howManyClicks = 5;

	public TextMeshPro tmp;

	public void Awake()
	{
		howManyClicks = 5;
		ShowPUIDCountdown();
	}

	public void OnDisable()
	{
		howManyClicks = 5;
		ShowPUIDCountdown();
	}

	public void ShowPUIDCountdown()
	{
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID5);
		if (howManyClicks == 4)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID4);
		}
		else if (howManyClicks == 3)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID3);
		}
		else if (howManyClicks == 2)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID2);
		}
		else if (howManyClicks == 1)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShowAccountSupportID1);
		}
		else if (howManyClicks == 0)
		{
			if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline)
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.YouAreNotOnline);
			}
			else
			{
				text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SupportIDLabel, DestroyableSingleton<EOSManager>.Instance.ProductUserId);
				if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.WaitingForParent)
				{
					text += "-W";
				}
				text = (GUIUtility.systemCopyBuffer = (DestroyableSingleton<EOSManager>.Instance.IsMinorOrWaiting() ? (text + "-M") : ((DataManager.Player.Account.LoginStatus != EOSManager.AccountLoginStatus.TempAccount) ? (text + "-A") : (text + "-E"))));
			}
		}
		else if (howManyClicks == -1)
		{
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CopiedText);
		}
		((TMP_Text)tmp).text = text;
		howManyClicks--;
		if (howManyClicks < -1)
		{
			howManyClicks = 5;
		}
	}
}
