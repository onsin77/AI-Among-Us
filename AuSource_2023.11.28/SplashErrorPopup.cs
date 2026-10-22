using TMPro;
using UnityEngine;

public class SplashErrorPopup : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro TitleText;

	[SerializeField]
	private TextMeshPro InfoText;

	[SerializeField]
	private AudioSource ShowSFX;

	public bool IsShowing => ((Component)this).gameObject.activeSelf;

	public void Show(string titleText, string infoText)
	{
		if (!IsShowing)
		{
			((TMP_Text)TitleText).text = titleText;
			((TMP_Text)InfoText).text = infoText;
			((Component)this).gameObject.SetActive(true);
			ShowSFX.Play();
		}
	}

	public void OnButtonPressed()
	{
		Application.Quit();
	}
}
