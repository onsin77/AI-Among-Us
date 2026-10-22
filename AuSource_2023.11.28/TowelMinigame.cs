using Rewired;
using UnityEngine;

public class TowelMinigame : Minigame
{
	private FloatRange towelRange = new FloatRange(-2.5f, -0.15f);

	public SpriteRenderer BasketGauge;

	public Collider2D BasketHitbox;

	public Collider2D[] Towels;

	private Controller controller = new Controller();

	private int towelsDone;

	public AudioClip TowelGrab;

	public AudioClip TowelLand;

	public Transform interactionCursor;

	public SpriteRenderer openHand;

	public SpriteRenderer holdingHand;

	public Vector2Range ValidArea;

	public float stickMoveSpeed = 4f;

	public float stickHeldSpeed = 3f;

	public float towelDragSpeed = 4f;

	private Collider2D[] overlapResults;

	private bool isHolding;

	private GameObject heldTowel;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		overlapResults = new Collider2D[Towels.Length * 3];
		SetupInput(disableCursor: true);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)openHand);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)holdingHand);
		((Renderer)openHand).enabled = true;
		((Renderer)holdingHand).enabled = false;
	}

	private void Update()
	{
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		controller.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!((Component)interactionCursor).gameObject.activeSelf)
			{
				((Component)interactionCursor).gameObject.SetActive(true);
			}
			if (isHolding)
			{
				((Renderer)openHand).enabled = false;
				((Renderer)holdingHand).enabled = true;
			}
			else
			{
				((Renderer)openHand).enabled = true;
				((Renderer)holdingHand).enabled = false;
			}
			Player player = ReInput.players.GetPlayer(0);
			Vector2 val = new Vector2(player.GetAxis(13), player.GetAxis(14));
			Vector3 localPosition = interactionCursor.localPosition;
			if (!isHolding)
			{
				localPosition.x += val.x * Time.deltaTime * stickMoveSpeed;
				localPosition.y += val.y * Time.deltaTime * stickMoveSpeed;
			}
			else
			{
				localPosition.x += val.x * Time.deltaTime * stickHeldSpeed;
				localPosition.y += val.y * Time.deltaTime * stickHeldSpeed;
			}
			int num = Physics2D.OverlapCircleNonAlloc(Vector2.op_Implicit(interactionCursor.position), ((Component)interactionCursor).transform.localScale.x, overlapResults, LayerMask.GetMask(new string[1] { "UICollide" }));
			localPosition.x = Mathf.Clamp(localPosition.x, ValidArea.min.x, ValidArea.max.x);
			localPosition.y = Mathf.Clamp(localPosition.y, ValidArea.min.y, ValidArea.max.y);
			interactionCursor.localPosition = localPosition;
			if (player.GetButton(11))
			{
				if (!isHolding)
				{
					for (int i = 0; i < num; i++)
					{
						Collider2D val2 = overlapResults[i];
						if (Object.op_Implicit((Object)(object)((Component)val2).GetComponent<Rigidbody2D>()))
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(TowelGrab, loop: false);
							}
							heldTowel = ((Component)val2).gameObject;
							isHolding = true;
							break;
						}
					}
				}
				else
				{
					if (heldTowel.activeSelf)
					{
						heldTowel.GetComponent<Rigidbody2D>().velocity = Vector2.op_Implicit((localPosition - heldTowel.transform.localPosition) * towelDragSpeed);
					}
					if (heldTowel.GetComponent<Collider2D>().IsTouching(BasketHitbox))
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySoundImmediate(TowelLand, loop: false);
						}
						towelsDone++;
						heldTowel.gameObject.SetActive(false);
					}
				}
			}
			if (player.GetButtonUp(11))
			{
				isHolding = false;
				if (Object.op_Implicit((Object)(object)heldTowel))
				{
					heldTowel.GetComponent<Rigidbody2D>().velocity = Vector2.op_Implicit((localPosition - heldTowel.transform.localPosition) * towelDragSpeed);
					heldTowel = null;
				}
			}
		}
		else if (((Component)interactionCursor).gameObject.activeSelf)
		{
			((Component)interactionCursor).gameObject.SetActive(false);
		}
		Collider2D[] towels = Towels;
		foreach (Collider2D val3 in towels)
		{
			if (!((Behaviour)val3).isActiveAndEnabled)
			{
				continue;
			}
			if (!val3.IsTouching(BasketHitbox))
			{
				switch (controller.CheckDrag(val3))
				{
				case DragState.TouchStart:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(TowelGrab, loop: false);
					}
					break;
				case DragState.Dragging:
				{
					Rigidbody2D component = ((Component)val3).GetComponent<Rigidbody2D>();
					component.velocity = (controller.DragPosition - component.position) * 4f;
					break;
				}
				}
			}
			else
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySoundImmediate(TowelLand, loop: false);
				}
				towelsDone++;
				((Component)val3).gameObject.SetActive(false);
			}
		}
		((Component)BasketGauge).transform.localPosition = new Vector3(0f, towelRange.Lerp((float)towelsDone / (float)Towels.Length), 0.5f);
		if (towelsDone >= Towels.Length)
		{
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
		}
	}
}
