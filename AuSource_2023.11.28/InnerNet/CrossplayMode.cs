using AmongUs.Data;

namespace InnerNet;

public static class CrossplayMode
{
	public static int GetCrossplayFlags()
	{
		if (DataManager.Settings.Multiplayer.CrossPlay)
		{
			return int.MaxValue;
		}
		return 4;
	}
}
