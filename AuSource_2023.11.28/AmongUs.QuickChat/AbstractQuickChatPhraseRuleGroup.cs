using System;

namespace AmongUs.QuickChat;

[Serializable]
public abstract class AbstractQuickChatPhraseRuleGroup
{
	public abstract bool Evaluate(QuickChatContext context);
}
