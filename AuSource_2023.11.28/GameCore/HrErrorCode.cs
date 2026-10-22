using System.Collections.Generic;

namespace GameCore;

public static class HrErrorCode
{
	public enum ErrorCode : uint
	{
		E_GAME_MISSING_GAME_CONFIG = 2279931935u,
		E_GAMERUNTIME_NOT_INITIALIZED = 2300838144u,
		E_GAMERUNTIME_DLL_NOT_FOUND = 2300838145u,
		E_GAMERUNTIME_VERSION_MISMATCH = 2300838146u,
		E_GAMERUNTIME_WINDOW_NOT_FOREGROUND = 2300838147u,
		E_GAMERUNTIME_SUSPENDED = 2300838148u,
		E_GAMERUNTIME_UNINITIALIZE_ACTIVEOBJECTS = 2300838149u,
		E_GAMERUNTIME_MULTIPLAYER_NOT_CONFIGURED = 2300838150u,
		E_GAMEUSER_MAX_USERS_ADDED = 2300858624u,
		E_GAMEUSER_SIGNED_OUT = 2300858625u,
		E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED = 2300858626u,
		E_GAMEUSER_DEFERRAL_NOT_AVAILABLE = 2300858627u,
		E_GAMEUSER_USER_NOT_FOUND = 2300858628u,
		E_GAMEUSER_NO_TOKEN_REQUIRED = 2300858629u,
		E_GAMEUSER_NO_DEFAULT_USER = 2300858630u,
		E_GAMEUSER_FAILED_TO_RESOLVE = 2300858631u,
		E_GAMEUSER_NO_TITLE_ID = 2300858632u,
		E_GAMEUSER_UNKNOWN_GAME_IDENTITY = 2300858633u,
		E_GAMEUSER_NO_PACKAGE_IDENTITY = 2300858640u,
		E_GAMEUSER_FAILED_TO_GET_TOKEN = 2300858641u,
		E_GAMEUSER_INVALID_APP_CONFIGURATION = 2300858642u,
		E_GAMEUSER_MALFORMED_MSAAPPID = 2300858643u,
		E_GAMEUSER_INCONSISTENT_MSAAPPID_AND_TITLEID = 2300858644u,
		E_GAMEPACKAGE_APP_NOT_PACKAGED = 2300858880u,
		E_GAMEPACKAGE_NO_INSTALLED_LANGUAGES = 2300858881u,
		E_GAMEPACKAGE_NO_STORE_ID = 2300858882u,
		E_GAMEPACKAGE_INVALID_SELECTOR = 2300858883u,
		E_GAMEPACKAGE_DOWNLOAD_REQUIRED = 2300858884u,
		E_GAMEPACKAGE_NO_TAG_CHANGE = 2300858885u,
		E_GAMEPACKAGE_DLC_NOT_SUPPORTED = 2300858886u,
		E_GAMEPACKAGE_DUPLICATE_ID_VALUES = 2300858887u,
		E_GAMEPACKAGE_NO_PACKAGE_IDENTIFIER = 2300858888u,
		E_GAMEPACKAGE_CONFIG_NO_ROOT_NODE = 2300858889u,
		E_GAMEPACKAGE_CONFIG_ZERO_VERSION = 2300858890u,
		E_GAMEPACKAGE_CONFIG_NO_MSAAPPID_NOTITLEID = 2300858891u,
		E_GAMEPACKAGE_CONFIG_DEPRECATED_PC_ENTRIES = 2300858892u,
		E_GAMEPACKAGE_CONFIG_SUM_REQUIRES_MSAAPPID = 2300858893u,
		E_GAMEPACKAGE_CONFIG_NO_CODE_CLOUD_SAVES_REQUIRES_MSAAPPID = 2300858894u,
		E_GAMEPACKAGE_CONFIG_MSAAPPID_OR_TITLEID_IS_DEFAULT = 2300858895u,
		E_GAMESTORE_LICENSE_ACTION_NOT_APPLICABLE_TO_PRODUCT = 2300859136u,
		E_GAMESTORE_NETWORK_ERROR = 2300859137u,
		E_GAMESTORE_SERVER_ERROR = 2300859138u,
		E_GAMESTORE_INSUFFICIENT_QUANTITY = 2300859139u,
		E_GAMESTORE_ALREADY_PURCHASED = 2300859140u,
		E_GAMESTREAMING_NOT_INITIALIZED = 2300859392u,
		E_GAMESTREAMING_CLIENT_NOT_CONNECTED = 2300859393u,
		E_GAMESTREAMING_NO_DATA = 2300859394u,
		E_GAMESTREAMING_NO_DATACENTER = 2300859395u,
		E_GAMESTREAMING_NOT_STREAMING_CONTROLLER = 2300859396u
	}

