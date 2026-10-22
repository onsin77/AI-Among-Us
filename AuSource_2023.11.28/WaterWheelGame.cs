using Rewired;
using UnityEngine;

public class WaterWheelGame : Minigame
{
	public SpriteRenderer Wheel;

	public VerticalSpriteGauge WaterLevel;

	public SpriteRenderer Watertop;

	public int WheelScale = 4;

	public AudioClip FillStart;

	public AudioClip FillLoop;

	public AudioClip WheelTurn;

	private TouchpadBehavior touchpad;

	private float Rate = 0.01f;

	private AudioSource fillSound;

	private Vector2 prevStickInput = Vector2.zero;

	private bool hadPrev;

	private bool grabbed;

	private float Water
	{
		get
		{
			return WaterLevel.Value;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, 1f);
			WaterLevel.Value = num;
			MyNormTask.Data[ConsoleId] = (byte)(num * 255f);
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		WaterLevel.Value = (float)(int)MyNormTask.Data[ConsoleId] / 255f;
		if (Constants.ShouldPlaySfx())
		{
			fillSound = SoundManager.Instance.PlaySound(FillStart, loop: false);
		}
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		Water += Time.deltaTime * Rate;
		if (Object.op_Implicit((Object)(object)fillSound) && !fillSound.isPlaying)
		{
			fillSound = SoundManager.Instance.PlaySound(FillLoop, loop: true);
		}
		if (Object.op_Implicit((Object)(object)fillSound))
		{
			fillSound.volume = Mathf.Lerp(0f, 1f, Rate * 5f);
			fillSound.pitch = Mathf.Lerp(0.75f, 1.25f, Water);
		}
		Vector2 vector = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
		if (touchpad.IsTouching())
		{
			Vector2 centerToTouch = touchpad.GetCenterToTouch();
			vector = centerToTouch.normalized;
		}
		if (vector.magnitude > 0.9f)
		{
			vector = vector.normalized;
			if (hadPrev)
			{
				float num = prevStickInput.AngleSigned(vector);
				if (num > 180f)
				{
					num -= 360f;
				}
				if (num < -180f)
				{
					num += 360f;
				}
				num /= (float)WheelScale;
				Vector3 localEulerAngles = ((Component)Wheel).transform.localEulerAngles;
				float z = localEulerAngles.z;
				float num2 = Mathf.Clamp(localEulerAngles.z + num, 0.0001f, 358.99f);
				if (Mathf.Abs(localEulerAngles.z - num2) > 1f && Constants.ShouldPlaySfx())
				{
					AudioSource val = SoundManager.Instance.PlaySound(WheelTurn, loop: false);
					if (val.timeSamples == 0)
					{
						val.pitch = FloatRange.Next(0.9f, 1.1f);
					}
				}
				localEulerAngles.z = num2;
				((Component)Wheel).transform.localEulerAngles = localEulerAngles;
				Rate += num / (float)(360 * WheelScale);
				float num3 = Mathf.Abs(z - num2) * 0.01f;
				VibrationManager.Vibrate(num3, num3, 0.02f);
			}
			prevStickInput = vector;
			hadPrev = true;
		}
		else
		{
			hadPrev = false;
		}
		if (grabbed)
		{
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			Vector2 val2 = Vector2.op_Implicit(((Component)Wheel).transform.position);
			float num4 = (controller.DragStartPosition - val2).AngleSigned(controller.DragPosition - val2);
			if (num4 > 180f)
			{
				num4 -= 360f;
			}
			if (num4 < -180f)
			{
				num4 += 360f;
			}
			num4 /= (float)WheelScale;
			Vector3 localEulerAngles2 = ((Component)Wheel).transform.localEulerAngles;
			float num5 = Mathf.Clamp(localEulerAngles2.z + num4, 0.0001f, 358.99f);
			if (Mathf.Abs(localEulerAngles2.z - num5) > 1f && Constants.ShouldPlaySfx())
			{
				AudioSource val3 = SoundManager.Instance.PlaySound(WheelTurn, loop: false);
				if (val3.timeSamples == 0)
				{
					val3.pitch = FloatRange.Next(0.9f, 1.1f);
				}
			}
			localEulerAngles2.z = num5;
			((Component)Wheel).transform.localEulerAngles = localEulerAngles2;
			Rate += num4 / (float)(360 * WheelScale);
			controller.ResetDragPosition();
		}
		else if (WaterLevel.Value >= 1f)
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
		Vector3 localPosition = ((Component)Watertop).transform.localPosition;
		localPosition.y = WaterLevel.TopY;
		((Component)Watertop).transform.localPosition = localPosition;
	}

	public void Grab()
	{
		grabbed = !grabbed;
	}

	public override void Close()
	{
		if (Object.op_Implicit((Object)(object)fillSound))
		{
			fillSound.Stop();
		}
		base.Close();
	}

	public WaterWheelGame()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
	}
}
