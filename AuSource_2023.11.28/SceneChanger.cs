using AmongUs.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
	public static class SceneManagerCallbacks
	{
		static SceneManagerCallbacks()
		{
			Debug.Log((object)"Hooked up SceneManager callback");
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
		}

		private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			if ((int)mode == 1)
			{
				SceneManager.SetActiveScene(scene);
			}
		}

		public static void Init()
		{
		}
	}

	public string TargetScene;

	public bool disallowBasedOnSwitchParentalControls;

	public bool disallowBasedOnAssetPackDownloads;

	public GameObject ConnectIcon;

	public ButtonClickedEvent BeforeSceneChange;

	private AsyncOperation loadOp;

	public void Click()
	{
		if (AllowSceneChange())
		{
			ChangeScene();
		}
	}

	private void ChangeScene()
	{
		Debug.Log((object)("SceneChanger::ChangeScene to: " + TargetScene));
		((UnityEvent)BeforeSceneChange).Invoke();
		ChangeScene(TargetScene);
	}

	public static void ChangeScene(string target)
	{
		SceneManager.LoadScene(target);
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void BeginLoadingScene()
	{
		BeginLoadingSceneInternal((LoadSceneMode)0);
	}

	public void BeginLoadingSceneAdditive()
	{
		BeginLoadingSceneInternal((LoadSceneMode)1);
	}

	private void BeginLoadingSceneInternal(LoadSceneMode mode)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		SceneManagerCallbacks.Init();
		if (loadOp == null)
		{
			Debug.Log((object)("Begin async loading " + TargetScene));
			loadOp = SceneManager.LoadSceneAsync(TargetScene, mode);
			loadOp.allowSceneActivation = false;
		}
	}

	public void AllowFinishLoadingScene()
	{
		if (loadOp != null)
		{
			loadOp.allowSceneActivation = true;
			loadOp = null;
		}
	}

	private bool AllowSceneChange()
	{
		return CheckHowToPlay();
	}

	private bool CheckHowToPlay()
	{
		if (!DataManager.Player.Onboarding.ViewedHideAndSeekHowToPlay)
		{
			HowToPlayScene.SceneOnClose = TargetScene;
			SceneManager.LoadScene("HowToPlay");
			return false;
		}
		return true;
	}
}
