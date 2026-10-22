using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;

public class CycleButtonBehaviour : MonoBehaviour, ITranslatedText
{
	public StringNames[] options;

	public StringNames BaseText;

	public TextMeshPro Text;

	public SpriteRenderer Background;

	public ButtonRolloverHandler Rollover;

	public GameObject helpButton;

	private int curSelection;

	public TextMeshPro quickChatInstructions;

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
		if ((Object)(object)helpButton != (Object)null)
		{
			helpButton.SetActive(DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly);
		}
		((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(options[curSelection]);
	}

	public void UpdateText(int selection)
	{
		curSelection = selection;
		ResetText();
		if (DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
		{
			((TMP_Text)quickChatInstructions).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsStart) + " ";
			if (DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.LoggedIn && DestroyableSingleton<EOSManager>.Instance.IsMinorOrWaiting() && !DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
			{
				TextMeshPro val = quickChatInstructions;
				((TMP_Text)val).text = ((TMP_Text)val).text + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsChild);
			}
			TextMeshPro val2 = quickChatInstructions;
			((TMP_Text)val2).text = ((TMP_Text)val2).text + "\n\n" + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.QuickChatInstructionsFull);
		}
	}
}
