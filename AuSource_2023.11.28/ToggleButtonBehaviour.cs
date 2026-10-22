using TMPro;
using UnityEngine;

public class ToggleButtonBehaviour : MonoBehaviour, ITranslatedText
{
	public StringNames BaseText;

	public TextMeshPro Text;

	public SpriteRenderer Background;

	public ButtonRolloverHandler Rollover;

	private bool onState;

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
		}
	}

	public void ResetText()
	{
		((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(BaseText) + ": " + DestroyableSingleton<TranslationController>.Instance.GetString(onState ? StringNames.SettingsOn : StringNames.SettingsOff);
	}

	public void UpdateText(bool on)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		onState = on;
		Color color = (Color)(on ? new Color(0f, 1f, 42f / 255f, 1f) : Color.white);
		Background.color = color;
		ResetText();
		if (Object.op_Implicit((Object)(object)Rollover))
		{
			Rollover.ChangeOutColor(color);
		}
	}
}
