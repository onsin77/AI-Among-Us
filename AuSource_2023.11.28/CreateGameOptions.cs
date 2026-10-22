using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using PowerTools;
using UnityEngine;

public class CreateGameOptions : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public GameObject Content;

	public SpriteRenderer Foreground;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public PassiveButton RegionButton;

	private readonly Logger logger = new Logger("CreateGameOptions", Logger.Level.Info, Logger.Category.Gameplay);

	public void Show()
	{
		if (!DestroyableSingleton<AccountManager>.Instance.CanPlayOnline())
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.NotAuthorized;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
		}
		else if (StatsManager.Instance.AmBanned)
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
		}
		else
		{
			((Component)this).gameObject.SetActive(true);
			Content.SetActive(false);
			((MonoBehaviour)this).StartCoroutine(CoShow());
		}
	}

	private IEnumerator CoShow()
	{
		((Component)Foreground).gameObject.SetActive(true);
		yield return Effects.ColorFade(Foreground, Color.clear, Color.black, 0.1f);
		Content.SetActive(true);
		((Component)RegionButton).gameObject.SetActive(false);
		yield return null;
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
		yield return Effects.ColorFade(Foreground, Color.black, Color.clear, 0.1f);
		((Component)Foreground).gameObject.SetActive(false);
	}

	public void StartIcon()
	{
		if (Object.op_Implicit((Object)(object)connectIcon))
		{
			connectIcon.Play(connectClip);
		}
	}

	public void StopIcon()
	{
		if (Object.op_Implicit((Object)(object)connectIcon))
		{
			connectIcon.Stop();
			((Component)connectIcon).GetComponent<SpriteRenderer>().sprite = null;
		}
	}

	public void Hide()
	{
		((MonoBehaviour)this).StartCoroutine(CoHide());
	}

	private IEnumerator CoHide()
	{
		((Component)Foreground).gameObject.SetActive(true);
		yield return Effects.ColorFade(Foreground, Color.clear, Color.black, 0.1f);
		Content.SetActive(false);
		yield return Effects.ColorFade(Foreground, Color.black, Color.clear, 0.1f);
		((Component)Foreground).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		((Component)RegionButton).gameObject.SetActive(true);
	}

	public void Confirm()
	{
		if (DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			logger.Info("Hosting an online game");
			((MonoBehaviour)this).StartCoroutine(CoStartGame());
		}
	}

	private IEnumerator CoStartGame()
	{
		DataManager.Settings.Save();
		SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f);
		((Component)Foreground).gameObject.SetActive(true);
		yield return Effects.ColorFade(Foreground, Color.clear, Color.black, 0.2f);
		yield return AmongUsClient.Instance.CoCreateOnlineGame();
	}

	private IEnumerator ContinueCoStart()
	{
		StopIcon();
		DataManager.Settings.Save();
		SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f);
		((Component)Foreground).gameObject.SetActive(true);
		yield return Effects.ColorFade(Foreground, Color.clear, Color.black, 0.2f);
		yield return AmongUsClient.Instance.CoCreateOnlineGame();
	}
}
