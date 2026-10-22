using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.Data;
using Assets.CoreScripts;
using InnerNet;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class AmongUsClient : InnerNetClient
{
	public static AmongUsClient Instance;

	public string OnlineScene;

	public string MainMenuScene;

	public GameData GameDataPrefab;

	public PlayerControl PlayerPrefab;

	public List<AssetReference> ShipPrefabs;

	public int TutorialMapId;

	public float SpawnRadius = 1.75f;

	public DiscoveryState discoverState;

	public List<IDisconnectHandler> DisconnectHandlers = new List<IDisconnectHandler>();

	public List<IGameListHandler> GameListHandlers = new List<IGameListHandler>();

	public CrossplayPrivilegeErrorType CrossplayPrivilegeError;

	private int MAX_CLIENT_WAIT_TIME = 10;

	private readonly Logger logger = new Logger("AmongUsClient", Logger.Level.Info, Logger.Category.Gameplay);

	private AsyncOperationHandle<GameObject> ShipLoadingAsyncHandle;

	public IEnumerator CoCreateOnlineGame()
	{
		NetworkMode = NetworkModes.OnlineGame;
		if (DestroyableSingleton<ServerManager>.Instance.IsHttp)
		{
			HttpMatchmakerManager.HostServer hostServer = null;
			string matchmakerToken = null;
			yield return DestroyableSingleton<HttpMatchmakerManager>.Instance.CoFindHostServer((HttpMatchmakerManager.HostServer host, string mmToken) =>
			{
				hostServer = host;
				matchmakerToken = mmToken;
			});
			if (hostServer != null)
			{
				yield return CoConnectToGameServer(MatchMakerModes.HostAndClient, hostServer.Ip, hostServer.Port, matchmakerToken);
				yield break;
			}
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			SceneManager.LoadScene("MMOnline");
		}
		else
		{
			yield return CoConnectToGameServer(MatchMakerModes.HostAndClient, DestroyableSingleton<ServerManager>.Instance.UdpNetAddress, DestroyableSingleton<ServerManager>.Instance.UdpNetPort, null);
		}
	}

	public IEnumerator CoJoinOnlinePublicGame(int gameId, string ipAddress, ushort port)
	{
		string matchmakerToken = string.Empty;
		if (DestroyableSingleton<ServerManager>.Instance.IsHttp)
		{
			yield return DestroyableSingleton<HttpMatchmakerManager>.Instance.CoGetOrRefreshToken((string token) =>
			{
				matchmakerToken = token;
			});
			if (string.IsNullOrEmpty(matchmakerToken))
			{
				yield break;
			}
		}
		yield return CoJoinOnlineGameDirect(gameId, ipAddress, port, matchmakerToken);
	}

	public IEnumerator CoJoinOnlineGameFromCode(int gameId)
	{
		NetworkMode = NetworkModes.OnlineGame;
		GameId = gameId;
		while (!DestroyableSingleton<EOSManager>.InstanceExists)
		{
			yield return null;
		}
		yield return DestroyableSingleton<EOSManager>.Instance.WaitForLoginFlow();
		if (DestroyableSingleton<ServerManager>.Instance.IsHttp)
		{
			HttpMatchmakerManager.HostServer hostServer = null;
			string matchmakerToken = null;
			yield return DestroyableSingleton<HttpMatchmakerManager>.Instance.CoFindGameCode(gameId, (HttpMatchmakerManager.HostServer host, string mmToken) =>
			{
				hostServer = host;
				matchmakerToken = mmToken;
			});
			if (hostServer != null)
			{
				yield return CoJoinOnlineGameDirect(GameId, hostServer.Ip, hostServer.Port, matchmakerToken);
				yield break;
			}
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			SceneManager.LoadScene("MMOnline");
		}
		else
		{
			yield return CoConnectToGameServer(MatchMakerModes.Client, DestroyableSingleton<ServerManager>.Instance.UdpNetAddress, DestroyableSingleton<ServerManager>.Instance.UdpNetPort, null);
		}
	}

	public IEnumerator CoFindGame()
	{
		NetworkMode = NetworkModes.OnlineGame;
		if (DestroyableSingleton<ServerManager>.Instance.IsHttp)
		{
			string matchmakerToken = null;
			yield return DestroyableSingleton<HttpMatchmakerManager>.Instance.CoGetOrRefreshToken((string token) =>
			{
				matchmakerToken = token;
			});
			if (string.IsNullOrEmpty(matchmakerToken))
			{
				DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			}
			else
			{
				SceneManager.LoadScene("FindAGame");
			}
			yield break;
		}
		SetEndpoint(DestroyableSingleton<ServerManager>.Instance.UdpNetAddress, DestroyableSingleton<ServerManager>.Instance.UdpNetPort, DestroyableSingleton<ServerManager>.Instance.UdpUseDtls);
		OnlineScene = "OnlineGame";
		mode = MatchMakerModes.Client;
		yield return CoConnect(null);
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		if (LastDisconnectReason == DisconnectReasons.ExitGame)
		{
			HostId = ClientId;
			SceneManager.LoadScene("FindAGame");
		}
	}

	private IEnumerator CoConnectToGameServer(MatchMakerModes mode, string ipAddress, ushort port, string matchmakerToken)
	{
		SetEndpoint(ipAddress, port, DestroyableSingleton<ServerManager>.Instance.UdpUseDtls);
		MainMenuScene = "MMOnline";
		OnlineScene = "OnlineGame";
		Connect(mode, matchmakerToken);
		yield return WaitForConnectionOrFail();
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		if (ClientId < 0)
		{
			SceneManager.LoadScene("MMOnline");
		}
	}

	private IEnumerator CoJoinOnlineGameDirect(int gameId, string ipAddress, ushort port, string matchmakerToken)
	{
		NetworkMode = NetworkModes.OnlineGame;
		GameId = gameId;
		yield return CoConnectToGameServer(MatchMakerModes.Client, ipAddress, port, matchmakerToken);
	}

	public void Awake()
	{
		if (Object.op_Implicit((Object)(object)Instance))
		{
			if ((Object)(object)Instance != (Object)(object)this)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
		else
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			Application.targetFrameRate = 60;
		}
	}

	public void StartGame()
	{
		SendStartGame();
		discoverState = DiscoveryState.Off;
	}

	public override void Update()
	{
		base.Update();
	}

	public void ExitGame(DisconnectReasons reason)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		((MonoBehaviour)this).StopAllCoroutines();
		AbortLoadingAssets();
		DestroyableSingleton<FriendsListManager>.Instance.ReparentUI();
		SoundManager.Instance.StopAllSound();
		discoverState = DiscoveryState.Off;
		DisconnectHandlers.Clear();
		DisconnectInternal(reason);
		DestroyableSingleton<AchievementManager>.Instance.OnMatchExited();
		GameDebugCommands.RemoveCommands();
		Logger.GlobalInstance.Debug("Loading new scene " + MainMenuScene);
		SceneManager.LoadScene(MainMenuScene);
		Logger.GlobalInstance.Debug("Finished loading scene");
	}

	protected override void OnGetGameList(TotalGameData totalGames, List<GameListing> availableGames)
	{
		for (int i = 0; i < GameListHandlers.Count; i++)
		{
			try
			{
				GameListHandlers[i].HandleList(totalGames, availableGames);
			}
			catch (Exception ex)
			{
				logger.Error("AmongUsClient::OnGetGameList: Exception:");
				Debug.LogException(ex, (Object)(object)this);
			}
		}
	}

	protected override void OnReportedPlayer(ReportOutcome outcome, int clientId, string playerName, ReportReasons reason)
	{
	}

	protected override void OnGameCreated(string gameIdString)
	{
	}

	protected override void OnWaitForHost(string gameIdString)
	{
		if (GameState != GameStates.Joined)
		{
			logger.Info("Waiting for host: " + gameIdString);
			if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
			{
				DestroyableSingleton<WaitForHostPopup>.Instance.Show();
			}
		}
	}

	protected void AbortLoadingAssets()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (ShipLoadingAsyncHandle.IsValid())
		{
			Addressables.Release<GameObject>(ShipLoadingAsyncHandle);
			ShipLoadingAsyncHandle = default;
		}
	}

	protected IEnumerator CoStartGameHost()
	{
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			LobbyBehaviour.Instance.Despawn();
		}
		if (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			int num = Mathf.Clamp((int)GameOptionsManager.Instance.CurrentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
			try
			{
				if (num == 0 && Constants.ShouldFlipSkeld())
				{
					num = 3;
				}
				else if (num == 3 && !Constants.ShouldFlipSkeld())
				{
					num = 0;
				}
			}
			catch (Exception ex)
			{
				logger.Error("AmongUsClient::CoStartGame: Exception:");
				Debug.LogException(ex, (Object)(object)this);
			}
			ShipLoadingAsyncHandle = ShipPrefabs[num].InstantiateAsync((Transform)null, false);
			yield return ShipLoadingAsyncHandle;
			GameObject result = ShipLoadingAsyncHandle.Result;
			ShipLoadingAsyncHandle = default;
			ShipStatus.Instance = result.GetComponent<ShipStatus>();
			Spawn(ShipStatus.Instance);
		}
		float timer = 0f;
		while (true)
		{
			bool stopWaiting = true;
			int num2 = MAX_CLIENT_WAIT_TIME;
			if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 5 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 4)
			{
				num2 = MAX_CLIENT_WAIT_TIME + 5;
			}
			lock (allClients)
			{
				for (int i = 0; i < allClients.Count; i++)
				{
					ClientData clientData = allClients[i];
					if (clientData.Id != ClientId && !clientData.IsReady)
					{
						if (timer < (float)num2)
						{
							stopWaiting = false;
							continue;
						}
						SendLateRejection(clientData.Id, DisconnectReasons.ClientTimeout);
						clientData.IsReady = true;
						OnPlayerLeft(clientData, DisconnectReasons.ClientTimeout);
					}
				}
			}
			yield return null;
			if (stopWaiting)
			{
				break;
			}
			timer += Time.deltaTime;
		}
		DestroyableSingleton<RoleManager>.Instance.SelectRoles();
		ShipStatus.Instance.Begin();
		SendClientReady();
	}

	protected IEnumerator CoStartGameClient()
	{
		while (PlayerControl.LocalPlayer.Data == null && !AmHost)
		{
			yield return null;
		}
		float timer = 0f;
		float timeOut = MAX_CLIENT_WAIT_TIME + 5;
		if (GameOptionsManager.Instance.CurrentGameOptions.MapId == 5 || GameOptionsManager.Instance.CurrentGameOptions.MapId == 4)
		{
			timeOut = MAX_CLIENT_WAIT_TIME + 10;
		}
		while (!Object.op_Implicit((Object)(object)ShipStatus.Instance) || Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			if (AmHost)
			{
				yield break;
			}
			if (timer > timeOut)
			{
				((MonoBehaviour)this).StartCoroutine(CoWaitForDisconnect());
				yield break;
			}
			timer += Time.deltaTime;
			yield return null;
		}
		SendClientReady();
		timer = 0f;
		while (true)
		{
			bool stopWaiting = true;
			lock (allClients)
			{
				for (int i = 0; i < allClients.Count; i++)
				{
					if (!allClients[i].IsReady)
					{
						stopWaiting = false;
					}
				}
			}
			yield return null;
			if (stopWaiting)
			{
				break;
			}
			if (timer > timeOut)
			{
				((MonoBehaviour)this).StartCoroutine(CoWaitForDisconnect());
				yield break;
			}
			timer += Time.deltaTime;
		}
		while (((Object)(object)PlayerControl.LocalPlayer.Data.Role == (Object)null || PlayerControl.LocalPlayer.myTasks.Count == 0) && !AmHost)
		{
			yield return null;
		}
	}

	private IEnumerator CoWaitForDisconnect()
	{
		Logger.GlobalInstance.Debug("Reached timeout while loading game client");
		yield return (object)new WaitForEndOfFrame();
		MainMenuScene = "MMOnline";
		ExitGame(DisconnectReasons.Error);
	}

	protected override IEnumerator CoStartGame()
	{
		if (DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
		{
			DestroyableSingleton<HudManager>.Instance.GameMenu.Close();
		}
		DestroyableSingleton<UnityTelemetry>.Instance.Init();
		logger.Info("Received game start: " + AmHost);
		yield return null;
		while (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			yield return null;
		}
		while (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			yield return null;
		}
		PlayerControl.LocalPlayer.moveable = false;
		((Behaviour)PlayerControl.LocalPlayer.MyPhysics.inputHandler).enabled = true;
		PlayerCustomizationMenu playerCustomizationMenu = Object.FindObjectOfType<PlayerCustomizationMenu>();
		if (Object.op_Implicit((Object)(object)playerCustomizationMenu))
		{
			playerCustomizationMenu.Close(canMove: false);
		}
		GameSettingMenu gameSettingMenu = Object.FindObjectOfType<GameSettingMenu>();
		if (Object.op_Implicit((Object)(object)gameSettingMenu))
		{
			gameSettingMenu.Close();
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DisconnectHandlers.Remove(DestroyableSingleton<GameStartManager>.Instance);
			Object.Destroy((Object)(object)((Component)DestroyableSingleton<GameStartManager>.Instance).gameObject);
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			DestroyableSingleton<DiscordManager>.Instance.SetPlayingGame();
		}
		if (!string.IsNullOrEmpty(DataManager.Player.Store.ActiveCosmicube))
		{
			Instance.SetActivePodType(DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(DataManager.Player.Store.ActiveCosmicube).podId);
		}
		else
		{
			PlayerStorageManager.CloudPlayerPrefs playerPrefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
			Instance.SetActivePodType(playerPrefs.ActivePodType);
		}
		DestroyableSingleton<FriendsListManager>.Instance.ConfirmationScreen.Cancel();
		DestroyableSingleton<FriendsListManager>.Instance.Ui.Close(force: true);
		DestroyableSingleton<FriendsListManager>.Instance.ReparentUI();
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f, showLoader: true);
		while (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			yield return null;
		}
		StatsManager.Instance.BanPoints++;
		StatsManager.Instance.LastGameStarted = DateTime.UtcNow;
		if (AmHost)
		{
			GameData.Instance.SetDirty();
			yield return CoStartGameHost();
		}
		else
		{
			yield return CoStartGameClient();
			if (AmHost)
			{
				yield return CoStartGameHost();
			}
		}
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			PlayerControl playerControl = GameData.Instance.AllPlayers[i].Object;
			if (Object.op_Implicit((Object)(object)playerControl))
			{
				playerControl.moveable = true;
				((Behaviour)playerControl.NetTransform).enabled = true;
				((Behaviour)playerControl.MyPhysics).enabled = true;
				playerControl.MyPhysics.Awake();
				playerControl.MyPhysics.ResetMoveState();
				((Behaviour)playerControl.Collider).enabled = true;
				ShipStatus.Instance.SpawnPlayer(playerControl, GameData.Instance.PlayerCount, initialSpawn: true);
			}
		}
		DestroyableSingleton<FriendsListManager>.Instance.SetRecentlyPlayed(GameData.Instance.AllPlayers);
		TempData.TimeGameStarted = Time.realtimeSinceStartup;
		int map = Mathf.Clamp((int)GameOptionsManager.Instance.CurrentGameOptions.MapId, 0, Constants.MapNames.Length - 1);
		string roomCode = GameCode.IntToGameName(Instance.GameId);
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.StartGame(PlayerControl.LocalPlayer.Data, GameData.Instance.PlayerCount, GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, Instance.NetworkMode, (MapNames)map, GameOptionsManager.Instance.CurrentGameOptions.GameMode, roomCode, DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name, GameOptionsManager.Instance.CurrentGameOptions, GameData.Instance.AllPlayers);
		try
		{
			DestroyableSingleton<UnityTelemetry>.Instance.StartGame(Instance.AmHost, GameData.Instance.PlayerCount, GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, Instance.NetworkMode, StatsManager.Instance.GetStat(StringNames.StatsGamesImpostor), StatsManager.Instance.GetStat(StringNames.StatsGamesStarted), StatsManager.Instance.GetStat(StringNames.StatsCrewmateStreak));
			GameData.PlayerOutfit defaultOutfit = PlayerControl.LocalPlayer.Data.DefaultOutfit;
			DestroyableSingleton<UnityTelemetry>.Instance.StartGameCosmetics(defaultOutfit.ColorId, defaultOutfit.HatId, defaultOutfit.SkinId, defaultOutfit.PetId, defaultOutfit.VisorId, defaultOutfit.NamePlateId);
		}
		catch
		{
		}
		GameDebugCommands.AddCommands();
	}

	protected override void OnBecomeHost()
	{
		ClientData clientData = FindClientById(ClientId);
		if (IsGameStarted)
		{
			if (!Object.op_Implicit((Object)(object)clientData.Character))
			{
				CreatePlayer(clientData);
			}
		}
		else if (!Object.op_Implicit((Object)(object)clientData.Character))
		{
			OnGameJoined(null);
		}
		logger.Info("Became Host");
		RemoveUnownedObjects();
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			logger.Warning("Trying to recover from no game data");
			GameData.Instance = Object.Instantiate<GameData>(GameDataPrefab);
			Spawn(GameData.Instance);
		}
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance) && Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			LobbyBehaviour.Instance.Despawn();
		}
	}

	protected override void OnGameEnd(EndGameResult endGameResult)
	{
		StatsManager.Instance.BanPoints -= 1.5f;
		StatsManager.Instance.LastGameStarted = DateTime.MinValue;
		GameOverReason gameOverReason = endGameResult.GameOverReason;
		bool showAd = endGameResult.ShowAd;
		DisconnectHandlers.Clear();
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			try
			{
				Minigame.Instance.Close();
				Minigame.Instance.Close();
			}
			catch (Exception ex)
			{
				logger.Error("AmongUsClient::OnGameEnd Exception: 1");
				Debug.LogException(ex, (Object)(object)this);
			}
		}
		float durationInSeconds = Time.realtimeSinceStartup - TempData.TimeGameStarted;
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.EndGame(durationInSeconds, gameOverReason, GameData.Instance.AllPlayers);
		try
		{
			DestroyableSingleton<UnityTelemetry>.Instance.EndGame(gameOverReason);
		}
		catch (Exception ex2)
		{
			logger.Error("AmongUsClient::OnGameEnd Exception: 2");
			Debug.LogException(ex2, (Object)(object)this);
		}
		TempData.EndReason = gameOverReason;
		TempData.showAd = showAd;
		GameManager.Instance.DidHumansWin(gameOverReason);
		TempData.OnGameEnd();
		ProgressionManager.XpGrantResult xpGrantedResult = endGameResult.XpGrantResult ?? ProgressionManager.XpGrantResult.Default();
		ProgressionManager.CurrencyGrantResult currencyGrantResult = endGameResult.BeansGrantResult ?? ProgressionManager.CurrencyGrantResult.Default();
		ProgressionManager.CurrencyGrantResult currencyGrantResult2 = endGameResult.PodsGrantResult ?? ProgressionManager.CurrencyGrantResult.Default();
		TempData.XpGrantedResult = xpGrantedResult;
		TempData.BeansGrantResult = currencyGrantResult;
		TempData.PodsGrantResult = currencyGrantResult2;
		if (endGameResult.XpGrantResult != null)
		{
			DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp;
			if (endGameResult.XpGrantResult.LevelledUp)
			{
				DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp - endGameResult.XpGrantResult.XpRequiredToLevelUp;
				DataManager.Player.Stats.Level = endGameResult.XpGrantResult.NewLevel;
				DataManager.Player.Stats.XpForNextLevel = endGameResult.XpGrantResult.XpRequiredToLevelUpNextLevel;
			}
			else
			{
				DataManager.Player.Stats.Xp = endGameResult.XpGrantResult.OldXpAmount + endGameResult.XpGrantResult.GrantedXp;
				DataManager.Player.Stats.Level = endGameResult.XpGrantResult.OldLevel;
				DataManager.Player.Stats.XpForNextLevel = endGameResult.XpGrantResult.XpRequiredToLevelUp;
			}
			DataManager.Player.Save();
		}
		DestroyableSingleton<InventoryManager>.Instance.ChangePodCount(currencyGrantResult2.PodId, (int)currencyGrantResult2.GrantedPodsWithMultiplierApplied);
		DestroyableSingleton<InventoryManager>.Instance.UnusedBeans += (int)currencyGrantResult.GrantedPodsWithMultiplierApplied;
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (playerInfo != null && playerInfo.Role.DidWin(gameOverReason))
			{
				TempData.winners.Add(new WinningPlayerData(playerInfo));
			}
		}
		GameDebugCommands.RemoveCommands();
		((MonoBehaviour)this).StartCoroutine(CoEndGame());
	}

	public IEnumerator CoEndGame()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.5f);
		SceneManager.LoadScene("EndGame");
	}

	protected override void OnPlayerJoined(ClientData data)
	{
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
		}
		if (AmHost && data.InScene)
		{
			CreatePlayer(data);
		}
	}

	protected override void OnGameJoined(string gameIdString)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!string.IsNullOrWhiteSpace(OnlineScene))
		{
			SceneManager.LoadScene(OnlineScene);
		}
		if (NetworkMode != NetworkModes.FreePlay)
		{
			if (!string.IsNullOrEmpty(DataManager.Player.Store.ActiveCosmicube))
			{
				Instance.SetActivePodType(DestroyableSingleton<CosmicubeManager>.Instance.GetCubeDataByID(DataManager.Player.Store.ActiveCosmicube).podId);
				return;
			}
			PlayerStorageManager.CloudPlayerPrefs playerPrefs = DestroyableSingleton<PlayerStorageManager>.Instance.PlayerPrefs;
			Instance.SetActivePodType(playerPrefs.ActivePodType);
		}
	}

	protected override void OnPlayerLeft(ClientData data, DisconnectReasons reason)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected Obj, but got Unknown
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
		}
		PlayerControl character = data.Character;
		if (Object.op_Implicit((Object)(object)character))
		{
			for (int num = DisconnectHandlers.Count - 1; num > -1; num--)
			{
				IDisconnectHandler disconnectHandler = DisconnectHandlers[num];
				if (disconnectHandler is MonoBehaviour && !Object.op_Implicit((Object)(MonoBehaviour)disconnectHandler))
				{
					DisconnectHandlers.RemoveAt(num);
				}
				else
				{
					try
					{
						disconnectHandler.HandleDisconnect(character, reason);
					}
					catch (Exception ex)
					{
						logger.Error("AmongUsClient::OnPlayerLeft: Exception: 1");
						Debug.LogException(ex, (Object)(object)this);
						DisconnectHandlers.RemoveAt(num);
					}
				}
			}
			Object.Destroy((Object)(object)((Component)character).gameObject);
		}
		else
		{
			logger.Warning($"A player without a character disconnected: {data.Id}: {data.InScene}");
			for (int num2 = DisconnectHandlers.Count - 1; num2 > -1; num2--)
			{
				IDisconnectHandler disconnectHandler2 = DisconnectHandlers[num2];
				if (disconnectHandler2 is MonoBehaviour && !Object.op_Implicit((Object)(MonoBehaviour)disconnectHandler2))
				{
					DisconnectHandlers.RemoveAt(num2);
				}
				else
				{
					try
					{
						disconnectHandler2.HandleDisconnect();
					}
					catch (Exception ex2)
					{
						logger.Error("AmongUsClient::OnPlayerLeft: Exception: 2");
						Debug.LogException(ex2, (Object)(object)this);
						DisconnectHandlers.RemoveAt(num2);
					}
				}
			}
		}
		if (AmHost)
		{
			if (!Object.op_Implicit((Object)(object)GameData.Instance))
			{
				ExitGame(DisconnectReasons.Error);
				return;
			}
			if (GameManager.Instance.LogicOptions.IsDefaults)
			{
				GameManager.Instance.LogicOptions.SetRecommendations(GameData.Instance.PlayerCount, Instance.NetworkMode);
			}
		}
		RemoveUnownedObjects();
	}

	protected override void OnDisconnected()
	{
		DestroyableSingleton<FriendsListManager>.Instance.ReparentUI();
		SceneManager.LoadScene(MainMenuScene);
	}

	protected override IEnumerator CoOnPlayerChangedScene(ClientData client, string currentScene)
	{
		client.InScene = true;
		if (!AmHost)
		{
			yield break;
		}
		if (currentScene.Equals("Tutorial"))
		{
			GameData.Instance = Object.Instantiate<GameData>(GameDataPrefab);
			Spawn(GameData.Instance);
			GameManager.DestroyInstance();
			GameManager netObjParent = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
			Spawn(netObjParent);
			int index = ((TutorialMapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : TutorialMapId);
			ShipLoadingAsyncHandle = ShipPrefabs[index].InstantiateAsync((Transform)null, false);
			yield return ShipLoadingAsyncHandle;
			GameObject result = ShipLoadingAsyncHandle.Result;
			ShipLoadingAsyncHandle = default;
			Spawn(result.GetComponent<ShipStatus>());
			CreatePlayer(client);
		}
		else
		{
			if (!currentScene.Equals("OnlineGame"))
			{
				yield break;
			}
			if (client.Id != ClientId)
			{
				SendInitialData(client.Id);
			}
			else
			{
				if (NetworkMode == NetworkModes.LocalGame)
				{
					((MonoBehaviour)this).StartCoroutine(CoBroadcastManager());
				}
				if (!Object.op_Implicit((Object)(object)GameData.Instance))
				{
					GameData.Instance = Object.Instantiate<GameData>(GameDataPrefab);
					Spawn(GameData.Instance);
				}
				GameManager.DestroyInstance();
				GameManager netObjParent2 = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
				Spawn(netObjParent2);
			}
			CreatePlayer(client);
		}
	}

	private void CreatePlayer(ClientData clientData)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)clientData.Character))
		{
			return;
		}
		if (!AmHost)
		{
			logger.Debug("Waiting for host to make my player");
			return;
		}
		if (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			GameData.Instance = Object.Instantiate<GameData>(GameDataPrefab);
			Spawn(GameData.Instance);
		}
		sbyte availableId = GameData.Instance.GetAvailableId();
		if (availableId == -1)
		{
			SendLateRejection(clientData.Id, DisconnectReasons.GameFull);
			logger.Info("Overfilled room.");
			return;
		}
		Vector2 val = Vector2.zero;
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			val = new Vector2(-1.9f, 3.25f);
		}
		PlayerControl playerControl = Object.Instantiate<PlayerControl>(PlayerPrefab, Vector2.op_Implicit(val), Quaternion.identity);
		playerControl.PlayerId = (byte)availableId;
		playerControl.FriendCode = clientData.FriendCode;
		playerControl.Puid = clientData.ProductUserId;
		clientData.Character = playerControl;
		UpdateCachedClients(clientData, clientData.Character);
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			ShipStatus.Instance.SpawnPlayer(playerControl, Palette.PlayerColors.Length, initialSpawn: false);
		}
		Spawn(playerControl, clientData.Id, SpawnFlags.IsClientCharacter);
		GameData.Instance.AddPlayer(playerControl);
		if (GameManager.Instance.LogicOptions.IsDefaults)
		{
			GameManager.Instance.LogicOptions.SetRecommendations(GameData.Instance.PlayerCount, Instance.NetworkMode);
		}
	}

	private IEnumerator CoBroadcastManager()
	{
		while (!Object.op_Implicit((Object)(object)GameData.Instance))
		{
			yield return null;
		}
		int lastPlayerCount = 0;
		discoverState = DiscoveryState.Broadcast;
		while (discoverState == DiscoveryState.Broadcast)
		{
			if (lastPlayerCount != GameData.Instance.PlayerCount)
			{
				lastPlayerCount = GameData.Instance.PlayerCount;
				string data = $"{DataManager.Player.Customization.Name}~Open~{GameData.Instance.PlayerCount}~";
				DestroyableSingleton<InnerDiscover>.Instance.Interval = 1f;
				DestroyableSingleton<InnerDiscover>.Instance.StartAsServer(data);
			}
			yield return null;
		}
		DestroyableSingleton<InnerDiscover>.Instance.StopServer();
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void InitSceneChangeListener()
	{
		Logger.GlobalInstance.Info("[AmongUsClient::InitSceneChangeListener()] Initializing OnActiveSceneChange detection");
		SceneManager.activeSceneChanged += OnActiveSceneChange;
	}

	private static void OnActiveSceneChange(Scene from, Scene to)
	{
		Logger.GlobalInstance.Info("[AmongUsClient::OnActiveSceneChange()] Scene change detected. Unloading unused assets and running garbage collector.");
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}
