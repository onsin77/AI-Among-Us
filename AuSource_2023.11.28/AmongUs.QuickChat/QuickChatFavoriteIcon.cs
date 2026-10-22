using System;
using UnityEngine;

namespace AmongUs.QuickChat;

public class QuickChatFavoriteIcon : MonoBehaviour
{
	private const string STATE_ACTIVE_IDLE = "ActiveIdle";

	private const string STATE_INACTIVE_IDLE = "InactiveIdle";

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private BoxCollider2D collider;

	[SerializeField]
	private PassiveButton button;

	public Vector2 Size
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return collider.size;
		}
	}

	public event Action OnPressEvent;

	private void Awake()
	{
		button.AddOnClickListeners(OnButtonPressed);
	}

	public void Setup(bool active)
	{
		animator.Play(active ? "ActiveIdle" : "InactiveIdle");
		SetFavorite(active);
	}

	public void SetFavorite(bool isFavorite)
	{
		animator.SetBool("Active", isFavorite);
	}

	public void SetHeight(float height)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y = (0f - height) / 2f;
		((Component)this).transform.localPosition = localPosition;
		collider.size = new Vector2(collider.size.x, height);
	}

	private void OnButtonPressed()
	{
		OnPressEvent?.Invoke();
	}
}
