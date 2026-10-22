using System;
using System.Collections.Generic;
using Rewired.Components;
using Rewired.UI;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Rewired.Integration.UnityUI;

[AddComponentMenu("Rewired/Rewired Standalone Input Module")]
public sealed class RewiredStandaloneInputModule : RewiredPointerInputModule
{
	[Serializable]
	public class PlayerSetting
	{
		public int playerId;

		public List<PlayerMouse> playerMice = new List<PlayerMouse>();

		public PlayerSetting()
		{
		}

		private PlayerSetting(PlayerSetting other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			playerId = other.playerId;
			playerMice = new List<PlayerMouse>();
			if (other.playerMice == null)
			{
				return;
			}
			foreach (PlayerMouse item in other.playerMice)
			{
				playerMice.Add(item);
			}
		}

		public PlayerSetting Clone()
		{
			return new PlayerSetting(this);
		}
	}

	private const string DEFAULT_ACTION_MOVE_HORIZONTAL = "UIHorizontal";

	private const string DEFAULT_ACTION_MOVE_VERTICAL = "UIVertical";

	private const string DEFAULT_ACTION_SUBMIT = "UISubmit";

	private const string DEFAULT_ACTION_CANCEL = "UICancel";

	[Tooltip("(Optional) Link the Rewired Input Manager here for easier access to Player ids, etc.")]
	[SerializeField]
	private InputManager_Base rewiredInputManager;

	[Tooltip("Use all Rewired game Players to control the UI. This does not include the System Player. If enabled, this setting overrides individual Player Ids set in Rewired Player Ids.")]
	[SerializeField]
	private bool useAllRewiredGamePlayers;

	[Tooltip("Allow the Rewired System Player to control the UI.")]
	[SerializeField]
	private bool useRewiredSystemPlayer;

	[Tooltip("A list of Player Ids that are allowed to control the UI. If Use All Rewired Game Players = True, this list will be ignored.")]
	[SerializeField]
	private int[] rewiredPlayerIds = new int[1];

	[SerializeField]
	[Tooltip("Allow only Players with Player.isPlaying = true to control the UI.")]
	private bool usePlayingPlayersOnly;

	[SerializeField]
	[Tooltip("Player Mice allowed to interact with the UI. Each Player that owns a Player Mouse must also be allowed to control the UI or the Player Mouse will not function.")]
	private List<PlayerMouse> playerMice = new List<PlayerMouse>();

	[Tooltip("Makes an axis press always move only one UI selection. Enable if you do not want to allow scrolling through UI elements by holding an axis direction.")]
	[SerializeField]
	private bool moveOneElementPerAxisPress;

	[Tooltip("If enabled, Action Ids will be used to set the Actions. If disabled, string names will be used to set the Actions.")]
	[SerializeField]
	private bool setActionsById;

	[SerializeField]
	[Tooltip("Id of the horizontal Action for movement (if axis events are used).")]
	private int horizontalActionId = -1;

	[SerializeField]
	[Tooltip("Id of the vertical Action for movement (if axis events are used).")]
	private int verticalActionId = -1;

	[SerializeField]
	[Tooltip("Id of the Action used to submit.")]
	private int submitActionId = -1;

	[Tooltip("Id of the Action used to cancel.")]
	[SerializeField]
	private int cancelActionId = -1;

	[SerializeField]
	[Tooltip("Name of the horizontal axis for movement (if axis events are used).")]
	private string m_HorizontalAxis = "UIHorizontal";

	[SerializeField]
	[Tooltip("Name of the vertical axis for movement (if axis events are used).")]
	private string m_VerticalAxis = "UIVertical";

	[SerializeField]
	[Tooltip("Name of the action used to submit.")]
	private string m_SubmitButton = "UISubmit";

	[Tooltip("Name of the action used to cancel.")]
	[SerializeField]
	private string m_CancelButton = "UICancel";

	[SerializeField]
	[Tooltip("Number of selection changes allowed per second when a movement button/axis is held in a direction.")]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	[Tooltip("Delay in seconds before vertical/horizontal movement starts repeating continouously when a movement direction is held.")]
	private float m_RepeatDelay;

	[SerializeField]
	[Tooltip("Allows the mouse to be used to select elements.")]
	private bool m_allowMouseInput = true;

	[SerializeField]
	[Tooltip("Allows the mouse to be used to select elements if the device also supports touch control.")]
	private bool m_allowMouseInputIfTouchSupported = true;

	[SerializeField]
	[Tooltip("Allows touch input to be used to select elements.")]
	private bool m_allowTouchInput = true;

	[Tooltip("Deselects the current selection on mouse/touch click when the pointer is not over a selectable object.")]
	[SerializeField]
	private bool m_deselectIfBackgroundClicked = true;

