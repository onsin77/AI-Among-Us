using Assets.CoreScripts;
using UnityEngine;

public class StoreArmsTaskConsole : Console
{
	public AudioClip useSound;

	public Sprite[] Images;

	public int usesPerStep = 2;

	private int timesUsed;

	private PlayerTask FindTask(PlayerControl pc)
	{
		for (int i = 0; i < pc.myTasks.Count; i++)
		{
			PlayerTask playerTask = pc.myTasks[i];
			if (!playerTask.IsComplete && playerTask.ValidConsole(this))
			{
				return playerTask;
			}
		}
		return null;
	}

	public override void Use()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (canUse)
		{
			PlayerControl localPlayer = PlayerControl.LocalPlayer;
			NormalPlayerTask normalPlayerTask = (NormalPlayerTask)FindTask(localPlayer);
			AfterUse(normalPlayerTask);
			DestroyableSingleton<UnityTelemetry>.Instance.WriteUse(localPlayer.PlayerId, normalPlayerTask.TaskType, ((Component)this).transform.position);
		}
	}

	protected virtual void AfterUse(NormalPlayerTask task)
	{
		if (Object.op_Implicit((Object)(object)useSound) && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(useSound, loop: false);
		}
		if (++timesUsed % usesPerStep == 0)
		{
			task.NextStep();
		}
		int num = timesUsed % Images.Length;
		Image.sprite = Images[num];
	}
}
