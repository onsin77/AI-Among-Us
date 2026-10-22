using System;
using UnityEngine;

[Serializable]
public class PlayerBodySprite
{
	public SpriteRenderer BodySprite;

	public PlayerPettingHand PettingHand;

	public SpriteRenderer HandHat;

	public Sprite GhostSprite;

	public PlayerBodyTypes Type;

	public Vector3 flippedCosmeticOffset;

	public bool Visible
	{
		get
		{
			return ((Renderer)BodySprite).enabled;
		}
		set
		{
			((Renderer)BodySprite).enabled = value;
		}
	}
}
