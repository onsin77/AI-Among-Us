using System;
using System.Collections;
using AmongUs.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionScreen : MonoBehaviour
{
	private const float TimeToFillSeconds = 0.5f;

	private const float TimeToGlowSeconds = 0.75f;

	public PoolablePlayer Player;

	[Header("Xp")]
	public AudioClip XpSound;

	public AudioClip LevelUpSound;

	public ProgressBar XpBar;

	public TextMeshPro XpEarnedNowText;

	public AnimationCurve XpEarnedNowCurve;

	public TextMeshPro LevelText;

	public Transform LevelCircle;

	public TextMeshPro FutureLevelText;

	[Header("Pods")]
	public CurrencyEarned PodsEarned;

	[Header("Beans")]
	public CurrencyEarned BeansEarned;

	public void Activate()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		ProgressionManager.XpGrantResult xpGrantedResult = TempData.XpGrantedResult;
		uint oldLevel = xpGrantedResult.OldLevel;
		uint newLevel = xpGrantedResult.NewLevel;
		XpBar.Value = xpGrantedResult.OldXpAmount;
		XpBar.CapValue = xpGrantedResult.OldXpAmount;
		XpBar.MaxValue = xpGrantedResult.XpRequiredToLevelUp;
		XpBar.GlowAlpha = 0f;
		((TMP_Text)XpEarnedNowText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.XpGainedValue, xpGrantedResult.GrantedXp);
		((Graphic)XpEarnedNowText).color = Palette.ClearWhite;
		if (oldLevel == xpGrantedResult.MaxLevel)
		{
			XpBar.Value = 1f;
			XpBar.CapValue = 1f;
			XpBar.MaxValue = 1f;
			((TMP_Text)LevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Max);
			((Graphic)FutureLevelText).color = Palette.ClearWhite;
		}
		else if (newLevel == xpGrantedResult.MaxLevel)
		{
			((TMP_Text)FutureLevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Max);
			((TMP_Text)LevelText).text = ProgressionManager.FormatVisualLevel(oldLevel);
		}
		else
		{
			((TMP_Text)LevelText).text = ProgressionManager.FormatVisualLevel(oldLevel);
			string text = ProgressionManager.FormatVisualLevel((newLevel == oldLevel) ? (oldLevel + 1) : newLevel);
			((TMP_Text)FutureLevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LevelShorthand, text);
		}
		ProgressionManager.CurrencyGrantResult podsGrantResult = TempData.PodsGrantResult;
		CosmicubeData dataForPodId = DestroyableSingleton<CosmicubeManager>.Instance.GetDataForPodId(podsGrantResult.PodId);
		if (!((Object)(object)dataForPodId == (Object)null) && !(dataForPodId.GetCosmicubeName() == "Empty Cosmicube") && podsGrantResult.GrantedPodsWithMultiplierApplied != 0)
		{
			PodsEarned.Initialize(podsGrantResult);
		}
		else
		{
			((Component)PodsEarned).gameObject.SetActive(false);
		}
		ProgressionManager.CurrencyGrantResult beansGrantResult = TempData.BeansGrantResult;
		if (beansGrantResult.GrantedPodsWithMultiplierApplied != 0)
		{
			BeansEarned.Initialize(beansGrantResult);
		}
		else
		{
			((Component)BeansEarned).gameObject.SetActive(false);
		}
		if (((Component)BeansEarned).gameObject.activeSelf != ((Component)PodsEarned).gameObject.activeSelf)
		{
			Transform val = (((Component)BeansEarned).gameObject.activeSelf ? ((Component)BeansEarned).transform : ((Component)PodsEarned).transform);
			val.localPosition = new Vector3(0f, val.localPosition.y, ((Component)this).transform.localPosition.z);
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		Player.UpdateFromEitherPlayerDataOrCache(data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, includePet: true);
		Player.ToggleName(active: false);
		((MonoBehaviour)this).StartCoroutine(DoAnimations(xpGrantedResult));
	}

	private IEnumerator DoAnimations(ProgressionManager.XpGrantResult xpGainedResult)
	{
		yield return Effects.Wait(0.2f);
		if ((xpGainedResult.MaxLevel != DataManager.Player.Stats.Level || xpGainedResult.OldLevel != xpGainedResult.NewLevel) && xpGainedResult.GrantedXp != 0)
		{
			yield return AnimateXpAndLevelUp(xpGainedResult);
		}
		yield return AnimatePodsAndBeans();
	}

	private IEnumerator AnimateXpAndLevelUp(ProgressionManager.XpGrantResult xpGainedResult)
	{
		ulong newXpAmount = Math.Min(xpGainedResult.XpRequiredToLevelUp, xpGainedResult.OldXpAmount + xpGainedResult.GrantedXp);
		uint oldXpAmount = xpGainedResult.OldXpAmount;
		XpBar.CapValue = newXpAmount;
		((MonoBehaviour)this).StartCoroutine(Effects.Lerp(1.25f, (float t) =>
		{
			XpEarnedNowText.transform.SetLocalX(0.9f + XpEarnedNowCurve.Evaluate(t));
		}));
		((MonoBehaviour)this).StartCoroutine(Effects.ColorFade(XpEarnedNowText, Palette.ClearWhite, Color.white, 0.625f));
		((MonoBehaviour)this).StartCoroutine(AnimateXpBarFill(oldXpAmount, newXpAmount));
		yield return Effects.Wait(0.625f);
		yield return Effects.ColorFade(XpEarnedNowText, Color.white, Palette.ClearWhite, 0.625f);
		if (xpGainedResult.LevelledUp)
		{
			if (DataManager.Player.Stats.Level >= xpGainedResult.MaxLevel)
			{
				((TMP_Text)LevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MaxLevel);
				((Graphic)FutureLevelText).color = Palette.ClearWhite;
			}
			else
			{
				((TMP_Text)LevelText).text = ProgressionManager.Instance.CurrentVisualLevel;
				XpBar.MaxValue = xpGainedResult.XpRequiredToLevelUpNextLevel;
			}
			newXpAmount = xpGainedResult.GrantedXp + xpGainedResult.OldXpAmount - xpGainedResult.XpRequiredToLevelUp;
			XpBar.Value = 0f;
			XpBar.GlowAlpha = 1f;
			XpBar.CapValue = xpGainedResult.XpRequiredToLevelUpNextLevel;
			XpBar.MaxValue = xpGainedResult.XpRequiredToLevelUpNextLevel;
			((Component)XpEarnedNowText).gameObject.SetActive(false);
			SoundManager.Instance.PlaySound(LevelUpSound, loop: false, 0.5f);
			yield return Effects.ScaleIn(LevelCircle, 2f, 1f, 0.5f);
			yield return Effects.All(XpBar.ScaleDownY(0.75f), Effects.Lerp(0.75f, (float t) =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				Transform transform = FutureLevelText.transform;
				Vector3 localPosition = transform.localPosition;
				localPosition = new Vector3(localPosition.x, Mathf.Lerp(0f, -0.3f, t), localPosition.z);
				transform.localPosition = localPosition;
				((Graphic)FutureLevelText).color = ((Graphic)FutureLevelText).color.SetAlpha(1f - t);
			}));
			yield return Effects.Wait(0.2f);
			uint num = DataManager.Player.Stats.Level + 1;
			if (num >= xpGainedResult.MaxLevel)
			{
				((TMP_Text)FutureLevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Max);
			}
			else
			{
				((TMP_Text)FutureLevelText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LevelShorthand, ProgressionManager.FormatVisualLevel(num));
			}
			((MonoBehaviour)this).StartCoroutine(Effects.Lerp(0.75f, (float t) =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				Transform transform = FutureLevelText.transform;
				Vector3 localPosition = transform.localPosition;
				localPosition = new Vector3(localPosition.x, Mathf.Lerp(0.3f, 0f, t), localPosition.z);
				transform.localPosition = localPosition;
				((Graphic)FutureLevelText).color = ((Graphic)FutureLevelText).color.SetAlpha(t);
			}));
			yield return AnimateXpBarFill(0u, newXpAmount);
		}
	}

	private IEnumerator AnimateXpBarFill(uint oldXpAmount, ulong newXpAmount)
	{
		float startTime = Time.time;
		if (Mathf.Approximately(XpBar.GlowAlpha, 0f))
		{
			while (Time.time - startTime < 0.75f)
			{
				XpBar.GlowAlpha = Mathf.Lerp(0f, 1f, (Time.time - startTime) / 0.75f);
				yield return null;
			}
		}
		XpBar.ResetScale();
		XpBar.GlowAlpha = 0f;
		startTime = Time.time;
		SoundManager.Instance.PlaySound(XpSound, loop: false);
		while (Time.time - startTime < 0.5f)
		{
			float t = (Time.time - startTime) / 0.5f;
			XpBar.Value = Mathf.Lerp((float)oldXpAmount, (float)newXpAmount, Effects.ExpOut(t));
			yield return null;
		}
		if (Mathf.Approximately(XpBar.GlowAlpha, 1f))
		{
			while (Time.time - startTime < 0.75f)
			{
				XpBar.GlowAlpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / 0.75f);
				yield return null;
			}
		}
	}

	private IEnumerator AnimatePodsAndBeans()
	{
		if (((Component)BeansEarned).gameObject.activeSelf)
		{
			yield return BeansEarned.ShowAmount();
		}
		if (((Component)PodsEarned).gameObject.activeSelf)
		{
			yield return PodsEarned.ShowAmount();
		}
		if (((Component)BeansEarned).gameObject.activeSelf)
		{
			ProgressionManager.CurrencyGrantResult beansGrantResult = TempData.BeansGrantResult;
			if (beansGrantResult.GrantedPodsWithMultiplierApplied != beansGrantResult.GrantedPodsPreMultiplier)
			{
				yield return BeansEarned.ShowMultiplier();
			}
			((MonoBehaviour)this).StartCoroutine(BeansEarned.EnterWallet());
		}
		if (((Component)PodsEarned).gameObject.activeSelf)
		{
			ProgressionManager.CurrencyGrantResult podsGrantResult = TempData.PodsGrantResult;
			if (podsGrantResult.GrantedPodsWithMultiplierApplied != podsGrantResult.GrantedPodsPreMultiplier)
			{
				yield return PodsEarned.ShowMultiplier();
			}
			((MonoBehaviour)this).StartCoroutine(PodsEarned.EnterWallet());
		}
	}
}
