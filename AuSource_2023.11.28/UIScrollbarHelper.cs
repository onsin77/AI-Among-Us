using UnityEngine;

public class UIScrollbarHelper : MonoBehaviour
{
	public int index;

	public bool isDirectChild;

	public Vector2 GetLocalPosition(Scroller scrollRect)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (isDirectChild)
		{
			return Vector2.op_Implicit(((Component)this).transform.localPosition);
		}
		Vector3 position = ((Component)this).transform.position;
		return Vector2.op_Implicit(((Component)scrollRect.Inner).transform.InverseTransformPoint(position));
	}
}
