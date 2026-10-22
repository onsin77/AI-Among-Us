using UnityEngine;

public class SignIn : MonoBehaviour
{
	[SerializeField]
	private GameObject mainScreen;

	[SerializeField]
	private GameObject signInScreen;

	[SerializeField]
	private GameObject createAccountScreen;

	public void Close()
	{
		((Component)this).GetComponent<TransitionOpen>().Close();
	}

	public void Open()
	{
		((Component)this).GetComponent<TransitionOpen>().OnEnable();
	}
}
