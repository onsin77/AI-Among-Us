using System;
using System.Collections;
using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-20000)]
public class PSManager : DestroyableSingleton<PSManager>
{
	public Texture2D ps4SessionImage;

	[NonSerialized]
	public byte[] ps4SessionImageAsPNG;

	public JoinSessionSource joinSessionSource;

	public bool gameplayStarted;

	private bool keepBlockerActive;

	public string GetSessionName()
	{
		string text = ((GameOptionsManager.Instance.CurrentGameOptions.MapId == 0 && Constants.ShouldFlipSkeld()) ? "Dleks" : Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.MapId]);
		string text2 = "";
		if (Object.op_Implicit((Object)(object)GameData.Instance))
		{
			text2 = ((AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame) ? $"{GameData.Instance.PlayerCount}/{GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers}" : $"{GameData.Instance.PlayerCount}/{15}");
		}
		else
		{
			text2 = ((AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame) ? $"0/{GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers}" : $"0/{15}");
		}
		string text3 = ((!Object.op_Implicit((Object)(object)LobbyBehaviour.Instance)) ? "In-Game" : "Lobby");
		return text3 + " - " + text2 + " - " + text;
	}

	public void JoinGameFromCode(string gameCode)
	{
		int gameCode2 = GameCode.GameNameToInt(gameCode);
		((MonoBehaviour)this).StartCoroutine(JoinGame(gameCode2));
	}

	public void JoinGameFromCode(int gameCode)
	{
		((MonoBehaviour)this).StartCoroutine(JoinGame(gameCode));
	}

	public IEnumerator JoinGame(int gameCode)
	{
		while (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			yield return null;
		}
		while (!DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
		{
			yield return null;
		}
		if (AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			if (AmongUsClient.Instance.GameId == gameCode)
			{
				Debug.LogError((object)"HandleEvents::OnHandleInvite: Already in the game we're trying to join");
				yield break;
			}
			Debug.LogError((object)"HandleEvents::OnHandleInvite: Already Connected");
			AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		}
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(true);
		yield return AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameCode);
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(false);
		if (AmongUsClient.Instance.ClientId < 0)
		{
			Debug.LogError((object)"HandleEvents::JoinGame: ClientId < 0");
			SceneManager.LoadScene("MMOnline");
		}
		Debug.LogError((object)$"HandleEvents::JoinGame: ClientID = {AmongUsClient.Instance.ClientId}");
	}

	public void CreateGame(GameModes gameMode)
	{
		((MonoBehaviour)this).StartCoroutine(CreateGameCo(gameMode));
	}

	private IEnumerator CreateGameCo(GameModes gameMode)
	{
		while (!Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			yield return null;
		}
		while (!DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
		{
			yield return null;
		}
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(true);
		keepBlockerActive = true;
		((MonoBehaviour)this).StartCoroutine(KeepLoadBlockerActive());
		if (AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		}
		yield return (object)new WaitForSeconds(1f);
		DataManager.Settings.Multiplayer.LastPlayedGameMode = gameMode;
		yield return AmongUsClient.Instance.CoCreateOnlineGame();
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(false);
		keepBlockerActive = false;
		if (AmongUsClient.Instance.ClientId < 0)
		{
			Debug.LogError((object)"HandleEvents::CreateGame: ClientId < 0");
			SceneManager.LoadScene("MMOnline");
		}
		Debug.LogError((object)$"HandleEvents::CreateGame: ClientID = {AmongUsClient.Instance.ClientId}");
	}

	private IEnumerator KeepLoadBlockerActive()
	{
		while (keepBlockerActive)
		{
			if (!DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.activeSelf)
			{
				DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(true);
			}
			yield return null;
		}
		DestroyableSingleton<AccountManager>.Instance.postLoadWaiting.SetActive(false);
	}
}
