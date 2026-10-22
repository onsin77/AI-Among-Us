using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.Data;
using AmongUs.QuickChat;
using InnerNet;
using TMPro;
using UnityEngine;

public class ChatController : MonoBehaviour
{
	private delegate string GetFormattedNameFunc(string name);

	private const float SCREEN_SIZE_PERCENT = 0.8f;

	private const float MAX_CHAT_SEND_RATE = 3f;

	private const float X_DISTANCE_FROM_QUICKCHAT = -0.39f;

	private static readonly Logger Logger = new Logger("ChatController", Logger.Level.Info, Logger.Category.UI);

	[SerializeField]
	[Header("Buttons")]
	private GameObject chatButton;

	[SerializeField]
	private AspectPosition chatButtonAspectPosition;

	[Space(10f)]
	[SerializeField]
	private BanMenu banButton;

	[SerializeField]
	private GameObject openKeyboardButton;

	[SerializeField]
	private PassiveButton quickChatButton;

	[Header("Chat Screen")]
	[SerializeField]
	private GameObject chatScreen;

	[SerializeField]
	private ObjectPoolBehavior chatBubblePool;

	[SerializeField]
	private Scroller scroller;

	[SerializeField]
	private SpriteRenderer backgroundImage;

	[SerializeField]
	private SpriteRenderer chatNotifyDot;

	[SerializeField]
	private TextMeshPro sendRateMessageText;

	[SerializeField]
	private AspectPosition aspectPosition;

	[Header("Free Chat")]
	[SerializeField]
	private FreeChatInputField freeChatField;

	[Header("Quick Chat")]
	[SerializeField]
	private QuickChatMenu quickChatMenu;

	[SerializeField]
	private QuickChatPreviewField quickChatField;

	[SerializeField]
	[Header("SFX")]
	private AudioClip messageSound;

	[SerializeField]
	private AudioClip warningSound;

	[Header("Animations")]
	[SerializeField]
	private AnimationCurve chatAnimationIn;

	[SerializeField]
	private AnimationCurve chatAnimationOut;

	[SerializeField]
	[Header("Console Controller Navigation")]
	private UiElement backButton;

	[SerializeField]
	private UiElement defaultButtonSelected;

	[SerializeField]
	private List<UiElement> controllerSelectable;

	private ChatControllerState state;

	private Vector3 targetChatPosition;

	private float timeSinceLastMessage = 3f;

	private Coroutine notificationRoutine;

	private SpecialInputHandler specialInputHandler;

	private float targetScale;

	public ChatControllerState State
	{
		get
		{
			if (!chatScreen.activeInHierarchy)
			{
				return ChatControllerState.Closed;
			}
			return state;
		}
	}

	public bool IsOpenOrOpening
	{
		get
		{
			if (State != ChatControllerState.Opening)
			{
				return State == ChatControllerState.Open;
			}
			return true;
		}
	}

	public bool IsClosedOrClosing
	{
		get
		{
			if (State != ChatControllerState.Closing)
			{
				return State == ChatControllerState.Closed;
			}
			return true;
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (State != ChatControllerState.Opening)
			{
				return State == ChatControllerState.Closing;
			}
			return true;
		}
	}

