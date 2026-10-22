using Rewired;
using UnityEngine;

public class RefuelStage : Minigame
{
	public float RefuelDuration = 5f;

	private Color darkRed = Color32.op_Implicit(new Color32((byte)90, (byte)0, (byte)0, byte.MaxValue));

	private Color red = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)58, (byte)0, byte.MaxValue));

	private Color green = Color.green;

	public SpriteRenderer redLight;

	public SpriteRenderer greenLight;

	public VerticalGauge srcGauge;

	public VerticalGauge destGauge;

	public AudioClip RefuelSound;

	private float timer;

	private bool isDown;

	private bool complete;

	private bool usingController;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		timer = (float)(int)MyNormTask.Data[0] / 255f;
		SetupInput(disableCursor: true);
	}

	public void FixedUpdate()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (ReInput.players.GetPlayer(0).GetButton(21))
		{
			if (!isDown)
			{
				usingController = true;
				Refuel();
			}
		}
		else if (isDown && usingController)
		{
			usingController = false;
			Refuel();
		}
		if (complete)
		{
			return;
		}
		if (isDown && timer < 1f)
		{
			timer += Time.fixedDeltaTime / RefuelDuration;
			MyNormTask.Data[0] = (byte)Mathf.Min(255f, timer * 255f);
			if (timer >= 1f)
			{
				complete = true;
				if (Object.op_Implicit((Object)(object)greenLight))
				{
					greenLight.color = green;
				}
				if (Object.op_Implicit((Object)(object)redLight))
				{
					redLight.color = darkRed;
				}
				if (MyNormTask.MaxStep == 1)
				{
					MyNormTask.NextStep();
				}
				else if (ShipStatus.Instance is AirshipStatus || ShipStatus.Instance is FungleShipStatus)
				{
					MyNormTask.Data[0] = 0;
					MyNormTask.Data[1] = (byte)(BoolRange.Next() ? 1u : 2u);
					MyNormTask.NextStep();
				}
				else
				{
					MyNormTask.Data[0] = 0;
					MyNormTask.Data[1]++;
					if (MyNormTask.Data[1] % 2 == 0)
					{
						MyNormTask.NextStep();
					}
					MyNormTask.UpdateArrowAndLocation();
				}
			}
		}
		destGauge.value = timer;
		if (Object.op_Implicit((Object)(object)srcGauge))
		{
			srcGauge.value = 1f - timer;
		}
	}

	public void Refuel()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (complete)
		{
			((Component)((Component)this).transform.parent).GetComponent<Minigame>().Close();
			return;
		}
		isDown = !isDown;
		if (Object.op_Implicit((Object)(object)redLight))
		{
			redLight.color = (isDown ? red : darkRed);
		}
		if (isDown)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound("Refuel", RefuelSound, loop: true, GetRefuelDynamics, SoundManager.Instance.SfxChannel);
			}
		}
		else
		{
			SoundManager.Instance.StopSound(RefuelSound);
		}
	}

	private void GetRefuelDynamics(AudioSource player, float dt)
	{
		player.volume = 1f;
		player.pitch = Mathf.Lerp(0.75f, 1.25f, timer);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(RefuelSound);
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			Minigame.Instance.Close();
		}
	}

	public RefuelStage()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
	}
}
