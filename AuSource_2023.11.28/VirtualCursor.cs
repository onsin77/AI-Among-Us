using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;

public class VirtualCursor : MonoBehaviour
{
	public float speed = 5f;

	private float currentSpeed;

	public float acceleration = 1f;

	public Vector2 screenBounds;

	public Vector3 position;

	private const float deadzone = 0.03f;

	private const float touchpadSensitivity = 4f;

	public Camera cam;

	public static Vector2 currentPosition;

	public static bool buttonDown;

	public static bool joystickMoved;

	public static VirtualCursor instance;

	public static int horizontalAxis = 9;

	public static int verticalAxis = 10;

	private int framesVisible;

	private const int minFramesToAppear = 3;

	private SpriteRenderer sr;

	private Vector2 prevTouchPos;

	private bool setTouchPos;

	public static bool CursorActive
	{
		get
		{
			if (Object.op_Implicit((Object)(object)instance) && ((Behaviour)instance).isActiveAndEnabled)
			{
				return SpecialInputHandler.disableVirtualCursorCount == 0;
			}
			return false;
		}
	}

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		sr = ((Component)this).GetComponentInChildren<SpriteRenderer>(true);
	}

	private void OnEnable()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		cam = Camera.main;
		if (Object.op_Implicit((Object)(object)cam))
		{
			SetScreenPosition(Vector2.zero);
		}
		framesVisible = 0;
		((Renderer)sr).enabled = false;
	}

	private void OnDestroy()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		cam = Camera.main;
		position = ((Component)this).transform.position;
		if (Object.op_Implicit((Object)(object)cam))
		{
			float num = (float)Screen.width / (float)Screen.height;
			screenBounds = new Vector2(num * cam.orthographicSize, cam.orthographicSize);
		}
	}

	public void SetWorldPosition(Vector2 worldPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (((Component)this).transform.position = Vector2.op_Implicit(worldPos));
		currentPosition = Vector2.op_Implicit(val);
		position = Vector2.op_Implicit(worldPos - new Vector2(((Component)cam).transform.position.x, ((Component)cam).transform.position.y));
	}

	public void SetScreenPosition(Vector2 screenPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		position = Vector2.op_Implicit(screenPos);
		Vector3 val = (((Component)this).transform.position = position + ((Component)cam).transform.position);
		currentPosition = Vector2.op_Implicit(val);
	}

	private void Update()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		Vector3 val = new Vector3(player.GetAxis(horizontalAxis), player.GetAxis(verticalAxis), 0f);
		joystickMoved = val.x != 0f || val.y != 0f;
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && SpecialInputHandler.count == 0)
		{
			if (framesVisible < 3)
			{
				framesVisible++;
			}
			else
			{
				((Renderer)sr).enabled = true;
			}
		}
		else
		{
			framesVisible = 0;
			((Renderer)sr).enabled = false;
		}
		if (ControllerManager.Instance.IsUiControllerActive)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		if (player.controllers.joystickCount > 0)
		{
			IDualShock4Extension extension = ((Controller)player.controllers.Joysticks[0]).GetExtension<IDualShock4Extension>();
			if (extension != null && extension.touchCount > 0)
			{
				Vector2 val2 = default;
				extension.GetTouchPosition(0, ref val2);
				float num = (float)Screen.width / (float)Screen.height;
				if (setTouchPos)
				{
					Vector2 val3 = val2 - prevTouchPos;
					position.x += val3.x * (4f * num);
					position.y += val3.y * 4f;
				}
				setTouchPos = true;
				prevTouchPos = val2;
			}
			else
			{
				setTouchPos = false;
			}
		}
		if (val.magnitude > 0.03f)
		{
			currentSpeed += Time.deltaTime * acceleration;
			if (currentSpeed > speed)
			{
				currentSpeed = speed;
			}
			position += val * Time.deltaTime * currentSpeed;
		}
		else
		{
			currentSpeed = 0f;
		}
		if (!Object.op_Implicit((Object)(object)cam))
		{
			cam = Camera.main;
		}
		if (Object.op_Implicit((Object)(object)cam))
		{
			float num2 = (float)Screen.width / (float)Screen.height;
			screenBounds = new Vector2(num2 * cam.orthographicSize, cam.orthographicSize);
			position.x = Mathf.Clamp(position.x, 0f - screenBounds.x, screenBounds.x);
			position.y = Mathf.Clamp(position.y, 0f - screenBounds.y, screenBounds.y);
			Vector3 val4 = (((Component)this).transform.position = position + ((Component)cam).transform.position);
			currentPosition = Vector2.op_Implicit(val4);
		}
		buttonDown = player.GetButton(11);
	}
}
