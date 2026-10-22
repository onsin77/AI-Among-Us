using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BugReportPopup : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField]
	private PassiveButton[] closeButtons;

	[SerializeField]
	private PassiveButton submitButton;

	[Header("Form")]
	[SerializeField]
	private DropdownButton categoryDropdown;

	[SerializeField]
	private TextBoxTMP descriptionField;

	[SerializeField]
	private PassiveButton descriptionFieldButton;

	[SerializeField]
	private Checkbox attachScreenshotCheckbox;

	[SerializeField]
	[Header("Feedback & Validation")]
	private TextMeshPro descriptionCharacterCount;

	[SerializeField]
	private TextMeshPro submittedText;

	[SerializeField]
	[Header("Console Controller Navigation")]
	private UiElement backButton;

	[SerializeField]
	private UiElement defaultButtonSelected;

	[SerializeField]
	private List<UiElement> controllerSelectable;

	private bool descriptionEntered;

	private StringNames selectedCategory;

	private void Start()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected Obj, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected Obj, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected Obj, but got Unknown
		StringNames[] bugReportCategories = BugReportService.BugReportCategories;
		foreach (StringNames id in bugReportCategories)
		{
			categoryDropdown.AddOption(DestroyableSingleton<TranslationController>.Instance.GetString(id));
		}
		categoryDropdown.SetOnOptionSelect(UpdateSelectedCategory);
		((UnityEvent)submitButton.OnClick).AddListener((UnityAction)SubmitReport);
		closeButtons.ForEach((PassiveButton b) =>
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			((UnityEvent)b.OnClick).AddListener((UnityAction)(() =>
			{
				((Component)this).gameObject.SetActive(false);
			}));
		});
		((UnityEvent)descriptionField.OnChange).AddListener((UnityAction)ValidateDescription);
		((UnityEvent)descriptionFieldButton.OnClick).AddListener((UnityAction)FocusDescription);
	}

	private void FocusDescription()
	{
		if (!descriptionEntered)
		{
			descriptionField.Clear();
		}
		descriptionField.GiveFocus();
	}

	private void UpdateSelectedCategory(int selectedIndex)
	{
		if (selectedIndex < 0 || selectedIndex >= BugReportService.BugReportCategories.Length)
		{
			selectedIndex = 0;
			categoryDropdown.SetSelectedIndex(0);
		}
		selectedCategory = BugReportService.BugReportCategories[selectedIndex];
	}

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, backButton, defaultButtonSelected, controllerSelectable);
		submitButton.SetButtonEnableState(enabled: true);
		((Component)submittedText).gameObject.SetActive(false);
		descriptionField.Clear();
		categoryDropdown.SetSelectedIndex(0);
		selectedCategory = BugReportService.BugReportCategories[categoryDropdown.SelectedIndex];
		attachScreenshotCheckbox.IsChecked = false;
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void SubmitReport()
	{
		StringNames bugReportCategory = selectedCategory;
		string descriptionFieldText = descriptionField.text;
		if (descriptionFieldText.Length > descriptionField.characterLimit)
		{
			((MonoBehaviour)this).StartCoroutine(Effects.SwayX(descriptionCharacterCount.transform));
		}
		else if (string.IsNullOrEmpty(descriptionFieldText))
		{
			((MonoBehaviour)this).StartCoroutine(Effects.SwayX(descriptionCharacterCount.transform));
		}
		else if (!BugReportService.Instance.CanSubmitFeedback())
		{
			((TMP_Text)submittedText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BugReportPopUpSubmissionFailedText), true);
		}
		else if (attachScreenshotCheckbox.IsChecked)
		{
			ControllerManager.Instance.CloseAndResetAll();
			DestroyableSingleton<CoroutineHelper>.Instance.DelayBySeconds(0.2f, () =>
			{
				((MonoBehaviour)DestroyableSingleton<CoroutineHelper>.Instance).StartCoroutine(BugReportService.Instance.SubmitUserFeedbackWithScreenshot(bugReportCategory, string.Empty, $"{bugReportCategory}: {descriptionFieldText}"));
			});
		}
		else
		{
			BugReportService.Instance.SubmitUserFeedback(bugReportCategory, string.Empty, $"{bugReportCategory}: {descriptionFieldText}");
			((TMP_Text)submittedText).SetText(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.BugReportPopUpSubmittedText), true);
			submitButton.SetButtonEnableState(enabled: false);
			((Component)submittedText).gameObject.SetActive(true);
		}
	}

	private void ValidateDescription()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		descriptionEntered = true;
		int length = descriptionField.text.Length;
		((TMP_Text)descriptionCharacterCount).text = $"{length}/{descriptionField.characterLimit}";
		if ((double)length < (double)descriptionField.characterLimit * 0.75)
		{
			((Graphic)descriptionCharacterCount).color = Color.white;
		}
		else if (length < descriptionField.characterLimit)
		{
			((Graphic)descriptionCharacterCount).color = new Color(1f, 1f, 0f, 1f);
		}
		else
		{
			((Graphic)descriptionCharacterCount).color = Color.red;
		}
	}
}
