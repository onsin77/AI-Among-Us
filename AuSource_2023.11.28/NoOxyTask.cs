using System.Linq;
using System.Text;
using UnityEngine;

public class NoOxyTask : SabotageTask
{
	private bool isComplete;

	private LifeSuppSystemType reactor;

	private bool even;

	public int targetNumber;

	public override int TaskStep => reactor.UserCount;

	public override bool IsComplete => isComplete;

	public override void Initialize()
	{
		targetNumber = IntRange.Next(0, 99999);
		ShipStatus instance = ShipStatus.Instance;
		reactor = (LifeSuppSystemType)instance.Systems[SystemTypes.LifeSupp];
		DestroyableSingleton<HudManager>.Instance.StartOxyFlash();
		SetupArrows();
	}

	private void FixedUpdate()
	{
		if (isComplete)
		{
			return;
		}
		if (!reactor.IsActive)
		{
			Complete();
			return;
		}
		for (int i = 0; i < Arrows.Length; i++)
		{
			((Component)Arrows[i]).gameObject.SetActive(!reactor.GetConsoleComplete(i));
		}
	}

	public override bool ValidConsole(Console console)
	{
		if (!reactor.GetConsoleComplete(console.ConsoleId))
		{
			return console.TaskTypes.Contains(TaskTypes.RestoreOxy);
		}
		return false;
	}

	public override void OnRemove()
	{
	}

	public override void Complete()
	{
		isComplete = true;
		PlayerControl.LocalPlayer.RemoveTask(this);
		if (didContribute)
		{
			StatsManager.Instance.IncrementStat(StringNames.StatsSabotagesFixed);
		}
		DestroyableSingleton<AchievementManager>.Instance.OnSabotageFixed(didContribute);
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageFixed(SystemTypes.LifeSupp);
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		even = !even;
		Color val = (even ? Color.yellow : Color.red);
		if (reactor != null)
		{
			sb.Append(val.ToTextColor());
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.RestoreOxy));
			sb.Append(" ");
			sb.Append(Mathf.CeilToInt(reactor.Countdown));
			sb.AppendLine($" ({reactor.UserCount}/{(byte)2})</color>");
		}
		else
		{
			sb.AppendLine(val.ToTextColor() + "Oxygen depleting</color>");
		}
		for (int i = 0; i < Arrows.Length; i++)
		{
			try
			{
				Arrows[i].image.color = val;
			}
			catch
			{
			}
		}
	}
}
