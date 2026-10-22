using UnityEngine;

public class ControllerDisconnectHandler : MonoBehaviour
{
	private bool isConnected = true;

	public GameObject ContinueBackground;

	public GameObject ContinueText;

	public GameObject obj;

	[Header("Console Controller Navigation")]
	public UiElement ContinueButton;

	private void Update()
	{
	}

	public void Close()
	{
		Debug.LogError((object)"Controller reconnected");
		isConnected = true;
		if (Object.op_Implicit((Object)(object)ContinueBackground))
		{
			ContinueBackground.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)ContinueText))
		{
			ContinueText.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)obj))
		{
			obj.SetActive(false);
		}
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	private void OnStateChange(uint index, bool newIsConnected)
	{
		if (!newIsConnected && isConnected)
		{
			Debug.LogError((object)"Controller disconnected");
			isConnected = false;
			if (Object.op_Implicit((Object)(object)ContinueBackground))
			{
				ContinueBackground.SetActive(true);
			}
			if (Object.op_Implicit((Object)(object)ContinueText))
			{
				ContinueText.SetActive(true);
			}
			if (Object.op_Implicit((Object)(object)obj))
			{
				obj.SetActive(true);
			}
			ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, ContinueButton, ContinueButton);
		}
	}
}
