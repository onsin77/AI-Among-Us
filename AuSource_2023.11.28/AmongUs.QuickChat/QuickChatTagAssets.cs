using System.Collections.Generic;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatTagAssets : ScriptableObject
{
	[Header("Tags")]
	[SerializeField]
	private QuickChatTag accusation;

	[SerializeField]
	private QuickChatTag crew;

	[SerializeField]
	private QuickChatTag deadCrew;

	[SerializeField]
	private QuickChatTag defense;

	[SerializeField]
	private QuickChatTag favorite;

	[SerializeField]
	private QuickChatTag location;

	[SerializeField]
	private QuickChatTag question;

	[SerializeField]
	private QuickChatTag remark;

	[SerializeField]
	private QuickChatTag role;

	[SerializeField]
	private QuickChatTag self;

	[SerializeField]
	private QuickChatTag system;

	[SerializeField]
	private QuickChatTag task;

	[Header("Lobby Tags")]
	[SerializeField]
	private QuickChatTag lobbyImpostor;

	[SerializeField]
	private QuickChatTag lobbyMeeting;

	[SerializeField]
	private QuickChatTag lobbyHiding;

	[SerializeField]
	private QuickChatTag lobbyFlashlight;

	[Header("Collections")]
	[SerializeField]
	private QuickChatTag[] visibleInClassicLobby;

	[SerializeField]
	private QuickChatTag[] visibleInHideAndSeekLobby;

	[SerializeField]
	private QuickChatTag[] visibleInGame;

	[SerializeField]
	private QuickChatTag[] all;

	public QuickChatTag Accusation => accusation;

	public QuickChatTag Crew => crew;

	public QuickChatTag DeadCrew => deadCrew;

	public QuickChatTag Defense => defense;

	public QuickChatTag Favorite => favorite;

	public QuickChatTag Location => location;

	public QuickChatTag Question => question;

	public QuickChatTag Remark => remark;

	public QuickChatTag Role => role;

	public QuickChatTag Self => self;

	public QuickChatTag System => system;

	public QuickChatTag Task => task;

	public QuickChatTag LobbyImpostor => lobbyImpostor;

	public QuickChatTag LobbyMeeting => lobbyMeeting;

	public QuickChatTag LobbyHiding => lobbyHiding;

	public QuickChatTag LobbyFlashlight => lobbyFlashlight;

	public QuickChatTag[] VisibleInClassicLobby => visibleInClassicLobby;

	public QuickChatTag[] VisibleInHideAndSeekLobby => visibleInHideAndSeekLobby;

	public QuickChatTag[] VisibleInGame => visibleInGame;

	public QuickChatTag[] All => all;

	public QuickChatTag GetTagByName(string name)
	{
		QuickChatTag[] array = All;
		foreach (QuickChatTag quickChatTag in array)
		{
			string text = ((Object)quickChatTag).name.Replace("QCTag_", "");
			if (name == text)
			{
				return quickChatTag;
			}
		}
		return null;
	}

	public QuickChatTag[] GetTagsByParsingKeys(params string[] parsingKeys)
	{
		if (parsingKeys == null || parsingKeys.Length < 1)
		{
			return null;
		}
		List<QuickChatTag> list = new List<QuickChatTag>();
		QuickChatTag[] array = All;
		foreach (QuickChatTag quickChatTag in array)
		{
			foreach (string key in parsingKeys)
			{
				if (quickChatTag.ContainsParsingKey(key) && !list.Contains(quickChatTag))
				{
					list.Add(quickChatTag);
				}
			}
		}
		return list.ToArray();
	}
}
