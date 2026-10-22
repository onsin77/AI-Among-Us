using UnityEngine;

public class FullAccount : MonoBehaviour
{
	[SerializeField]
	private GameObject randomizeNameButton;

	[SerializeField]
	private GameObject editNameButton;

	[SerializeField]
	private GameObject manageAccount;

	public void CanSetCustomName(bool canSetName)
	{
		if (!canSetName)
		{
			DestroyableSingleton<AccountManager>.Instance.CheckAndRegenerateName();
		}
		randomizeNameButton.SetActive(!canSetName);
		editNameButton.SetActive(canSetName);
	}

	public void UpdateLoggedInAccountVisuals()
	{
		if (DestroyableSingleton<EOSManager>.Instance.IsMinorOrWaiting())
		{
			CanSetCustomName(DestroyableSingleton<AccountManager>.Instance.CanMinorSetCustomDisplayName());
		}
		else
		{
			CanSetCustomName(canSetName: true);
		}
		if (DestroyableSingleton<EOSManager>.Instance.linkedExternalAccounts.Count == 0)
		{
			DestroyableSingleton<PersistentAccountLogger>.Instance.AddStringToLog("We're showing as fully logged in, but have 0 external accounts on PUID " + DestroyableSingleton<EOSManager>.Instance.ProductUserId);
			Debug.LogWarning((object)("We're showing as fully logged in, but have 0 external accounts on PUID " + DestroyableSingleton<EOSManager>.Instance.ProductUserId));
		}
	}
}
