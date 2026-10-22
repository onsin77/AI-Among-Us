using System.Linq;
using System.Text;
using UnityEngine;

public class HeliCharlesTask : SabotageTask
{
	private bool isComplete;

	private ICriticalSabotage sabotage;

	private bool even;

	public override int TaskStep => sabotage.UserCount;

	public override bool IsComplete => isComplete;

	public override void Initialize()
	{
		SetupArrows();
	}

	public void Awake()
	{
		sabotage = (ICriticalSabotage)ShipStatus.Instance.Systems[StartAt];
		DestroyableSingleton<HudManager>.Instance.StartReactorFlash();
	}

	private void FixedUpdate()
	{
		if (!isComplete && !sabotage.IsActive)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskType);
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
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		even = !even;
		Color val = (even ? Color.yellow : Color.red);
		sb.Append(val.ToTextColor());
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskType));
		sb.Append(" ");
		sb.Append((int)sabotage.Countdown);
		sb.AppendLine($" ({sabotage.UserCount}/{(byte)2})</color>");
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].image.color = val;
		}
	}
}
