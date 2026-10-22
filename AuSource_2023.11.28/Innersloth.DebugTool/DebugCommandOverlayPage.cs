using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Innersloth.DebugTool;

public class DebugCommandOverlayPage : DebugOverlayPage
{
	[SerializeField]
	[Header("List")]
	private DebugCommandButton commandButtonPrefab;

	[SerializeField]
	private Transform commandButtonParent;

	[SerializeField]
	private ScrollRect commandScrollList;

	[Header("Details")]
	[SerializeField]
	private GameObject detailsParent;

	[SerializeField]
	private TextMeshProUGUI basePath;

	[SerializeField]
	private TextMeshProUGUI commandName;

	[SerializeField]
	private TextMeshProUGUI commandDescription;

	[SerializeField]
	private Transform argumentsParent;

	[SerializeField]
	private TMP_InputField argumentPrefab;

	[SerializeField]
	private TextMeshProUGUI errorMessage;

	[SerializeField]
	private Button executeButton;

	private List<DebugCommandButton> allCommandButtons;

	private Dictionary<string, List<DebugCommandButton>> directoryContents;

	private Dictionary<string, DebugCommand> commands;

	private List<TMP_InputField> inputFields;

	private int selectedCommandIndex;

	private int selectedInputFieldIndex;

	public override string Name => "Commands";

	public override bool ConsumingNavActions => detailsParent.activeSelf;

	private void Update()
	{
		foreach (IDebugToolInputProvider inputProvider in InputProviders)
		{
			UpdateSelectedCommandIfScrolling(inputProvider);
			if (detailsParent.activeSelf)
			{
				int num = selectedInputFieldIndex;
				if (inputProvider.GetNavRight())
				{
					selectedInputFieldIndex++;
					if (selectedInputFieldIndex >= inputFields.Count)
					{
						selectedInputFieldIndex = 0;
					}
				}
				else if (inputProvider.GetNavLeft())
				{
					selectedInputFieldIndex--;
					if (selectedInputFieldIndex < 0)
					{
						selectedInputFieldIndex = inputFields.Count - 1;
					}
				}
				if (num != selectedInputFieldIndex)
				{
					((Selectable)inputFields[selectedInputFieldIndex]).Select();
				}
				if (inputProvider.GetCancel())
				{
					ResetAndHideDetails();
				}
				if (inputProvider.GetConfirm())
				{
					DebugCommand command = commands[allCommandButtons[selectedCommandIndex].Path];
					ExecuteCommandWithParameters(command);
				}
			}
			else if (inputProvider.GetConfirm())
			{
				OnButtonClick(allCommandButtons[selectedCommandIndex]);
			}
		}
	}

	private void UpdateSelectedCommandIfScrolling(IDebugToolInputProvider inputProvider)
	{
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		int num = selectedCommandIndex;
		float vertical = inputProvider.GetVertical();
		if (vertical < -0.9f)
		{
			selectedCommandIndex = Mathf.Clamp(selectedCommandIndex + 1, 0, allCommandButtons.Count - 1);
		}
		else if (vertical > 0.9f)
		{
			selectedCommandIndex = Mathf.Clamp(selectedCommandIndex - 1, 0, allCommandButtons.Count - 1);
		}
		DebugCommandButton debugCommandButton = allCommandButtons[selectedCommandIndex];
		if (num == selectedCommandIndex)
		{
			return;
		}
		if (num < selectedCommandIndex && !((Component)debugCommandButton).gameObject.activeInHierarchy)
		{
			for (int i = selectedCommandIndex + 1; i < allCommandButtons.Count; i++)
			{
				if (((Component)allCommandButtons[i]).gameObject.activeInHierarchy)
				{
					selectedCommandIndex = i;
					break;
				}
			}
		}
		else if (num > selectedCommandIndex && !((Component)debugCommandButton).gameObject.activeInHierarchy)
		{
			for (int num2 = selectedCommandIndex - 1; num2 >= 0; num2--)
			{
				if (((Component)allCommandButtons[num2]).gameObject.activeInHierarchy)
				{
					selectedCommandIndex = num2;
					break;
				}
			}
		}
		HighlightButton(selectedCommandIndex);
		RectTransform content = commandScrollList.content;
		Vector2 anchoredPosition = content.anchoredPosition;
		float num3 = selectedCommandIndex - num;
		Rect rect = ((RectTransform)((Component)commandButtonPrefab).transform).rect;
		content.anchoredPosition = anchoredPosition + new Vector2(0f, num3 * rect.height);
		ResetAndHideDetails();
	}

