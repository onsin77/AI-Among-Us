using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoTextBox : MonoBehaviour
{
	public SpriteRenderer background;

	public bool isConfirmWindow;

	public bool AutoSetOneButton = true;

	public TextMeshPro titleTexxt;

	public TextMeshPro bodyText;

	public TextMeshPro button1Text;

	public TextMeshPro button2Text;

	public PassiveButton button1;

	public PassiveButton button2;

	public Transform button1Trans;

	public Transform button2Trans;

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	public void Awake()
	{
		if (AutoSetOneButton)
		{
			SetOneButton();
		}
		if (isConfirmWindow)
		{
			SetConfirmWindow();
		}
	}

	public void Close()
	{
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	public void SetConfirmWindow()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		background.color = Color.green;
	}

	public void SetTwoButtons()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		((Component)button2Trans).gameObject.SetActive(true);
		button1Trans.localPosition = Vector2.op_Implicit(new Vector2(2f, button1Trans.localPosition.y));
		button2Trans.localPosition = Vector2.op_Implicit(new Vector2(-2f, button2Trans.localPosition.y));
	}

	public void SetOneButton()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)button2Trans).gameObject.SetActive(false);
		button1Trans.localPosition = Vector2.op_Implicit(new Vector2(0f, button1Trans.localPosition.y));
	}

	public void Update()
	{
		if (!ControllerManager.Instance.IsMenuActiveAtAll(((Object)((Component)this).gameObject).name))
		{
			ControllerNavMenu component = ((Component)this).GetComponent<ControllerNavMenu>();
			Debug.Log((object)"EOS: InfoTextBox overlay menu wasn't active, opening it...");
			component.OpenMenu(force: true);
		}
	}

	public void SetText(string text)
	{
		((TMP_Text)bodyText).text = text;
		SelectableHyperLinkHelper.AddSelectableUiForHyperlinks(selectableHyperLinks, ((Object)this).name);
		OnTextUpdated();
	}

	public void OnTextUpdated()
	{
		((TMP_Text)bodyText).text = SelectableHyperLinkHelper.DecomposeAnnouncementText(bodyText, selectableHyperLinks, ((Object)this).name, ((TMP_Text)bodyText).text);
		SelectableHyperLinkHelper.UpdateHyperlinkPositions(bodyText, selectableHyperLinks, ((Object)this).name);
	}
}
