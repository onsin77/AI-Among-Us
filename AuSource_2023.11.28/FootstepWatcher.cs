using UnityEngine;

internal class FootstepWatcher : MonoBehaviour, IStepWatcher
{
	public int priority;

	public Collider2D Area;

	public SoundGroup Sounds;

	public int Priority => priority;

	public SoundGroup MakeFootstep(PlayerControl player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (Area.OverlapPoint(player.GetTruePosition()))
		{
			return Sounds;
		}
		return null;
	}
}
