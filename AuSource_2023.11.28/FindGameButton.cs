using AmongUs.Data;
using PowerTools;
using UnityEngine;

public class FindGameButton : MonoBehaviour, IConnectButton
{
	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public void OnClick()
	{
		if (!DestroyableSingleton<AccountManager>.Instance.CanPlayOnline())
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.NotAuthorized;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
		}
		else if (StatsManager.Instance.AmBanned)
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
		}
		else
		{
			ContinueClick();
		}
	}

	private void ContinueClick()
	{
		DataManager.Settings.Save();
		if (DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			AmongUsClient.Instance.NetworkMode = NetworkModes.OnlineGame;
			AmongUsClient.Instance.MainMenuScene = "MMOnline";
			((MonoBehaviour)this).StartCoroutine(AmongUsClient.Instance.CoFindGame());
		}
	}

	public void StartIcon()
	{
		connectIcon.Play(connectClip);
	}

	public void StopIcon()
	{
		connectIcon.Stop();
		((Component)connectIcon).GetComponent<SpriteRenderer>().sprite = null;
	}
}
