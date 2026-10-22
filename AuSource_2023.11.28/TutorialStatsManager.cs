public class TutorialStatsManager : StatsManager
{
	protected override void LoadStats()
	{
		if (stats == null)
		{
			stats = new Stats();
		}
	}

	protected override void SaveStats()
	{
	}
}
