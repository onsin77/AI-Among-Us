using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AirshipUploadTask : NormalPlayerTask
{
	public ArrowBehaviour[] Arrows;

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

	protected override void FixedUpdate()
	{
		if (((Behaviour)Arrows[0]).isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			arrowSuspended = true;
			Arrows.SetAllGameObjectsActive(isActive: false);
		}
		else if (arrowSuspended && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			arrowSuspended = false;
			Arrows.SetAllGameObjectsActive(isActive: true);
		}
	}

	public override void UpdateArrowAndLocation()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (taskStep == 0 || IsComplete || !Owner.AmOwner)
		{
			Arrows.SetAllGameObjectsActive(isActive: false);
			return;
		}
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			arrowSuspended = true;
		}
		else
		{
			Arrows.SetAllGameObjectsActive(isActive: true);
		}
		List<Vector2> list = FindValidConsolesPositions();
		for (int i = 0; i < list.Count && i < Arrows.Length; i++)
		{
			Arrows[i].target = Vector2.op_Implicit(list[i]);
		}
		LocationDirty = true;
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0) ? StartAt : SystemTypes.Outside));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((taskStep == 0) ? StringNames.DownloadData : StringNames.UploadData));
		sb.Append(" (");
		sb.Append(taskStep);
		sb.Append("/");
		sb.Append(MaxStep);
		sb.AppendLine(") </color>");
	}
}
