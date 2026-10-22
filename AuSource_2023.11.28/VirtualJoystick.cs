using System.Runtime.CompilerServices;
using UnityEngine;

public class VirtualJoystick : MonoBehaviour, IVirtualJoystick
{
	public float InnerRadius = 0.64f;

	public float OuterRadius = 1.28f;

	public CircleCollider2D Outer;

	public SpriteRenderer Inner;

	public SpriteRenderer[] AllSprites;

	public bool IsDragged;

	public bool IsRightJoystick;

	private VirtualJoystickController myController = new VirtualJoystickController();

	public Vector2 DeltaL
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Vector2 DeltaR
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public void ToggleVisuals(bool on)
	{
		AllSprites.ForEach((SpriteRenderer s) =>
		{
			((Renderer)s).enabled = on;
		});
	}

	protected virtual void FixedUpdate()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		myController.Update();
		switch (myController.CheckDrag((Collider2D)(object)Outer, IsRightJoystick ? DestroyableSingleton<HudManager>.Instance.RightStickDeadZone : DestroyableSingleton<HudManager>.Instance.LeftStickDeadZone))
		{
		case DragState.TouchStart:
		case DragState.Dragging:
		{
			float num = OuterRadius - InnerRadius;
			Vector2 val = myController.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
			_ = val.magnitude;
			Vector2 val2 = new Vector2(Mathf.Sqrt(Mathf.Abs(val.x)) * Mathf.Sign(val.x), Mathf.Sqrt(Mathf.Abs(val.y)) * Mathf.Sign(val.y));
			if (IsRightJoystick)
			{
				DeltaR = Vector2.ClampMagnitude(val2 / OuterRadius, 1f);
			}
			else
			{
				DeltaL = Vector2.ClampMagnitude(val2 / OuterRadius, 1f);
			}
			((Component)Inner).transform.localPosition = Vector3.ClampMagnitude(Vector2.op_Implicit(val), num) + Vector3.back;
			IsDragged = true;
			break;
		}
		case DragState.Released:
			if (IsRightJoystick)
			{
				DeltaR = Vector2.zero;
			}
			else
			{
				DeltaL = Vector2.zero;
			}
			((Component)Inner).transform.localPosition = Vector3.back;
			IsDragged = false;
			break;
		case DragState.Holding:
			break;
		}
	}

	public virtual void UpdateJoystick(HowToPlaySceneMovementPageFingerBehaviour finger, Vector2 velocity, bool syncFinger)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)Inner).transform.localPosition;
		Vector3 val = Vector2.op_Implicit(velocity.normalized * InnerRadius);
		val.z = localPosition.z;
		if (syncFinger)
		{
			localPosition = Vector3.Lerp(localPosition, val, Time.fixedDeltaTime * 5f);
			((Component)Inner).transform.localPosition = localPosition;
			localPosition = ((Component)Inner).transform.position;
			localPosition.z = -26f;
			((Component)finger).transform.position = localPosition;
		}
		else if ((Object)(object)((Component)Inner).gameObject != (Object)(object)((Component)finger).gameObject)
		{
			((Component)Inner).transform.localPosition = val;
		}
	}
}
