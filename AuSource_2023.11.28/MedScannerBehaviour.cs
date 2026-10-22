using UnityEngine;

public class MedScannerBehaviour : MonoBehaviour
{
	public Vector3 Offset;

	public Vector3 Position
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
