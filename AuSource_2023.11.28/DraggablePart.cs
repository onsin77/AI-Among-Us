using System;
using UnityEngine;

public class DraggablePart : MonoBehaviour
{
	[SerializeField]
	private Collider2D collider;

	[SerializeField]
	private SpriteRenderer targetSprite;

	private Vector3 initialPosition;

	public event Action OnNewPositionSavedEvent;

	public event Action OnPartPositionResetEvent;

	public void Initialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		initialPosition = ((Component)this).transform.localPosition;
	}

	public Collider2D GetCollider()
	{
		return collider;
	}

	public void SnapIntoPlaceOrReset()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (collider.OverlapPoint(Vector2.op_Implicit(((Component)targetSprite).transform.position)))
		{
			OnNewPositionSavedEvent?.Invoke();
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			OnPartPositionResetEvent?.Invoke();
			((Component)this).transform.localPosition = initialPosition;
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
