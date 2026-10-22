using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.Demos;

[RequireComponent(typeof(Image))]
[AddComponentMenu("")]
public class TouchJoystickExample : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	public bool allowMouseControl = true;

	public int radius = 50;

	private Vector2 origAnchoredPosition;

	private Vector3 origWorldPosition;

	private Vector2 origScreenResolution;

	private ScreenOrientation origScreenOrientation;

	[NonSerialized]
	private bool hasFinger;

	[NonSerialized]
	private int lastFingerId;

	public Vector2 position
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

	private void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)SystemInfo.deviceType == 1)
		{
			allowMouseControl = false;
		}
		StoreOrigValues();
	}

	private void Update()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((float)Screen.width != origScreenResolution.x || (float)Screen.height != origScreenResolution.y || Screen.orientation != origScreenOrientation)
		{
			Restart();
			StoreOrigValues();
		}
	}

	private void Restart()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		hasFinger = false;
		Transform transform = ((Component)this).transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = origAnchoredPosition;
		position = Vector2.zero;
	}

	private void StoreOrigValues()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		origAnchoredPosition = ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		origWorldPosition = ((Component)this).transform.position;
		origScreenResolution = new Vector2((float)Screen.width, (float)Screen.height);
		origScreenOrientation = Screen.orientation;
	}

	private void UpdateValue(Vector3 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = origWorldPosition - value;
		val.y = 0f - val.y;
		val /= (float)radius;
		position = new Vector2(0f - val.x, val.y);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (!hasFinger && (allowMouseControl || !IsMousePointerId(eventData.pointerId)))
		{
			hasFinger = true;
			lastFingerId = eventData.pointerId;
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (eventData.pointerId == lastFingerId && (allowMouseControl || !IsMousePointerId(eventData.pointerId)))
		{
			Restart();
		}
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (hasFinger && eventData.pointerId == lastFingerId)
		{
			Vector3 val = new Vector3(eventData.position.x - origWorldPosition.x, eventData.position.y - origWorldPosition.y);
			val = Vector3.ClampMagnitude(val, (float)radius);
			Vector3 value = origWorldPosition + val;
			((Component)this).transform.position = value;
			UpdateValue(value);
		}
	}

	private static bool IsMousePointerId(int id)
	{
		if (id != -1 && id != -2)
		{
			return id == -3;
		}
		return true;
	}
}
