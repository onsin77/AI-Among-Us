using UnityEngine;

public class KidAccount : MonoBehaviour
{
	[SerializeField]
	private GameObject signInButton;

	[SerializeField]
	private GameObject createAccountButton;

	[SerializeField]
	private GameObject randomizeNameButton;

	[SerializeField]
	private GameObject editNameButton;

	[SerializeField]
	private GameObject requestPermission;

	[SerializeField]
	private GameObject logOutButton;

	public void CanSetCustomName(bool canSetName)
	{
		randomizeNameButton.SetActive(!canSetName);
		editNameButton.SetActive(canSetName);
	}

	public void CanSignIntoAccount(bool canSignIn)
	{
		signInButton.SetActive(canSignIn);
		createAccountButton.SetActive(false);
		requestPermission.SetActive(!canSignIn);
	}

	public void HasSignedIntoAccount(bool hasSignedIn)
	{
		signInButton.SetActive(!hasSignedIn);
		logOutButton.SetActive(hasSignedIn);
		requestPermission.SetActive(false);
		createAccountButton.SetActive(false);
	}
}
