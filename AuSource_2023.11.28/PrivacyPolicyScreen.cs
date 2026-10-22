using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using TMPro;
using UnityEngine;

public class PrivacyPolicyScreen : MonoBehaviour
{
	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement ManageDataButton;

	public UiElement AcceptButton;

	public OpenHyperlinks DefaultHyperlinkText;

	public OpenHyperlinks PlayStationHyperlinkText;

	public TextMeshPro PlayStationEULAText;

	private List<ITextPart> textConstituents = new List<ITextPart>();

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	public OpenHyperlinks HyperLinkText => DefaultHyperlinkText;

	public void Start()
	{
	}

	private void OnDisable()
	{
		ControllerUiElementsState menu = ControllerManager.Instance.GetMenu(((Object)this).name);
		DestroyGOs(menu);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public IEnumerator Show()
	{
		if (DataManager.Player.Onboarding.LastAcceptedPrivacyPolicyVersion == 4)
		{
			yield break;
		}
		((Component)this).gameObject.SetActive(true);
		List<UiElement> list = new List<UiElement> { ManageDataButton, AcceptButton };
		if (selectableHyperLinks.Count > 0)
		{
			foreach (SelectableHyperLink selectableHyperLink in selectableHyperLinks)
			{
				list.Add(selectableHyperLink.SelectableObject);
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, AcceptButton, list);
		while (((Component)this).gameObject.activeSelf)
		{
			yield return null;
		}
	}

	private void DestroyGOs(ControllerUiElementsState menu)
	{
		SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, ((Object)this).name);
	}

	private void DecomposePrivacyPolicyText(string text)
	{
		ControllerUiElementsState menu = ControllerManager.Instance.GetMenu(((Object)this).name);
		DestroyGOs(menu);
		SelectableHyperLinkHelper.DecomposeHyperlinkText(text, textConstituents, selectableHyperLinks);
		int num = 1;
		foreach (SelectableHyperLink selectableHyperLink in selectableHyperLinks)
		{
			((Component)selectableHyperLink).transform.SetParent(((Component)HyperLinkText).transform, false);
			if (menu != null)
			{
				selectableHyperLink.AddUiElement(menu);
			}
			selectableHyperLink.SetSelectionCallback(OnNewSelection);
			num++;
		}
	}

	private void OnNewSelection()
	{
		RecomposePrivacyPolicyText();
	}

	public void RecomposePrivacyPolicyText()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		HyperLinkText.Text = "";
		foreach (ITextPart textConstituent in textConstituents)
		{
			HyperLinkText.Text += textConstituent.GetText();
		}
		HyperLinkText.UpdateTMPMesh();
		List<string> list = new List<string>();
		foreach (ITextPart textConstituent2 in textConstituents)
		{
			SelectableHyperLink selectableHyperLink = textConstituent2 as SelectableHyperLink;
			if (Object.op_Implicit((Object)(object)selectableHyperLink))
			{
				int lineBreakCount = 0;
				if (HyperLinkText.GetWordPosition(selectableHyperLink.TaglessLinkText, list, out var bottomLeft, out var topRight, out lineBreakCount))
				{
					list.Add(selectableHyperLink.TaglessLinkText);
					Vector3 localPosition = (topRight + bottomLeft) * 0.5f;
					Vector3 localScale = (topRight - bottomLeft) * 1.5f;
					localPosition.z = ((Component)selectableHyperLink).transform.localPosition.z;
					((Component)selectableHyperLink).transform.localPosition = localPosition;
					((Component)selectableHyperLink).transform.localScale = localScale;
				}
				else
				{
					Debug.LogError((object)("Failed to find text '" + selectableHyperLink.TaglessLinkText + "'"));
				}
			}
		}
	}

	public void OnTextUpdated()
	{
		DecomposePrivacyPolicyText(HyperLinkText.Text);
		RecomposePrivacyPolicyText();
	}

	public void Close()
	{
		DataManager.Player.Onboarding.LastAcceptedPrivacyPolicyVersion = 4;
		DataManager.Player.Save();
		((Component)this).GetComponent<TransitionOpen>().Close();
	}
}
