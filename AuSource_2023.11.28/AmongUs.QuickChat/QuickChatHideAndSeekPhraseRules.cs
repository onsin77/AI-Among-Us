using System;
using UnityEngine;

namespace AmongUs.QuickChat;

[Serializable]
public class QuickChatHideAndSeekPhraseRules : AbstractQuickChatPhraseRuleGroup
{
	[SerializeField]
	private QuickChatPhaseRules phases;

	[SerializeField]
	private QuickChatMapRules maps;

	public QuickChatHideAndSeekPhraseRules(QuickChatPhaseRules phase, QuickChatMapRules map)
	{
		phases = phase;
		maps = map;
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
		return true;
	}
}
