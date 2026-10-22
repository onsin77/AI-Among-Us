using System.Text;
using UnityEngine;

public class MushroomMixupSabotageTask : PlayerTask
{
	private bool isComplete;

	private MushroomMixupSabotageSystem sabotage;

	public override int TaskStep
	{
		get
		{
			if (!IsComplete)
			{
				return 0;
			}
			return 1;
		}
	}

	public override bool IsComplete => isComplete;

	public override void Initialize()
	{
		sabotage = (MushroomMixupSabotageSystem)ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage];
	}

	public override bool ValidConsole(Console console)
	{
		return false;
	}

	public override void Complete()
	{
		PlayerControl.LocalPlayer.RemoveTask(this);
		isComplete = true;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		sb.AppendLine(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MushroomMixupSabotage, (int)sabotage.CurrentSecondsUntilHeal).Color(Color.yellow));
	}
}
