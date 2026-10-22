using UnityEngine;
using UnityEngine.Events;

public class Checkbox : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer checkmark;

	[SerializeField]
	private PassiveButton button;

	private bool oldValue;

	public bool IsChecked
	{
		get
		{
			return ((Renderer)checkmark).enabled;
		}
		set
		{
			((Renderer)checkmark).enabled = value;
		}
	}

	public void Toggle()
	{
		IsChecked = !IsChecked;
	}

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		((UnityEvent)button.OnClick).AddListener((UnityAction)Toggle);
		IsChecked = false;
	}
}
