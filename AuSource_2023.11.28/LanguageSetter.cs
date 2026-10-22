using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LanguageSetter : MonoBehaviour
{
	public LanguageButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private LanguageButton[] AllButtons;

	public TextMeshPro parentLangButton;

	private List<UiElement> selectableButtons = new List<UiElement>();

	public UiElement backButton;

	private void OnEnable()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (AllButtons == null)
		{
			return;
		}
		LanguageButton languageButton = null;
		LanguageButton[] allButtons = AllButtons;
		foreach (LanguageButton languageButton2 in allButtons)
		{
			if (((Graphic)languageButton2.Title).color == Color.green)
			{
				languageButton = languageButton2;
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, backButton, languageButton.Button, selectableButtons);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)((Component)this).gameObject).name);
	}

	public void Start()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected Obj, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		TranslatedImageSet[] array = DestroyableSingleton<TranslationController>.Instance.Languages.Values.ToArray();
		Collider2D component = ((Component)ButtonParent).GetComponent<Collider2D>();
		bool flag = false;
		LanguageButton languageButton = null;
		if (AllButtons == null)
		{
			flag = true;
		}
		Vector3 localPosition = new Vector3(0f, ButtonStart, -0.5f);
		AllButtons = new LanguageButton[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			LanguageButton button = Object.Instantiate<LanguageButton>(ButtonPrefab, ButtonParent.Inner);
			AllButtons[i] = button;
			button.Language = array[i];
			((TMP_Text)button.Title).text = array[i].Name;
			if (array[i].languageID == DataManager.Settings.Language.CurrentLanguage)
			{
				languageButton = button;
				((Graphic)button.Title).color = Color.green;
				((TMP_Text)parentLangButton).text = array[i].Name;
			}
			((UnityEvent)button.Button.OnClick).AddListener((UnityAction)(() =>
			{
				SetLanguage(button);
			}));
			button.Button.ClickMask = component;
			((Component)button).transform.localPosition = localPosition;
			localPosition.y -= ButtonHeight;
		}
		if (flag)
		{
			LanguageButton[] allButtons = AllButtons;
			foreach (LanguageButton languageButton2 in allButtons)
			{
				selectableButtons.Add(languageButton2.Button);
			}
			ControllerManager.Instance.OpenOverlayMenu(((Object)((Component)this).gameObject).name, backButton, ((Object)(object)languageButton != (Object)null) ? languageButton.Button : AllButtons[0].Button, selectableButtons);
		}
		ButtonParent.SetBoundsMax((float)array.Length * ButtonHeight - 2f * ButtonStart - 0.1f, 0f);
	}

	public void SetLanguage(LanguageButton selected)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < AllButtons.Length; i++)
		{
			((Graphic)AllButtons[i].Title).color = Color.white;
		}
		((Graphic)selected.Title).color = Color.green;
		((TMP_Text)parentLangButton).text = selected.Language.Name;
		DestroyableSingleton<TranslationController>.Instance.SetLanguage(selected.Language);
		Close();
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
