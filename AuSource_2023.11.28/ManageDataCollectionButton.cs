using System;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;

public class ManageDataCollectionButton : MonoBehaviour
{
	public GenericPopup PopupPrefab;

	public void ManageData()
	{
		DataPrivacy.FetchPrivacyUrl((Action<string>)Application.OpenURL, (Action<string>)ShowPopup);
	}

	private void ShowPopup(string error)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)PopupPrefab))
		{
			GenericPopup genericPopup = Object.Instantiate<GenericPopup>(PopupPrefab);
			((TMP_Text)genericPopup.TextAreaTMP).text = error;
			((Component)genericPopup).transform.SetWorldZ(((Component)this).transform.position.z - 1f);
		}
	}
}
