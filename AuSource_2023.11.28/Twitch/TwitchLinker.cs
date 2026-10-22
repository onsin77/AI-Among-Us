using UnityEngine;

namespace Twitch;

public class TwitchLinker : MonoBehaviour
{
	public void LaunchImplicitAuth()
	{
		DestroyableSingleton<TwitchManager>.Instance.LaunchImplicitAuth(((Component)this).transform);
	}
}
