using System.Collections;
using Rewired;
using UnityEngine;

public class FillCanistersGame : Minigame
{
	private Vector3 CanisterAppearPosition = new Vector3(0f, 4.5f, 0f);

	private Vector3 CanisterStartPosition = new Vector3(-0.75f, 1.5f, 0f);

	private Vector3 CanisterDragPosition = new Vector3(0.4f, -1f, 0f);

	private Vector3 CanisterSnapPosition = new Vector3(0f, -1f, 0f);

	private Vector3 CanisterAwayPosition = new Vector3(8f, -1f, 0f);

	public float FillTime = 2.5f;

	public CanisterBehaviour Canister;

	private Controller controller = new Controller();

	public AudioClip FillLoop;

	public AudioClip DropSound;

	public AudioClip GrabSound;

	public AudioClip PlugInSound;

	public AudioClip PlugOutSound;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	public void Start()
	{
		((MonoBehaviour)this).StartCoroutine(Run());
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public override void Close()
	{
		SoundManager.Instance.StopSound(FillLoop);
		base.Close();
	}

	private IEnumerator Run()
	{
		do
		{
			Canister.Gauge.Value = 0f;
			yield return Effects.Slide2D(((Component)Canister).transform, Vector2.op_Implicit(CanisterAppearPosition), Vector2.op_Implicit(CanisterStartPosition), 0.1f);
			controller.ClearTouch();
			while (true)
			{
				controller.Update();
				if (Controller.currentTouchType == Controller.TouchType.Joystick)
				{
					float num = ReInput.players.GetPlayer(0).GetAxisRaw(14);
					if (touchpad.IsTouching())
					{
						num = touchpad.GetTouchVector().y;
					}
					if (Mathf.Abs(num) > 0.2f)
					{
						if (!prevHadInput && Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(GrabSound, loop: false);
						}
						float num2 = FloatRange.ReverseLerp(((Component)Canister).transform.localPosition.y, CanisterDragPosition.y, CanisterStartPosition.y);
						num2 += num * 3f * Time.deltaTime;
						num2 = Mathf.Clamp01(num2);
						((Component)Canister).transform.localPosition = Vector3.Lerp(CanisterDragPosition, CanisterStartPosition, num2);
						prevHadInput = true;
					}
					else
					{
						if (prevHadInput)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(DropSound, loop: false);
							}
							if (FloatRange.ReverseLerp(((Component)Canister).transform.localPosition.y, CanisterDragPosition.y, CanisterStartPosition.y) < 0.05f)
							{
								if (Constants.ShouldPlaySfx())
								{
									SoundManager.Instance.PlaySound(PlugInSound, loop: false);
								}
								break;
							}
						}
						prevHadInput = false;
					}
				}
				else
				{
					switch (controller.CheckDrag(Canister.Hitbox))
					{
					case DragState.TouchStart:
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(GrabSound, loop: false);
						}
						goto IL_035f;
					case DragState.Dragging:
					{
						float num3 = FloatRange.ReverseLerp((Vector2.op_Implicit(controller.DragPosition) - ((Component)this).transform.position).y, CanisterDragPosition.y, CanisterStartPosition.y);
						((Component)Canister).transform.localPosition = Vector3.Lerp(CanisterDragPosition, CanisterStartPosition, num3);
						goto IL_035f;
					}
					case DragState.Released:
						break;
					default:
						goto IL_035f;
					}
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(DropSound, loop: false);
					}
					if (FloatRange.ReverseLerp(((Component)Canister).transform.localPosition.y, CanisterDragPosition.y, CanisterStartPosition.y) < 0.05f)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(PlugInSound, loop: false);
						}
						break;
					}
				}
				goto IL_035f;
				IL_035f:
				yield return null;
			}
			AudioSource fillSound = null;
			if (Constants.ShouldPlaySfx())
			{
				fillSound = SoundManager.Instance.PlaySound(FillLoop, loop: true);
			}
			yield return Effects.Slide2D(((Component)Canister).transform, Vector2.op_Implicit(CanisterDragPosition), Vector2.op_Implicit(CanisterSnapPosition), 0.1f);
			yield return Effects.Lerp(FillTime, (float t) =>
			{
				Canister.Gauge.Value = t;
			});
			if (Object.op_Implicit((Object)(object)fillSound))
			{
				fillSound.Stop();
			}
			Player player = ReInput.players.GetPlayer(0);
			float stickInput = player.GetAxisRaw(13);
			if (touchpad.IsTouching())
			{
				stickInput = touchpad.GetTouchVector().x;
			}
			bool hasNoRemoveInput = controller.CheckDrag(Canister.Hitbox) != DragState.TouchStart && player.GetAxisRaw(13) < 0.9f;
			while (hasNoRemoveInput)
			{
				controller.Update();
				hasNoRemoveInput = controller.CheckDrag(Canister.Hitbox) != DragState.TouchStart && stickInput < 0.9f;
				stickInput = ((!touchpad.IsTouching()) ? player.GetAxisRaw(13) : touchpad.GetTouchVector().x);
				yield return null;
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(PlugOutSound, loop: false);
			}
			yield return Effects.Slide2D(((Component)Canister).transform, Vector2.op_Implicit(CanisterSnapPosition), Vector2.op_Implicit(CanisterAwayPosition), 0.3f);
			MyNormTask.NextStep();
		}
		while (!MyNormTask.IsComplete);
		((MonoBehaviour)this).StartCoroutine(CoStartClose());
	}

	public FillCanistersGame()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
	}
}
