using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class TaskAddButton : MonoBehaviour
{
	public TextMeshPro Text;

	public SpriteRenderer Overlay;

	public SpriteRenderer FileImage;

	public ButtonRolloverHandler RolloverHandler;

	public Sprite CheckImage;

	public Sprite ExImage;

	public PlayerTask MyTask;

	[HideInInspector]
	public PassiveButton Button;

	private RoleBehaviour role;

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

	public RoleBehaviour Role
	{
		get
		{
			return role;
		}
		set
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			role = value;
			if (role.IsImpostor)
			{
				FileImage.color = Palette.ImpostorRed;
			}
			else
			{
				FileImage.color = Palette.CrewmateBlue;
			}
			RolloverHandler.OutColor = FileImage.color;
		}
	}

	private void Awake()
	{
		Button = ((Component)this).GetComponent<PassiveButton>();
	}

	public void Start()
	{
		if (Object.op_Implicit((Object)(object)Role))
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			((Renderer)Overlay).enabled = data.Role.Role == Role.Role;
			Overlay.sprite = CheckImage;
			return;
		}
		PlayerTask playerTask = FindTaskByType();
		if (Object.op_Implicit((Object)(object)playerTask))
		{
			((Renderer)Overlay).enabled = true;
			Overlay.sprite = (playerTask.IsComplete ? CheckImage : ExImage);
		}
		else
		{
			((Renderer)Overlay).enabled = false;
		}
	}

	public void Update()
	{
		if (Object.op_Implicit((Object)(object)Role))
		{
			((Renderer)Overlay).enabled = PlayerControl.LocalPlayer.Data.Role.Role == Role.Role;
		}
	}

	public void AddTask()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Role))
		{
			if (PlayerControl.LocalPlayer.Data.Role.Role != Role.Role)
			{
				if (PlayerTask.DestroyTasksOfType<ImportantTextTask>(PlayerControl.LocalPlayer) && !PlayerTask.PlayerHasTaskOfType<NormalPlayerTask>(PlayerControl.LocalPlayer))
				{
					ShipStatus.Instance.Begin();
				}
				PlayerControl.LocalPlayer.RpcSetRole(Role.Role);
				((Component)PlayerControl.LocalPlayer).transform.position = Vector2.op_Implicit(SafePositionWorld);
				if (PlayerControl.LocalPlayer.Data.Role is ImpostorGhostRole impostorGhostRole)
				{
					impostorGhostRole.WasManuallyPicked = true;
				}
			}
		}
		else
		{
			PlayerTask playerTask = FindTaskByType();
			if (!Object.op_Implicit((Object)(object)playerTask))
			{
				AddTaskToPlayer(MyTask);
				Overlay.sprite = ExImage;
				((Renderer)Overlay).enabled = true;
			}
			else
			{
				PlayerControl.LocalPlayer.RemoveTask(playerTask);
				((Renderer)Overlay).enabled = false;
			}
		}
	}

	public static void AddTaskToPlayer(PlayerTask taskPrefab)
	{
		PlayerTask playerTask = Object.Instantiate<PlayerTask>(taskPrefab, ((Component)PlayerControl.LocalPlayer).transform);
		PlayerControl.LocalPlayer.myTasks.Add(playerTask);
		playerTask.Id = GameData.Instance.TutOnlyAddTask(PlayerControl.LocalPlayer.PlayerId);
		playerTask.Index = taskPrefab.Index;
		playerTask.Owner = PlayerControl.LocalPlayer;
		playerTask.Initialize();
	}

	private PlayerTask FindTaskByType()
	{
		for (int num = PlayerControl.LocalPlayer.myTasks.Count - 1; num > -1; num--)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[num];
			if (playerTask.TaskType == MyTask.TaskType)
			{
				if (playerTask.TaskType == TaskTypes.DivertPower)
				{
					if (((DivertPowerTask)playerTask).TargetSystem == ((DivertPowerTask)MyTask).TargetSystem)
					{
						return playerTask;
					}
				}
				else if (playerTask.TaskType == TaskTypes.EmptyGarbage || playerTask.TaskType == TaskTypes.RecordTemperature || playerTask.TaskType == TaskTypes.UploadData)
				{
					if (playerTask.StartAt == MyTask.StartAt)
					{
						return playerTask;
					}
				}
				else
				{
					if (playerTask.TaskType != TaskTypes.FixWeatherNode)
					{
						return playerTask;
					}
					if (((WeatherNodeTask)playerTask).NodeId == ((WeatherNodeTask)MyTask).NodeId)
					{
						return playerTask;
					}
				}
			}
		}
		return null;
	}
}
