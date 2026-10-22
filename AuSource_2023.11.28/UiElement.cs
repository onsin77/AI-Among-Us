using UnityEngine;
using UnityEngine.Events;

public abstract class UiElement : MonoBehaviour
{
	public UnityEvent OnMouseOver;

	public UnityEvent OnMouseOut;

	public ControllerNavigation ControllerNav = new ControllerNavigation
	{
		mode = ControllerNavigation.Mode.Automatic
	};

	public virtual void ReceiveMouseOut()
	{
		OnMouseOut.Invoke();
	}

	public virtual void ReceiveMouseOver()
	{
		OnMouseOver.Invoke();
	}
}
