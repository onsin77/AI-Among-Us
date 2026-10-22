using System.Collections.Generic;

namespace AmongUs.QuickChat;

public interface IQuickChatPhraseCollector
{
	void Collect(QuickChatContext context, List<QuickChatPhrase> output);
}
