using Rewired;
using UnityEngine;

public class AirshipGarbageGame : Minigame
{
	public GarbageCanBehaviour[] GarbagePrefabs;

	public Sprite RelaxeHandle;

	public Sprite PulledHandle;

	private GarbageCanBehaviour can;

	public AudioClip grabSound;

	public Controller controller = new Controller();

	public Transform handCursorObject;

	public GameObject waitingHands;

	public GameObject grabbedHands;

	public SpriteRenderer[] handSprites;

	private bool prevHadLeftInput;

	private const float stickVelocityMagnitude = 6f;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		can = Object.Instantiate<GarbageCanBehaviour>(GarbagePrefabs[ConsoleId], ((Component)this).transform);
		SetupInput(disableCursor: true);
		SpriteRenderer[] array = handSprites;
		foreach (SpriteRenderer playerMaterialColors in array)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)playerMaterialColors);
		}
	}

	private void Update()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (amOpening || amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			handCursorObject.position = ((Component)can.Handle).transform.position;
			if (player.GetButton(24) && player.GetButton(21))
			{
				if (!grabbedHands.activeSelf)
				{
					grabbedHands.SetActive(true);
					waitingHands.SetActive(false);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(grabSound, loop: false);
					}
				}
				can.Handle.sprite = PulledHandle;
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				if (axis2DRaw.magnitude > 0.9f)
				{
					if (!prevHadLeftInput)
					{
						can.Body.velocity = axis2DRaw.normalized * 6f;
					}
					prevHadLeftInput = true;
				}
				else
				{
					prevHadLeftInput = false;
				}
			}
			else
			{
				if (!waitingHands.activeSelf)
				{
					grabbedHands.SetActive(false);
					waitingHands.SetActive(true);
				}
				can.Handle.sprite = RelaxeHandle;
			}
		}
		else
		{
			switch (controller.CheckDrag(can.Hitbox))
			{
			case DragState.TouchStart:
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(grabSound, loop: false);
				}
				break;
			case DragState.Dragging:
			{
				can.Handle.sprite = PulledHandle;
				Vector2 val = controller.DragPosition - Vector2.op_Implicit(((Component)can.Handle).transform.position);
				can.Body.velocity = 10f * val;
				break;
			}
			default:
				can.Handle.sprite = RelaxeHandle;
				break;
			}
		}
		if (!can.Body.IsTouching(can.Success))
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose(0.7f));
		}
	}
}
