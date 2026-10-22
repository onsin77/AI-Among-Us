using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using TMPro;
using UnityEngine;

public class MeetingHud : InnerNetObject, IDisconnectHandler
{
	private struct VoterState
	{
		public byte VoterId;

		public byte VotedForId;

		public bool AmDead => VotedForId == 252;

		public bool SkippedVote => VotedForId == 253;

		public static VoterState Deserialize(MessageReader reader)
		{
			MessageReader val = reader.ReadMessage();
			return new VoterState
			{
				VoterId = val.Tag,
				VotedForId = val.ReadByte()
			};
		}

		public void Serialize(MessageWriter writer)
		{
			writer.StartMessage(VoterId);
			writer.Write(VotedForId);
			writer.EndMessage();
		}
	}

	public enum VoteStates
	{
		Animating,
		Discussion,
		NotVoted,
		Voted,
		Results,
		Proceeding
	}

	private const int NumColumns = 3;

	private const float ResultsTime = 5f;

	private const float Depth = 0f;

	public static MeetingHud Instance;

	public SpriteRenderer BlackBackground;

	public SpriteRenderer[] OuterMasks;

	public SpriteRenderer[] PlayerColoredParts;

	public MeetingIntroAnimation MeetingIntro;

	public Transform ButtonParent;

	public TextMeshPro TitleText;

	public Vector3 VoteOrigin = new Vector3(-3.6f, 1.75f);

	public Vector3 VoteButtonOffsets = Vector2.op_Implicit(new Vector2(3.6f, -0.91f));

	public PlayerVoteArea SkipVoteButton;

	private PlayerVoteArea[] playerStates = new PlayerVoteArea[0];

	public PlayerVoteArea PlayerButtonPrefab;

	public SpriteRenderer PlayerVotePrefab;

	public Sprite CrackedGlass;

	public SpriteRenderer Glass;

	public PassiveButton ProceedButton;

	public AudioClip VoteSound;

	public AudioClip VoteLockinSound;

	public AudioClip VoteEndingSound;

	[SerializeField]
	private Transform meetingContents;

	private VoteStates state;

	public GameObject SkippedVoting;

	public SpriteRenderer HostIcon;

	private GameData.PlayerInfo exiledPlayer;

	private bool wasTie;

	public TextMeshPro TimerText;

	public float discussionTimer;

	private byte reporterId;

	private bool amDead;

	private float resultsStartedAt;

	private int lastSecond = 10;

	private readonly Logger logger = new Logger("MeetingHud", Logger.Level.Debug, Logger.Category.UI);

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public UiElement ProceedButtonUi;

	public List<UiElement> ControllerSelectable;

	public VoteStates CurrentState => state;

