using System;
using UnityEngine;

public class DateHide : MonoBehaviour
{
	public int MonthStart;

	public int DayStart;

	public int HourStart;

	public int MonthEnd;

	public int DayEnd;

	public int HourEnd;

	private void Awake()
	{
		try
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = new DateTime(utcNow.Year, MonthStart, DayStart, HourStart, 0, 0);
			DateTime dateTime2 = new DateTime(utcNow.Year, MonthEnd, DayEnd, HourEnd, 0, 0);
			if (dateTime <= utcNow && utcNow <= dateTime2)
			{
				return;
			}
		}
		catch
		{
		}
		((Component)this).gameObject.SetActive(false);
	}
}
