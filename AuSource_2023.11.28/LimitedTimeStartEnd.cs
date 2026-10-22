using System;

[Serializable]
public struct LimitedTimeStartEnd
{
	public LimitedTime timeStart;

	public LimitedTime timeEnd;

	public bool IsAvailable()
	{
		if (timeStart.IsNowAfterThisTime())
		{
			return timeEnd.IsNowBeforeThisTime();
		}
		return false;
	}

	public bool IsLimitedTime()
	{
		if (!timeStart.IsLimitedTime())
		{
			return timeEnd.IsLimitedTime();
		}
		return true;
	}

	public bool HasEndDate()
	{
		return timeEnd.IsLimitedTime();
	}

	public bool IsLimitedAndAvailable()
	{
		if (IsLimitedTime())
		{
			return IsAvailable();
		}
		return false;
	}

	public bool HasEndDateAndAvailable()
	{
		if (HasEndDate())
		{
			return IsAvailable();
		}
		return false;
	}
}
