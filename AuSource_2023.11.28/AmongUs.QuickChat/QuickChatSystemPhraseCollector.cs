using System.Collections.Generic;

namespace AmongUs.QuickChat;

public class QuickChatSystemPhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		StringNames[] systems = context.Systems;
		foreach (StringNames localizationKey in systems)
		{
			output.Add(QuickChatPhrase.NewSimplePhrase(localizationKey, isInput: true, isNegative: false));
		}
	}
}
