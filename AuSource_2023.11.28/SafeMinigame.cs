using System;
using System.Linq;
using Rewired;
using TMPro;
using UnityEngine;

public class SafeMinigame : Minigame
{
	private const float LowTumblerThreshold = 5f;

	private const float HighTumblerThreshold = 7f;

	private const float ReverseDirThreshold = 0.15f;

	public TextMeshPro ComboText;

	public Collider2D Tumbler;

	public SpriteRenderer TumblerBehind;

	public Collider2D Spinner;

	public SpriteRenderer[] Arrows;

	private int[] combo = new int[3];

	private bool[] latched = new bool[3];

	private bool[] vibration = new bool[3];

	private Controller controller = new Controller();

	private float lastTumDir;

	private Vector2 lastMouseVec;

	private float spinTime;

	private float spinDel;

	private float spinVel;

	private float reversalBuffer;

	public AudioClip DialTurnSound;

	public AudioClip DialGoodSound;

	public AudioClip SpinnerStartSound;

	public AudioClip SpinnerFreeSound;

	public AudioClip SpinnerStopSound;

	private AudioSource loopSound;

	private bool prevHadLeftInput;

	private bool prevHadRightInput;

	private Vector2 prevLeftInput = Vector2.zero;

	private Vector2 prevRightInput = Vector2.zero;

	private float leftStickStartAngle;

	public float TopSpinRate = 360f;

	public void OnEnable()
	{
		Begin(null);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		((Component)Spinner).GetComponent<SpriteRenderer>().color = Palette.DisabledGrey;
		do
		{
			for (int i = 0; i < combo.Length; i++)
			{
				combo[i] = IntRange.Next(8) + 1;
			}
		}
		while (combo[0] == combo[1] || combo[1] == combo[2]);
		Vector3 eulerAngles = ((Component)Tumbler).transform.eulerAngles;
		eulerAngles.z = new FloatRange(0f, 360f).NextMinDistance(combo[0] * 45, 45f);
		((Component)Tumbler).transform.eulerAngles = eulerAngles;
		combo[1] = -combo[1];
		UpdateComboInstructions();
		loopSound = SoundManager.Instance.GetNamedSfxSource("spinnerLoop");
		loopSound.volume = 0f;
		loopSound.pitch = 0.5f;
		loopSound.loop = false;
		loopSound.clip = SpinnerFreeSound;
		SetupInput(disableCursor: true);
	}

	private void UpdateComboInstructions()
	{
		int num = latched.LastIndexOf((bool b) => b) + 2;
		((TMP_Text)ComboText).text = string.Join(" - ", from c in combo.Take(num)
			select Mathf.Abs(c) - 1);
		for (int num2 = 0; num2 < Arrows.Length; num2++)
		{
			((Renderer)Arrows[num2]).enabled = num2 < num;
		}
	}

