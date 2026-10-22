using System.Collections.Generic;
using UnityEngine;

public class TimeOutPopupHandler : MonoBehaviour
{
	public UiElement BackButton;

	public List<UiElement> Buttons;

	public void Retry()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void PlayOffline()
	{
		DestroyableSingleton<EOSManager>.Instance.PlayOffline();
		((Component)this).gameObject.SetActive(false);
	}

	public void Close()
	{
		Retry();
	}
}
