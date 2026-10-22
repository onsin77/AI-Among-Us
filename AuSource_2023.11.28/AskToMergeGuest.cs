using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AskToMergeGuest : MonoBehaviour
{
	public PassiveButton GoAheadButton;

	public PassiveButton NotRightNowButton;

	public TextMeshPro InfoText;

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	private bool accountIDShow;

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void Start()
	{
		((TMP_Text)InfoText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MergeGuestAccountText, Constants.GetCurrentPlatformName());
		SelectableHyperLinkHelper.AddSelectableUiForHyperlinks(selectableHyperLinks, ((Object)this).name);
		OnTextUpdated();
	}

	public void OnTextUpdated()
	{
		((TMP_Text)InfoText).text = SelectableHyperLinkHelper.DecomposeAnnouncementText(InfoText, selectableHyperLinks, ((Object)this).name, ((TMP_Text)InfoText).text);
		SelectableHyperLinkHelper.UpdateHyperlinkPositions(InfoText, selectableHyperLinks, ((Object)this).name);
	}
}
