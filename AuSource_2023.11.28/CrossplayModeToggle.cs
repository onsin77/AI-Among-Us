using AmongUs.Data;
using TMPro;
using UnityEngine;

public class CrossplayModeToggle : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro CrossplayModeText;

	[SerializeField]
	private CreateOptionsPicker OptionsPicker;

	public void OnEnable()
	{
		UpdateDisplay();
	}

	public void CycleMode()
	{
		DataManager.Settings.Multiplayer.CrossPlay = !DataManager.Settings.Multiplayer.CrossPlay;
		DataManager.Settings.Save();
		UpdateDisplay();
		if ((Object)(object)OptionsPicker != (Object)null)
		{
			OptionsPicker.Refresh();
		}
	}

	public void UpdateDisplay()
	{
		((TMP_Text)CrossplayModeText).text = (DataManager.Settings.Multiplayer.CrossPlay ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrossPlayAllPlatforms) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrossPlaySamePlatform));
	}
}
