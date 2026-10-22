using UnityEngine;

public class ShiftButtonsCrossplayEnabled : MonoBehaviour
{
	[SerializeField]
	private GameObject crossplayObj;

	[SerializeField]
	private GameObject chatTypeHelpMenu;

	private const float shiftAmt = 1.11f;

	private void Start()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!crossplayObj.activeSelf)
		{
			((Component)this).gameObject.transform.localPosition = new Vector3(1.11f, 0f, 0f);
			Vector3 localPosition = chatTypeHelpMenu.transform.localPosition;
			localPosition.x -= 1.8499999f;
			chatTypeHelpMenu.transform.localPosition = localPosition;
		}
	}
}
