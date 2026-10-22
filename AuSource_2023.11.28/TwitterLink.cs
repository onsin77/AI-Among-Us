using UnityEngine;

public class TwitterLink : MonoBehaviour
{
	public string LinkUrl = "https://www.twitter.com/InnerslothDevs";

	public void Click()
	{
		Constants.OpenURL(LinkUrl);
	}
}
