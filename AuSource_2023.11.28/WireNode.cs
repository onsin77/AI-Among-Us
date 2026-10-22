using UnityEngine;

public class WireNode : MonoBehaviour
{
	public Collider2D hitbox;

	public SpriteRenderer[] WireColors;

	public SpriteRenderer BaseSymbol;

	public sbyte WireId;

	internal void SetColor(Color color, Sprite symbol)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		BaseSymbol.sprite = symbol;
		for (int i = 0; i < WireColors.Length; i++)
		{
			WireColors[i].color = color;
		}
	}
}
