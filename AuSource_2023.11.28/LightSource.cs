using System.Collections.Generic;
using AmongUs.Data;
using UnityEngine;

public class LightSource : MonoBehaviour
{
	public const string FLASHLIGHT_KEYWORD = "FLASHLIGHT";

	private const float FLASHLIGHT_LERP_SPEED = 5f;

	private const float FLASHLIGHT_LERP_SPEED_CONSOLE = 10f;

	private const float DEFAULT_PLAYER_RADIUS = 0.5f;

	public static Dictionary<GameObject, NoShadowBehaviour> NoShadows = new Dictionary<GameObject, NoShadowBehaviour>();

	public static Dictionary<GameObject, OneWayShadows> OneWayShadows = new Dictionary<GameObject, OneWayShadows>();

	[SerializeField]
	private float viewDistance = 3f;

	[SerializeField]
	private Material lightCutawayMaterial;

	[SerializeField]
	private Mesh lightChildMesh;

	[SerializeField]
	[Space(10f)]
	private LightSourceRendererType rendererType;

	[SerializeField]
	private bool useFlashlight;

	[Header("GPU Settings")]
	[SerializeField]
	private Material gpuShadowCasterMaterial;

	[SerializeField]
	private int gpuShadowmapResolution = 2048;

	[SerializeField]
	private RenderTextureFormat gpuPreferredRTFormat = (RenderTextureFormat)15;

	[Header("Raycast Settings")]
	[SerializeField]
	private int raycastMinRayCount = 16;

	[SerializeField]
	private float raycastTolerance = 0.001f;

	private LightSourceRenderer renderer;

	private GameObject lightChild;

	private MeshFilter lightChildMeshFilter;

	private Controller controller = new Controller();

	private float flashlightSize = 0.25f;

	private Vector2 lastFlashlightDirection;

	private Vector3 lightOffset;

	private Transform touchFlashlightTarget;

	public float ViewDistance => viewDistance;

	public float PlayerRadius => 0.5f;

	public Material LightCutawayMaterial => lightCutawayMaterial;

	public GameObject LightChild => lightChild;

	public Mesh LightChildMesh => lightChildMesh;

	public MeshFilter LightChildMeshFilter => lightChildMeshFilter;

