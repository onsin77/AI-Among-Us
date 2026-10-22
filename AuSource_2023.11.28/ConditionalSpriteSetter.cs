using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ConditionalSpriteSetter : MonoBehaviour
{
	public bool ignoreIfNoSpriteForPlatform = true;

	public ConditionalSprite sprite;

	private void Start()
	{
		SpriteRenderer component = ((Component)this).GetComponent<SpriteRenderer>();
		if (Object.op_Implicit((Object)(object)component) && (Object.op_Implicit((Object)(object)sprite.Select()) || !ignoreIfNoSpriteForPlatform))
		{
			component.sprite = sprite;
		}
	}
}
