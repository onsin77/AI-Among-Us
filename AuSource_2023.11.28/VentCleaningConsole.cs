using UnityEngine;

public class VentCleaningConsole : Console
{
	public AudioClip ImpostorDiscoveredSound;

	public override void Use()
	{
		CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
		if (!canUse)
		{
			return;
		}
		VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem;
		if (!PlayerControl.LocalPlayer.Data.IsDead && ventilationSystem != null && ventilationSystem.IsImpostorInsideVent(ConsoleId))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ImpostorDiscoveredSound, loop: false, 0.8f);
			}
			VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, ConsoleId);
		}
		else
		{
			base.Use();
		}
	}
}
