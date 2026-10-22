using System.Collections;
using TMPro;
using UnityEngine;

public class RestorePurchases : MonoBehaviour
{
	[SerializeField]
	private InfoTextBox errorDisplay;

	private bool isRestoring;

	public void RestoreiOSPurchases()
	{
		if (!isRestoring)
		{
			((MonoBehaviour)this).StartCoroutine(CoRestoreiOSPurchases());
		}
	}

	private IEnumerator CoRestoreiOSPurchases()
	{
		isRestoring = true;
		errorDisplay.SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Processing));
		((Component)errorDisplay).gameObject.SetActive(true);
		errorDisplay.SetOneButton();
		((TMP_Text)errorDisplay.button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Cancel), true);
		DestroyableSingleton<EOSManager>.Instance.validateOldDLC = new CompletionToken<PurchaseStates>(PurchaseStates.NotStarted);
		DestroyableSingleton<StoreManager>.Instance.ProcessOldReceipts();
		yield return DestroyableSingleton<EOSManager>.Instance.validateOldDLC.WaitForCompletion();
		errorDisplay.SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Success));
		((TMP_Text)errorDisplay.button1Text).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Okay), true);
		isRestoring = false;
	}
}
