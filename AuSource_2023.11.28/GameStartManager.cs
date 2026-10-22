using System.Collections;
using AmongUs.Data;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStartManager : DestroyableSingleton<GameStartManager>, IDisconnectHandler
{
	private enum StartingStates
	{
		NotStarting,
		Countdown,
		Starting
	}

	private const float CountdownDuration = 5.0001f;

	public int MinPlayers = 4;

	public TextMeshPro PlayerCounter;

	private int LastPlayerCount = -1;

	public GameObject GameSizePopup;

	public GameObject GameRoomButton;

	public TextMeshPro GameRoomNameInfo;

	public TextMeshPro GameRoomNameCode;

	public LobbyBehaviour LobbyPrefab;

	public TextMeshPro GameStartText;

	public TextMeshPro startLabelText;

	public SpriteRenderer StartButton;

	public ActionMapGlyphDisplay StartButtonGlyph;

	public GameObject StartButtonGlyphContainer;

	public SpriteRenderer MakePublicButton;

	public ControllerHeldButtonBehaviour MakePublicButtonBehaviour;

	public GameObject MakePublicButtonGlyph;

	public Sprite PublicGameImage;

	public Sprite PrivateGameImage;

	public TextMeshPro privatePublicText;

	public SpriteRenderer ShareOnDiscordButton;

	public GameObject InviteFriendsButton;

	private StartingStates startState;

	private float countDownTimer;

	private IEnumerator copyGameCodeCoroutine;

	private ImageTranslator publicButtonTranslator;

	public void Start()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		DataManager.Settings.Gameplay.OnStreamerModeChanged += UpdateStreamerModeUI;
		UpdateStreamerModeUI();
		if (GameCode.IntToGameName(AmongUsClient.Instance.GameId) != null)
		{
			((TMP_Text)GameRoomNameInfo).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCodeInfo);
		}
		else
		{
			((Component)StartButton).transform.localPosition = new Vector3(0f, -0.2f, 0f);
			PlayerCounter.transform.localPosition = new Vector3(0f, -0.8f, 0f);
			GameRoomButton.gameObject.SetActive(false);
		}
		AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		if (!AmongUsClient.Instance.AmHost)
		{
			((Component)StartButton).gameObject.SetActive(false);
			((Behaviour)MakePublicButtonBehaviour).enabled = false;
			ActionMapGlyphDisplay componentInChildren = ((Component)MakePublicButton).GetComponentInChildren<ActionMapGlyphDisplay>(true);
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				((Component)componentInChildren).gameObject.SetActive(false);
			}
		}
		else
		{
			LobbyBehaviour.Instance = Object.Instantiate<LobbyBehaviour>(LobbyPrefab);
			AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance);
		}
		((Component)MakePublicButton).gameObject.SetActive(AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame);
		((Component)ShareOnDiscordButton).gameObject.SetActive(false);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		DataManager.Settings.Gameplay.OnStreamerModeChanged -= UpdateStreamerModeUI;
	}

	private void UpdateStreamerModeUI()
	{
		string text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		if (text == null)
		{
			text = "";
		}
		((TMP_Text)GameRoomNameCode).text = (DataManager.Settings.Gameplay.StreamerMode ? "******" : text);
	}

	public void MakePublic()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			AmongUsClient.Instance.ChangeGamePublic(!AmongUsClient.Instance.IsGamePublic);
			MakePublicButton.sprite = (AmongUsClient.Instance.IsGamePublic ? PublicGameImage : PrivateGameImage);
		}
	}

	public void ShareGameInvite()
	{
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			DestroyableSingleton<DiscordManager>.Instance.ShareGameOnDiscord();
		}
	}

	public void Update()
	{
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)GameData.Instance) || !Object.op_Implicit((Object)(object)GameManager.Instance))
		{
			return;
		}
		MakePublicButton.sprite = (AmongUsClient.Instance.IsGamePublic ? PublicGameImage : PrivateGameImage);
		((TMP_Text)privatePublicText).text = (AmongUsClient.Instance.IsGamePublic ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PublicHeader) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PrivateHeader));
		if ((Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305)) && Input.GetKeyDown((KeyCode)99))
		{
			ClipboardHelper.PutClipboardString(GameCode.IntToGameName(AmongUsClient.Instance.GameId));
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			bool active = AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && DestroyableSingleton<DiscordManager>.Instance.CanShareGameOnDiscord() && DestroyableSingleton<DiscordManager>.Instance.HasValidPartyID();
			((Component)ShareOnDiscordButton).gameObject.SetActive(active);
		}
		if (GameData.Instance.PlayerCount != LastPlayerCount)
		{
			LastPlayerCount = GameData.Instance.PlayerCount;
			string arg = "<color=#FF0000FF>";
			if (LastPlayerCount > MinPlayers)
			{
				arg = "<color=#00FF00FF>";
			}
			if (LastPlayerCount == MinPlayers)
			{
				arg = "<color=#FFFF00FF>";
			}
			if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
			{
				((TMP_Text)PlayerCounter).text = $"{arg}{LastPlayerCount}/{15}";
			}
			else
			{
				((TMP_Text)PlayerCounter).text = $"{arg}{LastPlayerCount}/{GameManager.Instance.LogicOptions.MaxPlayers}";
			}
			StartButton.color = ((LastPlayerCount >= MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
			((Graphic)startLabelText).color = ((LastPlayerCount >= MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
			StartButtonGlyph?.SetColor((LastPlayerCount >= MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
			if (DestroyableSingleton<DiscordManager>.InstanceExists)
			{
				if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
				{
					DestroyableSingleton<DiscordManager>.Instance.SetInLobbyHost(LastPlayerCount, GameManager.Instance.LogicOptions.MaxPlayers, AmongUsClient.Instance.GameId);
				}
				else
				{
					DestroyableSingleton<DiscordManager>.Instance.SetInLobbyClient(LastPlayerCount, GameManager.Instance.LogicOptions.MaxPlayers, AmongUsClient.Instance.GameId);
				}
			}
		}
		if (!AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (startState == StartingStates.Countdown)
		{
			int num = Mathf.CeilToInt(countDownTimer);
			countDownTimer -= Time.deltaTime;
			int num2 = Mathf.CeilToInt(countDownTimer);
			((TMP_Text)GameStartText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameStarting, num2);
			if (num != num2)
			{
				PlayerControl.LocalPlayer.RpcSetStartCounter(num2);
			}
			if (num2 <= 0)
			{
				FinallyBegin();
			}
		}
		else
		{
			((TMP_Text)GameStartText).text = string.Empty;
		}
	}

	public void ResetStartState()
	{
		startState = StartingStates.NotStarting;
		if (Object.op_Implicit((Object)(object)StartButton) && Object.op_Implicit((Object)(object)((Component)StartButton).gameObject))
		{
			((Component)StartButton).gameObject.SetActive(AmongUsClient.Instance.AmHost);
		}
		if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
		}
	}

	public void CopyGameCode()
	{
		GUIUtility.systemCopyBuffer = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		Debug.Log((object)"[GameStartManager] Game name copied");
		if (copyGameCodeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(copyGameCodeCoroutine);
		}
		copyGameCodeCoroutine = CoCopyGameCode();
		((MonoBehaviour)this).StartCoroutine(copyGameCodeCoroutine);
	}

	private IEnumerator CoCopyGameCode()
	{
		((TMP_Text)GameRoomNameInfo).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CopiedText);
		yield return (object)new WaitForSeconds(2f);
		((TMP_Text)GameRoomNameInfo).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCodeInfo);
		copyGameCodeCoroutine = null;
	}

	public void SetStartCounter(sbyte sec)
	{
		if (sec == -1)
		{
			((TMP_Text)GameStartText).text = string.Empty;
			return;
		}
		((TMP_Text)GameStartText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameStarting, sec);
	}

	public void BeginGame()
	{
		if (startState == StartingStates.NotStarting)
		{
			if ((DataManager.Player.Onboarding.AlwaysShowMinPlayerWarning || !DataManager.Player.Onboarding.ViewedMinPlayerWarning) && GameData.Instance.PlayerCount == MinPlayers)
			{
				GameSizePopup.SetActive(true);
			}
			else if (GameData.Instance.PlayerCount < MinPlayers)
			{
				((MonoBehaviour)this).StartCoroutine(Effects.SwayX(PlayerCounter.transform));
			}
			else
			{
				ReallyBegin(neverShow: false);
			}
		}
	}

	public void ReallyBegin(bool neverShow)
	{
		startState = StartingStates.Countdown;
		GameSizePopup.SetActive(false);
		if (neverShow)
		{
			DataManager.Player.Onboarding.AlwaysShowMinPlayerWarning = false;
		}
		DataManager.Player.Onboarding.ViewedMinPlayerWarning = true;
		DataManager.Player.Save();
		((Component)StartButton).gameObject.SetActive(false);
		countDownTimer = 5.0001f;
		startState = StartingStates.Countdown;
	}

	public void FinallyBegin()
	{
		if (startState == StartingStates.Countdown)
		{
			startState = StartingStates.Starting;
			if (DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
			{
				DestroyableSingleton<HudManager>.Instance.GameMenu.Close();
			}
			AmongUsClient.Instance.StartGame();
			AmongUsClient.Instance.DisconnectHandlers.Remove(this);
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void HandleDisconnect(PlayerControl pc, DisconnectReasons reason)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			LastPlayerCount = -1;
			if (Object.op_Implicit((Object)(object)StartButton))
			{
				((Component)StartButton).gameObject.SetActive(true);
			}
			((Behaviour)MakePublicButtonBehaviour).enabled = true;
		}
	}

	public void HandleDisconnect()
	{
		HandleDisconnect(null, DisconnectReasons.ExitGame);
	}

	public void ShowInviteMenu()
	{
	}

	public void ToggleButtonGlyphs(bool enabled)
	{
		MakePublicButtonGlyph.SetActive(enabled);
		StartButtonGlyphContainer.SetActive(enabled);
	}
}
