using UnityEngine;

public class AutoScroll : MonoBehaviour
{
	public Transform t;

	public float speed;

	public float stoppingPoint;

	public float initialDelay = 2f;

	public bool FinishedScrolling;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		t.localPosition = Vector3.zero;
		initialDelay = 2f;
	}

	private void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		FinishedScrolling = false;
		t.localPosition = Vector3.zero;
		initialDelay = 2f;
	}

	private void Update()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (initialDelay > 0f)
		{
			initialDelay -= Time.deltaTime;
			return;
		}
		if (t.localPosition.y < stoppingPoint)
		{
			t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + Time.deltaTime * speed, t.localPosition.z);
		}
		else
		{
			FinishedScrolling = true;
		}
		if (t.localPosition.y > stoppingPoint)
		{
			t.localPosition = new Vector3(t.localPosition.x, stoppingPoint, t.localPosition.z);
		}
	}
}