	[Tooltip("Deselects the current selection on mouse/touch click before selecting the next object.")]
	[SerializeField]
	private bool m_deselectBeforeSelecting = true;

	[SerializeField]
	[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
	[Tooltip("Forces the module to always be active.")]
	private bool m_ForceModuleActive;

	[NonSerialized]
	private int[] playerIds;

	private bool recompiling;

	[NonSerialized]
	private bool isTouchSupported;

	[NonSerialized]
	private double m_PrevActionTime;

	[NonSerialized]
	private Vector2 m_LastMoveVector;

	[NonSerialized]
	private int m_ConsecutiveMoveCount;

	[NonSerialized]
	private bool m_HasFocus = true;

	public InputManager_Base RewiredInputManager
	{
		get
		{
			return rewiredInputManager;
		}
		set
		{
			rewiredInputManager = value;
		}
	}

	public bool UseAllRewiredGamePlayers
	{
		get
		{
			return useAllRewiredGamePlayers;
		}
		set
		{
			bool flag = value != useAllRewiredGamePlayers;
			useAllRewiredGamePlayers = value;
			if (flag)
			{
				SetupRewiredVars();
			}
		}
	}

	public bool UseRewiredSystemPlayer
	{
		get
		{
			return useRewiredSystemPlayer;
		}
		set
		{
			bool flag = value != useRewiredSystemPlayer;
			useRewiredSystemPlayer = value;
			if (flag)
			{
				SetupRewiredVars();
			}
		}
	}

	public int[] RewiredPlayerIds
	{
		get
		{
			return (int[])rewiredPlayerIds.Clone();
		}
		set
		{
			rewiredPlayerIds = ((value != null) ? ((int[])value.Clone()) : new int[0]);
			SetupRewiredVars();
		}
	}

	public bool UsePlayingPlayersOnly
	{
		get
		{
			return usePlayingPlayersOnly;
		}
		set
		{
			usePlayingPlayersOnly = value;
		}
	}

	public List<PlayerMouse> PlayerMice
	{
		get
		{
			return new List<PlayerMouse>(playerMice);
		}
		set
		{
			if (value == null)
			{
				playerMice = new List<PlayerMouse>();
				SetupRewiredVars();
			}
			else
			{
				playerMice = new List<PlayerMouse>(value);
				SetupRewiredVars();
			}
		}
	}

	public bool MoveOneElementPerAxisPress
	{
		get
		{
			return moveOneElementPerAxisPress;
		}
		set
		{
			moveOneElementPerAxisPress = value;
		}
	}

	public bool allowMouseInput
	{
		get
		{
			return m_allowMouseInput;
		}
		set
		{
			m_allowMouseInput = value;
		}
	}

	public bool allowMouseInputIfTouchSupported
	{
		get
		{
			return m_allowMouseInputIfTouchSupported;
		}
		set
		{
			m_allowMouseInputIfTouchSupported = value;
		}
	}

	public bool allowTouchInput
	{
		get
		{
			return m_allowTouchInput;
		}
		set
		{
			m_allowTouchInput = value;
		}
	}

	public bool deselectIfBackgroundClicked
	{
		get
		{
			return m_deselectIfBackgroundClicked;
		}
		set
		{
			m_deselectIfBackgroundClicked = value;
		}
	}

	private bool deselectBeforeSelecting
	{
		get
		{
			return m_deselectBeforeSelecting;
		}
		set
		{
			m_deselectBeforeSelecting = value;
		}
	}

	public bool SetActionsById
	{
		get
		{
			return setActionsById;
		}
		set
		{
			if (setActionsById != value)
			{
				setActionsById = value;
				SetupRewiredVars();
			}
		}
	}

	public int HorizontalActionId
	{
		get
		{
			return horizontalActionId;
		}
		set
		{
			if (value != horizontalActionId)
			{
				horizontalActionId = value;
				if (ReInput.isReady)
				{
					m_HorizontalAxis = ((ReInput.mapping.GetAction(value) != null) ? ReInput.mapping.GetAction(value).name : string.Empty);
				}
			}
		}
	}

	public int VerticalActionId
	{
		get
		{
			return verticalActionId;
		}
		set
		{
			if (value != verticalActionId)
			{
				verticalActionId = value;
				if (ReInput.isReady)
				{
					m_VerticalAxis = ((ReInput.mapping.GetAction(value) != null) ? ReInput.mapping.GetAction(value).name : string.Empty);
				}
			}
		}
	}

	public int SubmitActionId
	{
		get
		{
			return submitActionId;
		}
		set
		{
			if (value != submitActionId)
			{
				submitActionId = value;
				if (ReInput.isReady)
				{
					m_SubmitButton = ((ReInput.mapping.GetAction(value) != null) ? ReInput.mapping.GetAction(value).name : string.Empty);
				}
			}
		}
	}

	public int CancelActionId
	{
		get
		{
			return cancelActionId;
		}
		set
		{
			if (value != cancelActionId)
			{
				cancelActionId = value;
				if (ReInput.isReady)
				{
					m_CancelButton = ((ReInput.mapping.GetAction(value) != null) ? ReInput.mapping.GetAction(value).name : string.Empty);
				}
			}
		}
	}

	protected override bool isMouseSupported
	{
		get
		{
			if (!base.isMouseSupported)
			{
				return false;
			}
			if (!m_allowMouseInput)
			{
				return false;
			}
			if (!isTouchSupported)
			{
				return true;
			}
			return m_allowMouseInputIfTouchSupported;
		}
	}

	private bool isTouchAllowed => m_allowTouchInput;

	[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead")]
	public bool allowActivationOnMobileDevice
	{
		get
		{
			return m_ForceModuleActive;
		}
		set
		{
			m_ForceModuleActive = value;
		}
	}

	public bool forceModuleActive
	{
		get
		{
			return m_ForceModuleActive;
		}
		set
		{
			m_ForceModuleActive = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
		}
	}

	public float repeatDelay
	{
		get
		{
			return m_RepeatDelay;
		}
		set
		{
			m_RepeatDelay = value;
		}
	}

	public string horizontalAxis
	{
		get
		{
			return m_HorizontalAxis;
		}
		set
		{
			if (!(m_HorizontalAxis == value))
			{
				m_HorizontalAxis = value;
				if (ReInput.isReady)
				{
					horizontalActionId = ReInput.mapping.GetActionId(value);
				}
			}
		}
	}

	public string verticalAxis
	{
		get
		{
			return m_VerticalAxis;
		}
		set
		{
			if (!(m_VerticalAxis == value))
			{
				m_VerticalAxis = value;
				if (ReInput.isReady)
				{
					verticalActionId = ReInput.mapping.GetActionId(value);
				}
			}
		}
	}

	public string submitButton
	{
		get
		{
			return m_SubmitButton;
		}
		set
		{
			if (!(m_SubmitButton == value))
			{
				m_SubmitButton = value;
				if (ReInput.isReady)
				{
					submitActionId = ReInput.mapping.GetActionId(value);
				}
			}
		}
	}

	public string cancelButton
	{
		get
		{
			return m_CancelButton;
		}
		set
		{
			if (!(m_CancelButton == value))
			{
				m_CancelButton = value;
				if (ReInput.isReady)
				{
					cancelActionId = ReInput.mapping.GetActionId(value);
				}
			}
		}
	}

	private RewiredStandaloneInputModule()
	{
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		isTouchSupported = defaultTouchInputSource.touchSupported;
		TouchInputModule component = ((Component)this).GetComponent<TouchInputModule>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
		ReInput.InitializedEvent += OnRewiredInitialized;
		InitializeRewired();
	}

	public override void UpdateModule()
	{
		CheckEditorRecompile();
		if (!recompiling && ReInput.isReady && !m_HasFocus)
		{
			ShouldIgnoreEventsOnNoFocus();
		}
	}

	public override bool IsModuleSupported()
	{
		return true;
	}

	public override bool ShouldActivateModule()
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Invalid comparison between Unknown and I4
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Invalid comparison between Unknown and I4
		if (!((BaseInputModule)this).ShouldActivateModule())
		{
			return false;
		}
		if (recompiling)
		{
			return false;
		}
		if (!ReInput.isReady)
		{
			return false;
		}
		bool flag = m_ForceModuleActive;
		for (int i = 0; i < playerIds.Length; i++)
		{
			Player player = ReInput.players.GetPlayer(playerIds[i]);
			if (player != null && (!usePlayingPlayersOnly || player.isPlaying))
			{
				flag |= GetButtonDown(player, submitActionId);
				flag |= GetButtonDown(player, cancelActionId);
				if (moveOneElementPerAxisPress)
				{
					flag |= GetButtonDown(player, horizontalActionId) || GetNegativeButtonDown(player, horizontalActionId);
					flag |= GetButtonDown(player, verticalActionId) || GetNegativeButtonDown(player, verticalActionId);
				}
				else
				{
					flag |= !Mathf.Approximately(GetAxis(player, horizontalActionId), 0f);
					flag |= !Mathf.Approximately(GetAxis(player, verticalActionId), 0f);
				}
			}
		}
		if (isMouseSupported)
		{
			flag |= DidAnyMouseMove();
			flag |= GetMouseButtonDownOnAnyMouse(0);
		}
		if (isTouchAllowed)
		{
			for (int j = 0; j < defaultTouchInputSource.touchCount; j++)
			{
				Touch touch = defaultTouchInputSource.GetTouch(j);
				flag |= (int)touch.phase == 0 || (int)touch.phase == 1 || (int)touch.phase == 2;
			}
		}
		return flag;
	}

	public override void ActivateModule()
	{
		if (m_HasFocus || !ShouldIgnoreEventsOnNoFocus())
		{
			((BaseInputModule)this).ActivateModule();
			GameObject val = ((BaseInputModule)this).eventSystem.currentSelectedGameObject;
			if ((Object)(object)val == (Object)null)
			{
				val = ((BaseInputModule)this).eventSystem.firstSelectedGameObject;
			}
			((BaseInputModule)this).eventSystem.SetSelectedGameObject(val, ((BaseInputModule)this).GetBaseEventData());
		}
	}

	public override void DeactivateModule()
	{
		((BaseInputModule)this).DeactivateModule();
		ClearSelection();
	}

	public override void Process()
	{
		if (!ReInput.isReady || (!m_HasFocus && ShouldIgnoreEventsOnNoFocus()) || !((Behaviour)this).enabled || !((Component)this).gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = SendUpdateEventToSelectedObject();
		if (((BaseInputModule)this).eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		if (!ProcessTouchEvents() && isMouseSupported)
		{
			ProcessMouseEvents();
		}
	}

	private bool ProcessTouchEvents()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!isTouchAllowed)
		{
			return false;
		}
		for (int i = 0; i < defaultTouchInputSource.touchCount; i++)
		{
			Touch touch = defaultTouchInputSource.GetTouch(i);
			if ((int)touch.type != 1)
			{
				PlayerPointerEventData touchPointerEventData = GetTouchPointerEventData(0, 0, touch, out var pressed, out var released);
				ProcessTouchPress((PointerEventData)(object)touchPointerEventData, pressed, released);
				if (!released)
				{
					ProcessMove(touchPointerEventData);
					ProcessDrag(touchPointerEventData);
				}
				else
				{
					RemovePointerData(touchPointerEventData);
				}
			}
		}
		return defaultTouchInputSource.touchCount > 0;
	}

	private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		RaycastResult pointerCurrentRaycast = pointerEvent.pointerCurrentRaycast;
		GameObject gameObject = pointerCurrentRaycast.gameObject;
		if (pressed)
		{
			pointerEvent.eligibleForClick = true;
			pointerEvent.delta = Vector2.zero;
			pointerEvent.dragging = false;
			pointerEvent.useDragThreshold = true;
			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			HandleMouseTouchDeselectionOnSelectionChanged(gameObject, (BaseEventData)(object)pointerEvent);
			if ((Object)(object)pointerEvent.pointerEnter != (Object)(object)gameObject)
			{
				((BaseInputModule)this).HandlePointerExitAndEnter(pointerEvent, gameObject);
				pointerEvent.pointerEnter = gameObject;
			}
			GameObject val = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerDownHandler);
			if ((Object)(object)val == (Object)null)
			{
				val = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			double unscaledTime = ReInput.time.unscaledTime;
			if ((Object)(object)val == (Object)(object)pointerEvent.lastPress)
			{
				if (unscaledTime - (double)pointerEvent.clickTime < 0.30000001192092896)
				{
					int clickCount = pointerEvent.clickCount + 1;
					pointerEvent.clickCount = clickCount;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.clickTime = (float)unscaledTime;
			}
			else
			{
				pointerEvent.clickCount = 1;
			}
			pointerEvent.pointerPress = val;
			pointerEvent.rawPointerPress = gameObject;
			pointerEvent.clickTime = (float)unscaledTime;
			pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if ((Object)(object)pointerEvent.pointerDrag != (Object)null)
			{
				ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (released)
		{
			ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if ((Object)(object)pointerEvent.pointerPress == (Object)(object)eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerPress, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			else if ((Object)(object)pointerEvent.pointerDrag != (Object)null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, (BaseEventData)(object)pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			if ((Object)(object)pointerEvent.pointerDrag != (Object)null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			if ((Object)(object)pointerEvent.pointerDrag != (Object)null)
			{
				ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.pointerDrag = null;
			ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerExitHandler);
			pointerEvent.pointerEnter = null;
		}
	}

	private bool SendSubmitEventToSelectedObject()
	{
		if ((Object)(object)((BaseInputModule)this).eventSystem.currentSelectedGameObject == (Object)null)
		{
			return false;
		}
		if (recompiling)
		{
			return false;
		}
		BaseEventData baseEventData = ((BaseInputModule)this).GetBaseEventData();
		for (int i = 0; i < playerIds.Length; i++)
		{
			Player player = ReInput.players.GetPlayer(playerIds[i]);
			if (player != null && (!usePlayingPlayersOnly || player.isPlaying))
			{
				if (GetButtonDown(player, submitActionId))
				{
					ExecuteEvents.Execute<ISubmitHandler>(((BaseInputModule)this).eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
					break;
				}
				if (GetButtonDown(player, cancelActionId))
				{
					ExecuteEvents.Execute<ICancelHandler>(((BaseInputModule)this).eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
					break;
				}
			}
		}
		return ((AbstractEventData)baseEventData).used;
	}

	private Vector2 GetRawMoveVector()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (recompiling)
		{
			return Vector2.zero;
		}
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < playerIds.Length; i++)
		{
			Player player = ReInput.players.GetPlayer(playerIds[i]);
			if (player == null || (usePlayingPlayersOnly && !player.isPlaying))
			{
				continue;
			}
			float num = GetAxis(player, horizontalActionId);
			float num2 = GetAxis(player, verticalActionId);
			if (Mathf.Approximately(num, 0f))
			{
				num = 0f;
			}
			if (Mathf.Approximately(num2, 0f))
			{
				num2 = 0f;
			}
			if (moveOneElementPerAxisPress)
			{
				if (GetButtonDown(player, horizontalActionId) && num > 0f)
				{
					zero.x++;
				}
				if (GetNegativeButtonDown(player, horizontalActionId) && num < 0f)
				{
					zero.x--;
				}
				if (GetButtonDown(player, verticalActionId) && num2 > 0f)
				{
					zero.y++;
				}
				if (GetNegativeButtonDown(player, verticalActionId) && num2 < 0f)
				{
					zero.y--;
				}
			}
			else
			{
				if (GetButton(player, horizontalActionId) && num > 0f)
				{
					zero.x++;
				}
				if (GetNegativeButton(player, horizontalActionId) && num < 0f)
				{
					zero.x--;
				}
				if (GetButton(player, verticalActionId) && num2 > 0f)
				{
					zero.y++;
				}
				if (GetNegativeButton(player, verticalActionId) && num2 < 0f)
				{
					zero.y--;
				}
			}
		}
		return zero;
	}

	private bool SendMoveEventToSelectedObject()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Invalid comparison between Unknown and I4
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (recompiling)
		{
			return false;
		}
		double unscaledTime = ReInput.time.unscaledTime;
		Vector2 rawMoveVector = GetRawMoveVector();
		if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
		{
			m_ConsecutiveMoveCount = 0;
			return false;
		}
		bool flag = Vector2.Dot(rawMoveVector, m_LastMoveVector) > 0f;
		CheckButtonOrKeyMovement(out var downHorizontal, out var downVertical);
		AxisEventData val = null;
		bool flag2 = downHorizontal | downVertical;
		if (flag2)
		{
			val = ((BaseInputModule)this).GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0f);
			MoveDirection moveDir = val.moveDir;
			flag2 = (((int)moveDir == 1 || (int)moveDir == 3) & downVertical) || (((int)moveDir == 0 || (int)moveDir == 2) & downHorizontal);
		}
		if (!flag2)
		{
			if (m_RepeatDelay > 0f)
			{
				flag2 = ((!flag || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + (double)(1f / m_InputActionsPerSecond)) : (unscaledTime > m_PrevActionTime + (double)m_RepeatDelay));
			}
			else
			{
				flag2 = unscaledTime > m_PrevActionTime + (double)(1f / m_InputActionsPerSecond);
			}
		}
		if (!flag2)
		{
			return false;
		}
		if (val == null)
		{
			val = ((BaseInputModule)this).GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0f);
		}
		if ((int)val.moveDir != 4)
		{
			ExecuteEvents.Execute<IMoveHandler>(((BaseInputModule)this).eventSystem.currentSelectedGameObject, (BaseEventData)(object)val, ExecuteEvents.moveHandler);
			if (!flag)
			{
				m_ConsecutiveMoveCount = 0;
			}
			if (m_ConsecutiveMoveCount == 0 || !(downHorizontal | downVertical))
			{
				m_ConsecutiveMoveCount++;
			}
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = rawMoveVector;
		}
		else
		{
			m_ConsecutiveMoveCount = 0;
		}
		return ((AbstractEventData)val).used;
	}

	private void CheckButtonOrKeyMovement(out bool downHorizontal, out bool downVertical)
	{
		downHorizontal = false;
		downVertical = false;
		for (int i = 0; i < playerIds.Length; i++)
		{
			Player player = ReInput.players.GetPlayer(playerIds[i]);
			if (player != null && (!usePlayingPlayersOnly || player.isPlaying))
			{
				downHorizontal |= GetButtonDown(player, horizontalActionId) || GetNegativeButtonDown(player, horizontalActionId);
				downVertical |= GetButtonDown(player, verticalActionId) || GetNegativeButtonDown(player, verticalActionId);
			}
		}
	}

	private void ProcessMouseEvents()
	{
		for (int i = 0; i < playerIds.Length; i++)
		{
			Player player = ReInput.players.GetPlayer(playerIds[i]);
			if (player != null && (!usePlayingPlayersOnly || player.isPlaying))
			{
				int mouseInputSourceCount = GetMouseInputSourceCount(playerIds[i]);
				for (int j = 0; j < mouseInputSourceCount; j++)
				{
					ProcessMouseEvent(playerIds[i], j);
				}
			}
		}
	}

	private void ProcessMouseEvent(int playerId, int pointerIndex)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		MouseState mousePointerEventData = GetMousePointerEventData(playerId, pointerIndex);
		if (mousePointerEventData == null)
		{
			return;
		}
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(0).eventData;
		ProcessMousePress(eventData);
		ProcessMove(eventData.buttonData);
		ProcessDrag(eventData.buttonData);
		ProcessMousePress(mousePointerEventData.GetButtonState(1).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(1).eventData.buttonData);
		ProcessMousePress(mousePointerEventData.GetButtonState(2).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(2).eventData.buttonData);
		IMouseInputSource mouseInputSource = GetMouseInputSource(playerId, pointerIndex);
		if (mouseInputSource != null)
		{
			for (int i = 3; i < mouseInputSource.buttonCount; i++)
			{
				ProcessMousePress(mousePointerEventData.GetButtonState(i).eventData);
				ProcessDrag(mousePointerEventData.GetButtonState(i).eventData.buttonData);
			}
			Vector2 scrollDelta = ((PointerEventData)eventData.buttonData).scrollDelta;
			if (!Mathf.Approximately(scrollDelta.sqrMagnitude, 0f))
			{
				RaycastResult pointerCurrentRaycast = ((PointerEventData)eventData.buttonData).pointerCurrentRaycast;
				ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(pointerCurrentRaycast.gameObject), (BaseEventData)(object)eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if ((Object)(object)((BaseInputModule)this).eventSystem.currentSelectedGameObject == (Object)null)
		{
			return false;
		}
		BaseEventData baseEventData = ((BaseInputModule)this).GetBaseEventData();
		ExecuteEvents.Execute<IUpdateSelectedHandler>(((BaseInputModule)this).eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return ((AbstractEventData)baseEventData).used;
	}

	private void ProcessMousePress(MouseButtonEventData data)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		PlayerPointerEventData buttonData = data.buttonData;
		if (GetMouseInputSource(buttonData.playerId, buttonData.inputSourceIndex) == null)
		{
			return;
		}
		RaycastResult pointerCurrentRaycast = ((PointerEventData)buttonData).pointerCurrentRaycast;
		GameObject gameObject = pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			((PointerEventData)buttonData).eligibleForClick = true;
			((PointerEventData)buttonData).delta = Vector2.zero;
			((PointerEventData)buttonData).dragging = false;
			((PointerEventData)buttonData).useDragThreshold = true;
			((PointerEventData)buttonData).pressPosition = ((PointerEventData)buttonData).position;
			((PointerEventData)buttonData).pointerPressRaycast = ((PointerEventData)buttonData).pointerCurrentRaycast;
			HandleMouseTouchDeselectionOnSelectionChanged(gameObject, (BaseEventData)(object)buttonData);
			GameObject val = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject, (BaseEventData)(object)buttonData, ExecuteEvents.pointerDownHandler);
			if ((Object)(object)val == (Object)null)
			{
				val = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			double unscaledTime = ReInput.time.unscaledTime;
			if ((Object)(object)val == (Object)(object)((PointerEventData)buttonData).lastPress)
			{
				if (unscaledTime - (double)((PointerEventData)buttonData).clickTime < 0.30000001192092896)
				{
					int clickCount = ((PointerEventData)buttonData).clickCount + 1;
					((PointerEventData)buttonData).clickCount = clickCount;
				}
				else
				{
					((PointerEventData)buttonData).clickCount = 1;
				}
				((PointerEventData)buttonData).clickTime = (float)unscaledTime;
			}
			else
			{
				((PointerEventData)buttonData).clickCount = 1;
			}
			((PointerEventData)buttonData).pointerPress = val;
			((PointerEventData)buttonData).rawPointerPress = gameObject;
			((PointerEventData)buttonData).clickTime = (float)unscaledTime;
			((PointerEventData)buttonData).pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if ((Object)(object)((PointerEventData)buttonData).pointerDrag != (Object)null)
			{
				ExecuteEvents.Execute<IInitializePotentialDragHandler>(((PointerEventData)buttonData).pointerDrag, (BaseEventData)(object)buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute<IPointerUpHandler>(((PointerEventData)buttonData).pointerPress, (BaseEventData)(object)buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if ((Object)(object)((PointerEventData)buttonData).pointerPress == (Object)(object)eventHandler && ((PointerEventData)buttonData).eligibleForClick)
			{
				ExecuteEvents.Execute<IPointerClickHandler>(((PointerEventData)buttonData).pointerPress, (BaseEventData)(object)buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if ((Object)(object)((PointerEventData)buttonData).pointerDrag != (Object)null && ((PointerEventData)buttonData).dragging)
			{
				ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject, (BaseEventData)(object)buttonData, ExecuteEvents.dropHandler);
			}
			((PointerEventData)buttonData).eligibleForClick = false;
			((PointerEventData)buttonData).pointerPress = null;
			((PointerEventData)buttonData).rawPointerPress = null;
			if ((Object)(object)((PointerEventData)buttonData).pointerDrag != (Object)null && ((PointerEventData)buttonData).dragging)
			{
				ExecuteEvents.Execute<IEndDragHandler>(((PointerEventData)buttonData).pointerDrag, (BaseEventData)(object)buttonData, ExecuteEvents.endDragHandler);
			}
			((PointerEventData)buttonData).dragging = false;
			((PointerEventData)buttonData).pointerDrag = null;
			if ((Object)(object)gameObject != (Object)(object)((PointerEventData)buttonData).pointerEnter)
			{
				((BaseInputModule)this).HandlePointerExitAndEnter((PointerEventData)(object)buttonData, (GameObject)null);
				((BaseInputModule)this).HandlePointerExitAndEnter((PointerEventData)(object)buttonData, gameObject);
			}
		}
	}

	private void HandleMouseTouchDeselectionOnSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
	{
		if (m_deselectIfBackgroundClicked && m_deselectBeforeSelecting)
		{
			DeselectIfSelectionChanged(currentOverGo, pointerEvent);
			return;
		}
		GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
		if (m_deselectIfBackgroundClicked)
		{
			if ((Object)(object)eventHandler != (Object)(object)((BaseInputModule)this).eventSystem.currentSelectedGameObject && (Object)(object)eventHandler != (Object)null)
			{
				((BaseInputModule)this).eventSystem.SetSelectedGameObject((GameObject)null, pointerEvent);
			}
		}
		else if (m_deselectBeforeSelecting && (Object)(object)eventHandler != (Object)null && (Object)(object)eventHandler != (Object)(object)((BaseInputModule)this).eventSystem.currentSelectedGameObject)
		{
			((BaseInputModule)this).eventSystem.SetSelectedGameObject((GameObject)null, pointerEvent);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		m_HasFocus = hasFocus;
	}

	private bool ShouldIgnoreEventsOnNoFocus()
	{
		if (!ReInput.isReady)
		{
			return true;
		}
		return ReInput.configuration.ignoreInputWhenAppNotInFocus;
	}

	protected override void OnDestroy()
	{
		((UIBehaviour)this).OnDestroy();
		ReInput.InitializedEvent -= OnRewiredInitialized;
		ReInput.ShutDownEvent -= OnRewiredShutDown;
		ReInput.EditorRecompileEvent -= OnEditorRecompile;
	}

	protected override bool IsDefaultPlayer(int playerId)
	{
		if (playerIds == null)
		{
			return false;
		}
		if (!ReInput.isReady)
		{
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < playerIds.Length; j++)
			{
				Player player = ReInput.players.GetPlayer(playerIds[j]);
				if (player != null && (i >= 1 || !usePlayingPlayersOnly || player.isPlaying) && (i >= 2 || player.controllers.hasMouse))
				{
					return playerIds[j] == playerId;
				}
			}
		}
		return false;
	}

	private void InitializeRewired()
	{
		if (!ReInput.isReady)
		{
			Debug.LogError((object)"Rewired is not initialized! Are you missing a Rewired Input Manager in your scene?");
			return;
		}
		ReInput.ShutDownEvent -= OnRewiredShutDown;
		ReInput.ShutDownEvent += OnRewiredShutDown;
		ReInput.EditorRecompileEvent -= OnEditorRecompile;
		ReInput.EditorRecompileEvent += OnEditorRecompile;
		SetupRewiredVars();
	}

	private void SetupRewiredVars()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		SetUpRewiredActions();
		if (useAllRewiredGamePlayers)
		{
			IList<Player> list = (useRewiredSystemPlayer ? ReInput.players.AllPlayers : ReInput.players.Players);
			playerIds = new int[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				playerIds[i] = list[i].id;
			}
		}
		else
		{
			bool flag = false;
			List<int> list2 = new List<int>(rewiredPlayerIds.Length + 1);
			for (int j = 0; j < rewiredPlayerIds.Length; j++)
			{
				Player player = ReInput.players.GetPlayer(rewiredPlayerIds[j]);
				if (player != null && !list2.Contains(player.id))
				{
					list2.Add(player.id);
					if (player.id == 9999999)
					{
						flag = true;
					}
				}
			}
			if (useRewiredSystemPlayer && !flag)
			{
				list2.Insert(0, ReInput.players.GetSystemPlayer().id);
			}
			playerIds = list2.ToArray();
		}
		SetUpRewiredPlayerMice();
	}

	private void SetUpRewiredPlayerMice()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		ClearMouseInputSources();
		for (int i = 0; i < playerMice.Count; i++)
		{
			PlayerMouse val = playerMice[i];
			if (!UnityTools.IsNullOrDestroyed<PlayerMouse>(val))
			{
				AddMouseInputSource((IMouseInputSource)(object)val);
			}
		}
	}

	private void SetUpRewiredActions()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		if (!setActionsById)
		{
			horizontalActionId = ReInput.mapping.GetActionId(m_HorizontalAxis);
			verticalActionId = ReInput.mapping.GetActionId(m_VerticalAxis);
			submitActionId = ReInput.mapping.GetActionId(m_SubmitButton);
			cancelActionId = ReInput.mapping.GetActionId(m_CancelButton);
			return;
		}
		InputAction action = ReInput.mapping.GetAction(horizontalActionId);
		m_HorizontalAxis = ((action != null) ? action.name : string.Empty);
		if (action == null)
		{
			horizontalActionId = -1;
		}
		action = ReInput.mapping.GetAction(verticalActionId);
		m_VerticalAxis = ((action != null) ? action.name : string.Empty);
		if (action == null)
		{
			verticalActionId = -1;
		}
		action = ReInput.mapping.GetAction(submitActionId);
		m_SubmitButton = ((action != null) ? action.name : string.Empty);
		if (action == null)
		{
			submitActionId = -1;
		}
		action = ReInput.mapping.GetAction(cancelActionId);
		m_CancelButton = ((action != null) ? action.name : string.Empty);
		if (action == null)
		{
			cancelActionId = -1;
		}
	}

	private bool GetButton(Player player, int actionId)
	{
		if (actionId < 0)
		{
			return false;
		}
		return player.GetButton(actionId);
	}

	private bool GetButtonDown(Player player, int actionId)
	{
		if (actionId < 0)
		{
			return false;
		}
		return player.GetButtonDown(actionId);
	}

	private bool GetNegativeButton(Player player, int actionId)
	{
		if (actionId < 0)
		{
			return false;
		}
		return player.GetNegativeButton(actionId);
	}

	private bool GetNegativeButtonDown(Player player, int actionId)
	{
		if (actionId < 0)
		{
			return false;
		}
		return player.GetNegativeButtonDown(actionId);
	}

	private float GetAxis(Player player, int actionId)
	{
		if (actionId < 0)
		{
			return 0f;
		}
		return player.GetAxis(actionId);
	}

	private void CheckEditorRecompile()
	{
		if (recompiling && ReInput.isReady)
		{
			recompiling = false;
			InitializeRewired();
		}
	}

	private void OnEditorRecompile()
	{
		recompiling = true;
		ClearRewiredVars();
	}

	private void ClearRewiredVars()
	{
		Array.Clear(playerIds, 0, playerIds.Length);
		ClearMouseInputSources();
	}

	private bool DidAnyMouseMove()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < playerIds.Length; i++)
		{
			int num = playerIds[i];
			Player player = ReInput.players.GetPlayer(num);
			if (player == null || (usePlayingPlayersOnly && !player.isPlaying))
			{
				continue;
			}
			int mouseInputSourceCount = GetMouseInputSourceCount(num);
			for (int j = 0; j < mouseInputSourceCount; j++)
			{
				IMouseInputSource mouseInputSource = GetMouseInputSource(num, j);
				if (mouseInputSource != null)
				{
					Vector2 screenPositionDelta = mouseInputSource.screenPositionDelta;
					if (screenPositionDelta.sqrMagnitude > 0f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool GetMouseButtonDownOnAnyMouse(int buttonIndex)
	{
		for (int i = 0; i < playerIds.Length; i++)
		{
			int num = playerIds[i];
			Player player = ReInput.players.GetPlayer(num);
			if (player == null || (usePlayingPlayersOnly && !player.isPlaying))
			{
				continue;
			}
			int mouseInputSourceCount = GetMouseInputSourceCount(num);
			for (int j = 0; j < mouseInputSourceCount; j++)
			{
				IMouseInputSource mouseInputSource = GetMouseInputSource(num, j);
				if (mouseInputSource != null && mouseInputSource.GetButtonDown(buttonIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnRewiredInitialized()
	{
		InitializeRewired();
	}

	private void OnRewiredShutDown()
	{
		ClearRewiredVars();
	}
}
