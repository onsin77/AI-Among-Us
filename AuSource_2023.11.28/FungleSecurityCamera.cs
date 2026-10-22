using System;
using UnityEngine;

public class FungleSecurityCamera : MonoBehaviour
{
	public Action<Collision2D> OnCollision;

	public Action<Collision2D> OnExitCollision;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private Rigidbody2D rigidbody;

	public RenderTexture RenderTexture
	{
		set
		{
			cam.targetTexture = value;
		}
	}

	public float OrthographicSize
	{
		set
		{
			cam.orthographicSize = value;
		}
	}

	public bool CollisionsEnabled
	{
		get
		{
			return !rigidbody.isKinematic;
		}
		set
		{
			rigidbody.isKinematic = !value;
		}
	}

	public void Move(Vector3 delta)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (CollisionsEnabled)
		{
			rigidbody.MovePosition(Vector2.op_Implicit(((Component)this).transform.position + delta));
		}
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		OnCollision(col);
	}

	private void OnCollisionStay2D(Collision2D col)
	{
		OnCollision(col);
	}

	private void OnCollisionExit2D(Collision2D col)
	{
		OnExitCollision(col);
	}
}
