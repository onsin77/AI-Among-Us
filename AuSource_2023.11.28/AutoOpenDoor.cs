using System;

public class AutoOpenDoor : PlainDoor
{
	public const float CooldownDuration = 30f;

	private const float ClosedDuration = 10f;

	public float ClosedTimer;

	public float CooldownTimer;

	public override void SetDoorway(bool open)
	{
		if (!open)
		{
			ClosedTimer = 10f;
			CooldownTimer = 30f;
		}
		base.SetDoorway(open);
	}

	public override bool DoUpdate(float dt)
	{
		CooldownTimer = Math.Max(CooldownTimer - dt, 0f);
		if (ClosedTimer > 0f)
		{
			ClosedTimer = Math.Max(ClosedTimer - dt, 0f);
			if (ClosedTimer == 0f)
			{
				SetDoorway(open: true);
				return true;
			}
		}
		return false;
	}
}
