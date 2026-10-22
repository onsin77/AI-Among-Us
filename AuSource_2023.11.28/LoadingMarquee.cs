using UnityEngine;

public class LoadingMarquee : MonoBehaviour
{
	public float duration = 5f;

	public float distanceFromBottom = 0.25f;

	private FloatRange XRange;

	private float timer;

	public void Start()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect * 1.1f;
		XRange = new FloatRange(num, 0f - num);
		((Component)this).transform.localPosition = new Vector3(num, 0f - main.orthographicSize + distanceFromBottom, -1f);
	}

	public void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.x = XRange.Lerp(timer / duration % 1f);
		((Component)this).transform.localPosition = localPosition;
	}
}
