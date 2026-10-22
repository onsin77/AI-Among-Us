using System.Linq;

public class WaterWayTask : NormalPlayerTask
{
	public override void Initialize()
	{
		base.Initialize();
		Data = new byte[3];
	}

	public override bool ValidConsole(Console console)
	{
		if (console.TaskTypes.Contains(TaskType))
		{
			return Data[console.ConsoleId] < byte.MaxValue;
		}
		return false;
	}
}