	private void Awake()
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		specialInputHandler = ((Component)this).GetComponentInChildren<SpecialInputHandler>(true);
		freeChatField.OnSubmitEvent += OnFreeChatSubmit;
		freeChatField.OnChangedEvent += OnFreeChatChanged;
		quickChatButton.AddOnClickListeners(OnQuickChatButtonClicked);
		quickChatMenu.OnOpenEvent += OnQuickChatOpened;
		quickChatMenu.OnCloseEvent += OnQuickChatClosed;
		quickChatMenu.OnBuilderChangedEvent += OnQuickChatBuilderChanged;
		quickChatField.OnSubmitEvent += OnQuickChatSubmit;
		quickChatField.OnClearPressedEvent += OnQuickChatClearPressed;
		quickChatField.OnUndoPressedEvent += OnQuickChatUndoPressed;
		targetChatPosition = chatScreen.transform.localPosition;
	}

	private void OnEnable()
	{
		if ((Object)(object)specialInputHandler != (Object)null)
		{
			specialInputHandler.disableVirtualCursor = true;
		}
		DataManager.Settings.Multiplayer.OnChatModeChanged += OnChatModeSettingsChanged;
		ResolutionManager.ResolutionChanged += OnResolutionChanged;
		quickChatMenu.Close();
		UpdateChatMode();
	}

	private void OnDisable()
	{
		DataManager.Settings.Multiplayer.OnChatModeChanged -= OnChatModeSettingsChanged;
		ResolutionManager.ResolutionChanged -= OnResolutionChanged;
	}

	private void Update()
	{
		timeSinceLastMessage += Time.deltaTime;
		if (((Behaviour)sendRateMessageText).isActiveAndEnabled)
		{
			float num = 3f - timeSinceLastMessage;
			if (num < 0f)
			{
				((Component)sendRateMessageText).gameObject.SetActive(false);
				return;
			}
			((TMP_Text)sendRateMessageText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ChatRateLimit, Mathf.CeilToInt(num));
		}
	}

	private void LateUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(quickChatMenu.WorldPosition);
		Vector3 localPosition = chatScreen.transform.localPosition;
		localPosition.x = Mathf.Min(targetChatPosition.x, val.x - -0.39f * chatScreen.transform.localScale.x);
		chatScreen.transform.localPosition = localPosition;
	}

	public void SendChat()
	{
		float num = 3f - timeSinceLastMessage;
		if (num > 0f)
		{
			((Component)sendRateMessageText).gameObject.SetActive(true);
			((TMP_Text)sendRateMessageText).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ChatRateLimit, Mathf.CeilToInt(num));
			return;
		}
		if (quickChatMenu.CanSend)
		{
			SendQuickChat();
		}
		else
		{
			if (quickChatMenu.IsOpen || string.IsNullOrWhiteSpace(freeChatField.Text) || DataManager.Settings.Multiplayer.ChatMode != QuickChatModes.FreeChatOrQuickChat)
			{
				return;
			}
			SendFreeChat();
		}
		timeSinceLastMessage = 0f;
		freeChatField.Clear();
		quickChatMenu.Clear();
		quickChatField.Clear();
		UpdateChatMode();
	}

	public void OnMeetingStart()
	{
		quickChatMenu.Close();
	}

	private void UpdateChatMode()
	{
		bool flag = DataManager.Settings.Multiplayer.ChatMode != QuickChatModes.QuickChatOnly && quickChatMenu.IsEmpty && !quickChatMenu.IsOpen;
		if (freeChatField.Visible != flag)
		{
			freeChatField.SetVisible(flag);
		}
		if (quickChatField.Visible != !flag)
		{
			quickChatField.SetVisible(!flag);
		}
		banButton.Hide();
		CheckKeyboardButton();
	}

	private void CheckKeyboardButton()
	{
		bool flag = DataManager.Settings.Multiplayer.ChatMode != QuickChatModes.QuickChatOnly;
		if (openKeyboardButton.activeSelf != flag)
		{
			openKeyboardButton.SetActive(flag);
		}
	}

	private void OnChatModeSettingsChanged()
	{
		UpdateChatMode();
	}

	private void OnResolutionChanged(float aspectRatio, int width, int height, bool fullscreen)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		chatButtonAspectPosition.AdjustPosition();
		targetChatPosition = chatButton.transform.localPosition;
		Close();
	}

	public void SetVisible(bool visible)
	{
		Logger.Debug("SetVisible() :: Chat is " + (visible ? "visible" : "hidden"));
		ForceClosed();
		((Component)this).gameObject.SetActive(visible);
	}

	public void Close()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoClose());
	}

	public void ForceClosed()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		quickChatMenu.CloseImmediately();
		freeChatField.Unfocus();
		chatScreen.SetActive(false);
		banButton.SetVisible(show: false);
		banButton.Hide();
		state = ChatControllerState.Closed;
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapButtonGlyph(isActive: true);
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ToggleButtonGlyphs(enabled: true);
		}
		ConsoleJoystick.SetMode_Menu();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Toggle()
	{
		CustomNetworkTransform customNetworkTransform = (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) ? PlayerControl.LocalPlayer.NetTransform : null);
		if (!Object.op_Implicit((Object)(object)customNetworkTransform) || ((Component)DestroyableSingleton<FriendsListManager>.Instance.Ui).gameObject.activeSelf)
		{
			return;
		}
		((MonoBehaviour)this).StopAllCoroutines();
		if (IsOpenOrOpening)
		{
			((MonoBehaviour)this).StartCoroutine(CoClose());
			if (DestroyableSingleton<FriendsListManager>.InstanceExists)
			{
				DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(isGrayedOut: false);
			}
			return;
		}
		chatScreen.SetActive(true);
		customNetworkTransform.Halt();
		((MonoBehaviour)this).StartCoroutine(CoOpen());
		if (DestroyableSingleton<FriendsListManager>.InstanceExists)
		{
			DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(isGrayedOut: true);
		}
	}

	public void HideBanButton()
	{
		banButton.Hide();
	}

	public void UpdateVoteTimerText(StringNames key, int value)
	{
		quickChatMenu.UpdateVoteTimerText(key, value);
	}

	private IEnumerator CoOpen()
	{
		CheckKeyboardButton();
		state = ChatControllerState.Opening;
		Keyframe val = chatAnimationIn.keys.Last();
		float duration = val.time;
		chatButtonAspectPosition.AdjustPosition();
		quickChatMenu.Clear();
		quickChatField.Clear();
		quickChatMenu.Close();
		banButton.Hide();
		banButton.SetVisible(show: true);
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapButtonGlyph(isActive: false);
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ToggleButtonGlyphs(enabled: false);
		}
		ConsoleJoystick.SetMode_QuickChat();
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, backButton, defaultButtonSelected, controllerSelectable);
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			freeChatField.Focus();
		}
		Vector3 scale = Vector3.one;
		targetScale = AspectSize.CalculateSize(chatButton.transform.localPosition, backgroundImage.sprite, 0.8f);
		targetChatPosition = chatButton.transform.localPosition;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float num = chatAnimationIn.Evaluate(timer);
			scale.y = (scale.x = Mathf.LerpUnclamped(0.01f, targetScale, num));
			chatScreen.transform.localScale = scale;
			yield return null;
		}
		((Renderer)chatNotifyDot).enabled = false;
		state = ChatControllerState.Open;
	}

	private IEnumerator CoClose()
	{
		state = ChatControllerState.Closing;
		Keyframe val = chatAnimationOut.keys.Last();
		float duration = val.time;
		quickChatMenu.Close();
		banButton.Hide();
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.ToggleMapButtonGlyph(isActive: true);
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ToggleButtonGlyphs(enabled: true);
		}
		openKeyboardButton.SetActive(false);
		freeChatField.Unfocus();
		ConsoleJoystick.SetMode_None();
		Vector3 scale = Vector3.one;
		float targetScale = AspectSize.CalculateSize(chatButton.transform.localPosition, backgroundImage.sprite, 0.8f);
		targetChatPosition = chatButton.transform.localPosition;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float num = chatAnimationOut.Evaluate(timer);
			scale.y = (scale.x = Mathf.LerpUnclamped(targetScale, 0.01f, num));
			chatScreen.transform.localScale = scale;
			yield return null;
		}
		banButton.SetVisible(show: false);
		chatScreen.SetActive(false);
		ConsoleJoystick.SetMode_Menu();
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		state = ChatControllerState.Closed;
		freeChatField.ForceKeyboardClose();
	}

	public void AddChatNote(GameData.PlayerInfo srcPlayer, ChatNoteTypes noteType)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (srcPlayer == null)
		{
			return;
		}
		ChatBubble pooledBubble = GetPooledBubble();
		pooledBubble.SetCosmetics(srcPlayer);
		((Component)pooledBubble).transform.SetParent(scroller.Inner);
		((Component)pooledBubble).transform.localScale = Vector3.one;
		pooledBubble.SetNotification();
		if (noteType == ChatNoteTypes.DidVote)
		{
			int rem = MeetingHud.Instance.GetVotesRemaining();
			SetChatBubbleName(pooledBubble, srcPlayer, isDead: false, didVote: true, Color.green, (string returnedName) => DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingHasVoted, returnedName, rem));
		}
		pooledBubble.SetText(string.Empty);
		pooledBubble.AlignChildren();
		AlignAllBubbles();
		if (!IsOpenOrOpening && notificationRoutine == null)
		{
			notificationRoutine = ((MonoBehaviour)this).StartCoroutine(BounceDot());
		}
		if ((Object)(object)srcPlayer.Object != (Object)(object)PlayerControl.LocalPlayer)
		{
			SoundManager.Instance.PlaySound(messageSound, loop: false).pitch = 0.5f + (float)(int)srcPlayer.PlayerId / 15f;
		}
	}

	public void AddChat(PlayerControl sourcePlayer, string chatText, bool censor = true)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)sourcePlayer) || !Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			return;
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		GameData.PlayerInfo data2 = sourcePlayer.Data;
		if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
		{
			return;
		}
		ChatBubble pooledBubble = GetPooledBubble();
		try
		{
			((Component)pooledBubble).transform.SetParent(scroller.Inner);
			((Component)pooledBubble).transform.localScale = Vector3.one;
			bool flag = (Object)(object)sourcePlayer == (Object)(object)PlayerControl.LocalPlayer;
			if (flag)
			{
				pooledBubble.SetRight();
			}
			else
			{
				pooledBubble.SetLeft();
			}
			bool didVote = Object.op_Implicit((Object)(object)MeetingHud.Instance) && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
			pooledBubble.SetCosmetics(data2);
			SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2));
			if (censor && DataManager.Settings.Multiplayer.CensorChat)
			{
				chatText = BlockedWords.CensorWords(chatText);
			}
			pooledBubble.SetText(chatText);
			pooledBubble.AlignChildren();
			AlignAllBubbles();
			if (!IsOpenOrOpening && notificationRoutine == null)
			{
				notificationRoutine = ((MonoBehaviour)this).StartCoroutine(BounceDot());
			}
			if (!flag)
			{
				SoundManager.Instance.PlaySound(messageSound, loop: false).pitch = 0.5f + (float)(int)sourcePlayer.PlayerId / 15f;
			}
		}
		catch (Exception message)
		{
			Logger.Error(message);
			chatBubblePool.Reclaim(pooledBubble);
		}
	}

	public void AddChatWarning(string warningText)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ChatBubble pooledBubble = GetPooledBubble();
		try
		{
			((Component)pooledBubble).transform.SetParent(scroller.Inner);
			((Component)pooledBubble).transform.localScale = Vector3.one;
			pooledBubble.SetRight();
			pooledBubble.SetWarning(warningText);
			pooledBubble.AlignChildren();
			AlignAllBubbles();
			if (!IsOpenOrOpening && notificationRoutine == null)
			{
				notificationRoutine = ((MonoBehaviour)this).StartCoroutine(BounceDot());
			}
			SoundManager.Instance.PlaySound(warningSound, loop: false);
		}
		catch (Exception message)
		{
			Logger.Error(message);
			chatBubblePool.Reclaim(pooledBubble);
		}
	}

	private void SetChatBubbleName(ChatBubble bubble, GameData.PlayerInfo playerInfo, bool isDead, bool didVote, Color nameColor, GetFormattedNameFunc getFormattedNameFunc = null)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(playerInfo.PlayerName))
		{
			string playerName = ((getFormattedNameFunc == null) ? playerInfo.PlayerName : getFormattedNameFunc(playerInfo.PlayerName));
			bubble.SetName(playerName, isDead, didVote, nameColor);
			return;
		}
		bubble.SetName("...", isDead, didVote, nameColor);
		((MonoBehaviour)this).StartCoroutine(WaitForNameToExist(playerInfo, (GameData.PlayerInfo player) =>
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			string playerName2 = ((getFormattedNameFunc == null) ? playerInfo.PlayerName : getFormattedNameFunc(playerInfo.PlayerName));
			bubble.SetName(playerName2, isDead, didVote, nameColor);
			bubble.AlignChildren();
			AlignAllBubbles();
		}));
	}

	private IEnumerator WaitForNameToExist(GameData.PlayerInfo player, Action<GameData.PlayerInfo> onNameExistsCallback)
	{
		yield return null;
		while (string.IsNullOrEmpty(player.PlayerName))
		{
			yield return null;
		}
		onNameExistsCallback?.Invoke(player);
	}

	private void AlignAllBubbles()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		List<PoolableBehavior> activeChildren = chatBubblePool.activeChildren;
		for (int num2 = activeChildren.Count - 1; num2 >= 0; num2--)
		{
			ChatBubble chatBubble = activeChildren[num2] as ChatBubble;
			num += chatBubble.Background.size.y;
			Vector3 localPosition = ((Component)chatBubble).transform.localPosition;
			localPosition.y = -1.85f + num;
			((Component)chatBubble).transform.localPosition = localPosition;
			num += 0.15f;
		}
		float num3 = -0.3f;
		Scroller scroller = this.scroller;
		float num4 = 0f - num;
		Bounds bounds = this.scroller.Hitbox.bounds;
		scroller.SetYBoundsMin(Mathf.Min(0f, num4 + bounds.size.y + num3));
	}

	private IEnumerator BounceDot()
	{
		((Renderer)chatNotifyDot).enabled = true;
		yield return Effects.Bounce(((Component)chatNotifyDot).transform);
		notificationRoutine = null;
	}

	private ChatBubble GetPooledBubble()
	{
		if (chatBubblePool.NotInUse == 0)
		{
			chatBubblePool.ReclaimOldest();
		}
		return chatBubblePool.Get<ChatBubble>();
	}

	private void OnFreeChatSubmit()
	{
		SendChat();
	}

	private void OnFreeChatChanged()
	{
		quickChatMenu.Clear();
		UpdateChatMode();
	}

	private void SendFreeChat()
	{
		string text = freeChatField.Text;
		if (UrlFinder.TryFindUrl(text.ToCharArray(), out var _, out var _))
		{
			Logger.Warning(string.Format("{0}() :: ABORTED, URL was found. Showing {1} instead!", "SendFreeChat", StringNames.FreeChatLinkWarning));
			AddChatWarning(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FreeChatLinkWarning));
		}
		else
		{
			Logger.Debug("SendFreeChat () :: Sending message: '" + text + "'");
			PlayerControl.LocalPlayer.RpcSendChat(text);
		}
	}

	private void OpenQuickChat()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = targetScale;
		Bounds bounds = backgroundImage.sprite.bounds;
		float num2 = num * bounds.size.x;
		float num3 = 3.5f;
		Camera uICamera = DestroyableSingleton<HudManager>.Instance.UICamera;
		float num4 = num2 + -0.39f + num3 + 0.1f;
		float num5 = CameraSafeArea.GetSafeOrthographicSize(uICamera) * 2f * uICamera.aspect;
		if (num4 > num5)
		{
			num3 -= Mathf.Clamp(num4 - num5, 0f, 100f);
		}
		quickChatMenu.Open(Mathf.Max(num3, 2.5f));
	}

	private void OnQuickChatOpened()
	{
		chatButton.gameObject.SetActive(false);
		ConsoleJoystick.SetMode_QuickChat();
		UpdateChatMode();
	}

	private void OnQuickChatClosed()
	{
		chatButton.gameObject.SetActive(true);
		UpdateChatMode();
	}

	private void OnQuickChatSubmit()
	{
		if (!quickChatMenu.CanSend)
		{
			quickChatField.AnimateInvalidSend();
		}
		else
		{
			SendChat();
		}
	}

	private void OnQuickChatClearPressed()
	{
		quickChatMenu.Clear();
	}

	private void OnQuickChatUndoPressed()
	{
		quickChatMenu.Undo();
	}

	private void OnQuickChatBuilderChanged(QuickChatPhraseBuilder builder)
	{
		quickChatField.UpdatePreview(builder);
		UpdateChatMode();
	}

	private void OnQuickChatButtonClicked()
	{
		if (!IsClosedOrClosing && !quickChatMenu.IsAnimating)
		{
			if (quickChatMenu.IsOpen)
			{
				quickChatMenu.Close();
			}
			else
			{
				OpenQuickChat();
			}
		}
	}

	private void SendQuickChat()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("SendQuickChat() Sending message (see stack for more info)");
		quickChatMenu.BuildMessageLog(stringBuilder);
		Logger.Info(stringBuilder);
		PlayerControl.LocalPlayer.RpcSendQuickChat(quickChatMenu.BuildMessageData());
		quickChatMenu.Clear();
		quickChatField.Clear();
	}
}
