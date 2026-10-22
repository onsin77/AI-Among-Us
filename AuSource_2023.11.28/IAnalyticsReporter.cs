using System.Collections.Generic;
using AmongUs.GameOptions;

public interface IAnalyticsReporter
{
	void StartGame(GameData.PlayerInfo localPlayer, int playerCount, int impostorCount, NetworkModes networkMode, MapNames map, GameModes gameMode, string roomCode, string serverName, IGameOptions options, List<GameData.PlayerInfo> players);

	void EndGame(float durationInSeconds, GameOverReason endReason, List<GameData.PlayerInfo> players);

	void VentUsed(GameData.PlayerInfo player);

	void Kill(GameData.PlayerInfo victim, GameData.PlayerInfo killer);

	void SabotageStart(SystemTypes type);

	void SabotageFixed(SystemTypes type);

	void TaskCompleted(GameData.PlayerInfo player, PlayerTask task);

	void MeetingStarted(GameData.PlayerInfo callingPlayer, bool bodyReported);

	void MeetingEnded(float durationInSeconds, GameData.PlayerInfo playerEjected);

	void MinigameOpened(GameData.PlayerInfo callingPlayer, TaskTypes task);

	void MinigameClosed(GameData.PlayerInfo callingPlayer, TaskTypes task, float durationInSeconds, bool isCompleted);
}
