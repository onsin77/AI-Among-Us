using System.Linq;
using System.Text;

public class UploadDataTask : NormalPlayerTask
{
	public SystemTypes EndAt = SystemTypes.Admin;

	public override bool ValidConsole(Console console)
	{
		if (console.Room != StartAt || !console.ValidTasks.Any((TaskSet set) => TaskType == set.taskType && set.taskStep.Contains(taskStep)))
		{
			if (taskStep == 1)
			{
				return console.TaskTypes.Contains(TaskType);
			}
			return false;
		}
		return true;
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0) ? StartAt : EndAt));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0) ? StringNames.DownloadData : StringNames.UploadData));
		sb.Append(" (");
		sb.Append(taskStep);
		sb.Append("/");
		sb.Append(MaxStep);
		sb.AppendLine(") </color>");
	}
}
