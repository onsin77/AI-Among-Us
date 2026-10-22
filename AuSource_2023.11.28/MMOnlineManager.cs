using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;

public class MMOnlineManager : DestroyableSingleton<MMOnlineManager>
{
	public GameObject HelpMenu;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public bool IsControllerManagerSceneInit;

	public void Start()
	{
		ControllerManager.Instance.NewScene(((Object)this).name, BackButton, DefaultButtonSelected, ControllerSelectable);
		IsControllerManagerSceneInit = true;
		if (Object.op_Implicit((Object)(object)VirtualCursor.instance))
		{
			((Component)VirtualCursor.instance).gameObject.SetActive(false);
		}
		if ((Object)(object)HelpMenu != (Object)null)
		{
			if (!DataManager.Player.Onboarding.ViewedOnlineHelpMessage)
			{
				DataManager.Player.Onboarding.ViewedOnlineHelpMessage = true;
				DataManager.Player.Save();
				HelpMenu.gameObject.SetActive(true);
			}
			else
			{
				HelpMenu.gameObject.SetActive(false);
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp((KeyCode)27))
		{
			SceneChanger.ChangeScene("MainMenu");
		}
	}
}
