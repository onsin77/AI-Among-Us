using TMPro;
using UnityEngine;

public class PopupDialog : MonoBehaviour
{
	public TextMeshPro workingText;

	public float secondsBetweenDots = 0.7f;

	public int maxDots = 3;

	public string currentProgressText = "";

	private float textUpdateTimer;

	private static PopupDialog instance;

	public static void Display()
	{
		if (!Object.op_Implicit((Object)(object)instance))
		{
			instance = Object.Instantiate<GameObject>(Resources.Load<GameObject>("WaitForConnectionDialog")).GetComponent<PopupDialog>();
		}
	}

	public static void Dispose()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)instance);
		}
	}

	private void Start()
	{
		((Behaviour)ControllerManager.Instance).enabled = false;
	}

	private void OnDestroy()
	{
		((Behaviour)ControllerManager.Instance).enabled = true;
	}

	public void Update()
	{
		textUpdateTimer -= Time.unscaledDeltaTime;
		if (textUpdateTimer <= 0f)
		{
			textUpdateTimer += secondsBetweenDots;
			if (currentProgressText.Length == maxDots)
			{
				currentProgressText = "";
			}
			else
			{
				currentProgressText += ".";
			}
			((TMP_Text)workingText).text = currentProgressText;
		}
	}
}
