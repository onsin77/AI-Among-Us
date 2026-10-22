using UnityEngine;

public class VirtualJoystickController
{
	private Collider2D amTouching;

	private int fingerId = -1;

	private Controller.TouchState MyTouch;

	public Vector2 DragPosition
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (fingerId < 0)
			{
				return Vector2.zero;
			}
			return MyTouch.Position;
		}
	}

	public Vector2 DragStartPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return MyTouch.DownAt;
		}
	}

	public Camera mainCam { get; set; }

	public void Update()
	{
		if (!Object.op_Implicit((Object)(object)mainCam))
		{
			mainCam = Camera.main;
		}
	}

	public DragState CheckDrag(Collider2D coll, Collider2D deadZone = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)coll))
		{
			return DragState.NoTouch;
		}
		if (fingerId <= -1)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (((Object)(object)deadZone == (Object)null || !deadZone.OverlapPoint(touch.position)) && coll.OverlapPoint(Vector2.op_Implicit(mainCam.ScreenToWorldPoint(Vector2.op_Implicit(touch.position)))))
				{
					fingerId = touch.fingerId;
					MyTouch = new Controller.TouchState();
					MyTouch.ScreenPosition = touch.position;
					MyTouch.Position = Vector2.op_Implicit(mainCam.ScreenToWorldPoint(Vector2.op_Implicit(touch.position)));
					MyTouch.IsDown = true;
					MyTouch.active = true;
					MyTouch.FingerId = touch.fingerId;
					MyTouch.ScreenDownAt = MyTouch.ScreenPosition;
					MyTouch.DownAt = MyTouch.Position;
					amTouching = coll;
					MyTouch.dragState = DragState.TouchStart;
					return DragState.TouchStart;
				}
			}
		}
		if (fingerId > -1)
		{
			if (!Object.op_Implicit((Object)(object)amTouching) || !((Behaviour)amTouching).isActiveAndEnabled)
			{
				MyTouch = null;
				fingerId = -1;
				amTouching = null;
			}
			if ((Object)(object)coll != (Object)(object)amTouching)
			{
				return DragState.NoTouch;
			}
			Touch? val = null;
			for (int j = 0; j < Input.touchCount; j++)
			{
				Touch touch2 = Input.GetTouch(j);
				if (touch2.fingerId == fingerId)
				{
					val = touch2;
				}
			}
			if (val.HasValue)
			{
				Touch value;
				if ((Object)(object)deadZone != (Object)null)
				{
					Camera val2 = mainCam;
					value = val.Value;
					if (deadZone.OverlapPoint(Vector2.op_Implicit(val2.ScreenToWorldPoint(Vector2.op_Implicit(value.position)))))
					{
						goto IL_01f3;
					}
				}
				Controller.TouchState myTouch = MyTouch;
				value = val.Value;
				myTouch.ScreenPosition = value.position;
				Controller.TouchState myTouch2 = MyTouch;
				Camera val3 = mainCam;
				value = val.Value;
				myTouch2.Position = Vector2.op_Implicit(val3.ScreenToWorldPoint(Vector2.op_Implicit(value.position)));
				MyTouch.IsDown = true;
				MyTouch.active = true;
				if (Vector2.Distance(MyTouch.ScreenDownAt, MyTouch.ScreenPosition) > 10f || MyTouch.dragState == DragState.Dragging)
				{
					MyTouch.dragState = DragState.Dragging;
					return DragState.Dragging;
				}
				MyTouch.dragState = DragState.Holding;
				return DragState.Holding;
			}
			goto IL_01f3;
		}
		fingerId = -1;
		return DragState.NoTouch;
		IL_01f3:
		amTouching = null;
		fingerId = -1;
		MyTouch = null;
		return DragState.Released;
	}
}
