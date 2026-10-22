using UnityEngine;

public class KeySlotBehaviour : MonoBehaviour
{
	public Sprite Highlit;

	public Sprite Inserted;

	public Sprite Finished;

	public SpriteRenderer Image;

	public BoxCollider2D Hitbox;

	internal void SetFinished()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Image.sprite = Finished;
		((Component)this).transform.localEulerAngles = new Vector3(0f, 0f, 90f);
	}

	internal void SetInserted()
	{
		Image.sprite = Inserted;
	}

	internal void SetHighlight()
	{
		Image.sprite = Highlit;
	}
}
