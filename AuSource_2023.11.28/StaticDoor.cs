using UnityEngine;

public class StaticDoor : MonoBehaviour
{
	public Sprite OpenDoorImage;

	public Sprite CloseDoorImage;

	public bool IsOpen { get; private set; }

	public void SetOpen(bool isOpen)
	{
		IsOpen = isOpen;
		Collider2D component = ((Component)this).GetComponent<Collider2D>();
		SpriteRenderer component2 = ((Component)this).GetComponent<SpriteRenderer>();
		EdgeCollider2D componentInChildren = ((Component)this).GetComponentInChildren<EdgeCollider2D>();
		if (isOpen)
		{
			((Behaviour)component).enabled = false;
			component2.sprite = OpenDoorImage;
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				((Behaviour)componentInChildren).enabled = false;
			}
		}
		else
		{
			((Behaviour)component).enabled = true;
			component2.sprite = CloseDoorImage;
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				((Behaviour)componentInChildren).enabled = true;
			}
		}
	}
}
