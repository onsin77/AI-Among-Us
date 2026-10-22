using UnityEngine;

public class ParallaxChild : MonoBehaviour
{
	[HideInInspector]
	public Vector3 BasePosition;

	public void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		BasePosition = ((Component)this).transform.localPosition;
	}
}
