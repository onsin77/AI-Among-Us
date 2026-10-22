using System;
using UnityEngine;

namespace AmongUs.QuickChat;

[Serializable]
public class QuickChatPhraseInput
{
	[SerializeField]
	private QuickChatTag[] tags;

	public QuickChatTag[] Tags => tags;

	public QuickChatPhraseInput()
	{
	}

	public QuickChatPhraseInput(params QuickChatTag[] tags)
	{
		this.tags = tags;
	}
}
