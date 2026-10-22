using UnityEngine;

public class DragSlot : MonoBehaviour
{
	public Vector3 Offset;

	public Behaviour Occupant;

	public Vector3 TargetPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position + Offset;
		}
	}
}