	public void RpcClose()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			Close();
		}
		AmongUsClient.Instance.SendRpc(NetId, 22, (SendOption)1);
	}

	public void CmdCastVote(byte playerId, byte suspectIdx)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CastVote(playerId, suspectIdx);
			return;
		}
		MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(NetId, 24, (SendOption)1, AmongUsClient.Instance.HostId);
		val.Write(playerId);
		val.Write(suspectIdx);
		AmongUsClient.Instance.FinishRpcImmediately(val);
	}

	private void RpcVotingComplete(VoterState[] states, GameData.PlayerInfo exiled, bool tie)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			VotingComplete(states, exiled, tie);
		}
		MessageWriter val = AmongUsClient.Instance.StartRpc(NetId, 23, (SendOption)1);
		val.WritePacked(states.Length);
		foreach (VoterState voterState in states)
		{
			voterState.Serialize(val);
		}
		val.Write(exiled?.PlayerId ?? byte.MaxValue);
		val.Write(tie);
		val.EndMessage();
	}

	private void RpcClearVote(int clientId)
	{
		if (AmongUsClient.Instance.ClientId == clientId)
		{
			ClearVote();
			return;
		}
		MessageWriter msg = AmongUsClient.Instance.StartRpcImmediately(NetId, 25, (SendOption)1, clientId);
		AmongUsClient.Instance.FinishRpcImmediately(msg);
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(playerStates.Length);
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			writer.StartMessage(playerVoteArea.TargetPlayerId);
			playerStates[i].Serialize(writer);
			writer.EndMessage();
		}
		ClearDirtyBits();
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			PopulateButtons(0);
		}
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			MessageReader msg = reader.ReadMessage();
			PlayerVoteArea playerVoteArea = playerStates.FirstOrDefault((PlayerVoteArea ps) => ps.TargetPlayerId == msg.Tag);
			if (Object.op_Implicit((Object)(object)playerVoteArea))
			{
				playerVoteArea.Deserialize(msg);
				if (playerVoteArea.DidReport)
				{
					reporterId = playerVoteArea.TargetPlayerId;
				}
			}
		}
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.CloseMeeting:
			Close();
			break;
		case RpcCalls.CastVote:
		{
			byte srcPlayerId = reader.ReadByte();
			byte suspectPlayerId = reader.ReadByte();
			CastVote(srcPlayerId, suspectPlayerId);
			break;
		}
		case RpcCalls.VotingComplete:
		{
			VoterState[] array = new VoterState[reader.ReadPackedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = VoterState.Deserialize(reader);
			}
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
			bool tie = reader.ReadBoolean();
			VotingComplete(array, playerById, tie);
			break;
		}
		case RpcCalls.ClearVote:
			ClearVote();
			break;
		}
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void Awake()
	{
		if (!Object.op_Implicit((Object)(object)Instance))
		{
			Instance = this;
		}
		else if ((Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void Start()
	{
		BlackBackground.sprite = ShipStatus.Instance.MeetingBackground;
		SetMasksEnabled(enabled: false);
		SpriteRenderer[] playerColoredParts = PlayerColoredParts;
		foreach (SpriteRenderer playerMaterialColors in playerColoredParts)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
		((Component)DestroyableSingleton<HudManager>.Instance.Chat).gameObject.SetActive(true);
		DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
		DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		SkipVoteButton.SetTargetPlayerId(253);
		SkipVoteButton.Parent = this;
		((Component)Camera.main).GetComponent<FollowerCamera>().Locked = true;
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			SetForegroundForDead();
		}
		AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		PlayerVoteArea[] array = playerStates;
		foreach (PlayerVoteArea playerVoteArea in array)
		{
			if (!playerVoteArea.AmDead)
			{
				ControllerSelectable.Add(playerVoteArea.PlayerButton);
			}
		}
		DestroyableSingleton<AchievementManager>.Instance.OnMeetingCalled();
	}

	public override void OnDestroy()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		base.OnDestroy();
	}

	private void SetForegroundForDead()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		amDead = true;
		((Component)SkipVoteButton).gameObject.SetActive(false);
		SkipVoteButton.AmDead = true;
		Glass.sprite = CrackedGlass;
		Glass.color = Palette.White_75Alpha;
	}

	public void Update()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		if (state == VoteStates.Animating)
		{
			return;
		}
		discussionTimer += Time.deltaTime;
		UpdateButtons();
		LogicOptionsNormal logicOptionsNormal = GameManager.Instance.LogicOptions as LogicOptionsNormal;
		switch (state)
		{
		case VoteStates.Discussion:
		{
			int discussionTime = logicOptionsNormal.GetDiscussionTime();
			if (discussionTimer < (float)discussionTime)
			{
				float num3 = (float)discussionTime - discussionTimer;
				UpdateTimerText(StringNames.MeetingVotingBegins, Mathf.CeilToInt(num3));
				for (int i = 0; i < playerStates.Length; i++)
				{
					playerStates[i].SetDisabled();
				}
				SkipVoteButton.SetDisabled();
				break;
			}
			state = VoteStates.NotVoted;
			bool active = logicOptionsNormal.GetVotingTime() > 0;
			((Component)TimerText).gameObject.SetActive(active);
			for (int j = 0; j < playerStates.Length; j++)
			{
				playerStates[j].SetEnabled();
			}
			SkipVoteButton.SetEnabled();
			break;
		}
		case VoteStates.NotVoted:
		case VoteStates.Voted:
		{
			int votingTime = logicOptionsNormal.GetVotingTime();
			if (votingTime > 0)
			{
				float num4 = discussionTimer - (float)logicOptionsNormal.GetDiscussionTime();
				float num5 = Mathf.Max(0f, (float)votingTime - num4);
				UpdateTimerText(StringNames.MeetingVotingEnds, Mathf.CeilToInt(num5));
				if (state == VoteStates.NotVoted && Mathf.CeilToInt(num5) <= lastSecond)
				{
					lastSecond--;
					((MonoBehaviour)this).StartCoroutine(Effects.PulseColor(TimerText, Color.red, Color.white, 0.25f));
					SoundManager.Instance.PlaySound(VoteEndingSound, loop: false).pitch = Mathf.Lerp(1.5f, 0.8f, (float)lastSecond / 10f);
				}
				if (AmongUsClient.Instance.AmHost && num4 >= (float)votingTime)
				{
					ForceSkipAll();
				}
			}
			break;
		}
		case VoteStates.Results:
			if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
			{
				float num = discussionTimer - resultsStartedAt;
				float num2 = Mathf.Max(0f, 5f - num);
				UpdateTimerText(StringNames.MeetingProceeds, Mathf.CeilToInt(num2));
				if (AmongUsClient.Instance.AmHost && num2 <= 0f)
				{
					HandleProceed();
				}
			}
			break;
		}
	}

	private void UpdateTimerText(StringNames key, int value)
	{
		((TMP_Text)TimerText).text = DestroyableSingleton<TranslationController>.Instance.GetString(key, value);
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.UpdateVoteTimerText(key, value);
		}
	}

	private void SetMasksEnabled(bool enabled)
	{
		SpriteRenderer[] outerMasks = OuterMasks;
		for (int i = 0; i < outerMasks.Length; i++)
		{
			((Renderer)outerMasks[i]).enabled = enabled;
		}
	}

	public IEnumerator CoIntro(GameData.PlayerInfo reporter, GameData.PlayerInfo reportedBody, GameData.PlayerInfo[] deadBodies)
	{
		SkipVoteButton.SetDisabled();
		((Component)this).transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform);
		((Component)this).transform.localPosition = Vector3.zero;
		meetingContents.localPosition = new Vector3(0f, -10f, 0f);
		DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
		DestroyableSingleton<HudManager>.Instance.SetHudActive(isActive: false);
		bool flag = reportedBody == null;
		MeetingCalledAnimation prefab = (flag ? ShipStatus.Instance.EmergencyOverlay : ShipStatus.Instance.ReportOverlay);
		GameData.PlayerInfo playerInfo = (flag ? reporter : reportedBody);
		DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowMeeting(prefab, playerInfo);
		yield return DestroyableSingleton<HudManager>.Instance.KillOverlay.WaitForFinish();
		MeetingIntro.Init(reporter, deadBodies);
		SetMasksEnabled(enabled: true);
		yield return Effects.Slide2D(meetingContents, new Vector2(0f, -10f), new Vector2(0f, 0f), 0.25f);
		yield return Effects.Wait(0.5f);
		yield return MeetingIntro.CoRun();
		SetMasksEnabled(enabled: false);
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingWhoIsTitle);
		state = VoteStates.Discussion;
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, DefaultButtonSelected, ControllerSelectable);
		ConsoleJoystick.SetMode_Menu();
	}

	private IEnumerator CoStartCutscene()
	{
		ConsoleJoystick.SetMode_Task();
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f);
		ExileController exileController = Object.Instantiate<ExileController>(ShipStatus.Instance.ExileCutscenePrefab);
		((Component)exileController).transform.SetParent(((Component)DestroyableSingleton<HudManager>.Instance).transform, false);
		((Component)exileController).transform.localPosition = new Vector3(0f, 0f, -60f);
		exileController.Begin(exiledPlayer, wasTie);
		DespawnOnDestroy = false;
		if (Object.op_Implicit((Object)(object)MapBehaviour.Instance))
		{
			MapBehaviour.Instance.Close();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void ServerStart(byte reporter)
	{
		reporterId = reporter;
		PopulateButtons(reporter);
	}

	public void Close()
	{
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(data.IsDead);
		DestroyableSingleton<HudManager>.Instance.Chat.HideBanButton();
		((MonoBehaviour)this).StartCoroutine(CoStartCutscene());
	}

	private void VotingComplete(VoterState[] states, GameData.PlayerInfo exiled, bool tie)
	{
		if (state == VoteStates.Results)
		{
			return;
		}
		state = VoteStates.Results;
		resultsStartedAt = discussionTimer;
		exiledPlayer = exiled;
		wasTie = tie;
		((Component)SkipVoteButton).gameObject.SetActive(false);
		SkippedVoting.gameObject.SetActive(true);
		AmongUsClient.Instance.DisconnectHandlers.Remove(this);
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			PlayerControl playerControl = GameData.Instance.AllPlayers[i].Object;
			if ((Object)(object)playerControl != (Object)null && playerControl.Data != null && Object.op_Implicit((Object)(object)playerControl.Data.Role))
			{
				playerControl.Data.Role.OnVotingComplete();
			}
		}
		PopulateResults(states);
		SetupProceedButton();
		try
		{
			VoterState voterState = states.FirstOrDefault((VoterState s) => s.VoterId == PlayerControl.LocalPlayer.PlayerId);
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VotedForId);
			DestroyableSingleton<AchievementManager>.Instance.OnMeetingVote(PlayerControl.LocalPlayer.Data, playerById);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
			ControllerManager.Instance.CloseOverlayMenu(((Object)DestroyableSingleton<HudManager>.Instance.Chat).name);
		}
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, ProceedButtonUi);
		DestroyableSingleton<DebugAnalytics>.Instance.Analytics.MeetingEnded(Time.realtimeSinceStartup - TempData.TimeLastMeetingStarted, exiled);
	}

	public bool Select(int suspectStateIdx)
	{
		LogicOptionsNormal logicOptionsNormal = GameManager.Instance.LogicOptions as LogicOptionsNormal;
		if (discussionTimer < (float)logicOptionsNormal.GetDiscussionTime())
		{
			return false;
		}
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return false;
		}
		SoundManager.Instance.PlaySound(VoteSound, loop: false).volume = 0.8f;
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (suspectStateIdx != playerVoteArea.TargetPlayerId)
			{
				playerVoteArea.ClearButtons();
			}
		}
		if (suspectStateIdx != -1)
		{
			SkipVoteButton.ClearButtons();
		}
		return true;
	}

	public void Confirm(byte suspectStateIdx)
	{
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			for (int i = 0; i < playerStates.Length; i++)
			{
				PlayerVoteArea obj = playerStates[i];
				obj.ClearButtons();
				obj.voteComplete = true;
			}
			SkipVoteButton.ClearButtons();
			SkipVoteButton.voteComplete = true;
			((Component)SkipVoteButton).gameObject.SetActive(false);
			VoteStates voteStates = state;
			if (voteStates != VoteStates.NotVoted)
			{
				_ = 4;
				return;
			}
			state = VoteStates.Voted;
			CmdCastVote(PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);
		}
	}

	public void HandleDisconnect(PlayerControl pc, DisconnectReasons reason)
	{
		if (!AmongUsClient.Instance.AmHost || playerStates == null || !Object.op_Implicit((Object)(object)pc) || !Object.op_Implicit((Object)(object)GameData.Instance))
		{
			return;
		}
		int num = playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == pc.PlayerId);
		PlayerVoteArea obj = playerStates[num];
		obj.AmDead = true;
		((Component)obj.Overlay).gameObject.SetActive(true);
		for (int num2 = 0; num2 < playerStates.Length; num2++)
		{
			PlayerVoteArea playerVoteArea = playerStates[num2];
			if (playerVoteArea.AmDead || !playerVoteArea.DidVote || playerVoteArea.VotedFor != pc.PlayerId)
			{
				continue;
			}
			playerVoteArea.UnsetVote();
			SetDirtyBit(1u);
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
			if (playerById != null)
			{
				int clientIdFromCharacter = AmongUsClient.Instance.GetClientIdFromCharacter(playerById.Object);
				if (clientIdFromCharacter != -1)
				{
					RpcClearVote(clientIdFromCharacter);
				}
			}
		}
		SetDirtyBit(1u);
		CheckForEndVoting();
		if (state == VoteStates.Results)
		{
			SetupProceedButton();
		}
	}

	public void HandleDisconnect()
	{
	}

	private void ForceSkipAll()
	{
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (!playerVoteArea.DidVote)
			{
				playerVoteArea.VotedFor = 254;
				SetDirtyBit(1u);
			}
		}
		CheckForEndVoting();
	}

	public void CastVote(byte srcPlayerId, byte suspectPlayerId)
	{
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(srcPlayerId);
		GameData.PlayerInfo playerById2 = GameData.Instance.GetPlayerById(suspectPlayerId);
		logger.Debug("PlayerId:" + playerById.PlayerId + " has voted for " + ((playerById2 != null) ? ("PlayerId: " + playerById2.PlayerId) : "No one"));
		int num = playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == srcPlayerId);
		PlayerVoteArea playerVoteArea = playerStates[num];
		if (!playerVoteArea.AmDead && !playerVoteArea.DidVote)
		{
			if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId || AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame)
			{
				SoundManager.Instance.PlaySound(VoteLockinSound, loop: false);
			}
			playerVoteArea.SetVote(suspectPlayerId);
			SetDirtyBit(1u);
			CheckForEndVoting();
			PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
		}
	}

	public void ClearVote()
	{
		for (int i = 0; i < playerStates.Length; i++)
		{
			playerStates[i].voteComplete = false;
		}
		SkipVoteButton.voteComplete = false;
		((Component)SkipVoteButton).gameObject.SetActive(true);
		state = VoteStates.NotVoted;
	}

	private void CheckForEndVoting()
	{
		if (playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
		{
			Dictionary<byte, int> self = CalculateVotes();
			KeyValuePair<byte, int> max = self.MaxPair(out var tie);
			GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.FirstOrDefault((GameData.PlayerInfo v) => !tie && v.PlayerId == max.Key);
			VoterState[] array = new VoterState[playerStates.Length];
			for (int num = 0; num < playerStates.Length; num++)
			{
				PlayerVoteArea playerVoteArea = playerStates[num];
				array[num] = new VoterState
				{
					VoterId = playerVoteArea.TargetPlayerId,
					VotedForId = playerVoteArea.VotedFor
				};
			}
			RpcVotingComplete(array, exiled, tie);
		}
	}

	private Dictionary<byte, int> CalculateVotes()
	{
		Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != byte.MaxValue && playerVoteArea.VotedFor != 254)
			{
				if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var value))
				{
					dictionary[playerVoteArea.VotedFor] = value + 1;
				}
				else
				{
					dictionary[playerVoteArea.VotedFor] = 1;
				}
			}
		}
		return dictionary;
	}

	public void HandleProceed()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			if (state == VoteStates.Results)
			{
				state = VoteStates.Proceeding;
				RpcClose();
			}
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Effects.SwayX(((Component)HostIcon).transform));
		}
	}

	private void SetupProceedButton()
	{
		if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
		{
			((Component)TimerText).gameObject.SetActive(false);
			((Component)ProceedButton).gameObject.SetActive(true);
			((Component)HostIcon).gameObject.SetActive(true);
			GameData.PlayerInfo host = GameData.Instance.GetHost();
			if (host != null)
			{
				PlayerMaterial.SetColors(host.DefaultOutfit.ColorId, (Renderer)(object)HostIcon);
			}
			else
			{
				((Renderer)HostIcon).enabled = false;
			}
		}
	}

	private void PopulateResults(VoterState[] states)
	{
		((TMP_Text)TitleText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults);
		int num = 0;
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			playerVoteArea.ClearForResults();
			int num2 = 0;
			for (int j = 0; j < states.Length; j++)
			{
				VoterState voterState = states[j];
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
				if (playerById == null)
				{
					logger.Error($"Couldn't find player info for voter: {voterState.VoterId}");
				}
				else if (i == 0 && voterState.SkippedVote)
				{
					BloopAVoteIcon(playerById, num, SkippedVoting.transform);
					num++;
				}
				else if (voterState.VotedForId == playerVoteArea.TargetPlayerId)
				{
					BloopAVoteIcon(playerById, num2, ((Component)playerVoteArea).transform);
					num2++;
				}
			}
		}
	}

	private void BloopAVoteIcon(GameData.PlayerInfo voterPlayer, int index, Transform parent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		SpriteRenderer val = Object.Instantiate<SpriteRenderer>(PlayerVotePrefab);
		if (GameManager.Instance.LogicOptions.GetAnonymousVotes())
		{
			PlayerMaterial.SetColors(Palette.DisabledGrey, (Renderer)(object)val);
		}
		else
		{
			PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, (Renderer)(object)val);
		}
		((Component)val).transform.SetParent(parent);
		((Component)val).transform.localScale = Vector3.zero;
		PlayerVoteArea component = ((Component)parent).GetComponent<PlayerVoteArea>();
		if ((Object)(object)component != (Object)null)
		{
			((Renderer)val).material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
		}
		((MonoBehaviour)this).StartCoroutine(Effects.Bloop((float)index * 0.3f, ((Component)val).transform));
		((Component)parent).GetComponent<VoteSpreader>().AddVote(val);
	}

	private void UpdateButtons()
	{
		if (PlayerControl.LocalPlayer.Data.IsDead && !amDead)
		{
			SetForegroundForDead();
		}
		if (!AmongUsClient.Instance.AmHost)
		{
			return;
		}
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
			if (playerById == null)
			{
				playerVoteArea.SetDisabled();
				continue;
			}
			bool flag = playerById.Disconnected || playerById.IsDead;
			if (flag != playerVoteArea.AmDead)
			{
				playerVoteArea.SetDead(reporterId == playerById.PlayerId, flag, playerById.Role.Role == RoleTypes.GuardianAngel);
				SetDirtyBit(1u);
			}
		}
	}

	private void PopulateButtons(byte reporter)
	{
		playerStates = new PlayerVoteArea[GameData.Instance.PlayerCount];
		for (int i = 0; i < playerStates.Length; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			PlayerVoteArea playerVoteArea = (playerStates[i] = CreateButton(playerInfo));
			playerVoteArea.Parent = this;
			playerVoteArea.SetTargetPlayerId(playerInfo.PlayerId);
			playerVoteArea.SetDead(reporter == playerInfo.PlayerId, playerInfo.Disconnected || playerInfo.IsDead, (Object)(object)playerInfo.Role != (Object)null && playerInfo.Role.Role == RoleTypes.GuardianAngel);
			if ((Object)(object)playerInfo.Role == (Object)null)
			{
				logger.Error("player.Role is null when calling PopulateButtons in MeetingHud");
			}
			playerVoteArea.UpdateOverlay();
		}
		PlayerVoteArea[] array = playerStates;
		foreach (PlayerVoteArea playerVoteArea2 in array)
		{
			if (!playerVoteArea2.AmDead)
			{
				ControllerManager.Instance.AddSelectableUiElement(playerVoteArea2.PlayerButton);
			}
		}
		SortButtons();
	}

	private void SortButtons()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		PlayerVoteArea[] array = (from p in playerStates
			orderby p.AmDead ? 50 : 0, p.TargetPlayerId
			select p).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			int num2 = num % 3;
			int num3 = num / 3;
			((Component)array[num]).transform.localPosition = VoteOrigin + new Vector3(VoteButtonOffsets.x * (float)num2, VoteButtonOffsets.y * (float)num3, -0.9f - (float)num3 * 0.01f);
		}
	}

	private PlayerVoteArea CreateButton(GameData.PlayerInfo playerInfo)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		PlayerVoteArea playerVoteArea = Object.Instantiate<PlayerVoteArea>(PlayerButtonPrefab, ((Component)ButtonParent).transform);
		playerVoteArea.SetCosmetics(playerInfo);
		PlayerNameColor.Set(playerInfo, playerVoteArea.NameText);
		((Component)playerVoteArea).transform.localScale = Vector3.one;
		return playerVoteArea;
	}

	public bool DidVote(byte playerId)
	{
		return playerStates.First((PlayerVoteArea p) => p.TargetPlayerId == playerId).DidVote;
	}

	public int GetVotesRemaining()
	{
		try
		{
			return playerStates.Count((PlayerVoteArea ps) => !ps.AmDead && !ps.DidVote);
		}
		catch
		{
			return 0;
		}
	}

	public MeetingHud()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
	}
}
