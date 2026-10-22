using AmongUs.Data;
using UnityEngine;

public class TimeOutableScreen : MonoBehaviour
{
	private float timer;

	private static int screenCount;

	private void OnEnable()
	{
		timer = 0f;
		screenCount++;
	}

	private void OnDisable()
	{
		timer = 0f;
		screenCount--;
		if (screenCount == 0 && Object.op_Implicit((Object)(object)DestroyableSingleton<EOSManager>.Instance))
		{
			DestroyableSingleton<EOSManager>.Instance.HideTimeout();
		}
	}

	private void Update()
	{
		if ((DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline || !DestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow()) && ControllerManager.Instance.IsUiControllerActive && ControllerManager.Instance.CurrentUiState.MenuName == ((Object)((Component)this).gameObject).name && DestroyableSingleton<EOSManager>.Instance.FinishedAssets)
		{
			timer += Time.unscaledDeltaTime;
			if (timer >= DestroyableSingleton<EOSManager>.Instance.TimeOutTime)
			{
				DestroyableSingleton<EOSManager>.Instance.ShowTimeout();
				timer = 0f;
			}
		}
		else
		{
			timer = 0f;
		}
	}
}
