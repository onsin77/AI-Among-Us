using TMPro;
using UnityEngine;

public class ShowerMinigame : Minigame
{
	public VerticalGauge Gauge;

	public TextMeshPro PercentText;

	private float timer;

	public float MaxTime = 12f;

	public AudioClip washSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		timer = MaxTime * (float)(int)MyNormTask.Data[0] / 100f;
		((TMP_Text)PercentText).text = 100 - MyNormTask.Data[0] + "%";
		AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
		if (Object.op_Implicit((Object)(object)airshipStatus) && Object.op_Implicit((Object)(object)airshipStatus.ShowerParticles))
		{
			airshipStatus.ShowerParticles.Play();
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(washSound, loop: true, 1f, SoundManager.Instance.SfxChannel);
		}
		SetupInput(disableCursor: true);
		VibrationManager.Vibrate(2f, 2f, 0f, VibrationManager.VibrationFalloff.None, washSound, loopClip: true);
	}

	public void Update()
	{
		if (amClosing == CloseState.None)
		{
			timer += Time.deltaTime;
			MyNormTask.Data[0] = (byte)(timer / MaxTime * 100f);
			Gauge.value = 1f - timer / MaxTime;
			((TMP_Text)PercentText).text = 100 - MyNormTask.Data[0] + "%";
			if (MyNormTask.Data[0] >= 100)
			{
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose(0.5f));
			}
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(washSound);
		AirshipStatus obj = ShipStatus.Instance as AirshipStatus;
		if (obj != null)
		{
			obj.ShowerParticles.Stop();
		}
		VibrationManager.CancelVibration(washSound);
		base.Close();
	}
}
