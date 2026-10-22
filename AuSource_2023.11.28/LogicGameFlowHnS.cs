using System.Collections;
using AmongUs.Data;
using Hazel;
using UnityEngine;

public class LogicGameFlowHnS : LogicGameFlow
{
	private const int SECONDS_TO_BEEP = 10;

	private const float SECONDS_TO_SET_DIRTY = 1f;

	private readonly HideAndSeekManager hideAndSeekManager;

	private readonly HideAndSeekTimerBar timerBarPrefab;

	private HideAndSeekTimerBar timerBar;

	private float totalHideTime = float.MaxValue;

	private float currentHideTime = float.MaxValue;

	private float totalFinalHideTime = float.MaxValue;

	private float currentFinalHideTime = float.MaxValue;

	private float secondsSinceLastSetDirty;

	private Coroutine beepCoroutine;

	public float CurrentFinalHideTime => currentFinalHideTime;

	public float TotalFinalHideTime => totalFinalHideTime;

	public bool IsFinalCountdown => currentHideTime <= 0f;

	public LogicGameFlowHnS(HideAndSeekManager manager, HideAndSeekTimerBar timerBarPrefab)
		: base(manager)
	{
		hideAndSeekManager = manager;
		this.timerBarPrefab = timerBarPrefab;
	}

	public bool SeekerAdminMapEnabled(PlayerControl player)
	{
		int item = GetPlayerCounts().aliveHumans;
		if (player.inVent)
		{
			return false;
		}
		if (player.Data == null || (Object)(object)player.Data.Role == (Object)null)
		{
			return false;
		}
		if (!player.inVent && player.Data.Role.IsImpostor && IsFinalCountdown && hideAndSeekManager.LogicOptionsHnS.GetSeekerFinalMap())
		{
			return true;
		}
		if (player.Data.Role.IsImpostor)
		{
			return item <= (GameData.Instance.PlayerCount - 1) / 3;
		}
		return false;
	}

	public void OnTaskComplete(float timeDeduction)
	{
		if ((Object)(object)timerBar != (Object)null)
		{
			timerBar.TaskComplete();
		}
		AdjustEscapeTimer(timeDeduction, forceDirty: true);
	}

	public override void OnGameStart()
	{
		totalHideTime = hideAndSeekManager.LogicOptionsHnS.GetEscapeTime();
		currentHideTime = totalHideTime;
		totalFinalHideTime = hideAndSeekManager.LogicOptionsHnS.GetFinalCountdownTime();
		currentFinalHideTime = totalFinalHideTime;
		if ((Object)(object)timerBar != (Object)null)
		{
			Object.Destroy((Object)(object)timerBar);
		}
		timerBar = Object.Instantiate<HideAndSeekTimerBar>(timerBarPrefab, ((Component)DestroyableSingleton<HudManager>.Instance).transform.parent);
		SetDirty();
	}

	public float GetTotalRoundTime()
	{
		float escapeTime = hideAndSeekManager.LogicOptionsHnS.GetEscapeTime();
		float finalCountdownTime = hideAndSeekManager.LogicOptionsHnS.GetFinalCountdownTime();
		return escapeTime + finalCountdownTime;
	}

	public float GetTotalTimeRemaining()
	{
		return currentHideTime + currentFinalHideTime;
	}

	public float GetRoundTimeElapsed()
	{
		return GetTotalRoundTime() - GetTotalTimeRemaining();
	}

