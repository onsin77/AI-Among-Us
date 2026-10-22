using System.Collections.Generic;

namespace AmongUs.QuickChat;

public class QuickChatRolePhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		StringNames[] roles = context.Roles;
		foreach (StringNames localizationKey in roles)
		{
			output.Add(QuickChatPhrase.NewSimplePhrase(localizationKey, isInput: true, isNegative: false));
		}
	}
}
