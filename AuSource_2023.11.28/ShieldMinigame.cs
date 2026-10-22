using Rewired;
using UnityEngine;

public class ShieldMinigame : Minigame
{
	public Color OnColor = Color.white;

	public Color OffColor = Color.red;

	public SpriteRenderer[] Shields;

	public SpriteRenderer Gauge;

	private byte shields;

	public AudioClip ShieldOnSound;

	public AudioClip ShieldOffSound;

	public Transform selectedButtonHighlight;

	private Controller controller = new Controller();

	private int oldSelectedIndex = -1;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		shields = MyNormTask.Data[0];
		UpdateButtons();
		SetupInput(disableCursor: true);
	}

	public void ToggleShield(int i)
	{
		if (MyNormTask.IsComplete)
		{
			return;
		}
		byte b = (byte)(1 << i);
		shields ^= b;
		MyNormTask.Data[0] = shields;
		if ((shields & b) != 0)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ShieldOnSound, loop: false);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ShieldOffSound, loop: false);
		}
		if (shields == 127)
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
			if (ShipStatus.Instance.ShieldsImages.Length != 0 && !ShipStatus.Instance.ShieldsImages[0].IsPlaying())
			{
				PlayerControl.LocalPlayer.RpcPlayAnimation(1);
			}
		}
	}

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
		int num = 0;
		if (val.sqrMagnitude > 0.5f)
		{
			val = val.normalized;
			float num2 = float.NegativeInfinity;
			num = -1;
			Vector2 val2 = Vector2.op_Implicit(((Component)Shields[0]).transform.localPosition);
			for (int i = 1; i < Shields.Length; i++)
			{
				Vector2 val3 = Vector2.op_Implicit(((Component)Shields[i]).transform.localPosition) - val2;
				float num3 = Vector2.Dot(val3.normalized, val);
				if (num == -1 || num3 > num2)
				{
					num = i;
					num2 = num3;
				}
			}
		}
		if (oldSelectedIndex != num)
		{
			Vector3 localPosition = ((Component)selectedButtonHighlight).transform.localPosition;
			Vector3 localPosition2 = ((Component)Shields[num]).transform.localPosition;
			localPosition.x = localPosition2.x;
			localPosition.y = localPosition2.y;
			((Component)selectedButtonHighlight).transform.localPosition = localPosition;
			oldSelectedIndex = num;
		}
		if (player.GetButtonDown(11))
		{
			ToggleShield(num);
		}
	}

	public void FixedUpdate()
	{
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < Shields.Length; i++)
		{
			bool flag = (shields & (1 << i)) == 0;
			if (!flag)
			{
				num++;
			}
			Shields[i].color = (flag ? OffColor : OnColor);
		}
		if (shields == 127)
		{
			((Component)Gauge).transform.Rotate(0f, 0f, Time.fixedDeltaTime * 45f);
			Gauge.color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			float num2 = Mathf.Lerp(0.1f, 0.5f, (float)num / 6f);
			Gauge.color = new Color(1f, num2, num2, 1f);
		}
	}

	public ShieldMinigame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
