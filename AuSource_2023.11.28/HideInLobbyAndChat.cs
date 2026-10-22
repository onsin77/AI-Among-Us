using UnityEngine;

public class HideInLobbyAndChat : MonoBehaviour
{
	public GameObject ObjectToHide;

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			ObjectToHide.SetActive(false);
		}
		else
		{
			ObjectToHide.SetActive(true);
		}
		if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening)
		{
			ObjectToHide.SetActive(false);
		}
	}
}
