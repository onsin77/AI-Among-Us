using System;
using UnityEngine;

namespace AmongUs.QuickChat;

[Serializable]
public class QuickChatClassicPhraseRules : AbstractQuickChatPhraseRuleGroup
{
	[SerializeField]
	private int minDeadCrew;

	[SerializeField]
	private int minMeetingCount;

	[SerializeField]
	private QuickChatPhaseRules phases;

	[SerializeField]
	private QuickChatMapRules maps;

	[SerializeField]
	private QuickChatRoleRules roles;

	public QuickChatClassicPhraseRules(QuickChatPhaseRules phase, QuickChatMapRules map, QuickChatRoleRules roles, int deadCrewCount, int meetingCount)
	{
		phases = phase;
		maps = map;
		this.roles = roles;
		minDeadCrew = deadCrewCount;
		minMeetingCount = meetingCount;
	}

	public override bool Evaluate(QuickChatContext context)
	{
		if (!phases.Evaluate(context))
		{
			return false;
		}
		if (!maps.Evaluate(context))
		{
			return false;
		}
		if (!roles.Evaluate(context))
		{
			return false;
		}
		if (minDeadCrew > 0 && context.DeadPlayers.Length < minDeadCrew)
		{
			return false;
		}
		if (minMeetingCount > 0 && context.MeetingCount < minMeetingCount)
		{
			return false;
		}
		return true;
	}
}
