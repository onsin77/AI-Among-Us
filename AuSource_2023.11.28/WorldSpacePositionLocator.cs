using UnityEngine;

public class WorldSpacePositionLocator : MonoBehaviour
{
	public Vector2 WorldSpacePosition;

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		WorldSpacePosition = Vector2.op_Implicit(((Component)this).transform.position);
	}
}
