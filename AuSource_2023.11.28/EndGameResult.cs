using Hazel;

public class EndGameResult
{
	public readonly GameOverReason GameOverReason;

	public readonly bool ShowAd;

	public readonly ProgressionManager.XpGrantResult XpGrantResult;

	public readonly ProgressionManager.CurrencyGrantResult BeansGrantResult;

	public readonly ProgressionManager.CurrencyGrantResult PodsGrantResult;

	public EndGameResult(GameOverReason gameOverReason, bool showAd, ProgressionManager.XpGrantResult xpGrantResult, ProgressionManager.CurrencyGrantResult podsGrantResult, ProgressionManager.CurrencyGrantResult beansGrantResult)
	{
		GameOverReason = gameOverReason;
		ShowAd = showAd;
		XpGrantResult = xpGrantResult;
		BeansGrantResult = beansGrantResult;
		PodsGrantResult = podsGrantResult;
	}

	public EndGameResult(GameOverReason gameOverReason, bool showAd)
	{
		GameOverReason = gameOverReason;
		ShowAd = showAd;
		XpGrantResult = null;
		BeansGrantResult = null;
		PodsGrantResult = null;
	}

	public static EndGameResult Create(MessageReader reader)
	{
		GameOverReason gameOverReason = (GameOverReason)reader.ReadByte();
		bool showAd = reader.ReadBoolean();
		if (reader.Position < reader.Length)
		{
			MessageReader val = reader.ReadMessage();
			if (val.Tag == 1)
			{
				uint oldXpAmount = (uint)val.ReadInt32();
				uint grantedXp = (uint)val.ReadInt32();
				uint num = (uint)val.ReadInt32();
				val.ReadInt32();
				uint oldLevel = (uint)val.ReadInt32();
				uint newLevel = (uint)val.ReadInt32();
				uint maxLevel = (uint)val.ReadInt32();
				string text = val.ReadString();
				val.ReadInt32();
				uint grantedPodsWithMultiplierApplied = (uint)val.ReadInt32();
				val.ReadInt32();
				uint grantedPodsWithMultiplierApplied2 = (uint)val.ReadInt32();
				bool flag = val.ReadBoolean();
				float multiplier = 1f;
				float multiplier2 = 1f;
				uint xpRequiredToLevelUpNextLevel = num;
				if (flag)
				{
					multiplier = val.ReadSingle();
					multiplier2 = val.ReadSingle();
					xpRequiredToLevelUpNextLevel = (uint)val.ReadInt32();
				}
				ProgressionManager.XpGrantResult xpGrantResult = new ProgressionManager.XpGrantResult(grantedXp, oldXpAmount, num, xpRequiredToLevelUpNextLevel, flag, oldLevel, newLevel, maxLevel);
				ProgressionManager.CurrencyGrantResult podsGrantResult = new ProgressionManager.CurrencyGrantResult(text, (uint)DestroyableSingleton<InventoryManager>.Instance.GetPodCount(text), grantedPodsWithMultiplierApplied, multiplier);
				ProgressionManager.CurrencyGrantResult beansGrantResult = new ProgressionManager.CurrencyGrantResult(Constants.Beans, (uint)DestroyableSingleton<InventoryManager>.Instance.UnusedBeans, grantedPodsWithMultiplierApplied2, multiplier2);
				return new EndGameResult(gameOverReason, showAd, xpGrantResult, podsGrantResult, beansGrantResult);
			}
		}
		return new EndGameResult(gameOverReason, showAd);
	}
}
