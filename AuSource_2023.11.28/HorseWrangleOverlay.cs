using System.Collections;
using UnityEngine;

public class HorseWrangleOverlay : OverlayKillAnimation
{
	[SerializeField]
	private SpriteRenderer impostorForeground;

	[SerializeField]
	private SpriteRenderer impostorHand;

	[SerializeField]
	private SpriteRenderer[] victimSprites;

	public override IEnumerator CoShow(KillOverlay parent)
	{
		victimParts.SetHatAndVisorOnFloor();
		PlayerMaterial.SetColors(killerParts.ColorId, (Renderer)(object)impostorForeground);
		PlayerMaterial.SetColors(killerParts.ColorId, (Renderer)(object)impostorHand);
		victimSprites.ForEach((SpriteRenderer b) =>
		{
			PlayerMaterial.SetColors(victimParts.ColorId, (Renderer)(object)b);
		});
		yield return base.CoShow(parent);
	}
}
