using System.Collections.Generic;

namespace AmongUs.QuickChat;

public class QuickChatLocationPhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		StringNames[] locations = context.Locations;
		foreach (StringNames localizationKey in locations)
		{
			output.Add(QuickChatPhrase.NewSimplePhrase(localizationKey, isInput: true, isNegative: false));
		}
	}
}
