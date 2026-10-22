using UnityEngine;

public class CreateStoreButton : MonoBehaviour
{
	public GameObject ConnectIcon;

	public void Click()
	{
		DestroyableSingleton<TransitionFade>.Instance.DoTransitionFade(null, ((Component)DestroyableSingleton<StoreMenu>.Instance).gameObject, DestroyableSingleton<StoreMenu>.Instance.Open);
	}
}
