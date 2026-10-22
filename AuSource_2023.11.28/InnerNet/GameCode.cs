using System.Linq;

namespace InnerNet;

public static class GameCode
{
	public const int V2Flag = int.MinValue;

	public const int MaxGameNumber = 676;

	public static readonly int GameCodeV2MinVersion = Constants.GetVersion(2020, 9, 6, 0);

	private static readonly string V2 = "QWXRTYLPESDFGHUJKZOCVBINMA";

	private static readonly int[] V2Map = (from v in Enumerable.Range(65, 26)
		select V2.IndexOf((char)v)).ToArray();

	public static string IntToGameName(int gameId)
	{
		if (gameId < -1)
		{
			return IntToGameNameV2(gameId);
		}
		char[] array = new char[4]
		{
			(char)(gameId & 0xFF),
			(char)((gameId >> 8) & 0xFF),
			(char)((gameId >> 16) & 0xFF),
			(char)((gameId >> 24) & 0xFF)
		};
		if (array.Any((char c) => c < 'A' || c > 'z'))
		{
			return null;
		}
		return new string(array);
	}

	public static int GameNameToInt(string gameId)
	{
		if (gameId.Length == 6)
		{
			return GameNameToIntV2(gameId);
		}
		if (gameId.Length != 4)
		{
			return -1;
		}
		gameId = gameId.ToUpperInvariant();
		if (gameId.Any((char c) => c < 'A' || c > 'Z'))
		{
			return 0;
		}
		return (int)(gameId[0] | ((uint)gameId[1] << 8) | ((uint)gameId[2] << 16) | ((uint)gameId[3] << 24));
	}

	public static int CreateGameId(int sn, int gn)
	{
		return int.MinValue | (gn & 0x3FF) | ((sn & 0xFFFFF) << 10);
	}

	private static string IntToGameNameV2(int gameId)
	{
		char[] array = new char[6];
		int num = gameId & 0x3FF;
		int num2 = (gameId >>> 10) & 0xFFFFF;
		array[0] = V2[num % 26];
		array[1] = V2[num / 26];
		array[2] = V2[num2 % 26];
		array[3] = V2[num2 / 26 % 26];
		array[4] = V2[num2 / 676 % 26];
		array[5] = V2[num2 / 17576 % 26];
		if (array.Any((char c) => c < 'A' || c > 'z'))
		{
			return null;
		}
		return new string(array);
	}

	private static int GameNameToIntV2(string gameId)
	{
		if (gameId.Length != 6)
		{
			return -1;
		}
		gameId = gameId.ToUpperInvariant();
		if (gameId.Any((char c) => c < 'A' || c > 'z'))
		{
			return 0;
		}
		int gn = V2Map[gameId[0] - 65] + V2Map[gameId[1] - 65] * 26;
		return CreateGameId(V2Map[gameId[2] - 65] + V2Map[gameId[3] - 65] * 26 + V2Map[gameId[4] - 65] * 676 + V2Map[gameId[5] - 65] * 17576, gn);
	}
}
