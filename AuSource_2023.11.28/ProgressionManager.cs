using AmongUs.Data;
using UnityEngine;

public class ProgressionManager
{
	public class XpGrantResult
	{
		public readonly uint GrantedXp;

		public readonly uint OldXpAmount;

		public readonly uint XpRequiredToLevelUp;

		public readonly uint XpRequiredToLevelUpNextLevel;

		public readonly uint OldLevel;

		public readonly uint NewLevel;

		public readonly uint MaxLevel;

		public readonly bool LevelledUp;

		public XpGrantResult(uint grantedXp, uint oldXpAmount, uint xpRequiredToLevelUp, uint xpRequiredToLevelUpNextLevel, bool levelledUp, uint oldLevel, uint newLevel, uint maxLevel)
		{
			GrantedXp = grantedXp;
			OldXpAmount = oldXpAmount;
			XpRequiredToLevelUp = xpRequiredToLevelUp;
			XpRequiredToLevelUpNextLevel = xpRequiredToLevelUpNextLevel;
			LevelledUp = levelledUp;
			OldLevel = oldLevel;
			NewLevel = newLevel;
			MaxLevel = maxLevel;
		}

		public static XpGrantResult Default()
		{
			return new XpGrantResult(0u, DataManager.Player.Stats.Xp, DataManager.Player.Stats.XpForNextLevel, DataManager.Player.Stats.XpForNextLevel, levelledUp: false, DataManager.Player.Stats.Level, DataManager.Player.Stats.Level, 99u);
		}
	}

	public class CurrencyGrantResult
	{
		public readonly string PodId;

		public readonly uint OldPodAmount;

		public readonly uint GrantedPodsWithMultiplierApplied;

		public readonly float Multiplier;

		public uint GrantedPodsPreMultiplier
		{
			get
			{
				if (Mathf.Approximately(Multiplier, 0f))
				{
					return GrantedPodsWithMultiplierApplied;
				}
				return (uint)((float)GrantedPodsWithMultiplierApplied / Multiplier);
			}
		}

		public CurrencyGrantResult(string podId, uint oldPodAmount, uint grantedPodsWithMultiplierApplied, float multiplier)
		{
			PodId = podId;
			OldPodAmount = oldPodAmount;
			GrantedPodsWithMultiplierApplied = grantedPodsWithMultiplierApplied;
			Multiplier = multiplier;
		}

		public static CurrencyGrantResult Default()
		{
			return new CurrencyGrantResult(string.Empty, 0u, 0u, 0f);
		}
	}

	public const int FirstLevel = 0;

	public static ProgressionManager Instance = new ProgressionManager();

	private const uint MaxLevel = 99u;

	public string CurrentVisualLevel => (DataManager.Player.Stats.Level + 1).ToString();

	public static string FormatVisualLevel(uint playerLevel)
	{
		if (playerLevel == uint.MaxValue)
		{
			return 1.ToString();
		}
		return (playerLevel + 1).ToString();
	}
}
