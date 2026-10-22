using TMPro;
using UnityEngine;

public class PingTracker : MonoBehaviour
{
	public TextMeshPro text;

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)AmongUsClient.Instance))
		{
			if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
			{
				((Component)this).gameObject.SetActive(false);
			}
			((TMP_Text)text).text = $"Ping: {AmongUsClient.Instance.Ping} ms";
		}
	}
}
