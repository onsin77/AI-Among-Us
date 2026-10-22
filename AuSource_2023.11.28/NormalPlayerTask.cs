using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NormalPlayerTask : PlayerTask
{
	public enum TaskLength
	{
		None,
		Common,
		Short,
		Long
	}

	public enum TimerState
	{
		NotStarted,
		Started,
		Finished
	}

	public TaskLength Length;

	public int taskStep;

	public int MaxStep;

	public bool ShowTaskStep = true;

	public bool ShowTaskTimer;

	public TimerState TimerStarted;

	public float TaskTimer;

	public byte[] Data;

	public ArrowBehaviour Arrow;

	[Tooltip("To have multiple texts on multistage tasks")]
	[SerializeField]
	private bool useMultipleText;

	[SerializeField]
	[Tooltip("For tasks that need a number of steps completed before stage 2")]
	private int maxNumStepsStage1;

	[SerializeField]
	[Tooltip("Used only if useMultipleText is selected")]
	private StringNames textStage1;

	[SerializeField]
	[Tooltip("Used only if useMultipleText is selected")]
	private StringNames textStage2;

	protected bool arrowSuspended;

	public override int TaskStep => taskStep;

	public override bool IsComplete => taskStep >= MaxStep;

	public override void Initialize()
	{
		if (Object.op_Implicit((Object)(object)Arrow) && Object.op_Implicit((Object)(object)Owner) && !Owner.AmOwner)
		{
			((Component)Arrow).gameObject.SetActive(false);
		}
		HasLocation = true;
		LocationDirty = true;
		switch (TaskType)
		{
		case TaskTypes.FixShower:
		case TaskTypes.FixAntenna:
		{
			float value = (BoolRange.Next() ? FloatRange.Next(0f, 0.1f) : (1f - FloatRange.Next(0f, 0.1f)));
			Data = BitConverter.GetBytes(value);
			break;
		}
		case TaskTypes.RewindTapes:
		{
			Data = new byte[8];
			float num4 = IntRange.Next(6, 18) * 3600 + IntRange.Next(0, 60) * 60 + IntRange.Next(0, 60);
			BitConverter.GetBytes(num4).CopyTo(Data, 0);
			BitConverter.GetBytes(num4 + (float)(IntRange.RandomSign() * (IntRange.Next(5, 7) * 60 + IntRange.Next(0, 60)))).CopyTo(Data, 4);
			break;
		}
		case TaskTypes.PickUpTowels:
		{
			Data = new byte[8];
			int[] array = Enumerable.Range(0, 14).ToArray();
			array.Shuffle();
			for (byte b4 = 0; b4 < Data.Length; b4++)
			{
				Data[b4] = (byte)array[b4];
			}
			break;
		}
		case TaskTypes.SortRecords:
			Data = new byte[4];
			break;
		case TaskTypes.CleanToilet:
			Data = new byte[1];
			Data[0] = IntRange.NextByte(0, 4);
			break;
		case TaskTypes.StartFans:
		{
			Data = new byte[4];
			for (byte b = 0; b < Data.Length; b++)
			{
				Data[b] = IntRange.NextByte(0, 4);
			}
			Data[IntRange.NextByte(0, 4)] = IntRange.NextByte(1, 4);
			break;
		}
		case TaskTypes.ResetBreakers:
		{
			Data = new byte[7];
			for (byte b3 = 0; b3 < Data.Length; b3++)
			{
				Data[b3] = b3;
			}
			Data.Shuffle();
			break;
		}
		case TaskTypes.OpenWaterways:
			Data = new byte[3];
			break;
		case TaskTypes.Decontaminate:
			Data = new byte[1];
			Data[0] = IntRange.NextByte(10, 30);
			break;
		case TaskTypes.ReplaceWaterJug:
			Data = new byte[1];
			break;
		case TaskTypes.WaterPlants:
			Data = new byte[4];
			break;
		case TaskTypes.EnterIdCode:
			Data = BitConverter.GetBytes(IntRange.Next(1, 99999));
			break;
		case TaskTypes.ChartCourse:
			Data = new byte[4];
			break;
		case TaskTypes.InspectSample:
			Data = new byte[2];
			break;
		case TaskTypes.FuelEngines:
		case TaskTypes.ExtractFuel:
			Data = new byte[2];
			break;
		case TaskTypes.StartReactor:
			Data = new byte[6];
			break;
		case TaskTypes.PrimeShields:
		{
			Data = new byte[1];
			int num2 = 0;
			for (int num3 = 0; num3 < 7; num3++)
			{
				byte b2 = (byte)(1 << num3);
				if (BoolRange.Next(0.7f))
				{
					Data[0] |= b2;
					num2++;
				}
			}
			Data[0] &= 118;
			break;
		}
		case TaskTypes.AlignEngineOutput:
			Data = new byte[4];
			Data[0] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
			Data[1] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
			break;
		case TaskTypes.FixWiring:
		{
			Data = new byte[MaxStep];
			Console console5 = PickRandomConsoles(TaskTypes.FixWiring, Data).First((Console v) => v.ConsoleId == Data[0]);
			StartAt = console5.Room;
			break;
		}
		case TaskTypes.CollectShells:
		{
			Console console4 = PickRandomConsoles(taskStep, TaskTypes.CollectShells).First();
			Data = new byte[1] { (byte)console4.ConsoleId };
			StartAt = console4.Room;
			break;
		}
		case TaskTypes.RoastMarshmallow:
		{
			List<Console> source3 = PickRandomConsoles(taskStep, TaskTypes.RoastMarshmallow);
			Data = new byte[2]
			{
				(byte)source3.First((Console c) => c.ConsoleId != 0).ConsoleId,
				0
			};
			break;
		}
		case TaskTypes.ReplaceParts:
		{
			Console console3 = PickRandomConsoles(taskStep, TaskTypes.ReplaceParts).First();
			Data = new byte[1] { (byte)console3.ConsoleId };
			StartAt = console3.Room;
			break;
		}
		case TaskTypes.CrankGenerator:
		{
			Console console2 = PickRandomConsoles(taskStep, TaskTypes.CrankGenerator).First();
			Data = new byte[1] { (byte)console2.ConsoleId };
			StartAt = console2.Room;
			break;
		}
		case TaskTypes.CollectSamples:
		{
			List<Console> source2 = PickRandomConsoles(taskStep, TaskTypes.CollectSamples);
			Data = new byte[1] { (byte)source2.First().ConsoleId };
			break;
		}
		case TaskTypes.HelpCritter:
		{
			List<Console> source = PickRandomConsoles(taskStep, TaskTypes.HelpCritter);
			Data = new byte[1] { (byte)source.First().ConsoleId };
			break;
		}
		case TaskTypes.VentCleaning:
		{
			byte[] consoleIds = new byte[MaxStep];
			Console console = PickRandomConsoles(TaskTypes.VentCleaning, consoleIds).First((Console v) => v.ConsoleId == consoleIds[0]);
			StartAt = console.Room;
			Data = new byte[1 + consoleIds.Length];
			consoleIds.CopyTo(Data, 0);
			int num = IntRange.Next(4, 8);
			Data[MaxStep] = (byte)num;
			break;
		}
		case TaskTypes.CatchFish:
			Data = new byte[4];
			Data[1] = IntRange.NextByte(0, 8);
			Data[2] = IntRange.NextByte(0, 8);
			Data[3] = IntRange.NextByte(0, 8);
			break;
		}
	}

	public void NextStep()
	{
		taskStep++;
		UpdateArrowAndLocation();
		if (taskStep >= MaxStep)
		{
			taskStep = MaxStep;
			if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
			{
				return;
			}
			if (DestroyableSingleton<HudManager>.InstanceExists)
			{
				DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
				StatsManager.Instance.IncrementStat(StringNames.StatsTasksCompleted);
				DestroyableSingleton<AchievementManager>.Instance.OnTaskComplete(TaskType);
				if (PlayerTask.AllTasksCompleted(PlayerControl.LocalPlayer))
				{
					StatsManager.Instance.IncrementStat(StringNames.StatsAllTasksCompleted);
				}
			}
			PlayerControl.LocalPlayer.RpcCompleteTask(Id);
		}
		else if (ShowTaskStep && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound, loop: false);
		}
	}

	public virtual void UpdateArrowAndLocation()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)Arrow))
		{
			return;
		}
		if (!Owner.AmOwner)
		{
			((Component)Arrow).gameObject.SetActive(false);
		}
		else if (!IsComplete)
		{
			if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				arrowSuspended = true;
			}
			else
			{
				((Component)Arrow).gameObject.SetActive(true);
			}
			if (TaskType == TaskTypes.FixWiring)
			{
				Console console = FindSpecialConsole((Console c) => c.TaskTypes.Contains(TaskTypes.FixWiring) && c.ConsoleId == Data[taskStep]);
				Arrow.target = ((Component)console).transform.position;
				StartAt = console.Room;
			}
			else if (TaskType == TaskTypes.AlignEngineOutput)
			{
				if (AlignGame.IsSuccess(Data[2]))
				{
					Arrow.target = ((Component)FindSpecialConsole((Console c) => c.TaskTypes.Contains(TaskTypes.AlignEngineOutput) && c.ConsoleId == 0)).transform.position;
					StartAt = SystemTypes.UpperEngine;
				}
				else
				{
					Arrow.target = ((Component)FindSpecialConsole((Console console6) => console6.TaskTypes.Contains(TaskTypes.AlignEngineOutput) && console6.ConsoleId == 1)).transform.position;
					StartAt = SystemTypes.LowerEngine;
				}
			}
			else if (TaskType == TaskTypes.StartFans)
			{
				Console console2 = FindSpecialConsole((Console c) => ValidConsole(c) && c.ConsoleId == 1);
				Arrow.target = ((Component)console2).transform.position;
				StartAt = console2.Room;
			}
			else if (TaskType == TaskTypes.ReplaceParts)
			{
				Console console3 = PickRandomConsoles(taskStep - 1, TaskTypes.ReplaceParts).First((Console c) => c.ConsoleId == Data[0]);
				List<Console> list = PickRandomConsoles(taskStep, TaskTypes.ReplaceParts);
				Console console4 = null;
				if (console3.Room == SystemTypes.Reactor)
				{
					console4 = list.Find((Console c) => c.Room == SystemTypes.Comms);
				}
				else if (console3.Room == SystemTypes.UpperEngine)
				{
					console4 = list.Find((Console c) => c.Room == SystemTypes.Dropship);
				}
				if ((Object)(object)console4 == (Object)null)
				{
					throw new NullReferenceException($"Could not find appropriate next console for ReplaceParts task. Previous console's room was {console3.Room}");
				}
				Arrow.target = ((Component)console4).transform.position;
				StartAt = console4.Room;
				Data[0] = (byte)console4.ConsoleId;
			}
			else if (TaskType == TaskTypes.CatchFish && MaxStep == 3)
			{
				((Component)Arrow).gameObject.SetActive(false);
			}
			else if (TaskType == TaskTypes.RoastMarshmallow)
			{
				((Component)Arrow).gameObject.SetActive(false);
				if (TaskStep > 0)
				{
					((Component)Arrow).gameObject.SetActive(true);
					Arrow.target = ((Component)PickRandomConsoles(TaskStep, TaskTypes.RoastMarshmallow).First((Console c) => c.ConsoleId == 0)).transform.position;
				}
			}
			else
			{
				Console console5 = FindObjectPos();
				if (Object.op_Implicit((Object)(object)console5))
				{
					Arrow.target = ((Component)console5).transform.position;
					StartAt = console5.Room;
				}
			}
			LocationDirty = true;
		}
		else
		{
			((Component)Arrow).gameObject.SetActive(false);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (TimerStarted == TimerState.Started)
		{
			TaskTimer -= Time.deltaTime;
			if (TaskTimer <= 0f)
			{
				TaskTimer = 0f;
				TimerStarted = TimerState.Finished;
			}
		}
		if (Object.op_Implicit((Object)(object)Arrow))
		{
			if (IsComplete && ((Behaviour)Arrow).isActiveAndEnabled)
			{
				arrowSuspended = false;
				((Component)Arrow).gameObject.SetActive(false);
			}
			else if (((Behaviour)Arrow).isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				arrowSuspended = true;
				((Component)Arrow).gameObject.SetActive(false);
			}
			else if (arrowSuspended && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				arrowSuspended = false;
				((Component)Arrow).gameObject.SetActive(true);
			}
		}
	}

	public override bool ValidConsole(Console console)
	{
		if (TaskType == TaskTypes.ResetBreakers)
		{
			if (!console.TaskTypes.Contains(TaskTypes.ResetBreakers))
			{
				return false;
			}
			int num = Array.IndexOf(Data, (byte)console.ConsoleId);
			return taskStep <= num;
		}
		if (TaskType == TaskTypes.SortRecords)
		{
			if (!console.TaskTypes.Contains(TaskType))
			{
				return false;
			}
			int num2 = Data.IndexOf((byte b) => b != 0 && b != byte.MaxValue);
			if (num2 != -1)
			{
				return console.ConsoleId == Data[num2];
			}
			return console.ConsoleId == 0;
		}
		if (TaskType == TaskTypes.CleanToilet)
		{
			if (console.TaskTypes.Contains(TaskType))
			{
				return console.ConsoleId == Data[0];
			}
			return false;
		}
		if (TaskType == TaskTypes.EmptyGarbage)
		{
			if (console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(taskStep)))
			{
				if (taskStep != 0 || console.Room != StartAt)
				{
					return taskStep == 1;
				}
				return true;
			}
			return false;
		}
		if (TaskType == TaskTypes.ReplaceParts)
		{
			if (console.ValidTasks.Length != 0 && console.ValidTasks[0].taskType == TaskType && console.ValidTasks[0].taskStep.Contains(taskStep))
			{
				return console.ConsoleId == Data[0];
			}
			return false;
		}
		if (TaskType == TaskTypes.CrankGenerator)
		{
			if (console.TaskTypes.Any((TaskTypes t) => t == TaskType))
			{
				return Data[0] == console.ConsoleId;
			}
			return false;
		}
		if (TaskType == TaskTypes.CollectShells)
		{
			if (console.TaskTypes.Any((TaskTypes t) => t == TaskType))
			{
				return Data[0] == console.ConsoleId;
			}
			return false;
		}
		if (TaskType == TaskTypes.CollectSamples)
		{
			if (console.ValidTasks.Length != 0 && console.ValidTasks[0].taskType == TaskType && console.ValidTasks[0].taskStep.Contains(taskStep))
			{
				if (console.ConsoleId != Data[0])
				{
					return taskStep > 0;
				}
				return true;
			}
			return false;
		}
		if (TaskType == TaskTypes.HelpCritter)
		{
			if (console.ValidTasks.Length != 0 && console.ValidTasks[0].taskType == TaskType && console.ValidTasks[0].taskStep.Contains(taskStep))
			{
				if (console.ConsoleId != Data[0])
				{
					return taskStep > 0;
				}
				return true;
			}
			return false;
		}
		if (TaskType == TaskTypes.FixWiring)
		{
			if (console.TaskTypes.Contains(TaskType))
			{
				return console.ConsoleId == Data[taskStep];
			}
			return false;
		}
		if (TaskType == TaskTypes.VentCleaning)
		{
			if (console.TaskTypes.Contains(TaskType))
			{
				return console.ConsoleId == Data[taskStep];
			}
			return false;
		}
		if (TaskType == TaskTypes.AlignEngineOutput)
		{
			if (console.TaskTypes.Contains(TaskType))
			{
				return console.ConsoleId == taskStep;
			}
			return false;
		}
		if (TaskType == TaskTypes.FuelEngines || TaskType == TaskTypes.ExtractFuel)
		{
			return console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(Data[1]));
		}
		if (TaskType == TaskTypes.RecordTemperature)
		{
			if (console.Room == StartAt)
			{
				return console.TaskTypes.Any((TaskTypes tt) => tt == TaskType);
			}
			return false;
		}
		if (TaskType == TaskTypes.CatchFish)
		{
			if (console.Room != StartAt || (taskStep != 0 && MaxStep != 3))
			{
				if (MaxStep == 2)
				{
					return console.ValidTasks.Any((TaskSet set) => set.taskType == TaskType && set.taskStep.Contains(taskStep));
				}
				return false;
			}
			return true;
		}
		if (TaskType == TaskTypes.RoastMarshmallow)
		{
			if (taskStep != 0 || Data[0] != (byte)console.ConsoleId || !console.TaskTypes.Any((TaskTypes t) => t == TaskTypes.RoastMarshmallow))
			{
				if (taskStep == 1 && Data[1] == (byte)console.ConsoleId)
				{
					return console.TaskTypes.Any((TaskTypes t) => t == TaskTypes.RoastMarshmallow);
				}
				return false;
			}
			return true;
		}
		if (!console.TaskTypes.Any((TaskTypes tt) => tt == TaskType))
		{
			return console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(taskStep));
		}
		return true;
	}

	public override void Complete()
	{
		taskStep = MaxStep;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		bool flag = ShouldYellowText();
		if (flag)
		{
			if (IsComplete)
			{
				sb.Append("<color=#00DD00FF>");
			}
			else
			{
				sb.Append("<color=#FFFF00FF>");
			}
		}
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StartAt));
		sb.Append(": ");
		if (useMultipleText)
		{
			if (maxNumStepsStage1 > 0)
			{
				sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0 || MaxStep == maxNumStepsStage1) ? textStage1 : textStage2));
			}
			else
			{
				sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0) ? textStage1 : textStage2));
			}
		}
		else
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskType));
		}
		if (ShowTaskTimer && TimerStarted == TimerState.Started)
		{
			sb.Append(" (");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv, (int)TaskTimer));
			sb.Append(")");
		}
		else if (ShowTaskStep)
		{
			sb.Append(" (");
			sb.Append(taskStep);
			sb.Append("/");
			sb.Append(MaxStep);
			sb.Append(")");
		}
		if (flag)
		{
			sb.Append("</color>");
		}
		sb.AppendLine();
	}

	private bool ShouldYellowText()
	{
		if ((TaskType == TaskTypes.FuelEngines || TaskType == TaskTypes.ExtractFuel) && Data[1] > 0)
		{
			return true;
		}
		if (taskStep <= 0)
		{
			return TimerStarted != TimerState.NotStarted;
		}
		return true;
	}

	private static List<Console> PickRandomConsoles(TaskTypes taskType, byte[] consoleIds)
	{
		List<Console> list = ShipStatus.Instance.AllConsoles.Where((Console t) => t.TaskTypes.Contains(taskType)).ToList();
		List<Console> list2 = new List<Console>(list);
		for (int num = 0; num < consoleIds.Length; num++)
		{
			if (num + 1 > list.Count)
			{
				throw new IndexOutOfRangeException($"Could not find {consoleIds.Length} valid consoles for task {taskType}");
			}
			int index = list2.RandomIdx();
			consoleIds[num] = (byte)list2[index].ConsoleId;
			list2.RemoveAt(index);
		}
		Array.Sort(consoleIds);
		return list;
	}

	private static List<Console> PickRandomConsoles(int taskStep, TaskTypes taskType)
	{
		List<Console> list = ShipStatus.Instance.AllConsoles.Where((Console t) => t.TaskTypes.Contains(taskType) || t.ValidTasks.Any((TaskSet x) => x.taskType == taskType && x.taskStep.Contains(taskStep))).ToList();
		list.Shuffle();
		return list;
	}
}
