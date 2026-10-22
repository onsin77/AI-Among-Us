using TMPro;
using UnityEngine;

public class VersionShower : MonoBehaviour
{
	public TextMeshPro text;

	public void Start()
	{
		string text = "v" + Application.version;
		text += "s";
		if (!DetectTamper.Detect())
		{
			text += "h";
		}
		if (!string.IsNullOrEmpty("(build num: 3708)"))
		{
			text += " (build num: 3708)";
		}
		((TMP_Text)this.text).text = text;
		Screen.sleepTimeout = -1;
		Debug.Log((object)("Among Us Version " + text + " Pipeline Build Num: " + 3708));
	}
}
