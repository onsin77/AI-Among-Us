using System;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;

public class TouchpadBehavior : MonoBehaviour
{
	private float aspect = (float)Screen.width / (float)Screen.height;

	private bool touching;

	private bool firstTouch;

	private Vector2 toCenter = new Vector2(0.5f, 0.5f);

	private Vector2 firstTouchPos;

	private Vector2 delta;

	private Vector2 fromCenter;

	private IDualShock4Extension ds4;

	public float touchpadSensitivity = 3f;

	private void Start()
	{
		GetExtension();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(GetExtension));
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(GetExtension));
	}

	private void GetExtension()
	{
		Player player = ReInput.players.GetPlayer(0);
		ds4 = null;
		if (player != null && player.controllers.joystickCount > 0)
		{
			ds4 = ((Controller)player.controllers.Joysticks[0]).GetExtension<IDualShock4Extension>();
		}
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (ds4 != null && ds4.touchCount > 0)
		{
			Vector2 val = default;
			ds4.GetTouchPosition(0, ref val);
			if (!touching)
			{
				touching = true;
				firstTouch = true;
				firstTouchPos = val;
			}
			else
			{
				firstTouch = false;
			}
			delta = val - firstTouchPos;
			delta.x *= touchpadSensitivity * aspect;
			delta.y *= touchpadSensitivity;
			fromCenter = val - toCenter;
			fromCenter.x *= touchpadSensitivity * aspect;
			fromCenter.y *= touchpadSensitivity;
		}
		else
		{
			touching = false;
		}
	}

	public bool IsTouching()
	{
		return touching;
	}

	public bool IsFirstTouch()
	{
		return firstTouch;
	}

	public void ResetTouch()
	{
		touching = false;
		firstTouch = false;
	}

	public Vector2 GetTouchVector()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (touching)
		{
			return delta;
		}
		return Vector2.zero;
	}

	public Vector2 GetCenterToTouch()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (touching)
		{
			return fromCenter;
		}
		return Vector2.zero;
	}

	public TouchpadBehavior()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
	}
}
