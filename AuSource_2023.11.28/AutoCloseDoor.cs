using System;

public class AutoCloseDoor : PlainDoor
{
	private const float OpenDuration = 10f;

	private float OpenTime;

	public override void SetDoorway(bool open)
	{
		if (open)
		{
			OpenTime = 10f;
		}
		base.SetDoorway(open);
	}

	public override bool DoUpdate(float dt)
	{
		if (OpenTime > 0f)
		{
			OpenTime = Math.Max(OpenTime - dt, 0f);
			if (OpenTime == 0f)
			{
				SetDoorway(open: false);
				return true;
			}
		}
		return false;
	}
}
