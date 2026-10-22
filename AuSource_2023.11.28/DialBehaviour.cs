using UnityEngine;

public class DialBehaviour : MonoBehaviour
{
	public FloatRange DialRange;

	public Collider2D collider;

	public Controller myController = new Controller();

	public float Value;

	public bool Engaged;

	public Transform DialTrans;

	public Transform DialShadTrans;

	public void Update()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Engaged = false;
		myController.Update();
		if (myController.CheckDrag(collider) == DragState.Dragging)
		{
			Vector2 vector = myController.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
			float num = Vector2.up.AngleSigned(vector);
			if (num < -180f)
			{
				num += 360f;
			}
			num = DialRange.Clamp(num);
			SetValue(num);
			Engaged = true;
		}
	}

	public void SetValue(float angle)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Value = angle;
		Vector3 localEulerAngles = new Vector3(0f, 0f, angle);
		DialTrans.localEulerAngles = localEulerAngles;
		DialShadTrans.localEulerAngles = localEulerAngles;
	}
}
