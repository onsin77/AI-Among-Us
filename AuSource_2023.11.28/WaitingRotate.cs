using UnityEngine;

public class WaitingRotate : MonoBehaviour
{
	public float speed = 20f;

	public float distanceFromBottom = 0.25f;

	public void Start()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect * 1.1f;
		((Component)this).transform.localPosition = new Vector3(num - 1f, 0f - main.orthographicSize + distanceFromBottom, -1f);
	}

	public void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.RotateAround(((Component)this).transform.position, Vector3.forward, speed * Time.deltaTime);
	}
}
