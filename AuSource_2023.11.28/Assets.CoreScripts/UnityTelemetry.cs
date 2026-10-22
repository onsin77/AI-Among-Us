using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.CoreScripts;

public class UnityTelemetry : DestroyableSingleton<UnityTelemetry>
{
	private static readonly string[] ColorNames = new string[23]
	{
		"Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown",
		"Cyan", "Lime", "Mahogany", "Rose", "Banana", "Gray", "Tan", "Lilac", "Aqua", "Cobalt",
		"Barf", "Salmon", "Chocolate"
	};

	private bool amHost;

	private bool gameStarted;

	private DateTime timeStarted;

	public void Init()
	{
		gameStarted = true;
	}

	public void StartGame(bool isHost, int playerCount, int impostorCount, NetworkModes gameMode, uint timesImpostor, uint gamesPlayed, uint crewStreak)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		amHost = isHost;
		timeStarted = DateTime.UtcNow;
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{
				"Platform",
				(byte)Application.platform
			},
			{ "TimesImpostor", timesImpostor },
			{ "CrewStreak", crewStreak },
			{ "GamesPlayed", gamesPlayed },
			{ "GameMode", gameMode }
		};
		if (amHost)
		{
			dictionary.Add("PlayerCount", playerCount);
			dictionary.Add("InfectedCount", impostorCount);
		}
		Analytics.CustomEvent("StartGame", (IDictionary<string, object>)dictionary);
	}

	public void StartGameCosmetics(int colorId, string hatId, string skinId, string petId, string visorId, string nameplateId)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{
				"Color",
				ColorNames[colorId]
			},
			{
				"Hat",
				((Object)DestroyableSingleton<HatManager>.Instance.GetHatById(hatId)).name
			},
			{
				"Skin",
				((Object)DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId)).name
			},
			{
				"Pet",
				((Object)DestroyableSingleton<HatManager>.Instance.GetPetById(petId)).name
			},
			{
				"Visor",
				((Object)DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId)).name
			},
			{
				"Nameplate",
				((Object)DestroyableSingleton<HatManager>.Instance.GetNamePlateById(nameplateId)).name
			}
		};
		Analytics.CustomEvent("StartGameCosmetics", (IDictionary<string, object>)dictionary);
	}

	public void WriteMeetingStarted(bool isEmergency)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (amHost)
		{
			Analytics.CustomEvent("MeetingStarted", (IDictionary<string, object>)new Dictionary<string, object> { { "IsEmergency", isEmergency } });
		}
	}

	public void WriteMeetingEnded(float duration)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (amHost)
		{
			Analytics.CustomEvent("MeetingEnded", (IDictionary<string, object>)new Dictionary<string, object> { { "IsEmergency", duration } });
		}
	}

	public void WritePosition(byte playerNum, Vector2 worldPos)
	{
	}

	public void WriteMurder()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("Murder");
		}
	}

	public void WriteSabotageUsed(SystemTypes systemType)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("SabotageUsed", (IDictionary<string, object>)new Dictionary<string, object> { { "SystemType", systemType } });
		}
	}

	public void CardSwipeComplete(int attempts)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("CardSwipeComplete", (IDictionary<string, object>)new Dictionary<string, object> { { "Attempts", attempts } });
		}
	}

	public void WriteUse(byte playerNum, TaskTypes taskType, Vector3 worldPos)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("ConsoleUsed", (IDictionary<string, object>)new Dictionary<string, object> { { "TaskType", taskType } });
		}
	}

	public void WriteCompleteTask(TaskTypes taskType)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("TaskComplete", (IDictionary<string, object>)new Dictionary<string, object> { { "TaskType", taskType } });
		}
	}

	internal void WriteDisconnect(DisconnectReasons reason)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("Disconnect", (IDictionary<string, object>)new Dictionary<string, object> { { "Reason", reason } });
		}
	}

	public void EndGame(GameOverReason endReason)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "Reason", endReason } };
			if (amHost)
			{
				dictionary.Add("DurationSec", (DateTime.UtcNow - timeStarted).TotalSeconds);
			}
			Analytics.CustomEvent("EndGame", (IDictionary<string, object>)dictionary);
		}
	}

	public void SendWho()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("SentWho");
		}
	}

	public void WonGame(int colorId, string hatId, string skinId, string petId, string visorId, string nameplateId)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (gameStarted)
		{
			Analytics.CustomEvent("WonGame", (IDictionary<string, object>)new Dictionary<string, object>
			{
				{
					"Color",
					ColorNames[colorId]
				},
				{
					"Hat",
					((Object)DestroyableSingleton<HatManager>.Instance.GetHatById(hatId)).name
				},
				{
					"Skin",
					((Object)DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId)).name
				},
				{
					"Pet",
					((Object)DestroyableSingleton<HatManager>.Instance.GetPetById(petId)).name
				},
				{
					"Visor",
					((Object)DestroyableSingleton<HatManager>.Instance.GetVisorById(visorId)).name
				},
				{
					"Nameplate",
					((Object)DestroyableSingleton<HatManager>.Instance.GetNamePlateById(nameplateId)).name
				}
			});
		}
	}
}
