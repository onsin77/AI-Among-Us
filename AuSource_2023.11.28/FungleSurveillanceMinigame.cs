using System;
using System.Linq;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

public class FungleSurveillanceMinigame : Minigame
{
	private const float MAGIC_Z_INDEX = 50f;

	[SerializeField]
	private FungleSecurityCamera securityCamera;

	[SerializeField]
	private MeshRenderer viewport;

	[SerializeField]
	private Vector3 initialOffset;

	[SerializeField]
	private float cameraOrthographicSize;

	[Header("Speeds")]
	[SerializeField]
	private float keyboardMoveSpeed;

	[SerializeField]
	private float buttonMoveSpeed;

	[SerializeField]
	private float joystickMoveSpeed;

	[SerializeField]
	private float mobileJoystickMoveSpeed;

	[Header("Navigation")]
	[SerializeField]
	private PassiveButton arrowUp;

	[SerializeField]
	private PassiveButton arrowDown;

	[SerializeField]
	private PassiveButton arrowLeft;

	[SerializeField]
	private PassiveButton arrowRight;

	[SerializeField]
	private VirtualJoystick virtualJoystick;

	private Vector2 cameraSpeed;

	private void FixedUpdate()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (amOpening || amClosing != CloseState.None)
		{
			return;
		}
		Vector2 val;
		if (DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.joystick != null)
		{
			val = DestroyableSingleton<HudManager>.Instance.joystick.DeltaL;
			if (val.sqrMagnitude > 0.01f)
			{
				SetSpeed(DestroyableSingleton<HudManager>.Instance.joystick.DeltaL * keyboardMoveSpeed);
				goto IL_00ca;
			}
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Vector2 axis2DRaw = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
			SetSpeed(axis2DRaw * joystickMoveSpeed);
		}
		else
		{
			val = virtualJoystick.DeltaR;
			if (val.sqrMagnitude > 0f)
			{
				SetSpeed(virtualJoystick.DeltaR * mobileJoystickMoveSpeed);
			}
		}
		goto IL_00ca;
		IL_00ca:
		if (cameraSpeed.sqrMagnitude > 0f)
		{
			securityCamera.Move(Vector2.op_Implicit(cameraSpeed));
			cameraSpeed = Vector2.zero;
		}
	}

	private void Update()
	{
		if (!amOpening)
		{
			securityCamera.CollisionsEnabled = true;
		}
	}

	public override void Begin(PlayerTask task)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected Obj, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected Obj, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected Obj, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected Obj, but got Unknown
		base.Begin(task);
		RenderTexture temporary = RenderTexture.GetTemporary(512, 512, 24, (RenderTextureFormat)0);
		securityCamera.RenderTexture = temporary;
		securityCamera.OrthographicSize = cameraOrthographicSize;
		FungleShipStatus fungleShipStatus = ShipStatus.Instance as FungleShipStatus;
		if (Object.op_Implicit((Object)(object)fungleShipStatus) && fungleShipStatus.LastBinocularPos != Vector2.zero)
		{
			((Component)securityCamera).transform.localPosition = new Vector3(fungleShipStatus.LastBinocularPos.x, fungleShipStatus.LastBinocularPos.y, 50f);
		}
		else
		{
			((Component)securityCamera).transform.position = ((Component)ShipStatus.Instance.AllRooms.First((PlainShipRoom room) => room.RoomId == SystemTypes.MeetingRoom)).transform.position + initialOffset;
		}
		securityCamera.CollisionsEnabled = false;
		((Renderer)viewport).material.SetTexture("_MainTex", (Texture)(object)temporary);
		((UnityEvent)arrowDown.OnClick).AddListener((UnityAction)(() =>
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			SetSpeed(new Vector2(0f, 0f - buttonMoveSpeed));
		}));
		((UnityEvent)arrowUp.OnClick).AddListener((UnityAction)(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			SetSpeed(new Vector2(0f, buttonMoveSpeed));
		}));
		((UnityEvent)arrowLeft.OnClick).AddListener((UnityAction)(() =>
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			SetSpeed(new Vector2(0f - buttonMoveSpeed, 0f));
		}));
		((UnityEvent)arrowRight.OnClick).AddListener((UnityAction)(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			SetSpeed(new Vector2(buttonMoveSpeed, 0f));
		}));
		FungleSecurityCamera fungleSecurityCamera = securityCamera;
		fungleSecurityCamera.OnCollision = (Action<Collision2D>)Delegate.Combine(fungleSecurityCamera.OnCollision, new Action<Collision2D>(OnCameraCollision));
		FungleSecurityCamera fungleSecurityCamera2 = securityCamera;
		fungleSecurityCamera2.OnExitCollision = (Action<Collision2D>)Delegate.Combine(fungleSecurityCamera2.OnExitCollision, new Action<Collision2D>(OnCameraExitCollision));
		virtualJoystick.ToggleVisuals(on: false);
		SetupInput(disableCursor: true);
	}

	public override void Close()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.Close();
		FungleShipStatus fungleShipStatus = ShipStatus.Instance as FungleShipStatus;
		if (Object.op_Implicit((Object)(object)fungleShipStatus))
		{
			fungleShipStatus.LastBinocularPos = new Vector2(((Component)securityCamera).transform.localPosition.x, ((Component)securityCamera).transform.localPosition.y);
		}
	}

	private void SetSpeed(Vector2 speed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		cameraSpeed = speed;
	}

	private void OnCameraCollision(Collision2D col)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < col.contactCount; i++)
		{
			ContactPoint2D val = col.contacts[i];
			((Component)arrowRight).gameObject.SetActive(val.normal.x > -0.9f);
			((Component)arrowUp).gameObject.SetActive(val.normal.y > -0.9f);
			((Component)arrowLeft).gameObject.SetActive(val.normal.x < 0.9f);
			((Component)arrowDown).gameObject.SetActive(val.normal.y < 0.9f);
		}
	}

	private void OnCameraExitCollision(Collision2D col)
	{
		((Component)arrowRight).gameObject.SetActive(true);
		((Component)arrowUp).gameObject.SetActive(true);
		((Component)arrowLeft).gameObject.SetActive(true);
		((Component)arrowDown).gameObject.SetActive(true);
	}
}
