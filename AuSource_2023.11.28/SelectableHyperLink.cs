using System;
using UnityEngine;

internal class SelectableHyperLink : MonoBehaviour, ITextPart
{
	public SelectableHyperLink parentHyperlink;

	[SerializeField]
	private UiElement HyperLink;

	[SerializeField]
	private string selectedColor;

	[SerializeField]
	private string unselectedColor;

	private Action onNewSelection;

	private string hyperLinkText;

	private string hyperLinkUrl;

	private string fullHyperLinkText;

	public string LinkText
	{
		get
		{
			return fullHyperLinkText;
		}
		set
		{
			fullHyperLinkText = value;
			SelectableHyperLinkHelper.ExtractUrl(fullHyperLinkText, out hyperLinkUrl, out hyperLinkText);
			if (fullHyperLinkText.IndexOf("<color>") < 0)
			{
				fullHyperLinkText = "<color=#" + unselectedColor + "><link=\"" + hyperLinkUrl + "\">" + hyperLinkText + "</link></color> ";
			}
			else
			{
				fullHyperLinkText = "<link=\"" + hyperLinkUrl + "\">" + hyperLinkText + "</link> ";
			}
		}
	}

	public string TaglessLinkText => hyperLinkText;

	public UiElement SelectableObject => HyperLink;

	public void SetSelectionCallback(Action _onNewSelection)
	{
		onNewSelection = _onNewSelection;
	}

	public string GetText()
	{
		return LinkText;
	}

	public void RemoveUiElement(ControllerUiElementsState menu)
	{
		ControllerManager.Instance.RemoveSelectableUiElement(HyperLink, menu);
	}

	public void AddUiElement(ControllerUiElementsState menu)
	{
		ControllerManager.Instance.AddSelectableUiElement(HyperLink, menu);
	}

	public void OnSelection()
	{
		if (Object.op_Implicit((Object)(object)parentHyperlink))
		{
			parentHyperlink.OnSelection();
			return;
		}
		fullHyperLinkText = fullHyperLinkText.Replace(unselectedColor, selectedColor);
		onNewSelection?.Invoke();
	}

	public void OnUnselection()
	{
		if (Object.op_Implicit((Object)(object)parentHyperlink))
		{
			parentHyperlink.OnUnselection();
			return;
		}
		fullHyperLinkText = fullHyperLinkText.Replace(selectedColor, unselectedColor);
		onNewSelection?.Invoke();
	}

	public void Clicked()
	{
		Debug.LogError((object)("SelectableHyperLink.Clicked loading url:" + hyperLinkUrl));
		if (Object.op_Implicit((Object)(object)parentHyperlink))
		{
			Constants.OpenURL(parentHyperlink.hyperLinkUrl);
		}
		else
		{
			Constants.OpenURL(hyperLinkUrl);
		}
	}
}
