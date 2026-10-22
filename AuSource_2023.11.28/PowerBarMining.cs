using System;
using Rewired;
using UnityEngine;

public class PowerBarMining : MonoBehaviour
{
	private static readonly string shaderPercentage = "_NormalizedUvs";

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private PassiveButton passiveButton;

	private float barSpeed;

	private float currentPercentage;

	private bool updatePowerMeter;

	private Action<float, Vector3> hitRock;

	private Action<float> onPitch;

	private Player gamepad;

	private Vector3 clickPos;

	private Controller controller = new Controller();

	private void Awake()
	{
		gamepad = ReInput.players.GetPlayer(0);
	}

	private void PowerMeterClick()
	{
		if (!IsUsingController())
		{
			StopPowerMeter();
		}
	}

	private bool IsUsingController()
	{
		return ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
	}

	private void Update()
	{
		controller.Update();
		if (updatePowerMeter)
		{
			currentPercentage += barSpeed * Time.deltaTime;
			currentPercentage = Mathf.Clamp(currentPercentage, -1f, 0f);
			if (currentPercentage >= 0f || currentPercentage <= -1f)
			{
				barSpeed *= -1f;
			}
			onPitch?.Invoke(currentPercentage + 1f);
			SetPercentage(currentPercentage);
			if (gamepad.GetButtonUp(21) || controller.AnyTouchUp)
			{
				StopPowerMeter();
			}
		}
	}

	public void Initialize(float speed, Action<float, Vector3> hitAction, Vector3 pos, Action<float> pitchAction)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		onPitch = pitchAction;
		barSpeed = speed;
		hitRock = hitAction;
		currentPercentage = -1f;
		SetPercentage(currentPercentage);
		updatePowerMeter = true;
		clickPos = pos;
	}

	private void SetPercentage(float value)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)spriteRenderer).material.SetVector(shaderPercentage, new Vector4(value, 1f, 0f, 1f));
	}

	private void StopPowerMeter()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (updatePowerMeter)
		{
			updatePowerMeter = false;
			hitRock?.Invoke(currentPercentage + 1f, clickPos);
		}
	}
}
