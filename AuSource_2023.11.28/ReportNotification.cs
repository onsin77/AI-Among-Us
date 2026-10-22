using UnityEngine;

public class ReportNotification : MonoBehaviour
{
	private float timeOnScreen;

	private void Awake()
	{
		timeOnScreen = 8f;
	}

	private void Update()
	{
		timeOnScreen -= Time.deltaTime;
		if (timeOnScreen <= 0f)
		{
			Close();
		}
	}

	public void Close()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
