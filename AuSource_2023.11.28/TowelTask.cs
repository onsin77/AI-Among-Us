using System.Linq;
using System.Text;

public class TowelTask : NormalPlayerTask
{
	public override bool ValidConsole(Console console)
	{
		if (TaskType == TaskTypes.PickUpTowels && console.TaskTypes.Contains(TaskTypes.PickUpTowels))
		{
			if (Data.IndexOf((byte b) => b == console.ConsoleId) != -1)
			{
				return true;
			}
			if (Data.All((byte b) => b == 250) && console.ConsoleId == 255)
			{
				return true;
			}
		}
		return false;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		int num = Data.Count((byte b) => b == 250);
		bool flag = num > 0;
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskType));
		if (num < Data.Length)
		{
			sb.Append(" (");
			sb.Append(num);
			sb.Append("/");
			sb.Append(Data.Length);
			sb.Append(")");
		}
		if (flag)
		{
			sb.Append("</color>");
		}
		sb.AppendLine();
	}
}
