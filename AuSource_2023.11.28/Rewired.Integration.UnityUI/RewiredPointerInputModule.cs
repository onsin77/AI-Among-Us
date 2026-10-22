using System;
using System.Collections.Generic;
using System.Text;
using Rewired.UI;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rewired.Integration.UnityUI;

public abstract class RewiredPointerInputModule : BaseInputModule
{
	protected class MouseState
	{
		private List<ButtonState> m_TrackedButtons = new List<ButtonState>();

		public bool AnyPressesThisFrame()
		{
			for (int i = 0; i < m_TrackedButtons.Count; i++)
			{
				if (m_TrackedButtons[i].eventData.PressedThisFrame())
				{
					return true;
				}
			}
			return false;
		}

		public bool AnyReleasesThisFrame()
		{
			for (int i = 0; i < m_TrackedButtons.Count; i++)
			{
				if (m_TrackedButtons[i].eventData.ReleasedThisFrame())
				{
					return true;
				}
			}
			return false;
		}

		public ButtonState GetButtonState(int button)
		{
			ButtonState buttonState = null;
			for (int i = 0; i < m_TrackedButtons.Count; i++)
			{
				if (m_TrackedButtons[i].button == button)
				{
					buttonState = m_TrackedButtons[i];
					break;
				}
			}
			if (buttonState == null)
			{
				buttonState = new ButtonState
				{
					button = button,
					eventData = new MouseButtonEventData()
				};
				m_TrackedButtons.Add(buttonState);
			}
			return buttonState;
		}

