using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
	public void Start()
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void OnClick()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		}
		else
		{
			SceneChanger.ChangeScene("MainMenu");
		}
	}
}