	public override void Show()
	{
		List<DebugCommand> list = new List<DebugCommand>(DebugTool.Commands);
		for (int num = commandButtonParent.childCount - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)commandButtonParent.GetChild(num)).gameObject);
		}
		ResetAndHideDetails();
		list.Sort((DebugCommand c1, DebugCommand c2) => string.Compare(c1.CommandPath, c2.CommandPath, StringComparison.Ordinal));
		allCommandButtons = new List<DebugCommandButton>();
		commands = new Dictionary<string, DebugCommand>();
		directoryContents = new Dictionary<string, List<DebugCommandButton>>();
		int num2 = 0;
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			DebugCommand debugCommand = list[num3];
			string[] array = debugCommand.CommandPath.Split('/');
			string[] array2 = new string[array.Length];
			for (int num4 = 0; num4 < array.Length - 1; num4++)
			{
				string text = (array2[num4] = string.Join("/", array, 0, num4 + 1));
				if (!directoryContents.ContainsKey(text))
				{
					directoryContents[text] = new List<DebugCommandButton>();
					string text2 = ((num4 > 0) ? array2[num4 - 1] : string.Empty);
					DebugCommandButton debugCommandButton = CreateDirectory(array[num4], text2, text, num2++);
					allCommandButtons.Add(debugCommandButton);
					debugCommandButton.SetIndentLevel(num4);
					debugCommandButton.ToggleDirectory(directory: true);
					debugCommandButton.Expanded = false;
					if (text2 != string.Empty)
					{
						directoryContents[text2].Add(debugCommandButton);
						((Component)debugCommandButton).gameObject.SetActive(false);
					}
				}
			}
			DebugCommandButton button = Object.Instantiate<DebugCommandButton>(commandButtonPrefab, commandButtonParent);
			allCommandButtons.Add(button);
			string directory = debugCommand.Directory;
			int btnIndex = num2;
			button.Initialize(debugCommand.CommandName, debugCommand.CommandPath, () =>
			{
				selectedCommandIndex = btnIndex;
				HighlightButton(btnIndex);
				OnButtonClick(button);
			}, btnIndex);
			num2++;
			button.SetIndentLevel(directory.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length);
			button.ToggleDirectory(directory: false);
			commands[debugCommand.CommandPath] = debugCommand;
			if (directory.Length > 0)
			{
				directoryContents[directory].Add(button);
				((Component)button).gameObject.SetActive(false);
			}
		}
		selectedCommandIndex = 0;
		HighlightButton(selectedCommandIndex);
	}

	public override void OnBackButtonPressed()
	{
		ResetAndHideDetails();
	}

	private DebugCommandButton CreateDirectory(string directoryName, string parentDirectoryPath, string directoryPath, int index)
	{
		DebugCommandButton directoryButton = Object.Instantiate<DebugCommandButton>(commandButtonPrefab, commandButtonParent);
		directoryButton.Initialize(directoryName, directoryPath, () =>
		{
			selectedCommandIndex = index;
			HighlightButton(selectedCommandIndex);
			OnButtonClick(directoryButton);
		}, index);
		return directoryButton;
	}

	private void OnButtonClick(DebugCommandButton btn)
	{
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected Obj, but got Unknown
		if (btn.IsDirectory)
		{
			List<DebugCommandButton> collection = directoryContents[btn.Path];
			bool flag = !btn.Expanded;
			Queue<DebugCommandButton> queue = new Queue<DebugCommandButton>(collection);
			while (queue.Count > 0)
			{
				DebugCommandButton debugCommandButton = queue.Dequeue();
				((Component)debugCommandButton).gameObject.SetActive(flag);
				if (flag || !debugCommandButton.IsDirectory || !debugCommandButton.Expanded)
				{
					continue;
				}
				foreach (DebugCommandButton item in directoryContents[debugCommandButton.Path])
				{
					queue.Enqueue(item);
				}
				debugCommandButton.Expanded = false;
			}
			btn.Expanded = flag;
			return;
		}
		DebugCommand command = commands[btn.Path];
		ParameterInfo[] parameters = command.ActionInfo.GetParameters();
		if (parameters.Length == 0)
		{
			if (command.CloseBeforeExecute)
			{
				DebugUIEvents.OnBeforeCommandExecuted(command);
			}
			ResetAndHideDetails();
			command.ActionInfo.Invoke(command.Target, Array.Empty<object>());
			return;
		}
		ResetAndHideDetails();
		detailsParent.gameObject.SetActive(true);
		((TMP_Text)basePath).SetText(command.Directory, true);
		((TMP_Text)commandName).SetText(command.CommandName, true);
		((TMP_Text)commandDescription).SetText(command.Description, true);
		inputFields = new List<TMP_InputField>();
		selectedInputFieldIndex = 0;
		for (int i = 0; i < parameters.Length; i++)
		{
			TMP_InputField input = Object.Instantiate<TMP_InputField>(argumentPrefab, argumentsParent);
			((TMP_Text)((Component)((Component)input).transform.GetChild(0).Find("Placeholder")).GetComponent<TextMeshProUGUI>()).SetText(command.GetArgumentPlaceholder(i), true);
			inputFields.Add(input);
			((UnityEvent<string>)(object)input.onSelect).AddListener((UnityAction<string>)((string _) =>
			{
				KeyboardInputProvider.GetText(input.text, input.characterLimit, (string result) =>
				{
					input.text = result;
				});
			}));
			if (i == 0)
			{
				((Selectable)input).Select();
			}
		}
		((UnityEventBase)executeButton.onClick).RemoveAllListeners();
		((UnityEvent)executeButton.onClick).AddListener((UnityAction)(() =>
		{
			ExecuteCommandWithParameters(command);
		}));
		DebugUIEvents.OnDetailsToggledVisible(obj: true);
	}

	private void ExecuteCommandWithParameters(DebugCommand command)
	{
		ParameterInfo[] parameters = command.ActionInfo.GetParameters();
		object[] array = new object[parameters.Length];
		List<string> list = new List<string>();
		for (int i = 0; i < parameters.Length; i++)
		{
			string rawValue = inputFields[i].text.Trim();
			try
			{
				array[i] = DebugTool.ParseInputToType(rawValue, parameters[i].Name, parameters[i].ParameterType);
			}
			catch (Exception ex)
			{
				list.Add(ex.Message);
			}
		}
		if (list.Count > 0)
		{
			((TMP_Text)errorMessage).text = "Unexpected error(s) occurred:\n" + string.Join("\n", list);
			return;
		}
		if (command.CloseBeforeExecute)
		{
			DebugUIEvents.OnBeforeCommandExecuted(command);
		}
		try
		{
			command.ActionInfo.Invoke(command.Target, array);
			((TMP_Text)errorMessage).text = string.Empty;
		}
		catch (TargetInvocationException ex2)
		{
			if (command.CloseBeforeExecute)
			{
				throw ex2.InnerException ?? ex2;
			}
			((TMP_Text)errorMessage).text = "Unexpected error occurred: " + ((ex2.InnerException == null) ? ex2.Message : ex2.InnerException.Message);
		}
		catch (Exception ex3)
		{
			if (command.CloseBeforeExecute)
			{
				throw;
			}
			((TMP_Text)errorMessage).text = "Unexpected error occurred: " + ex3.Message;
		}
	}

	private void ResetAndHideDetails()
	{
		if (detailsParent.activeSelf)
		{
			detailsParent.SetActive(false);
			((UnityEventBase)executeButton.onClick).RemoveAllListeners();
			inputFields = new List<TMP_InputField>();
			((TMP_Text)errorMessage).text = string.Empty;
			for (int num = argumentsParent.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)argumentsParent.GetChild(num)).gameObject);
			}
			DebugUIEvents.OnDetailsToggledVisible(obj: false);
		}
	}

	private void HighlightButton(int highlightedIndex)
	{
		for (int i = 0; i < allCommandButtons.Count; i++)
		{
			DebugCommandButton debugCommandButton = allCommandButtons[i];
			bool highlighted = i == highlightedIndex;
			debugCommandButton.ToggleHighlight(highlighted);
		}
	}
}
