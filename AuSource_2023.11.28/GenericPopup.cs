using TMPro;
using UnityEngine;

public class GenericPopup : MonoBehaviour
{
	public TextMeshPro TextAreaTMP;

	public bool destroyOnClose;

	public void Show(string text = "")
	{
		if (Object.op_Implicit((Object)(object)TextAreaTMP))
		{
			((TMP_Text)TextAreaTMP).text = text;
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		if (destroyOnClose)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
