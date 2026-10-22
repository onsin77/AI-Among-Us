using System.Collections.Generic;

public static class TempData
{
	public static DeathReason LastDeathReason;

	public static float TimeGameStarted;

	public static float TimeLastMeetingStarted;

	public static GameOverReason EndReason = GameOverReason.HumansByTask;

	public static bool showAd;

	public static List<WinningPlayerData> winners = new List<WinningPlayerData>();

	public static ProgressionManager.XpGrantResult XpGrantedResult;

	public static ProgressionManager.CurrencyGrantResult PodsGrantResult;

	public static ProgressionManager.CurrencyGrantResult BeansGrantResult;

	public static int MeetingCount = 0;

	public static int RoundsPlayedInSession = 0;

	public static void OnMeetingStart()
	{
		MeetingCount++;
	}

	public static void OnGameEnd()
	{
		RoundsPlayedInSession++;
		Reset();
	}

	public static void Reset()
	{
		winners = new List<WinningPlayerData>();
		XpGrantedResult = null;
		PodsGrantResult = null;
		BeansGrantResult = null;
		MeetingCount = 0;
	}

	public static void OnDisconnected()
	{
		RoundsPlayedInSession = 0;
	}
}
