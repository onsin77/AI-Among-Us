using System;
using Rewired;
using UnityEngine;

public class AdjustSteeringGame : Minigame
{
	private const float ArrowOffset = -3.25f;

	private const float ThrustOffset = -2.15f;

	private static readonly FloatRange ThrustRange = new FloatRange(-1.84f, 1.84f);

	private static readonly FloatRange SteeringRange = new FloatRange(-30f, 30f);

	public Collider2D Thrust;

	public Collider2D Steering;

	public SpriteRenderer ThrustTarget;

	public SpriteRenderer SteeringTarget;

	private Controller controller = new Controller();

	private float TargetThrustY;

	private float TargetSteeringRot;

	private bool thrustLocked;

	private bool steeringLocked;

	private float startAngle;

	public AudioClip HornSound;

	private bool prevHadLeftInput;

	private bool prevHadRightInput;

	private Vector2 prevRightStickInput = Vector2.zero;

	private bool prevThrustWasGood;

	private bool prevSteeringWasGood;

	private bool playerHasDualshock;

	private const float rotationSensitivity = 0.035f;

	private const float hintVibrationIntensity = 0.5f;

	public void HonkHorn()
	{
		SoundManager.Instance.PlaySoundImmediate(HornSound, loop: false);
	}

	public override void Begin(PlayerTask task)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		TargetThrustY = ThrustRange.Next();
		((Component)ThrustTarget).transform.localPosition = new Vector3(-3.25f, TargetThrustY);
		float num = ThrustRange.NextMinDistance(TargetThrustY, 0.5f);
		((Component)Thrust).transform.localPosition = new Vector3(-2.15f, num, -1f);
		TargetSteeringRot = SteeringRange.Next();
		((Component)SteeringTarget).transform.localEulerAngles = new Vector3(0f, 0f, TargetSteeringRot);
		float num2 = SteeringRange.NextMinDistance(TargetSteeringRot, 15f);
		((Component)Steering).transform.localEulerAngles = new Vector3(0f, 0f, num2);
		playerHasDualshock = false;
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(UpdateControllerIsDualshock));
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			UpdateControllerIsDualshock();
		}
		SetupInput(disableCursor: true, playerHasDualshock);
	}

	private void OnDisable()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(UpdateControllerIsDualshock));
	}

	private void UpdateControllerIsDualshock()
	{
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			Controller currentController = DestroyableSingleton<ActiveInputManager>.Instance.CurrentController;
			playerHasDualshock = false;
			for (int i = 0; i < Constants.PlayStationControllers.Length; i++)
			{
				if (currentController.name == Constants.PlayStationControllers[i])
				{
					playerHasDualshock = true;
					break;
				}
			}
		}
		else
		{
			playerHasDualshock = false;
		}
		if (Object.op_Implicit((Object)(object)inputHandler))
		{
			inputHandler.suppressInGamePlayerList = playerHasDualshock;
		}
	}

	public void Update()
	{
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			if ((playerHasDualshock && player.GetButtonDown(38)) || player.GetButtonDown(51))
			{
				HonkHorn();
			}
			if (!thrustLocked)
			{
				float axisRaw = player.GetAxisRaw(14);
				Vector3 localPosition = ((Component)Thrust).transform.localPosition;
				if (Mathf.Abs(axisRaw) > 0.1f)
				{
					localPosition.y += axisRaw * Time.deltaTime * 4f;
					localPosition.y = ThrustRange.Clamp(localPosition.y);
					((Component)Thrust).transform.localPosition = localPosition;
					prevHadLeftInput = true;
				}
				else
				{
					if (prevHadLeftInput && Mathf.Abs(localPosition.y - TargetThrustY) < 0.075f)
					{
						thrustLocked = true;
						localPosition.y = TargetThrustY;
						((Component)Thrust).transform.localPosition = localPosition;
					}
					prevHadLeftInput = false;
				}
				if (!thrustLocked)
				{
					bool flag = Mathf.Abs(localPosition.y - TargetThrustY) < 0.075f;
					if (flag && !prevThrustWasGood)
					{
						VibrationManager.Vibrate(0.5f, 0f, 0.1f);
					}
					prevThrustWasGood = flag;
				}
			}
			if (!steeringLocked)
			{
				Vector3 localEulerAngles = ((Component)Steering).transform.localEulerAngles;
				if (localEulerAngles.z > 180f)
				{
					localEulerAngles.z -= 360f;
				}
				Vector2 val = player.GetAxis2DRaw(16, 17);
				_ = ((Component)Thrust).transform.localPosition;
				float magnitude = val.magnitude;
				if (magnitude > 0.9f)
				{
					if (prevHadRightInput)
					{
						val /= magnitude;
						float num = Vector2.SignedAngle(prevRightStickInput, val);
						localEulerAngles.z += num * 0.035f;
						localEulerAngles.z = SteeringRange.Clamp(localEulerAngles.z);
						((Component)Steering).transform.localEulerAngles = localEulerAngles;
					}
					prevRightStickInput = val;
					prevHadRightInput = true;
				}
				else
				{
					if (prevHadRightInput && Mathf.Abs(localEulerAngles.z - TargetSteeringRot) < 5f)
					{
						steeringLocked = true;
						localEulerAngles.z = TargetSteeringRot;
						((Component)Steering).transform.localEulerAngles = localEulerAngles;
					}
					prevHadRightInput = false;
				}
				if (!steeringLocked)
				{
					bool flag2 = Mathf.Abs(localEulerAngles.z - TargetSteeringRot) < 5f;
					if (flag2 && !prevSteeringWasGood)
					{
						VibrationManager.Vibrate(0f, 0.5f, 0.1f);
					}
					prevSteeringWasGood = flag2;
				}
			}
		}
		else
		{
			if (!thrustLocked)
			{
				Vector3 localPosition2 = ((Component)Thrust).transform.localPosition;
				switch (controller.CheckDrag(Thrust))
				{
				case DragState.Dragging:
				{
					Vector2 val2 = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
					val2.y = ThrustRange.Clamp(val2.y);
					localPosition2.y = val2.y;
					((Component)Thrust).transform.localPosition = localPosition2;
					break;
				}
				case DragState.Released:
					if (Mathf.Abs(localPosition2.y - TargetThrustY) < 0.075f)
					{
						thrustLocked = true;
						localPosition2.y = TargetThrustY;
						((Component)Thrust).transform.localPosition = localPosition2;
					}
					break;
				}
			}
			if (!steeringLocked)
			{
				Vector3 localEulerAngles2 = ((Component)Steering).transform.localEulerAngles;
				if (localEulerAngles2.z > 180f)
				{
					localEulerAngles2.z -= 360f;
				}
				switch (controller.CheckDrag(Steering))
				{
				case DragState.TouchStart:
					startAngle = localEulerAngles2.z;
					break;
				case DragState.Dragging:
				{
					Vector2 val3 = controller.DragPosition - controller.DragStartPosition;
					localEulerAngles2.z = SteeringRange.Clamp(val3.x * -15f + startAngle);
					((Component)Steering).transform.localEulerAngles = localEulerAngles2;
					break;
				}
				case DragState.Released:
					if (Mathf.Abs(localEulerAngles2.z - TargetSteeringRot) < 5f)
					{
						steeringLocked = true;
						localEulerAngles2.z = TargetSteeringRot;
						((Component)Steering).transform.localEulerAngles = localEulerAngles2;
					}
					break;
				}
			}
		}
		if (thrustLocked && steeringLocked)
		{
			MyNormTask.NextStep();
			Close();
		}
	}

	public AdjustSteeringGame()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
