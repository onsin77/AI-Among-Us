using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerAgeData : AbstractSaveDataGroup
{
	public static readonly DateTime DEFAULT_DATE_OF_BIRTH = DateTime.MinValue;

	public static readonly DateTime DEFAULT_LATEST_SET_DATE = DateTime.MinValue;

	[JsonProperty("dateOfBirth")]
	private DateTime dateOfBirth = DEFAULT_DATE_OF_BIRTH;

	[JsonProperty("latestSetDate")]
	private DateTime latestSetDate = DEFAULT_LATEST_SET_DATE;

	public DateTime DateOfBirth
	{
		get
		{
			return dateOfBirth;
		}
		set
		{
			SetValue(ref dateOfBirth, value, OnDateOfBirthChanged);
		}
	}

	public DateTime LatestSetDate
	{
		get
		{
			return latestSetDate;
		}
		set
		{
			SetValue(ref latestSetDate, value, OnLatestSetDateChanged);
		}
	}

	public event Action OnDateOfBirthChanged;

	public event Action OnLatestSetDateChanged;
}
