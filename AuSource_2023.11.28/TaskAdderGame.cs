using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class TaskAdderGame : Minigame
{
	public TextMeshPro PathText;

	public TaskFolder RootFolderPrefab;

	public TaskAddButton TaskPrefab;

	public Transform TaskParent;

	public List<TaskFolder> Hierarchy = new List<TaskFolder>();

	public List<Transform> ActiveItems = new List<Transform>();

	public TaskAddButton RoleButton;

	public float folderWidth;

	public float fileWidth;

	public float lineWidth;

	private TaskFolder Root;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement FolderBackButton;

	public List<UiElement> ControllerSelectable;

	private string restorePreviousSelectionByFolderName = string.Empty;

	public UiElement restorePreviousSelectionFound;

	public Vector2 SafePositionWorld
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, null, ControllerSelectable, gridNav: true);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.ResetAll();
	}

	public override void Begin(PlayerTask t)
	{
		base.Begin(t);
		Root = Object.Instantiate<TaskFolder>(RootFolderPrefab, ((Component)this).transform);
		((Component)Root).gameObject.SetActive(false);
		Dictionary<SystemTypes, TaskFolder> folders = new Dictionary<SystemTypes, TaskFolder>();
		PopulateRoot(Root, folders, ShipStatus.Instance.CommonTasks);
		PopulateRoot(Root, folders, ShipStatus.Instance.LongTasks);
		PopulateRoot(Root, folders, ShipStatus.Instance.ShortTasks);
		Root.SubFolders = Root.SubFolders.OrderBy((TaskFolder f) => f.FolderName).ToList();
		ShowFolder(Root);
	}

	private void PopulateRoot(TaskFolder rootFolder, Dictionary<SystemTypes, TaskFolder> folders, NormalPlayerTask[] taskList)
	{
		foreach (NormalPlayerTask normalPlayerTask in taskList)
		{
			SystemTypes systemTypes = normalPlayerTask.StartAt;
			if (normalPlayerTask is DivertPowerTask)
			{
				systemTypes = ((DivertPowerTask)normalPlayerTask).TargetSystem;
			}
			if (systemTypes == SystemTypes.LowerEngine)
			{
				systemTypes = SystemTypes.UpperEngine;
			}
			if (!folders.TryGetValue(systemTypes, out var value))
			{
				TaskFolder taskFolder = (folders[systemTypes] = Object.Instantiate<TaskFolder>(RootFolderPrefab, ((Component)this).transform));
				value = taskFolder;
				((Component)value).gameObject.SetActive(false);
				if (systemTypes == SystemTypes.UpperEngine)
				{
					value.FolderName = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Engines);
				}
				else
				{
					value.FolderName = DestroyableSingleton<TranslationController>.Instance.GetString(systemTypes);
				}
				rootFolder.SubFolders.Add(value);
			}
			value.Children.Add(normalPlayerTask);
		}
	}

	public void GoToRoot()
	{
		Hierarchy.Clear();
		ShowFolder(Root);
	}

	public void GoUpOne()
	{
		if (Hierarchy.Count > 1)
		{
			TaskFolder taskFolder = Hierarchy[Hierarchy.Count - 2];
			Hierarchy.RemoveAt(Hierarchy.Count - 1);
			Hierarchy.RemoveAt(Hierarchy.Count - 1);
			ShowFolder(taskFolder);
		}
	}

	public void ShowFolder(TaskFolder taskFolder)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(64);
		Hierarchy.Add(taskFolder);
		for (int i = 0; i < Hierarchy.Count; i++)
		{
			stringBuilder.Append(Hierarchy[i].FolderName);
			stringBuilder.Append("\\");
		}
		((TMP_Text)PathText).text = stringBuilder.ToString();
		for (int j = 0; j < ActiveItems.Count; j++)
		{
			Object.Destroy((Object)(object)((Component)ActiveItems[j]).gameObject);
		}
		ActiveItems.Clear();
		float xCursor = 0f;
		float yCursor = 0f;
		float maxHeight = 0f;
		for (int k = 0; k < taskFolder.SubFolders.Count; k++)
		{
			TaskFolder taskFolder2 = Object.Instantiate<TaskFolder>(taskFolder.SubFolders[k], TaskParent);
			((Component)taskFolder2).gameObject.SetActive(true);
			taskFolder2.Parent = this;
			((Component)taskFolder2).transform.localPosition = new Vector3(xCursor, yCursor, 0f);
			((Component)taskFolder2).transform.localScale = Vector3.one;
			float num = maxHeight;
			Bounds bounds = ((TMP_Text)taskFolder2.Text).bounds;
			maxHeight = Mathf.Max(num, bounds.size.y + 1.1f);
			xCursor += folderWidth;
			if (xCursor > lineWidth)
			{
				xCursor = 0f;
				yCursor -= maxHeight;
				maxHeight = 0f;
			}
			ActiveItems.Add(((Component)taskFolder2).transform);
			if ((Object)(object)taskFolder2 != (Object)null && (Object)(object)taskFolder2.Button != (Object)null)
			{
				ControllerManager.Instance.AddSelectableUiElement(taskFolder2.Button);
				if (!string.IsNullOrEmpty(restorePreviousSelectionByFolderName) && taskFolder2.FolderName.Equals(restorePreviousSelectionByFolderName))
				{
					restorePreviousSelectionFound = taskFolder2.Button;
				}
			}
		}
		bool flag = false;
		List<PlayerTask> list = taskFolder.Children.OrderBy((PlayerTask t) => t.TaskType).ToList();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			TaskAddButton taskAddButton = Object.Instantiate<TaskAddButton>(TaskPrefab);
			taskAddButton.MyTask = list[num2];
			if (taskAddButton.MyTask.TaskType == TaskTypes.DivertPower)
			{
				SystemTypes targetSystem = ((DivertPowerTask)taskAddButton.MyTask).TargetSystem;
				((TMP_Text)taskAddButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPowerTo, DestroyableSingleton<TranslationController>.Instance.GetString(targetSystem));
			}
			else if (taskAddButton.MyTask.TaskType == TaskTypes.FixWeatherNode)
			{
				int nodeId = ((WeatherNodeTask)taskAddButton.MyTask).NodeId;
				((TMP_Text)taskAddButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode) + " " + DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[nodeId]);
			}
			else
			{
				((TMP_Text)taskAddButton.Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(taskAddButton.MyTask.TaskType);
			}
			AddFileAsChild(taskFolder, taskAddButton, ref xCursor, ref yCursor, ref maxHeight);
			if (!((Object)(object)taskAddButton != (Object)null) || !((Object)(object)taskAddButton.Button != (Object)null))
			{
				continue;
			}
			ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
			if (Hierarchy.Count != 1 && !flag)
			{
				TaskFolder component = ((Component)ControllerManager.Instance.CurrentUiState.CurrentSelection).GetComponent<TaskFolder>();
				if ((Object)(object)component != (Object)null)
				{
					restorePreviousSelectionByFolderName = component.FolderName;
				}
				ControllerManager.Instance.SetDefaultSelection(taskAddButton.Button);
				flag = true;
			}
		}
		if (Hierarchy.Count == 1)
		{
			for (int num3 = 0; num3 < DestroyableSingleton<RoleManager>.Instance.AllRoles.Length; num3++)
			{
				RoleBehaviour roleBehaviour = DestroyableSingleton<RoleManager>.Instance.AllRoles[num3];
				if (roleBehaviour.Role == RoleTypes.ImpostorGhost || roleBehaviour.Role == RoleTypes.CrewmateGhost)
				{
					continue;
				}
				TaskAddButton taskAddButton2 = Object.Instantiate<TaskAddButton>(RoleButton);
				taskAddButton2.SafePositionWorld = SafePositionWorld;
				((TMP_Text)taskAddButton2.Text).text = "Be_" + roleBehaviour.NiceName + ".exe";
				AddFileAsChild(Root, taskAddButton2, ref xCursor, ref yCursor, ref maxHeight);
				taskAddButton2.Role = roleBehaviour;
				if ((Object)(object)taskAddButton2 != (Object)null && (Object)(object)taskAddButton2.Button != (Object)null)
				{
					ControllerManager.Instance.AddSelectableUiElement(taskAddButton2.Button);
					if (num3 == 0 && (Object)(object)restorePreviousSelectionFound != (Object)null)
					{
						ControllerManager.Instance.SetDefaultSelection(restorePreviousSelectionFound);
						restorePreviousSelectionByFolderName = string.Empty;
						restorePreviousSelectionFound = null;
					}
					else if (num3 == 0)
					{
						ControllerManager.Instance.SetDefaultSelection(taskAddButton2.Button);
					}
				}
			}
		}
		if (Hierarchy.Count == 1)
		{
			ControllerManager.Instance.SetBackButton(BackButton);
		}
		else
		{
			ControllerManager.Instance.SetBackButton(FolderBackButton);
		}
	}

	private void AddFileAsChild(TaskFolder taskFolder, TaskAddButton item, ref float xCursor, ref float yCursor, ref float maxHeight)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		((Component)item).transform.SetParent(TaskParent);
		((Component)item).transform.localPosition = new Vector3(xCursor, yCursor, 0f);
		((Component)item).transform.localScale = Vector3.one;
		float num = maxHeight;
		Bounds bounds = ((TMP_Text)item.Text).bounds;
		maxHeight = Mathf.Max(num, bounds.size.y + 1.1f);
		xCursor += fileWidth;
		if (xCursor > lineWidth)
		{
			xCursor = 0f;
			yCursor -= maxHeight;
			maxHeight = 0f;
		}
		ActiveItems.Add(((Component)item).transform);
	}
}
