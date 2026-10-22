using AmongUs.GameOptions;
using UnityEngine;

public class GameManagerCreator : MonoBehaviour
{
	public NormalGameManager NormalGameManagerPrefab;

	public HideAndSeekManager HideAndSeekManagerPrefab;

	public static GameManagerCreator Instance { get; private set; }

	public static GameManager CreateGameManager(GameModes mode)
	{
		if ((Object)(object)GameManager.Instance != (Object)null)
		{
			Object.Destroy((Object)(object)GameManager.Instance);
		}
		Debug.Log((object)("Creating GameManager: " + mode));
		GameManager result = null;
		switch (mode)
		{
		case GameModes.HideNSeek:
			result = Object.Instantiate<HideAndSeekManager>(Instance.HideAndSeekManagerPrefab);
			break;
		case GameModes.Normal:
			result = Object.Instantiate<NormalGameManager>(Instance.NormalGameManagerPrefab);
			break;
		}
		return result;
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Logger.GlobalInstance.Error("There can be only one game manager creator! Destroying this one...");
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
	}
}
