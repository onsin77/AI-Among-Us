using UnityEngine;

public class AspectSpacer : MonoBehaviour
{
	public float xSpacing;

	public float defaultAspectRatio = 1.7777778f;

	public bool spaceWiderAspectRatios;

	public void OnEnable()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if (main.aspect > defaultAspectRatio && !spaceWiderAspectRatios)
		{
			return;
		}
		float num = xSpacing * (main.aspect / defaultAspectRatio);
		float num2 = Mathf.Ceil((float)(-((Component)this).transform.childCount) / 2f) * num;
		foreach (Transform item in ((Component)this).transform)
		{
			Transform val = item;
			val.localPosition = new Vector3(num2 + (float)val.GetSiblingIndex() * num, val.localPosition.y, val.localPosition.z);
		}
	}
}