	public override void OnGameEnd()
	{
		if ((Object)(object)timerBar != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)timerBar).gameObject);
		}
		if (beepCoroutine != null)
		{
			((MonoBehaviour)Manager).StopCoroutine(beepCoroutine);
		}
		beepCoroutine = null;
	}

	public override void FixedUpdate()
	{
		secondsSinceLastSetDirty += Time.fixedDeltaTime;
		if (IsFinalCountdown)
		{
			AdjustFinalEscapeTimer(Time.fixedDeltaTime);
		}
		else
		{
			AdjustEscapeTimer(Time.fixedDeltaTime, forceDirty: false);
		}
	}

	public override void OnDestroy()
	{
		if ((Object)(object)timerBar != (Object)null)
		{
			Object.Destroy((Object)(object)timerBar);
		}
	}

	public override bool IsGameOverDueToDeath()
	{
		var (num, num2, num3) = GetPlayerCounts();
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			return true;
		}
		if (num <= 0)
		{
			return true;
		}
		return false;
	}

	public override void CheckEndCriteria()
	{
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		var (num, num2, _) = GetPlayerCounts();
		if (num2 <= 0 && !DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Manager.RpcEndGame(GameOverReason.ImpostorDisconnect, !DataManager.Player.Ads.HasPurchasedAdRemoval);
		}
		if (num <= 0)
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				Manager.RpcEndGame(GameOverReason.HideAndSeek_ByKills, !DataManager.Player.Ads.HasPurchasedAdRemoval);
				return;
			}
			DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorKills));
			Manager.ReviveEveryoneFreeplay();
		}
		else if (!DestroyableSingleton<TutorialManager>.InstanceExists && AllTimersExpired())
		{
			Manager.RpcEndGame(GameOverReason.HideAndSeek_ByTimer, !DataManager.Player.Ads.HasPurchasedAdRemoval);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(currentHideTime);
		writer.Write(currentFinalHideTime);
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		float num = reader.ReadSingle();
		currentFinalHideTime = reader.ReadSingle();
		if (num <= 0f && currentHideTime > 0f)
		{
			OnFinalCountdownTriggered();
		}
		currentHideTime = num;
	}

	private void OnFinalCountdownTriggered()
	{
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!allPlayerControl.Data.Role.IsImpostor && !allPlayerControl.Data.IsDead)
			{
				allPlayerControl.ClearTasks();
				PlayerTask.GetOrCreateTask<ImportantTextTask>(allPlayerControl).Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HideActionButton);
			}
		}
		if (!PlayerControl.LocalPlayer.Data.IsDead && (Object)(object)Minigame.Instance != (Object)null)
		{
			Minigame.Instance?.ForceClose();
		}
		timerBar.StartFinalHide();
		SoundManager.Instance.PlaySound(hideAndSeekManager.FinalHideAlertSFX, loop: false);
		DestroyableSingleton<HudManager>.Instance.SetAlertOverlay(enabled: true);
	}

	private void AdjustEscapeTimer(float timeDeduction, bool forceDirty)
	{
		float num = currentHideTime;
		currentHideTime -= timeDeduction;
		currentHideTime = Mathf.Max(currentHideTime, 0f);
		if (currentHideTime <= 10f && beepCoroutine == null)
		{
			beepCoroutine = ((MonoBehaviour)hideAndSeekManager).StartCoroutine(BeepAlmostEverySecond());
		}
		if (num > 0f && currentHideTime <= 0f)
		{
			OnFinalCountdownTriggered();
		}
		timerBar.UpdateTimer(currentHideTime, totalHideTime);
		if (forceDirty || secondsSinceLastSetDirty > 1f)
		{
			SetDirty();
			secondsSinceLastSetDirty = 0f;
		}
	}

	private IEnumerator BeepAlmostEverySecond()
	{
		while (!IsFinalCountdown)
		{
			float num = currentHideTime / 10f;
			float pitch = 1.5f - num / 2f;
			SoundManager.Instance.PlaySoundImmediate(hideAndSeekManager.FinalHideCountdownSFX, loop: false, 1f, pitch);
			yield return (object)new WaitForSeconds(1f);
		}
		yield return Effects.Wait(currentFinalHideTime - 10f);
		while (currentFinalHideTime > 0f)
		{
			float num2 = currentFinalHideTime / 10f;
			float pitch2 = 1.5f - num2 / 2f;
			SoundManager.Instance.PlaySoundImmediate(hideAndSeekManager.FinalHideCountdownSFX, loop: false, 1f, pitch2);
			yield return (object)new WaitForSeconds(1f);
		}
	}

	private void AdjustFinalEscapeTimer(float timeDeduction)
	{
		currentFinalHideTime -= timeDeduction;
		currentFinalHideTime = Mathf.Max(currentFinalHideTime, 0f);
		timerBar.UpdateTimer(currentFinalHideTime, totalFinalHideTime);
		if (secondsSinceLastSetDirty > 1f)
		{
			SetDirty();
			secondsSinceLastSetDirty = 0f;
		}
	}

	private bool AllTimersExpired()
	{
		if (currentHideTime <= 0f)
		{
			return currentFinalHideTime <= 0f;
		}
		return false;
	}
}
