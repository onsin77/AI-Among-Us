using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class ControllerManager : MonoBehaviour
{
	public static ControllerManager Instance;

	[HideInInspector]
	[SerializeField]
	private int playerId;

	[SerializeField]
	private Player player;

	private const string DISCONNECT_POPUP_MENUNAME = "DisconnectPopup_Real";

	private const float DELTA_SINCE_LAST_BUTTON_HIGHLIGHT_THRESHOLD = 0.25f;

	private const float DELTA_SINCE_LAST_SLIDER_MOVEMENT_THRESHOLD = 0.1f;

	private bool inputDetectedLastFrame;

	[SerializeField]
	private GameObject PrefabMouseCursorForConsole;

	private GameObject ConsoleMouse;

	[HideInInspector]
	public bool IsButtonHeld;

	public ControllerUiElementsState CurrentUiState;

	public List<ControllerUiElementsState> CurrentUiStateStack;

	private float deltaSinceLastUiHighlight;

	private bool disabledVirtualCursor;

	private int frameDelaySinceInputChanged;

	private Dictionary<Vector2, AxisDirection> directions = new Dictionary<Vector2, AxisDirection>
	{
		{
			Vector2.up,
			AxisDirection.Up
		},
		{
			Vector2.down,
			AxisDirection.Down
		},
		{
			Vector2.left,
			AxisDirection.Left
		},
		{
			Vector2.right,
			AxisDirection.Right
		}
	};

	private readonly Logger logger = new Logger("ControllerManager", Logger.Level.Error, Logger.Category.UI);

	private string currentScene;

	public bool IsUiControllerActive
	{
		get
		{
			if ((Object)(object)CurrentUiState.BackButton != (Object)null || (Object)(object)CurrentUiState.CurrentSelection != (Object)null || !string.IsNullOrEmpty(CurrentUiState.MenuName))
			{
				return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)Instance))
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (!Object.op_Implicit((Object)(object)Instance))
		{
			Instance = this;
		}
		SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(OnInputTypeChanged));
	}

	private void SceneManager_activeSceneChanged(Scene from, Scene to)
	{
		currentScene = to.name;
	}

	private void SceneManager_sceneUnloaded(Scene scene)
	{
		if (currentScene == scene.name)
		{
			CurrentUiStateStack.Clear();
			ResetAll();
		}
	}

	private void Start()
	{
		if ((Object)(object)VirtualCursor.instance != (Object)null)
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		player = ReInput.players.GetPlayer(playerId);
		ConsoleMouse = Object.Instantiate<GameObject>(PrefabMouseCursorForConsole);
		if ((Object)(object)ConsoleMouse != (Object)null)
		{
			ConsoleMouse.gameObject.SetActive(true);
			Object.DontDestroyOnLoad((Object)(object)ConsoleMouse);
		}
	}

	public bool IsMenuActiveAtAll(string menuName)
	{
		if (!IsUiControllerActive)
		{
			return false;
		}
		if (CurrentUiState.MenuName == menuName)
		{
			return true;
		}
		foreach (ControllerUiElementsState item in CurrentUiStateStack)
		{
			if (item.MenuName == menuName)
			{
				return true;
			}
		}
		return false;
	}

	private void OnInputTypeChanged()
	{
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			if (Object.op_Implicit((Object)(object)CurrentUiState.CurrentSelection) && ((Behaviour)CurrentUiState.CurrentSelection).isActiveAndEnabled)
			{
				CurrentUiState.CurrentSelection.ReceiveMouseOver();
			}
		}
		else if (Object.op_Implicit((Object)(object)CurrentUiState.CurrentSelection) && ((Behaviour)CurrentUiState.CurrentSelection).isActiveAndEnabled)
		{
			foreach (ControllerUiElementsState item in CurrentUiStateStack)
			{
				if (Object.op_Implicit((Object)(object)item.CurrentSelection))
				{
					item.CurrentSelection.ReceiveMouseOut();
				}
			}
			CurrentUiState.CurrentSelection.ReceiveMouseOut();
		}
		frameDelaySinceInputChanged = 3;
	}

	public void NewScene(string menuName, UiElement backButton, UiElement defaultSelection, List<UiElement> selectableElements, bool gridNav = false)
	{
		GameObject val = GameObject.Find("DisconnectPopup_Real");
		DisconnectPopup disconnectPopup = null;
		if ((Object)(object)val != (Object)null)
		{
			DisconnectPopup component = val.GetComponent<DisconnectPopup>();
			if ((Object)(object)component != (Object)null && ((Behaviour)component).isActiveAndEnabled)
			{
				disconnectPopup = component;
				logger.Warning("DisconnectPopup_Real menu found, restore after new scene.");
			}
		}
		ControllerUiElementsState controllerUiElementsState = new ControllerUiElementsState();
		controllerUiElementsState.EnforceGridNavigation = gridNav;
		controllerUiElementsState.BackButton = backButton;
		controllerUiElementsState.MenuName = menuName;
		controllerUiElementsState.IsScene = true;
		CurrentUiStateStack.Add(controllerUiElementsState);
		SetUpSelectables(controllerUiElementsState, defaultSelection, selectableElements);
		OpenTopmostMenu();
		if (CurrentUiState == controllerUiElementsState)
		{
			SetDefaultSelection(defaultSelection, selectableElements);
		}
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance))
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		if ((Object)(object)disconnectPopup != (Object)null && ((Behaviour)disconnectPopup).isActiveAndEnabled)
		{
			disconnectPopup.Show();
		}
	}

	public void OpenOverlayMenu(string menuName, UiElement backButton)
	{
		OpenOverlayMenu(menuName, backButton, null, new List<UiElement>());
	}

	public void OpenOverlayMenu(string menuName, UiElement backButton, UiElement defaultSelection)
	{
		OpenOverlayMenu(menuName, backButton, defaultSelection, new List<UiElement>());
	}

	public void OpenOverlayMenu(string menuName, UiElement backButton, UiElement defaultSelection, List<UiElement> selectableElements, bool gridNav = false)
	{
		string text = (Object.op_Implicit((Object)(object)backButton) ? ((Object)backButton).name : "null");
		string text2 = (Object.op_Implicit((Object)(object)defaultSelection) ? ((Object)defaultSelection).name : "null");
		string text3 = ((selectableElements != null) ? selectableElements.Count.ToString() : "null");
		logger.Debug($"Menu: {menuName} => ControllerManager.OpenOverlayMenu(backButton={text}, defaultSelection={text2}, selectableElements.Count={text3}, gridNav={gridNav})");
		foreach (ControllerUiElementsState item in CurrentUiStateStack)
		{
			if (item.MenuName == menuName)
			{
				logger.Warning("Attempted to open an already-open menu");
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)DestroyableSingleton<EOSManager>.Instance) && menuName != ((Object)DestroyableSingleton<EOSManager>.Instance.TimeOutPopup).name)
		{
			DestroyableSingleton<EOSManager>.Instance.HideTimeout();
		}
		ControllerUiElementsState controllerUiElementsState = new ControllerUiElementsState();
		controllerUiElementsState.EnforceGridNavigation = gridNav;
		controllerUiElementsState.BackButton = backButton;
		controllerUiElementsState.MenuName = menuName;
		CurrentUiStateStack.Add(controllerUiElementsState);
		SetUpSelectables(controllerUiElementsState, defaultSelection, selectableElements);
		OpenTopmostMenu();
		if (CurrentUiState == controllerUiElementsState)
		{
			SetDefaultSelection(defaultSelection, selectableElements);
		}
	}

	public ControllerUiElementsState GetMenu(string menuName)
	{
		for (int i = 0; i < CurrentUiStateStack.Count; i++)
		{
			if (CurrentUiStateStack[i].MenuName == menuName)
			{
				return CurrentUiStateStack[i];
			}
		}
		logger.Error("OpenSpecificMenu - couldn't find specific menu:" + menuName);
		return null;
	}

	public void OpenSpecificMenu(string menuNameToOpen)
	{
		for (int i = 0; i < CurrentUiStateStack.Count; i++)
		{
			if (CurrentUiStateStack[i].MenuName == menuNameToOpen)
			{
				CurrentUiState = CurrentUiStateStack[i];
				HighlightSelection(CurrentUiState.CurrentSelection);
				return;
			}
		}
		logger.Warning("OpenSpecificMenu - couldn't find specific menu:" + menuNameToOpen);
	}

	private void OpenTopmostMenu()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		ControllerUiElementsState currentUiState = CurrentUiState;
		CurrentUiState = null;
		float num = 0f;
		if (CurrentUiStateStack.Count > 0)
		{
			CurrentUiState = CurrentUiStateStack[0];
			UiElement uiElement = CurrentUiState.CurrentSelection;
			if (!Object.op_Implicit((Object)(object)uiElement) && CurrentUiState.SelectableUiElements.Count > 0)
			{
				foreach (UiElement selectableUiElement in CurrentUiState.SelectableUiElements)
				{
					uiElement = selectableUiElement;
					if (Object.op_Implicit((Object)(object)uiElement) && ((Behaviour)uiElement).isActiveAndEnabled)
					{
						break;
					}
				}
			}
			if (!Object.op_Implicit((Object)(object)uiElement))
			{
				uiElement = CurrentUiState.BackButton;
			}
			if (!Object.op_Implicit((Object)(object)uiElement))
			{
				logger.Warning($"Error: No button/back button for menu {CurrentUiState.MenuName}, {uiElement}");
				return;
			}
			num = ((Component)uiElement).transform.position.z;
			CurrentUiState.zPos = num;
		}
		for (int i = 1; i < CurrentUiStateStack.Count; i++)
		{
			ControllerUiElementsState controllerUiElementsState = CurrentUiStateStack[i];
			UiElement uiElement2 = controllerUiElementsState.CurrentSelection;
			if (!Object.op_Implicit((Object)(object)uiElement2) && controllerUiElementsState.SelectableUiElements.Count > 0)
			{
				foreach (UiElement selectableUiElement2 in controllerUiElementsState.SelectableUiElements)
				{
					uiElement2 = selectableUiElement2;
					if (Object.op_Implicit((Object)(object)uiElement2) && ((Behaviour)uiElement2).isActiveAndEnabled)
					{
						break;
					}
				}
			}
			if (!Object.op_Implicit((Object)(object)uiElement2))
			{
				uiElement2 = controllerUiElementsState.BackButton;
			}
			if (!Object.op_Implicit((Object)(object)uiElement2))
			{
				logger.Error("Error: No button/back button for menu " + controllerUiElementsState.MenuName);
				continue;
			}
			float num2 = (controllerUiElementsState.zPos = ((Component)uiElement2).transform.position.z);
			if (num2 <= num)
			{
				num = num2;
				CurrentUiState = controllerUiElementsState;
			}
		}
		if (CurrentUiState != null)
		{
			if (CurrentUiState != currentUiState)
			{
				HighlightSelection(CurrentUiState.CurrentSelection);
			}
			logger.Debug("Set current state to topmost menu " + CurrentUiState.MenuName);
		}
		else
		{
			CurrentUiState = currentUiState;
			ResetAll();
			logger.Info("There were no menu states to open");
		}
	}

	public void SetUpSelectables(ControllerUiElementsState state, UiElement defaultSelection, List<UiElement> selectables)
	{
		if (selectables != null)
		{
			if (selectables.Count == 0)
			{
				state.SelectableUiElements.Clear();
			}
			else
			{
				state.SelectableUiElements.AddRange(selectables);
			}
		}
		if (!((Object)(object)defaultSelection == (Object)null))
		{
			if (!state.SelectableUiElements.Contains(defaultSelection))
			{
				state.SelectableUiElements.Add(defaultSelection);
			}
			state.CurrentSelection = defaultSelection;
		}
	}

	public void SetDefaultSelection(UiElement defaultSelection, List<UiElement> selectableElements = null)
	{
		if ((Object)(object)defaultSelection != (Object)null)
		{
			HighlightSelection(defaultSelection);
		}
	}

	public void SetBackButton(UiElement backButton)
	{
		if ((Object)(object)backButton != (Object)null)
		{
			CurrentUiState.BackButton = backButton;
		}
	}

	public void UnsetBackButton(UiElement backButton)
	{
		if ((Object)(object)CurrentUiState.BackButton == (Object)(object)backButton)
		{
			CurrentUiState.BackButton = null;
		}
	}

	public void AddSelectableUiElement(UiElement uiElement, bool forceSelect = false)
	{
		if (!((Object)(object)uiElement == (Object)null))
		{
			if (!CurrentUiState.SelectableUiElements.Contains(uiElement))
			{
				CurrentUiState.SelectableUiElements.Add(uiElement);
			}
			if (forceSelect)
			{
				HighlightSelection(uiElement);
			}
		}
	}

	public void RemoveSelectableUiElement(UiElement uiElement)
	{
		if (!((Object)(object)uiElement == (Object)null))
		{
			CurrentUiState.SelectableUiElements.Remove(uiElement);
			if ((Object)(object)CurrentUiState.CurrentSelection == (Object)(object)uiElement)
			{
				CurrentUiState.CurrentSelection = null;
			}
		}
	}

	public void AddSelectableUiElement(UiElement uiElement, ControllerUiElementsState menu)
	{
		if (!((Object)(object)uiElement == (Object)null) && !menu.SelectableUiElements.Contains(uiElement))
		{
			menu.SelectableUiElements.Add(uiElement);
		}
	}

	public void RemoveSelectableUiElement(UiElement uiElement, ControllerUiElementsState menu)
	{
		if (!((Object)(object)uiElement == (Object)null))
		{
			menu.SelectableUiElements.Remove(uiElement);
			if ((Object)(object)menu.CurrentSelection == (Object)(object)uiElement)
			{
				menu.CurrentSelection = null;
			}
		}
	}

	public void ClearDestroyedSelectableUiElements()
	{
		if (CurrentUiState != null && CurrentUiState.SelectableUiElements != null && CurrentUiState.SelectableUiElements.Count != 0)
		{
			CurrentUiState.SelectableUiElements.RemoveAll((UiElement item) => (Object)(object)item == (Object)null);
		}
	}

	public void ClearDestroyedSelectableUiElements(ControllerUiElementsState state)
	{
		if (state != null && state.SelectableUiElements != null && state.SelectableUiElements.Count != 0)
		{
			state.SelectableUiElements.RemoveAll((UiElement item) => (Object)(object)item == (Object)null);
		}
	}

	public void CloseOverlayMenu(string menuName)
	{
		logger.Info("Menu: " + menuName + " => CloseOverlayMenu()");
		bool flag = false;
		if ((Object)(object)CurrentUiState.CurrentSelection != (Object)null && CurrentUiState.MenuName == menuName)
		{
			CurrentUiState.CurrentSelection.ReceiveMouseOut();
		}
		if (CurrentUiStateStack.Count > 0)
		{
			if (string.Equals(CurrentUiState.MenuName, menuName, StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				CurrentUiStateStack.Remove(CurrentUiState);
				OpenTopmostMenu();
				HighlightSelection(CurrentUiState.CurrentSelection);
			}
			else
			{
				for (int i = 0; i < CurrentUiStateStack.Count; i++)
				{
					if (string.Equals(CurrentUiStateStack[i].MenuName, menuName, StringComparison.OrdinalIgnoreCase))
					{
						CurrentUiStateStack.RemoveAt(i);
						logger.Debug("Closed currently-inactive menu " + menuName);
						break;
					}
				}
			}
		}
		else if (IsUiControllerActive && string.Equals(CurrentUiState.MenuName, menuName, StringComparison.OrdinalIgnoreCase))
		{
			flag = true;
			ResetAll();
		}
		if (flag)
		{
			return;
		}
		List<ControllerUiElementsState> list = new List<ControllerUiElementsState>();
		foreach (ControllerUiElementsState item in CurrentUiStateStack)
		{
			if (string.Equals(item.MenuName, menuName, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			foreach (ControllerUiElementsState item2 in list)
			{
				logger.Debug("Found menu: " + item2.MenuName + " in lower stack, removing.");
				CurrentUiStateStack.Remove(item2);
			}
			return;
		}
		logger.Warning("Attempting to close: " + menuName + ", but menu was never opened.");
	}

	public void CloseAndResetAll()
	{
		foreach (ControllerUiElementsState item in new List<ControllerUiElementsState>(CurrentUiStateStack))
		{
			PassiveUiElement passiveUiElement = item.BackButton as PassiveUiElement;
			if ((Object)(object)passiveUiElement != (Object)null)
			{
				passiveUiElement.ReceiveClickDown();
			}
		}
		ResetAll();
	}

	public void ResetAll()
	{
		logger.Info("ResetAll()");
		if ((Object)(object)CurrentUiState.CurrentSelection != (Object)null)
		{
			CurrentUiState.CurrentSelection.ReceiveMouseOut();
		}
		CurrentUiState.Reset();
		CurrentUiStateStack.Clear();
		deltaSinceLastUiHighlight = 0f;
	}

	private void Update()
	{
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		if (frameDelaySinceInputChanged > 0)
		{
			frameDelaySinceInputChanged--;
		}
		else
		{
			if (!IsUiControllerActive || RadialMenu.instances != 0)
			{
				return;
			}
			if (!Object.op_Implicit((Object)(object)CurrentUiState.CurrentSelection) || !((Behaviour)CurrentUiState.CurrentSelection).isActiveAndEnabled)
			{
				PickTopSelectable();
			}
			if (player.GetButtonDown(12))
			{
				PassiveUiElement passiveUiElement = CurrentUiState.BackButton as PassiveUiElement;
				if ((Object)(object)passiveUiElement != (Object)null)
				{
					passiveUiElement.ReceiveClickDown();
				}
				else
				{
					ButtonBehavior buttonBehavior = CurrentUiState.BackButton as ButtonBehavior;
					if ((Object)(object)buttonBehavior != (Object)null)
					{
						buttonBehavior.ReceiveClick();
					}
				}
			}
			if ((Object)(object)CurrentUiState.CurrentSelection == (Object)null)
			{
				return;
			}
			IsButtonHeld = player.GetButton(11);
			if (player.GetButtonDown(11))
			{
				PassiveUiElement passiveUiElement2 = CurrentUiState.CurrentSelection as PassiveUiElement;
				if ((Object)(object)passiveUiElement2 != (Object)null && ((Behaviour)passiveUiElement2).isActiveAndEnabled)
				{
					passiveUiElement2.ReceiveClickDown();
				}
				else
				{
					ButtonBehavior buttonBehavior2 = CurrentUiState.CurrentSelection as ButtonBehavior;
					if ((Object)(object)buttonBehavior2 != (Object)null && ((Behaviour)buttonBehavior2).isActiveAndEnabled)
					{
						buttonBehavior2.ReceiveClick();
					}
				}
			}
			deltaSinceLastUiHighlight += Time.deltaTime;
			bool flag = false;
			bool buttonDown = player.GetButtonDown(25);
			bool buttonDown2 = player.GetButtonDown(26);
			bool buttonDown3 = player.GetButtonDown(27);
			bool buttonDown4 = player.GetButtonDown(28);
			float num = 0f;
			float num2 = 0f;
			if (buttonDown | buttonDown2)
			{
				num2 = (buttonDown ? 1 : (-1));
				flag = true;
			}
			else if (buttonDown3 | buttonDown4)
			{
				num = (buttonDown3 ? 1 : (-1));
				flag = true;
			}
			else
			{
				num = player.GetAxis(9);
				num2 = player.GetAxis(10);
			}
			if (Mathf.Abs(num) >= 0.3f || Mathf.Abs(num2) >= 0.3f)
			{
				SlideBar slideBar = CurrentUiState.CurrentSelection as SlideBar;
				if ((Object)(object)slideBar != (Object)null)
				{
					if (slideBar.Vertical && num2 != 0f && (double)Mathf.Abs(num2) > 0.3)
					{
						VerticalAxisInputForSlideBar(num2, slideBar, flag);
						return;
					}
					if (num != 0f && (double)Mathf.Abs(num) > 0.3)
					{
						HorizontalAxisInputForSlideBar(num, slideBar, flag);
						return;
					}
				}
				if ((deltaSinceLastUiHighlight > 0.25f || !inputDetectedLastFrame) | flag)
				{
					UiElement uiElement = null;
					Vector2 origin = Vector2.op_Implicit(((Component)CurrentUiState.CurrentSelection).transform.position);
					PassiveUiElement passiveUiElement3 = CurrentUiState.CurrentSelection as PassiveUiElement;
					if ((Object)(object)passiveUiElement3 != (Object)null && passiveUiElement3.Colliders.Length != 0)
					{
						Bounds bounds = passiveUiElement3.Colliders[0].bounds;
						origin = Vector2.op_Implicit(bounds.center);
					}
					Vector2 direction = Vector2.op_Implicit(new Vector3(num, num2));
					if (CurrentUiState.CurrentSelection.ControllerNav.mode == ControllerNavigation.Mode.Explicit)
					{
						AxisDirection direction2 = FindClosestDirection(direction);
						uiElement = FindUiElementByExplicitNavigation(CurrentUiState.CurrentSelection, CurrentUiState.CurrentSelection, direction2);
					}
					else
					{
						if (!CurrentUiState.EnforceGridNavigation)
						{
							float num3 = 0.15f;
							direction = direction.normalized;
							for (int i = 0; i < 4; i++)
							{
								uiElement = FindUiElementByRaycast(origin, direction, num3);
								if ((Object)(object)uiElement != (Object)null)
								{
									break;
								}
								num3 *= 1.5f;
							}
						}
						AxisDirection direction3 = FindClosestDirection(direction);
						if ((Object)(object)uiElement == (Object)null)
						{
							float orthogonalOffsetThreshold = (CurrentUiState.EnforceGridNavigation ? 0.25f : 100f);
							uiElement = FindUiElementByGridDirection(origin, direction3, orthogonalOffsetThreshold);
						}
					}
					if ((Object)(object)uiElement != (Object)null)
					{
						HighlightSelection(uiElement);
					}
				}
				inputDetectedLastFrame = true;
			}
			else
			{
				deltaSinceLastUiHighlight = 0f;
				inputDetectedLastFrame = false;
			}
		}
	}

	private AxisDirection FindClosestDirection(Vector2 direction)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MinValue;
		AxisDirection result = AxisDirection.None;
		foreach (KeyValuePair<Vector2, AxisDirection> direction2 in directions)
		{
			float num2 = Vector2.Dot(direction, direction2.Key);
			if (num2 > num)
			{
				num = num2;
				result = direction2.Value;
			}
		}
		return result;
	}

	private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2, Vector2 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = vec2 - vec1;
		float num = ((vec2.y < vec1.y) ? (-1f) : 1f);
		return Vector2.Angle(direction, val) * num;
	}

	private UiElement FindUiElementByRaycast(Vector2 origin, Vector2 direction, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit2D[] array = Physics2D.CircleCastAll(origin, radius, direction);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D val = array[i];
			if (!((double)val.distance > 0.0) || !((Object)(object)val.collider != (Object)null) || !((Object)(object)((Component)val.transform).gameObject != (Object)(object)((Component)CurrentUiState.CurrentSelection).gameObject))
			{
				continue;
			}
			GameObject gameObject = ((Component)val.transform).gameObject;
			UiElement uiElement = gameObject.GetComponent<UiElement>();
			if ((Object)(object)uiElement == (Object)null)
			{
				uiElement = gameObject.GetComponentInParent<UiElement>();
				if ((Object)(object)uiElement == (Object)(object)CurrentUiState.CurrentSelection)
				{
					uiElement = null;
				}
				if ((Object)(object)uiElement != (Object)null)
				{
					PassiveUiElement passiveUiElement = uiElement as PassiveUiElement;
					if (!Object.op_Implicit((Object)(object)passiveUiElement) || !passiveUiElement.Colliders.Contains(val.collider))
					{
						uiElement = null;
					}
				}
			}
			if ((Object)(object)uiElement != (Object)null && CurrentUiState.SelectableUiElements.Contains(uiElement))
			{
				return uiElement;
			}
		}
		return null;
	}

	private UiElement FindUiElementByExplicitNavigation(UiElement start, UiElement from, AxisDirection direction)
	{
		UiElement uiElement = null;
		switch (direction)
		{
		case AxisDirection.Up:
			uiElement = from.ControllerNav.selectOnUp;
			break;
		case AxisDirection.Down:
			uiElement = from.ControllerNav.selectOnDown;
			break;
		case AxisDirection.Left:
			uiElement = from.ControllerNav.selectOnLeft;
			break;
		case AxisDirection.Right:
			uiElement = from.ControllerNav.selectOnRight;
			break;
		case AxisDirection.None:
			return null;
		}
		if (Object.op_Implicit((Object)(object)uiElement))
		{
			if ((Object)(object)uiElement == (Object)(object)start)
			{
				return null;
			}
			if (((Behaviour)uiElement).isActiveAndEnabled)
			{
				return uiElement;
			}
			if (uiElement.ControllerNav.mode == ControllerNavigation.Mode.Explicit)
			{
				return FindUiElementByExplicitNavigation(start, uiElement, direction);
			}
		}
		return null;
	}

	private UiElement FindUiElementByGridDirection(Vector2 origin, AxisDirection direction, float orthogonalOffsetThreshold)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.zero;
		Vector2 val2 = Vector2.zero;
		UiElement uiElement = null;
		switch (direction)
		{
		case AxisDirection.Down:
			val = new Vector2(0f, -1f);
			val2 = new Vector2(1f, 0f);
			break;
		case AxisDirection.Up:
			val = new Vector2(0f, 1f);
			val2 = new Vector2(1f, 0f);
			break;
		case AxisDirection.Left:
			val = new Vector2(-1f, 0f);
			val2 = new Vector2(0f, 1f);
			break;
		case AxisDirection.Right:
			val = new Vector2(1f, 0f);
			val2 = new Vector2(0f, 1f);
			break;
		case AxisDirection.None:
			return null;
		}
		float num = Vector2.Dot(origin, val);
		float num2 = Vector2.Dot(origin, val2);
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		foreach (UiElement selectableUiElement in CurrentUiState.SelectableUiElements)
		{
			if ((Object)(object)selectableUiElement == (Object)null || !((Behaviour)selectableUiElement).isActiveAndEnabled || (Object)(object)selectableUiElement == (Object)(object)CurrentUiState.CurrentSelection)
			{
				continue;
			}
			float num5 = Vector2.Dot(Vector2.op_Implicit(((Component)selectableUiElement).gameObject.transform.position), val);
			if (num5 <= num)
			{
				continue;
			}
			float num6 = Vector2.Dot(Vector2.op_Implicit(((Component)selectableUiElement).gameObject.transform.position), val2);
			float num7 = Mathf.Abs(num2 - num6);
			if (!(num7 > orthogonalOffsetThreshold))
			{
				float num8 = num5 - num;
				if ((num8 <= num3 || Mathf.Abs(num8 - num3) <= 0.01f) && (num7 < num4 || Mathf.Abs(num7 - num4) <= 0.01f || num7 == 0f))
				{
					num3 = num8;
					num4 = num7;
					uiElement = selectableUiElement;
				}
			}
		}
		if ((Object)(object)uiElement != (Object)null)
		{
			logger.Debug("FindUiElementByGridDirection: " + ((Object)((Component)uiElement).gameObject).name);
		}
		return uiElement;
	}

	[Obsolete("FindUiElementByDistance is deprecated, please use FindUiElementByGridDirection instead.")]
	private UiElement FindUiElementByDistance(Vector2 origin, AxisDirection direction)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		UiElement uiElement = null;
		float num = float.MinValue;
		switch (direction)
		{
		case AxisDirection.Up:
		{
			float y2 = origin.y;
			foreach (UiElement selectableUiElement in CurrentUiState.SelectableUiElements)
			{
				if (!((Behaviour)selectableUiElement).isActiveAndEnabled)
				{
					continue;
				}
				Vector2 val4 = Vector2.op_Implicit(((Component)selectableUiElement).transform.position);
				if (y2 < val4.y)
				{
					float num5 = Vector2.Distance(origin, val4);
					if ((Object)(object)uiElement == (Object)null || num5 < num)
					{
						uiElement = selectableUiElement;
						num = num5;
					}
				}
			}
			break;
		}
		case AxisDirection.Down:
		{
			float y = origin.y;
			foreach (UiElement selectableUiElement2 in CurrentUiState.SelectableUiElements)
			{
				if (!((Behaviour)selectableUiElement2).isActiveAndEnabled)
				{
					continue;
				}
				Vector2 val3 = Vector2.op_Implicit(((Component)selectableUiElement2).transform.position);
				if (y > val3.y)
				{
					float num4 = Vector2.Distance(origin, val3);
					if ((Object)(object)uiElement == (Object)null || num4 < num)
					{
						uiElement = selectableUiElement2;
						num = num4;
					}
				}
			}
			break;
		}
		case AxisDirection.Left:
		{
			float x2 = origin.x;
			foreach (UiElement selectableUiElement3 in CurrentUiState.SelectableUiElements)
			{
				if (!((Behaviour)selectableUiElement3).isActiveAndEnabled)
				{
					continue;
				}
				Vector2 val2 = Vector2.op_Implicit(((Component)selectableUiElement3).transform.position);
				if (x2 > val2.x)
				{
					float num3 = Vector2.Distance(origin, val2);
					if ((Object)(object)uiElement == (Object)null || num3 < num)
					{
						uiElement = selectableUiElement3;
						num = num3;
					}
				}
			}
			break;
		}
		case AxisDirection.Right:
		{
			float x = origin.x;
			foreach (UiElement selectableUiElement4 in CurrentUiState.SelectableUiElements)
			{
				if (!((Behaviour)selectableUiElement4).isActiveAndEnabled)
				{
					continue;
				}
				Vector2 val = Vector2.op_Implicit(((Component)selectableUiElement4).transform.position);
				if (x < val.x)
				{
					float num2 = Vector2.Distance(origin, val);
					if ((Object)(object)uiElement == (Object)null || num2 < num)
					{
						uiElement = selectableUiElement4;
						num = num2;
					}
				}
			}
			break;
		}
		}
		return uiElement;
	}

	private void VerticalAxisInputForSlideBar(float v, SlideBar slideBar, bool dpadDetected)
	{
		if ((deltaSinceLastUiHighlight > 0.1f) | dpadDetected)
		{
			if (v > 0f)
			{
				slideBar.ControllerIncrease();
			}
			else
			{
				slideBar.ControllerDecrease();
			}
			deltaSinceLastUiHighlight = 0f;
		}
	}

	private void HorizontalAxisInputForSlideBar(float h, SlideBar slideBar, bool dpadDetected)
	{
		if ((deltaSinceLastUiHighlight > 0.1f) | dpadDetected)
		{
			if (h > 0f)
			{
				slideBar.ControllerIncrease();
			}
			else
			{
				slideBar.ControllerDecrease();
			}
			deltaSinceLastUiHighlight = 0f;
		}
	}

	private void HighlightSelection(UiElement selection)
	{
		if ((Object)(object)selection == (Object)null || !((Behaviour)selection).isActiveAndEnabled)
		{
			if (Object.op_Implicit((Object)(object)selection))
			{
				logger.Warning($"Failed to highlight {((Object)selection).name}, selection.isActiveAndEnabled={((Behaviour)selection).isActiveAndEnabled}");
			}
			else
			{
				logger.Warning("Failed to highlight, selection is null");
			}
			return;
		}
		if (Object.op_Implicit((Object)(object)CurrentUiState.CurrentSelection))
		{
			CurrentUiState.CurrentSelection.ReceiveMouseOut();
		}
		if (DestroyableSingleton<PassiveButtonManager>.InstanceExists)
		{
			DestroyableSingleton<PassiveButtonManager>.Instance.LoseFocusForAll();
		}
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			selection.ReceiveMouseOver();
			if (Object.op_Implicit((Object)(object)((Component)selection).GetComponent<ColorChip>()) || Object.op_Implicit((Object)(object)((Component)selection).GetComponent<AnnouncementPanel>()))
			{
				((UnityEvent)(selection as PassiveButton).OnClick).Invoke();
			}
		}
		CurrentUiState.CurrentSelection = selection;
		deltaSinceLastUiHighlight = 0f;
	}

	public void SetCurrentSelected(UiElement selection)
	{
		HighlightSelection(selection);
	}

	public void PickTopSelectable()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		float num = -10000f;
		UiElement uiElement = null;
		foreach (UiElement selectableUiElement in CurrentUiState.SelectableUiElements)
		{
			if (Object.op_Implicit((Object)(object)selectableUiElement) && ((Behaviour)selectableUiElement).isActiveAndEnabled && ((Component)selectableUiElement).transform.position.y > num)
			{
				num = ((Component)selectableUiElement).transform.position.y;
				uiElement = selectableUiElement;
			}
		}
		if (Object.op_Implicit((Object)(object)uiElement))
		{
			HighlightSelection(uiElement);
		}
	}

	[Conditional("DEBUG")]
	public void PrintSelectableUiList()
	{
	}

	[Conditional("DEBUG")]
	public void ClearSelectableUiList()
	{
	}

	public ControllerManager()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
	}
}