		public void SetButtonState(int button, FramePressState stateForMouseButton, PlayerPointerEventData data)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			ButtonState buttonState = GetButtonState(button);
			buttonState.eventData.buttonState = stateForMouseButton;
			buttonState.eventData.buttonData = data;
		}
	}

	public class MouseButtonEventData
	{
		public FramePressState buttonState;

		public PlayerPointerEventData buttonData;

		public bool PressedThisFrame()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			if ((int)buttonState != 0)
			{
				return (int)buttonState == 2;
			}
			return true;
		}

		public bool ReleasedThisFrame()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			if ((int)buttonState != 1)
			{
				return (int)buttonState == 2;
			}
			return true;
		}
	}

	protected class ButtonState
	{
		private int m_Button;

		private MouseButtonEventData m_EventData;

		public MouseButtonEventData eventData
		{
			get
			{
				return m_EventData;
			}
			set
			{
				m_EventData = value;
			}
		}

		public int button
		{
			get
			{
				return m_Button;
			}
			set
			{
				m_Button = value;
			}
		}
	}

	private sealed class UnityInputSource : IMouseInputSource, ITouchInputSource
	{
		private Vector2 m_MousePosition;

		private Vector2 m_MousePositionPrev;

		private int m_LastUpdatedFrame = -1;

		int IMouseInputSource.playerId
		{
			get
			{
				TryUpdate();
				return 0;
			}
		}

		int ITouchInputSource.playerId
		{
			get
			{
				TryUpdate();
				return 0;
			}
		}

		bool IMouseInputSource.enabled
		{
			get
			{
				TryUpdate();
				return true;
			}
		}

		bool IMouseInputSource.locked
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Invalid comparison between Unknown and I4
				TryUpdate();
				return (int)Cursor.lockState == 1;
			}
		}

		int IMouseInputSource.buttonCount
		{
			get
			{
				TryUpdate();
				return 3;
			}
		}

		Vector2 IMouseInputSource.screenPosition
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				TryUpdate();
				return Vector2.op_Implicit(Input.mousePosition);
			}
		}

		Vector2 IMouseInputSource.screenPositionDelta
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				TryUpdate();
				return m_MousePosition - m_MousePositionPrev;
			}
		}

		Vector2 IMouseInputSource.wheelDelta
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				TryUpdate();
				return Input.mouseScrollDelta;
			}
		}

		bool ITouchInputSource.touchSupported
		{
			get
			{
				TryUpdate();
				return Input.touchSupported;
			}
		}

		int ITouchInputSource.touchCount
		{
			get
			{
				TryUpdate();
				return Input.touchCount;
			}
		}

		bool IMouseInputSource.GetButtonDown(int button)
		{
			TryUpdate();
			return Input.GetMouseButtonDown(button);
		}

		bool IMouseInputSource.GetButtonUp(int button)
		{
			TryUpdate();
			return Input.GetMouseButtonUp(button);
		}

		bool IMouseInputSource.GetButton(int button)
		{
			TryUpdate();
			return Input.GetMouseButton(button);
		}

		Touch ITouchInputSource.GetTouch(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			TryUpdate();
			return Input.GetTouch(index);
		}

		private void TryUpdate()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (Time.frameCount != m_LastUpdatedFrame)
			{
				m_LastUpdatedFrame = Time.frameCount;
				m_MousePositionPrev = m_MousePosition;
				m_MousePosition = Vector2.op_Implicit(Input.mousePosition);
			}
		}
	}

	public const int kMouseLeftId = -1;

	public const int kMouseRightId = -2;

	public const int kMouseMiddleId = -3;

	public const int kFakeTouchesId = -4;

	private const int customButtonsStartingId = -2147483520;

	private const int customButtonsMaxCount = 128;

	private const int customButtonsLastId = -2147483392;

	private readonly List<IMouseInputSource> m_MouseInputSourcesList = new List<IMouseInputSource>();

	private Dictionary<int, Dictionary<int, PlayerPointerEventData>[]> m_PlayerPointerData = new Dictionary<int, Dictionary<int, PlayerPointerEventData>[]>();

	private ITouchInputSource m_UserDefaultTouchInputSource;

	private UnityInputSource __m_DefaultInputSource;

	private readonly MouseState m_MouseState = new MouseState();

	private UnityInputSource defaultInputSource
	{
		get
		{
			if (__m_DefaultInputSource == null)
			{
				return __m_DefaultInputSource = new UnityInputSource();
			}
			return __m_DefaultInputSource;
		}
	}

	private IMouseInputSource defaultMouseInputSource => (IMouseInputSource)(object)defaultInputSource;

	protected ITouchInputSource defaultTouchInputSource => (ITouchInputSource)(object)defaultInputSource;

	protected virtual bool isMouseSupported
	{
		get
		{
			int count = m_MouseInputSourcesList.Count;
			if (count == 0)
			{
				return defaultMouseInputSource.enabled;
			}
			for (int i = 0; i < count; i++)
			{
				if (m_MouseInputSourcesList[i].enabled)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected bool IsDefaultMouse(IMouseInputSource mouse)
	{
		return defaultMouseInputSource == mouse;
	}

	public IMouseInputSource GetMouseInputSource(int playerId, int mouseIndex)
	{
		if (mouseIndex < 0)
		{
			throw new ArgumentOutOfRangeException("mouseIndex");
		}
		if (m_MouseInputSourcesList.Count == 0 && IsDefaultPlayer(playerId))
		{
			return defaultMouseInputSource;
		}
		int count = m_MouseInputSourcesList.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			IMouseInputSource val = m_MouseInputSourcesList[i];
			if (!UnityTools.IsNullOrDestroyed<IMouseInputSource>(val) && val.playerId == playerId)
			{
				if (mouseIndex == num)
				{
					return val;
				}
				num++;
			}
		}
		return null;
	}

	public void RemoveMouseInputSource(IMouseInputSource source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_MouseInputSourcesList.Remove(source);
	}

	public void AddMouseInputSource(IMouseInputSource source)
	{
		if (UnityTools.IsNullOrDestroyed<IMouseInputSource>(source))
		{
			throw new ArgumentNullException("source");
		}
		m_MouseInputSourcesList.Add(source);
	}

	public int GetMouseInputSourceCount(int playerId)
	{
		if (m_MouseInputSourcesList.Count == 0 && IsDefaultPlayer(playerId))
		{
			return 1;
		}
		int count = m_MouseInputSourcesList.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			IMouseInputSource val = m_MouseInputSourcesList[i];
			if (!UnityTools.IsNullOrDestroyed<IMouseInputSource>(val) && val.playerId == playerId)
			{
				num++;
			}
		}
		return num;
	}

	public ITouchInputSource GetTouchInputSource(int playerId, int sourceIndex)
	{
		if (!UnityTools.IsNullOrDestroyed<ITouchInputSource>(m_UserDefaultTouchInputSource))
		{
			return m_UserDefaultTouchInputSource;
		}
		return defaultTouchInputSource;
	}

	public void RemoveTouchInputSource(ITouchInputSource source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (m_UserDefaultTouchInputSource == source)
		{
			m_UserDefaultTouchInputSource = null;
		}
	}

	public void AddTouchInputSource(ITouchInputSource source)
	{
		if (UnityTools.IsNullOrDestroyed<ITouchInputSource>(source))
		{
			throw new ArgumentNullException("source");
		}
		m_UserDefaultTouchInputSource = source;
	}

	public int GetTouchInputSourceCount(int playerId)
	{
		if (!IsDefaultPlayer(playerId))
		{
			return 0;
		}
		return 1;
	}

	protected void ClearMouseInputSources()
	{
		m_MouseInputSourcesList.Clear();
	}

	protected abstract bool IsDefaultPlayer(int playerId);

	protected bool GetPointerData(int playerId, int pointerIndex, int pointerTypeId, out PlayerPointerEventData data, bool create, PointerEventType pointerEventType)
	{
		if (!m_PlayerPointerData.TryGetValue(playerId, out var value))
		{
			value = new Dictionary<int, PlayerPointerEventData>[pointerIndex + 1];
			for (int i = 0; i < value.Length; i++)
			{
				value[i] = new Dictionary<int, PlayerPointerEventData>();
			}
			m_PlayerPointerData.Add(playerId, value);
		}
		if (pointerIndex >= value.Length)
		{
			Dictionary<int, PlayerPointerEventData>[] array = new Dictionary<int, PlayerPointerEventData>[pointerIndex + 1];
			for (int j = 0; j < value.Length; j++)
			{
				array[j] = value[j];
			}
			array[pointerIndex] = new Dictionary<int, PlayerPointerEventData>();
			value = array;
			m_PlayerPointerData[playerId] = value;
		}
		Dictionary<int, PlayerPointerEventData> dictionary = value[pointerIndex];
		if (!dictionary.TryGetValue(pointerTypeId, out data))
		{
			if (!create)
			{
				return false;
			}
			data = CreatePointerEventData(playerId, pointerIndex, pointerTypeId, pointerEventType);
			dictionary.Add(pointerTypeId, data);
			return true;
		}
		data.mouseSource = ((pointerEventType == PointerEventType.Mouse) ? GetMouseInputSource(playerId, pointerIndex) : null);
		data.touchSource = ((pointerEventType == PointerEventType.Touch) ? GetTouchInputSource(playerId, pointerIndex) : null);
		return false;
	}

	private PlayerPointerEventData CreatePointerEventData(int playerId, int pointerIndex, int pointerTypeId, PointerEventType pointerEventType)
	{
		PlayerPointerEventData playerPointerEventData = new PlayerPointerEventData(((BaseInputModule)this).eventSystem)
		{
			playerId = playerId,
			inputSourceIndex = pointerIndex
		};
		((PointerEventData)playerPointerEventData).pointerId = pointerTypeId;
		playerPointerEventData.sourceType = pointerEventType;
		PlayerPointerEventData playerPointerEventData2 = playerPointerEventData;
		switch (pointerEventType)
		{
		case PointerEventType.Mouse:
			playerPointerEventData2.mouseSource = GetMouseInputSource(playerId, pointerIndex);
			break;
		case PointerEventType.Touch:
			playerPointerEventData2.touchSource = GetTouchInputSource(playerId, pointerIndex);
			break;
		}
		if (pointerTypeId == -1)
		{
			playerPointerEventData2.buttonIndex = 0;
		}
		else if (pointerTypeId == -2)
		{
			playerPointerEventData2.buttonIndex = 1;
		}
		else if (pointerTypeId == -3)
		{
			playerPointerEventData2.buttonIndex = 2;
		}
		else if (pointerTypeId >= -2147483520 && pointerTypeId <= -2147483392)
		{
			playerPointerEventData2.buttonIndex = pointerTypeId - -2147483520;
		}
		return playerPointerEventData2;
	}

	protected void RemovePointerData(PlayerPointerEventData data)
	{
		if (m_PlayerPointerData.TryGetValue(data.playerId, out var value) && (uint)data.inputSourceIndex < (uint)value.Length)
		{
			value[data.inputSourceIndex].Remove(((PointerEventData)data).pointerId);
		}
	}

	protected PlayerPointerEventData GetTouchPointerEventData(int playerId, int touchDeviceIndex, Touch input, out bool pressed, out bool released)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		bool pointerData = GetPointerData(playerId, touchDeviceIndex, input.fingerId, out var data, create: true, PointerEventType.Touch);
		((AbstractEventData)data).Reset();
		pressed = pointerData || (int)input.phase == 0;
		released = (int)input.phase == 4 || (int)input.phase == 3;
		if (pointerData)
		{
			((PointerEventData)data).position = input.position;
		}
		if (pressed)
		{
			((PointerEventData)data).delta = Vector2.zero;
		}
		else
		{
			((PointerEventData)data).delta = input.position - ((PointerEventData)data).position;
		}
		((PointerEventData)data).position = input.position;
		((PointerEventData)data).button = (InputButton)0;
		((BaseInputModule)this).eventSystem.RaycastAll((PointerEventData)(object)data, base.m_RaycastResultCache);
		RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(base.m_RaycastResultCache);
		((PointerEventData)data).pointerCurrentRaycast = pointerCurrentRaycast;
		base.m_RaycastResultCache.Clear();
		return data;
	}

	protected virtual MouseState GetMousePointerEventData(int playerId, int mouseIndex)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		IMouseInputSource mouseInputSource = GetMouseInputSource(playerId, mouseIndex);
		if (mouseInputSource == null)
		{
			return null;
		}
		bool pointerData = GetPointerData(playerId, mouseIndex, -1, out var data, create: true, PointerEventType.Mouse);
		((AbstractEventData)data).Reset();
		if (pointerData)
		{
			((PointerEventData)data).position = mouseInputSource.screenPosition;
		}
		Vector2 screenPosition = mouseInputSource.screenPosition;
		if (mouseInputSource.locked || !mouseInputSource.enabled)
		{
			((PointerEventData)data).position = new Vector2(-1f, -1f);
			((PointerEventData)data).delta = Vector2.zero;
		}
		else
		{
			((PointerEventData)data).delta = screenPosition - ((PointerEventData)data).position;
			((PointerEventData)data).position = screenPosition;
		}
		((PointerEventData)data).scrollDelta = mouseInputSource.wheelDelta;
		((PointerEventData)data).button = (InputButton)0;
		((BaseInputModule)this).eventSystem.RaycastAll((PointerEventData)(object)data, base.m_RaycastResultCache);
		RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(base.m_RaycastResultCache);
		((PointerEventData)data).pointerCurrentRaycast = pointerCurrentRaycast;
		base.m_RaycastResultCache.Clear();
		GetPointerData(playerId, mouseIndex, -2, out var data2, create: true, PointerEventType.Mouse);
		CopyFromTo((PointerEventData)(object)data, (PointerEventData)(object)data2);
		((PointerEventData)data2).button = (InputButton)1;
		GetPointerData(playerId, mouseIndex, -3, out var data3, create: true, PointerEventType.Mouse);
		CopyFromTo((PointerEventData)(object)data, (PointerEventData)(object)data3);
		((PointerEventData)data3).button = (InputButton)2;
		for (int i = 3; i < mouseInputSource.buttonCount; i++)
		{
			GetPointerData(playerId, mouseIndex, -2147483520 + i, out var data4, create: true, PointerEventType.Mouse);
			CopyFromTo((PointerEventData)(object)data, (PointerEventData)(object)data4);
			((PointerEventData)data4).button = (InputButton)(-1);
		}
		m_MouseState.SetButtonState(0, StateForMouseButton(playerId, mouseIndex, 0), data);
		m_MouseState.SetButtonState(1, StateForMouseButton(playerId, mouseIndex, 1), data2);
		m_MouseState.SetButtonState(2, StateForMouseButton(playerId, mouseIndex, 2), data3);
		for (int j = 3; j < mouseInputSource.buttonCount; j++)
		{
			GetPointerData(playerId, mouseIndex, -2147483520 + j, out var data5, create: false, PointerEventType.Mouse);
			m_MouseState.SetButtonState(j, StateForMouseButton(playerId, mouseIndex, j), data5);
		}
		return m_MouseState;
	}

	protected PlayerPointerEventData GetLastPointerEventData(int playerId, int pointerIndex, int pointerTypeId, bool ignorePointerTypeId, PointerEventType pointerEventType)
	{
		if (!ignorePointerTypeId)
		{
			GetPointerData(playerId, pointerIndex, pointerTypeId, out var data, create: false, pointerEventType);
			return data;
		}
		if (!m_PlayerPointerData.TryGetValue(playerId, out var value))
		{
			return null;
		}
		if ((uint)pointerIndex >= (uint)value.Length)
		{
			return null;
		}
		using (Dictionary<int, PlayerPointerEventData>.Enumerator enumerator = value[pointerIndex].GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Value;
			}
		}
		return null;
	}

	private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!useDragThreshold)
		{
			return true;
		}
		Vector2 val = pressPos - currentPos;
		return val.sqrMagnitude >= threshold * threshold;
	}

	protected virtual void ProcessMove(PlayerPointerEventData pointerEvent)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		RaycastResult pointerCurrentRaycast;
		GameObject val;
		if (pointerEvent.sourceType == PointerEventType.Mouse)
		{
			IMouseInputSource mouseInputSource = GetMouseInputSource(pointerEvent.playerId, pointerEvent.inputSourceIndex);
			if (mouseInputSource != null)
			{
				object obj;
				if (mouseInputSource.enabled && !mouseInputSource.locked)
				{
					pointerCurrentRaycast = ((PointerEventData)pointerEvent).pointerCurrentRaycast;
					obj = pointerCurrentRaycast.gameObject;
				}
				else
				{
					obj = null;
				}
				val = (GameObject)obj;
			}
			else
			{
				val = null;
			}
		}
		else
		{
			if (pointerEvent.sourceType != PointerEventType.Touch)
			{
				throw new NotImplementedException();
			}
			pointerCurrentRaycast = ((PointerEventData)pointerEvent).pointerCurrentRaycast;
			val = pointerCurrentRaycast.gameObject;
		}
		((BaseInputModule)this).HandlePointerExitAndEnter((PointerEventData)(object)pointerEvent, val);
	}

	protected virtual void ProcessDrag(PlayerPointerEventData pointerEvent)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!((PointerEventData)pointerEvent).IsPointerMoving() || (Object)(object)((PointerEventData)pointerEvent).pointerDrag == (Object)null)
		{
			return;
		}
		if (pointerEvent.sourceType == PointerEventType.Mouse)
		{
			IMouseInputSource mouseInputSource = GetMouseInputSource(pointerEvent.playerId, pointerEvent.inputSourceIndex);
			if (mouseInputSource == null || mouseInputSource.locked || !mouseInputSource.enabled)
			{
				return;
			}
		}
		if (!((PointerEventData)pointerEvent).dragging && ShouldStartDrag(((PointerEventData)pointerEvent).pressPosition, ((PointerEventData)pointerEvent).position, ((BaseInputModule)this).eventSystem.pixelDragThreshold, ((PointerEventData)pointerEvent).useDragThreshold))
		{
			ExecuteEvents.Execute<IBeginDragHandler>(((PointerEventData)pointerEvent).pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.beginDragHandler);
			((PointerEventData)pointerEvent).dragging = true;
		}
		if (((PointerEventData)pointerEvent).dragging)
		{
			if ((Object)(object)((PointerEventData)pointerEvent).pointerPress != (Object)(object)((PointerEventData)pointerEvent).pointerDrag)
			{
				ExecuteEvents.Execute<IPointerUpHandler>(((PointerEventData)pointerEvent).pointerPress, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerUpHandler);
				((PointerEventData)pointerEvent).eligibleForClick = false;
				((PointerEventData)pointerEvent).pointerPress = null;
				((PointerEventData)pointerEvent).rawPointerPress = null;
			}
			ExecuteEvents.Execute<IDragHandler>(((PointerEventData)pointerEvent).pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.dragHandler);
		}
	}

	public override bool IsPointerOverGameObject(int pointerTypeId)
	{
		foreach (KeyValuePair<int, Dictionary<int, PlayerPointerEventData>[]> playerPointerDatum in m_PlayerPointerData)
		{
			Dictionary<int, PlayerPointerEventData>[] value = playerPointerDatum.Value;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].TryGetValue(pointerTypeId, out var value2) && (Object)(object)((PointerEventData)value2).pointerEnter != (Object)null)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected void ClearSelection()
	{
		BaseEventData baseEventData = ((BaseInputModule)this).GetBaseEventData();
		foreach (KeyValuePair<int, Dictionary<int, PlayerPointerEventData>[]> playerPointerDatum in m_PlayerPointerData)
		{
			Dictionary<int, PlayerPointerEventData>[] value = playerPointerDatum.Value;
			for (int i = 0; i < value.Length; i++)
			{
				foreach (KeyValuePair<int, PlayerPointerEventData> item in value[i])
				{
					((BaseInputModule)this).HandlePointerExitAndEnter((PointerEventData)(object)item.Value, (GameObject)null);
				}
				value[i].Clear();
			}
		}
		((BaseInputModule)this).eventSystem.SetSelectedGameObject((GameObject)null, baseEventData);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("<b>Pointer Input Module of type: </b>" + ((object)this).GetType());
		stringBuilder.AppendLine();
		foreach (KeyValuePair<int, Dictionary<int, PlayerPointerEventData>[]> playerPointerDatum in m_PlayerPointerData)
		{
			stringBuilder.AppendLine("<B>Player Id:</b> " + playerPointerDatum.Key);
			Dictionary<int, PlayerPointerEventData>[] value = playerPointerDatum.Value;
			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.AppendLine("<B>Pointer Index:</b> " + i);
				foreach (KeyValuePair<int, PlayerPointerEventData> item in value[i])
				{
					stringBuilder.AppendLine("<B>Button Id:</b> " + item.Key);
					stringBuilder.AppendLine(((object)item.Value).ToString());
				}
			}
		}
		return stringBuilder.ToString();
	}

	protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
	{
		if ((Object)(object)ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo) != (Object)(object)((BaseInputModule)this).eventSystem.currentSelectedGameObject)
		{
			((BaseInputModule)this).eventSystem.SetSelectedGameObject((GameObject)null, pointerEvent);
		}
	}

	protected void CopyFromTo(PointerEventData from, PointerEventData to)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		to.position = from.position;
		to.delta = from.delta;
		to.scrollDelta = from.scrollDelta;
		to.pointerCurrentRaycast = from.pointerCurrentRaycast;
		to.pointerEnter = from.pointerEnter;
	}

	protected FramePressState StateForMouseButton(int playerId, int mouseIndex, int buttonId)
	{
		IMouseInputSource mouseInputSource = GetMouseInputSource(playerId, mouseIndex);
		if (mouseInputSource != null)
		{
			bool buttonDown = mouseInputSource.GetButtonDown(buttonId);
			bool buttonUp = mouseInputSource.GetButtonUp(buttonId);
			if (!(buttonDown & buttonUp))
			{
				if (!buttonDown)
				{
					if (!buttonUp)
					{
						return (FramePressState)3;
					}
					return (FramePressState)1;
				}
				return (FramePressState)0;
			}
			return (FramePressState)2;
		}
		return (FramePressState)3;
	}
}
