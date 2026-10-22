using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class PetBehaviour : MonoBehaviour
{
	private const float MIN_DISTANCE = 0.2f;

	private const float DAMPING = 0.7f;

	private const float EASING = 0.2f;

	private const float SPEED = 5f;

	private const float SNAP_DISTANCE = 2f;

	[SerializeField]
	private PetData data;

	[Space(10f)]
	[SerializeField]
	private SpriteAnim animator;

	[SerializeField]
	private Rigidbody2D rigidbody;

	[SerializeField]
	private Collider2D collider;

	[Header("Offsets")]
	[SerializeField]
	private float yOffset = -0.1f;

	[SerializeField]
	private Transform pettingHandPosition;

	[Header("Animations")]
	[SerializeField]
	private AnimationClip idleClip;

	[SerializeField]
	private AnimationClip sadClip;

	[SerializeField]
	private AnimationClip scaredClip;

	[SerializeField]
	private AnimationClip walkClip;

	[SerializeField]
	private AnimationClip petClip;

	[Header("Sprite Renderers")]
	[SerializeField]
	private SpriteRenderer[] renderers;

	[SerializeField]
	private SpriteRenderer[] shadows;

	private PlayerControl targetPlayer;

	private bool visible = true;

	private bool flipX;

	private bool beingPet;

	private bool manualMoving;

	public PetData Data => data;

	public PlayerControl TargetPlayer => targetPlayer;

	public Vector3 PettingHandPosition
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)pettingHandPosition != (Object)null))
			{
				return ((Component)this).transform.position;
			}
			return pettingHandPosition.position;
		}
	}

	public Vector3 PettingPlayerPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position + new Vector3(-0.65f, -0.25100002f);
		}
	}

	public int RendererCount => renderers.Length;

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
			ForEachRenderer(includeShadows: true, (SpriteRenderer x) =>
			{
				((Renderer)x).enabled = visible;
			});
		}
	}

	public bool FlipX
	{
		get
		{
			return flipX;
		}
		set
		{
			flipX = value;
			ForEachRenderer(includeShadows: true, (SpriteRenderer x) =>
			{
				x.flipX = flipX;
			});
		}
	}

	public void SetupNewPet(PetData data, Rigidbody2D body, CircleCollider2D collider, SpriteAnim animator, AnimationClip idleClip, AnimationClip scaredClip, AnimationClip walkClip, AnimationClip sadClip, AnimationClip petClip, SpriteRenderer[] renderers, SpriteRenderer[] shadows)
	{
		this.data = data;
		rigidbody = body;
		this.collider = (Collider2D)(object)collider;
		this.animator = animator;
		this.idleClip = idleClip;
		this.scaredClip = scaredClip;
		this.walkClip = walkClip;
		this.sadClip = sadClip;
		this.petClip = petClip;
		this.renderers = renderers;
		this.shadows = shadows;
		((Component)this).gameObject.layer = 8;
	}

	public void SetTargetPlayer(PlayerControl targetPlayer)
	{
		this.targetPlayer = targetPlayer;
	}

	public void SetDefaultMaterial()
	{
		ForEachRenderer(includeShadows: false, (SpriteRenderer x) =>
		{
			if (((Object)((Renderer)x).sharedMaterial.shader).name == "Sprites/Default")
			{
				((Renderer)x).material = DestroyableSingleton<HatManager>.Instance.DefaultShader;
			}
		});
		ForEachShadow((SpriteRenderer x) =>
		{
			((Renderer)x).material = DestroyableSingleton<HatManager>.Instance.DefaultShader;
		});
	}

	public void SetCrewmateColor(int colorId)
	{
		ForEachRenderer(includeShadows: false, (SpriteRenderer x) =>
		{
			PlayerMaterial.SetColors(colorId, (Renderer)(object)x);
		});
	}

	public void SetRawColor(Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		ForEachRenderer(includeShadows: false, (SpriteRenderer x) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			x.color = color;
		});
	}

	public void SetAsLocalPlayer()
	{
		ForEachRenderer(includeShadows: true, (SpriteRenderer x) =>
		{
			PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(x, isLocalPlayer: true);
		});
	}

	public bool HasCrewmateColorShader()
	{
		SpriteRenderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			if (((Object)((Renderer)array[i]).sharedMaterial.shader).name.Contains("PlayerShader"))
			{
				return true;
			}
		}
		return false;
	}

	public void SetIdleOrMourning()
	{
		if ((Object)(object)TargetPlayer != (Object)null && !TargetPlayer.Data.IsDead)
		{
			SetIdle();
		}
		else
		{
			SetMourning();
		}
	}

	public void SetMourning()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		targetPlayer = null;
		rigidbody.velocity = Vector2.zero;
		animator.Play(sadClip);
	}

	public void SetIdle()
	{
		animator.Play(idleClip);
	}

	public void SetScared()
	{
		animator.Play(scaredClip);
	}

	public void PrepareForUI()
	{
		((Behaviour)collider).enabled = false;
		ForEachRenderer(includeShadows: false, (SpriteRenderer x) =>
		{
			((Component)x).gameObject.layer = LayerMask.NameToLayer("UI");
		});
	}

	public void PrepareForPreview(bool previewUsesCrewmateColor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		if (previewUsesCrewmateColor)
		{
			Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
			ForEachRenderer(includeShadows: false, (SpriteRenderer x) =>
			{
				((Renderer)x).sharedMaterial = defaultMaterial;
			});
		}
		else
		{
			SetCrewmateColor(0);
		}
		ForEachShadow((SpriteRenderer x) =>
		{
			((Renderer)x).enabled = false;
		});
	}

	private void Start()
	{
		if (Application.targetFrameRate > 30)
		{
			rigidbody.interpolation = (RigidbodyInterpolation2D)1;
		}
	}

	private void Update()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		if (manualMoving)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)TargetPlayer) || beingPet)
		{
			rigidbody.velocity = Vector2.zero;
			return;
		}
		if (TargetPlayer.Data.IsDead)
		{
			((Component)this).transform.position = Vector2.op_Implicit(TargetPlayer.GetTruePosition());
			SetMourning();
			return;
		}
		Vector2 truePosition = TargetPlayer.GetTruePosition();
		Vector2 truePosition2 = GetTruePosition();
		Vector2 val = rigidbody.velocity;
		Vector2 val2 = truePosition - truePosition2;
		float num = 0f;
		if (TargetPlayer.CanMove)
		{
			num = 0.2f;
		}
		if (val2.sqrMagnitude > num)
		{
			if (val2.sqrMagnitude > 2f)
			{
				((Component)this).transform.position = Vector2.op_Implicit(truePosition);
				return;
			}
			val2 *= 5f * TargetPlayer.MyPhysics.SpeedMod;
			val = val * 0.8f + val2 * 0.2f;
		}
		else
		{
			val *= 0.7f;
		}
		AnimationClip currentAnimation = animator.GetCurrentAnimation();
		if (val.sqrMagnitude > 0.01f)
		{
			if ((Object)(object)currentAnimation != (Object)(object)walkClip)
			{
				animator.Play(walkClip);
			}
			if (val.x < -0.01f)
			{
				FlipX = true;
			}
			else if (val.x > 0.01f)
			{
				FlipX = false;
			}
		}
		else if ((Object)(object)currentAnimation == (Object)(object)walkClip)
		{
			animator.Play(idleClip);
		}
		rigidbody.velocity = val;
	}

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.z = (localPosition.y + yOffset) / 1000f + 0.0002f;
		((Component)this).transform.localPosition = localPosition;
	}

	public void SetGettingPet(bool petting, Vector2 petPos)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		FlipX = true;
		bool flag = beingPet != petting;
		beingPet = petting;
		((Component)this).transform.position = Vector2.op_Implicit(petPos);
		Visible = true;
		manualMoving = false;
		if (!beingPet & flag)
		{
			SetIdleOrMourning();
		}
	}

	public void StartPetAnim()
	{
		animator.Play(petClip);
	}

	private Vector2 GetTruePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(((Component)this).transform.position) + collider.offset * 0.7f;
	}

	private void ForEachRenderer(bool includeShadows, Action<SpriteRenderer> action)
	{
		renderers.ForEach((SpriteRenderer x) =>
		{
			action(x);
		});
		if (includeShadows)
		{
			ForEachShadow(action);
		}
	}

	private void ForEachShadow(Action<SpriteRenderer> action)
	{
		shadows.ForEach((SpriteRenderer x) =>
		{
			action(x);
		});
	}

	public IEnumerator MoveBehindSource(float speed)
	{
		manualMoving = true;
		Vector2 truePosition = TargetPlayer.GetTruePosition();
		truePosition.y += 0.1f;
		yield return WalkPetTo(truePosition, speed);
	}

	private IEnumerator WalkPetTo(Vector2 worldPos, float speed, float tolerance = 0.01f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		manualMoving = true;
		Rigidbody2D body = rigidbody;
		if ((Object)(object)animator.GetCurrentAnimation() != (Object)(object)walkClip)
		{
			animator.Play(walkClip);
		}
		Vector2 del = worldPos - Vector2.op_Implicit(((Component)this).transform.position);
		while (del.sqrMagnitude > tolerance)
		{
			Mathf.Clamp(del.magnitude * 2f, 0.05f, 1f);
			body.velocity = del.normalized * speed;
			yield return null;
			Vector2 velocity = body.velocity;
			if (velocity.magnitude < 0.005f && (double)del.sqrMagnitude < 0.1)
			{
				break;
			}
			del = worldPos - Vector2.op_Implicit(((Component)this).transform.position);
		}
		del = default;
		body.velocity = Vector2.zero;
		manualMoving = false;
	}
}
