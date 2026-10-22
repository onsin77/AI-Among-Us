using System.Collections.Generic;
using AmongUs.GameOptions;

public static class GameModesHelpers
{
	public static Dictionary<GameModes, StringNames> ModeToName = new Dictionary<GameModes, StringNames>
	{
		{
			GameModes.None,
			StringNames.GameTypeError
		},
		{
			GameModes.Normal,
			StringNames.GameTypeClassic
		},
		{
			GameModes.HideNSeek,
			StringNames.GameTypeHideAndSeek
		}
	};
}
