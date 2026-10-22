using System;
using UnityEngine;

namespace AmongUs.QuickChat;

[Serializable]
public class QuickChatPhaseRules : AbstractQuickChatPhraseRuleGroup
{
	[SerializeField]
	private bool inLobby;

	[SerializeField]
	private bool inGame;

	public QuickChatPhaseRules(bool inLobby, bool inGame)
	{
		this.inLobby = inLobby;
		this.inGame = inGame;
	}

	public override bool Evaluate(QuickChatContext context)
	{
		switch (context.Phase)
		{
		default:
			QuickChatMenu.Logger.Error(string.Format("[{0}] {1}.{2} has not been configured. Return FALSE.", "QuickChatPhaseRules", "QuickChatContextPhase", context.Phase));
			return false;
		case QuickChatContextPhase.Lobby:
			return inLobby;
		case QuickChatContextPhase.Game:
			return inGame;
		}
	}
}
