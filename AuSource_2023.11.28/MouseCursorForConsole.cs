using UnityEngine;

public class MouseCursorForConsole : MonoBehaviour
{
	public float speed = 5f;

	public float acceleration = 1f;

	public Vector2 screenBounds;

	public Vector3 position;

	public Camera cam;

	public static Vector2 currentPosition;

	public static bool buttonDown;

	public static bool joystickMoved;

	public static MouseCursorForConsole instance;

	public static int horizontalAxis = 9;

	public static int verticalAxis = 10;

	private int framesVisible;

	private const int minFramesToAppear = 3;

	private SpriteRenderer sr;

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
		((Renderer)sr).enabled = false;
	}
}
