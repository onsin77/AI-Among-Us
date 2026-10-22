using UnityEngine;

public class GarbageBehaviour : MonoBehaviour
{
	public void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.localPosition.y < -3.49f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
