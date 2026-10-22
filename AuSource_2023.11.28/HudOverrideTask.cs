using System.Linq;
using System.Text;
using UnityEngine;

public class HudOverrideTask : SabotageTask, IHudOverrideTask
{
	private bool isComplete;

	private HudOverrideSystemType system;

	private bool even;

	public override int TaskStep
	{
		get
		{
			if (!isComplete)
			{
				return 0;
			}
			return 1;
		}
	}

	public override bool IsComplete => isComplete;

	public override void Initialize()
	{
		ShipStatus instance = ShipStatus.Instance;
		system = instance.Systems[SystemTypes.Comms] as HudOverrideSystemType;
		SetupArrows();
	}

	private void FixedUpdate()
	{
		if (!isComplete && !system.IsActive)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixComms);
	}

	public override void Complete()
	{
		isComplete = true;
		PlayerControl.LocalPlayer.RemoveTask(this);
		if (didContribute)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsSabotagesFixed);
		}
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageFixed(SystemTypes.Comms);
		DestroyableSingleton<AchievementManager>.Instance.OnSabotageFixed(didContribute);
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		even = !even;
		Color val = (even ? Color.yellow : Color.red);
		sb.Append(val.ToTextColor());
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.FixComms));
		sb.Append("</color>");
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].image.color = val;
		}
	}
}
