using System.Linq;
using System.Text;
using UnityEngine;

public class ReactorTask : SabotageTask
{
	private bool isComplete;

	private ICriticalSabotage reactor;

	private bool even;

	public override int TaskStep => reactor.UserCount;

	public override bool IsComplete => isComplete;

	public override void Initialize()
	{
		SetupArrows();
	}

	public void Awake()
	{
		reactor = (ICriticalSabotage)ShipStatus.Instance.Systems[StartAt];
		DestroyableSingleton<HudManager>.Instance.StartReactorFlash();
		if (ShipStatus.Instance.AllRooms.FirstOrDefault((PlainShipRoom r) => r.RoomId == StartAt) is IReactorShipRoom reactorShipRoom)
		{
			reactorShipRoom.StartMeltdown();
		}
	}

	private void FixedUpdate()
	{
		if (!isComplete && !reactor.IsActive)
		{
			Complete();
		}
	}

	public override bool ValidConsole(Console console)
	{
		return console.TaskTypes.Contains(TaskType);
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
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.SabotageFixed(SystemTypes.Reactor);
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
		sb.Append((int)reactor.Countdown);
		sb.AppendLine($" ({reactor.UserCount}/{(byte)2})</color>");
		for (int i = 0; i < Arrows.Length; i++)
		{
			Arrows[i].image.color = val;
		}
	}
}