	public Vector3 LightOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return lightOffset;
		}
	}

	public float FlashlightSize => CalculateFlashlightSize();

	public Material GpuShadowCasterMaterial => gpuShadowCasterMaterial;

	public int GpuShadowmapResolution => gpuShadowmapResolution;

	public RenderTextureFormat GpuPreferredRTFormat
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return gpuPreferredRTFormat;
		}
	}

	public int RaycastMinimumRayCount => raycastMinRayCount;

	public float RaycastTolerance => raycastTolerance;

	private void Awake()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		GameObject val = new GameObject("LightChild");
		val.layer = 10;
		lightChildMeshFilter = val.AddComponent<MeshFilter>();
		lightCutawayMaterial = new Material(lightCutawayMaterial);
		lightCutawayMaterial.DisableKeyword("FLASHLIGHT");
		((Renderer)val.AddComponent<MeshRenderer>()).sharedMaterial = lightCutawayMaterial;
		lightChild = val;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		position.z -= 7f;
		UpdateFlashlightAngle();
		LightCutawayMaterial.SetFloat("_PlayerRadius", PlayerRadius);
		LightCutawayMaterial.SetFloat("_LightRadius", ViewDistance);
		LightCutawayMaterial.SetVector("_LightOffset", Vector4.op_Implicit(LightOffset));
		LightCutawayMaterial.SetFloat("_FlashlightSize", FlashlightSize);
		LightCutawayMaterial.SetFloat("_FlashlightAngle", PlayerControl.LocalPlayer.FlashlightAngle);
		lightChild.transform.position = position;
		renderer.Render(Vector2.op_Implicit(position));
	}

	public void Initialize(Vector3 lightOffset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = lightOffset;
		this.lightOffset = lightOffset;
		UpdateShadowType();
		SetFlashlightEnabled(enabled: false);
	}

	public void SetViewDistance(float distance)
	{
		viewDistance = distance;
	}

	public void SetupLightingForGameplay(bool enableFlashlight, float flashlightSize, Transform touchFlashlightTarget)
	{
		this.flashlightSize = flashlightSize;
		this.touchFlashlightTarget = ((Component)touchFlashlightTarget).transform;
		SetFlashlightEnabled(enableFlashlight);
	}

	private float CalculateFlashlightSize()
	{
		float num = ViewDistance + PlayerRadius / 2f - 0.001f;
		return Mathf.Clamp(flashlightSize * 2f * num, 0f, num);
	}

	private void SetFlashlightEnabled(bool enabled)
	{
		useFlashlight = enabled;
		if (enabled)
		{
			LightCutawayMaterial.EnableKeyword("FLASHLIGHT");
		}
		else
		{
			LightCutawayMaterial.DisableKeyword("FLASHLIGHT");
		}
	}

	private void UpdateShadowType()
	{
		if (rendererType == LightSourceRendererType.Auto)
		{
			rendererType = LightSourceRenderer.GetPlatformType();
		}
		if (renderer == null || renderer.Type != rendererType)
		{
			if (renderer != null)
			{
				renderer.Dispose();
			}
			renderer = LightSourceRenderer.Create(rendererType, this);
			renderer.Initialize();
			SetFlashlightEnabled(useFlashlight);
		}
	}

	private void UpdateFlashlightAngle()
	{
		if ((Object)(object)PlayerControl.LocalPlayer != (Object)null)
		{
			controller.Update();
			PlayerControl.LocalPlayer.FlashlightAngle = GetFlashlightAngle();
		}
	}

	private float GetFlashlightAngle()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		switch (DataManager.Settings.Input.InputMode)
		{
		case ControlTypes.Keyboard:
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
			{
				return GetKeyboardMouseFAngle();
			}
			return GetControllerFAngle();
		case ControlTypes.Controller:
			return GetControllerFAngle();
		case ControlTypes.VirtualJoystick:
			return GetVirtualJFAngle();
		case ControlTypes.ScreenJoystick:
			return GetScreenFAngle();
		default:
		{
			Vector2 velocity = PlayerControl.LocalPlayer.MyPhysics.Velocity;
			Vector2 normalized = velocity.normalized;
			Vector3 val = Vector3.Slerp(Vector2.op_Implicit(lastFlashlightDirection), Vector2.op_Implicit(normalized), Time.deltaTime * 5f);
			lastFlashlightDirection = Vector2.op_Implicit(val);
			return Mathf.Atan2(val.y, val.x);
		}
		}
	}

	private float GetVirtualJFAngle()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			result = GetControllerFAngle();
		}
		else if ((Object)(object)DestroyableSingleton<HudManager>.Instance.joystickR != (Object)null)
		{
			Vector2 deltaR = DestroyableSingleton<HudManager>.Instance.joystickR.DeltaR;
			Vector3 val = Vector3.Slerp(Vector2.op_Implicit(lastFlashlightDirection), Vector2.op_Implicit(deltaR), Time.deltaTime * 10f);
			lastFlashlightDirection = Vector2.op_Implicit(val);
			result = Mathf.Atan2(val.y, val.x);
		}
		return result;
	}

	private float GetKeyboardMouseFAngle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(controller.HoverPosition);
		Vector3 val2 = ((Component)this).transform.position - LightOffset - val;
		Vector3 normalized = val2.normalized;
		return Mathf.Atan2(0f - normalized.y, 0f - normalized.x);
	}

	private float GetControllerFAngle()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((!DestroyableSingleton<HudManager>.InstanceExists || DestroyableSingleton<HudManager>.Instance.joystick == null) ? Vector2.zero : DestroyableSingleton<HudManager>.Instance.joystick.DeltaR);
		Vector3 val2 = Vector3.Slerp(Vector2.op_Implicit(lastFlashlightDirection), Vector2.op_Implicit(val), Time.deltaTime * 10f);
		lastFlashlightDirection = Vector2.op_Implicit(val2);
		return Mathf.Atan2(val2.y, val2.x);
	}

	private float GetScreenFAngle()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		float result;
		if (!((Object)(object)touchFlashlightTarget != (Object)null))
		{
			result = ((ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick) ? Mathf.Atan2(lastFlashlightDirection.y, lastFlashlightDirection.x) : GetControllerFAngle());
		}
		else if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			result = GetControllerFAngle();
			if (((Component)touchFlashlightTarget).gameObject.activeSelf)
			{
				((Component)touchFlashlightTarget).gameObject.SetActive(false);
			}
		}
		else if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
		{
			if (!((Component)touchFlashlightTarget).gameObject.activeSelf)
			{
				((Component)touchFlashlightTarget).gameObject.SetActive(true);
			}
			result = GetAngleDot();
		}
		else
		{
			int num = 0;
			Controller.TouchState[] touches = controller.Touches;
			for (int i = 0; i < touches.Length; i++)
			{
				if (touches[i].IsDown)
				{
					num++;
				}
			}
			if (num > 1)
			{
				if (!((Component)touchFlashlightTarget).gameObject.activeSelf)
				{
					Controller.TouchState touch = controller.GetTouch(1);
					touchFlashlightTarget.position = new Vector3(touch.Position.x, touch.Position.y, touchFlashlightTarget.position.z);
					((Component)touchFlashlightTarget).gameObject.SetActive(true);
				}
				result = GetAngleDot();
			}
			else
			{
				result = GetAngleMove();
				if (((Component)touchFlashlightTarget).gameObject.activeSelf)
				{
					((Component)touchFlashlightTarget).gameObject.SetActive(false);
				}
			}
		}
		return result;
	}

	private float GetAngleDot()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = touchFlashlightTarget.position - ((Component)this).transform.position;
		lastFlashlightDirection = Vector2.op_Implicit(val.normalized);
		return Mathf.Atan2(lastFlashlightDirection.y, lastFlashlightDirection.x);
	}

	private float GetAngleMove()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 velocity = PlayerControl.LocalPlayer.MyPhysics.Velocity;
		Vector2 normalized = velocity.normalized;
		Vector3 val = Vector3.Slerp(Vector2.op_Implicit(lastFlashlightDirection), Vector2.op_Implicit(normalized), Time.deltaTime * 5f);
		lastFlashlightDirection = Vector2.op_Implicit(val);
		return Mathf.Atan2(val.y, val.x);
	}

	public LightSource()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
	}
}
