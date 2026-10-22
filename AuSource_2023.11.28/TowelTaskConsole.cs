using UnityEngine;

public class TowelTaskConsole : AutoTaskConsole
{
	protected override void AfterUse(NormalPlayerTask task)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)useSound) && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(useSound, loop: false);
		}
		int num = task.Data.IndexOf((byte b) => b == ConsoleId);
		task.Data[num] = 250;
		Image.color = Palette.ClearWhite;
	}
}
