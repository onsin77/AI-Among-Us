using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.Demos;

[RequireComponent(typeof(Image))]
[AddComponentMenu("")]
public class TouchButtonExample : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public bool allowMouseControl = true;

	public bool isPressed { get; private set; }

	private void Awake()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)SystemInfo.deviceType == 1)
		{
			allowMouseControl = false;
		}
	}

	private void Restart()
	{
		isPressed = false;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (allowMouseControl || !IsMousePointerId(eventData.pointerId))
		{
			isPressed = true;
		}
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (allowMouseControl || !IsMousePointerId(eventData.pointerId))
		{
			isPressed = false;
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
