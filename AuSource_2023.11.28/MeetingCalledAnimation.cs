using System.Collections;
using UnityEngine;

public class MeetingCalledAnimation : OverlayAnimation
{
	public AspectPosition emergencyText;

	public PoolablePlayer playerParts;

	public AudioClip Stinger;

	public float StingerVolume = 0.6f;

	public AnimationCurve CrewmateSlide;

	public void Initialize(GameData.PlayerInfo reportInfo)
	{
		playerParts.UpdateFromEitherPlayerDataOrCache(reportInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, includePet: false);
	}

	public override IEnumerator CoShow(KillOverlay parent)
	{
		((Component)this).gameObject.SetActive(true);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(Stinger, loop: false).volume = StingerVolume;
		}
		AspectPosition playerTransform = ((Component)playerParts).GetComponent<AspectPosition>();
		_ = playerTransform.DistanceFromEdge;
		yield return Effects.All(Effects.Lerp(2.5f, (float t) =>
		{
			emergencyText.SetNormalizedX(CrewmateSlide.Evaluate(t), 0.5f);
		}), Effects.Lerp(2.5f, (float t) =>
		{
			playerTransform.SetNormalizedX(CrewmateSlide.Evaluate(t), 0.5f);
		}));
		((Component)this).gameObject.SetActive(false);
	}
}
