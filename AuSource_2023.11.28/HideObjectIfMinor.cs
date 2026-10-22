using System;
using AmongUs.Data;
using UnityEngine;

public class HideObjectIfMinor : MonoBehaviour
{
	private void Awake()
	{
		AccountManager instance = DestroyableSingleton<AccountManager>.Instance;
		instance.OnLoggedInStatusChange = (Action)Delegate.Combine(instance.OnLoggedInStatusChange, new Action(UpdateVisual));
	}

	private void OnDestroy()
	{
		AccountManager instance = DestroyableSingleton<AccountManager>.Instance;
		instance.OnLoggedInStatusChange = (Action)Delegate.Remove(instance.OnLoggedInStatusChange, new Action(UpdateVisual));
	}

	private void Start()
	{
		UpdateVisual();
	}

	private void OnEnable()
	{
		UpdateVisual();
	}

	public void UpdateVisual()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsMinorOrWaiting() || DataManager.Player.Account.LoginStatus == EOSManager.AccountLoginStatus.Offline)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).gameObject.SetActive(true);
		}
	}
}
