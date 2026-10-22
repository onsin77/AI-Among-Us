using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveDataInitialization : MonoBehaviour
{
	public Texture2D ps4SaveImage;

	public Texture2D ps5SaveImage;

	private void Awake()
	{
	}

	private void Update()
	{
		SceneManager.LoadScene("SplashIntro");
	}
}
