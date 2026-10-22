using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.Xbox;

public class XboxSdk : MonoBehaviour
{
	public delegate void OnGameSaveLoadedHandler(object sender, GameSaveLoadedArgs e);

	public delegate void OnErrorHandler(object sender, ErrorEventArgs e);

	public Text dlcOutputTextBox;

	[Header("You can find the value of the scid in your MicrosoftGame.config")]
	public string scid;

	public Text gamertagLabel;

	public bool signInOnStart = true;

	private static XboxSdk _xboxHelpers;

	private static bool _initialized;

	private const string _GAME_SAVE_CONTAINER_NAME = "x_game_save_default_container";

	private const string _GAME_SAVE_BLOB_NAME = "x_game_save_default_blob";

	public static XboxSdk Helpers
	{
		get
		{
			if ((Object)(object)_xboxHelpers == (Object)null)
			{
				XboxSdk[] array = Object.FindObjectsOfType<XboxSdk>();
				if (array.Length != 0)
				{
					_xboxHelpers = array[0];
					_xboxHelpers._Initialize();
				}
				else
				{
					_LogError("Error: Could not find Xbox prefab. Make sure you have added the Xbox prefab to your scene.");
				}
			}
			return _xboxHelpers;
		}
	}

	public event OnGameSaveLoadedHandler OnGameSaveLoaded;

	public event OnErrorHandler OnError;

	private void Start()
	{
		_Initialize();
	}

	private void _Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
	}

	public void SignIn()
	{
	}

	public void Save(byte[] data)
	{
	}

	public void LoadSaveData()
	{
	}

	public void UnlockAchievement(string achievementId)
	{
	}

	private void Update()
	{
	}

	protected static bool Succeeded(int hresult, string operationFriendlyName)
	{
		bool result = false;
		if (HR.SUCCEEDED(hresult))
		{
			result = true;
		}
		else
		{
			string text = hresult.ToString("X8");
			string text2 = operationFriendlyName + " failed.";
			_LogError($"{text2} Error code: hr=0x{text}");
			if (Helpers.OnError != null)
			{
				Helpers.OnError(Helpers, new ErrorEventArgs(text, text2));
			}
		}
		return result;
	}

	private static void _LogError(string message)
	{
		Debug.Log((object)message);
	}
}
