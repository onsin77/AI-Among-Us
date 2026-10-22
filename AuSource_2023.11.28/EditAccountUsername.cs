using System.Collections.Generic;
using AmongUs.Data;
using Assets.InnerNet;
using TMPro;
using UnityEngine;

public class EditAccountUsername : MonoBehaviour
{
	public TextMeshPro UsernameText;

	public TextMeshPro SuccessText;

	public GameObject SuccessMessage;

	public GameObject PopUp;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, DefaultButtonSelected, ControllerSelectable);
	}

	public virtual void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void SaveUsername()
	{
		if (string.IsNullOrWhiteSpace(((TMP_Text)UsernameText).text))
		{
			return;
		}
		DestroyableSingleton<FriendsListManager>.Instance.SetFriendCode(((TMP_Text)UsernameText).text, (ResponseState cb, Response<ResponseFriendCode> response) =>
		{
			if (cb == ResponseState.Success)
			{
				PopUp.SetActive(false);
				SuccessMessage.SetActive(true);
				string friendCode = response.Data.Attributes.Username + "#" + response.Data.Attributes.Discriminator;
				DestroyableSingleton<EOSManager>.Instance.FriendCode = friendCode;
				DataManager.Settings.Multiplayer.AllowFriendInvites = true;
				DataManager.Settings.Save();
			}
		});
	}

	public void RandomizeName()
	{
		((TMP_Text)UsernameText).SetText(DestroyableSingleton<AccountManager>.Instance.GetRandomName(), true);
	}

	public void Close()
	{
		PopUp.SetActive(true);
		SuccessMessage.SetActive(false);
		((Component)this).gameObject.SetActive(false);
	}
}
