using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextTranslatorTMP : MonoBehaviour, ITranslatedText
{
	public StringNames TargetText;

	public string defaultStr;

	public bool ToUpper;

	public bool ResetOnlyWhenNoDefault;

	public UnityEvent OnTranslate;

	public void ResetText()
	{
		if (!ResetOnlyWhenNoDefault || (defaultStr == null && !(defaultStr != "")))
		{
			TextMeshPro component = ((Component)this).GetComponent<TextMeshPro>();
			string text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(TargetText, defaultStr);
			if (ToUpper)
			{
				text = text.ToUpperInvariant();
			}
			if ((Object)(object)component != (Object)null)
			{
				((TMP_Text)component).text = text;
				((TMP_Text)component).ForceMeshUpdate(false, false);
			}
			else
			{
				TextMeshProUGUI component2 = ((Component)this).GetComponent<TextMeshProUGUI>();
				((TMP_Text)component2).text = text;
				((TMP_Text)component2).ForceMeshUpdate(false, false);
			}
			if (OnTranslate != null)
			{
				OnTranslate.Invoke();
			}
		}
	}

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
		ResetText();
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			try
			{
				DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
			}
			catch
			{
			}
		}
	}
}
