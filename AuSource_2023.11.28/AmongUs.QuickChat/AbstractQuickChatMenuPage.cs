using UnityEngine;

namespace AmongUs.QuickChat;

public abstract class AbstractQuickChatMenuPage : MonoBehaviour
{
	private QuickChatMenu menu;

	private float width;

	private float height;

	public QuickChatMenu Menu => menu;

	public float Width => width;

	public float Height => height;

	public virtual PassiveButton DefaultSelection => null;

	public virtual void Initialize(QuickChatMenu menu, float width)
	{
		this.menu = menu;
		this.width = width;
	}

	public virtual void Show(float height)
	{
		this.height = height;
		((Component)this).gameObject.SetActive(true);
	}

	public virtual void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
