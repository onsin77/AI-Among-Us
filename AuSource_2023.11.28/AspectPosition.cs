using UnityEngine;

public class AspectPosition : MonoBehaviour
{
	public enum EdgeAlignments
	{
		RightBottom = 6,
		LeftBottom = 5,
		RightTop = 10,
		Left = 1,
		Right = 2,
		Top = 8,
		Bottom = 4,
		LeftTop = 9,
		Center = 16
	}

	public Camera parentCam;

	private const int LeftFlag = 1;

	private const int RightFlag = 2;

	private const int BottomFlag = 4;

	private const int TopFlag = 8;

	private const int CenterFlag = 16;

	public bool updateAlways;

	[Tooltip("Screen Coordinates 0-1")]
	public Vector2 anchorPoint = new Vector2(0.5f, 0.5f);

	public Vector3 DistanceFromEdge;

	public EdgeAlignments Alignment;

	public void Update()
	{
		if (updateAlways)
		{
			AdjustPosition();
		}
	}

	private void OnEnable()
	{
		parentCam = (Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main);
		AdjustPosition();
		ResolutionManager.ResolutionChanged += OnResolutionChanged;
	}

	internal void SetNormalizedX(float nx, float widthPadding)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		float num = CameraSafeArea.GetSafeOrthographicSize(parentCam) * parentCam.aspect + widthPadding;
		float x = DistanceFromEdge.x;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.x = Mathf.Lerp(x, num * 2f - x, nx);
		int alignment = (int)Alignment;
		if ((alignment & 1) != 0)
		{
			localPosition.x -= num;
		}
		else if ((alignment & 2) != 0)
		{
			localPosition.x = num - localPosition.x;
		}
		((Component)this).transform.localPosition = localPosition;
	}

	private void OnDisable()
	{
		ResolutionManager.ResolutionChanged -= OnResolutionChanged;
	}

	public void AdjustPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min((Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main).aspect, safeArea.width / safeArea.height);
		AdjustPosition(aspect);
	}

	private void OnResolutionChanged(float aspectRatio, int width, int height, bool fullscreen)
	{
		AdjustPosition(aspectRatio);
	}

	public void AdjustPosition(float aspect)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Camera cam = (Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main);
		float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(cam);
		if (Alignment == EdgeAlignments.Center)
		{
			((Component)this).transform.localPosition = ComputePositionAnchor(cam, DistanceFromEdge, anchorPoint, aspect);
		}
		else
		{
			((Component)this).transform.localPosition = ComputePosition(Alignment, DistanceFromEdge, safeOrthographicSize, aspect);
		}
	}

	public static Vector3 ComputeWorldPosition(Camera cam, EdgeAlignments alignment, Vector3 relativePos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min(cam.aspect, safeArea.width / safeArea.height);
		float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(cam);
		return ((Component)cam).transform.TransformPoint(ComputePosition(alignment, relativePos, safeOrthographicSize, aspect));
	}

	public static Vector3 ComputePosition(EdgeAlignments alignment, Vector3 relativePos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Rect safeArea = Screen.safeArea;
		Camera main = Camera.main;
		float aspect = Mathf.Min(main.aspect, safeArea.width / safeArea.height);
		float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(main);
		return ComputePosition(alignment, relativePos, safeOrthographicSize, aspect);
	}

	public static Vector3 ComputePosition(EdgeAlignments alignment, Vector3 relativePos, float cHeight, float aspect)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = cHeight * aspect;
		Vector3 val = relativePos;
		if ((alignment & EdgeAlignments.Left) != 0)
		{
			val.x -= num;
		}
		else if ((alignment & EdgeAlignments.Right) != 0)
		{
			val.x = num - val.x;
		}
		if ((alignment & EdgeAlignments.Bottom) != 0)
		{
			val.y -= cHeight;
		}
		else if ((alignment & EdgeAlignments.Top) != 0)
		{
			val.y = cHeight - val.y;
		}
		return val;
	}

	public static Vector3 ComputePositionSafeArea(EdgeAlignments alignment, Vector3 relativePos, float cHeight, float aspect, Camera cam)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		float num = cHeight * aspect;
		Vector3 val = relativePos;
		Rect safeArea;
		if ((alignment & EdgeAlignments.Left) != 0)
		{
			val.x -= num;
			safeArea = Screen.safeArea;
			Vector3 val2 = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, 0f, 0f)) - cam.ScreenToWorldPoint(Vector3.zero);
			val.x += val2.x / 2f;
		}
		else if ((alignment & EdgeAlignments.Right) != 0)
		{
			val.x = num - val.x;
			Vector3 val3 = cam.ScreenToWorldPoint(new Vector3((float)Screen.width, 0f, 0f));
			safeArea = Screen.safeArea;
			Vector3 val2 = val3 - cam.ScreenToWorldPoint(new Vector3(safeArea.xMax, 0f, 0f));
			val.x -= val2.x / 2f;
		}
		if ((alignment & EdgeAlignments.Bottom) != 0)
		{
			val.y -= cHeight;
			safeArea = Screen.safeArea;
			Vector3 val2 = cam.ScreenToWorldPoint(new Vector3(0f, safeArea.yMin, 0f)) - cam.ScreenToWorldPoint(Vector3.zero);
			val.y += val2.y / 2f;
		}
		else if ((alignment & EdgeAlignments.Top) != 0)
		{
			val.y = cHeight - val.y;
			Vector3 val4 = cam.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height, 0f));
			safeArea = Screen.safeArea;
			Vector3 val2 = val4 - cam.ScreenToWorldPoint(new Vector3(0f, safeArea.yMax, 0f));
			val.y += val2.y / 2f;
		}
		return val;
	}

	public Vector3 ComputePositionAnchor(Camera cam, Vector3 relativePos, Vector2 anchor, float aspect)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float num = cam.orthographicSize * 2f;
		float num2 = num * aspect;
		Vector3 val = new Vector3(num2 * anchor.x, num * anchor.y, 0f);
		Vector3 val2 = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
		Vector3 val3 = val + val2 + relativePos;
		val3 -= ((Component)cam).transform.position;
		val3.z = relativePos.z;
		Rect safeArea = Screen.safeArea;
		Vector3 val4 = cam.ScreenToWorldPoint(new Vector3(safeArea.xMin, 0f, 0f)) - cam.ScreenToWorldPoint(Vector3.zero);
		val3.x += val4.x / 2f;
		return val3;
	}

	public AspectPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
