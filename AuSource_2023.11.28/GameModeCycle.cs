using AmongUs.Data;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class GameModeCycle : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro GameModeText;

	[SerializeField]
	private CreateOptionsPicker OptionsPicker;

	public void CycleGameMode()
	{
		if (DataManager.Settings.Multiplayer.LastPlayedGameMode == GameModes.Normal)
		{
			SetGameMode(GameModes.HideNSeek);
		}
		else
		{
			SetGameMode(GameModes.Normal);
		}
	}

	public void SetGameMode(GameModes mode)
	{
		DataManager.Settings.Multiplayer.LastPlayedGameMode = mode;
		DataManager.Settings.Save();
		GameOptionsManager.Instance.SwitchGameMode(mode);
		((TMP_Text)GameModeText).text = DestroyableSingleton<TranslationController>.Instance.GetString(GameModesHelpers.ModeToName[mode]);
		OptionsPicker.Refresh();
	}

	private void Awake()
	{
		if (DataManager.Settings.Multiplayer.LastPlayedGameMode == GameModes.None)
		{
			SetGameMode(GameModes.Normal);
		}
		else
		{
			SetGameMode(DataManager.Settings.Multiplayer.LastPlayedGameMode);
		}
	}
}
