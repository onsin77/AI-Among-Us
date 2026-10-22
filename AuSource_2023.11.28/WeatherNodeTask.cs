using System.Linq;
using System.Text;

public class WeatherNodeTask : NormalPlayerTask
{
	public int NodeId;

	public Minigame Stage2Prefab;

	public override bool ValidConsole(Console console)
	{
		if (taskStep != 0 || console.ConsoleId != NodeId || !console.TaskTypes.Contains(TaskType))
		{
			return console.ValidTasks.Any((TaskSet t) => t.Contains(this));
		}
		return true;
	}

	public override Minigame GetMinigamePrefab()
	{
		if (taskStep == 0)
		{
			return MinigamePrefab;
		}
		return Stage2Prefab;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		if (taskStep > 0)
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
		if (taskStep == 0)
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[NodeId]));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode));
		}
		else
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StartAt));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode));
		}
		sb.Append(" (");
		sb.Append(taskStep);
		sb.Append("/");
		sb.Append(MaxStep);
		sb.AppendLine(")");
		if (taskStep > 0)
		{
			sb.Append("</color>");
		}
	}
}
