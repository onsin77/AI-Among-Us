using Rewired;
using UnityEngine;

public class KeyMinigame : Minigame
{
	public KeySlotBehaviour[] Slots;

	private int keyState;

	public SpriteRenderer KeyImage;

	public Sprite normalImage;

	public Sprite insertImage;

	public BoxCollider2D key;

	private int targetSlotId;

	private Controller controller = new Controller();

	public AudioClip KeyGrab;

	public AudioClip KeyInsert;

	public AudioClip KeyOpen;

	public AudioClip KeyTurn;

	private TouchpadBehavior touchpad;

	private Vector3 initialPos;

	private bool prevHadInput;

	private Vector2 prevInputDir;

	private float currentAngle;

	public GameObject moveKeyGlyph;

	public GameObject turnKeyGlyph;

	public void Start()
	{
		targetSlotId = (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) ? PlayerControl.LocalPlayer.PlayerId : 0).Wrap(Slots.Length);
		Slots[targetSlotId].SetHighlight();
		touchpad = ((Component)this).GetComponent<TouchpadBehavior>();
		SetupInput(disableCursor: true);
	}

	public void Update()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0662: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_083c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		if (keyState == 2)
		{
			return;
		}
		KeySlotBehaviour keySlotBehaviour = Slots[targetSlotId];
		controller.Update();
		switch (keyState)
		{
		case 0:
			if (!moveKeyGlyph.activeSelf)
			{
				moveKeyGlyph.SetActive(true);
			}
			if (turnKeyGlyph.activeSelf)
			{
				turnKeyGlyph.SetActive(false);
			}
			break;
		case 1:
			if (moveKeyGlyph.activeSelf)
			{
				moveKeyGlyph.SetActive(false);
			}
			if (!turnKeyGlyph.activeSelf)
			{
				turnKeyGlyph.SetActive(true);
			}
			break;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 val = Vector2.zero;
			val = player.GetAxis2DRaw(13, 14);
			if ((double)val.sqrMagnitude > 0.5)
			{
				if (!prevHadInput)
				{
					if (keyState == 0)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(KeyGrab, loop: false);
						}
					}
					else if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(KeyTurn, loop: false);
					}
				}
				if (keyState == 0)
				{
					Vector3 val2 = Vector2.op_Implicit(val * Time.deltaTime * 5f);
					Transform transform = ((Component)key).transform;
					transform.localPosition += val2;
					if (((Collider2D)key).IsTouching((Collider2D)(object)keySlotBehaviour.Hitbox))
					{
						KeyImage.sprite = insertImage;
					}
					else
					{
						KeyImage.sprite = normalImage;
					}
				}
				else
				{
					val = val.normalized;
					if (prevHadInput)
					{
						currentAngle += Vector2.SignedAngle(prevInputDir, val);
						currentAngle = Mathf.Clamp(currentAngle, 0f, 90f);
						((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
					}
					else
					{
						currentAngle = ((Component)key).transform.localEulerAngles.z;
					}
					prevInputDir = val;
				}
				prevHadInput = true;
			}
			else if (touchpad.IsTouching())
			{
				if (touchpad.IsFirstTouch())
				{
					initialPos = ((Component)key).transform.localPosition;
				}
				val = touchpad.GetTouchVector();
				if (!prevHadInput)
				{
					if (keyState == 0)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(KeyGrab, loop: false);
						}
					}
					else if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(KeyTurn, loop: false);
					}
				}
				if (keyState == 0)
				{
					((Component)key).transform.localPosition = Vector2.op_Implicit(val) + initialPos;
					if (((Collider2D)key).IsTouching((Collider2D)(object)keySlotBehaviour.Hitbox))
					{
						KeyImage.sprite = insertImage;
					}
					else
					{
						KeyImage.sprite = normalImage;
					}
				}
				else
				{
					Vector2 centerToTouch = touchpad.GetCenterToTouch();
					val = centerToTouch.normalized;
					if (prevHadInput)
					{
						currentAngle += Vector2.SignedAngle(prevInputDir, val);
						currentAngle = Mathf.Clamp(currentAngle, 0f, 90f);
						((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
					}
					else
					{
						currentAngle = ((Component)key).transform.localEulerAngles.z;
					}
					prevInputDir = val;
				}
				prevHadInput = true;
			}
			else
			{
				if (!prevHadInput)
				{
					return;
				}
				if (keyState == 0)
				{
					if (((Collider2D)key).IsTouching((Collider2D)(object)keySlotBehaviour.Hitbox))
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(KeyInsert, loop: false);
						}
						keyState = 1;
						key.size = new Vector2(2f, 2f);
						Vector3 position = ((Component)keySlotBehaviour).transform.position;
						position.z--;
						((Component)key).transform.position = position;
						KeyImage.sprite = insertImage;
						keySlotBehaviour.SetInserted();
					}
				}
				else
				{
					float num = ((Component)key).transform.localEulerAngles.z;
					if (num > 180f)
					{
						num -= 360f;
					}
					num %= 360f;
					if (Mathf.Abs(num) > 80f)
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(KeyOpen, loop: false);
						}
						keySlotBehaviour.SetFinished();
						((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						keyState = 2;
						MyNormTask.NextStep();
						((MonoBehaviour)this).StartCoroutine(CoStartClose());
					}
					else
					{
						((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, 0f);
					}
				}
				prevHadInput = false;
			}
			return;
		}
		switch (controller.CheckDrag((Collider2D)(object)key))
		{
		case DragState.TouchStart:
			if (keyState == 0)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(KeyGrab, loop: false);
				}
			}
			else if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(KeyTurn, loop: false);
			}
			break;
		case DragState.Dragging:
			if (keyState == 0)
			{
				Vector2 val3 = controller.DragPosition - Vector2.op_Implicit(((Component)this).transform.position);
				((Component)key).transform.localPosition = Vector2.op_Implicit(val3);
				if (((Collider2D)key).IsTouching((Collider2D)(object)keySlotBehaviour.Hitbox))
				{
					KeyImage.sprite = insertImage;
				}
				else
				{
					KeyImage.sprite = normalImage;
				}
			}
			else
			{
				Vector2 val4 = Vector2.op_Implicit(((Component)key).transform.position);
				float num3 = Vector2.SignedAngle(controller.DragStartPosition - val4, controller.DragPosition - val4);
				((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(num3, -90f, 90f));
			}
			break;
		case DragState.Released:
		{
			if (keyState == 0)
			{
				if (((Collider2D)key).IsTouching((Collider2D)(object)keySlotBehaviour.Hitbox))
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(KeyInsert, loop: false);
					}
					keyState = 1;
					key.size = new Vector2(2f, 2f);
					Vector3 position2 = ((Component)keySlotBehaviour).transform.position;
					position2.z--;
					((Component)key).transform.position = position2;
					KeyImage.sprite = insertImage;
					keySlotBehaviour.SetInserted();
				}
				break;
			}
			float num2 = ((Component)key).transform.localEulerAngles.z;
			if (num2 > 180f)
			{
				num2 -= 360f;
			}
			num2 %= 360f;
			if (Mathf.Abs(num2) > 80f)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(KeyOpen, loop: false);
				}
				keySlotBehaviour.SetFinished();
				((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				keyState = 2;
				MyNormTask.NextStep();
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			else
			{
				((Component)key).transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
			break;
		}
		case DragState.Holding:
			break;
		}
	}
}
