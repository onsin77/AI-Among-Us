using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FilterTagsMenu : MonoBehaviour
{
	[SerializeField]
	private CreateOptionsPicker Parent;

	[SerializeField]
	private ObjectPoolBehavior ButtonPool;

	[SerializeField]
	private TextMeshPro ButtonText;

	[SerializeField]
	private GameObject Content;

	[Header("Console Controller Navigation")]
	[SerializeField]
	private UiElement BackButton;

	private List<UiElement> controllerSelectable;

	private GameFilterOptions targetOpts;

	public void Open()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected Obj, but got Unknown
		Content.gameObject.SetActive(true);
		GameFilterOptions validGameFilterOptions = DataManager.Settings.Multiplayer.ValidGameFilterOptions;
		float num = ((float)Mathf.CeilToInt((float)validGameFilterOptions.FilterTags.Count / 10f) / 2f - 0.5f) * -2.5f;
		controllerSelectable = new List<UiElement>();
		int num2 = 0;
		foreach (string entry in validGameFilterOptions.FilterTags)
		{
			ChatLanguageButton button = ButtonPool.Get<ChatLanguageButton>();
			((Component)button).transform.localPosition = new Vector3(num + (float)(num2 / 10) * 2.5f, 2f - (float)(num2 % 10) * 0.5f, 0f);
			((TMP_Text)button.Text).text = entry.ToString();
			((UnityEventBase)button.Button.OnClick).RemoveAllListeners();
			((UnityEvent)button.Button.OnClick).AddListener((UnityAction)(() =>
			{
				ChooseOption(button, entry);
			}));
			button.SetSelected(targetOpts.FilterTags.Contains(entry));
			controllerSelectable.Add(button.Button);
			num2++;
		}
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, controllerSelectable[0], controllerSelectable);
	}

	public void ChooseOption(ChatLanguageButton button, string filter)
	{
		if (targetOpts.FilterTags.Contains(filter) && targetOpts.FilterTags.Count > 1)
		{
			targetOpts.FilterTags.Remove(filter);
			button.SetSelected(selected: false);
		}
		else
		{
			targetOpts.FilterTags.Add(filter);
			button.SetSelected(selected: true);
		}
		UpdateButtonText();
	}

	public void Close()
	{
		DataManager.Settings.ForceSave();
		ButtonPool.ReclaimAll();
		Content.gameObject.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void Start()
	{
		DestroyableSingleton<FilterTagManager>.Instance.RefreshTags();
		if (Parent.mode == SettingsMode.Host)
		{
			targetOpts = DataManager.Settings.Multiplayer.HostGameFilterOptions;
		}
		else
		{
			targetOpts = DataManager.Settings.Multiplayer.SearchGameFilterOptions;
		}
		UpdateButtonText();
	}

	private void UpdateButtonText()
	{
		if (Parent.mode == SettingsMode.Host)
		{
			targetOpts = DataManager.Settings.Multiplayer.HostGameFilterOptions;
		}
		else
		{
			targetOpts = DataManager.Settings.Multiplayer.SearchGameFilterOptions;
		}
		switch (targetOpts.FilterTags.Count)
		{
		case 0:
			((TMP_Text)ButtonText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoneLabel);
			return;
		case 1:
			((TMP_Text)ButtonText).text = targetOpts.FilterTags.First();
			return;
		}
		string text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.More, targetOpts.FilterTags.Count - 1);
		((TMP_Text)ButtonText).text = targetOpts.FilterTags.First() + " + " + text.ToLowerInvariant();
	}
}
