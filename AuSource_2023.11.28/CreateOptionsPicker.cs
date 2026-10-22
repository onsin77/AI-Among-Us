using System.Collections.Generic;
using AmongUs.Data;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreateOptionsPicker : MonoBehaviour
{
	private const float MaxPlayerButtonWidth = 0.5f;

	public SpriteRenderer MaxPlayerButtonPrefab;

	private List<SpriteRenderer> MaxPlayerButtons = new List<SpriteRenderer>();

	public Transform MaxPlayersRoot;

	public MapPickerMenu MapMenu;

	public ImpostorsOptionButton[] ImpostorButtons;

	public TextMeshPro LanguageButton;

	public TextMeshPro GameModeText;

	public TextMeshPro ImpostorText;

	public SettingsMode mode;

	public CrewVisualizer CrewArea;

	public CreateGameOptions optionsMenu;

	public void Awake()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected Obj, but got Unknown
		if (!Object.op_Implicit((Object)(object)MaxPlayersRoot))
		{
			return;
		}
		for (int i = 4; i <= 15; i++)
		{
			SpriteRenderer val = Object.Instantiate<SpriteRenderer>(MaxPlayerButtonPrefab, MaxPlayersRoot);
			((Component)val).transform.localPosition = new Vector3((float)(i - 4) * 0.5f, 0f, 0f);
			int numPlayers = i;
			((Object)val).name = numPlayers.ToString();
			PassiveButton component = ((Component)val).GetComponent<PassiveButton>();
			((UnityEvent)component.OnClick).AddListener((UnityAction)(() =>
			{
				SetMaxPlayersButtons(numPlayers);
			}));
			((TMP_Text)((Component)val).GetComponentInChildren<TextMeshPro>()).text = ((Object)val).name;
			MaxPlayerButtons.Add(val);
			optionsMenu.ControllerSelectable.Add(component);
		}
	}

	public void Start()
	{
		Refresh();
	}

	public void Refresh()
	{
		IGameOptions targetOptions = GetTargetOptions();
		if (targetOptions.TryGetIntArray(Int32ArrayOptionNames.MaxImpostors, out var value))
		{
			if (value[targetOptions.MaxPlayers] < 1)
			{
				SetImpostorButtons(1);
			}
			else if (value[targetOptions.MaxPlayers] > 3)
			{
				SetImpostorButtons(3);
			}
		}
		UpdateImpostorsButtons(targetOptions.NumImpostors);
		UpdateMaxPlayersButtons(targetOptions);
		UpdateLanguageButton((uint)targetOptions.Keywords);
		MapMenu.UpdateMapButtons(targetOptions.MapId);
		((TMP_Text)GameModeText).text = DestroyableSingleton<TranslationController>.Instance.GetString(GameModesHelpers.ModeToName[GameOptionsManager.Instance.CurrentGameOptions.GameMode]);
	}

	public IGameOptions GetTargetOptions()
	{
		if (mode == SettingsMode.Host)
		{
			return GameOptionsManager.Instance.GameHostOptions;
		}
		IGameOptions gameSearchOptions = GameOptionsManager.Instance.GameSearchOptions;
		if (gameSearchOptions.MapId == 0)
		{
			gameSearchOptions.ToggleMapFilter(0);
		}
		return gameSearchOptions;
	}

	private void SetTargetOptions(IGameOptions data)
	{
		if (mode == SettingsMode.Host)
		{
			GameOptionsManager.Instance.GameHostOptions = data;
		}
		else
		{
			GameOptionsManager.Instance.GameSearchOptions = data;
		}
	}

	public void SetMaxPlayersButtons(int maxPlayers)
	{
		IGameOptions targetOptions = GetTargetOptions();
		if (!targetOptions.TryGetIntArray(Int32ArrayOptionNames.MinPlayers, out var value))
		{
			Debug.LogError((object)"CreateOptions picker could not obtain MinPlayers array from game options during SetMaxPlayersButtons");
		}
		else if (maxPlayers >= value[targetOptions.NumImpostors])
		{
			targetOptions.SetInt(Int32OptionNames.MaxPlayers, maxPlayers);
			SetTargetOptions(targetOptions);
			if (DestroyableSingleton<FindAGameManager>.InstanceExists)
			{
				DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
			}
			UpdateMaxPlayersButtons(targetOptions);
		}
	}

	private void UpdateMaxPlayersButtons(IGameOptions opts)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)CrewArea))
		{
			CrewArea.SetCrewSize(opts.MaxPlayers, opts.NumImpostors);
		}
		if (!opts.TryGetIntArray(Int32ArrayOptionNames.MinPlayers, out var value))
		{
			Debug.LogError((object)"CreateOptions picker could not obtain MinPlayers array from game options during UpdateMaxPlayersButtons");
			return;
		}
		for (int i = 0; i < MaxPlayerButtons.Count; i++)
		{
			SpriteRenderer val = MaxPlayerButtons[i];
			((Renderer)val).enabled = ((Object)val).name == opts.MaxPlayers.ToString();
			((Graphic)((Component)val).GetComponentInChildren<TextMeshPro>()).color = ((int.Parse(((Object)val).name) < value[opts.NumImpostors]) ? Palette.DisabledGrey : Color.white);
		}
	}

	public void SetImpostorButtons(int numImpostors)
	{
		IGameOptions targetOptions = GetTargetOptions();
		int[] intArray = targetOptions.GetIntArray(Int32ArrayOptionNames.MaxImpostors);
		if (numImpostors <= intArray[targetOptions.MaxPlayers])
		{
			targetOptions.SetInt(Int32OptionNames.NumImpostors, numImpostors);
			SetTargetOptions(targetOptions);
			if (!targetOptions.TryGetIntArray(Int32ArrayOptionNames.MinPlayers, out var value))
			{
				Debug.LogError((object)"CreateOptions picker could not obtain MinPlayers array from game options during SetImpostorButtons");
				return;
			}
			int num = value[numImpostors];
			SetMaxPlayersButtons(Mathf.Max(targetOptions.MaxPlayers, num));
			UpdateImpostorsButtons(numImpostors);
		}
	}

	private void UpdateImpostorsButtons(int numImpostors)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		IGameOptions targetOptions = GetTargetOptions();
		bool flag = targetOptions.GetIntArray(Int32ArrayOptionNames.MaxImpostors)[targetOptions.MaxPlayers] <= 1;
		for (int i = 0; i < ImpostorButtons.Length; i++)
		{
			ImpostorsOptionButton impostorsOptionButton = ImpostorButtons[i];
			if (Object.op_Implicit((Object)(object)impostorsOptionButton.SpriteRenderer))
			{
				((Renderer)impostorsOptionButton.SpriteRenderer).enabled = ((Object)impostorsOptionButton).name == numImpostors.ToString();
			}
			if (targetOptions.GameMode == GameModes.HideNSeek)
			{
				impostorsOptionButton.SetOptionEnabled(!flag);
			}
			else
			{
				impostorsOptionButton.SetOptionEnabled(b: true);
			}
		}
		((Graphic)ImpostorText).color = ((targetOptions.GameMode == GameModes.HideNSeek) ? Palette.DisabledGrey : Color.white);
	}

	public void SetMap(int mapId)
	{
		IGameOptions targetOptions = GetTargetOptions();
		if (mode == SettingsMode.Host)
		{
			targetOptions.SetByte(ByteOptionNames.MapId, (byte)mapId);
		}
		else
		{
			targetOptions.ToggleMapFilter((byte)mapId);
		}
		SetTargetOptions(targetOptions);
		if (DestroyableSingleton<FindAGameManager>.InstanceExists)
		{
			DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
		}
		MapMenu.UpdateMapButtons(mapId);
	}

	public void SetLanguageFilter(uint keyword)
	{
		IGameOptions targetOptions = GetTargetOptions();
		targetOptions.SetUInt(UInt32OptionNames.Keywords, keyword);
		SetTargetOptions(targetOptions);
		if (DestroyableSingleton<FindAGameManager>.InstanceExists)
		{
			DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
		}
		UpdateLanguageButton(keyword);
	}

	private void UpdateLanguageButton(uint flag)
	{
		if (flag == 1)
		{
			((TMP_Text)LanguageButton).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherLanguage);
		}
		else
		{
			((TMP_Text)LanguageButton).text = ChatLanguageSet.Instance.GetString(flag);
		}
	}

	public void SetGameMode(GameModes mode)
	{
		DataManager.Settings.Multiplayer.LastPlayedGameMode = mode;
		DataManager.Settings.Save();
		GameOptionsManager.Instance.SwitchGameMode(mode);
		((TMP_Text)GameModeText).text = DestroyableSingleton<TranslationController>.Instance.GetString(GameModesHelpers.ModeToName[mode]);
		Refresh();
	}
}
