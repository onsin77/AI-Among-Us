using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
	public TextMeshPro WinText;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public PoolablePlayer PlayerPrefab;

	public Sprite GhostSprite;

	public AudioClip DisconnectStinger;

	public AudioClip CrewStinger;

	public AudioClip ImpostorStinger;

	public EndGameNavigation Navigation;

	private const float ScaleAll = 0.9f;

	private const float OffsetWidth = 1f;

	private float stingerTime;

	public void Start()
	{
		SetEverythingUp();
		((MonoBehaviour)this).StartCoroutine(CoBegin());
		((MonoBehaviour)this).Invoke("ShowButtons", 1.1f);
		ConsoleJoystick.SetMode_Menu();
		DestroyableSingleton<AchievementManager>.Instance.OnMatchEnd(TempData.EndReason, TempData.winners.Any((WinningPlayerData h) => h.IsYou));
	}

	private void ShowButtons()
	{
		((Component)FrontMost).gameObject.SetActive(false);
		if (TempData.XpGrantedResult.GrantedXp != 0 || TempData.PodsGrantResult.GrantedPodsWithMultiplierApplied != 0 || TempData.BeansGrantResult.GrantedPodsWithMultiplierApplied != 0)
		{
			Navigation.ShowNavigationToProgressionScreen();
		}
		else
		{
			Navigation.ShowDefaultNavigation();
		}
	}

	private void SetEverythingUp()
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		StatsManager.Instance.IncrementStat(StringNames.StatsGamesFinished);
		Navigation.HideButtons();
		bool flag = GameManager.Instance.DidHumansWin(TempData.EndReason);
		if (TempData.EndReason == GameOverReason.ImpostorDisconnect)
		{
			StatsManager.Instance.AddDrawReason(TempData.EndReason);
			((TMP_Text)WinText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorDisconnected);
			SoundManager.Instance.PlaySound(DisconnectStinger, loop: false);
		}
		else
		{
			if (TempData.winners.Any((WinningPlayerData h) => h.IsYou))
			{
				StatsManager.Instance.AddWinReason(TempData.EndReason, GameManager.Instance.LogicOptions.MapId, PlayerControl.LocalPlayer.Data.RoleWhenAlive.GetValueOrDefault());
				DestroyableSingleton<AchievementManager>.Instance.SetWinMap(GameManager.Instance.LogicOptions.MapId);
				((TMP_Text)WinText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Victory);
				((Renderer)BackgroundBar).material.SetColor("_Color", Palette.CrewmateBlue);
				WinningPlayerData winningPlayerData = TempData.winners.FirstOrDefault((WinningPlayerData h) => h.IsYou);
				if (winningPlayerData != null)
				{
					DestroyableSingleton<UnityTelemetry>.Instance.WonGame(winningPlayerData.ColorId, winningPlayerData.HatId, winningPlayerData.SkinId, winningPlayerData.PetId, winningPlayerData.VisorId, winningPlayerData.NamePlateId);
				}
			}
			else
			{
				StatsManager.Instance.AddLoseReason(TempData.EndReason);
				((TMP_Text)WinText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Defeat);
				((Graphic)WinText).color = Color.red;
			}
			if (flag)
			{
				SoundManager.Instance.PlayDynamicSound("Stinger", CrewStinger, loop: false, GetStingerVol, SoundManager.Instance.MusicChannel);
			}
			else
			{
				SoundManager.Instance.PlayDynamicSound("Stinger", ImpostorStinger, loop: false, GetStingerVol, SoundManager.Instance.MusicChannel);
			}
		}
		int num = Mathf.CeilToInt(7.5f);
		List<WinningPlayerData> list = TempData.winners.OrderBy((WinningPlayerData b) => b.IsYou ? (-1) : 0).ToList();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			WinningPlayerData winningPlayerData2 = list[num2];
			int num3 = ((num2 % 2 != 0) ? 1 : (-1));
			int num4 = (num2 + 1) / 2;
			float num5 = (float)num4 / (float)num;
			float num6 = Mathf.Lerp(1f, 0.75f, num5);
			float num7 = ((num2 == 0) ? (-8) : (-1));
			PoolablePlayer poolablePlayer = Object.Instantiate<PoolablePlayer>(PlayerPrefab, ((Component)this).transform);
			((Component)poolablePlayer).transform.localPosition = new Vector3(1f * (float)num3 * (float)num4 * num6, FloatRange.SpreadToEdges(-1.125f, 0f, num4, num), num7 + (float)num4 * 0.01f) * 0.9f;
			float num8 = Mathf.Lerp(1f, 0.65f, num5) * 0.9f;
			Vector3 val = new Vector3(num8, num8, 1f);
			((Component)poolablePlayer).transform.localScale = val;
			if (winningPlayerData2.IsDead)
			{
				poolablePlayer.SetBodyAsGhost();
				poolablePlayer.SetDeadFlipX(num2 % 2 == 0);
			}
			else
			{
				poolablePlayer.SetFlipX(num2 % 2 == 0);
			}
			poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2, PlayerMaterial.MaskType.None, winningPlayerData2.IsDead, includePet: true);
			if (flag)
			{
				poolablePlayer.ToggleName(active: false);
				continue;
			}
			Color color = (winningPlayerData2.IsImpostor ? Palette.ImpostorRed : Palette.White);
			poolablePlayer.SetName(winningPlayerData2.PlayerName, val.Inv(), color, -15f);
			Vector3 namePosition = new Vector3(0f, -1.31f, -0.5f);
			poolablePlayer.SetNamePosition(namePosition);
			if (Constants.ShouldHorseAround() && GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek)
			{
				poolablePlayer.SetBodyType(PlayerBodyTypes.Normal);
				poolablePlayer.SetFlipX(flipped: false);
			}
		}
	}

	private void GetStingerVol(AudioSource source, float dt)
	{
		stingerTime += dt * 0.75f;
		source.volume = Mathf.Clamp(1f / stingerTime, 0f, 1f);
	}

	private IEnumerator CoBegin()
	{
		GameManager.Instance.EndGame();
		Color c = ((Graphic)WinText).color;
		Color fade = Color.black;
		_ = Color.white;
		Vector3 titlePos = WinText.transform.localPosition;
		float timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			float num = Mathf.Min(1f, timer / 3f);
			((Renderer)Foreground).material.SetFloat("_Rad", ForegroundRadius.ExpOutLerp(num * 2f));
			fade.a = Mathf.Lerp(1f, 0f, num * 3f);
			FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
			((Graphic)WinText).color = c;
			titlePos.y = 2.7f - num * 0.3f;
			WinText.transform.localPosition = titlePos;
			yield return null;
		}
		((Component)FrontMost).gameObject.SetActive(false);
	}

	public static IEnumerator WaitWithTimeout(Func<bool> success)
	{
		for (float timer = 0f; timer < 5f; timer += Time.deltaTime)
		{
			if (success())
			{
				break;
			}
			yield return null;
		}
	}
}
