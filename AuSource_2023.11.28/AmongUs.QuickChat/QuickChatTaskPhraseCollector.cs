using System.Collections.Generic;

namespace AmongUs.QuickChat;

public class QuickChatTaskPhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		StringNames[] tasks = context.Tasks;
		foreach (StringNames localizationKey in tasks)
		{
			output.Add(QuickChatPhrase.NewSimplePhrase(localizationKey, isInput: true, isNegative: false));
		}
	}
}
