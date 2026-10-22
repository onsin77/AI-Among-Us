using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerAccountData : AbstractSaveDataGroup
{
	public const EOSManager.AccountLoginStatus DEFAULT_LOGIN_STATUS = EOSManager.AccountLoginStatus.Offline;

	public const string DEFAULT_GUARDIAN_EMAIL = "";

	public const bool DEFAULT_DELETE_DEVICE_ID_FAILED = false;

	[JsonProperty("loginStatus")]
	private EOSManager.AccountLoginStatus loginStatus;

	[JsonProperty("guardianEmail")]
	private string guardianEmail = "";

	[JsonProperty("deleteDeviceIDFailed")]
	private bool deleteDeviceIDFailed;

	public EOSManager.AccountLoginStatus LoginStatus
	{
		get
		{
			return loginStatus;
		}
		set
		{
			SetValue(ref loginStatus, value, OnLoginStatusChanged);
		}
	}

	public string GuardianEmail
	{
		get
		{
			return guardianEmail;
		}
		set
		{
			SetValue(ref guardianEmail, value, OnGuardianEmailChanged);
		}
	}

	public bool DeleteDeviceIDFailed
	{
		get
		{
			return deleteDeviceIDFailed;
		}
		set
		{
			SetValue(ref deleteDeviceIDFailed, value, OnDeleteDeviceIDFailedChanged);
		}
	}

	public event Action OnLoginStatusChanged;

	public event Action OnGuardianEmailChanged;

	public event Action OnDeleteDeviceIDFailedChanged;
}
