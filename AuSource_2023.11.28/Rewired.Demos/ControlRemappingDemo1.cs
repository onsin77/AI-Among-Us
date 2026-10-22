using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class ControlRemappingDemo1 : MonoBehaviour
{
	private class ControllerSelection
	{
		private int _id;

		private int _idPrev;

		private ControllerType _type;

		private ControllerType _typePrev;

		public int id
		{
			get
			{
				return _id;
			}
			set
			{
				_idPrev = _id;
				_id = value;
			}
		}

		public ControllerType type
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _type;
			}
			set
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				_typePrev = _type;
				_type = value;
			}
		}

		public int idPrev => _idPrev;

		public ControllerType typePrev
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _typePrev;
			}
		}

		public bool hasSelection => _id >= 0;

		public ControllerSelection()
		{
			Clear();
		}

		public void Set(int id, ControllerType type)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			this.id = id;
			this.type = type;
		}

		public void Clear()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			_id = -1;
			_idPrev = -1;
			_type = (ControllerType)2;
			_typePrev = (ControllerType)2;
		}
	}

	private class DialogHelper
	{
		public enum DialogType
		{
			None = 0,
			JoystickConflict = 1,
			ElementConflict = 2,
			KeyConflict = 3,
			DeleteAssignmentConfirmation = 10,
			AssignElement = 11
		}

		private const float openBusyDelay = 0.25f;

		private const float closeBusyDelay = 0.1f;

		private DialogType _type;

		private bool _enabled;

		private float _busyTime;

		private bool _busyTimerRunning;

		private Action<int> drawWindowDelegate;

		private WindowFunction drawWindowFunction;

		private WindowProperties windowProperties;

		private int currentActionId;

		private Action<int, UserResponse> resultCallback;

		private float busyTimer
		{
			get
			{
				if (!_busyTimerRunning)
				{
					return 0f;
				}
				return _busyTime - Time.realtimeSinceStartup;
			}
		}

		public bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (value)
				{
					if (_type != DialogType.None)
					{
						StateChanged(0.25f);
					}
				}
				else
				{
					_enabled = value;
					_type = DialogType.None;
					StateChanged(0.1f);
				}
			}
		}

		public DialogType type
		{
			get
			{
				if (!_enabled)
				{
					return DialogType.None;
				}
				return _type;
			}
			set
			{
				if (value == DialogType.None)
				{
					_enabled = false;
					StateChanged(0.1f);
				}
				else
				{
					_enabled = true;
					StateChanged(0.25f);
				}
				_type = value;
			}
		}

		public bool busy => _busyTimerRunning;

		public DialogHelper()
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected Obj, but got Unknown
			drawWindowDelegate = DrawWindow;
			drawWindowFunction = drawWindowDelegate.Invoke;
		}

		public void StartModal(int queueActionId, DialogType type, WindowProperties windowProperties, Action<int, UserResponse> resultCallback)
		{
			StartModal(queueActionId, type, windowProperties, resultCallback, -1f);
		}

		public void StartModal(int queueActionId, DialogType type, WindowProperties windowProperties, Action<int, UserResponse> resultCallback, float openBusyDelay)
		{
			currentActionId = queueActionId;
			this.windowProperties = windowProperties;
			this.type = type;
			this.resultCallback = resultCallback;
			if (openBusyDelay >= 0f)
			{
				StateChanged(openBusyDelay);
			}
		}

		public void Update()
		{
			Draw();
			UpdateTimers();
		}

		public void Draw()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			if (_enabled)
			{
				bool flag = GUI.enabled;
				GUI.enabled = true;
				GUILayout.Window(windowProperties.windowId, windowProperties.rect, drawWindowFunction, windowProperties.title, Array.Empty<GUILayoutOption>());
				GUI.FocusWindow(windowProperties.windowId);
				if (GUI.enabled != flag)
				{
					GUI.enabled = flag;
				}
			}
		}

		public void DrawConfirmButton()
		{
			DrawConfirmButton("Confirm");
		}

		public void DrawConfirmButton(string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title, Array.Empty<GUILayoutOption>()))
			{
				Confirm(UserResponse.Confirm);
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void DrawConfirmButton(UserResponse response)
		{
			DrawConfirmButton(response, "Confirm");
		}

		public void DrawConfirmButton(UserResponse response, string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title, Array.Empty<GUILayoutOption>()))
			{
				Confirm(response);
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void DrawCancelButton()
		{
			DrawCancelButton("Cancel");
		}

		public void DrawCancelButton(string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title, Array.Empty<GUILayoutOption>()))
			{
				Cancel();
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void Confirm()
		{
			Confirm(UserResponse.Confirm);
		}

		public void Confirm(UserResponse response)
		{
			resultCallback(currentActionId, response);
			Close();
		}

		public void Cancel()
		{
			resultCallback(currentActionId, UserResponse.Cancel);
			Close();
		}

		private void DrawWindow(int windowId)
		{
			windowProperties.windowDrawDelegate(windowProperties.title, windowProperties.message);
		}

		private void UpdateTimers()
		{
			if (_busyTimerRunning && busyTimer <= 0f)
			{
				_busyTimerRunning = false;
			}
		}

		private void StartBusyTimer(float time)
		{
			_busyTime = time + Time.realtimeSinceStartup;
			_busyTimerRunning = true;
		}

		private void Close()
		{
			Reset();
			StateChanged(0.1f);
		}

		private void StateChanged(float delay)
		{
			StartBusyTimer(delay);
		}

		private void Reset()
		{
			_enabled = false;
			_type = DialogType.None;
			currentActionId = -1;
			resultCallback = null;
		}

		private void ResetTimers()
		{
			_busyTimerRunning = false;
		}

		public void FullReset()
		{
			Reset();
			ResetTimers();
		}
	}

	private abstract class QueueEntry
	{
		public enum State
		{
			Waiting,
			Confirmed,
			Canceled
		}

		private static int uidCounter;

		public int id { get; protected set; }

		public QueueActionType queueActionType { get; protected set; }

		public State state { get; protected set; }

		public UserResponse response { get; protected set; }

		protected static int nextId
		{
			get
			{
				int result = uidCounter;
				uidCounter++;
				return result;
			}
		}

		public QueueEntry(QueueActionType queueActionType)
		{
			id = nextId;
			this.queueActionType = queueActionType;
		}

		public void Confirm(UserResponse response)
		{
			state = State.Confirmed;
			this.response = response;
		}

		public void Cancel()
		{
			state = State.Canceled;
		}
	}

	private class JoystickAssignmentChange : QueueEntry
	{
		public int playerId { get; private set; }

		public int joystickId { get; private set; }

		public bool assign { get; private set; }

		public JoystickAssignmentChange(int newPlayerId, int joystickId, bool assign)
			: base(QueueActionType.JoystickAssignment)
		{
			playerId = newPlayerId;
			this.joystickId = joystickId;
			this.assign = assign;
		}
	}

	private class ElementAssignmentChange : QueueEntry
	{
		public ElementAssignmentChangeType changeType { get; set; }

		public Context context { get; private set; }

		public ElementAssignmentChange(ElementAssignmentChangeType changeType, Context context)
			: base(QueueActionType.ElementAssignment)
		{
			this.changeType = changeType;
			this.context = context;
		}

		public ElementAssignmentChange(ElementAssignmentChange other)
			: this(other.changeType, other.context.Clone())
		{
		}
	}

	private class FallbackJoystickIdentification : QueueEntry
	{
		public int joystickId { get; private set; }

		public string joystickName { get; private set; }

		public FallbackJoystickIdentification(int joystickId, string joystickName)
			: base(QueueActionType.FallbackJoystickIdentification)
		{
			this.joystickId = joystickId;
			this.joystickName = joystickName;
		}
	}

	private class Calibration : QueueEntry
	{
		public int selectedElementIdentifierId;

		public bool recording;

		public Player player { get; private set; }

		public ControllerType controllerType
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public Joystick joystick { get; private set; }

		public CalibrationMap calibrationMap { get; private set; }

		public Calibration(Player player, Joystick joystick, CalibrationMap calibrationMap)
			: base(QueueActionType.Calibrate)
		{
			this.player = player;
			this.joystick = joystick;
			this.calibrationMap = calibrationMap;
			selectedElementIdentifierId = -1;
		}
	}

	private struct WindowProperties
	{
		public int windowId;

		public Rect rect;

		public Action<string, string> windowDrawDelegate;

		public string title;

		public string message;
	}

	private enum QueueActionType
	{
		None,
		JoystickAssignment,
		ElementAssignment,
		FallbackJoystickIdentification,
		Calibrate
	}

	private enum ElementAssignmentChangeType
	{
		Add,
		Replace,
		Remove,
		ReassignOrRemove,
		ConflictCheck
	}

	public enum UserResponse
	{
		Confirm,
		Cancel,
		Custom1,
		Custom2
	}

	private const float defaultModalWidth = 250f;

	private const float defaultModalHeight = 200f;

	private const float assignmentTimeout = 5f;

	private DialogHelper dialog;

	private InputMapper inputMapper = new InputMapper();

	private ConflictFoundEventData conflictFoundEventData;

	private bool guiState;

	private bool busy;

	private bool pageGUIState;

	private Player selectedPlayer;

	private int selectedMapCategoryId;

	private ControllerSelection selectedController;

	private ControllerMap selectedMap;

	private bool showMenu;

	private bool startListening;

	private Vector2 actionScrollPos;

	private Vector2 calibrateScrollPos;

	private Queue<QueueEntry> actionQueue;

	private bool setupFinished;

	[NonSerialized]
	private bool initialized;

	private bool isCompiling;

	private GUIStyle style_wordWrap;

	private GUIStyle style_centeredBox;

	private void Awake()
	{
		inputMapper.options.timeout = 5f;
		inputMapper.options.ignoreMouseXAxis = true;
		inputMapper.options.ignoreMouseYAxis = true;
		Initialize();
	}

	private void OnEnable()
	{
		Subscribe();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void Initialize()
	{
		dialog = new DialogHelper();
		actionQueue = new Queue<QueueEntry>();
		selectedController = new ControllerSelection();
		ReInput.ControllerConnectedEvent += JoystickConnected;
		ReInput.ControllerPreDisconnectEvent += JoystickPreDisconnect;
		ReInput.ControllerDisconnectedEvent += JoystickDisconnected;
		ResetAll();
		initialized = true;
		ReInput.userDataStore.Load();
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void Setup()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		if (!setupFinished)
		{
			style_wordWrap = new GUIStyle(GUI.skin.label);
			style_wordWrap.wordWrap = true;
			style_centeredBox = new GUIStyle(GUI.skin.box);
			style_centeredBox.alignment = (TextAnchor)4;
			setupFinished = true;
		}
	}

	private void Subscribe()
	{
		Unsubscribe();
		inputMapper.ConflictFoundEvent += OnConflictFound;
		inputMapper.StoppedEvent += OnStopped;
	}

	private void Unsubscribe()
	{
		inputMapper.RemoveAllEventListeners();
	}

	public void OnGUI()
	{
		if (initialized)
		{
			Setup();
			HandleMenuControl();
			if (!showMenu)
			{
				DrawInitialScreen();
				return;
			}
			SetGUIStateStart();
			ProcessQueue();
			DrawPage();
			ShowDialog();
			SetGUIStateEnd();
			busy = false;
		}
	}

	private void HandleMenuControl()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if (!dialog.enabled && (int)Event.current.type == 8 && ReInput.players.GetSystemPlayer().GetButtonDown("Menu"))
		{
			if (showMenu)
			{
				ReInput.userDataStore.Save();
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	private void Close()
	{
		ClearWorkingVars();
		showMenu = false;
	}

	private void Open()
	{
		showMenu = true;
	}

	private void DrawInitialScreen()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected Obj, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ActionElementMap firstElementMapWithAction = ReInput.players.GetSystemPlayer().controllers.maps.GetFirstElementMapWithAction("Menu", true);
		GUIContent val = ((firstElementMapWithAction == null) ? new GUIContent("There is no element assigned to open the menu!") : new GUIContent("Press " + firstElementMapWithAction.elementIdentifierName + " to open the menu."));
		GUILayout.BeginArea(GetScreenCenteredRect(300f, 50f));
		GUILayout.Box(val, style_centeredBox, new GUILayoutOption[2]
		{
			GUILayout.ExpandHeight(true),
			GUILayout.ExpandWidth(true)
		});
		GUILayout.EndArea();
	}

	private void DrawPage()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (GUI.enabled != pageGUIState)
		{
			GUI.enabled = pageGUIState;
		}
		GUILayout.BeginArea(new Rect(((float)Screen.width - (float)Screen.width * 0.9f) * 0.5f, ((float)Screen.height - (float)Screen.height * 0.9f) * 0.5f, (float)Screen.width * 0.9f, (float)Screen.height * 0.9f));
		DrawPlayerSelector();
		DrawJoystickSelector();
		DrawMouseAssignment();
		DrawControllerSelector();
		DrawCalibrateButton();
		DrawMapCategories();
		actionScrollPos = GUILayout.BeginScrollView(actionScrollPos, Array.Empty<GUILayoutOption>());
		DrawCategoryActions();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawPlayerSelector()
	{
		if (ReInput.players.allPlayerCount == 0)
		{
			GUILayout.Label("There are no players.", Array.Empty<GUILayoutOption>());
			return;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Players:", Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		foreach (Player player in ReInput.players.GetPlayers(true))
		{
			if (selectedPlayer == null)
			{
				selectedPlayer = player;
			}
			bool flag = ((player == selectedPlayer) ? true : false);
			bool flag2 = GUILayout.Toggle(flag, (player.descriptiveName != string.Empty) ? player.descriptiveName : player.name, GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (flag2 != flag && flag2)
			{
				selectedPlayer = player;
				selectedController.Clear();
				selectedMapCategoryId = -1;
			}
		}
		GUILayout.EndHorizontal();
	}

	private void DrawMouseAssignment()
	{
		bool enabled = GUI.enabled;
		if (selectedPlayer == null)
		{
			GUI.enabled = false;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Assign Mouse:", Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		bool flag = ((selectedPlayer != null && selectedPlayer.controllers.hasMouse) ? true : false);
		bool flag2 = GUILayout.Toggle(flag, "Assign Mouse", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		if (flag2 != flag)
		{
			if (flag2)
			{
				selectedPlayer.controllers.hasMouse = true;
				foreach (Player player in ReInput.players.Players)
				{
					if (player != selectedPlayer)
					{
						player.controllers.hasMouse = false;
					}
				}
			}
			else
			{
				selectedPlayer.controllers.hasMouse = false;
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawJoystickSelector()
	{
		bool enabled = GUI.enabled;
		if (selectedPlayer == null)
		{
			GUI.enabled = false;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Assign Joysticks:", Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		bool flag = ((selectedPlayer == null || selectedPlayer.controllers.joystickCount == 0) ? true : false);
		bool flag2 = GUILayout.Toggle(flag, "None", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		if (flag2 != flag)
		{
			selectedPlayer.controllers.ClearControllersOfType((ControllerType)2);
			ControllerSelectionChanged();
		}
		if (selectedPlayer != null)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				flag = selectedPlayer.controllers.ContainsController((Controller)(object)joystick);
				flag2 = GUILayout.Toggle(flag, ((Controller)joystick).name, GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
				if (flag2 != flag)
				{
					EnqueueAction(new JoystickAssignmentChange(selectedPlayer.id, ((Controller)joystick).id, flag2));
				}
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawControllerSelector()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Invalid comparison between Unknown and I4
		if (selectedPlayer == null)
		{
			return;
		}
		bool enabled = GUI.enabled;
		GUILayout.Space(15f);
		GUILayout.Label("Controller to Map:", Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (!selectedController.hasSelection)
		{
			selectedController.Set(0, (ControllerType)0);
			ControllerSelectionChanged();
		}
		bool flag = (int)selectedController.type == 0;
		if (GUILayout.Toggle(flag, "Keyboard", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }) != flag)
		{
			selectedController.Set(0, (ControllerType)0);
			ControllerSelectionChanged();
		}
		if (!selectedPlayer.controllers.hasMouse)
		{
			GUI.enabled = false;
		}
		flag = (int)selectedController.type == 1;
		if (GUILayout.Toggle(flag, "Mouse", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }) != flag)
		{
			selectedController.Set(0, (ControllerType)1);
			ControllerSelectionChanged();
		}
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
		foreach (Joystick joystick in selectedPlayer.controllers.Joysticks)
		{
			flag = (int)selectedController.type == 2 && selectedController.id == ((Controller)joystick).id;
			if (GUILayout.Toggle(flag, ((Controller)joystick).name, GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }) != flag)
			{
				selectedController.Set(((Controller)joystick).id, (ControllerType)2);
				ControllerSelectionChanged();
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawCalibrateButton()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		if (selectedPlayer == null)
		{
			return;
		}
		bool enabled = GUI.enabled;
		GUILayout.Space(10f);
		Controller val = (selectedController.hasSelection ? selectedPlayer.controllers.GetController(selectedController.type, selectedController.id) : null);
		if (val == null || (int)selectedController.type != 2)
		{
			GUI.enabled = false;
			GUILayout.Button("Select a controller to calibrate", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (GUI.enabled != enabled)
			{
				GUI.enabled = enabled;
			}
		}
		else if (GUILayout.Button("Calibrate " + val.name, new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
		{
			Joystick val2 = (Joystick)(object)((val is Joystick) ? val : null);
			if (val2 != null)
			{
				CalibrationMap calibrationMap = ((ControllerWithAxes)val2).calibrationMap;
				if (calibrationMap != null)
				{
					EnqueueAction(new Calibration(selectedPlayer, val2, calibrationMap));
				}
			}
		}
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawMapCategories()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		if (selectedPlayer == null || !selectedController.hasSelection)
		{
			return;
		}
		bool enabled = GUI.enabled;
		GUILayout.Space(15f);
		GUILayout.Label("Categories:", Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		foreach (InputMapCategory userAssignableMapCategory in ReInput.mapping.UserAssignableMapCategories)
		{
			if (!selectedPlayer.controllers.maps.ContainsMapInCategory(selectedController.type, ((InputCategory)userAssignableMapCategory).id))
			{
				GUI.enabled = false;
			}
			else if (selectedMapCategoryId < 0)
			{
				selectedMapCategoryId = ((InputCategory)userAssignableMapCategory).id;
				selectedMap = selectedPlayer.controllers.maps.GetFirstMapInCategory(selectedController.type, selectedController.id, ((InputCategory)userAssignableMapCategory).id);
			}
			bool flag = ((((InputCategory)userAssignableMapCategory).id == selectedMapCategoryId) ? true : false);
			if (GUILayout.Toggle(flag, (((InputCategory)userAssignableMapCategory).descriptiveName != string.Empty) ? ((InputCategory)userAssignableMapCategory).descriptiveName : ((InputCategory)userAssignableMapCategory).name, GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }) != flag)
			{
				selectedMapCategoryId = ((InputCategory)userAssignableMapCategory).id;
				selectedMap = selectedPlayer.controllers.maps.GetFirstMapInCategory(selectedController.type, selectedController.id, ((InputCategory)userAssignableMapCategory).id);
			}
			if (GUI.enabled != enabled)
			{
				GUI.enabled = enabled;
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawCategoryActions()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Invalid comparison between Unknown and I4
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Invalid comparison between Unknown and I4
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Invalid comparison between Unknown and I4
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Invalid comparison between Unknown and I4
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Invalid comparison between Unknown and I4
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Invalid comparison between Unknown and I4
		if (selectedPlayer == null || selectedMapCategoryId < 0)
		{
			return;
		}
		bool enabled = GUI.enabled;
		if (selectedMap == null)
		{
			return;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Actions:", Array.Empty<GUILayoutOption>());
		InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(selectedMapCategoryId);
		if (mapCategory == null)
		{
			return;
		}
		InputCategory actionCategory = ReInput.mapping.GetActionCategory(((InputCategory)mapCategory).name);
		if (actionCategory == null)
		{
			return;
		}
		float num = 150f;
		foreach (InputAction item in ReInput.mapping.ActionsInCategory(actionCategory.id))
		{
			string text = ((item.descriptiveName != string.Empty) ? item.descriptiveName : item.name);
			if ((int)item.type == 1)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(text, new GUILayoutOption[1] { GUILayout.Width(num) });
				DrawAddActionMapButton(selectedPlayer.id, item, (AxisRange)1, selectedController, selectedMap);
				foreach (ActionElementMap allMap in selectedMap.AllMaps)
				{
					if (allMap.actionId == item.id)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, (AxisRange)1, selectedController, selectedMap, allMap);
					}
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				if ((int)item.type != 0)
				{
					continue;
				}
				if ((int)selectedController.type != 0)
				{
					GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
					GUILayout.Label(text, new GUILayoutOption[1] { GUILayout.Width(num) });
					DrawAddActionMapButton(selectedPlayer.id, item, (AxisRange)0, selectedController, selectedMap);
					foreach (ActionElementMap allMap2 in selectedMap.AllMaps)
					{
						if (allMap2.actionId == item.id && (int)allMap2.elementType != 1 && (int)allMap2.axisType != 2)
						{
							DrawActionAssignmentButton(selectedPlayer.id, item, (AxisRange)0, selectedController, selectedMap, allMap2);
							DrawInvertButton(selectedPlayer.id, item, (Pole)0, selectedController, selectedMap, allMap2);
						}
					}
					GUILayout.EndHorizontal();
				}
				string text2 = ((item.positiveDescriptiveName != string.Empty) ? item.positiveDescriptiveName : (item.descriptiveName + " +"));
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(text2, new GUILayoutOption[1] { GUILayout.Width(num) });
				DrawAddActionMapButton(selectedPlayer.id, item, (AxisRange)1, selectedController, selectedMap);
				foreach (ActionElementMap allMap3 in selectedMap.AllMaps)
				{
					if (allMap3.actionId == item.id && (int)allMap3.axisContribution == 0 && (int)allMap3.axisType != 1)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, (AxisRange)1, selectedController, selectedMap, allMap3);
					}
				}
				GUILayout.EndHorizontal();
				string text3 = ((item.negativeDescriptiveName != string.Empty) ? item.negativeDescriptiveName : (item.descriptiveName + " -"));
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(text3, new GUILayoutOption[1] { GUILayout.Width(num) });
				DrawAddActionMapButton(selectedPlayer.id, item, (AxisRange)2, selectedController, selectedMap);
				foreach (ActionElementMap allMap4 in selectedMap.AllMaps)
				{
					if (allMap4.actionId == item.id && (int)allMap4.axisContribution == 1 && (int)allMap4.axisType != 1)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, (AxisRange)2, selectedController, selectedMap, allMap4);
					}
				}
				GUILayout.EndHorizontal();
			}
		}
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DrawActionAssignmentButton(int playerId, InputAction action, AxisRange actionRange, ControllerSelection controller, ControllerMap controllerMap, ActionElementMap elementMap)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected Obj, but got Unknown
		if (GUILayout.Button(elementMap.elementIdentifierName, new GUILayoutOption[2]
		{
			GUILayout.ExpandWidth(false),
			GUILayout.MinWidth(30f)
		}))
		{
			Context context = new Context
			{
				actionId = action.id,
				actionRange = actionRange,
				controllerMap = controllerMap,
				actionElementMapToReplace = elementMap
			};
			EnqueueAction(new ElementAssignmentChange(ElementAssignmentChangeType.ReassignOrRemove, context));
			startListening = true;
		}
		GUILayout.Space(4f);
	}

	private void DrawInvertButton(int playerId, InputAction action, Pole actionAxisContribution, ControllerSelection controller, ControllerMap controllerMap, ActionElementMap elementMap)
	{
		bool invert = elementMap.invert;
		bool flag = GUILayout.Toggle(invert, "Invert", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
		if (flag != invert)
		{
			elementMap.invert = flag;
		}
		GUILayout.Space(10f);
	}

	private void DrawAddActionMapButton(int playerId, InputAction action, AxisRange actionRange, ControllerSelection controller, ControllerMap controllerMap)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		if (GUILayout.Button("Add...", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
		{
			Context context = new Context
			{
				actionId = action.id,
				actionRange = actionRange,
				controllerMap = controllerMap
			};
			EnqueueAction(new ElementAssignmentChange(ElementAssignmentChangeType.Add, context));
			startListening = true;
		}
		GUILayout.Space(10f);
	}

	private void ShowDialog()
	{
		dialog.Update();
	}

	private void DrawModalWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			dialog.DrawConfirmButton("Okay");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawModalWindow_OkayOnly(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			dialog.DrawConfirmButton("Okay");
			GUILayout.EndHorizontal();
		}
	}

	private void DrawElementAssignmentWindow(string title, string message)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!dialog.enabled)
		{
			return;
		}
		GUILayout.Space(5f);
		GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
		GUILayout.FlexibleSpace();
		if (!(actionQueue.Peek() is ElementAssignmentChange elementAssignmentChange))
		{
			dialog.Cancel();
			return;
		}
		float num;
		if (!dialog.busy)
		{
			if (startListening && (int)inputMapper.status == 0)
			{
				inputMapper.Start(elementAssignmentChange.context);
				startListening = false;
			}
			if (conflictFoundEventData != null)
			{
				dialog.Confirm();
				return;
			}
			num = inputMapper.timeRemaining;
			if (num == 0f)
			{
				dialog.Cancel();
				return;
			}
		}
		else
		{
			num = inputMapper.options.timeout;
		}
		GUILayout.Label("Assignment will be canceled in " + (int)Mathf.Ceil(num) + "...", style_wordWrap, Array.Empty<GUILayoutOption>());
	}

	private void DrawElementAssignmentProtectedConflictWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (!(actionQueue.Peek() is ElementAssignmentChange))
			{
				dialog.Cancel();
				return;
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			dialog.DrawConfirmButton(UserResponse.Custom1, "Add");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawElementAssignmentNormalConflictWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (!(actionQueue.Peek() is ElementAssignmentChange))
			{
				dialog.Cancel();
				return;
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			dialog.DrawConfirmButton(UserResponse.Confirm, "Replace");
			GUILayout.FlexibleSpace();
			dialog.DrawConfirmButton(UserResponse.Custom1, "Add");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawReassignOrRemoveElementAssignmentWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			dialog.DrawConfirmButton("Reassign");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton("Remove");
			GUILayout.EndHorizontal();
		}
	}

	private void DrawFallbackJoystickIdentificationWindow(string title, string message)
	{
		if (!dialog.enabled)
		{
			return;
		}
		if (!(actionQueue.Peek() is FallbackJoystickIdentification fallbackJoystickIdentification))
		{
			dialog.Cancel();
			return;
		}
		GUILayout.Space(5f);
		GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
		GUILayout.Label("Press any button or axis on \"" + fallbackJoystickIdentification.joystickName + "\" now.", style_wordWrap, Array.Empty<GUILayoutOption>());
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Skip", Array.Empty<GUILayoutOption>()))
		{
			dialog.Cancel();
		}
		else if (!dialog.busy && ReInput.controllers.SetUnityJoystickIdFromAnyButtonOrAxisPress(fallbackJoystickIdentification.joystickId, 0.8f, false))
		{
			dialog.Confirm();
		}
	}

	private void DrawCalibrationWindow(string title, string message)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		if (!dialog.enabled)
		{
			return;
		}
		if (!(actionQueue.Peek() is Calibration calibration))
		{
			dialog.Cancel();
			return;
		}
		GUILayout.Space(5f);
		GUILayout.Label(message, style_wordWrap, Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		bool enabled = GUI.enabled;
		GUILayout.BeginVertical(new GUILayoutOption[1] { GUILayout.Width(200f) });
		calibrateScrollPos = GUILayout.BeginScrollView(calibrateScrollPos, Array.Empty<GUILayoutOption>());
		if (calibration.recording)
		{
			GUI.enabled = false;
		}
		IList<ControllerElementIdentifier> axisElementIdentifiers = ((ControllerWithAxes)calibration.joystick).AxisElementIdentifiers;
		for (int i = 0; i < axisElementIdentifiers.Count; i++)
		{
			ControllerElementIdentifier val = axisElementIdentifiers[i];
			bool flag = calibration.selectedElementIdentifierId == val.id;
			bool flag2 = GUILayout.Toggle(flag, val.name, GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (flag != flag2)
			{
				calibration.selectedElementIdentifierId = val.id;
			}
		}
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (calibration.selectedElementIdentifierId >= 0)
		{
			float axisRawById = ((ControllerWithAxes)calibration.joystick).GetAxisRawById(calibration.selectedElementIdentifierId);
			GUILayout.Label("Raw Value: " + axisRawById, Array.Empty<GUILayoutOption>());
			int axisIndexById = ((ControllerWithAxes)calibration.joystick).GetAxisIndexById(calibration.selectedElementIdentifierId);
			AxisCalibration axis = calibration.calibrationMap.GetAxis(axisIndexById);
			GUILayout.Label("Calibrated Value: " + ((ControllerWithAxes)calibration.joystick).GetAxisById(calibration.selectedElementIdentifierId), Array.Empty<GUILayoutOption>());
			GUILayout.Label("Zero: " + axis.calibratedZero, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Min: " + axis.calibratedMin, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Max: " + axis.calibratedMax, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Dead Zone: " + axis.deadZone, Array.Empty<GUILayoutOption>());
			GUILayout.Space(15f);
			bool flag3 = GUILayout.Toggle(axis.enabled, "Enabled", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (axis.enabled != flag3)
			{
				axis.enabled = flag3;
			}
			GUILayout.Space(10f);
			bool flag4 = GUILayout.Toggle(calibration.recording, "Record Min/Max", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (flag4 != calibration.recording)
			{
				if (flag4)
				{
					axis.calibratedMax = 0f;
					axis.calibratedMin = 0f;
				}
				calibration.recording = flag4;
			}
			if (calibration.recording)
			{
				axis.calibratedMin = Mathf.Min(new float[3] { axis.calibratedMin, axisRawById, axis.calibratedMin });
				axis.calibratedMax = Mathf.Max(new float[3] { axis.calibratedMax, axisRawById, axis.calibratedMax });
				GUI.enabled = false;
			}
			if (GUILayout.Button("Set Zero", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
			{
				axis.calibratedZero = axisRawById;
			}
			if (GUILayout.Button("Set Dead Zone", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
			{
				axis.deadZone = axisRawById;
			}
			bool flag5 = GUILayout.Toggle(axis.invert, "Invert", GUIStyle.op_Implicit("Button"), new GUILayoutOption[1] { GUILayout.ExpandWidth(false) });
			if (axis.invert != flag5)
			{
				axis.invert = flag5;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("Reset", new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }))
			{
				axis.Reset();
			}
			if (GUI.enabled != enabled)
			{
				GUI.enabled = enabled;
			}
		}
		else
		{
			GUILayout.Label("Select an axis to begin.", Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		if (calibration.recording)
		{
			GUI.enabled = false;
		}
		if (GUILayout.Button("Close", Array.Empty<GUILayoutOption>()))
		{
			calibrateScrollPos = default;
			dialog.Confirm();
		}
		if (GUI.enabled != enabled)
		{
			GUI.enabled = enabled;
		}
	}

	private void DialogResultCallback(int queueActionId, UserResponse response)
	{
		foreach (QueueEntry item in actionQueue)
		{
			if (item.id == queueActionId)
			{
				if (response != UserResponse.Cancel)
				{
					item.Confirm(response);
				}
				else
				{
					item.Cancel();
				}
				break;
			}
		}
	}

	private Rect GetScreenCenteredRect(float width, float height)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return new Rect((float)Screen.width * 0.5f - width * 0.5f, (float)((double)Screen.height * 0.5 - (double)(height * 0.5f)), width, height);
	}

	private void EnqueueAction(QueueEntry entry)
	{
		if (entry != null)
		{
			busy = true;
			GUI.enabled = false;
			actionQueue.Enqueue(entry);
		}
	}

	private void ProcessQueue()
	{
		if (dialog.enabled || busy || actionQueue.Count == 0)
		{
			return;
		}
		while (actionQueue.Count > 0)
		{
			QueueEntry queueEntry = actionQueue.Peek();
			bool flag = false;
			switch (queueEntry.queueActionType)
			{
			case QueueActionType.JoystickAssignment:
				flag = ProcessJoystickAssignmentChange((JoystickAssignmentChange)queueEntry);
				break;
			case QueueActionType.ElementAssignment:
				flag = ProcessElementAssignmentChange((ElementAssignmentChange)queueEntry);
				break;
			case QueueActionType.FallbackJoystickIdentification:
				flag = ProcessFallbackJoystickIdentification((FallbackJoystickIdentification)queueEntry);
				break;
			case QueueActionType.Calibrate:
				flag = ProcessCalibration((Calibration)queueEntry);
				break;
			}
			if (flag)
			{
				actionQueue.Dequeue();
				continue;
			}
			break;
		}
	}

	private bool ProcessJoystickAssignmentChange(JoystickAssignmentChange entry)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			return true;
		}
		if (!entry.assign)
		{
			player.controllers.RemoveController((ControllerType)2, entry.joystickId);
			ControllerSelectionChanged();
			return true;
		}
		if (player.controllers.ContainsController((ControllerType)2, entry.joystickId))
		{
			return true;
		}
		if (!ReInput.controllers.IsJoystickAssigned(entry.joystickId) || entry.state == QueueEntry.State.Confirmed)
		{
			player.controllers.AddController((ControllerType)2, entry.joystickId, true);
			ControllerSelectionChanged();
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Joystick Reassignment",
			message = "This joystick is already assigned to another player. Do you want to reassign this joystick to " + player.descriptiveName + "?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawModalWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessElementAssignmentChange(ElementAssignmentChange entry)
	{
		switch (entry.changeType)
		{
		case ElementAssignmentChangeType.ReassignOrRemove:
			return ProcessRemoveOrReassignElementAssignment(entry);
		case ElementAssignmentChangeType.Remove:
			return ProcessRemoveElementAssignment(entry);
		case ElementAssignmentChangeType.Add:
		case ElementAssignmentChangeType.Replace:
			return ProcessAddOrReplaceElementAssignment(entry);
		case ElementAssignmentChangeType.ConflictCheck:
			return ProcessElementAssignmentConflictCheck(entry);
		default:
			throw new NotImplementedException();
		}
	}

	private bool ProcessRemoveOrReassignElementAssignment(ElementAssignmentChange entry)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if (entry.context.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			ElementAssignmentChange elementAssignmentChange = new ElementAssignmentChange(entry);
			elementAssignmentChange.changeType = ElementAssignmentChangeType.Remove;
			actionQueue.Enqueue(elementAssignmentChange);
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			ElementAssignmentChange elementAssignmentChange2 = new ElementAssignmentChange(entry);
			elementAssignmentChange2.changeType = ElementAssignmentChangeType.Replace;
			actionQueue.Enqueue(elementAssignmentChange2);
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
		{
			title = "Reassign or Remove",
			message = "Do you want to reassign or remove this assignment?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawReassignOrRemoveElementAssignmentWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessRemoveElementAssignment(ElementAssignmentChange entry)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (entry.context.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			entry.context.controllerMap.DeleteElementMap(entry.context.actionElementMapToReplace.id);
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.DeleteAssignmentConfirmation, new WindowProperties
		{
			title = "Remove Assignment",
			message = "Are you sure you want to remove this assignment?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawModalWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessAddOrReplaceElementAssignment(ElementAssignmentChange entry)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (entry.state == QueueEntry.State.Canceled)
		{
			inputMapper.Stop();
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			if ((int)Event.current.type != 8)
			{
				return false;
			}
			if (conflictFoundEventData != null)
			{
				ElementAssignmentChange elementAssignmentChange = new ElementAssignmentChange(entry);
				elementAssignmentChange.changeType = ElementAssignmentChangeType.ConflictCheck;
				actionQueue.Enqueue(elementAssignmentChange);
			}
			return true;
		}
		string text;
		if ((int)entry.context.controllerMap.controllerType != 0)
		{
			text = (((int)entry.context.controllerMap.controllerType != 1) ? "Press any button or axis to assign it to this action." : "Press any mouse button or axis to assign it to this action.\n\nTo assign mouse movement axes, move the mouse quickly in the direction you want mapped to the action. Slow movements will be ignored.");
		}
		else
		{
			text = (((int)Application.platform != 0 && (int)Application.platform != 1) ? "Press any key to assign it to this action. You may also use the modifier keys Control, Alt, and Shift. If you wish to assign a modifier key itself to this action, press and hold the key for 1 second." : "Press any key to assign it to this action. You may also use the modifier keys Command, Control, Alt, and Shift. If you wish to assign a modifier key itself to this action, press and hold the key for 1 second.");
			if (Application.isEditor)
			{
				text += "\n\nNOTE: Some modifier key combinations will not work in the Unity Editor, but they will work in a game build.";
			}
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
		{
			title = "Assign",
			message = text,
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawElementAssignmentWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessElementAssignmentConflictCheck(ElementAssignmentChange entry)
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (entry.context.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			inputMapper.Stop();
			return true;
		}
		if (conflictFoundEventData == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			if (entry.response == UserResponse.Confirm)
			{
				conflictFoundEventData.responseCallback((ConflictResponse)1);
			}
			else
			{
				if (entry.response != UserResponse.Custom1)
				{
					throw new NotImplementedException();
				}
				conflictFoundEventData.responseCallback((ConflictResponse)2);
			}
			return true;
		}
		if (conflictFoundEventData.isProtected)
		{
			string message = conflictFoundEventData.assignment.elementDisplayName + " is already in use and is protected from reassignment. You cannot remove the protected assignment, but you can still assign the action to this element. If you do so, the element will trigger multiple actions when activated.";
			dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
			{
				title = "Assignment Conflict",
				message = message,
				rect = GetScreenCenteredRect(250f, 200f),
				windowDrawDelegate = DrawElementAssignmentProtectedConflictWindow
			}, DialogResultCallback);
		}
		else
		{
			string message2 = conflictFoundEventData.assignment.elementDisplayName + " is already in use. You may replace the other conflicting assignments, add this assignment anyway which will leave multiple actions assigned to this element, or cancel this assignment.";
			dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
			{
				title = "Assignment Conflict",
				message = message2,
				rect = GetScreenCenteredRect(250f, 200f),
				windowDrawDelegate = DrawElementAssignmentNormalConflictWindow
			}, DialogResultCallback);
		}
		return false;
	}

	private bool ProcessFallbackJoystickIdentification(FallbackJoystickIdentification entry)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Joystick Identification Required",
			message = "A joystick has been attached or removed. You will need to identify each joystick by pressing a button on the controller listed below:",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawFallbackJoystickIdentificationWindow
		}, DialogResultCallback, 1f);
		return false;
	}

	private bool ProcessCalibration(Calibration entry)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Calibrate Controller",
			message = "Select an axis to calibrate on the " + ((Controller)entry.joystick).name + ".",
			rect = GetScreenCenteredRect(450f, 480f),
			windowDrawDelegate = DrawCalibrationWindow
		}, DialogResultCallback);
		return false;
	}

	private void PlayerSelectionChanged()
	{
		ClearControllerSelection();
	}

	private void ControllerSelectionChanged()
	{
		ClearMapSelection();
	}

	private void ClearControllerSelection()
	{
		selectedController.Clear();
		ClearMapSelection();
	}

	private void ClearMapSelection()
	{
		selectedMapCategoryId = -1;
		selectedMap = null;
	}

	private void ResetAll()
	{
		ClearWorkingVars();
		initialized = false;
		showMenu = false;
	}

	private void ClearWorkingVars()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		selectedPlayer = null;
		ClearMapSelection();
		selectedController.Clear();
		actionScrollPos = default;
		dialog.FullReset();
		actionQueue.Clear();
		busy = false;
		startListening = false;
		conflictFoundEventData = null;
		inputMapper.Stop();
	}

	private void SetGUIStateStart()
	{
		guiState = true;
		if (busy)
		{
			guiState = false;
		}
		pageGUIState = guiState && !busy && !dialog.enabled && !dialog.busy;
		if (GUI.enabled != guiState)
		{
			GUI.enabled = guiState;
		}
	}

	private void SetGUIStateEnd()
	{
		guiState = true;
		if (!GUI.enabled)
		{
			GUI.enabled = guiState;
		}
	}

	private void JoystickConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (ReInput.controllers.IsControllerAssigned(args.controllerType, args.controllerId))
		{
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (allPlayer.controllers.ContainsController(args.controllerType, args.controllerId))
				{
					ReInput.userDataStore.LoadControllerData(allPlayer.id, args.controllerType, args.controllerId);
				}
			}
		}
		else
		{
			ReInput.userDataStore.LoadControllerData(args.controllerType, args.controllerId);
		}
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void JoystickPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (selectedController.hasSelection && args.controllerType == selectedController.type && args.controllerId == selectedController.id)
		{
			ClearControllerSelection();
		}
		if (!showMenu)
		{
			return;
		}
		if (ReInput.controllers.IsControllerAssigned(args.controllerType, args.controllerId))
		{
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (allPlayer.controllers.ContainsController(args.controllerType, args.controllerId))
				{
					ReInput.userDataStore.SaveControllerData(allPlayer.id, args.controllerType, args.controllerId);
				}
			}
			return;
		}
		ReInput.userDataStore.SaveControllerData(args.controllerType, args.controllerId);
	}

	private void JoystickDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (showMenu)
		{
			ClearWorkingVars();
		}
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void OnConflictFound(ConflictFoundEventData data)
	{
		conflictFoundEventData = data;
	}

	private void OnStopped(StoppedEventData data)
	{
		conflictFoundEventData = null;
	}

	public void IdentifyAllJoysticks()
	{
		if (ReInput.controllers.joystickCount == 0)
		{
			return;
		}
		ClearWorkingVars();
		Open();
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			actionQueue.Enqueue(new FallbackJoystickIdentification(((Controller)joystick).id, ((Controller)joystick).name));
		}
	}

	protected void CheckRecompile()
	{
	}

	private void RecompileWindow(int windowId)
	{
	}

	public ControlRemappingDemo1()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
