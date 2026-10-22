using UnityEngine;

public class PlayOnlineButtonSprite : MonoBehaviour
{
	[SerializeField]
	private Sprite Colored;

	[SerializeField]
	private Sprite Greyscale;

	[SerializeField]
	private SpriteRenderer Sprite;

	public void Start()
	{
		SetState(DestroyableSingleton<AccountManager>.Instance.CanPlayOnline());
	}

	public void SetState(bool b)
	{
		if (b)
		{
			SetColored();
		}
		else
		{
			SetGreyscale();
		}
	}

	public void SetColored()
	{
		Sprite.sprite = Colored;
	}

	public void SetGreyscale()
	{
		Sprite.sprite = Greyscale;
	}
}
