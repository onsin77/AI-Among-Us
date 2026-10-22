using UnityEngine;

public class HoverAnimBehaviour : MonoBehaviour
{
	public FloatRange YMovement;

	public float Speed = 1f;

	public float Shift = 1f;

	private float offset;

	private void Start()
	{
		offset = FloatRange.Next(0f, 10f);
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		float v = Mathf.Sin((Time.time + offset) * Speed) / 2f + Shift;
		localPosition.y = YMovement.Lerp(v);
		((Component)this).transform.localPosition = localPosition;
	}
}
