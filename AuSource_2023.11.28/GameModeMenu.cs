using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameModeMenu : MonoBehaviour
{
	public CreateOptionsPicker Parent;

	public ObjectPoolBehavior ButtonPool;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		defaultButtonSelected = null;
		controllerSelectable = new List<UiElement>();
	}

	public void OnEnable()
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected Obj, but got Unknown
		uint gameMode = (uint)Parent.GetTargetOptions().GameMode;
		float num = ((float)Mathf.CeilToInt((float)Enum.GetNames(typeof(GameModes)).Length / 10f) / 2f - 0.5f) * -2.5f;
		controllerSelectable.Clear();
		int num2 = 0;
		foreach (GameModes entry in Enum.GetValues(typeof(GameModes)))
		{
			if (entry != GameModes.None)
			{
				ChatLanguageButton chatLanguageButton = ButtonPool.Get<ChatLanguageButton>();
				((Component)chatLanguageButton).transform.localPosition = new Vector3(num + (float)(num2 / 10) * 2.5f, 2f - (float)(num2 % 10) * 0.5f, 0f);
				((TMP_Text)chatLanguageButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(GameModesHelpers.ModeToName[entry]);
				((UnityEventBase)chatLanguageButton.Button.OnClick).RemoveAllListeners();
				((UnityEvent)chatLanguageButton.Button.OnClick).AddListener((UnityAction)(() =>
				{
					ChooseOption(entry);
				}));
				chatLanguageButton.SetSelected((int)entry == gameMode);
				controllerSelectable.Add(chatLanguageButton.Button);
				if ((int)entry == gameMode)
				{
					defaultButtonSelected = chatLanguageButton.Button;
				}
				num2++;
			}
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, defaultButtonSelected, controllerSelectable);
	}

	public void OnDisable()
	{
		ButtonPool.ReclaimAll();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void ChooseOption(GameModes mode)
	{
		Parent.SetGameMode(mode);
		Close();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
