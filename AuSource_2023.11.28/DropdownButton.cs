using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DropdownButton : MonoBehaviour
{
	[SerializeField]
	private float buttonStartY;

	[SerializeField]
	private float buttonHeight;

	[SerializeField]
	private GameObject optionsScreen;

	[SerializeField]
	private PassiveButton openOptionsButton;

	[SerializeField]
	private PassiveButton closeButton;

	[SerializeField]
	private TextMeshPro buttonText;

	[SerializeField]
	private Collider2D clickMask;

	[SerializeField]
	private ObjectPoolBehavior optionPool;

	private List<string> options = new List<string>();

	private Action<int> onOptionSelect;

	public int SelectedIndex { get; private set; }

	private string DropdownName => ((Object)this).name + "DropdownOptionsMenu";

	public void AddOption(string optionName)
	{
		if (options.Count == 0)
		{
			((TMP_Text)buttonText).text = optionName;
		}
		options.Add(optionName);
	}

	public void SetSelectedIndex(int i)
	{
		if (i >= 0 && i < options.Count)
		{
			SelectedIndex = i;
			((TMP_Text)buttonText).text = options[i];
		}
	}

	public void SetOnOptionSelect(Action<int> onOptionSelect)
	{
		this.onOptionSelect = onOptionSelect;
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		((UnityEvent)openOptionsButton.OnClick).AddListener((UnityAction)OpenDropdownOptions);
		((UnityEvent)closeButton.OnClick).AddListener((UnityAction)Close);
	}

	private void OpenDropdownOptions()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (options.Count == 0)
		{
			Debug.LogError((object)("Dropdown button " + ((Object)this).name + " had no options"));
			return;
		}
		if (optionsScreen.activeInHierarchy)
		{
			Debug.LogError((object)"Tried to open options screen when it was already open.");
			return;
		}
		Vector3 position = new Vector3(0f, buttonStartY, 0f);
		List<UiElement> list = new List<UiElement>();
		for (int i = 0; i < options.Count; i++)
		{
			int buttonIndex = i;
			string text = options[i];
			DropdownOption dropdownOption = optionPool.Get<DropdownOption>();
			dropdownOption.Initialize(text, () =>
			{
				SelectOption(buttonIndex);
			}, clickMask, position);
			list.Add(dropdownOption.ButtonUiElement);
			position.y -= buttonHeight;
		}
		optionsScreen.SetActive(true);
		Vector3 localPosition = ((Component)this).transform.localPosition;
		optionsScreen.transform.localPosition = new Vector3(0f - localPosition.x, 0f - localPosition.y, optionsScreen.transform.localPosition.z);
		ControllerManager.Instance.OpenOverlayMenu(DropdownName, closeButton, list[SelectedIndex], list);
	}

	private void SelectOption(int buttonIndex)
	{
		Close();
		SetSelectedIndex(buttonIndex);
		onOptionSelect(buttonIndex);
	}

	private void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(DropdownName);
		optionsScreen.SetActive(false);
		optionPool.ReclaimAll();
	}
}
