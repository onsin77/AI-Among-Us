using System;

[Serializable]
public class TaskSet
{
	public TaskTypes taskType;

	public IntRange taskStep = new IntRange(0, 0);

	public bool Contains(PlayerTask t)
	{
		if (t.TaskType == taskType)
		{
			return taskStep.Contains(t.TaskStep);
		}
		return false;
	}
}
