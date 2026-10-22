using System;
using UnityEngine;

public class DraggablePartFixed : MonoBehaviour
{
	[SerializeField]
	private Collider2D collider;

	[SerializeField]
	private Transform targetPoint;

	[SerializeField]
	private GameObject brokenPart;

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private Color finalColor;

	private Vector3 initialPosition;

	private Vector3 initialScale;

	public event Action OnNewPositionSavedEvent;

	public event Action OnPartPositionResetEvent;

	public void Initialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		initialPosition = ((Component)this).transform.localPosition;
		initialScale = ((Component)this).transform.localScale;
	}

	public Collider2D GetCollider()
	{
		return collider;
	}

	public void StartDrag()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector3.one;
	}

	public void SnapIntoPlaceOrReset()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (collider.OverlapPoint(Vector2.op_Implicit(targetPoint.position)) && !brokenPart.activeInHierarchy)
		{
			OnNewPositionSavedEvent?.Invoke();
			((Behaviour)collider).enabled = false;
			((Component)this).transform.localPosition = targetPoint.localPosition;
			sprite.color = finalColor;
		}
		else
		{
			OnPartPositionResetEvent?.Invoke();
			((Component)this).transform.localPosition = initialPosition;
			((Component)this).transform.localScale = initialScale;
		}
	}

	public void UpdatePartPosition(Vector2 dragPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = Vector2.op_Implicit(dragPosition);
		position.z = ((Component)this).transform.position.z;
		((Component)this).transform.position = position;
	}
}
