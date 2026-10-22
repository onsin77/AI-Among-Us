using UnityEngine;

public class PingBehaviour : ArrowBehaviour
{
	[SerializeField]
	private AudioClip soundOnEnable;

	public bool AmSeeker;

	public override void Awake()
	{
		base.Awake();
		minDistanceToShowArrow = 0f;
	}

	public override void UpdatePosition()
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
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		Vector2 del = Vector2.op_Implicit(target - ((Component)main).transform.position);
		float num = del.magnitude / (main.orthographicSize * perc);
		if ((Object)(object)image != (Object)null)
		{
			((Renderer)image).enabled = num > minDistanceToShowArrow;
		}
		Vector2 val = Vector2.op_Implicit(main.WorldToViewportPoint(target));
		if (AmSeeker && Between(val.x, 0f, 1f) && Between(val.y, 0f, 1f))
		{
			CloseBehaviour(del, num);
		}
		else
		{
			DistancedBehaviour(val, del, num, main);
		}
	}

	protected override void CloseBehaviour(Vector2 del, float delLen)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector2.op_Implicit(Vector2.op_Implicit(target));
		float num = Mathf.Clamp(delLen, 0f, MaxScale);
		((Component)this).transform.localScale = new Vector3(num, num, num);
	}

	public override void SetImageEnabled(bool isOn)
	{
		base.SetImageEnabled(isOn);
		if (((Renderer)image).enabled)
		{
			SoundManager.Instance.PlaySound(soundOnEnable, loop: false);
		}
	}
}
