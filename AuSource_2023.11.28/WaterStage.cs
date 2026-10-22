using Rewired;
using UnityEngine;

public class WaterStage : Minigame
{
	public float RefuelDuration = 5f;

	public SpriteRenderer waterButton;

	public Sprite buttonDownSprite;

	public Sprite buttonUpSprite;

	public VerticalGauge srcGauge;

	public VerticalGauge destGauge;

	public AudioClip RefuelSound;

	private float timer;

	private bool isDown;

	private bool complete;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		timer = (float)(int)MyNormTask.Data[0] / 255f;
	}

	public void FixedUpdate()
	{
		if (complete)
		{
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			bool button = ReInput.players.GetPlayer(0).GetButton(11);
			if (button != isDown)
			{
				isDown = button;
				if (!button)
				{
					SoundManager.Instance.StopSound(RefuelSound);
				}
				waterButton.sprite = (button ? buttonDownSprite : buttonUpSprite);
			}
		}
		if (isDown && timer < 1f)
		{
			timer += Time.fixedDeltaTime / RefuelDuration;
			MyNormTask.Data[0] = (byte)Mathf.Min(255f, timer * 255f);
			if (timer >= 1f)
			{
				complete = true;
				MyNormTask.Data[0] = 0;
				MyNormTask.NextStep();
				SoundManager.Instance.StopSound(RefuelSound);
				((Component)((Component)this).transform.parent).GetComponent<Minigame>().Close();
			}
		}
		if (Object.op_Implicit((Object)(object)destGauge))
		{
			destGauge.value = timer;
		}
		if (Object.op_Implicit((Object)(object)srcGauge))
		{
			srcGauge.value = 1f - timer;
		}
	}

	public void Refuel()
	{
		if (complete)
		{
			((Component)((Component)this).transform.parent).GetComponent<Minigame>().Close();
			return;
		}
		isDown = !isDown;
		waterButton.sprite = (isDown ? buttonDownSprite : buttonUpSprite);
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
		if (MyNormTask.taskStep == 0)
		{
			player.pitch = Mathf.Lerp(0.75f, 1.25f, timer);
		}
		else
		{
			player.pitch = Mathf.Lerp(1.25f, 0.75f, timer);
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(RefuelSound);
		if (Object.op_Implicit((Object)(object)Minigame.Instance))
		{
			Minigame.Instance.Close();
		}
	}
}
