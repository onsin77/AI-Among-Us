using UnityEngine;

public class SignInScreen : MonoBehaviour
{
	public PassiveButton signInButton;

	public bool IsOpen()
	{
		return ((Component)this).gameObject.activeSelf;
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
