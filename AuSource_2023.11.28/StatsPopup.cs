using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;

public class StatsPopup : MonoBehaviour
{
	public TextMeshPro StatsText;

	public TextMeshPro NumbersText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private static readonly List<StringNames> BaseStatsToShow = new List<StringNames>
	{
		StringNames.StatsBodiesReported,
		StringNames.StatsEmergenciesCalled,
		StringNames.StatsTasksCompleted,
		StringNames.StatsAllTasksCompleted,
		StringNames.StatsSabotagesFixed,
		StringNames.StatsImpostorKills,
		StringNames.StatsTimesMurdered,
		StringNames.StatsTimesEjected,
		StringNames.StatsCrewmateStreak,
		StringNames.StatsGamesImpostor,
		StringNames.StatsGamesCrewmate,
		StringNames.StatsGamesStarted,
		StringNames.StatsGamesFinished,
		StringNames.StatsHidenSeekGamesCrewmateSurvived,
		StringNames.StatsHidenSeekTimesVented,
		StringNames.StatsTimesPettedPet,
		StringNames.StatsImpostorKills_HideAndSeek
	};

	private static readonly List<StringNames> RoleSpecificStatsToShow = new List<StringNames>
	{
		StringNames.StatsEngineerVents,
		StringNames.StatsGuardianAngelCrewmatesProtected,
		StringNames.StatsScientistChargesGained,
		StringNames.StatsShapeshifterShiftedKills
	};

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void OnEnable()
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		StringBuilder stringBuilder2 = new StringBuilder(256);
		foreach (StringNames item in BaseStatsToShow)
		{
			AppendStat(stringBuilder, stringBuilder2, item, StatsManager.Instance.GetStat(item));
		}
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsFastestCrewmateWin_HideAndSeek, GetFloatStatStr(StatsManager.Instance.GetFastestHideAndSeekCrewmateWin()));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsFastestImpostorWin_HideAndSeek, GetFloatStatStr(StatsManager.Instance.GetFastestHideAndSeekImpostorWin()));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorVoteWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorByVote));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorKillsWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorByKill));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorSabotageWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorBySabotage));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsCrewmateVoteWins, StatsManager.Instance.GetWinReason(GameOverReason.HumansByVote));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsCrewmateTaskWins, StatsManager.Instance.GetWinReason(GameOverReason.HumansByTask));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsHideAndSeekCrewmateVictory, StatsManager.Instance.GetWinReason(GameOverReason.HideAndSeek_ByTimer));
		AppendStat(stringBuilder, stringBuilder2, StringNames.StatsHideAndSeekImpostorVictory, StatsManager.Instance.GetWinReason(GameOverReason.HideAndSeek_ByKills));
		RoleTypes[] array = Enum.GetValues(typeof(RoleTypes)).Cast<RoleTypes>().ToArray();
		foreach (RoleTypes roleTypes in array)
		{
			if (roleTypes != RoleTypes.ImpostorGhost && roleTypes != RoleTypes.CrewmateGhost)
			{
				AppendStat(stringBuilder, stringBuilder2, StringNames.StatsRoleWins, StatsManager.Instance.GetRoleWinCount(roleTypes), DestroyableSingleton<RoleManager>.Instance.GetRole(roleTypes)?.NiceName ?? ((object)roleTypes));
			}
		}
		foreach (StringNames item2 in RoleSpecificStatsToShow)
		{
			AppendStat(stringBuilder, stringBuilder2, item2, StatsManager.Instance.GetStat(item2));
		}
		((TMP_Text)StatsText).text = stringBuilder.ToString();
		((TMP_Text)NumbersText).text = stringBuilder2.ToString();
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton);
	}

	private static object GetFloatStatStr(float stat)
	{
		if (float.IsNaN(stat))
		{
			return "---";
		}
		return Mathf.Round(stat * 1000f) * 0.001f;
	}

	private static void AppendStat(StringBuilder str, StringBuilder strNums, StringNames statName, object stat, params object[] additionalTranslationParams)
	{
		str.AppendLine(DestroyableSingleton<TranslationController>.Instance.GetString(statName, additionalTranslationParams));
		strNums.Append(stat);
		strNums.AppendLine();
	}
}
