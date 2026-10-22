using System;
using System.Collections;
using UnityEngine;

public class SplashManager : MonoBehaviour
{
	[SerializeField]
	private SplashErrorPopup errorPopup;

	[SerializeField]
	private SceneChanger sceneChanger;

	[SerializeField]
	private float minimumSecondsBeforeSceneChange;

	[SerializeField]
	private LogoAnimFinish logoAnimFinish;

	[SerializeField]
	private GameObject loadingObject;

	private bool doneLoadingRefdata;

	private bool startedSceneLoad;

	private float startTime;

	private void Start()
	{
		startTime = Time.time;
		((MonoBehaviour)this).StartCoroutine(InitializeRefdata());
	}

	private void Update()
	{
		if (doneLoadingRefdata && !startedSceneLoad && Time.time - startTime > minimumSecondsBeforeSceneChange)
		{
			sceneChanger.AllowFinishLoadingScene();
			startedSceneLoad = true;
		}
	}

	private IEnumerator InitializeRefdata()
	{
		LogoAnimFinish logoAnimFinish = this.logoAnimFinish;
		logoAnimFinish.FinishLogoAnim = (Action)Delegate.Combine(logoAnimFinish.FinishLogoAnim, new Action(CheckLoadingRefData));
		yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
		try
		{
			DestroyableSingleton<TranslationController>.Instance.Initialize();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		LogoAnimFinish logoAnimFinish2 = this.logoAnimFinish;
		logoAnimFinish2.FinishLogoAnim = (Action)Delegate.Remove(logoAnimFinish2.FinishLogoAnim, new Action(CheckLoadingRefData));
		loadingObject.SetActive(false);
		sceneChanger.BeginLoadingScene();
		doneLoadingRefdata = true;
		Logger.GlobalInstance.Info("Done initializing refdata");
	}

	private void CheckLoadingRefData()
	{
		if (!doneLoadingRefdata)
		{
			loadingObject.SetActive(true);
		}
	}
}
