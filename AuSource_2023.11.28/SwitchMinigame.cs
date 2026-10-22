using Rewired;
using UnityEngine;

public class SwitchMinigame : Minigame
{
	public Color OnColor = Color.green;

	public Color OffColor = new Color(0.1f, 0.3f, 0.1f);

	private ShipStatus ship;

	public SpriteRenderer[] switches;

	public SpriteRenderer[] lights;

	public RadioWaveBehaviour top;

	public HorizontalGauge middle;

	public FlatWaveBehaviour bottom;

	public AudioClip FlipSound;

	public Transform selectorHighlight;

	private int selectedIndex;

	private float selectorCooldown;

	private bool prevHadVerticalInput;

	public override void Begin(PlayerTask task)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		ship = Object.FindObjectOfType<ShipStatus>();
		SwitchSystem switchSystem = ship.Systems[SystemTypes.Electrical] as SwitchSystem;
		for (int i = 0; i < switches.Length; i++)
		{
			byte b = (byte)(1 << i);
			int num = switchSystem.ActualSwitches & b;
			lights[i].color = ((num == (switchSystem.ExpectedSwitches & b)) ? OnColor : OffColor);
			switches[i].flipY = num >> i == 0;
		}
		SetupInput(disableCursor: true);
	}

	public void FixedUpdate()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		((Component)selectorHighlight).gameObject.SetActive(ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick);
		Player player = ReInput.players.GetPlayer(0);
		float axisRaw = player.GetAxisRaw(13);
		float axisRaw2 = player.GetAxisRaw(17);
		if (Mathf.Abs(axisRaw) > 0.7f)
		{
			if (selectorCooldown <= 0f)
			{
				selectedIndex += (int)Mathf.Sign(axisRaw);
				selectedIndex = Mathf.Clamp(selectedIndex, 0, switches.Length);
				((Component)selectorHighlight).transform.localPosition = ((Component)switches[selectedIndex]).transform.localPosition;
				selectorCooldown = 0.2f;
			}
			else
			{
				selectorCooldown -= Time.deltaTime;
			}
		}
		else
		{
			selectorCooldown = 0f;
		}
		if (Mathf.Abs(axisRaw2) > 0.7f)
		{
			if (!prevHadVerticalInput)
			{
				if (axisRaw2 > 0.7f)
				{
					if (switches[selectedIndex].flipY)
					{
						FlipSwitch(selectedIndex);
					}
				}
				else if (axisRaw2 < -0.7f && !switches[selectedIndex].flipY)
				{
					FlipSwitch(selectedIndex);
				}
			}
			prevHadVerticalInput = true;
		}
		else
		{
			prevHadVerticalInput = false;
		}
		int num = 0;
		SwitchSystem switchSystem = ship.Systems[SystemTypes.Electrical] as SwitchSystem;
		for (int i = 0; i < switches.Length; i++)
		{
			byte b = (byte)(1 << i);
			int num2 = switchSystem.ActualSwitches & b;
			if (num2 == (switchSystem.ExpectedSwitches & b))
			{
				num++;
				lights[i].color = OnColor;
			}
			else
			{
				lights[i].color = OffColor;
			}
			switches[i].flipY = num2 >> i == 0;
		}
		float num3 = (float)num / (float)switches.Length;
		bottom.Center = 0.47f * num3;
		top.NoiseLevel = 1f - num3;
		middle.Value = switchSystem.Level + (Mathf.PerlinNoise(0f, Time.time * 51f) - 0.5f) * 0.04f;
		if (num == switches.Length)
		{
			((MonoBehaviour)this).StartCoroutine(CoStartClose(0.5f));
		}
	}

	public void FlipSwitch(int switchIdx)
	{
		if (amClosing != CloseState.None)
		{
			return;
		}
		int num = 0;
		SwitchSystem switchSystem = ship.Systems[SystemTypes.Electrical] as SwitchSystem;
		for (int i = 0; i < switches.Length; i++)
		{
			byte b = (byte)(1 << i);
			if ((switchSystem.ActualSwitches & b) == (switchSystem.ExpectedSwitches & b))
			{
				num++;
			}
		}
		if (num == switches.Length)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(FlipSound, loop: false);
		}
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)switchIdx);
		try
		{
			((SabotageTask)MyTask).MarkContributed();
		}
		catch
		{
		}
	}

	public SwitchMinigame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
	}
}