	private void Update()
	{
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		Player player = ReInput.players.GetPlayer(0);
		float delta = 0f;
		bool flag = latched.All((bool b) => b);
		if (!flag)
		{
			int num = latched.IndexOf((bool b) => !b);
			int num2 = combo[num] * 45;
			Vector3 eulerAngles = ((Component)Tumbler).transform.eulerAngles;
			if (eulerAngles.z < 0f)
			{
				eulerAngles.z += 360f;
			}
			bool flag2 = false;
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				Vector2 val = player.GetAxis2DRaw(13, 14);
				float magnitude = val.magnitude;
				if (magnitude > 0.9f)
				{
					val /= magnitude;
					if (!prevHadLeftInput)
					{
						lastTumDir = 0f;
						leftStickStartAngle = Mathf.Atan2(val.y, val.x) - eulerAngles.z * ((float)Math.PI / 180f);
					}
					else
					{
						delta = Vector2.SignedAngle(prevLeftInput, val) * ((float)Math.PI / 180f);
						eulerAngles.z = (Mathf.Atan2(val.y, val.x) - leftStickStartAngle) * 57.29578f;
						((Component)Tumbler).transform.eulerAngles = eulerAngles;
						flag2 = true;
					}
					prevLeftInput = val;
					prevHadLeftInput = true;
				}
				else
				{
					if (prevHadLeftInput && num == combo.Length - 1)
					{
						if (AngleNear(eulerAngles.z + 45f, lastTumDir, num2, 7f))
						{
							latched[num] = true;
						}
						UpdateComboInstructions();
					}
					prevHadLeftInput = false;
				}
			}
			else
			{
				switch (controller.CheckDrag(Tumbler))
				{
				case DragState.TouchStart:
				{
					lastTumDir = 0f;
					Vector2 val3 = controller.DragPosition - Vector2.op_Implicit(((Component)Tumbler).transform.position);
					val3.Normalize();
					leftStickStartAngle = Mathf.Atan2(val3.y, val3.x) - eulerAngles.z * ((float)Math.PI / 180f);
					lastMouseVec = val3;
					break;
				}
				case DragState.Dragging:
				{
					Vector2 val2 = controller.DragPosition - Vector2.op_Implicit(((Component)Tumbler).transform.position);
					val2.Normalize();
					delta = Vector2.SignedAngle(lastMouseVec, val2) * ((float)Math.PI / 180f);
					eulerAngles.z = (Mathf.Atan2(val2.y, val2.x) - leftStickStartAngle) * 57.29578f;
					flag2 = true;
					((Component)Tumbler).transform.eulerAngles = eulerAngles;
					lastMouseVec = val2;
					break;
				}
				case DragState.Released:
					if (num == combo.Length - 1)
					{
						if (AngleNear(eulerAngles.z + 45f, lastTumDir, num2, 7f))
						{
							latched[num] = true;
						}
						UpdateComboInstructions();
					}
					break;
				}
			}
			if (flag2)
			{
				CheckTumblr(delta, eulerAngles.z, num, num2);
			}
			if (latched.All((bool b) => b))
			{
				spinDel = 0f;
				controller.ClearTouch();
				TumblerBehind.color = Palette.DisabledGrey;
				((Component)Tumbler).GetComponent<SpriteRenderer>().color = Palette.DisabledGrey;
				((Component)Spinner).GetComponent<SpriteRenderer>().color = Color.white;
			}
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Vector2 val4 = player.GetAxis2DRaw(16, 17);
			float magnitude2 = val4.magnitude;
			if (magnitude2 > 0.9f)
			{
				val4 /= magnitude2;
				if (!prevHadRightInput)
				{
					spinTime = 0f;
				}
				else
				{
					spinTime += Time.deltaTime;
					Vector2 val5 = Vector2.op_Implicit(((Component)Spinner).transform.position);
					(lastMouseVec - val5).WheelAngle(controller.DragPosition - val5);
					lastMouseVec = controller.DragPosition;
					float num3 = spinVel;
					float num4 = Vector2.SignedAngle(prevRightInput, val4);
					spinVel += num4 / (3f * spinTime);
					if (spinVel > TopSpinRate)
					{
						spinVel = TopSpinRate;
					}
					if (spinVel < 0f - TopSpinRate)
					{
						spinVel = 0f - TopSpinRate;
					}
					if (Mathf.Sign(num3) != Mathf.Sign(spinVel) && Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(SpinnerStartSound, loop: false);
					}
				}
				prevRightInput = val4;
				prevHadRightInput = true;
			}
			else
			{
				prevHadRightInput = false;
			}
		}
		else
		{
			switch (controller.CheckDrag(Spinner))
			{
			case DragState.TouchStart:
				lastMouseVec = controller.DragPosition;
				spinTime = 0f;
				break;
			case DragState.Dragging:
			{
				spinTime += Time.deltaTime;
				Vector2 val6 = Vector2.op_Implicit(((Component)Spinner).transform.position);
				float num5 = (lastMouseVec - val6).WheelAngle(controller.DragPosition - val6);
				lastMouseVec = controller.DragPosition;
				float num6 = spinVel;
				spinVel += num5 / (2.5f * spinTime);
				if (spinVel > TopSpinRate)
				{
					spinVel = TopSpinRate;
				}
				if (spinVel < 0f - TopSpinRate)
				{
					spinVel = 0f - TopSpinRate;
				}
				if (Mathf.Sign(num6) != Mathf.Sign(spinVel) && Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(SpinnerStartSound, loop: false);
				}
				break;
			}
			}
		}
		if (Constants.ShouldPlaySfx())
		{
			float num7 = Mathf.Min(1f, Mathf.Abs(spinVel / 60f));
			float volume = num7 * num7 * num7;
			loopSound.volume = volume;
			float num8 = Mathf.Lerp(0.5f, 1.25f, num7);
			loopSound.pitch = Mathf.Lerp(loopSound.pitch, num8, Time.deltaTime);
		}
		float num9 = ((Component)Spinner).transform.localEulerAngles.z - 180f;
		float num10 = spinVel * Time.deltaTime;
		((Component)Spinner).transform.Rotate(0f, 0f, num10);
		float num11 = ((Component)Spinner).transform.localEulerAngles.z - 180f;
		if (Constants.ShouldPlaySfx() && Mathf.Sign(num9) != Mathf.Sign(num11))
		{
			loopSound.Play();
		}
		spinDel += num10;
		spinVel = Mathf.Lerp(spinVel, 0f, Time.deltaTime);
		if (flag && Mathf.Abs(spinDel) > 720f)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(SpinnerStopSound, loop: false);
			}
			MyNormTask.NextStep();
			Close();
		}
	}

	private void CheckTumblr(float delta, float tumRotZ, int unlatched, int expected)
	{
		float num = lastTumDir;
		float num2 = 0f - Mathf.Sign(delta);
		if (num2 != 0f && num != num2)
		{
			reversalBuffer += delta;
			if (Mathf.Abs(reversalBuffer) > 0.15f)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(DialTurnSound, loop: false);
				}
				lastTumDir = 0f - Mathf.Sign(delta);
				reversalBuffer = 0f;
			}
		}
		if (num != 0f && lastTumDir != num)
		{
			if (AngleNear(tumRotZ + 45f, num, expected, 7f))
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(DialGoodSound, loop: false);
				}
				latched[unlatched] = true;
			}
			else
			{
				latched = new bool[3];
				vibration = new bool[3];
			}
			UpdateComboInstructions();
		}
		else
		{
			if (!AngleNear(tumRotZ + 45f, num, expected, 5f))
			{
				return;
			}
			latched[unlatched] = true;
			UpdateComboInstructions();
			latched[unlatched] = false;
			if (AngleNear(tumRotZ + 45f, num, expected, 7f))
			{
				reversalBuffer = 0.15f * (0f - num2);
				if (!vibration[unlatched])
				{
					VibrationManager.Vibrate(0.5f, 0.5f, 0.2f, VibrationManager.VibrationFalloff.Linear);
					vibration[unlatched] = true;
				}
			}
		}
	}

	public override void Close()
	{
		SoundManager.Instance.StopNamedSound("spinnerLoop");
		base.Close();
	}

	private bool AngleNear(float actual, float dir, float expected, float Threshold)
	{
		if (actual < 0f)
		{
			actual += 360f;
		}
		if (Mathf.Sign(dir) != Mathf.Sign(expected))
		{
			return false;
		}
		expected = Mathf.Abs(expected);
		if (actual < 90f && expected > 270f)
		{
			actual += 360f;
		}
		if (expected < 90f && actual > 270f)
		{
			expected += 360f;
		}
		if (actual >= expected - Threshold)
		{
			return actual <= expected + Threshold;
		}
		return false;
	}

	public SafeMinigame()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
	}
}
