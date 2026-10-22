using UnityEngine;

public class WorldSpaceDistance : MonoBehaviour
{
	public GameObject TargetObj;

	public float Distance;

	public float DistanceSq;

	private void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TargetObj == (Object)null))
		{
			Distance = Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(TargetObj.transform.position));
			DistanceSq = Distance * Distance;
		}
	}
}
