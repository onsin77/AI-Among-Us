using System;
using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using Rewired.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class CalibrationWindow : Window
{
	public enum ButtonIdentifier
	{
		Done,
		Cancel,
		Default,
		Calibrate
	}

	private const float minSensitivityOtherAxes = 0.1f;

	private const float maxDeadzone = 0.8f;

	[SerializeField]
	private RectTransform rightContentContainer;

	[SerializeField]
	private RectTransform valueDisplayGroup;

	[SerializeField]
	private RectTransform calibratedValueMarker;

	[SerializeField]
	private RectTransform rawValueMarker;

	[SerializeField]
	private RectTransform calibratedZeroMarker;

	[SerializeField]
	private RectTransform deadzoneArea;

	[SerializeField]
	private Slider deadzoneSlider;

	[SerializeField]
	private Slider zeroSlider;

	[SerializeField]
	private Slider sensitivitySlider;

	[SerializeField]
	private Toggle invertToggle;

	[SerializeField]
	private RectTransform axisScrollAreaContent;

	[SerializeField]
	private Button doneButton;

	[SerializeField]
	private Button calibrateButton;

	[SerializeField]
	private TMP_Text doneButtonLabel;

	[SerializeField]
	private TMP_Text cancelButtonLabel;

	[SerializeField]
	private TMP_Text defaultButtonLabel;

	[SerializeField]
	private TMP_Text deadzoneSliderLabel;

	[SerializeField]
	private TMP_Text zeroSliderLabel;

	[SerializeField]
	private TMP_Text sensitivitySliderLabel;

	[SerializeField]
	private TMP_Text invertToggleLabel;

	[SerializeField]
	private TMP_Text calibrateButtonLabel;

	[SerializeField]
	private GameObject axisButtonPrefab;

	private Joystick joystick;

	private string origCalibrationData;

	private int selectedAxis = -1;

	private AxisCalibrationData origSelectedAxisCalibrationData;

	private float displayAreaWidth;

	private List<Button> axisButtons;

	private Dictionary<int, Action<int>> buttonCallbacks;

	private int playerId;

	private RewiredStandaloneInputModule rewiredStandaloneInputModule;

	private int menuHorizActionId = -1;

	private int menuVertActionId = -1;

	private float minSensitivity;

	private bool axisSelected
	{
		get
		{
			if (joystick == null)
			{
				return false;
			}
			if (selectedAxis < 0 || selectedAxis >= ((ControllerWithAxes)joystick).calibrationMap.axisCount)
			{
				return false;
			}
			return true;
		}
	}

	private AxisCalibration axisCalibration
	{
		get
		{
			if (!axisSelected)
			{
				return null;
			}
			return ((ControllerWithAxes)joystick).calibrationMap.GetAxis(selectedAxis);
		}
	}

	public override void Initialize(int id, Func<int, bool> isFocusedCallback)
	{
		if ((Object)(object)rightContentContainer == (Object)null || (Object)(object)valueDisplayGroup == (Object)null || (Object)(object)calibratedValueMarker == (Object)null || (Object)(object)rawValueMarker == (Object)null || (Object)(object)calibratedZeroMarker == (Object)null || (Object)(object)deadzoneArea == (Object)null || (Object)(object)deadzoneSlider == (Object)null || (Object)(object)sensitivitySlider == (Object)null || (Object)(object)zeroSlider == (Object)null || (Object)(object)invertToggle == (Object)null || (Object)(object)axisScrollAreaContent == (Object)null || (Object)(object)doneButton == (Object)null || (Object)(object)calibrateButton == (Object)null || (Object)(object)axisButtonPrefab == (Object)null || (Object)(object)doneButtonLabel == (Object)null || (Object)(object)cancelButtonLabel == (Object)null || (Object)(object)defaultButtonLabel == (Object)null || (Object)(object)deadzoneSliderLabel == (Object)null || (Object)(object)zeroSliderLabel == (Object)null || (Object)(object)sensitivitySliderLabel == (Object)null || (Object)(object)invertToggleLabel == (Object)null || (Object)(object)calibrateButtonLabel == (Object)null)
		{
			Debug.LogError((object)"Rewired Control Mapper: All inspector values must be assigned!");
			return;
		}
		axisButtons = new List<Button>();
		buttonCallbacks = new Dictionary<int, Action<int>>();
		doneButtonLabel.text = ControlMapper.GetLanguage().done;
		cancelButtonLabel.text = ControlMapper.GetLanguage().cancel;
		defaultButtonLabel.text = ControlMapper.GetLanguage().default_;
		deadzoneSliderLabel.text = ControlMapper.GetLanguage().calibrateWindow_deadZoneSliderLabel;
		zeroSliderLabel.text = ControlMapper.GetLanguage().calibrateWindow_zeroSliderLabel;
		sensitivitySliderLabel.text = ControlMapper.GetLanguage().calibrateWindow_sensitivitySliderLabel;
		invertToggleLabel.text = ControlMapper.GetLanguage().calibrateWindow_invertToggleLabel;
		calibrateButtonLabel.text = ControlMapper.GetLanguage().calibrateWindow_calibrateButtonLabel;
		base.Initialize(id, isFocusedCallback);
	}

	public void SetJoystick(int playerId, Joystick joystick)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected Obj, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			return;
		}
		this.playerId = playerId;
		this.joystick = joystick;
		if (joystick == null)
		{
			Debug.LogError((object)"Rewired Control Mapper: Joystick cannot be null!");
			return;
		}
		float num = 0f;
		for (int i = 0; i < ((ControllerWithAxes)joystick).axisCount; i++)
		{
			int index = i;
			GameObject val = UITools.InstantiateGUIObject<Button>(axisButtonPrefab, (Transform)(object)axisScrollAreaContent, "Axis" + i);
			Button button = val.GetComponent<Button>();
			((UnityEvent)button.onClick).AddListener((UnityAction)(() =>
			{
				OnAxisSelected(index, button);
			}));
			TMP_Text componentInSelfOrChildren = UnityTools.GetComponentInSelfOrChildren<TMP_Text>(val);
			if ((Object)(object)componentInSelfOrChildren != (Object)null)
			{
				componentInSelfOrChildren.text = ControlMapper.GetLanguage().GetElementIdentifierName((Controller)(object)joystick, ((ControllerWithAxes)joystick).AxisElementIdentifiers[i].id, (AxisRange)0);
			}
			if (num == 0f)
			{
				num = UnityTools.GetComponentInSelfOrChildren<LayoutElement>(val).minHeight;
			}
			axisButtons.Add(button);
		}
		float spacing = ((HorizontalOrVerticalLayoutGroup)((Component)axisScrollAreaContent).GetComponent<VerticalLayoutGroup>()).spacing;
		axisScrollAreaContent.sizeDelta = new Vector2(axisScrollAreaContent.sizeDelta.x, Mathf.Max((float)((ControllerWithAxes)joystick).axisCount * (num + spacing) - spacing, axisScrollAreaContent.sizeDelta.y));
		origCalibrationData = ((ControllerWithAxes)joystick).calibrationMap.ToXmlString();
		displayAreaWidth = rightContentContainer.sizeDelta.x;
		rewiredStandaloneInputModule = ((Component)((Component)this).gameObject.transform.root).GetComponentInChildren<RewiredStandaloneInputModule>();
		if ((Object)(object)rewiredStandaloneInputModule != (Object)null)
		{
			menuHorizActionId = ReInput.mapping.GetActionId(rewiredStandaloneInputModule.horizontalAxis);
			menuVertActionId = ReInput.mapping.GetActionId(rewiredStandaloneInputModule.verticalAxis);
		}
		if (((ControllerWithAxes)joystick).axisCount > 0)
		{
			SelectAxis(0);
		}
		defaultUIElement = ((Component)doneButton).gameObject;
		RefreshControls();
		Redraw();
	}

	public void SetButtonCallback(ButtonIdentifier buttonIdentifier, Action<int> callback)
	{
		if (initialized && callback != null)
		{
			if (buttonCallbacks.ContainsKey((int)buttonIdentifier))
			{
				buttonCallbacks[(int)buttonIdentifier] = callback;
			}
			else
			{
				buttonCallbacks.Add((int)buttonIdentifier, callback);
			}
		}
	}

	public override void Cancel()
	{
		if (!initialized)
		{
			return;
		}
		if (joystick != null)
		{
			((ControllerWithAxes)joystick).ImportCalibrationMapFromXmlString(origCalibrationData);
		}
		if (!buttonCallbacks.TryGetValue(1, out var value))
		{
			if (cancelCallback != null)
			{
				cancelCallback.Invoke();
			}
		}
		else
		{
			value(id);
		}
	}

	protected override void Update()
	{
		if (initialized)
		{
			base.Update();
			UpdateDisplay();
		}
	}

	public void OnDone()
	{
		if (initialized && buttonCallbacks.TryGetValue(0, out var value))
		{
			value(id);
		}
	}

	public void OnCancel()
	{
		Cancel();
	}

	public void OnRestoreDefault()
	{
		if (initialized && joystick != null)
		{
			((ControllerWithAxes)joystick).calibrationMap.Reset();
			RefreshControls();
			Redraw();
		}
	}

	public void OnCalibrate()
	{
		if (initialized && buttonCallbacks.TryGetValue(3, out var value))
		{
			value(selectedAxis);
		}
	}

	public void OnInvert(bool state)
	{
		if (initialized && axisSelected)
		{
			axisCalibration.invert = state;
		}
	}

	public void OnZeroValueChange(float value)
	{
		if (initialized && axisSelected)
		{
			axisCalibration.calibratedZero = value;
			RedrawCalibratedZero();
		}
	}

	public void OnZeroCancel()
	{
		if (initialized && axisSelected)
		{
			axisCalibration.calibratedZero = origSelectedAxisCalibrationData.zero;
			RedrawCalibratedZero();
			RefreshControls();
		}
	}

	public void OnDeadzoneValueChange(float value)
	{
		if (initialized && axisSelected)
		{
			axisCalibration.deadZone = Mathf.Clamp(value, 0f, 0.8f);
			if (value > 0.8f)
			{
				deadzoneSlider.value = 0.8f;
			}
			RedrawDeadzone();
		}
	}

	public void OnDeadzoneCancel()
	{
		if (initialized && axisSelected)
		{
			axisCalibration.deadZone = origSelectedAxisCalibrationData.deadZone;
			RedrawDeadzone();
			RefreshControls();
		}
	}

	public void OnSensitivityValueChange(float value)
	{
		if (initialized && axisSelected)
		{
			SetSensitivity(axisCalibration, value);
		}
	}

	public void OnSensitivityCancel(float value)
	{
		if (initialized && axisSelected)
		{
			axisCalibration.sensitivity = origSelectedAxisCalibrationData.sensitivity;
			RefreshControls();
		}
	}

	public void OnAxisScrollRectScroll(Vector2 pos)
	{
		_ = initialized;
	}

	private void OnAxisSelected(int axisIndex, Button button)
	{
		if (initialized && joystick != null)
		{
			SelectAxis(axisIndex);
			RefreshControls();
			Redraw();
		}
	}

	private void UpdateDisplay()
	{
		RedrawValueMarkers();
	}

	private void Redraw()
	{
		RedrawCalibratedZero();
		RedrawValueMarkers();
	}

	private void RefreshControls()
	{
		if (!axisSelected)
		{
			deadzoneSlider.value = 0f;
			zeroSlider.value = 0f;
			sensitivitySlider.value = 0f;
			invertToggle.isOn = false;
		}
		else
		{
			deadzoneSlider.value = axisCalibration.deadZone;
			zeroSlider.value = axisCalibration.calibratedZero;
			sensitivitySlider.value = GetSliderSensitivity(axisCalibration);
			invertToggle.isOn = axisCalibration.invert;
		}
	}

	private void RedrawDeadzone()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (axisSelected)
		{
			float num = displayAreaWidth * axisCalibration.deadZone;
			deadzoneArea.sizeDelta = new Vector2(num, deadzoneArea.sizeDelta.y);
			deadzoneArea.anchoredPosition = new Vector2(axisCalibration.calibratedZero * (0f - ((Transform)deadzoneArea).parent.localPosition.x), deadzoneArea.anchoredPosition.y);
		}
	}

	private void RedrawCalibratedZero()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (axisSelected)
		{
			calibratedZeroMarker.anchoredPosition = new Vector2(axisCalibration.calibratedZero * (0f - ((Transform)deadzoneArea).parent.localPosition.x), calibratedZeroMarker.anchoredPosition.y);
			RedrawDeadzone();
		}
	}

	private void RedrawValueMarkers()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!axisSelected)
		{
			calibratedValueMarker.anchoredPosition = new Vector2(0f, calibratedValueMarker.anchoredPosition.y);
			rawValueMarker.anchoredPosition = new Vector2(0f, rawValueMarker.anchoredPosition.y);
			return;
		}
		float axis = ((ControllerWithAxes)joystick).GetAxis(selectedAxis);
		float num = Mathf.Clamp(((ControllerWithAxes)joystick).GetAxisRaw(selectedAxis), -1f, 1f);
		calibratedValueMarker.anchoredPosition = new Vector2(displayAreaWidth * 0.5f * axis, calibratedValueMarker.anchoredPosition.y);
		rawValueMarker.anchoredPosition = new Vector2(displayAreaWidth * 0.5f * num, rawValueMarker.anchoredPosition.y);
	}

	private void SelectAxis(int index)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= axisButtons.Count || (Object)(object)axisButtons[index] == (Object)null)
		{
			return;
		}
		((Selectable)axisButtons[index]).interactable = false;
		((Selectable)axisButtons[index]).Select();
		for (int i = 0; i < axisButtons.Count; i++)
		{
			if (i != index)
			{
				((Selectable)axisButtons[i]).interactable = true;
			}
		}
		selectedAxis = index;
		origSelectedAxisCalibrationData = axisCalibration.GetData();
		SetMinSensitivity();
	}

	public override void TakeInputFocus()
	{
		base.TakeInputFocus();
		if (selectedAxis >= 0)
		{
			SelectAxis(selectedAxis);
		}
		RefreshControls();
		Redraw();
	}

	private void SetMinSensitivity()
	{
		if (!axisSelected)
		{
			return;
		}
		minSensitivity = 0.1f;
		if ((Object)(object)rewiredStandaloneInputModule != (Object)null)
		{
			if (IsMenuAxis(menuHorizActionId, selectedAxis))
			{
				GetAxisButtonDeadZone(playerId, menuHorizActionId, ref minSensitivity);
			}
			else if (IsMenuAxis(menuVertActionId, selectedAxis))
			{
				GetAxisButtonDeadZone(playerId, menuVertActionId, ref minSensitivity);
			}
		}
	}

	private bool IsMenuAxis(int actionId, int axisIndex)
	{
		if ((Object)(object)rewiredStandaloneInputModule == (Object)null)
		{
			return false;
		}
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		int count = allPlayers.Count;
		for (int i = 0; i < count; i++)
		{
			IList<JoystickMap> maps = allPlayers[i].controllers.maps.GetMaps<JoystickMap>(((Controller)joystick).id);
			if (maps == null)
			{
				continue;
			}
			int count2 = maps.Count;
			for (int j = 0; j < count2; j++)
			{
				IList<ActionElementMap> axisMaps = ((ControllerMapWithAxes)maps[j]).AxisMaps;
				if (axisMaps == null)
				{
					continue;
				}
				int count3 = axisMaps.Count;
				for (int k = 0; k < count3; k++)
				{
					ActionElementMap val = axisMaps[k];
					if (val.actionId == actionId && val.elementIndex == axisIndex)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void GetAxisButtonDeadZone(int playerId, int actionId, ref float value)
	{
		InputAction action = ReInput.mapping.GetAction(actionId);
		if (action != null)
		{
			int behaviorId = action.behaviorId;
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				value = inputBehavior.buttonDeadZone + 0.1f;
			}
		}
	}

	private float GetSliderSensitivity(AxisCalibration axisCalibration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)axisCalibration.sensitivityType == 0)
		{
			return axisCalibration.sensitivity;
		}
		if ((int)axisCalibration.sensitivityType == 1)
		{
			return ProcessPowerValue(axisCalibration.sensitivity, 0f, sensitivitySlider.maxValue);
		}
		return axisCalibration.sensitivity;
	}

	public void SetSensitivity(AxisCalibration axisCalibration, float sliderValue)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		if ((int)axisCalibration.sensitivityType == 0)
		{
			axisCalibration.sensitivity = Mathf.Clamp(sliderValue, minSensitivity, float.PositiveInfinity);
			if (sliderValue < minSensitivity)
			{
				sensitivitySlider.value = minSensitivity;
			}
		}
		else if ((int)axisCalibration.sensitivityType == 1)
		{
			axisCalibration.sensitivity = ProcessPowerValue(sliderValue, 0f, sensitivitySlider.maxValue);
		}
		else
		{
			axisCalibration.sensitivity = sliderValue;
		}
	}

	private static float ProcessPowerValue(float value, float minValue, float maxValue)
	{
		value = Mathf.Clamp(value, minValue, maxValue);
		if (value > 1f)
		{
			value = MathTools.ValueInNewRange(value, 1f, maxValue, 1f, 0f);
		}
		else if (value < 1f)
		{
			value = MathTools.ValueInNewRange(value, 0f, 1f, maxValue, 1f);
		}
		return value;
	}
}
