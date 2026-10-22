using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos.GamepadTemplateUI;

public class GamepadTemplateUI : MonoBehaviour
{
	private class Stick
	{
		private RectTransform _transform;

		private Vector2 _origPosition;

		private int _xAxisElementId = -1;

		private int _yAxisElementId = -1;

		public Vector2 position
		{
			get
			{
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)_transform != (Object)null))
				{
					return Vector2.zero;
				}
				return _transform.anchoredPosition - _origPosition;
			}
			set
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)_transform == (Object)null))
				{
					_transform.anchoredPosition = _origPosition + value;
				}
			}
		}

		public Stick(RectTransform transform, int xAxisElementId, int yAxisElementId)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)transform == (Object)null))
			{
				_transform = transform;
				_origPosition = _transform.anchoredPosition;
				_xAxisElementId = xAxisElementId;
				_yAxisElementId = yAxisElementId;
			}
		}

		public void Reset()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_transform == (Object)null))
			{
				_transform.anchoredPosition = _origPosition;
			}
		}

		public bool ContainsElement(int elementId)
		{
			if ((Object)(object)_transform == (Object)null)
			{
				return false;
			}
			if (elementId != _xAxisElementId)
			{
				return elementId == _yAxisElementId;
			}
			return true;
		}

		public void SetAxisPosition(int elementId, float value)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_transform == (Object)null))
			{
				Vector2 val = position;
				if (elementId == _xAxisElementId)
				{
					val.x = value;
				}
				else if (elementId == _yAxisElementId)
				{
					val.y = value;
				}
				position = val;
			}
		}
	}

	private class UIElement
	{
		public int id;

		public ControllerUIElement element;

		public UIElement(int id, ControllerUIElement element)
		{
			this.id = id;
			this.element = element;
		}
	}

	private const float stickRadius = 20f;

	public int playerId;

	[SerializeField]
	private RectTransform leftStick;

	[SerializeField]
	private RectTransform rightStick;

	[SerializeField]
	private ControllerUIElement leftStickX;

	[SerializeField]
	private ControllerUIElement leftStickY;

	[SerializeField]
	private ControllerUIElement leftStickButton;

	[SerializeField]
	private ControllerUIElement rightStickX;

	[SerializeField]
	private ControllerUIElement rightStickY;

	[SerializeField]
	private ControllerUIElement rightStickButton;

	[SerializeField]
	private ControllerUIElement actionBottomRow1;

	[SerializeField]
	private ControllerUIElement actionBottomRow2;

	[SerializeField]
	private ControllerUIElement actionBottomRow3;

	[SerializeField]
	private ControllerUIElement actionTopRow1;

	[SerializeField]
	private ControllerUIElement actionTopRow2;

	[SerializeField]
	private ControllerUIElement actionTopRow3;

	[SerializeField]
	private ControllerUIElement leftShoulder;

	[SerializeField]
	private ControllerUIElement leftTrigger;

	[SerializeField]
	private ControllerUIElement rightShoulder;

	[SerializeField]
	private ControllerUIElement rightTrigger;

	[SerializeField]
	private ControllerUIElement center1;

	[SerializeField]
	private ControllerUIElement center2;

	[SerializeField]
	private ControllerUIElement center3;

	[SerializeField]
	private ControllerUIElement dPadUp;

	[SerializeField]
	private ControllerUIElement dPadRight;

	[SerializeField]
	private ControllerUIElement dPadDown;

	[SerializeField]
	private ControllerUIElement dPadLeft;

	private UIElement[] _uiElementsArray;

	private Dictionary<int, ControllerUIElement> _uiElements = new Dictionary<int, ControllerUIElement>();

	private IList<ControllerTemplateElementTarget> _tempTargetList = new List<ControllerTemplateElementTarget>(2);

	private Stick[] _sticks;

	private Player player => ReInput.players.GetPlayer(playerId);

	private void Awake()
	{
		_uiElementsArray = new UIElement[23]
		{
			new UIElement(0, leftStickX),
			new UIElement(1, leftStickY),
			new UIElement(17, leftStickButton),
			new UIElement(2, rightStickX),
			new UIElement(3, rightStickY),
			new UIElement(18, rightStickButton),
			new UIElement(4, actionBottomRow1),
			new UIElement(5, actionBottomRow2),
			new UIElement(6, actionBottomRow3),
			new UIElement(7, actionTopRow1),
			new UIElement(8, actionTopRow2),
			new UIElement(9, actionTopRow3),
			new UIElement(14, center1),
			new UIElement(15, center2),
			new UIElement(16, center3),
			new UIElement(19, dPadUp),
			new UIElement(20, dPadRight),
			new UIElement(21, dPadDown),
			new UIElement(22, dPadLeft),
			new UIElement(10, leftShoulder),
			new UIElement(11, leftTrigger),
			new UIElement(12, rightShoulder),
			new UIElement(13, rightTrigger)
		};
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElements.Add(_uiElementsArray[i].id, _uiElementsArray[i].element);
		}
		_sticks = new Stick[2]
		{
			new Stick(leftStick, 0, 1),
			new Stick(rightStick, 2, 3)
		};
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
	}

	private void Start()
	{
		if (ReInput.isReady)
		{
			DrawLabels();
		}
	}

	private void OnDestroy()
	{
		ReInput.ControllerConnectedEvent -= OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			DrawActiveElements();
		}
	}

	private void DrawActiveElements()
	{
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElementsArray[i].element.Deactivate();
		}
		for (int j = 0; j < _sticks.Length; j++)
		{
			_sticks[j].Reset();
		}
		IList<InputAction> actions = ReInput.mapping.Actions;
		for (int k = 0; k < actions.Count; k++)
		{
			ActivateElements(player, actions[k].id);
		}
	}

	private void ActivateElements(Player player, int actionId)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		float axis = player.GetAxis(actionId);
		if (axis == 0f)
		{
			return;
		}
		IList<InputActionSourceData> currentInputSources = player.GetCurrentInputSources(actionId);
		for (int i = 0; i < currentInputSources.Count; i++)
		{
			InputActionSourceData val = currentInputSources[i];
			IGamepadTemplate template = val.controller.GetTemplate<IGamepadTemplate>();
			if (template == null)
			{
				continue;
			}
			((IControllerTemplate)template).GetElementTargets(ControllerElementTarget.op_Implicit(val.actionElementMap), _tempTargetList);
			for (int j = 0; j < _tempTargetList.Count; j++)
			{
				ControllerTemplateElementTarget val2 = _tempTargetList[j];
				int id = val2.element.id;
				ControllerUIElement controllerUIElement = _uiElements[id];
				if ((int)val2.elementType == 0)
				{
					controllerUIElement.Activate(axis);
				}
				else if ((int)val2.elementType == 1 && (player.GetButton(actionId) || player.GetNegativeButton(actionId)))
				{
					controllerUIElement.Activate(1f);
				}
				GetStick(id)?.SetAxisPosition(id, axis * 20f);
			}
		}
	}

	private void DrawLabels()
	{
		for (int i = 0; i < _uiElementsArray.Length; i++)
		{
			_uiElementsArray[i].element.ClearLabels();
		}
		IList<InputAction> actions = ReInput.mapping.Actions;
		for (int j = 0; j < actions.Count; j++)
		{
			DrawLabels(player, actions[j]);
		}
	}

	private void DrawLabels(Player player, InputAction action)
	{
		Controller firstControllerWithTemplate = player.controllers.GetFirstControllerWithTemplate<IGamepadTemplate>();
		if (firstControllerWithTemplate == null)
		{
			return;
		}
		IGamepadTemplate template = firstControllerWithTemplate.GetTemplate<IGamepadTemplate>();
		ControllerMap map = player.controllers.maps.GetMap(firstControllerWithTemplate, "Default", "Default");
		if (map != null)
		{
			for (int i = 0; i < _uiElementsArray.Length; i++)
			{
				ControllerUIElement element = _uiElementsArray[i].element;
				int id = _uiElementsArray[i].id;
				IControllerTemplateElement element2 = ((IControllerTemplate)template).GetElement(id);
				DrawLabel(element, action, map, (IControllerTemplate)template, element2);
			}
		}
	}

	private void DrawLabel(ControllerUIElement uiElement, InputAction action, ControllerMap controllerMap, IControllerTemplate template, IControllerTemplateElement element)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (element.source == null)
		{
			return;
		}
		if ((int)element.source.type == 0)
		{
			IControllerTemplateElementSource source = element.source;
			IControllerTemplateAxisSource val = (IControllerTemplateAxisSource)(object)((source is IControllerTemplateAxisSource) ? source : null);
			ActionElementMap firstElementMapWithElementTarget;
			if (val.splitAxis)
			{
				firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val.positiveTarget, action.id, true);
				if (firstElementMapWithElementTarget != null)
				{
					uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)1);
				}
				firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val.negativeTarget, action.id, true);
				if (firstElementMapWithElementTarget != null)
				{
					uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)2);
				}
				return;
			}
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val.fullTarget, action.id, true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)0);
				return;
			}
			ControllerElementTarget val2 = new ControllerElementTarget(val.fullTarget);
			val2.axisRange = (AxisRange)1;
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val2, action.id, true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)1);
			}
			val2 = new ControllerElementTarget(val.fullTarget);
			val2.axisRange = (AxisRange)2;
			firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val2, action.id, true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)2);
			}
		}
		else if ((int)element.source.type == 1)
		{
			IControllerTemplateElementSource source2 = element.source;
			IControllerTemplateButtonSource val3 = (IControllerTemplateButtonSource)(object)((source2 is IControllerTemplateButtonSource) ? source2 : null);
			ActionElementMap firstElementMapWithElementTarget = controllerMap.GetFirstElementMapWithElementTarget(val3.target, action.id, true);
			if (firstElementMapWithElementTarget != null)
			{
				uiElement.SetLabel(firstElementMapWithElementTarget.actionDescriptiveName, (AxisRange)0);
			}
		}
	}

	private Stick GetStick(int elementId)
	{
		for (int i = 0; i < _sticks.Length; i++)
		{
			if (_sticks[i].ContainsElement(elementId))
			{
				return _sticks[i];
			}
		}
		return null;
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		DrawLabels();
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		DrawLabels();
	}
}
