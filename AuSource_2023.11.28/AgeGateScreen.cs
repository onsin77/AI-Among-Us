using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgeGateScreen : MonoBehaviour
{
	[SerializeField]
	private GameObject EnterDateScreen;

	[SerializeField]
	private GameObject MoreInfoScreen;

	[SerializeField]
	private GameObject VerifyScreen;

	[SerializeField]
	private Color DefaultColor;

	[SerializeField]
	private Color FilledColor;

	[SerializeField]
	private TextMeshPro privacyInfoText;

	[SerializeField]
	private TextMeshPro monthText;

	[SerializeField]
	private TextMeshPro dayText;

	[SerializeField]
	private TextMeshPro yearText;

	[SerializeField]
	private TextMeshPro fullDateText;

	private List<SelectableHyperLink> selectableHyperLinks = new List<SelectableHyperLink>();

	private string defaultMonthText;

	private string defaultDayText;

	private string defaultYearText;

	public int Day;

	public int Month;

	public int Year;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultSelection;

	public List<UiElement> selectableObjects;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void OnEnable()
	{
		SelectableHyperLinkHelper.AddSelectableUiForHyperlinks(selectableHyperLinks, ((Object)this).name);
	}

	public IEnumerator Show()
	{
		DateTime dateOfBirth = DataManager.Player.Age.DateOfBirth;
		Year = dateOfBirth.Year;
		Month = dateOfBirth.Month;
		Day = dateOfBirth.Day;
		defaultMonthText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Month);
		defaultDayText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Day);
		defaultYearText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Year);
		((TMP_Text)monthText).text = defaultMonthText;
		((TMP_Text)dayText).text = defaultDayText;
		((TMP_Text)yearText).text = defaultYearText;
		((Component)this).gameObject.SetActive(true);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultSelection, selectableObjects);
		while (((Component)this).gameObject.activeSelf)
		{
			yield return null;
		}
	}

	public bool IsDefaultDayText(string text)
	{
		return text == defaultDayText;
	}

	private void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)monthText).color = ((((TMP_Text)monthText).text == defaultMonthText) ? DefaultColor : FilledColor);
		((Graphic)dayText).color = ((((TMP_Text)dayText).text == defaultDayText) ? DefaultColor : FilledColor);
		((Graphic)yearText).color = ((((TMP_Text)yearText).text == defaultYearText) ? DefaultColor : FilledColor);
	}

	public void Close()
	{
		if (!ShakeIfInvalid())
		{
			DataManager.Player.Age.LatestSetDate = DateTime.UtcNow;
			DataManager.Player.Age.DateOfBirth = new DateTime(Year, Month, Day);
			DataManager.Player.Save();
			((Component)this).GetComponent<TransitionOpen>().Close();
		}
	}

	public void GoToVerify()
	{
		if (!ShakeIfInvalid())
		{
			((TMP_Text)fullDateText).text = new DateTime(Year, Month, Day).ToString(DestroyableSingleton<TranslationController>.Instance.dateFormats[DataManager.Settings.Language.CurrentLanguage], CultureInfo.InvariantCulture);
			EnterDateScreen.SetActive(false);
			VerifyScreen.SetActive(true);
		}
	}

	public void GoBackToEdit()
	{
		EnterDateScreen.SetActive(true);
		VerifyScreen.SetActive(false);
	}

	public void ToggleMoreInfo()
	{
		SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, ((Object)this).name);
		MoreInfoScreen.SetActive(!MoreInfoScreen.activeSelf);
		EnterDateScreen.SetActive(!EnterDateScreen.activeSelf);
		if (MoreInfoScreen.activeSelf)
		{
			((TMP_Text)privacyInfoText).text = SelectableHyperLinkHelper.DecomposeAnnouncementText(privacyInfoText, selectableHyperLinks, ((Object)this).name, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AgeVerificationMoreInfo));
			SelectableHyperLinkHelper.UpdateHyperlinkPositions(privacyInfoText, selectableHyperLinks, ((Object)this).name);
		}
	}

	public bool ShakeIfInvalid()
	{
		if (((TMP_Text)yearText).text == DateTime.UtcNow.Year.ToString() || ((TMP_Text)monthText).text == DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Month) || ((TMP_Text)dayText).text == DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Day) || ((TMP_Text)yearText).text == DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Year))
		{
			((MonoBehaviour)this).StartCoroutine(Effects.SwayX(((Component)this).transform));
			return true;
		}
		return false;
	}

	private void OnDestroy()
	{
		SelectableHyperLinkHelper.DestroyGOs(selectableHyperLinks, ((Object)this).name);
	}
}
