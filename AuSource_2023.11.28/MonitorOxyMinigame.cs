using System.Linq;
using Rewired;
using UnityEngine;

public class MonitorOxyMinigame : Minigame
{
	public SpriteRenderer[] Targets;

	public BoxCollider2D[] Sliders;

	public VerticalSpriteGauge[] Fills;

	public FloatRange YRange;

	public FloatRange[] RandomRanges;

	private Controller controller = new Controller();

	public AudioClip[] DragSounds;

	private AudioSource ActiveSound;

	public Transform selectorObject;

	private TouchpadBehavior touchpad;

	private float initialY;

	private int selectedIndex;

	private bool prevHadInput;

	private float selectCooldown;

	private bool isTouchInput;

	public override void Begin(PlayerTask task)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		for (int i = 0; i < Sliders.Length; i++)
		{
			BoxCollider2D obj = Sliders[i];
			Vector3 localPosition = ((Component)obj).transform.localPosition;
			localPosition.y = RandomRanges[i].Next();
			((Component)obj).transform.localPosition = localPosition;
			float value = YRange.ReverseLerp(localPosition.y);
			Fills[i].Value = value;
		}
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		controller.Update();
		Vector2 val = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
		if (touchpad.IsTouching())
		{
			val = touchpad.GetTouchVector();
			val.x *= 0.5f;
		}
		if (Mathf.Abs(val.x) > 0.7f)
		{
			if (selectCooldown <= 0f)
			{
				selectedIndex += (int)Mathf.Sign(val.x);
				selectedIndex = Mathf.Clamp(selectedIndex, 0, Sliders.Length - 1);
				selectorObject.SetParent(((Component)Sliders[selectedIndex]).transform, false);
				if (prevHadInput)
				{
					if (Object.op_Implicit((Object)(object)ActiveSound))
					{
						ActiveSound.Stop();
					}
					prevHadInput = false;
				}
				selectCooldown = 0.2f;
			}
		}
		else
		{
			selectCooldown = 0f;
		}
		BoxCollider2D val2 = Sliders[selectedIndex];
		if (((Behaviour)val2).enabled && Mathf.Abs(val.x) < 0.7f && !isTouchInput)
		{
			if (Mathf.Abs(val.y) > 0.05f || touchpad.IsTouching())
			{
				if (!prevHadInput && Constants.ShouldPlaySfx())
				{
					ActiveSound = SoundManager.Instance.PlaySound(DragSounds[selectedIndex], loop: true, 0.7f, SoundManager.Instance.SfxChannel);
				}
				Vector3 localPosition = ((Component)val2).transform.localPosition;
				if (touchpad.IsTouching())
				{
					if (touchpad.IsFirstTouch())
					{
						initialY = ((Component)val2).transform.localPosition.y;
					}
					localPosition.y = val.y + initialY;
				}
				else
				{
					localPosition.y += val.y * 2f * Time.deltaTime;
				}
				localPosition.y = YRange.Clamp(localPosition.y);
				((Component)val2).transform.localPosition = localPosition;
				float num = YRange.ReverseLerp(localPosition.y);
				Fills[selectedIndex].Value = num;
				if (Object.op_Implicit((Object)(object)ActiveSound))
				{
					ActiveSound.pitch = Mathf.Lerp(0.8f, 1.2f, num);
				}
				prevHadInput = true;
			}
			else
			{
				if (prevHadInput && Object.op_Implicit((Object)(object)ActiveSound))
				{
					ActiveSound.Stop();
				}
				SpriteRenderer val3 = Targets[selectedIndex];
				if (Mathf.Abs(((Component)val2).transform.localPosition.y - ((Component)val3).transform.localPosition.y) < 0.1f)
				{
					((Behaviour)val2).enabled = false;
					val3.color = Color.green;
					if (Sliders.All((BoxCollider2D s) => !((Behaviour)s).enabled))
					{
						MyNormTask.NextStep();
						((MonoBehaviour)this).StartCoroutine(CoStartClose());
					}
					VibrationManager.Vibrate(0.2f, 0.2f, 0.2f);
				}
				prevHadInput = false;
			}
		}
		for (int num2 = 0; num2 < Sliders.Length; num2++)
		{
			BoxCollider2D val4 = Sliders[num2];
			if (!((Behaviour)val4).enabled)
			{
				continue;
			}
			switch (controller.CheckDrag((Collider2D)(object)val4))
			{
			case DragState.TouchStart:
				if (Constants.ShouldPlaySfx())
				{
					ActiveSound = SoundManager.Instance.PlaySound(DragSounds[num2], loop: true, 0.7f, SoundManager.Instance.SfxChannel);
				}
				isTouchInput = true;
				break;
			case DragState.Dragging:
			{
				Vector2 val6 = controller.DragPosition - Vector2.op_Implicit(((Component)val4).transform.parent.position);
				Vector3 localPosition2 = ((Component)val4).transform.localPosition;
				localPosition2.y = YRange.Clamp(val6.y);
				((Component)val4).transform.localPosition = localPosition2;
				float num3 = YRange.ReverseLerp(localPosition2.y);
				Fills[num2].Value = num3;
				if (Object.op_Implicit((Object)(object)ActiveSound))
				{
					ActiveSound.pitch = Mathf.Lerp(0.8f, 1.2f, num3);
				}
				break;
			}
			case DragState.Released:
			{
				if (Object.op_Implicit((Object)(object)ActiveSound))
				{
					ActiveSound.Stop();
				}
				SpriteRenderer val5 = Targets[num2];
				if (Mathf.Abs(((Component)val4).transform.localPosition.y - ((Component)val5).transform.localPosition.y) < 0.1f)
				{
					((Behaviour)val4).enabled = false;
					val5.color = Color.green;
					if (Sliders.All((BoxCollider2D s) => !((Behaviour)s).enabled))
					{
						MyNormTask.NextStep();
						((MonoBehaviour)this).StartCoroutine(CoStartClose());
					}
					VibrationManager.Vibrate(0.2f, 0.2f, 0.2f);
				}
				isTouchInput = false;
				break;
			}
			}
		}
	}

	public override void Close()
	{
		if (Object.op_Implicit((Object)(object)ActiveSound))
		{
			ActiveSound.Stop();
		}
		base.Close();
	}
}
