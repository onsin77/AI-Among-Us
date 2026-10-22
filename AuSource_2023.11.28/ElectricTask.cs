using System.Linq;
using System.Text;
using UnityEngine;

public class ElectricTask : SabotageTask
{
	private bool isComplete;

	private SwitchSystem system;

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
		system = (SwitchSystem)instance.Systems[SystemTypes.Electrical];
		SetupArrows();
	}

	private void FixedUpdate()
	{
		if (!isComplete && system.ExpectedSwitches == system.ActualSwitches)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixLights);
	}

	public override void Complete()
	{
		isComplete = true;
		PlayerControl.LocalPlayer.RemoveTask(this);
		if (didContribute)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsSabotagesFixed);
		}
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageFixed(SystemTypes.Electrical);
		DestroyableSingleton<AchievementManager>.Instance.OnSabotageFixed(didContribute);
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		even = !even;
		Color val = (even ? Color.yellow : Color.red);
		sb.Append(val.ToTextColor());
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.FixLights));
		sb.AppendLine(" (%" + (int)(system.Level * 100f) + ")</color>");
		for (int i = 0; i < Arrows.Length; i++)
		{
			if (((Behaviour)Arrows[i]).isActiveAndEnabled)
			{
				Arrows[i].image.color = val;
			}
		}
	}
}
