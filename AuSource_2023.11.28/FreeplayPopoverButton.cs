using System;
using UnityEngine;

public class FreeplayPopoverButton : MonoBehaviour
{
	[SerializeField]
	private MapNames map;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private PassiveButton button;

	public MapNames Map => map;

	public UiElement Button => button;

	public Vector2 Size
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return collider.size * Vector2.op_Implicit(((Component)this).transform.localScale);
		}
	}

	public event Action<FreeplayPopoverButton> OnPressEvent;

	private void Awake()
	{
		button.AddOnClickListeners(OnPressed);
	}

	private void OnPressed()
	{
		OnPressEvent?.Invoke(this);
	}
}
