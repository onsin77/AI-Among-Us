using System.Collections.Generic;

namespace GameCore;

public class UserManager
{
	public enum UserOpResult
	{
		Success,
		NoDefaultUser,
		ResolveUserIssueRequired,
		UnclearedVetoes,
		UnknownError
	}

	private enum State
	{
		Initializing,
		GetContext,
		WaitForAddingUser,
		GetBasicInfo,
		InitializeNetwork,
		GrabAchievements,
		UserDisplayImage,
		ReturnMuteList,
		ReturnAvoidList,
		UserPermissionsCheck,
		WaitForNextTask,
		Error,
		Idle,
		End
	}

	public class UserData
	{
	}

	public delegate void AddUserCompletedDelegate(UserOpResult result);

	private const int E_GAMEUSER_NO_DEFAULT_USER = -1994108666;

	public const bool isSingleUserGame = true;

	public List<UserData> UserDataList = new List<UserData>();

	private UserData primaryUser;

	public UserData PrimaryUser
	{
		get
		{
			return primaryUser;
		}
		private set
		{
		}
	}
}
