using Rewired;
using UnityEngine;

public class Controller
{
	public class TouchState
	{
		public Vector2 ScreenDownAt;

		public Vector2 ScreenPosition;

		public Vector2 DownAt;

		public Vector2 Position;

		public bool WasDown;

		public bool IsDown;

		public bool TouchStart;

		public bool TouchEnd;

		public DragState dragState;

		public bool active;

		public int FingerId;
	}

	public enum TouchType
	{
		Joystick,
		Mouse,
		Touch
	}

	private const int maxTouchCount = 4;

	private const int mainTouchIndex = 0;

	public readonly TouchState[] Touches = new TouchState[4];

	private Collider2D amTouching;

	private int touchId = -1;

	private static Vector3 oldMousePos = new Vector3(0f, 0f, 0f);

	public static TouchType currentTouchType => (TouchType)ActiveInputManager.currentControlType;

	public Vector2 HoverPosition
	{
		get
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			for (int num = Touches.Length - 1; num >= 0; num--)
			{
				TouchState touchState = Touches[num];
				if (touchState.active)
				{
					return touchState.Position;
				}
			}
			return Vector2.zero;
		}
	}

	public bool AnyTouch
	{
		get
		{
			if (!Touches[0].IsDown)
			{
				return Touches[1].IsDown;
			}
			return true;
		}
	}

	public bool AnyTouchDown
	{
		get
		{
			if (!Touches[0].TouchStart)
			{
				return Touches[1].TouchStart;
			}
			return true;
		}
	}

	public bool AnyTouchUp
	{
		get
		{
			if (!Touches[0].TouchEnd)
			{
				return Touches[1].TouchEnd;
			}
			return true;
		}
	}

	public bool FirstDown => Touches[0].TouchStart;

	public Vector2 DragPosition
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (touchId < 0)
			{
				return Vector2.zero;
			}
			return Touches[touchId].Position;
		}
	}

	public Vector2 DragStartPosition
	{
		get
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return Touches[touchId].DownAt;
		}
	}

	public Camera mainCam { get; set; }

	public Controller()
	{
		Touches = new TouchState[4];
		for (int i = 0; i < Touches.Length; i++)
		{
			Touches[i] = new TouchState();
		}
	}

	public bool CheckHover(Collider2D coll)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)coll))
		{
			return false;
		}
		for (int i = 0; i < Touches.Length; i++)
		{
			TouchState touchState = Touches[i];
			if (touchState.active && coll.OverlapPoint(touchState.Position))
			{
				return true;
			}
		}
		return false;
	}

	public DragState CheckDrag(Collider2D coll)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)coll))
		{
			return DragState.NoTouch;
		}
		if (touchId > -1 && (!Object.op_Implicit((Object)(object)amTouching) || !((Behaviour)amTouching).isActiveAndEnabled))
		{
			touchId = -1;
			amTouching = null;
		}
		if (touchId > -1)
		{
			if ((Object)(object)coll != (Object)(object)amTouching)
			{
				return DragState.NoTouch;
			}
			TouchState touchState = Touches[touchId];
			if (touchState.IsDown)
			{
				if (Vector2.Distance(touchState.ScreenDownAt, touchState.ScreenPosition) > 10f || touchState.dragState == DragState.Dragging)
				{
					touchState.dragState = DragState.Dragging;
					return DragState.Dragging;
				}
				touchState.dragState = DragState.Holding;
				return DragState.Holding;
			}
			amTouching = null;
			touchId = -1;
			touchState.dragState = DragState.Released;
			return DragState.Released;
		}
		for (int i = 0; i < Touches.Length; i++)
		{
			TouchState touchState2 = Touches[i];
			if (touchState2.TouchStart && coll.OverlapPoint(touchState2.Position))
			{
				amTouching = coll;
				touchId = i;
				touchState2.dragState = DragState.TouchStart;
				return DragState.TouchStart;
			}
		}
		return DragState.NoTouch;
	}

	public void ResetDragPosition()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Touches[touchId].DownAt = Touches[touchId].Position;
	}

	public void ClearTouch()
	{
		if (touchId >= 0)
		{
			TouchState obj = Touches[touchId];
			obj.dragState = DragState.NoTouch;
			obj.TouchStart = true;
			amTouching = null;
			touchId = -1;
		}
	}

	public void Update()
	{
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)mainCam))
		{
			mainCam = Camera.main;
		}
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Touch)
		{
			for (int i = 0; i < Touches.Length; i++)
			{
				TouchState obj = Touches[i];
				obj.WasDown = obj.IsDown;
				obj.IsDown = false;
				obj.active = false;
			}
			for (int j = 0; j < Input.touchCount; j++)
			{
				Touch touch = Input.GetTouch(j);
				if (j < Touches.Length)
				{
					TouchState obj2 = Touches[j];
					obj2.ScreenPosition = touch.position;
					obj2.Position = Vector2.op_Implicit(mainCam.ScreenToWorldPoint(Vector2.op_Implicit(touch.position)));
					obj2.IsDown = true;
					obj2.active = true;
				}
			}
			for (int k = 0; k < Touches.Length; k++)
			{
				TouchState touchState = Touches[k];
				touchState.TouchStart = !touchState.WasDown && touchState.IsDown;
				if (touchState.TouchStart)
				{
					touchState.ScreenDownAt = touchState.ScreenPosition;
					touchState.DownAt = touchState.Position;
				}
				touchState.TouchEnd = touchState.WasDown && !touchState.IsDown;
			}
		}
		Player player = ReInput.players.GetPlayer(0);
		player.controllers.GetLastActiveController();
		if (currentTouchType == TouchType.Mouse)
		{
			if (player.controllers.hasMouse)
			{
				TouchState touchState2 = Touches[0];
				bool mouseButton = Input.GetMouseButton(0);
				touchState2.ScreenPosition = Vector2.op_Implicit(Input.mousePosition);
				touchState2.Position = Vector2.op_Implicit(mainCam.ScreenToWorldPoint(Input.mousePosition));
				touchState2.TouchStart = !touchState2.IsDown & mouseButton;
				if (touchState2.TouchStart)
				{
					touchState2.ScreenDownAt = touchState2.ScreenPosition;
					touchState2.DownAt = touchState2.Position;
				}
				touchState2.TouchEnd = touchState2.IsDown && !mouseButton;
				touchState2.IsDown = mouseButton;
				touchState2.active = true;
			}
			else
			{
				TouchState obj3 = Touches[0];
				obj3.TouchStart = false;
				obj3.TouchEnd = false;
				obj3.IsDown = false;
				obj3.active = false;
			}
		}
		if (currentTouchType != TouchType.Joystick)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance) && ((Behaviour)VirtualCursor.instance).isActiveAndEnabled && SpecialInputHandler.disableVirtualCursorCount == 0)
		{
			TouchState touchState3 = Touches[0];
			bool buttonDown = VirtualCursor.buttonDown;
			touchState3.TouchStart = !touchState3.IsDown & buttonDown;
			touchState3.Position = VirtualCursor.currentPosition;
			touchState3.ScreenPosition = Vector2.op_Implicit(mainCam.WorldToScreenPoint(Vector2.op_Implicit(touchState3.Position)));
			if (touchState3.TouchStart)
			{
				touchState3.ScreenDownAt = touchState3.ScreenPosition;
				touchState3.DownAt = touchState3.Position;
			}
			touchState3.TouchEnd = touchState3.IsDown && !buttonDown;
			touchState3.IsDown = buttonDown;
			touchState3.active = true;
		}
		else
		{
			TouchState obj4 = Touches[0];
			obj4.TouchStart = false;
			obj4.TouchEnd = false;
			obj4.IsDown = false;
			obj4.active = false;
		}
	}

	public void Reset()
	{
		for (int i = 0; i < Touches.Length; i++)
		{
			Touches[i] = new TouchState();
		}
		touchId = -1;
		amTouching = null;
	}

	public TouchState GetTouch(int i)
	{
		return Touches[i];
	}

	static Controller()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
	}
}
