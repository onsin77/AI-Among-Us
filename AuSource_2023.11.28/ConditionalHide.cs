using UnityEngine;

public class ConditionalHide : MonoBehaviour
{
	public RuntimePlatform[] HideForPlatforms = new RuntimePlatform[1] { (RuntimePlatform)2 };

	public RuntimePlatform[] OnlyShowForPlatforms = new RuntimePlatform[0];

	public bool ShowIfXboxTouchEnabled;

	private void Awake()
	{
		Hide();
	}

	public void Hide()
	{
		for (int i = 0; i < HideForPlatforms.Length; i++)
		{
			if ((int)HideForPlatforms[i] == 2)
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < OnlyShowForPlatforms.Length; j++)
		{
			int num = (int)OnlyShowForPlatforms[j];
			bool flag = false;
			if (num == 2)
			{
				flag = true;
			}
			if (flag)
			{
				((Component)this).gameObject.SetActive(true);
				break;
			}
			((Component)this).gameObject.SetActive(false);
		}
	}
}
