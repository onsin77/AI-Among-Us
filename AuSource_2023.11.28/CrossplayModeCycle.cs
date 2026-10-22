using AmongUs.Data;
using TMPro;
using UnityEngine;

public class CrossplayModeCycle : MonoBehaviour
{
	public TextMeshPro crossplayModeText;

	public SpriteRenderer backgroundSprite;

	public void OnEnable()
	{
		UpdateCrossplayButtonText();
	}

	public void CycleCrossplayMode()
	{
		DataManager.Settings.Multiplayer.CrossPlay = !DataManager.Settings.Multiplayer.CrossPlay;
		DataManager.Settings.Save();
		UpdateCrossplayButtonText();
	}

	public void UpdateCrossplayButtonText()
	{
		StringNames id = (DataManager.Settings.Multiplayer.CrossPlay ? StringNames.CrossPlayAllPlatforms : StringNames.CrossPlaySamePlatform);
		((TMP_Text)crossplayModeText).text = DestroyableSingleton<TranslationController>.Instance.GetString(id);
	}
}
