using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class PlatformTextTranslationTMP : MonoBehaviour, ITranslatedText
{
	public StringNames DefaultTargetText;

	public StringNames SwitchTargetText;

	public StringNames XboxTargetText;

	public StringNames PlaystationTargetText;

	public StringNames MobileTargetText;

	public void ResetText()
	{
		TextMeshPro component = ((Component)this).GetComponent<TextMeshPro>();
		((TMP_Text)component).text = DestroyableSingleton<TranslationController>.Instance.GetString(DefaultTargetText);
		((TMP_Text)component).ForceMeshUpdate(false, false);
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
