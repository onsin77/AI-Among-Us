using TMPro;
using UnityEngine;

public class NumberMenu : MonoBehaviour
{
	public TextMeshPro text;

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void SetValue(string val)
	{
		((TMP_Text)text).text = val;
		Close();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
