using UnityEngine;

public class WaitForHostPopup : DestroyableSingleton<WaitForHostPopup>
{
	public GameObject Content;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public void Show()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance) && AmongUsClient.Instance.ClientId > 0)
		{
			Content.SetActive(true);
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, DefaultButtonSelected);
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		Content.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Hide()
	{
		Content.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}
}
