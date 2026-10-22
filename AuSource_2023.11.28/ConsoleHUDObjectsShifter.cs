using UnityEngine;

[DefaultExecutionOrder(-10)]
public class ConsoleHUDObjectsShifter : MonoBehaviour
{
	private Camera cam;

	private Vector2 screenBounds;

	private static int instanceCount;

	private void OnEnable()
	{
		instanceCount++;
	}

	private void OnDisable()
	{
		instanceCount--;
		if (instanceCount == 0 && Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
		{
			DestroyableSingleton<HudManager>.Instance.consoleUIHorizontalShift = 0f;
		}
	}

	private void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)DestroyableSingleton<HudManager>.Instance))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)cam))
		{
			cam = Camera.main;
		}
		if (Object.op_Implicit((Object)(object)cam))
		{
			float num = (float)Screen.width / (float)Screen.height;
			screenBounds = new Vector2(num * cam.orthographicSize, cam.orthographicSize);
			Vector3 val = ((Component)this).transform.position - ((Component)cam).transform.position;
			if (val.x > 0f - screenBounds.x)
			{
				DestroyableSingleton<HudManager>.Instance.consoleUIHorizontalShift = val.x - (0f - screenBounds.x);
			}
		}
	}
}
