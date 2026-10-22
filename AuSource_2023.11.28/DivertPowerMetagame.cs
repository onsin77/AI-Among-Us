using UnityEngine;

public class DivertPowerMetagame : Minigame
{
	public Minigame DistributePrefab;

	public Minigame ReceivePrefab;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		Minigame minigame = ((MyNormTask.taskStep != 0) ? Object.Instantiate<Minigame>(ReceivePrefab, ((Component)this).transform.parent) : Object.Instantiate<Minigame>(DistributePrefab, ((Component)this).transform.parent));
		minigame.Begin(task);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
