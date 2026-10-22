using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FriendsListConfirmMenu : MonoBehaviour
{
	private enum ConfirmationStatus
	{
		Waiting,
		Yes,
		No
	}

	public enum ActionType
	{
		Friend,
		Block,
		Lobby,
		Unfriend,
		Unblock
	}

	public TextMeshPro PopUpText;

	private ConfirmationStatus confirmStatus;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public IEnumerator WaitForConfirm(Action cb, ActionType type, string recipientFriendCode)
	{
		switch (type)
		{
		case ActionType.Block:
			((TMP_Text)PopUpText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BlockPlayerConfirm, recipientFriendCode);
			break;
		case ActionType.Friend:
			((TMP_Text)PopUpText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AddFriendConfirm, recipientFriendCode);
			break;
		case ActionType.Lobby:
			((TMP_Text)PopUpText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.InviteToLobbyConfirm, recipientFriendCode);
			break;
		case ActionType.Unfriend:
			((TMP_Text)PopUpText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UnfriendConfirm, recipientFriendCode);
			break;
		case ActionType.Unblock:
			((TMP_Text)PopUpText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.UnblockConfirm, recipientFriendCode);
			break;
		}
		((Component)this).gameObject.SetActive(true);
		confirmStatus = ConfirmationStatus.Waiting;
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
		while (confirmStatus == ConfirmationStatus.Waiting)
		{
			yield return null;
		}
		if (confirmStatus == ConfirmationStatus.Yes)
		{
			cb();
		}
		((Component)this).gameObject.SetActive(false);
	}

	public void Confirm()
	{
		confirmStatus = ConfirmationStatus.Yes;
	}

	public void Cancel()
	{
		confirmStatus = ConfirmationStatus.No;
	}

	public virtual void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}
}
