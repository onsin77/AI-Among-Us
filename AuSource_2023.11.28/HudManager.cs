using System;
using System.Collections;
using System.Linq;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HudManager : DestroyableSingleton<HudManager>
{
	public FollowerCamera PlayerCam;

	public Camera UICamera;

	public MeetingHud MeetingPrefab;

	public KillButton KillButton;

	public AdminButton AdminButton;

	public SabotageButton SabotageButton;

	public VentButton ImpostorVentButton;

	public UseButton UseButton;

	public PetButton PetButton;

	public AbilityButton AbilityButton;

	public ReportButton ReportButton;

	public TextMeshPro GameSettings;

	public GameObject TaskStuff;

	public TaskPanelBehaviour TaskPanel;

	public CrewmatesKilledTracker CrewmatesKilled;

	public ChatController Chat;

	public DialogueBox Dialogue;

	public Transform TaskCompleteOverlay;

	private float taskDirtyTimer;

	public MeshRenderer ShadowQuad;

	public SpriteRenderer FullScreen;

	public PassiveButton MapButton;

	public GameObject MapButtonGlyph;

	public KillOverlay KillOverlay;

	public IVirtualJoystick joystick;

	public VirtualJoystick joystickR;

	public MonoBehaviour[] Joysticks;

	public MonoBehaviour RightVJoystick;

	public Collider2D LeftStickDeadZone;

	public Collider2D RightStickDeadZone;

	public DiscussBehaviour discussEmblem;

	public ShhhBehaviour shhhEmblem;

	public IntroCutscene IntroPrefab;

	public OptionsMenuBehaviour GameMenu;

	public NotificationPopper Notifier;

	public RoomTracker roomTracker;

	public AudioClip TaskCompleteSound;

	public AudioClip TaskUpdateSound;

	public Transform consoleUIRoot;

	public GameObject[] consoleUIObjects;

	public GameObject menuNavigationPrompts;

	public GameObject GameLoadAnimation;

	public LobbyTimerExtensionUI LobbyTimerExtensionUI;

	[NonSerialized]
	public float consoleUIHorizontalShift;

	public GameObject playerListPrompt;

	public AlertFlash AlertFlash;

	public DangerMeter DangerMeter;

	public GameObject SettingsButton;

	private StringBuilder tasksString = new StringBuilder();

	private DualshockLightManager.LightOverlayHandle lightFlashHandle;

	public Coroutine ReactorFlash { get; set; }

	public Coroutine OxyFlash { get; set; }

	public bool IsIntroDisplayed { get; private set; }

	public void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		playerListPrompt.SetActive(false);
		DelayedInitTouchType();
		((UnityEvent)MapButton.OnClick).AddListener((UnityAction)(() =>
		{
			ToggleMapVisible(GameManager.Instance.GetMapOptions());
		}));
	}

	public void OnGameStart()
	{
		Logger.GlobalInstance.Info("[HudManager] HudManager initializing on game start.");
		DelayedInitTouchType();
		LobbyTimerExtensionUI.HideAll();
	}

	public void ShowTaskComplete()
	{
		((MonoBehaviour)this).StartCoroutine(CoTaskComplete());
	}

	private IEnumerator CoTaskComplete()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(TaskCompleteSound, loop: false);
		}
		((Component)TaskCompleteOverlay).gameObject.SetActive(true);
		yield return Effects.Slide2D(TaskCompleteOverlay, new Vector2(0f, -8f), Vector2.zero, 0.25f);
		for (float time = 0f; time < 0.75f; time += Time.deltaTime)
		{
			yield return null;
		}
		yield return Effects.Slide2D(TaskCompleteOverlay, Vector2.zero, new Vector2(0f, 8f), 0.25f);
		((Component)TaskCompleteOverlay).gameObject.SetActive(false);
	}

	public void SetJoystickSize(float size)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (joystick != null && joystick is VirtualJoystick)
		{
			float num = Mathf.Lerp(0.65f, 1.1f, FloatRange.ReverseLerp(size, 0.5f, 1.5f));
			SetVirtualJoystickSize((VirtualJoystick)joystick, size, new Vector2(num, num));
			if ((Object)(object)joystickR != (Object)null)
			{
				num = Mathf.Lerp(0.65f, 1.1f, FloatRange.ReverseLerp(size, 0.5f, 1.5f));
				SetVirtualJoystickSize(joystickR, size, new Vector2(num + 0.4f, num - 0.6f));
			}
		}
	}

	private void SetVirtualJoystickSize(VirtualJoystick jStick, float size, Vector2 dist)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((Component)jStick).transform.localScale = new Vector3(size, size, 1f);
		AspectPosition component = ((Component)jStick).GetComponent<AspectPosition>();
		component.DistanceFromEdge = new Vector3(dist.x, dist.y, -10f);
		component.AdjustPosition();
	}

	public void SetTouchType(ControlTypes type)
	{
		if (joystick != null)
		{
			IVirtualJoystick virtualJoystick = joystick;
			Object.Destroy((Object)(object)((Component)((virtualJoystick is MonoBehaviour) ? virtualJoystick : null)).gameObject);
			joystick = null;
		}
		if ((Object)(object)joystickR != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)joystickR).gameObject);
			joystickR = null;
		}
		MonoBehaviour val = null;
		val = Object.Instantiate<MonoBehaviour>(Joysticks[(int)type]);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).transform.SetParent(((Component)this).transform, false);
			joystick = ((Component)val).GetComponent<IVirtualJoystick>();
		}
		GameOptionsManager.Instance.CurrentGameOptions.TryGetBool(BoolOptionNames.UseFlashlight, out var value);
		if (value && type == ControlTypes.VirtualJoystick)
		{
			MonoBehaviour val2 = null;
			val2 = Object.Instantiate<MonoBehaviour>(RightVJoystick);
			if ((Object)(object)val2 != (Object)null)
			{
				((Component)val2).transform.SetParent(((Component)this).transform, false);
				joystickR = ((Component)val2).GetComponent<VirtualJoystick>();
				joystickR.ToggleVisuals((Object)(object)LobbyBehaviour.Instance == (Object)null);
				Logger.GlobalInstance.Info(string.Format("[{0}] Initializing Right Joystick for Flashlight. [Use Flashlight: {1}] [Lobby: {2}]", "HudManager", value, (Object)(object)LobbyBehaviour.Instance != (Object)null));
			}
		}
		SetJoystickSize(DataManager.Settings.Input.TouchJoystickSize);
	}

	public void InitMap()
	{
		if (!Object.op_Implicit((Object)(object)MapBehaviour.Instance))
		{
			MapBehaviour.Instance = Object.Instantiate<MapBehaviour>(ShipStatus.Instance.MapPrefab, ((Component)this).transform);
			((Component)MapBehaviour.Instance).gameObject.SetActive(false);
		}
	}

	public void ToggleMapVisible(MapOptions options)
	{
		if (Object.op_Implicit((Object)(object)MapBehaviour.Instance) && ((Component)MapBehaviour.Instance).gameObject.activeSelf)
		{
			MapBehaviour.Instance.Close();
		}
		else if (!IsIntroDisplayed && Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			InitMap();
			MapBehaviour.Instance.Show(options);
		}
	}

	public void SetHudActive(PlayerControl localPlayer, RoleBehaviour role, bool isActive)
	{
		AbilityButton.ToggleVisible(isActive);
		if (isActive)
		{
			UseButton.Refresh();
			AbilityButton.Refresh(role.Ability);
		}
		else
		{
			UseButton.ToggleVisible(visible: false);
			PetButton.ToggleVisible(visible: false);
		}
		bool flag = localPlayer.Data != null && localPlayer.Data.IsDead;
		ReportButton.ToggleVisible(isActive && !flag && GameManager.Instance.CanReportBodies() && (Object)(object)ShipStatus.Instance != (Object)null);
		KillButton.ToggleVisible(isActive && role.IsImpostor && !flag);
		SabotageButton.ToggleVisible(isActive && role.IsImpostor);
		AdminButton.ToggleVisible(isActive && role.IsImpostor);
		ImpostorVentButton.ToggleVisible(isActive && !flag && role.IsImpostor && GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek);
		((Component)TaskPanel).gameObject.SetActive(isActive);
		((Component)roomTracker).gameObject.SetActive(isActive);
		if (joystick != null)
		{
			joystick.ToggleVisuals(isActive);
		}
		ToggleRightJoystick(isActive);
	}

	public void UpdateVoteTimerText(StringNames key, int value)
	{
		Chat.UpdateVoteTimerText(key, value);
	}

	public void SetHudActive(bool isActive)
	{
		SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, isActive);
	}

	public void ToggleRightJoystick(bool isActive)
	{
		if ((Object)(object)joystickR != (Object)null)
		{
			joystickR.ToggleVisuals(isActive);
		}
	}

	public void ToggleMapButton(bool isActive)
	{
		((Component)MapButton).gameObject.SetActive(isActive);
	}

	public void ToggleMapButtonGlyph(bool isActive)
	{
		MapButtonGlyph.SetActive(isActive);
	}

	public void ToggleSettingsButton(bool isActive)
	{
		SettingsButton.SetActive(isActive);
	}

	public void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)consoleUIRoot).transform.localPosition.x != consoleUIHorizontalShift)
		{
			Vector3 localPosition = ((Component)consoleUIRoot).transform.localPosition;
			localPosition.x = consoleUIHorizontalShift;
			((Component)consoleUIRoot).transform.localPosition = localPosition;
		}
		taskDirtyTimer += Time.deltaTime;
		if (!(taskDirtyTimer > 0.25f))
		{
			return;
		}
		float num = taskDirtyTimer;
		taskDirtyTimer = 0f;
		if (!Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer))
		{
			TaskPanel.SetTaskText(string.Empty);
			return;
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		if (data == null)
		{
			return;
		}
		bool flag = (Object)(object)data.Role != (Object)null && data.Role.IsImpostor;
		tasksString.Clear();
		if (PlayerControl.LocalPlayer.myTasks == null || PlayerControl.LocalPlayer.myTasks.Count == 0)
		{
			tasksString.Append("None");
		}
		else
		{
			for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
			{
				PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
				if (Object.op_Implicit((Object)(object)playerTask))
				{
					if (playerTask.TaskType == TaskTypes.FixComms && !flag)
					{
						tasksString.Clear();
						playerTask.AppendTaskText(tasksString);
						break;
					}
					playerTask.AppendTaskText(tasksString);
				}
			}
			if ((Object)(object)data.Role != (Object)null)
			{
				data.Role.AppendTaskHint(tasksString);
			}
			if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek && ShipStatus.Instance.HideCountdown > 0f)
			{
				ShipStatus.Instance.HideCountdown -= num;
				tasksString.Append("\n\n" + (int)ShipStatus.Instance.HideCountdown);
			}
			tasksString.TrimEnd();
		}
		TaskPanel.SetTaskText(tasksString.ToString());
	}

	public IEnumerator ShowEmblem(bool shhh)
	{
		if (shhh)
		{
			((Component)shhhEmblem).gameObject.SetActive(true);
			yield return shhhEmblem.PlayAnimation();
			((Component)shhhEmblem).gameObject.SetActive(false);
		}
		else
		{
			((Component)discussEmblem).gameObject.SetActive(true);
			yield return discussEmblem.PlayAnimation();
			((Component)discussEmblem).gameObject.SetActive(false);
		}
	}

	public void StartReactorFlash()
	{
		if (ReactorFlash == null)
		{
			ReactorFlash = ((MonoBehaviour)this).StartCoroutine(CoReactorFlash());
		}
	}

	public void StartOxyFlash()
	{
		if (OxyFlash == null)
		{
			OxyFlash = ((MonoBehaviour)this).StartCoroutine(CoReactorFlash());
		}
	}

	public void ShowPopUp(string text)
	{
		Dialogue.Show(text);
	}

	public void StopReactorFlash()
	{
		if (ReactorFlash != null)
		{
			((MonoBehaviour)this).StopCoroutine(ReactorFlash);
			ReactorFlash = null;
			((Component)FullScreen).gameObject.SetActive(false);
			PlainShipRoom plainShipRoom = ShipStatus.Instance.AllRooms.FirstOrDefault((PlainShipRoom r) => r is IReactorShipRoom);
			if (Object.op_Implicit((Object)(object)plainShipRoom))
			{
				((IReactorShipRoom)plainShipRoom).StopMeltdown();
			}
			if (lightFlashHandle != null)
			{
				lightFlashHandle.Dispose();
				lightFlashHandle = null;
			}
		}
	}

	public void StopOxyFlash()
	{
		if (OxyFlash != null)
		{
			((MonoBehaviour)this).StopCoroutine(OxyFlash);
			((Component)FullScreen).gameObject.SetActive(false);
			OxyFlash = null;
			if (lightFlashHandle != null)
			{
				lightFlashHandle.Dispose();
				lightFlashHandle = null;
			}
		}
	}

	public IEnumerator CoFadeFullScreen(Color source, Color target, float duration = 0.2f, bool showLoader = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		GameLoadAnimation.SetActive(showLoader);
		if (Chat.IsOpenOrOpening)
		{
			Chat.Close();
		}
		if (((Component)FullScreen).gameObject.activeSelf && FullScreen.color == target)
		{
			yield break;
		}
		((Component)FullScreen).gameObject.SetActive(true);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			if (!Object.op_Implicit((Object)(object)FullScreen))
			{
				yield break;
			}
			FullScreen.color = Color.Lerp(source, target, t / duration);
			yield return null;
		}
		if (Object.op_Implicit((Object)(object)FullScreen))
		{
			FullScreen.color = target;
			if (target.a < 0.05f)
			{
				((Component)FullScreen).gameObject.SetActive(false);
			}
		}
	}

	private IEnumerator CoReactorFlash()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		bool light = false;
		FullScreen.color = new Color(1f, 0f, 0f, 95f / 255f);
		while (true)
		{
			((Component)FullScreen).gameObject.SetActive(!((Component)FullScreen).gameObject.activeSelf);
			SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, loop: false);
			if (lightFlashHandle == null)
			{
				lightFlashHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
				lightFlashHandle.color = new Color(1f, 0f, 0f, 1f);
				lightFlashHandle.intensity = 1f;
			}
			light = !light;
			lightFlashHandle.color.a = (light ? 1f : 0f);
			yield return wait;
		}
	}

	public IEnumerator CoShowIntro()
	{
		while (!Object.op_Implicit((Object)(object)ShipStatus.Instance))
		{
			yield return null;
		}
		IsIntroDisplayed = true;
		LobbyTimerExtensionUI.HideAll();
		((Component)DestroyableSingleton<HudManager>.Instance.FullScreen).transform.localPosition = new Vector3(0f, 0f, -250f);
		yield return DestroyableSingleton<HudManager>.Instance.ShowEmblem(shhh: true);
		IntroCutscene introCutscene = Object.Instantiate<IntroCutscene>(IntroPrefab, ((Component)this).transform);
		yield return introCutscene.CoBegin();
		PlayerControl.LocalPlayer.SetKillTimer(10f);
		((SabotageSystemType)ShipStatus.Instance.Systems[SystemTypes.Sabotage]).SetInitialSabotageCooldown();
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out var value) && value is IDoorSystem)
		{
			((IDoorSystem)value).SetInitialSabotageCooldown();
		}
		yield return ShipStatus.Instance.PrespawnStep();
		PlayerControl.LocalPlayer.AdjustLighting();
		yield return CoFadeFullScreen(Color.black, Color.clear);
		((Component)FullScreen).transform.localPosition = new Vector3(0f, 0f, -500f);
		IsIntroDisplayed = false;
		((Component)CrewmatesKilled).gameObject.SetActive(GameManager.Instance.ShowCrewmatesKilled());
		GameManager.Instance.StartGame();
	}

	public void HideGameLoader()
	{
		GameLoadAnimation.SetActive(false);
	}

	public void OpenMeetingRoom(PlayerControl reporter)
	{
		if (!Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			Debug.Log((object)("Opening meeting room: " + (object)reporter));
			ShipStatus.Instance.RepairCriticalSabotages();
			MeetingHud.Instance = Object.Instantiate<MeetingHud>(MeetingPrefab);
			MeetingHud.Instance.ServerStart(reporter.PlayerId);
			AmongUsClient.Instance.Spawn(MeetingHud.Instance);
			TempData.OnMeetingStart();
			Chat.OnMeetingStart();
		}
	}

	public void SetAlertOverlay(bool enabled)
	{
		AlertFlash.SetOverlay(enabled);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (lightFlashHandle != null)
		{
			lightFlashHandle.Dispose();
			lightFlashHandle = null;
		}
	}

	public void NotifyOfDeath()
	{
		CrewmatesKilled.OnCrewmateKilled();
	}

	public void NotifyOfDisconnect(PlayerControl pc)
	{
		if (!pc.Data.Role.IsImpostor && ((Behaviour)CrewmatesKilled).isActiveAndEnabled)
		{
			CrewmatesKilled.OnCrewmateDisconnect();
		}
	}

	public void ToggleUseAndPetButton(IUsable useTarget, bool canPlayNormally, bool canPet)
	{
		if (!Object.op_Implicit((Object)(object)MapBehaviour.Instance) || !((Component)MapBehaviour.Instance).gameObject.activeSelf)
		{
			if ((useTarget == null) & canPlayNormally & canPet)
			{
				UseButton.ToggleVisible(visible: false);
				PetButton.ToggleVisible(visible: true);
				PetButton.SetTarget();
				PetButton.SetEnabled();
			}
			else
			{
				UseButton.ToggleVisible(visible: true);
				PetButton.ToggleVisible(visible: false);
				UseButton.SetTarget(useTarget);
			}
		}
	}

	public void ShowLobbyTimer(int timeRemainingSeconds)
	{
		LobbyTimerExtensionUI.ShowLobbyTimer(timeRemainingSeconds);
	}

	public void ShowLobbyTimerPopup(int timeRemainingSeconds, int timeGrantedSeconds)
	{
		LobbyTimerExtensionUI.ShowLobbyTimerPopup(timeRemainingSeconds, timeGrantedSeconds);
	}

	public void OnLobbyTimerExtended()
	{
		LobbyTimerExtensionUI.HideAll();
	}

	private void DelayedInitTouchType()
	{
		((MonoBehaviour)this).StartCoroutine(CoDelayedInitTouchType());
	}

	private IEnumerator CoDelayedInitTouchType()
	{
		yield return (object)new WaitForSeconds(0.3f);
		Logger.GlobalInstance.Info(string.Format("[{0}] InitTouchType : {1} Current Control Type: {2}", "HudManager", "ActiveInputManager", ActiveInputManager.currentControlType));
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			Logger.GlobalInstance.Info(string.Format("[{0}] InitTouchType : {1}", "HudManager", ControlTypes.Controller));
			SetTouchType(ControlTypes.Controller);
		}
		else
		{
			Logger.GlobalInstance.Info(string.Format("[{0}] InitTouchType : {1}", "HudManager", DataManager.Settings.Input.InputMode));
			SetTouchType(DataManager.Settings.Input.InputMode);
		}
	}
}
