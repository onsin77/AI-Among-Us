using System.Collections;
using Innersloth.DebugTool;
using UnityEngine;

public class KeyboardDebugToolInputProvider : IDebugToolInputProvider
{
	private const float VERTICAL_REPEAT_RATE = 0.25f;

	private float keyDownStartTime;

	private float keyUpStartTime;

	public IEnumerator CoInitialize()
	{
		yield return null;
	}

	public bool GetToggle()
	{
		return Input.GetKeyUp((KeyCode)96);
	}

	public float GetVertical()
	{
		if (CheckKeyPress((KeyCode)274, ref keyDownStartTime))
		{
			return -1f;
		}
		if (CheckKeyPress((KeyCode)273, ref keyUpStartTime))
		{
			return 1f;
		}
		return 0f;
	}

	public bool GetCopy()
	{
		if (Input.GetKey((KeyCode)306))
		{
			return Input.GetKeyUp((KeyCode)99);
		}
		return false;
	}

	public bool GetClear()
	{
		return Input.GetKeyUp((KeyCode)127);
	}

	public bool GetConfirm()
	{
		return Input.GetKeyUp((KeyCode)13);
	}

	public bool GetCancel()
	{
		return Input.GetKeyUp((KeyCode)27);
	}

	public bool GetNavLeft()
	{
		if (Input.GetKey((KeyCode)304))
		{
			return Input.GetKeyUp((KeyCode)9);
		}
		return false;
	}

	public bool GetNavRight()
	{
		if (!Input.GetKey((KeyCode)304))
		{
			return Input.GetKeyUp((KeyCode)9);
		}
		return false;
	}

	private static bool CheckKeyPress(KeyCode key, ref float startTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(key))
		{
			startTime = Time.time;
			return true;
		}
		if (startTime > 0f)
		{
			if (Input.GetKey(key))
			{
				if (Time.time - startTime > 0.25f)
				{
					startTime = Time.time;
					return true;
				}
			}
			else
			{
				startTime = -1f;
			}
		}
		return false;
	}
}
