using System;
using System.Collections;
using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using PowerTools;
using UnityEngine;

public class HostLocalGameButton : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public SpriteRenderer FillScreen;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public NetworkModes NetworkMode;

	public void Start()
	{
		if (DestroyableSingleton<MatchMaker>.InstanceExists)
		{
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		}
	}

	public void ClickHideNSeek()
	{
		GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
		((MonoBehaviour)this).StartCoroutine(CoStartGame());
	}

	public void OnClick()
	{
		GameOptionsManager.Instance.SwitchGameMode(GameModes.Normal);
		if (NetworkMode == NetworkModes.FreePlay)
		{
			if (!NameTextBehaviour.IsValidName(DataManager.Player.Customization.Name))
			{
				DataManager.Player.Customization.Name = "";
				DataManager.Player.Save();
			}
		}
		else
		{
			if (StatsManager.Instance.AmBanned)
			{
				AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
				DestroyableSingleton<DisconnectPopup>.Instance.Show();
				return;
			}
			if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
			{
				return;
			}
		}
		Logger.GlobalInstance.Info("Hosting a local game");
		((MonoBehaviour)this).StartCoroutine(CoStartGame());
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

	private IEnumerator CoStartGame()
	{
		try
		{
			SoundManager.Instance.StopAllSound();
			AmongUsClient.Instance.NetworkMode = NetworkMode;
			switch (NetworkMode)
			{
			case NetworkModes.LocalGame:
				DestroyableSingleton<InnerNetServer>.Instance.StartAsServer();
				AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, dtls: false);
				AmongUsClient.Instance.MainMenuScene = "MatchMaking";
				AmongUsClient.Instance.OnlineScene = "OnlineGame";
				break;
			case NetworkModes.FreePlay:
				DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
				AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, dtls: false);
				AmongUsClient.Instance.MainMenuScene = "MainMenu";
				AmongUsClient.Instance.OnlineScene = "Tutorial";
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)"HostGameButton::CoStartGame: Exception:");
			Debug.LogException(ex, (Object)(object)this);
			DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(ex.Message);
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			yield break;
		}
		yield return (object)new WaitForSeconds(0.1f);
		if (Object.op_Implicit((Object)(object)FillScreen))
		{
			SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f);
			((Component)FillScreen).gameObject.SetActive(true);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				FillScreen.color = Color.Lerp(Color.clear, Color.black, time / 0.25f);
				yield return null;
			}
			FillScreen.color = Color.black;
		}
		AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		if (AmongUsClient.Instance.mode == MatchMakerModes.None && Object.op_Implicit((Object)(object)FillScreen))
		{
			SoundManager.Instance.CrossFadeSound("MainBG", IntroMusic, 0.5f);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
				yield return null;
			}
			FillScreen.color = Color.clear;
		}
	}
}
