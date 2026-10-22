using System.Collections.Generic;

namespace AmongUs.QuickChat;

public class QuickChatCrewPhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		PlayerControl[] players = context.Players;
		foreach (PlayerControl playerControl in players)
		{
			output.Add(QuickChatPhrase.NewPlayerId(playerControl.PlayerId));
		}
	}
}
