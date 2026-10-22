using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlideOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public ButtonClickedEvent OnClose = new ButtonClickedEvent();

	public Camera parentCam;

	public Vector3 openPosition = Vector3.zero;

	public Vector3 closedPosition = Vector3.zero;

	public bool isOpen;

	[SerializeField]
	private GameObject subMenu;

	private Vector3 computedClosedPosition = Vector3.zero;

	public void Awake()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Camera val = (Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main);
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min(val.aspect, safeArea.width / safeArea.height);
		AdjustPosition(aspect);
		if ((Object)(object)subMenu != (Object)null)
		{
			subMenu.SetActive(false);
		}
	}

	public void Toggle()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		if (isOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Close()
	{
		if (isOpen)
		{
			((MonoBehaviour)this).StartCoroutine(AnimateClose());
		}
	}

	public void Open()
	{
		if (!isOpen)
		{
			((MonoBehaviour)this).StartCoroutine(AnimateOpen());
		}
	}

	private void AdjustPosition(float aspect)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Object.op_Implicit((Object)(object)parentCam) ? parentCam : Camera.main);
		computedClosedPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Left, closedPosition, safeOrthographicSize, aspect);
		if (!isOpen)
		{
			((Component)this).transform.localPosition = computedClosedPosition;
		}
	}

	private IEnumerator AnimateClose()
	{
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			Vector3 positionVector = Vector3.Lerp(openPosition, computedClosedPosition, num);
			SetPositionVector(positionVector);
			yield return null;
		}
		if ((Object)(object)subMenu != (Object)null)
		{
			subMenu.SetActive(false);
		}
		SetPositionVector(computedClosedPosition);
		isOpen = false;
	}

	private IEnumerator AnimateOpen()
	{
		if ((Object)(object)subMenu != (Object)null)
		{
			subMenu.SetActive(true);
		}
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float num = t / duration;
			Vector3 positionVector = Vector3.Lerp(computedClosedPosition, openPosition, num);
			SetPositionVector(positionVector);
			yield return null;
		}
		isOpen = true;
		SetPositionVector(openPosition);
	}

	private void SetPositionVector(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = pos;
	}

	private void OnEnable()
	{
		ResolutionManager.ResolutionChanged += OnResolutionChanged;
	}

	private void OnDisable()
	{
		ResolutionManager.ResolutionChanged -= OnResolutionChanged;
	}

	private void OnResolutionChanged(float aspectRatio, int width, int height, bool fullscreen)
	{
		AdjustPosition(aspectRatio);
	}

	public SlideOpen()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
	}
}
