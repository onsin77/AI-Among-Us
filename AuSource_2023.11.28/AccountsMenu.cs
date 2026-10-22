using UnityEngine;

public class AccountsMenu : MonoBehaviour
{
	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void SetValue(string val)
	{
		Close();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
