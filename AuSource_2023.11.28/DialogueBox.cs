using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
	public TextMeshPro target;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private void OnEnable()
	{
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, BackButton);
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
	}

	public void Show(string dialogue)
	{
		((TMP_Text)target).text = dialogue;
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			Minigame.Instance.Close();
		}
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			Minigame.Instance.Close();
		}
		PlayerControl.LocalPlayer.moveable = false;
		PlayerControl.LocalPlayer.NetTransform.Halt();
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		if (!PlayerControl.LocalPlayer.inVent)
		{
			PlayerControl.LocalPlayer.moveable = true;
		}
		((Component)Camera.main).GetComponent<FollowerCamera>().Locked = false;
	}
}
