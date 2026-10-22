using System.Runtime.CompilerServices;
using UnityEngine;

public class ScreenJoystick : MonoBehaviour, IVirtualJoystick
{
	private Collider2D[] hitBuffer = new Collider2D[20];

	private Controller myController = new Controller();

	private int touchId = -1;

	public Vector2 DeltaL
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

	public Vector2 DeltaR
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Vector2.zero;
		}
	}

	public void ToggleVisuals(bool on)
	{
	}

	private void FixedUpdate()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		if (touchId > -1)
		{
			Controller.TouchState touchState = myController.Touches[touchId];
			if (touchState.IsDown)
			{
				Vector2 val = Vector2.op_Implicit(Camera.main.ViewportToWorldPoint(Vector2.op_Implicit(new Vector2(0.5f, 0.5f))));
				Vector2 val2 = touchState.Position - val;
				DeltaL = val2.normalized;
			}
			else
			{
				touchId = -1;
				DeltaL = Vector2.zero;
			}
			return;
		}
		for (int i = 0; i < myController.Touches.Length; i++)
		{
			Controller.TouchState touchState2 = myController.Touches[i];
			if (!touchState2.TouchStart)
			{
				continue;
			}
			bool flag = false;
			int num = Physics2D.OverlapPointNonAlloc(touchState2.Position, hitBuffer, Constants.NotShipMask);
			for (int j = 0; j < num; j++)
			{
				Collider2D val3 = hitBuffer[j];
				if (Object.op_Implicit((Object)(object)((Component)val3).GetComponent<ButtonBehavior>()) || Object.op_Implicit((Object)(object)((Component)val3).GetComponent<PassiveButton>()))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				touchId = i;
				break;
			}
		}
	}
}
