using UnityEngine;

public class ArrowBehaviour : PoolableBehavior
{
	public Vector3 target;

	public float perc = 0.925f;

	public float MaxScale = 1f;

	protected float minDistanceToShowArrow = 0.3f;

	[HideInInspector]
	public SpriteRenderer image;

	public new virtual void Awake()
	{
		image = ((Component)this).GetComponent<SpriteRenderer>();
	}

	public void Update()
	{
		UpdatePosition();
	}

	public virtual void UpdatePosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		Vector2 del = Vector2.op_Implicit(target - ((Component)main).transform.position);
		float num = del.magnitude / (main.orthographicSize * perc);
		if ((Object)(object)image != (Object)null)
		{
			((Renderer)image).enabled = num > minDistanceToShowArrow;
		}
		Vector2 val = Vector2.op_Implicit(main.WorldToViewportPoint(target));
		if (Between(val.x, 0f, 1f) && Between(val.y, 0f, 1f))
		{
			CloseBehaviour(del, num);
		}
		else
		{
			DistancedBehaviour(val, del, num, main);
		}
		((Component)this).transform.LookAt2d(target);
	}

	public virtual void SetImageEnabled(bool isOn)
	{
		if ((Object)(object)image != (Object)null)
		{
			((Renderer)image).enabled = isOn;
		}
	}

	protected bool Between(float value, float min, float max)
	{
		if (value > min)
		{
			return value < max;
		}
		return false;
	}

	protected virtual void CloseBehaviour(Vector2 del, float delLen)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector2.op_Implicit(Vector2.op_Implicit(target) - del.normalized * 0.6f);
		float num = Mathf.Clamp(delLen, 0f, MaxScale);
		((Component)this).transform.localScale = new Vector3(num, num, num);
	}

	protected void DistancedBehaviour(Vector2 vpPoint, Vector2 del, float delLen, Camera cam)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(Mathf.Clamp(vpPoint.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vpPoint.y * 2f - 1f, -1f, 1f));
		float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(cam);
		float num = safeOrthographicSize * cam.aspect;
		Vector3 val2 = new Vector3(Mathf.LerpUnclamped(0f, num * 0.88f, val.x), Mathf.LerpUnclamped(0f, safeOrthographicSize * 0.79f, val.y), 0f);
		((Component)this).transform.position = ((Component)cam).transform.position + val2;
		((Component)this).transform.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
	}
}
