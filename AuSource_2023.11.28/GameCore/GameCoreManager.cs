using System;
using System.Threading;
using UnityEngine;

namespace GameCore;

public class GameCoreManager : MonoBehaviour
{
	public LocalizationProvider LocalizationProvider;

	public Action OnSuspend;

	public Action<double> OnResume;

	public Action<bool> OnResourceChange;

	private Thread dispatchJob;

	private bool stopExecution;

	public static GameCoreManager Instance { get; private set; }

	public UserManager UserManager { get; private set; }

	public NetworkManager NetworkManager { get; private set; }

	public SaveManager SaveManager { get; private set; }

	public StoreManager StoreManager { get; private set; }

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)this);
	}
}
