using UnityEngine;

public class Rotater : MonoBehaviour
{
	public float DegreesPerSecond = 360f;

	private void Update()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localEulerAngles = new Vector3(0f, 0f, Time.time * DegreesPerSecond);
	}
}
