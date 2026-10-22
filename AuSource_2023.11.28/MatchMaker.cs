using InnerNet;
using UnityEngine;

public class MatchMaker : DestroyableSingleton<MatchMaker>
{
	public TextBoxTMP GameIdText;

	private MonoBehaviour Connecter;

	public void Start()
	{
		if (Object.op_Implicit((Object)(object)GameIdText) && Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			GameIdText.SetText(GameCode.IntToGameName(AmongUsClient.Instance.GameId) ?? "");
		}
	}

	public bool Connecting<T>(T button) where T : MonoBehaviour, IConnectButton
	{
		if (!Object.op_Implicit((Object)(object)Connecter))
		{
			Connecter = (MonoBehaviour)(object)button;
			((IConnectButton)Connecter).StartIcon();
			return true;
		}
		((MonoBehaviour)this).StartCoroutine(Effects.SwayX(((Component)Connecter).transform));
		return false;
	}

	public void NotConnecting()
	{
		if (Object.op_Implicit((Object)(object)Connecter))
		{
			((IConnectButton)Connecter).StopIcon();
			Connecter = null;
		}
	}
}