	private static Dictionary<uint, string> ErrorMessages = new Dictionary<uint, string>
	{
		{ 2279931935u, "This app is missing a required MicrosoftGame.Config file." },
		{ 2300838144u, "The game runtime has not yet been initialized." },
		{ 2300838145u, "The game runtime DLL was not found." },
		{ 2300838146u, "The game runtime DLL does not support this version of the GDK." },
		{ 2300838147u, "The operation requires an application window to be in the foreground." },
		{ 2300838148u, "The game runtime is in a suspended state." },
		{ 2300838149u, "The game runtime has active objects at XGameRuntimeUninitialize." },
		{ 2300838150u, "There is no multiplayer activity set on the service for this user on this title." },
		{ 2300858624u, "Can’t add this user because the max number of users has been added." },
		{ 2300858625u, "Can’t perform the operation because the user is signed out." },
		{ 2300858626u, "Needs UI to resolve an issue with this user. In general, if you are getting E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED (and this can happen from multiple APIs), you should call XUserResolveIssueWithUiAsync." },
		{ 2300858627u, "Not an appropriate time to request deferral." },
		{ 2300858628u, "User matching the id was not found." },
		{ 2300858629u, "No token is required for this call." },
		{ 2300858630u, "There is no current default user. If you’re getting this error, it likely means that you called XUserAddAsync(AddDefaultUsersSilently, …). To fix the issue, you should call XUserAddAsync again, this time without AddDefaultUserSilently to get a user." },
		{ 2300858631u, "Failed to resolve the given privilege." },
		{ 2300858632u, "An Xbox live title id must be configured." },
		{ 2300858633u, "The game identity is not recognized. This error happens when the <MSAAppId> and <TitleId> don’t match the ones associated to the game." },
		{ 2300858640u, "A package identity must be configured." },
		{ 2300858641u, "The token request failed." },
		{ 2300858642u, "The MSAAppId in the game config is invalid or is not configured for the authentication stack." },
		{ 2300858643u, "The MSAAppId in the game config doesn’t match an expected format, either a 16 character string or GUID." },
		{ 2300858644u, "The MSAAppId and TitleId in the game config don’t match the ones associated to the game." },
		{ 2300858880u, "The game is not packaged in a container." },
		{ 2300858881u, "The game uses Intelligent Delivery to selectively install languages, but none of the languages are installed." },
		{ 2300858882u, "This API requires that the StoreId attribute be set in the application’s game config." },
		{ 2300858883u, "A chunk selector was provided that does not resolve to a chunk in the package." },
		{ 2300858884u, "One or more required chunks must be downloaded to mount the package on this device." },
		{ 2300858885u, "Change installed chunks in this package using Features, not tags." },
		{ 2300858886u, "The DLC being mounted is not supported by this game. Ensure the correct DLC compatibility is set in the game config." },
		{ 2300858887u, "The package contains more than one executable mapping to the same ‘Id’ attribute. Please check the game config for duplicate ‘Id’ attributes in the ‘Executable’ elements." },
		{ 2300858888u, "Could not resolve an identifier for the package. The package may not be installed." },
		{ 2300858889u, "Could not find a root <Game> node in the MicrosoftGame.config file. Please check the MicrosoftGame.config file to ensure it is properly formatted." },
		{ 2300858890u, "The ‘Version’ attribute in the ‘Identity’ node in the MicrosoftGame.config cannot be 0.0.0.0. Please increment or remove this attribute. If no version is specified, a default value of 1.0.0.0 will be used." },
		{ 2300858891u, "The ‘MSAAppId’ or ‘TitleId’ element in the MicrosoftGame.config is missing. If one of these values is specified, then they must both be specified when using configVersion >= 1. Please use the MicrosoftGame.config Editor “Associate with Store” feature to automatically retrieve Id values from Partner Center." },
		{ 2300858892u, "The DesktopRegistration entries “modFolder”, “enableWritesToPackageRoot”, “disableRegistryWriteVirtualization” and “disableFilesystemWriteVirtualization” are deprecated in configVersion >= 1." },
		{ 2300858893u, "The ‘MSAAppId’ and ‘TitleId’ elements in the MicrosoftGame.config are required when using the Simplified User Model (AdvancedUserModel element) and configVersion >= 1. Please use the MicrosoftGame.config Editor “Associate with Store” feature to automatically retrieve Id values from Partner Center." },
		{ 2300858894u, "The ‘MSAAppId’ and ‘TitleId’ elements in the MicrosoftGame.config are required when using No Code Cloud Saves (NoCodePCRoot element) and configVersion >= 1. Please use the MicrosoftGame.config Editor “Associate with Store” feature to automatically retrieve Id values from Partner Center." },
		{ 2300858895u, "The ‘MSAAppId’ or ‘TitleId’ element in the MicrosoftGame.config is a default value. If NoCodePCRoot or AdvancedUserModel is specified, then the TitleId and MSAAppId must both be valid Ids from Partner Center when using configVersion >= 1. Please use the MicrosoftGame.config Editor “Associate with Store” feature to automatically retrieve Id values from Partner Center." },
		{ 2300859136u, "The game requested a license for a product that cannot be licensed." },
		{ 2300859137u, "The game failed to communicate with the store network." },
		{ 2300859138u, "The game received a bad response from the store server." },
		{ 2300859139u, "The user does not have enough of this consumable to use the requested amount." },
		{ 2300859140u, "The user already owns this product." },
		{ 2300859392u, "The XGameStreaming runtime has not been initialized. Call XGameStreamingInitialize before calling other APIs." },
		{ 2300859393u, "The specified client is not connected." },
		{ 2300859394u, "The requested data is not available. The data may be available later." },
		{ 2300859395u, "The current machine is not running in a datacenter." },
		{ 2300859396u, "The current reading didn’t come from a streaming controller." }
	};

	public static string GetHrErrorCode(int hresult)
	{
		if (ErrorMessages.ContainsKey((uint)hresult))
		{
			return $"{(ErrorCode)hresult} - {ErrorMessages[(uint)hresult]}";
		}
		return $"{hresult} not in Error code list";
	}
}
