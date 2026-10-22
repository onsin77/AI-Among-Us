using AmongUs.Data;
using TMPro;
using UnityEngine;

public class SettingsLanguageMenu : MonoBehaviour
{
	public TextMeshPro selectedLangText;

	public void Awake()
	{
		((TMP_Text)selectedLangText).text = DestroyableSingleton<TranslationController>.Instance.Languages[DataManager.Settings.Language.CurrentLanguage].Name;
	}
}
