using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using TMPro;
using UnityEngine;

public class DeleteAccountPopupBox : MonoBehaviour
{
	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	public SpriteRenderer background;

	public TextMeshPro titleTexxt;

	public TextMeshPro bodyText;

	public TextMeshPro button1Text;

	public TextMeshPro button2Text;

	public PassiveButton button1;

	public PassiveButton button2;

	public Checkbox checkbox;

	public TextMeshPro checkboxText;

	public TextBoxTMP deleteConfirm;

	public void Close()
	{
		checkbox.IsChecked = false;
		deleteConfirm.SetText("");
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	private void OnEnable()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected Obj, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected Obj, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Invalid comparison between Unknown and I4
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Invalid comparison between Unknown and I4
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Invalid comparison between Unknown and I4
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Confirm), true);
		((TMP_Text)button2Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Cancel), true);
		((TMP_Text)titleTexxt).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DeleteAccount), true);
		((TMP_Text)bodyText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AccountDeleteHelp);
		button1.SetButtonEnableState(enabled: false);
		string text = "";
		uint productUserExternalAccountCount = DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetConnectInterface().GetProductUserExternalAccountCount(new GetProductUserExternalAccountCountOptions
		{
			TargetUserId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId)
		});
		ExternalAccountInfo val2 = default;
		for (uint num = 0u; num < productUserExternalAccountCount; num++)
		{
			CopyProductUserExternalAccountByIndexOptions val = new CopyProductUserExternalAccountByIndexOptions
			{
				TargetUserId = ProductUserId.FromString(DestroyableSingleton<EOSManager>.Instance.ProductUserId),
				ExternalAccountInfoIndex = num
			};
			if ((int)DestroyableSingleton<EOSManager>.Instance.PlatformInterface.GetConnectInterface().CopyProductUserExternalAccountByIndex(val, ref val2) != 0)
			{
				continue;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			ExternalAccountType accountIdType = val2.AccountIdType;
			if ((int)accountIdType <= 3)
			{
				if ((int)accountIdType == 0)
				{
					text += "Epic Games";
					continue;
				}
				if ((int)accountIdType == 3)
				{
					text += "Xbox Live";
					continue;
				}
			}
			else
			{
				if ((int)accountIdType == 10)
				{
					text += "Google Play";
					continue;
				}
				if ((int)accountIdType == 12)
				{
					text += "itch.io";
					continue;
				}
			}
			text += ((object)val2.AccountIdType/*cast due to constrained. prefix*/).ToString();
		}
		if (!string.IsNullOrEmpty(text))
		{
			((TMP_Text)checkboxText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ConfirmDeleteAccounts, text), true);
		}
		else
		{
			((TMP_Text)checkboxText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ConfirmDeleteAccountsEmpty, text), true);
		}
	}

	private void Update()
	{
		if (!ControllerManager.Instance.IsMenuActiveAtAll(((Object)((Component)this).gameObject).name))
		{
			ControllerNavMenu component = ((Component)this).GetComponent<ControllerNavMenu>();
			Logger.GlobalInstance.Info("EOS: DeleteAccountPopupBox overlay menu wasn't active, opening it...");
			component.OpenMenu(force: true);
		}
		button1.SetButtonEnableState(checkbox.IsChecked && deleteConfirm.text.Equals("delete", StringComparison.OrdinalIgnoreCase));
	}
}
