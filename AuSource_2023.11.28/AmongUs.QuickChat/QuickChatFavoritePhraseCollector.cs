using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatFavoritePhraseCollector : IQuickChatPhraseCollector
{
	public void Collect(QuickChatContext context, List<QuickChatPhrase> output)
	{
		QuickChatPhrase[] phrases = context.Data.GetPhrases(context, context.Data.Tags.All.Where((QuickChatTag x) => (Object)(object)x != (Object)(object)context.Data.Tags.Favorite).ToArray());
		foreach (QuickChatPhrase quickChatPhrase in phrases)
		{
			if (DataManager.Settings.Multiplayer.HasQuickChatFavorite(quickChatPhrase.PhraseKey) && !output.Contains(quickChatPhrase))
			{
				output.Add(quickChatPhrase);
			}
		}
	}
}
