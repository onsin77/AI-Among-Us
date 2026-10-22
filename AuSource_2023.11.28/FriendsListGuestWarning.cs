using AmongUs.Data;
using TMPro;
using UnityEngine;

public class FriendsListGuestWarning : MonoBehaviour
{
	public TextMeshPro Text;

	public GameObject EmailButton;

	public void SetUp()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.TempAccount)
		{
			EmailButton.SetActive(false);
			((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FriendListUnavailable);
		}
		else if ((int)DestroyableSingleton<AccountManager>.Instance.friendsListAllowed != 0 && (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn || DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.WaitingForParent))
		{
			EmailButton.SetActive(true);
			((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FriendsListPermissionsWarning);
		}
	}
}
