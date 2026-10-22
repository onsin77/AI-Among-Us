using UnityEngine;

public class SnowAttacher : MonoBehaviour
{
	public GameObject SnowPrefab;

	public void Start()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Object.Instantiate<GameObject>(SnowPrefab, ((Component)DestroyableSingleton<HudManager>.Instance).transform).transform.localPosition = new Vector3(0f, 3f, 0f);
	}
}
