using UnityEngine;

public class TwitchDeepLink : MonoBehaviour
{
	private void OnEnable()
	{
		if (!CanOpenTwitch.CheckUrl("twitch://broadcast?game_id=510218"))
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void OnClick()
	{
		Application.OpenURL("twitch://broadcast?game_id=510218");
	}
}
