using System;
using UnityEngine;

[Serializable]
public struct LimitedTime
{
	public int limitedYear;

	public int limitedMonth;

	public int limitedDay;

	public int limitedHour;

	public int limitedMinute;

	public LimitedTime(int year, int month, int day, int hour, int minute)
	{
		limitedYear = year;
		limitedMonth = month;
		limitedDay = day;
		limitedHour = hour;
		limitedMinute = minute;
	}

	public LimitedTime(DateTime time)
	{
		time = time.ToUniversalTime();
		limitedYear = time.Year;
		limitedMonth = time.Month;
		limitedDay = time.Day;
		limitedHour = time.Hour;
		limitedMinute = time.Minute;
	}

	public bool IsLimitedTime()
	{
		if (limitedYear == 0 && limitedMonth == 0 && limitedDay == 0 && limitedHour == 0)
		{
			return limitedMinute != 0;
		}
		return true;
	}

	public bool IsNowAfterThisTime()
	{
		return IsAfterThisTime(DateTime.UtcNow);
	}

	public bool IsAfterThisTime(DateTime time)
	{
		if (IsLimitedTime())
		{
			return time > GetDateTime();
		}
		return true;
	}

	public bool IsNowBeforeThisTime()
	{
		return IsBeforeThisTime(DateTime.UtcNow);
	}

	public bool IsBeforeThisTime(DateTime time)
	{
		if (IsLimitedTime())
		{
			return time <= GetDateTime();
		}
		return true;
	}

	public string TimeLeftFromNow()
	{
		return TimeLeft(DateTime.UtcNow);
	}

	public string TimeLeft(DateTime time)
	{
		if (!IsLimitedTime())
		{
			return "";
		}
		TimeSpan timeSpan = GetDateTime() - time;
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AvailableFor, timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}

	public string TimeLeftFromNowShort()
	{
		return TimeLeftShort(DateTime.UtcNow);
	}

	public string TimeLeftShort(DateTime time)
	{
		if (!IsLimitedTime())
		{
			return "";
		}
		TimeSpan timeSpan = GetDateTime() - time;
		if (timeSpan.Days > 0)
		{
			return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AbbreviatedDay, timeSpan.Days);
		}
		if (timeSpan.Hours > 0)
		{
			return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AbbreviatedHour, timeSpan.Hours);
		}
		if (timeSpan.Minutes > 0)
		{
			return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AbbreviatedMinute, timeSpan.Minutes);
		}
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AbbreviatedSecond, timeSpan.Seconds);
	}

	public DateTime GetDateTime()
	{
		DateTime utcNow = DateTime.UtcNow;
		int num = ((limitedYear == 0) ? utcNow.Year : limitedYear);
		num = Mathf.Clamp(num, 1, 9999);
		int num2 = ((limitedMonth == 0) ? 1 : limitedMonth);
		num2 = Mathf.Clamp(num2, 1, 12);
		int num3 = ((limitedDay == 0) ? 1 : limitedDay);
		num3 = Mathf.Clamp(num3, 1, DateTime.DaysInMonth(num, num2));
		int hour = Mathf.Clamp(limitedHour, 0, 23);
		int minute = Mathf.Clamp(limitedMinute, 0, 59);
		return new DateTime(num, num2, num3, hour, minute, 0, DateTimeKind.Utc);
	}
}
