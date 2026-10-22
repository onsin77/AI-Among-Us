using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ChatLanguageMenu : MonoBehaviour
{
	public CreateOptionsPicker Parent;

	public ObjectPoolBehavior ButtonPool;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		defaultButtonSelected = null;
		controllerSelectable = new List<UiElement>();
	}

	public void OnEnable()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected Obj, but got Unknown
		uint keywords = (uint)Parent.GetTargetOptions().Keywords;
		int num = ChatLanguageSet.Instance.Languages.Count / 10;
		if (ChatLanguageSet.Instance.Languages.Count != 10)
		{
			num++;
		}
		float num2 = ((float)num / 2f - 0.5f) * -2.5f;
		controllerSelectable.Clear();
		int num3 = 0;
		foreach (KeyValuePair<string, uint> language in ChatLanguageSet.Instance.Languages)
		{
			uint lang = language.Value;
			ChatLanguageButton chatLanguageButton = ButtonPool.Get<ChatLanguageButton>();
			((Component)chatLanguageButton).transform.localPosition = new Vector3(num2 + (float)(num3 / 10) * 2.5f, 2f - (float)(num3 % 10) * 0.5f, 0f);
			if (language.Key == "Other")
			{
				((TMP_Text)chatLanguageButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherLanguage);
			}
			else
			{
				((TMP_Text)chatLanguageButton.Text).text = language.Key;
			}
			((UnityEventBase)chatLanguageButton.Button.OnClick).RemoveAllListeners();
			((UnityEvent)chatLanguageButton.Button.OnClick).AddListener((UnityAction)(() =>
			{
				ChooseOption(lang);
			}));
			chatLanguageButton.SetSelected(language.Value == keywords);
			controllerSelectable.Add(chatLanguageButton.Button);
			if (language.Value == keywords)
			{
				defaultButtonSelected = chatLanguageButton.Button;
			}
			num3++;
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, defaultButtonSelected, controllerSelectable);
	}

	public void OnDisable()
	{
		ButtonPool.ReclaimAll();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void ChooseOption(uint language)
	{
		Parent.SetLanguageFilter(language);
		Close();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
