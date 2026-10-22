using System.Collections.Generic;
using AmongUs.GameOptions;

public class DummyAnalytics : IAnalyticsReporter
{
	public void StartGame(GameData.PlayerInfo localPlayer, int playerCount, int impostorCount, NetworkModes networkMode, MapNames map, GameModes gameMode, string roomCode, string serverName, IGameOptions options, List<GameData.PlayerInfo> players)
	{
	}

	public void EndGame(float durationInSeconds, GameOverReason endReason, List<GameData.PlayerInfo> players)
	{
	}

	public void VentUsed(GameData.PlayerInfo player)
	{
	}

	public void Kill(GameData.PlayerInfo victim, GameData.PlayerInfo killer)
	{
	}

	public void SabotageStart(SystemTypes type)
	{
	}

	public void SabotageFixed(SystemTypes type)
	{
	}

	public void TaskCompleted(GameData.PlayerInfo player, PlayerTask task)
	{
	}

	public void MeetingStarted(GameData.PlayerInfo callingPlayer, bool bodyReported)
	{
	}

	public void MeetingEnded(float durationInSeconds, GameData.PlayerInfo playerEjected)
	{
	}

	public void MinigameOpened(GameData.PlayerInfo callingPlayer, TaskTypes task)
	{
	}

	public void MinigameClosed(GameData.PlayerInfo callingPlayer, TaskTypes task, float durationInSeconds, bool isCompleted)
	{
	}
}
