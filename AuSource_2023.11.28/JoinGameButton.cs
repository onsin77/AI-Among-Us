using System.Collections;
using AmongUs.Data;
using InnerNet;
using PowerTools;
using TMPro;
using UnityEngine;

public class JoinGameButton : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public TextBoxTMP GameIdText;

	public TextMeshPro gameNameText;

	public float timeRecieved;

	public SpriteRenderer FillScreen;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public NetworkModes NetworkMode;

	public string netAddress;

	private void Start()
	{
		if (!((Object)(object)GameIdText == (Object)null))
		{
			GameIdText.Hidden = DataManager.Settings.Gameplay.StreamerMode;
		}
	}

	public void OnClick()
	{
		if (string.IsNullOrWhiteSpace(netAddress))
		{
			return;
		}
		if (NetworkMode == NetworkModes.OnlineGame && !DestroyableSingleton<AccountManager>.Instance.CanPlayOnline())
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.NotAuthorized;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
		}
		else if (!Object.op_Implicit((Object)(object)NameTextBehaviour.Instance) || !NameTextBehaviour.Instance.ShakeIfInvalid())
		{
			if (StatsManager.Instance.AmBanned)
			{
				AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
				DestroyableSingleton<DisconnectPopup>.Instance.Show();
			}
			else
			{
				ContinueOnClick();
			}
		}
	}

	private void ContinueOnClick()
	{
		DataManager.Settings.Save();
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		AmongUsClient.Instance.NetworkMode = NetworkMode;
		if (NetworkMode == NetworkModes.OnlineGame)
		{
			int num = GameCode.GameNameToInt(GameIdText.text);
			if (num == -1)
			{
				if (string.IsNullOrWhiteSpace(GameIdText.text))
				{
					TextTranslatorTMP component = ((Component)gameNameText).GetComponent<TextTranslatorTMP>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.ResetText();
					}
				}
				((MonoBehaviour)this).StartCoroutine(Effects.SwayX(((Component)GameIdText).transform));
				DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(num));
			}
		}
		else
		{
			AmongUsClient.Instance.SetEndpoint(netAddress, 22023, dtls: false);
			AmongUsClient.Instance.GameId = 32;
			AmongUsClient.Instance.NetworkMode = NetworkModes.LocalGame;
			AmongUsClient.Instance.MainMenuScene = "MatchMaking";
			((MonoBehaviour)this).StartCoroutine(JoinLocalGame());
		}
	}

	private IEnumerator JoinLocalGame()
	{
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
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.Connect(MatchMakerModes.Client, null);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		Logger.GlobalInstance.Info($"Attempted to join local game and got mode {AmongUsClient.Instance.mode}");
		if (AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			yield break;
		}
		if (Object.op_Implicit((Object)(object)FillScreen))
		{
			SoundManager.Instance.CrossFadeSound("MainBG", IntroMusic, 0.5f);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
				yield return null;
			}
			FillScreen.color = Color.clear;
		}
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
	}

	public void SetGameName(string[] gameNameParts)
	{
		int num = 15;
		((TMP_Text)gameNameText).text = $"{gameNameParts[0]} ({gameNameParts[2]}/{num})";
	}

	public void StartIcon()
	{
		connectIcon.Play(connectClip);
	}

	public void StopIcon()
	{
		connectIcon.Stop();
		((Component)connectIcon).GetComponent<SpriteRenderer>().sprite = null;
	}
}
